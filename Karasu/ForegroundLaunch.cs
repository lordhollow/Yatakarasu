using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Karasu
{
    internal static class ForegroundLaunch
    {
        static int generation;
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
        [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr window);
        [DllImport("user32.dll")] static extern bool IsIconic(IntPtr window);
        [DllImport("user32.dll")] static extern bool ShowWindowAsync(IntPtr window, int command);

        internal static void Start(ProcessStartInfo info, IntPtr origin)
        {
            var ticket = Interlocked.Increment(ref generation);
            var process = Process.Start(info); // Start failures still reach the launcher ACK.
            if (process == null) return; // Shell URLs may not return a process.
            if (origin == IntPtr.Zero) { process.Dispose(); return; }
            _ = Task.Run(() =>
            {
                using (process)
                {
                    try
                    {
                        WaitForWindow(() => { process.Refresh(); return process.MainWindowHandle; }, () =>
                        {
                            if (ticket != Volatile.Read(ref generation) || process.HasExited) return false;
                            var current = GetForegroundWindow(); uint owner;
                            GetWindowThreadProcessId(current, out owner);
                            return current == origin || owner == process.Id;
                        }, window =>
                        {
                            if (IsIconic(window)) ShowWindowAsync(window, 9); // Restore without moving/resizing.
                            SetForegroundWindow(window);
                        }, () => Thread.Sleep(100));
                    }
                    catch (InvalidOperationException) { } // Exited or non-GUI process.
                    catch (System.ComponentModel.Win32Exception) { } // Activation is best-effort.
                }
            });
        }

        // Bounded, asynchronous from the caller's perspective; no foreground enforcement loop.
        internal static bool WaitForWindow(Func<IntPtr> find, Func<bool> stillWanted, Action<IntPtr> activate, Action wait)
        {
            for (int i = 0; i < 100; i++)
            {
                if (!stillWanted()) return false;
                var window = find();
                if (window != IntPtr.Zero)
                {
                    if (!stillWanted()) return false;
                    activate(window); return true;
                }
                wait();
            }
            return false;
        }
    }
}
