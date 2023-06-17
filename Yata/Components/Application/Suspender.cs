using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Application
{
    /// <summary>
    /// サスペンダー
    /// </summary>
    class Suspender : IInternalApplication
    {

        public void Execute(Karasu karasu, string option)
        {
            System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Suspend, false, false);
        }
    }
}
