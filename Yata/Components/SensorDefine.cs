using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

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
        public PlotDefine(Match m)
        {
            Name = m.Groups["name"].Value;
            Unit = m.Groups["unit"].Value;
            Min = float.Parse(m.Groups["min"].Value);
            Max = float.Parse(m.Groups["max"].Value);
            LowLevelThreshold = float.Parse(m.Groups["ll"].Value);
            HighLevelThreshold = float.Parse(m.Groups["hl"].Value);
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

        public MeterDefine(Match m)
        {
            Name = m.Groups["name"].Value;
            Unit = m.Groups["unit"].Value;
            Min = float.Parse(m.Groups["min"].Value);
            Max = float.Parse(m.Groups["max"].Value);
            Id = m.Groups["id"].Value;
        }
        public string Name;
        public string Unit;
        public float Min;
        public float Max;
        public string Id;
    }

}
