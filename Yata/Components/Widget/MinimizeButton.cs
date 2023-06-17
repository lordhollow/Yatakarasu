using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Yata.Components.Widget
{
    class MinimizeButton : TextLabel
    {
        Form target;

        public MinimizeButton(Form targetForm) : base("-")
        {
            target = targetForm;
        }

        public override void Click(int x, int y)
        {
            target.WindowState = FormWindowState.Minimized;
        }
    }
}
