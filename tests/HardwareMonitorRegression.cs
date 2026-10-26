using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using Yata;
using Yata.Components;

// No real Computer is opened and no driver or hardware control is used.
static class HardwareMonitorRegression
{
    static readonly FieldInfo Busy = typeof(HardwareMonitor).GetField("updateInProgress", BindingFlags.Instance | BindingFlags.NonPublic);

    [STAThread]
    static int Main()
    {
        try
        {
            Run("Library null partition reproducer", LibraryNullPartition);
            Run("Failure isolation, retry and log deduplication", FailureAndRecovery);
            Run("Parallel failure isolation", () => FailureIsolation(true));
            Run("Sequential failure isolation", () => FailureIsolation(false));
            Run("Async updates cannot overlap", SingleFlight);
            Run("Async failures do not reach the UI context", AsyncFailure);
            Run("Startup visitor continues after a device failure", StartupFailure);
            Run("History snapshots and concurrent access", HistorySnapshots);
            Run("Missing samples do not become zero in aggregates", MissingAggregate);
            Run("Widgets accept missing values and recover", MissingWidgets);
            Console.WriteLine("All 10 regression checks passed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    static void Run(string name, Action check)
    {
        check();
        Console.WriteLine("PASS: " + name);
    }

    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    static HardwareMonitor Monitor(params FakeSensor[] sensors)
    {
        // Bypass the public constructor solely to keep tests independent of local hardware.
        // Startup initializes the target lists; the update gate is initially zero.
        var monitor = (HardwareMonitor)FormatterServices.GetUninitializedObject(typeof(HardwareMonitor));
        monitor.UpdateAsync = false;
        monitor.Startup(TimeSpan.FromMinutes(12), sensors);
        return monitor;
    }

    static void WaitIdle(HardwareMonitor monitor)
    {
        Check(SpinWait.SpinUntil(() => (int)Busy.GetValue(monitor) == 0, 5000), "Update did not finish.");
    }

    static void LibraryNullPartition()
    {
        var storage = FormatterServices.GetUninitializedObject(typeof(DiskInfoToolkit.Storage));
        var property = storage.GetType().GetProperty("Partitions");
        var partitions = (IList)Activator.CreateInstance(property.PropertyType);
        partitions.Add(null);
        storage.GetType().GetField("<Partitions>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(storage, partitions);
        try
        {
            storage.GetType().GetProperty("TotalFreeSize").GetValue(storage);
            throw new Exception("Expected the reported DiskInfoToolkit 1.1.2 failure.");
        }
        catch (TargetInvocationException ex)
        {
            Check(ex.InnerException is NullReferenceException, "Unexpected library failure.");
            Check(ex.InnerException.StackTrace.Contains("get_IsDynamicDisk"), "Library failure differs from the report.");
        }
    }

    static void FailureAndRecovery()
    {
        var hardware = new FakeHardware("retry-" + Guid.NewGuid().ToString("N"));
        var sample = new FakeSensor(hardware) { Value = 42 };
        var monitor = Monitor(sample);
        hardware.OnUpdate = () => { throw new NullReferenceException("Injected disk failure"); };
        for (int i = 0; i < 3; i++) monitor.Update();
        Check(!monitor.UpdateTargetSensor[0].Value.HasValue, "Failed device retained a valid reading.");
        var log = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hardware-monitor.errors.log");
        Check(File.ReadAllText(log).Split(new[] { "Hardware " + hardware.Identifier }, StringSplitOptions.None).Length == 2,
            "Repeated failures flooded the log.");
        hardware.OnUpdate = () => { };
        monitor.Update();
        Check(monitor.UpdateTargetSensor[0].Value == 42, "Device did not recover on retry.");
        hardware.OnUpdate = () => { throw new NullReferenceException("Injected disk failure after recovery"); };
        monitor.Update();
        Check(File.ReadAllText(log).Split(new[] { "Hardware " + hardware.Identifier }, StringSplitOptions.None).Length == 3,
            "A new failure after recovery was not logged.");
    }

    static void FailureIsolation(bool parallel)
    {
        var bad = new FakeHardware("bad");
        var child = new FakeHardware("child") { Parent = bad };
        var good = new FakeHardware("good");
        var badSample = new FakeSensor(bad) { Value = 10 };
        var childSample = new FakeSensor(child) { Value = 15 };
        var goodSample = new FakeSensor(good) { Value = 20 };
        var monitor = Monitor(badSample, childSample, goodSample);
        monitor.UpdateParall = parallel;
        bad.OnUpdate = () => { throw new NullReferenceException("Injected storage failure"); };
        monitor.UpdateTargetSensor.Add(new ValueHistorySensor(new ISensor[] { badSample, goodSample }, new Identifier("test", "sum"), true));
        monitor.Update();
        Check(good.Calls == 1 && monitor.UpdateTargetSensor[2].Value == 20, "Healthy device stopped updating.");
        Check(!monitor.UpdateTargetSensor[0].Value.HasValue && !monitor.UpdateTargetSensor[1].Value.HasValue,
            "A failed device or its child returned a valid reading.");
        Check(!monitor.UpdateTargetSensor[3].Value.HasValue, "Aggregate used a stale failed reading.");
    }

    static void SingleFlight()
    {
        var hardware = new FakeHardware("slow");
        var monitor = Monitor(new FakeSensor(hardware));
        using (var entered = new ManualResetEventSlim())
        using (var release = new ManualResetEventSlim())
        {
            hardware.OnUpdate = () =>
            {
                entered.Set();
                Check(release.Wait(5000), "Blocking test update timed out.");
            };
            monitor.UpdateAsync = true;
            monitor.Update();
            try
            {
                Check(entered.Wait(5000), "Async update did not start.");
                monitor.Update();
                monitor.UpdateAsync = false;
                monitor.Update();
                Check(hardware.Calls == 1, "A second update overlapped the first.");
            }
            finally { release.Set(); }
            WaitIdle(monitor);
            hardware.OnUpdate = () => { };
            monitor.Update();
            Check(hardware.Calls == 2, "Update gate was not released.");
        }
    }

    static void AsyncFailure()
    {
        var hardware = new FakeHardware("async-failure");
        var monitor = Monitor(new FakeSensor(hardware));
        var context = new RecordingContext();
        var original = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(context);
        try
        {
            monitor.UpdateAsync = true;
            hardware.OnUpdate = () => { throw new NullReferenceException("Injected async failure"); };
            monitor.Update();
            WaitIdle(monitor);
            Check(context.Posts == 0, "Failure or continuation was posted to the UI context.");
            // Also exercise the outer task boundary, outside an individual hardware Update.
            monitor.UpdateTargetSensor.Add(null);
            monitor.Update();
            WaitIdle(monitor);
            Check(context.Posts == 0, "Unexpected cycle failure escaped to the UI context.");
            monitor.UpdateTargetSensor.Remove(null);
            hardware.OnUpdate = () => { };
            monitor.Update();
            WaitIdle(monitor);
            Check(monitor.UpdateTargetSensor[0].Value.HasValue, "Async update did not recover.");
        }
        finally { SynchronizationContext.SetSynchronizationContext(original); }
    }

    static void StartupFailure()
    {
        var parent = new FakeHardware("startup-bad");
        var child = new FakeHardware("startup-child") { Parent = parent };
        var sample = new FakeSensor(child);
        child.Sensors = new ISensor[] { sample };
        parent.SubHardware = new IHardware[] { child };
        parent.OnUpdate = () => { throw new NullReferenceException("Injected initialization failure"); };
        var visitor = new InitializerVisitor();
        visitor.VisitHardware(parent);
        Check(child.Calls == 1 && visitor.AllSensors.Contains(sample), "Initialization stopped at failed hardware.");
    }

    static void HistorySnapshots()
    {
        var history = new ValueHistorySensor(new FakeSensor(new FakeHardware("history"))) { ValuesTimeWindow = TimeSpan.FromMinutes(12) };
        for (int i = 0; i < 5; i++) history.Value = 10;
        var snapshot = history.Values;
        for (int i = 0; i < 5; i++) history.Value = 20;
        Check(snapshot.Count() == 1 && history.Values.Count() == 2, "History was not a stable snapshot.");
        Parallel.Invoke(
            () => { for (int i = 0; i < 5000; i++) history.Value = i; },
            () => { for (int i = 0; i < 500; i++) foreach (var item in history.Values) Check(!float.IsNaN(item.Value), "Invalid history value."); });
        history.ClearValues();
        history.Value = 100;
        history.Value = null;
        for (int i = 0; i < 5; i++) history.Value = 20;
        Check(history.Values.Single().Value == 20, "Averages mixed samples across a failure.");
    }

    static void MissingAggregate()
    {
        var hardware = new FakeHardware("aggregate");
        var values = new ISensor[] { new FakeSensor(hardware) { Value = null }, new FakeSensor(hardware) { Value = 20 } };
        foreach (var sum in new[] { true, false })
        {
            var aggregate = new ValueHistorySensor(values, new Identifier("test", "aggregate"), sum);
            aggregate.Update();
            Check(!aggregate.Value.HasValue, "A missing input produced a partial aggregate.");
        }
    }

    static void MissingWidgets()
    {
        var files = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files");
        Directory.CreateDirectory(files);
        using (var icon = new Bitmap(48, 48)) icon.Save(Path.Combine(files, "SystemApp.png"));
        var sample = new FakeSensor(new FakeHardware("widget")) { Value = null };
        var assembly = typeof(HardwareMonitor).Assembly;
        var meter = (OwnerDrawWidget)Activator.CreateInstance(assembly.GetType("Yata.Components.Widget.Tachometer"), new object[] { 0, 100, sample });
        Check(meter.Update(), "Initial missing meter was not drawn.");
        var display = meter.GetType().GetField("disp", BindingFlags.Instance | BindingFlags.NonPublic);
        Check((string)display.GetValue(meter) == "N/A", "Meter did not show N/A.");
        var list = (OwnerDrawWidget)Activator.CreateInstance(assembly.GetType("Yata.Components.Widget.SensorCurrentList"));
        list.GetType().GetMethod("Add").Invoke(list, new object[] { sample, "Test", Color.Red });
        using (var bitmap = new Bitmap(200, 200))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            meter.Draw(graphics);
            list.Draw(graphics);
            sample.Value = 30;
            Check(meter.Update(), "Meter did not redraw on recovery.");
            meter.Draw(graphics);
            list.Draw(graphics);
            Check((string)display.GetValue(meter) != "N/A", "Meter remained missing after recovery.");
        }
    }

    sealed class RecordingContext : SynchronizationContext
    {
        public int Posts;
        public override void Post(SendOrPostCallback callback, object state) { Interlocked.Increment(ref Posts); }
    }

    sealed class FakeHardware : IHardware
    {
        public FakeHardware(string name) { Name = name; Identifier = new Identifier("test", name); }
        public int Calls;
        public Action OnUpdate = () => { };
        public HardwareType HardwareType => HardwareType.Storage;
        public Identifier Identifier { get; private set; }
        public string Name { get; set; }
        public IHardware Parent { get; set; }
        public ISensor[] Sensors { get; set; } = new ISensor[0];
        public IHardware[] SubHardware { get; set; } = new IHardware[0];
        public IDictionary<string, string> Properties => new Dictionary<string, string>();
        public event SensorEventHandler SensorAdded { add { } remove { } }
        public event SensorEventHandler SensorRemoved { add { } remove { } }
        public string GetReport() => "Fake hardware";
        public void Update() { Interlocked.Increment(ref Calls); OnUpdate(); }
        public void Accept(IVisitor visitor) { visitor.VisitHardware(this); }
        public void Traverse(IVisitor visitor) { foreach (var child in SubHardware) child.Accept(visitor); }
    }

    sealed class FakeSensor : ISensor
    {
        public FakeSensor(IHardware hardware) { Hardware = hardware; }
        public IControl Control => null;
        public IHardware Hardware { get; private set; }
        public Identifier Identifier => new Identifier(Hardware.Identifier, "sensor", "0");
        public int Index => 0;
        public bool IsDefaultHidden => false;
        public float? Max => Value;
        public float? Min => Value;
        public string Name { get; set; } = "Test sensor";
        public IReadOnlyList<IParameter> Parameters => new IParameter[0];
        public SensorType SensorType => SensorType.Temperature;
        public float? Value { get; set; } = 0;
        public IEnumerable<SensorValue> Values => new SensorValue[0];
        public TimeSpan ValuesTimeWindow { get; set; }
        public void Accept(IVisitor visitor) { visitor.VisitSensor(this); }
        public void Traverse(IVisitor visitor) { }
        public void ClearValues() { }
        public void ResetMax() { }
        public void ResetMin() { }
    }
}
