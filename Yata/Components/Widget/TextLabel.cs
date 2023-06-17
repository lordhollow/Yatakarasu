using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Widget
{
    class TextLabel : OwnerDrawWidget
    {
        Font font;
        string label;
        StringFormat fmt;
        bool needDraw = true;

        public TextLabel(string label) : base(WidgetScaleMode.Fixed, 48, 48)
        {
            font = new Font("Impact", 24);
            this.label = label;
            fmt = new StringFormat();
            fmt.Alignment = StringAlignment.Center;
            fmt.LineAlignment = StringAlignment.Center;
        }


        public override bool Update()
        {
            var ret = needDraw;
            needDraw = false;
            return ret;
        }


        public override void Draw(Graphics graphics)
        {
            graphics.DrawString(label, font, Brushes.White, new Rectangle(0, 0, Width, Height), fmt);
        }

    }
}
