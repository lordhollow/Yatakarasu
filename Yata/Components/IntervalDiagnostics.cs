using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;


namespace Yata.Components
{
    // On-demand process counters. Concurrent scopes are marked, never summed as
    // disjoint attribution. No counters, files or allocations are used while off.
    internal static class PageFaultDiagnostics
    {
        [StructLayout(LayoutKind.Sequential)]
        struct MemoryCounters
        {
            public uint Size, PageFaults;
            public UIntPtr PeakWorkingSet, WorkingSet, PeakPagedPool, PagedPool;
            public UIntPtr PeakNonPagedPool, NonPagedPool, Pagefile, PeakPagefile, PrivateBytes;
        }
        [DllImport("psapi.dll", SetLastError = true)]
        static extern bool GetProcessMemoryInfo(IntPtr process, out MemoryCounters counters, uint size);
        static uint ReadFaults()
        {
            MemoryCounters memory;
            if (!GetProcessMemoryInfo(new IntPtr(-1), out memory, (uint)Marshal.SizeOf(typeof(MemoryCounters))))
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            return memory.PageFaults;
        }
        internal static uint Difference(uint before, uint after) => unchecked(after - before);
        static Session current;
        internal static bool IsRunning => Volatile.Read(ref current) != null;
        internal static string DefaultDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Yatakarasu", "diagnostics");

