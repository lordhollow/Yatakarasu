using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;

namespace Yata.Components.Widget
{
    class SensorCurrentList : OwnerDrawWidget
    {
        List<SensorAnnotator> sensors = new List<SensorAnnotator>();
        List<float> values = new List<float>();

        Font font;

        /// <summary>
        /// この値より小さいときグレーアウト
        /// </summary>
        public float LowValueThreashold { get; set; } = float.MinValue;
        public float HighValueThreashold { get; set; } = float.MaxValue;

        public SensorCurrentList()
            : base(WidgetScaleMode.Fixed, 6 * 24, 8 * 24)
        {
            UpdateRate = 1;    //毎秒更新
            font = new Font("BIZ UDゴシック", 9.5f, FontStyle.Bold);
        }

        public void Add(ISensor sensor, string name, Color color)
        {
            sensors.Add(new SensorAnnotator
            {
                Sensor = sensor,
                DisplayName = name,
                Color = color,
            });
            values.Add(0);
        }

        public IEnumerable<SensorAnnotator> Sensors
        {
            get => sensors;
        }

        public override bool Update()
        {
            return true;
        }

        public string Unit { get; set; }

        public override void Draw(Graphics graphics)
        {
            graphics.Clear(Color.Transparent);

            var b = CommonResource.TransparentBackPanelBrush;
            graphics.FillRectangle(b, 0, 16 - 1, Width, sensors.Count * 14 + 2);

            var y = 0;
            graphics.DrawString(Caption, font, Brushes.White, 0, y);
            y += 16;
            for (var i = 0; i < sensors.Count; i++)
            {
                var s = sensors[i];
                var name = string.IsNullOrEmpty(s.DisplayName) ? s.Sensor.Name : s.DisplayName;
                graphics.DrawString(name, font, Brushes.LightGray, 8, y);

                var current = smoothing(i, s.Sensor.Value.Value);
                var numBrush = Brushes.LightGray;
                if (current <= LowValueThreashold) numBrush = Brushes.Gray;
                if (current > HighValueThreashold) numBrush = Brushes.Red;

                graphics.DrawString($"{current,4:0.0} {Unit}", font, numBrush, 82, y);
                DrawIndicatorColor(graphics, y, s.Color);
                y += 14;
            }
        }

        float smoothing(int index, float current)
        {
            float Speed = 9999;

            //1: 一定速度スムージング・・・あまりに遅すぎる。
            //Speed = 0.1f;

            //2: 比例スムージング
            Speed = Math.Max(0.1f, Math.Abs(current - values[index]) * 0.1f);

            //速度分増減
            if (current > values[index])
            {
                values[index] = Math.Min(current, values[index] + Speed);
            }
            else if (current < values[index])
            {
                values[index] = Math.Max(current, values[index] - Speed);
            }
            return values[index];
        }

        void DrawIndicatorColor(Graphics g, int y, Color c)
        {
            //外側の白丸
            using (var b = new SolidBrush(c))
            {
                g.FillRectangle(b, new RectangleF(2, y + 3, 4, 4));
            }
        }

    }
}
