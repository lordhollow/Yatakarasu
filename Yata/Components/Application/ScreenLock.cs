using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Yata.Components.Application
{
    internal class ScreenLock : IInternalApplication
    {
        [DllImport("user32.dll")]
        private static extern bool LockWorkStation();

        public ScreenLock()
        {
        }

        public void Execute(Karasu karasu, string option)
        {
            LockWorkStation();
        }

    }
}
