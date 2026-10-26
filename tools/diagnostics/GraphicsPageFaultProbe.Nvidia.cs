using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using LibreHardwareMonitor.Hardware;

static partial class GraphicsPageFaultProbe
{
    // Only audited read paths are invoked. Never construct Computer or
    // NvidiaGpu: their initialization/Close paths can restore fan control.
    internal static int RunNvidiaQueries(string output)
    {
        try
        {
            results.Add("case,round,iterations,elapsed_ms,total_page_faults,faults_per_iteration,private_bytes_delta,working_set_delta,gc0,gc1,gc2");
            var assembly = typeof(ISensor).Assembly;
            var api = assembly.GetType("LibreHardwareMonitor.Interop.NvApi", true);
            api.GetMethod("Initialize").Invoke(null, null);
            var enumerate = (Delegate)api.GetProperty("NvAPI_EnumPhysicalGPUs").GetValue(null);
            if (enumerate == null) throw new InvalidOperationException("NVAPI is unavailable.");
            var handleType = api.GetNestedType("NvPhysicalGpuHandle", BindingFlags.Public | BindingFlags.NonPublic);
            var handles = Array.CreateInstance(handleType, 64);
            var arguments = new object[] { handles, 0 };
            if (enumerate.DynamicInvoke(arguments).ToString() != "OK") throw new InvalidOperationException("GPU enumeration failed.");
            int count = (int)arguments[1];
            Console.WriteLine("NVIDIA devices: " + count);
            var type = assembly.GetType("LibreHardwareMonitor.Hardware.Gpu.NvidiaGpu", true);
            for (var parent = type; parent != typeof(object); parent = parent.BaseType)
                if (parent.GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) != null)
                    throw new InvalidOperationException("An unaudited finalizer prevents the read-only probe.");
            for (int index = 0; index < count; index++)
            {
                var device = FormatterServices.GetUninitializedObject(type);
                SetField(type, device, "_handle", handles.GetValue(index));
                SetField(type, device, "_clockVersion", 3);
                var queries = new List<KeyValuePair<string, Action>>();
                foreach (var methodName in new[] { "GetThermalSettings", "GetClockFrequencies", "GetFanCoolersStatus", "GetCoolerSettings", "GetDynamicPstatesInfoEx", "GetUsages", "GetPowerTopology", "GetVoltRailsStatus", "GetTachReading" })
                {
                    var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
                    var values = new object[] { null };
                    method.Invoke(device, values);
                    if (methodName == "GetClockFrequencies")
                        for (int version = 2; values[0].ToString() != "OK" && version >= 1; version--)
                        { SetField(type, device, "_clockVersion", version); method.Invoke(device, values); }
                    if (values[0].ToString() != "OK") { Console.WriteLine(methodName + ": " + values[0]); continue; }
                    queries.Add(new KeyValuePair<string, Action>(methodName, () =>
                    {
                        method.Invoke(device, values);
                        if (values[0].ToString() != "OK") throw new InvalidOperationException(methodName + " failed during sampling.");
                    }));
                }
                var thermal = type.GetMethod("GetThermalSensors", BindingFlags.Instance | BindingFlags.NonPublic);
                uint mask = 0;
                for (int bit = 0; bit < 32; bit++)
                {
                    var values = new object[] { 1u << bit, null }; thermal.Invoke(device, values);
                    if (values[1].ToString() != "OK") break;
                    mask = (1u << bit) * 2 - 1;
                }
                if (mask != 0)
                {
                    var values = new object[] { mask, null };
                    queries.Add(new KeyValuePair<string, Action>("GetThermalSensors", () =>
                    {
                        thermal.Invoke(device, values);
                        if (values[1].ToString() != "OK") throw new InvalidOperationException("Thermal query failed.");
                    }));
                }
                // Map the display handle to this physical adapter before memory queries.
                var displays = (Delegate)api.GetProperty("NvAPI_EnumNvidiaDisplayHandle").GetValue(null);
                var map = (Delegate)api.GetProperty("NvAPI_GetPhysicalGPUsFromDisplay").GetValue(null);
                if (displays != null && map != null)
                    for (int display = 0; display < 64; display++)
                    {
                        var displayArguments = new object[] { display, Activator.CreateInstance(api.GetNestedType("NvDisplayHandle", BindingFlags.Public | BindingFlags.NonPublic)) };
                        if (displays.DynamicInvoke(displayArguments).ToString() != "OK") break;
                        var mapped = Array.CreateInstance(handleType, 64);
                        var mapArguments = new object[] { displayArguments[1], mapped, 0u };
                        if (map.DynamicInvoke(mapArguments).ToString() != "OK") continue;
                        bool match = false;
                        for (int i = 0; i < (uint)mapArguments[2]; i++) match |= mapped.GetValue(i).Equals(handles.GetValue(index));
                        if (!match) continue;
                        SetField(type, device, "_displayHandle", displayArguments[1]);
                        var method = type.GetMethod("GetMemoryInfo", BindingFlags.Instance | BindingFlags.NonPublic);
                        var values = new object[] { null };
                        method.Invoke(device, values);
                        if (values[0].ToString() == "OK") queries.Add(new KeyValuePair<string, Action>("GetMemoryInfo", () => method.Invoke(device, values)));
                        break;
                    }
                AddNvmlQueries(assembly, api, handles.GetValue(index), index, queries);
                foreach (var query in queries)
                    for (int round = 1; round <= 3; round++) Measure("NVIDIA-" + index + "-" + query.Key, round, 30, query.Value, warmup: 5);
                ProfileOptionalUpdate(type, device, index, queries);
            }
            File.WriteAllLines(output, results); Console.WriteLine("CSV: " + Path.GetFullPath(output)); return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    static void SetField(Type type, object instance, string name, object value)
        => type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, value);

