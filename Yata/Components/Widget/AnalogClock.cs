using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Widget
{
    class AnalogClock : OwnerDrawWidget
    {
        Image bg;
        int min = -1;

        public AnalogClock() : base(WidgetScaleMode.Fixed, 168, 168)
        {
            bg = CommonResource.LoadYataImage("ClockBack.png");
            UpdateRate = 1;
        }

        public bool DrawSeconds { get; set; } = true;

        public bool SmoothSeconds { get; set; } = true;

        public override bool Update()
        {
            var min2 = DateTime.Now.Minute;
            var ret = min != min2;
            min = min2;
            return SmoothSeconds | ret;
        }

        public override void Draw(Graphics graphics)
        {
            var t = DateTime.Now;
            var h = t.Hour * 60 + t.Minute;
            if (h > 60 * 12)
            {
                h -= 60 * 12;
            }

            var lPos = t.Minute * 360 / 60;
            var sPos = (h * 360) / (60 * 12);
            var tPos = (t.Second + t.Millisecond / 1000.0) * 6;

            graphics.Clear(Color.Transparent);
            var rect = new Rectangle(0, 0, 168, 168);
            graphics.DrawImage(bg, rect, rect, GraphicsUnit.Pixel);

            drawClockHand(graphics, 35, 3, sPos, Color.LightGray);
            drawClockHand(graphics, 60, 2, lPos, Color.Yellow);
            if (DrawSeconds)
            {
                drawClockHand(graphics, 60, 1, (int)tPos, Color.Red);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="angle">0=360=上。時計回り。</param>
        /// <param name="color"></param>
        private void drawClockHand(Graphics g, float size, float w, float angle, Color color)
        {
            ////As Arc
            //using (var b = new SolidBrush(color))
            //{
            //    using (var p = new Pen(b) { Width = w, Alignment = System.Drawing.Drawing2D.PenAlignment.Center })
            //    {
            //        var bs = Width / 2 - size;
            //        g.FillPie(b, bs, bs, size * 2, size * 2, angle - w, w * 2);
            //        g.DrawPie(p, bs, bs, size * 2, size * 2, angle - 0.5f, 1f);
            //    }
            //}

            //As Line
            using (var b = new SolidBrush(color))
            {
                angle -= 90;
                var x = (float)Math.Cos(angle * Math.PI / 180);
                var y = (float)Math.Sin(angle * Math.PI / 180);

                using (var p = new Pen(b) { Width = w, Alignment = System.Drawing.Drawing2D.PenAlignment.Center })
                {
                    var bs = Width / 2;
                    g.DrawLine(p, bs - x * 5, bs - y * 5, bs + x * size, bs + y * size);
                }
            }

        }
    }

}
