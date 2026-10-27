using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using LibreHardwareMonitor.Hardware;

namespace Yata.Components
{
    // Shared by both windows. Polling never runs on the drawing or HW update thread.
    internal sealed class CodexUsageMonitor : IDisposable
    {
        internal static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);
        internal static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(10);
        readonly object gate = new object();
        readonly Func<DateTimeOffset> clock;
        readonly Func<CancellationToken, Task<CodexUsageSnapshot>> read;
        CancellationTokenSource cancellation;
        CodexUsageSnapshot snapshot;
        DateTimeOffset fetchedAt, nextAttempt;
        bool enabled, disposed, busy;
        string lastError;

        internal CodexUsageMonitor() : this(new CodexRateLimitClient().ReadAsync, () => DateTimeOffset.UtcNow) { }
        internal CodexUsageMonitor(Func<CancellationToken, Task<CodexUsageSnapshot>> read, Func<DateTimeOffset> clock)
        { this.read = read; this.clock = clock; }
        internal void SetEnabled(bool value)
        {
            lock (gate)
            {
                if (disposed || enabled == value) return;
                enabled = value;
                snapshot = null;
                nextAttempt = DateTimeOffset.MinValue;
                if (!value) cancellation?.Cancel();
            }
        }
        internal Task RefreshIfDue()
        {
            lock (gate)
            {
                if (!enabled || disposed || busy || clock() < nextAttempt) return Task.CompletedTask;
                busy = true;
                nextAttempt = clock() + PollInterval;
                cancellation = new CancellationTokenSource();
                var current = cancellation;
                return Task.Run(async () =>
                {
                    try
                    {
                        var result = await read(current.Token).ConfigureAwait(false);
                        lock (gate)
                        {
                            if (!disposed && enabled && !current.IsCancellationRequested)
                            { snapshot = result; fetchedAt = clock(); lastError = null; }
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (gate)
                        {
                            snapshot = null;
                            // Do not log server payloads, stderr, account IDs or credentials.
                            var error = ex.GetType().Name;
                            if (!current.IsCancellationRequested && lastError != error)
                            { System.Diagnostics.Trace.TraceWarning("Codex usage unavailable: " + error); lastError = error; }
                        }
                    }
                    finally
                    {
                        lock (gate) { busy = false; cancellation = null; current.Dispose(); }
                    }
                });
            }
        }
        internal float? Remaining(int minutes)
        {
            lock (gate)
            {
                var now = clock();
                return enabled && !disposed && snapshot != null && now - fetchedAt <= MaxAge
                    ? snapshot.Remaining(minutes, now) : null;
            }
        }
        public void Dispose()
        {
            lock (gate) { disposed = true; enabled = false; snapshot = null; cancellation?.Cancel(); }
        }
    }

    internal sealed class CodexUsageSnapshot
    {
        readonly Dictionary<int, Window> windows = new Dictionary<int, Window>();
        readonly DateTimeOffset observedAt;
        CodexUsageSnapshot(DateTimeOffset observedAt) { this.observedAt = observedAt; }
        sealed class Window { internal float Remaining; internal DateTimeOffset? Reset; internal bool ZeroUsage; }
        internal float? Remaining(int minutes, DateTimeOffset now)
        {
            Window window;
            return windows.TryGetValue(minutes, out window) && (!window.Reset.HasValue || now < window.Reset ||
                (window.ZeroUsage && window.Reset <= observedAt))
                ? window.Remaining : (float?)null;
        }
        internal static CodexUsageSnapshot Parse(string json, DateTimeOffset? observedAt = null)
        {
            var root = new JavaScriptSerializer().DeserializeObject(json) as Dictionary<string, object>;
            if (root == null) throw new FormatException("Invalid rate limit response.");
            var bucket = Object(root, "rateLimits");
            if (root.ContainsKey("rateLimitsByLimitId") && root["rateLimitsByLimitId"] != null)
                bucket = Object(Object(root, "rateLimitsByLimitId"), "codex");
            if (bucket != null && bucket.ContainsKey("limitId") && bucket["limitId"] != null && !Equals(bucket["limitId"], "codex")) bucket = null;
            var snapshot = new CodexUsageSnapshot(observedAt ?? DateTimeOffset.UtcNow);
            foreach (var key in new[] { "primary", "secondary" })
            {
                var source = Object(bucket, key);
                double minutes, used, reset;
                if (!Number(source, "windowDurationMins", out minutes) || (minutes != 300 && minutes != 10080) ||
                    !Number(source, "usedPercent", out used)) continue;
                DateTimeOffset? resetAt = null;
                if (source.ContainsKey("resetsAt") && source["resetsAt"] != null)
                {
                    if (!Number(source, "resetsAt", out reset) || reset != Math.Truncate(reset)) continue;
                    try { resetAt = DateTimeOffset.FromUnixTimeSeconds(checked((long)reset)); }
                    catch (ArgumentOutOfRangeException) { continue; }
                    catch (OverflowException) { continue; }
                }
                // Durations identify windows; primary/secondary order is not a contract.
                // A newly observed zero-usage window can retain the previous reset time.
                // Keep its explicit 100% reading; a cached window crossing a future reset still expires.
                snapshot.windows[(int)minutes] = new Window { Remaining = (float)Math.Max(0, Math.Min(100, 100 - used)), Reset = resetAt, ZeroUsage = used == 0 };
            }
            return snapshot;
        }
        internal static Dictionary<string, object> Object(Dictionary<string, object> value, string key)
            => value != null && value.TryGetValue(key, out var child) ? child as Dictionary<string, object> : null;
        static bool Number(Dictionary<string, object> value, string key, out double number)
        {
            number = 0;
            if (value == null || !value.TryGetValue(key, out var raw) || !(raw is int || raw is long || raw is double || raw is decimal)) return false;
            number = Convert.ToDouble(raw, System.Globalization.CultureInfo.InvariantCulture);
            return !double.IsNaN(number) && !double.IsInfinity(number);
        }
    }

    internal sealed class CodexUsageSensor : ISensor
    {
        readonly CodexUsageMonitor monitor;
        readonly int minutes;
        internal CodexUsageSensor(CodexUsageMonitor monitor, bool weekly)
        {
            this.monitor = monitor; minutes = weekly ? 10080 : 300;
            Identifier = new Identifier("codex", "remaining", weekly ? "1w" : "5h");
            Name = weekly ? "GPT残量 1w" : "GPT残量 5h";
            Index = weekly ? 1 : 0;
        }
        public Identifier Identifier { get; }
        public string Name { get; set; }
        public float? Value => monitor.Remaining(minutes);
        public SensorType SensorType => SensorType.Load;
        public IHardware Hardware => null;
        public IControl Control => null;
        public int Index { get; }
        public bool IsDefaultHidden => false;
        public float? Min => 0;
        public float? Max => 100;
        public IReadOnlyList<IParameter> Parameters => Array.Empty<IParameter>();
        public IEnumerable<SensorValue> Values => Array.Empty<SensorValue>();
        public TimeSpan ValuesTimeWindow { get; set; }
        public void Accept(IVisitor visitor) => visitor.VisitSensor(this);
        public void Traverse(IVisitor visitor) { }
        public void ClearValues() { }
        public void ResetMin() { }
        public void ResetMax() { }
    }
}
