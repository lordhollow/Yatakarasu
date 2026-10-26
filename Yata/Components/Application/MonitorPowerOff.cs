using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Yata.Components.Application
{
    internal sealed class MonitorPowerOff : IInternalApplication
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessageTimeout(IntPtr window, uint message, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);

        public void Execute(Karasu karasu, string option)
        {
            //クリックを離してから消灯し、操作直後の入力による復帰を避ける。
            var timer = new Timer { Interval = 700 };
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                timer.Dispose();
                IntPtr ignored;
                SendMessageTimeout(new IntPtr(0xffff), 0x0112, new IntPtr(0xf170), new IntPtr(2), 2, 1000, out ignored);
            };
            timer.Start();
        }
    }
}
