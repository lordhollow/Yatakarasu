using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using Yata.Components.Configuration;

namespace Yata.Components
{
    /// <summary>
    /// センサー定義ファイル(sensor.txt)解釈クラス
    /// </summary>
    class SensorDefine
    {
        public SensorDefine()
        {

        }

        public void ReadFromFile(string filename)
        {
            PlotDefines.Clear();
            MeterDefines.Clear();
            if (!File.Exists(filename)) return;
            var regPlot = new Regex(@"^Plot\t(?<name>.+?)\t(?<unit>.+?)\t(?<min>.+?)\t(?<max>.+?)\t(?<ll>.+?)\t(?<hl>.+)");
            var regPlotItem = new Regex(@"^\t(?<name>.+?)\t(?<id>[^\t]+)\t(?<r>\d+),(?<g>\d+),(?<b>\d+)(,(?<a>\d+))?");
            var regMeter = new Regex(@"^Meter\t(?<name>.+?)\t(?<unit>.+?)\t(?<min>.+?)\t(?<max>.+?)\t(?<id>[^\t]+)");

            foreach (var line in File.ReadAllLines(filename))
            {
                if (line.StartsWith("#")) continue;
                try
                {
                    var m = regPlot.Match(line);
                    if (m.Success)
                    {
                        PlotDefines.Add(new PlotDefine(m));
                        continue;
                    }
                    m = regPlotItem.Match(line);
                    if (m.Success)
                    {
                        PlotDefines[PlotDefines.Count - 1].Items.Add(new PlotItemDefine(m));
                        continue;
                    }
                    m = regMeter.Match(line);
                    if (m.Success)
                    {
                        MeterDefines.Add(new MeterDefine(m));
                        continue;
                    }
                }
                catch
                {
                    //Skip Line
                }
            }
        }

        public List<PlotDefine> PlotDefines = new List<PlotDefine>();
        public List<MeterDefine> MeterDefines = new List<MeterDefine>();
        public void EnsureFixedLayout()
        {
            while (PlotDefines.Count < 2) PlotDefines.Add(new PlotDefine { Name = "グラフ " + (PlotDefines.Count + 1) });
            while (MeterDefines.Count < 6) MeterDefines.Add(new MeterDefine { Name = "未設定" });
            PlotDefines = PlotDefines.Take(2).ToList();
            MeterDefines = MeterDefines.Take(6).ToList();
        }

        public void Save(string path)
        {
            if (PlotDefines.Count != 2 || MeterDefines.Count != 6) throw new FormatException("グラフは2個、タコメーターは6個で設定してください。");
            var text = new StringBuilder();
            foreach (var plot in PlotDefines)
            {
                ValidateRange(plot.Min, plot.Max);
                if (float.IsNaN(plot.LowLevelThreshold) || float.IsInfinity(plot.LowLevelThreshold) || float.IsNaN(plot.HighLevelThreshold) || float.IsInfinity(plot.HighLevelThreshold)) throw new FormatException("境界値には有限の数値を指定してください。");
                text.AppendLine(string.Join("\t", "Plot", Field(plot.Name), Field(plot.Unit), Number(plot.Min), Number(plot.Max), Number(plot.LowLevelThreshold), Number(plot.HighLevelThreshold)));
                foreach (var item in plot.Items)
                    text.AppendLine(string.Join("\t", "", Field(item.Name), Field(item.Id), $"{item.Color.R},{item.Color.G},{item.Color.B},{item.Color.A}"));
            }
            foreach (var meter in MeterDefines)
            {
                ValidateRange(meter.Min, meter.Max);
                text.AppendLine(string.Join("\t", "Meter", Field(string.IsNullOrWhiteSpace(meter.Name) ? "未設定" : meter.Name), Field(string.IsNullOrEmpty(meter.Unit) ? " " : meter.Unit), Number(meter.Min), Number(meter.Max), Field(string.IsNullOrWhiteSpace(meter.Id) ? "-" : meter.Id)));
            }
            ConfigurationFile.Save(path, text.ToString());
        }
        static string Field(string value) => ConfigurationFile.Field(value, "センサー設定");
        static string Number(float value) => value.ToString("R", CultureInfo.InvariantCulture);
        static void ValidateRange(float min, float max)
        {
            if (float.IsNaN(min) || float.IsNaN(max) || float.IsInfinity(min) || float.IsInfinity(max) || min >= max) throw new FormatException("最小値より大きい最大値を指定してください。");
        }
        public IEnumerable<string> SensorUsage()
        {
            foreach(var plot in PlotDefines)
            {
                foreach(var item in plot.Items)
                {
                    yield return item.Id;
                }
            }
            foreach(var meter in MeterDefines)
            {
                yield return meter.Id;
            }
        }
    }

    class PlotDefine
    {
        public PlotDefine() { Min = 0; Max = 100; Unit = "%"; }
        public PlotDefine(Match m)
        {
            Name = m.Groups["name"].Value;
            Unit = m.Groups["unit"].Value;
            Min = float.Parse(m.Groups["min"].Value, CultureInfo.InvariantCulture);
            Max = float.Parse(m.Groups["max"].Value, CultureInfo.InvariantCulture);
            LowLevelThreshold = float.Parse(m.Groups["ll"].Value, CultureInfo.InvariantCulture);
            HighLevelThreshold = float.Parse(m.Groups["hl"].Value, CultureInfo.InvariantCulture);
        }

        public List<PlotItemDefine> Items { get; private set; } = new List<PlotItemDefine>();

        public string Name;
        public string Unit;
        public float Min;
        public float Max;
        public float LowLevelThreshold = float.MinValue;
        public float HighLevelThreshold = float.MaxValue;
    }

    class PlotItemDefine
    {
        public PlotItemDefine() { }
        public PlotItemDefine(Match m)
        {
            Name = m.Groups["name"].Value;
            Id = m.Groups["id"].Value;
            var r = int.Parse(m.Groups["r"].Value);
            var g = int.Parse(m.Groups["g"].Value);
            var b = int.Parse(m.Groups["b"].Value);
            var a = string.IsNullOrEmpty(m.Groups["a"].Value) ? 255 : int.Parse(m.Groups["a"].Value);
            Color = System.Drawing.Color.FromArgb(a, r, g, b);
        }
        public string Name;
        public string Id;
        public System.Drawing.Color Color;
    }

    class MeterDefine
    {
        public MeterDefine() { Min = 0; Max = 100; Unit = " "; Id = ""; }
        public MeterDefine(Match m)
        {
            Name = m.Groups["name"].Value;
            Unit = m.Groups["unit"].Value;
            Min = float.Parse(m.Groups["min"].Value, CultureInfo.InvariantCulture);
            Max = float.Parse(m.Groups["max"].Value, CultureInfo.InvariantCulture);
            Id = m.Groups["id"].Value;
        }
        public string Name;
        public string Unit;
        public float Min;
        public float Max;
        public string Id;
    }

}
