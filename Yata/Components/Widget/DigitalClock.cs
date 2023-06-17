using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Widget
{
    class DigitalClock : OwnerDrawWidget
    {
        Font font;

        int min = -1;

        public DigitalClock() : base(WidgetScaleMode.Fixed, 7 * 24, 2 * 24)
        {
            font = new Font("Impact", 24);
        }

        public override bool Update()
        {
            var min2 = DateTime.Now.Minute;
            var ret = min != min2;
            min = min2;
            return ret;
        }

        public override void Draw(Graphics graphics)
        {
            graphics.Clear(Color.Transparent);
            graphics.DrawString(DateTime.Now.ToString("MM/dd HH:mm"), font, Brushes.White, 0, 0);
        }
    }
}
