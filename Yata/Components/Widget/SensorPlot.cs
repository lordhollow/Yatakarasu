using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LibreHardwareMonitor.Hardware;

namespace Yata.Components.Widget
{
    class SensorPlot : OwnerDrawWidget
    {
        List<SensorAnnotator> sensors = new List<SensorAnnotator>();

        ConstructionParameter parameter;

        Font font;

        public class ConstructionParameter
        {
            public int Width { get; private set; } = 552;

            public int PlotWidth { get; private set; } = 520;   //Width-32

            public int Height { get; private set; } = 192;

            public int PlotHeight { get; private set; } = 168;  //Height-24

            /// <summary>
            /// プロットエリアの上パディング(＝Y座標描画の上飛び出し分）
            /// </summary>
            public int PaddingTop { get; set; } = 8;
            /// <summary>
            /// プロットエリアの左パディング（＝Y座標描画用幅）
            /// </summary>
            public int PaddingLeft { get; set; } = 32;

            /// <summary>
            /// X軸幾つに割るか
            /// </summary>
            public int XBorders { get; private set; } = 10;

            public TimeSpan Length { get; set; } = TimeSpan.FromMinutes(12);

            /// <summary>
            /// フォントサイズ
            /// </summary>
            public float FontSize { get; private set; } = 8.0f;

            /// <summary>
            /// フォント名
            /// </summary>
            public string FontName { get; private set; } = "BIZ UDゴシック";

            /// <summary>
            /// フォントスタイル
            /// </summary>
            public FontStyle FontStyle { get; private set; } = FontStyle.Regular;

            public static ConstructionParameter WSVGA { get => new ConstructionParameter(); }

            public static ConstructionParameter WXGA
            {
                get
                {
                    return new ConstructionParameter()
                    {
                        Width = 820,   //WSVGAとWXGAの幅の差=1280-1024=256を標準幅(552)に加算
                        PlotWidth = 780,
                        PaddingLeft = 24,
                        Height = 192 + 84, //84は高さの差からランチャの増分(32)を引いた分の半分
                        PlotHeight = 168 + 84,
                        XBorders = 12,
                    };
                }
            }
        }

        public SensorPlot(ConstructionParameter p) : base(WidgetScaleMode.Fixed, p.Width, p.Height)
        {
            UpdateRate = 50;    //ValueHistorySensorがAverageSetterを最小しているとき、その窓のサイズ(5)*更新時間(10)にしておく。(更新間隔が伸びるため)
            parameter = p;
            font = new Font(parameter.FontName, parameter.FontSize, parameter.FontStyle);
        }

        public void AddRange(IEnumerable<SensorAnnotator> sensor)
        {
            sensors.AddRange(sensor);
        }

        public float Minimum { get; set; } = 0;
        public float Maximum { get; set; } = 100;
        public float LowLevelThreshold { get; set; } = float.MinValue;
        public float HighLevelThreshold { get; set; } = float.MaxValue;

        public SuspendedChecker SuspendChecker { get; set; }

        public override bool Update()
        {
            return true;
        }


