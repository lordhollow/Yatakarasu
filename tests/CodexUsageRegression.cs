using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using LibreHardwareMonitor.Hardware;
using Yata.Components;
using Yata.Settings;

static class CodexUsageRegression
{
    static readonly DateTimeOffset Start = DateTimeOffset.FromUnixTimeSeconds(1793000000);
    const string Limits = "{\"rateLimits\":{\"primary\":{\"usedPercent\":25,\"windowDurationMins\":300},\"secondary\":{\"usedPercent\":60,\"windowDurationMins\":10080}}}";
    static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "app-server") return FakeServer().GetAwaiter().GetResult();
        try
        {
            if (args.Contains("--live"))
            {
                var live = new CodexRateLimitClient().ReadAsync(CancellationToken.None).GetAwaiter().GetResult();
                Console.WriteLine("LIVE: 5h=" + live.Remaining(300, DateTimeOffset.UtcNow) + "%, 1w=" + live.Remaining(10080, DateTimeOffset.UtcNow) + "%");
                Check(live.Remaining(300, DateTimeOffset.UtcNow).HasValue && live.Remaining(10080, DateTimeOffset.UtcNow).HasValue, "Live windows are unavailable.");
                return 0;
            }
            Run("Remaining percent, bucket preference and window duration", Parsing);
            Run("Missing, invalid and expired windows stay unavailable", Missing);
            Run("Fresh idle windows retain 100 percent with a past reset timestamp", IdleWindows);
            Run("Polling is opt-in, shared, throttled and single-flight", Polling);
            Run("Failures, stale cache, cancellation and recovery", Recovery);
            Run("Account sensors use normal selection, history and aggregates", Integration);
            Run("JSONL handshake, notifications, errors and timeout", Protocol);
            Console.WriteLine("All 7 Codex usage checks passed.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    static void Run(string title, Action test) { test(); Console.WriteLine("PASS: " + title); }
    static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    static void Parsing()
    {
        var value = CodexUsageSnapshot.Parse(Limits);
        Check(value.Remaining(300, Start) == 75 && value.Remaining(10080, Start) == 40, "Usage was not subtracted from 100.");
        var swapped = CodexUsageSnapshot.Parse("{\"rateLimitsByLimitId\":{\"codex\":{\"primary\":{\"usedPercent\":7,\"windowDurationMins\":10080},\"secondary\":{\"usedPercent\":11,\"windowDurationMins\":300}}},\"rateLimits\":{\"primary\":{\"usedPercent\":99,\"windowDurationMins\":300}}}");
        Check(swapped.Remaining(300, Start) == 89 && swapped.Remaining(10080, Start) == 93, "Bucket/duration selection failed.");
        var clamped = CodexUsageSnapshot.Parse(Limits.Replace("25", "125").Replace("60", "-1"));
        Check(clamped.Remaining(300, Start) == 0 && clamped.Remaining(10080, Start) == 100, "Percentage clamping failed.");
    }
    static void Missing()
    {
        foreach (var json in new[] { "{}", "{\"rateLimits\":null}", Limits.Replace("25", "null").Replace("60", "null"), Limits.Replace("300", "15").Replace("10080", "1440"),
            Limits.Replace("25", "\"25\"").Replace("60", "true"), "{\"rateLimitsByLimitId\":{\"other\":{}},\"rateLimits\":{\"primary\":{\"usedPercent\":0,\"windowDurationMins\":300}}}", "{\"rateLimits\":{\"limitId\":\"other\",\"primary\":{\"usedPercent\":0,\"windowDurationMins\":300}}}" })
        {
            var missing = CodexUsageSnapshot.Parse(json);
            Check(!missing.Remaining(300, Start).HasValue && !missing.Remaining(10080, Start).HasValue, "Unknown data became a valid reading.");
        }
        var expired = CodexUsageSnapshot.Parse("{\"rateLimits\":{\"primary\":{\"usedPercent\":0,\"windowDurationMins\":300,\"resetsAt\":1793000000}}}", Start.AddSeconds(-1));
        Check(expired.Remaining(300, Start.AddSeconds(-1)) == 100 && !expired.Remaining(300, Start).HasValue, "Reset must invalidate the old reading.");
    }
    static void IdleWindows()
    {
        var json = "{\"rateLimits\":{\"primary\":{\"usedPercent\":0,\"windowDurationMins\":300,\"resetsAt\":1793000000},\"secondary\":{\"usedPercent\":40,\"windowDurationMins\":10080}}}";
        var fresh = CodexUsageSnapshot.Parse(json, Start);
        Check(fresh.Remaining(300, Start) == 100 && fresh.Remaining(10080, Start) == 60, "Fresh zero usage was mistaken for missing data at reset.");
        Check(CodexUsageSnapshot.Parse(json, Start.AddMinutes(1)).Remaining(300, Start.AddMinutes(1)) == 100, "Fresh idle reading with a past reset was rejected.");
        Check(!CodexUsageSnapshot.Parse(json.Replace("\"usedPercent\":0", "\"usedPercent\":1"), Start.AddMinutes(1)).Remaining(300, Start.AddMinutes(1)).HasValue, "Expired nonzero usage was accepted.");
        Check(!CodexUsageSnapshot.Parse(json.Replace("\"usedPercent\":0", "\"usedPercent\":null"), Start).Remaining(300, Start).HasValue, "Missing usage was fabricated as 100 percent.");
        var now = Start;
        using (var monitor = new CodexUsageMonitor(token => Task.FromResult(CodexUsageSnapshot.Parse(json, now)), () => now))
        {
            monitor.SetEnabled(true); monitor.RefreshIfDue().Wait();
            var sensor = new CodexUsageSensor(monitor, false);
            using (var meter = new Yata.Components.Widget.Tachometer(new[] { new Yata.Components.Widget.Tachometer.Layer(0, 100, sensor, "GPT(5h)", "%", true) }))
            using (var image = new System.Drawing.Bitmap(72, 72))
            using (var graphics = System.Drawing.Graphics.FromImage(image))
            {
                meter.Update(); meter.Draw(graphics);
                Check(meter.DisplayText == "GPT(5h)\r\n100.0\r\n%" && image.GetPixel(60, 36).ToArgb() == System.Drawing.Color.DarkSlateBlue.ToArgb(), "Fresh 100 percent rendered as a missing red meter.");
                now += CodexUsageMonitor.MaxAge + TimeSpan.FromSeconds(1);
                meter.Update(); meter.Draw(graphics);
                Check(!sensor.Value.HasValue && meter.DisplayText.Contains("----") && image.GetPixel(60, 36).ToArgb() == System.Drawing.Color.Red.ToArgb(), "Idle reading bypassed cache expiry.");
            }
        }
    }
    static void Polling()
    {
        var now = Start; int calls = 0;
        var pending = new TaskCompletionSource<CodexUsageSnapshot>();
        using (var monitor = new CodexUsageMonitor(token => { Interlocked.Increment(ref calls); return pending.Task; }, () => now))
        {
            monitor.RefreshIfDue().Wait(); Check(calls == 0, "Unselected sensors started a process.");
            monitor.SetEnabled(true);
            var first = monitor.RefreshIfDue();
            Check(SpinWait.SpinUntil(() => calls == 1, 2000), "Read did not start.");
            for (int i = 0; i < 100; i++) monitor.RefreshIfDue().Wait();
            Check(calls == 1 && !monitor.Remaining(300).HasValue, "Concurrent reads or a fabricated initial reading.");
            pending.SetResult(CodexUsageSnapshot.Parse(Limits)); first.Wait();
            Check(monitor.Remaining(300) == 75 && monitor.Remaining(10080) == 40, "Both windows did not share a read.");
            now += CodexUsageMonitor.PollInterval - TimeSpan.FromSeconds(1);
            monitor.RefreshIfDue().Wait(); Check(calls == 1, "Polled too frequently.");
            now += TimeSpan.FromSeconds(1);
            monitor.RefreshIfDue().Wait(); Check(calls == 2, "Did not poll at the boundary.");
        }
    }
    static void Recovery()
    {
        var now = Start; bool fail = false;
        using (var monitor = new CodexUsageMonitor(token => fail ? Task.FromException<CodexUsageSnapshot>(new IOException("private error")) : Task.FromResult(CodexUsageSnapshot.Parse(Limits)), () => now))
        {
            monitor.SetEnabled(true); monitor.RefreshIfDue().Wait();
            now += CodexUsageMonitor.MaxAge + TimeSpan.FromSeconds(1);
            Check(!monitor.Remaining(300).HasValue, "Stale reading retained.");
            fail = true; monitor.RefreshIfDue().Wait();
            Check(!monitor.Remaining(300).HasValue, "Failure retained the old reading.");
            fail = false; now += CodexUsageMonitor.PollInterval; monitor.RefreshIfDue().Wait();
            Check(monitor.Remaining(300) == 75, "Failed read did not recover.");
            monitor.SetEnabled(false); Check(!monitor.Remaining(300).HasValue, "Disabled data retained.");
        }
        var entered = new TaskCompletionSource<bool>();
        using (var monitor = new CodexUsageMonitor(async token => { entered.SetResult(true); await Task.Delay(5000, token); return CodexUsageSnapshot.Parse(Limits); }, () => now))
        {
            monitor.SetEnabled(true); var task = monitor.RefreshIfDue(); entered.Task.Wait(); monitor.SetEnabled(false);
            Check(task.Wait(2000) && !monitor.Remaining(300).HasValue, "Disable did not cancel a pending read.");
        }
    }
    static void Integration()
    {
        using (var usage = new CodexUsageMonitor(token => Task.FromResult(CodexUsageSnapshot.Parse(Limits)), () => Start))
        {
            var sensors = new ISensor[] { new CodexUsageSensor(usage, false), new CodexUsageSensor(usage, true) };
            var choices = SensorChoice.FromSensors(sensors);
            Check(choices.Count == 2 && choices.All(x => x.Unit == "%") && choices.Any(x => x.Id == "/codex/remaining/5h"), "Picker is missing the percent sensors.");
            var hw = (HardwareMonitor)FormatterServices.GetUninitializedObject(typeof(HardwareMonitor));
            typeof(HardwareMonitor).GetProperty("AllSensors").SetValue(hw, sensors.ToList());
            typeof(HardwareMonitor).GetField("codexUsage", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(hw, usage);
            hw.Startup(TimeSpan.FromMinutes(12), new[] { "/codex/remaining/5h", "/codex/remaining/1w", "/codex/remaining/5h+/codex/remaining/1w" });
            Check(hw.UpdateTargetHardware.Count == 0, "Account sensor treated as hardware.");
            Check(!hw["/codex/remaining/5h"].Value.HasValue, "An initial history reading became zero before fetching.");
            usage.RefreshIfDue().Wait(); hw.UpdateAsync = false;
            for (int i = 0; i < 5; i++) hw.Update();
            Check(hw["/codex/remaining/5h"].Value == 75 && hw["/codex/remaining/1w"].Value == 40 && hw.UpdateTargetSensor.Last().Value == 115, "Selection/history did not update.");
            Check(hw.UpdateTargetSensor.First().Values.Any(), "History missing.");
            hw.Startup(TimeSpan.Zero, Array.Empty<string>());
            Check(!usage.Remaining(300).HasValue, "Removing the last account sensor did not disable polling.");
        }
    }
    static void Protocol()
    {
        var exe = Assembly.GetExecutingAssembly().Location;
        var client = new CodexRateLimitClient(() => exe, TimeSpan.FromSeconds(2));
        using (var canceled = new CancellationTokenSource())
        {
            canceled.Cancel(); bool resolved = false;
            try { new CodexRateLimitClient(() => { resolved = true; return exe; }).ReadAsync(canceled.Token).GetAwaiter().GetResult(); }
            catch (OperationCanceledException) { }
            Check(!resolved, "A canceled read started resolving/launching Codex.");
        }
        try
        {
            Environment.SetEnvironmentVariable("YATA_TEST_CODEX_MODE", "ok");
            var result = client.ReadAsync(CancellationToken.None).GetAwaiter().GetResult();
            Check(result.Remaining(300, Start) == 75, "JSONL response was not parsed.");
            foreach (var mode in new[] { "error", "invalid", "timeout" })
            {
                Environment.SetEnvironmentVariable("YATA_TEST_CODEX_MODE", mode);
                bool rejected = false;
                try { client.ReadAsync(CancellationToken.None).GetAwaiter().GetResult(); }
                catch (Exception) { rejected = true; }
                Check(rejected, "Protocol failure was not detected: " + mode);
            }
        }
        finally { Environment.SetEnvironmentVariable("YATA_TEST_CODEX_MODE", null); }
    }
    static async Task<int> FakeServer()
    {
        var serializer = new JavaScriptSerializer();
        var init = serializer.Deserialize<Dictionary<string, object>>(Console.ReadLine());
        if (!Equals(init["method"], "initialize")) return 2;
        Console.WriteLine("{\"id\":1,\"result\":{}}");
        var initialized = serializer.Deserialize<Dictionary<string, object>>(Console.ReadLine());
        var request = serializer.Deserialize<Dictionary<string, object>>(Console.ReadLine());
        if (!Equals(initialized["method"], "initialized") || !Equals(request["method"], "account/rateLimits/read")) return 3;
        var mode = Environment.GetEnvironmentVariable("YATA_TEST_CODEX_MODE");
        if (mode == "timeout") await Task.Delay(30000);
        Console.WriteLine("{\"method\":\"account/rateLimits/updated\",\"params\":{}}");
        Console.WriteLine(mode == "error" ? "{\"id\":2,\"error\":{\"message\":\"private test payload\"}}" : mode == "invalid" ? "invalid-json" : "{\"id\":2,\"result\":" + Limits + "}");
        return 0;
    }
}
