using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Widget
{
    class VerticalBarMeter : OwnerDrawWidget
    {
        int max;
        int min;
        int prev;
        bool needUpdate;
        Func<int> ValueGetter = () => 0;

        public VerticalBarMeter(int min, int max, Func<int> getter)
            : base(WidgetScaleMode.Fixed, 2 * 24, 6 * 24)
        {
            this.min = min;
            this.max = max;
            prev = min;
            ValueGetter = getter;
            needUpdate = true;
        }

        public override bool Update()
        {
            var ret = needUpdate;
            needUpdate = false;
            var v = ValueGetter();
            if (v < min) v = min;
            if (v > max) v = max;
            if (v != prev) ret = true;
            prev = v;
            return ret;
        }

        public override void Draw(Graphics graphics)
        {
            graphics.Clear(Color.Transparent);
            var barHight = (int)(Height * (prev - min) / (float)(max - min));
            graphics.FillRectangle(CommonResource.TransparentBackPanelBrush, new Rectangle(0, 0, Width, Height - barHight));

            var bColor = Color.FromArgb(255, 240, 240, 255);
            using (var b = new SolidBrush(bColor))
            {
                graphics.FillRectangle(b, new Rectangle(0, Height - barHight, Width, barHight));
            }
        }
    }
}
