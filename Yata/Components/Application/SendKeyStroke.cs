using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Application
{
    internal class SendKeyStroke : IInternalApplication
    {
        public void Execute(Karasu karasu, string option)
        {
            System.Windows.Forms.SendKeys.Send(option);

        }
    }
}
