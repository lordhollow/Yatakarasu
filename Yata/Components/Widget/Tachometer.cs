using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;

namespace Yata.Components.Widget
{
    /// <summary>
    /// タコメーター
    /// </summary>
    class Tachometer : OwnerDrawWidget
    {
        float max;
        float min;
        float prev;
        string disp;
        bool needUpdate;
        Font font;
        ISensor target;

        public Tachometer(int min, int max, ISensor sensor)
            : base(WidgetScaleMode.Fixed, 72, 72)
        {
            this.min = min;
            this.max = max;
            prev = min;
            target = sensor;
            needUpdate = true;
            font = new Font("BIZ UDPゴシック", 7.5f);
        }

        /// <summary>
        /// 表示単位
        /// </summary>
        public string Unit { get; set; }

        public override bool Update()
        {
            //垂直バーと全く同じだから親クラスまとめてもいいかも
            var ret = needUpdate;
            needUpdate = false;
            try
            {
                var v = target.Value.Value;
                if (v < min) v = min;
                if (v > max) v = max;
                if (v != prev) ret = true;
                prev = v;
                if (target.SensorType == SensorType.Fan)
                {
                    disp = $"{prev:0}";
                }
                else
                {
                    disp = $"{prev:0.0}";
                }
            }
            finally { }
            return ret;
        }

        public override void Draw(Graphics graphics)
        {
            //0時～9時のところがメーターで9時から12時のところには表示が出る
            //(左上が表示なのは0,0から書けば良くて楽だから）
            var msg = $"{Caption}\r\n{disp}\r\n{Unit}";
            graphics.Clear(Color.Transparent);

            var barPos = (int)(270 * (prev - min) / (float)(max - min));
            Brush b = Brushes.DarkSlateBlue;
            if (barPos > 180) b = Brushes.DarkOrange;
            if (barPos > 250) b = Brushes.DarkRed;
            if (prev != min)
            {
                graphics.FillPie(b, new Rectangle(2, 2, Width - 4, Height - 4), -90, barPos);
            }

            graphics.DrawPie(Pens.Gray, new Rectangle(2, 2, Width - 4, Height - 4), -90, 270);

            graphics.DrawString(msg, font, Brushes.White, 0, 2);

        }
    }
}
