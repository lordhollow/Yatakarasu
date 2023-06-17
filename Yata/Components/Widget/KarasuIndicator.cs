using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Yata.Components.Widget
{
    class KarasuIndicator : OwnerDrawWidget
    {
        bool enable;
        bool needUpdate = true;
        Image img;
        Karasu karasu;

        public KarasuIndicator(Karasu k) : base(WidgetScaleMode.Fixed,48,48)
        {
            karasu = k;
            img = CommonResource.LoadYataImage("karasu.png");
        }

        public override bool Update()
        {
            var ret = needUpdate;
            needUpdate = false;

            if(enable != karasu.Active)
            {
                enable = karasu.Active;
                ret = true;
            }
            return ret;
        }

        public override void Draw(Graphics graphics)
        {
            graphics.Clear(Color.Transparent);
            if(enable)
            {
                var rect = new Rectangle(0, 0, 48, 48);
                graphics.DrawImage(img, rect, rect, GraphicsUnit.Pixel);
            }
            else
            {
                graphics.DrawString("KARAS\r\n    IS\r\n DEAD", SystemFonts.DialogFont, Brushes.Red, 0, 0);
            }
        }
    }
}