        public override void Draw(Graphics g)
        {
            var BaseTime = DateTime.Now;    //描画途中の時間ズレ防止用に基準時刻

            g.Clear(Color.Transparent);

            var plotWidth = parameter.PlotWidth;
            var plotHeight = parameter.PlotHeight;
            var paddingLeft = parameter.PaddingLeft;
            var paddingTop = parameter.PaddingTop;

            var plotArea = new Rectangle(paddingLeft, paddingTop, plotWidth, plotHeight);
            g.FillRectangle(CommonResource.TransparentBackPanelBrush, plotArea);

            FillSuspendedArea(g, BaseTime);

            var vGap = plotHeight / 4;
            var far = new StringFormat { Alignment = StringAlignment.Far, };
            for (var i = 0; i < 5; i++)
            {
                var y = i * vGap + paddingTop;
                g.DrawLine(Pens.Gray, paddingLeft, y, plotWidth + paddingLeft, y);

                var v = Minimum + ((Maximum - Minimum) / 4) * (4 - i);
                g.DrawString($"{v:0}", font, Brushes.Gray, new Rectangle(0, y - 2, paddingLeft, 16), far);
            }
            if ((LowLevelThreshold > Minimum) && (LowLevelThreshold < Maximum))
            {
                var y = ValueToY(LowLevelThreshold);
                using (var borderPen = new Pen(Color.Gray, 1))
                {
                    borderPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawLine(borderPen, paddingLeft, y, plotWidth + paddingLeft, y);
                }
            }
            if ((HighLevelThreshold > Minimum) && (HighLevelThreshold < Maximum))
            {
                var y = ValueToY(HighLevelThreshold);
                using (var borderPen = new Pen(Color.Red, 1))
                {
                    borderPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    g.DrawLine(borderPen, paddingLeft, y, plotWidth + paddingLeft, y);
                }
            }
            var hGap = plotWidth / parameter.XBorders;
            var center = new StringFormat { Alignment = StringAlignment.Center, };
            for (var i = 0; i <= parameter.XBorders; i++)
            {
                var x = i * hGap + paddingLeft;
                g.DrawLine(Pens.Gray, x, paddingTop, x, paddingTop + plotHeight);
                if ((i != 0) && (i != parameter.XBorders))
                {
                    g.DrawString($"-{(parameter.XBorders - i)} min", font, Brushes.Gray, new Rectangle(x - 30, paddingTop + plotHeight + 2, 60, 20), center);
                }
            }
            var orgClip = g.ClipBounds;
            g.SetClip(plotArea);

            foreach (var sensor in sensors)
            {

                using (var p = new Pen(sensor.Color))
                {
                    bool first = true;
                    SensorValue prevSv = new SensorValue();

                    foreach (var v in sensor.Sensor.Values)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            DrawSection(g, p, prevSv, v, BaseTime);
                        }
                        prevSv = v;
                    }
                }
            }

            g.SetClip(orgClip);
        }

        /// <summary>
        /// サスペンド中の領域を塗る
        /// </summary>
        /// <param name="g"></param>
        /// <param name="BaseTime"></param>
        private void FillSuspendedArea(Graphics g, DateTime BaseTime)
        {
            if (SuspendChecker != null)
            {
                //サスペンドチェッカーがあるときはサスペンド部分をちょい黒く塗る
                SuspendChecker.Visit((b, e) =>
                {
                    var bx = TimeToX(BaseTime - b);
                    var ex = TimeToX(BaseTime - e);
                    if (bx < parameter.PaddingLeft) bx = parameter.PaddingLeft;
                    if (ex > parameter.PaddingLeft + parameter.PlotWidth) ex = parameter.PaddingLeft + parameter.PlotWidth;
                    if (ex > parameter.PaddingLeft)
                    {
                        var suspendArea = new Rectangle(bx, parameter.PaddingTop, ex - bx, parameter.PlotHeight);
                        using (var brush = new SolidBrush(Color.FromArgb(64, 255, 0, 0)))
                        {
                            g.FillRectangle(brush, suspendArea);
                        }
                    }
                });
            }
        }

        private void DrawSection(Graphics g, Pen p, SensorValue start, SensorValue end, DateTime now)
        {
            var time = now - end.Time;
            if (time > parameter.Length) return;  //終端が十分以上前のデータは描画しない

            var x1 = TimeToX(now - start.Time);
            var y1 = ValueToY(start.Value);

            var x2 = TimeToX(time);
            var y2 = ValueToY(end.Value);

            g.DrawLine(p, x1, y1, x2, y2);
        }

        int TimeToX(TimeSpan t)
        {
            var tl = parameter.Length.TotalSeconds;
            var tp = t.TotalSeconds;
            var p = (1 - tp / tl) * parameter.PlotWidth + parameter.PaddingLeft;
            return (int)p;
        }

        int ValueToY(float v)
        {
            return (int)((1 - ((v - Minimum) / (Maximum - Minimum))) * parameter.PlotHeight + parameter.PaddingTop);
        }
    }
}
