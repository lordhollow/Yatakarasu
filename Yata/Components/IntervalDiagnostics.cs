using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace Yata.Components
{
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
