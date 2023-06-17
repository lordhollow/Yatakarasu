using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Yata.Components.Widget
{
    class CloseButton : TextLabel
    {
        Form target;

        public CloseButton(Form targetForm) :base("×")
        {
            target = targetForm;
        }

        public override void Click(int x, int y)
        {
            target.Close();
        }
    }
}