        internal static void Start(TimeSpan duration, string directory)
        {
            if (duration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(duration));
            lock (typeof(PageFaultDiagnostics))
            {
                if (current != null) throw new InvalidOperationException("Page fault recording is already running.");
                Directory.CreateDirectory(directory);
                Volatile.Write(ref current, new Session(duration, Path.Combine(directory, "page-faults-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture) + ".csv")));
            }
        }

        internal static Scope Measure(string target)
        {
            var session = Volatile.Read(ref current);
            if (session == null) return default(Scope);
            lock (session.Gate)
            {
                if (!session.Accepting) return default(Scope);
                Statistics statistics;
                if (!session.Targets.TryGetValue(target, out statistics))
                    session.Targets.Add(target, statistics = new Statistics { Target = target });
                try
                {
                    var first = ReadFaults();
                    var overlaps = session.Active++ > 0;
                    var revision = ++session.Revision;
                    return new Scope(session, statistics, first, Stopwatch.GetTimestamp(), revision, overlaps);
                }
                catch (Exception ex)
                {
                    Interlocked.CompareExchange(ref current, null, session);
                    Trace.TraceError("Page fault counter: " + ex);
                    return default(Scope);
                }
            }
        }

        // Called by the normal UI tick; waits for in-flight hardware scopes before
        // exporting so a slow or failed device cannot leave a partial final row.
        internal static string Complete(bool force = false)
        {
            var session = Volatile.Read(ref current);
            if (session == null) return null;
            lock (typeof(PageFaultDiagnostics))
            lock (session.Gate)
            {
                if (!ReferenceEquals(current, session)) return null;
                if (!force && session.Clock.Elapsed < session.Duration) return null;
                session.Accepting = false;
                if (session.Active != 0) return null;
                Volatile.Write(ref current, null);
                var faults = Difference(session.FirstFaults, ReadFaults());
                var elapsed = session.Clock.Elapsed.TotalMilliseconds;
                var lines = new List<string> { "kind,target,samples,overlapping_samples,faults,elapsed_ms,steady_nonoverlap_samples,steady_nonoverlap_faults,max_faults" };
                lines.Add(string.Format(CultureInfo.InvariantCulture, "process,Total,1,0,{0},{1:F3},0,0,{0}", faults, elapsed));
                foreach (var row in session.Targets.Values.OrderByDescending(x => x.Faults))
                    lines.Add(string.Format(CultureInfo.InvariantCulture, "scope,\"{0}\",{1},{2},{3},{4:F3},{5},{6},{7}",
                        row.Target.Replace("\"", "\"\""), row.Count, row.Overlaps, row.Faults, row.Milliseconds, row.SteadyCount, row.SteadyFaults, row.Maximum));
                File.WriteAllLines(session.Path, lines, Encoding.UTF8);
                return session.Path;
            }
        }

        internal sealed class Session
        {
            internal readonly object Gate = new object();
            internal readonly Dictionary<string, Statistics> Targets = new Dictionary<string, Statistics>(64);
            internal readonly Stopwatch Clock = Stopwatch.StartNew();
            internal readonly TimeSpan Duration;
            internal readonly string Path;
            internal readonly uint FirstFaults;
            internal bool Accepting = true;
            internal int Active;
            internal long Revision;
            internal Session(TimeSpan duration, string path) { Duration = duration; Path = path; ReadFaults(); FirstFaults = ReadFaults(); }
        }
        internal sealed class Statistics
        {
            internal string Target;
            internal long Count, Overlaps, Faults, Maximum, SteadyCount, SteadyFaults;
            internal double Milliseconds;
        }
        internal struct Scope : IDisposable
        {
            Session session;
            readonly Statistics statistics;
            readonly uint first;
            readonly long started, revision;
            readonly bool overlaps;
            internal Scope(Session session, Statistics statistics, uint first, long started, long revision, bool overlaps)
            { this.session = session; this.statistics = statistics; this.first = first; this.started = started; this.revision = revision; this.overlaps = overlaps; }
            public void Dispose()
            {
                var owner = session;
                if (owner == null) return;
                session = null;
                lock (owner.Gate)
                {
                    try
                    {
                        var faults = Difference(first, ReadFaults());
                        var concurrent = overlaps || revision != owner.Revision;
                        statistics.Count++; statistics.Faults += faults;
                        statistics.Maximum = Math.Max(statistics.Maximum, faults);
                        statistics.Milliseconds += (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;
                        if (concurrent) statistics.Overlaps++;
                        // Skip each target's first two calls (JIT and lazy setup).
                        else if (statistics.Count > 2) { statistics.SteadyCount++; statistics.SteadyFaults += faults; }
                    }
                    catch (Exception ex)
                    {
                        Interlocked.CompareExchange(ref current, null, owner);
                        Trace.TraceError("Page fault counter: " + ex);
                    }
                    finally { owner.Revision++; owner.Active--; }
                }
            }
        }
    }

    /// <summary>
    /// 実行間隔分析用
    /// </summary>
    class IntervalDiagnostics
    {
        Func<bool> action;

        /// <summary>
        /// 外れ値として無視する時間(ms)（action中にスリープが入ったなどを検知する目的）
        /// </summary>
        const long ignoreThreashold = 5000;

        /// <summary>
        /// インターバル計測用ストップウォッチ
        /// </summary>
        private Stopwatch intervalCounter;

        private Recorder intervalRecorder = new Recorder();

        private Recorder actionRecorder = new Recorder();

        public IntervalDiagnostics(Func<bool> method)
        {
            action = method;
        }

        public bool Do()
        {
            if (intervalCounter == null)
            {
                intervalCounter = new Stopwatch();
            }
            else
            {
                intervalRecorder.Push(intervalCounter.ElapsedMilliseconds);
            }

            var sw = new Stopwatch();
            sw.Start();
            var ret = action();
            actionRecorder.Push(sw.ElapsedMilliseconds);

            intervalCounter.Restart();
            return ret;
        }

        public void ResetMinMax()
        {
            intervalRecorder.ResetMinMax();
            actionRecorder.ResetMinMax();
        }

        public string Report
        {
            get => $"act: {actionRecorder},int:{intervalRecorder}";
        }

        public string IntervalReport { get => intervalRecorder.ToString(); }

        public string ActionReport { get => actionRecorder.ToString(); }

        class Recorder
        {
            const int windowSize = 10;

            long[] values;
            long[] valuesWithOutlier;

            int valueCount;
            int valueHead;

            int outlierCount;
            int outlierHead;

            public Recorder()
            {
                values = new long[windowSize];
                valuesWithOutlier = new long[windowSize];
                valueCount = valueHead = outlierCount = outlierHead = 0;
            }

            public void Push(long ms)
            {
                if (ms <= 1) return; //下方閾値以下は完全に無視する(何も実施するものがなかったUpdateがここ）

                Latest = ms;

                if ((Minimum == 0) || (Minimum > ms)) Minimum = ms;

                if (OutlierMaximum < ms) OutlierMaximum = ms;

                valuesWithOutlier[outlierHead++] = ms;
                if (outlierHead >= windowSize) outlierHead = 0;
                if (outlierCount < windowSize) outlierCount++;

                if (ms <= ignoreThreashold)
                {
                    if (Maximum < ms) Maximum = ms;
                    values[valueHead++] = ms;
                    if (valueHead >= windowSize) valueHead = 0;
                    if (valueCount < windowSize) valueCount++;
                }
            }

            public void ResetMinMax()
            {
                Minimum = Maximum = 0;
            }

            /// <summary>
            /// 最近
            /// </summary>
            public long Latest { get; private set; }

            /// <summary>
            /// 最小処理時間
            /// </summary>
            public long Minimum { get; private set; }
            /// <summary>
            /// 最大処理時間
            /// </summary>
            public long Maximum { get; private set; }
            /// <summary>
            /// 最大(外れ値込み）
            /// </summary>
            public long OutlierMaximum { get; private set; }

            public override string ToString()
            {
                var avg = valueCount == 0 ? 0 : values.Sum() / valueCount;
                var oAvg = outlierCount == 0 ? 0 : valuesWithOutlier.Sum() / outlierCount;
                return $"{Latest} ms({Minimum}～{Maximum}[{OutlierMaximum}] avg. {avg}[{oAvg}])";
            }
        }

    }
}
