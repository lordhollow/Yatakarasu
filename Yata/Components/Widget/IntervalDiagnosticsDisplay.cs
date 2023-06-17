using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Yata.Components.Widget
{
    class IntervalDiagnosticsDisplay : OwnerDrawWidget
    {
        Font font;
        IntervalDiagnostics diag;
        Karasu karasu;
        string msg;

        public IntervalDiagnosticsDisplay(IntervalDiagnostics diag, Karasu karasu) : base(WidgetScaleMode.Fixed, 552, 48)
        {
            this.diag = diag;
            this.karasu = karasu;
            font = new Font("BIZ UDゴシック", 9.0f);
        }

        public override bool Update()
        {
            msg = $"Action  : {diag.ActionReport}\r\nInterval: {diag.IntervalReport}\r\nKarasRequest T- {(DateTime.Now - karasu.LatestRequestTime).TotalSeconds:f} sec.";
            return true;
        }

        public override void Draw(Graphics graphics)
        {
            graphics.Clear(Color.Transparent);
            graphics.DrawString(msg, font, Brushes.White, 0, 0);
        }
    }
}