    static void ProfileOptionalUpdate(Type type, object device, int index, List<KeyValuePair<string, Action>> queries)
    {
        // Exercise the actual Update method with only the two audited optional
        // groups populated. Other groups, all controls and NVML handles stay null.
        for (var parent = type; parent != typeof(object); parent = parent.BaseType)
            foreach (var field in parent.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                if (field.FieldType == typeof(Identifier)) field.SetValue(device, new Identifier("gpu-nvidia", index.ToString()));
        var sensorType = type.GetField("_coreVoltage", BindingFlags.Instance | BindingFlags.NonPublic).FieldType;
        var constructor = Array.Find(sensorType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), c => c.GetParameters().Length == 5);
        var settings = new ProbeSettings();
        Func<int, SensorType, ISensor> create = (i, kind) =>
        {
            var sensor = (ISensor)constructor.Invoke(new object[] { "Probe", i, kind, device, settings });
            sensor.ValuesTimeWindow = TimeSpan.Zero; return sensor;
        };
        SetField(type, device, "_hotSpotTemperature", create(0, SensorType.Temperature));
        SetField(type, device, "_memoryJunctionTemperature", create(1, SensorType.Temperature));
        // Avoid display memory reads in this optional-group-only comparison.
        SetField(type, device, "_displayHandle", null);
        var powers = Array.CreateInstance(sensorType, 4);
        for (int i = 0; i < powers.Length; i++) powers.SetValue(create(i, SensorType.Load), i);
        var voltage = create(0, SensorType.Voltage);
        bool hasPower = queries.Exists(q => q.Key == "GetPowerTopology");
        bool hasVoltage = queries.Exists(q => q.Key == "GetVoltRailsStatus");
        SetField(type, device, "_powers", hasPower ? powers : null);
        SetField(type, device, "_coreVoltage", hasVoltage ? voltage : null);
        var policyType = typeof(Yata.OwnerDrawWidget).Assembly.GetType("Yata.Components.NvidiaOptionalQueries", true);
        var policy = Activator.CreateInstance(policyType, true);
        var configure = policyType.GetMethod("Configure", BindingFlags.Instance | BindingFlags.NonPublic);
        var restore = policyType.GetMethod("Restore", BindingFlags.Instance | BindingFlags.NonPublic);
        var hardware = new[] { (IHardware)device };
        try
        {
            for (int round = 1; round <= 3; round++) Measure("NVIDIA-" + index + "-optional-Update-before", round, 30, hardware[0].Update, warmup: 5);
            configure.Invoke(policy, new object[] { hardware, new HashSet<ISensor>() });
            for (int round = 1; round <= 3; round++) Measure("NVIDIA-" + index + "-optional-Update-unused", round, 30, hardware[0].Update, warmup: 5);
            if (hasPower)
            {
                configure.Invoke(policy, new object[] { hardware, new HashSet<ISensor> { (ISensor)powers.GetValue(0) } });
                Measure("NVIDIA-" + index + "-optional-Update-power-selected", 1, 30, hardware[0].Update, warmup: 5);
            }
            if (hasVoltage)
            {
                configure.Invoke(policy, new object[] { hardware, new HashSet<ISensor> { voltage } });
                Measure("NVIDIA-" + index + "-optional-Update-voltage-selected", 1, 30, hardware[0].Update, warmup: 5);
            }
        }
        finally { restore.Invoke(policy, null); }
    }

    sealed class ProbeSettings : ISettings
    {
        public bool Contains(string name) => false;
        public string GetValue(string name, string value) => value;
        public void SetValue(string name, string value) { }
        public void Remove(string name) { }
    }

    static void AddNvmlQueries(Assembly assembly, Type api, object gpuHandle, int index, List<KeyValuePair<string, Action>> queries)
    {
        var type = assembly.GetType("LibreHardwareMonitor.Interop.NvidiaML", true);
        if (!(bool)type.GetMethod("Initialize").Invoke(null, null)) return;
        object device = null;
        var bus = (Delegate)api.GetProperty("NvAPI_GPU_GetBusId").GetValue(null);
        if (bus != null)
        {
            var values = new object[] { gpuHandle, 0u };
            if (bus.DynamicInvoke(values).ToString() == "OK")
                device = type.GetMethod("NvmlDeviceGetHandleByPciBusId").Invoke(null, new object[] { string.Format(System.Globalization.CultureInfo.InvariantCulture, "0000:{0:X2}:00.0", values[1]) });
        }
        if (device == null) device = type.GetMethod("NvmlDeviceGetHandleByIndex").Invoke(null, new object[] { index });
        if (device == null) return;
        var power = type.GetMethod("NvmlDeviceGetPowerUsage");
        var powerArguments = new[] { device };
        if (power.Invoke(null, powerArguments) != null) queries.Add(new KeyValuePair<string, Action>("NVML-Power", () => power.Invoke(null, powerArguments)));
        var throughput = type.GetMethod("NvmlDeviceGetPcieThroughput");
        var counter = type.GetNestedType("NvmlPcieUtilCounter", BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var name in new[] { "RxBytes", "TxBytes" })
        {
            var values = new object[] { device, Enum.Parse(counter, name) };
            if (throughput.Invoke(null, values) != null) queries.Add(new KeyValuePair<string, Action>("NVML-" + name, () => throughput.Invoke(null, values)));
        }
        // Process-local NVML/NVAPI lifetime ends with the probe; no device Close call.
    }
}
