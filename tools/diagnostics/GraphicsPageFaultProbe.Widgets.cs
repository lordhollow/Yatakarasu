using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;
using Yata;

static partial class GraphicsPageFaultProbe
{
    internal static int RunIdleQueries(string output)
    {
        try
        {
            results.Add("case,round,iterations,elapsed_ms,total_page_faults,faults_per_iteration,private_bytes_delta,working_set_delta,gc0,gc1,gc2");
            var assembly = typeof(OwnerDrawWidget).Assembly;
            using (var target = new Bitmap(1280, 800, PixelFormat.Format32bppPArgb))
            using (var graphics = Graphics.FromImage(target))
            using (var clock = (OwnerDrawWidget)Activator.CreateInstance(assembly.GetType("Yata.Components.Widget.DigitalClock")))
            using (var meter = (OwnerDrawWidget)Activator.CreateInstance(assembly.GetType("Yata.Components.Widget.Tachometer"), 0f, 100f, new ProbeSensor()))
            using (var audio = (IDisposable)Activator.CreateInstance(assembly.GetType("Yata.Components.AudioVolumeController")))
            using (var window = new ProbeWindow())
            {
                var audioType = audio.GetType();
                var volume = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), audio, audioType.GetProperty("Volume").GetGetMethod());
                var capture = (Func<float>)Delegate.CreateDelegate(typeof(Func<float>), audio, audioType.GetProperty("CaptureVolume").GetGetMethod());
                var mute = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), audio, audioType.GetProperty("Mute").GetGetMethod());
                var captureMute = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), audio, audioType.GetProperty("CaptureMute").GetGetMethod());
                var foreground = (Action)Delegate.CreateDelegate(typeof(Action), assembly.GetType("Yata.GlobalWindowUtil").GetMethod("BackupForground"));
                meter.Update(); clock.Update(); window.Show(); Application.DoEvents();
                for (int round = 1; round <= 3; round++)
                {
                    Measure("digital-clock-update-draw", round, 200, () => { clock.Update(); DrawWidget(clock, graphics); });
                    Measure("six-tachometers-update-draw", round, 200, () => { for (int i = 0; i < 6; i++) { meter.Update(); DrawWidget(meter, graphics); } });
                    Measure("audio-four-property-reads", round, 200, () => { volume(); capture(); mute(); captureMute(); });
                    Measure("screen-query-and-unchanged-location", round, 200, () => { var bounds = Screen.AllScreens.Last().Bounds; window.Location = new Point(-20000 + bounds.Width % 2, -20000); });
                    Measure("foreground-backup", round, 200, foreground);
                }
            }
            File.WriteAllLines(output, results); Console.WriteLine("CSV: " + Path.GetFullPath(output)); return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr handle);
    internal static int SampleProcess(string output, int pid)
    {
        var handle = OpenProcess(0x1000, false, pid); // PROCESS_QUERY_LIMITED_INFORMATION, no injection or VM writes.
        if (handle == IntPtr.Zero) { Console.Error.WriteLine(new System.ComponentModel.Win32Exception()); return 1; }
        try
        {
            var samples = new List<string>(2100);
            samples.Add("elapsed_ms,page_faults,working_set,private_bytes");
            var timer = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                MemoryCounters counters;
                if (!GetProcessMemoryInfo(handle, out counters, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(MemoryCounters))))
                    throw new System.ComponentModel.Win32Exception();
                samples.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F3},{1},{2},{3}", timer.Elapsed.TotalMilliseconds, counters.PageFaultCount, counters.WorkingSet.ToUInt64(), counters.PrivateBytes.ToUInt64()));
                System.Threading.Thread.Sleep(10);
            } while (timer.Elapsed.TotalSeconds < 20);
            File.WriteAllLines(output, samples); Console.WriteLine("CSV: " + Path.GetFullPath(output)); return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        finally { CloseHandle(handle); }
    }
    internal static int RunWidgets(string output, string launcherFile)
    {
        try
        {
            results.Add("case,round,iterations,elapsed_ms,total_page_faults,faults_per_iteration,private_bytes_delta,working_set_delta,gc0,gc1,gc2");
            var assembly = typeof(OwnerDrawWidget).Assembly;
            using (var target = new Bitmap(1280, 800, PixelFormat.Format32bppPArgb))
            using (var graphics = Graphics.FromImage(target))
            using (var clock = (OwnerDrawWidget)Activator.CreateInstance(assembly.GetType("Yata.Components.Widget.AnalogClock")))
            using (var current = (OwnerDrawWidget)Activator.CreateInstance(assembly.GetType("Yata.Components.Widget.SensorCurrentList")))
            using (var clockArgb = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files", "clockback.png")))
            using (var clockPArgb = new Bitmap(clockArgb.Width, clockArgb.Height, PixelFormat.Format32bppPArgb))
            using (var font = new Font("BIZ UDゴシック", 9.5f, FontStyle.Bold))
            {
                Console.WriteLine("Clock image: " + clockArgb.Size + " / " + clockArgb.PixelFormat);
                using (var convert = Graphics.FromImage(clockPArgb)) convert.DrawImageUnscaled(clockArgb, 0, 0);
                var add = current.GetType().GetMethod("Add");
                for (int i = 0; i < 12; i++) add.Invoke(current, new object[] { new ProbeSensor(), "Sensor " + i, Color.Red });
                var parameterType = assembly.GetType("Yata.Components.Widget.SensorPlot+ConstructionParameter");
                var parameter = parameterType.GetProperty("WXGA").GetValue(null);
                using (var plot = (OwnerDrawWidget)Activator.CreateInstance(assembly.GetType("Yata.Components.Widget.SensorPlot"), parameter))
                {
                    plot.GetType().GetMethod("AddRange").Invoke(plot, new[] { current.GetType().GetProperty("Sensors").GetValue(current) });
                    for (int round = 1; round <= 3; round++)
                    {
                        Measure("analog-clock-widget", round, 200, () => DrawWidget(clock, graphics));
                        Measure("sensor-current-list-12", round, 200, () => DrawWidget(current, graphics));
                        Measure("sensor-plot-12x144", round, 100, () => DrawWidget(plot, graphics));
                        graphics.SetClip(new Rectangle(0, 0, 168, 168));
                        var rect = new Rectangle(0, 0, 168, 168);
                        Measure("clock-image-ARGB", round, 500, () => graphics.DrawImage(clockArgb, rect, rect, GraphicsUnit.Pixel));
                        Measure("clock-image-PArgb", round, 500, () => graphics.DrawImage(clockPArgb, rect, rect, GraphicsUnit.Pixel));
                        graphics.SetClip(new Rectangle(0, 0, 144, 192));
                        Measure("24-DrawString-calls", round, 200, () =>
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                graphics.DrawString("Sensor " + i, font, Brushes.LightGray, 8, 16 + i * 14);
                                graphics.DrawString("42.0 %", font, Brushes.LightGray, 82, 16 + i * 14);
                            }
                        });
                    }
                }
                ProfileLauncher(assembly, graphics, launcherFile);
            }
            File.WriteAllLines(output, results);
            Console.WriteLine("CSV: " + Path.GetFullPath(output)); return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    static void DrawWidget(OwnerDrawWidget widget, Graphics graphics)
    {
        graphics.ResetTransform(); graphics.SetClip(new Rectangle(0, 0, widget.Width, widget.Height)); widget.Draw(graphics);
    }
    static void ProfileLauncher(Assembly assembly, Graphics graphics, string source)
    {
        var definitions = new List<KeyValuePair<string, List<string[]>>>();
        if (!string.IsNullOrEmpty(source) && File.Exists(source))
        {
            var rows = File.ReadAllLines(source).Skip(1).Select(x => x.Split('\t')).Where(x => x.Length >= 4 && int.TryParse(x[0], out _) && int.TryParse(x[1], out _));
            foreach (var page in rows.GroupBy(x => int.Parse(x[0]) / 2).OrderBy(x => x.Key))
            {
                var entries = new List<string[]>();
                foreach (var fields in page)
                {
                    var image = fields.Length > 4 ? fields[4] : "";
                    if (image != "" && !image.Contains("::") && !Path.IsPathRooted(image)) image = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(source)), image);
                    entries.Add(new[] { (int.Parse(fields[0]) % 2).ToString(), fields[1], "App", "internal::EditLauncher", image, "" });
                }
                definitions.Add(new KeyValuePair<string, List<string[]>>("actual-page-" + (page.Key + 1), entries));
            }
        }
        var large = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "large-icon.png");
        using (var image = new Bitmap(256, 256, PixelFormat.Format32bppArgb))
        { using (var draw = Graphics.FromImage(image)) draw.Clear(Color.FromArgb(160, 200, 100, 50)); image.Save(large, ImageFormat.Png); }
        foreach (var kind in new[] { "atlas", "large-image", "fallback" })
        {
            var entries = new List<string[]>();
            for (int i = 0; i < 32; i++) entries.Add(new[] { (i / 16).ToString(), (i % 16).ToString(), "App " + i, "internal::EditLauncher", kind == "atlas" ? "InternalApplicationIconsImage::0,0" : kind == "large-image" ? large : "", "" });
            definitions.Add(new KeyValuePair<string, List<string[]>>(kind + "-32", entries));
        }
        var type = assembly.GetType("Yata.Components.Widget.CommandLauncher");
        var parameter = assembly.GetType("Yata.Components.Widget.CommandLauncher+ConstructionParameter").GetProperty("WXGA").GetValue(null);
        foreach (var definition in definitions)
        foreach (var success in new[] { true, false })
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "launcher-probe.txt");
            File.WriteAllLines(file, new[] { "probe-key" }.Concat(definition.Value.Select(x => string.Join("\t", x.Select((value, index) => index == 3 && !success ? "internal::ProbeNoOperation" : value)))), Encoding.UTF8);
            using (var launcher = (OwnerDrawWidget)Activator.CreateInstance(type, parameter))
            {
                type.GetMethod("LoadFromFile").Invoke(launcher, new object[] { file, null });
                launcher.Update(); DrawWidget(launcher, graphics);
                var first = definition.Value[0];
                int x = int.Parse(first[1]) * 80 + 40, y = int.Parse(first[0]) * 90 + 40;
                var indicator = type.GetField("lastClickState", BindingFlags.Instance | BindingFlags.NonPublic);
                var remaining = type.GetField("indicationLast", BindingFlags.Instance | BindingFlags.NonPublic);
                for (int round = 1; round <= 3; round++)
                {
                    if (success)
                    {
                        Measure(definition.Key + "-idle-tick", round, 1000, () => { if (launcher.Update()) throw new Exception("Idle launcher unexpectedly redraws."); });
                        Measure(definition.Key + "-page-redraw", round, 100, () => { type.GetMethod("ChangePage").Invoke(launcher, new object[] { 0 }); launcher.Update(); DrawWidget(launcher, graphics); });
                    }
                    Measure(definition.Key + (success ? "-green" : "-red") + "-cycle-30frames", round, 8, () =>
                    {
                        launcher.Click(x, y);
                        if ((bool)indicator.GetValue(launcher) != success) throw new Exception("Wrong focus color path.");
                        for (int frame = 0; frame < 30; frame++)
                        {
                            if (!launcher.Update()) throw new Exception("Focus animation did not redraw all frames.");
                            DrawWidget(launcher, graphics);
                        }
                        if ((int)remaining.GetValue(launcher) != 0 || launcher.Update()) throw new Exception("Focus indicator did not clear.");
                    }, warmup: 2);
                }
                ProfileLauncherWindow(assembly, launcher, definition.Key + (success ? "-green" : "-red"), x, y);
            }
        }
    }
    static void ProfileLauncherWindow(Assembly assembly, OwnerDrawWidget launcher, string name, int x, int y)
    {
        var type = assembly.GetType("Yata.WidgetContainer");
        var container = Activator.CreateInstance(type);
        type.GetMethod("Initialize").Invoke(container, new object[] { 1280, 800 });
        var update = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), container, type.GetMethod("Update"));
        var draw = (Action<Graphics>)Delegate.CreateDelegate(typeof(Action<Graphics>), container, type.GetMethod("Draw"));
        var click = (Action<int, int>)Delegate.CreateDelegate(typeof(Action<int, int>), container, type.GetMethod("Click"));
        type.GetMethods().Single(m => m.Name == "Add" && m.GetParameters().Length == 2).Invoke(container, new object[] { launcher, new Point(0, 620) });
        var context = BufferedGraphicsManager.Current;
        var originalMaximum = context.MaximumBuffer;
        try
        {
            using (var window = new ProbeWindow())
            {
                window.PaintFrame = draw;
                context.MaximumBuffer = new Size(Math.Max(originalMaximum.Width, 1281), Math.Max(originalMaximum.Height, 801));
                update(); window.Show(); Application.DoEvents();
                for (int round = 1; round <= 3; round++)
                {
                    int before = window.Frames;
                    Measure(name + "-WinForms-cycle-30frames", round, 4, () =>
                    {
                        click(x, y + 620);
                        for (int frame = 0; frame < 30; frame++)
                        {
                            if (!update()) throw new Exception("Container skipped a focus frame.");
                            window.PaintOnce();
                        }
                        if (update()) throw new Exception("Container kept redrawing after focus cleared.");
                    }, warmup: 2);
                    if (window.Frames - before != 180) throw new Exception("Focus did not use the full WM_PAINT path.");
                }
            }
        }
        finally
        {
            context.MaximumBuffer = originalMaximum;
            foreach (var field in new[] { "BacksurfaceGraphic", "Background", "Overlay", "BackSurface" })
                ((IDisposable)type.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(container)).Dispose();
        }
    }
    internal static int RunOsQueries(string output)
    {
        try
        {
            results.Add("case,round,iterations,elapsed_ms,total_page_faults,faults_per_iteration,private_bytes_delta,working_set_delta,gc0,gc1,gc2");
            var assembly = typeof(ISensor).Assembly;
            var cpu = assembly.GetType("LibreHardwareMonitor.Hardware.Cpu.CpuLoad").GetMethod("GetWindowsTimes", BindingFlags.Static | BindingFlags.NonPublic);
            var cpuArguments = new object[] { null, null };
            if ((bool)cpu.Invoke(null, cpuArguments))
                for (int round = 1; round <= 3; round++) Measure("CPU-OS-times-query", round, 100, () => cpu.Invoke(null, cpuArguments));
            var d3d = assembly.GetType("LibreHardwareMonitor.Hardware.D3DDisplayDevice");
            var ids = (string[])d3d.GetMethod("GetDeviceIdentifiers").Invoke(null, null);
            var query = d3d.GetMethod("GetDeviceInfoByIdentifier");
            Console.WriteLine("D3D device count: " + (ids == null ? 0 : ids.Length));
            if (ids != null)
                for (int i = 0; i < ids.Length; i++)
                {
                    var arguments = new object[] { ids[i], null };
                    if (!(bool)query.Invoke(null, arguments)) { Console.WriteLine("Adapter " + i + " unavailable or software-only."); continue; }
                    for (int round = 1; round <= 3; round++) Measure("D3D-adapter-" + i, round, 100, () => query.Invoke(null, arguments));
                }
            File.WriteAllLines(output, results);
            Console.WriteLine("CSV: " + Path.GetFullPath(output)); return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    sealed class ProbeSensor : ISensor
    {
        readonly SensorValue[] history = CreateHistory();
        static SensorValue[] CreateHistory()
        {
            var values = new SensorValue[144]; for (int i = 0; i < values.Length; i++) values[i] = new SensorValue(20 + i % 50, DateTime.Now.AddSeconds((i - 143) * 5)); return values;
        }
        public IControl Control => null;
        public IHardware Hardware => null;
        public Identifier Identifier => new Identifier("probe", "sensor");
        public int Index => 0;
        public bool IsDefaultHidden => false;
        public float? Max => 80;
        public float? Min => 10;
        public string Name { get; set; } = "Probe";
        public IReadOnlyList<IParameter> Parameters => new IParameter[0];
        public SensorType SensorType => SensorType.Load;
        public float? Value => 42;
        public IEnumerable<SensorValue> Values => history;
        public TimeSpan ValuesTimeWindow { get; set; }
        public void Accept(IVisitor visitor) { visitor.VisitSensor(this); }
        public void Traverse(IVisitor visitor) { }
        public void ClearValues() { }
        public void ResetMin() { }
        public void ResetMax() { }
    }
}

static class WidgetProbeRunner
{
    [STAThread]
    static int Main(string[] args) => args.Length > 1 && args[1] == "--os-queries"
        ? GraphicsPageFaultProbe.RunOsQueries(args[0]) : args.Length > 1 && args[1] == "--idle-queries"
        ? GraphicsPageFaultProbe.RunIdleQueries(args[0]) : args.Length > 2 && args[1] == "--process-samples"
        ? GraphicsPageFaultProbe.SampleProcess(args[0], int.Parse(args[2])) : GraphicsPageFaultProbe.RunWidgets(args[0], args.Length > 1 ? args[1] : null);
}
