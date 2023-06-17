using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using Yata.Components;

namespace Yata
{
    static class Program
    {
        static Mutex mutex;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (MutexCheck())
            {
                Thread.GetDomain().UnhandledException += Program_UnhandledException;

                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new YataMain());
                }
                finally
                {
                    mutex.Close();
                }
            }
        }

        /// <summary>
        /// 未処理例外ハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            mutex.Close();
            var ex = e.ExceptionObject as Exception;
            var msg = (ex == null) ? "Unknown Exception" : $"{ex.Message}\r\n{ex.StackTrace}";
            System.IO.File.WriteAllText("unhandled.exception.txt", msg);

            Application.Exit();
        }

        /// <summary>
        /// Mutexによる多重起動阻止
        /// </summary>
        static bool MutexCheck()
        {
            var mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(
              new MutexAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                MutexRights.Synchronize | MutexRights.Modify,
                AccessControlType.Allow
              )
            );
            //Karasuからのチェックも行うため、Globalに作る。そのためにセキュリティが必要。
            bool createNew;
            mutex = new Mutex(true, @"Global\YATAKARASU_198682546958649", out createNew, mutexSecurity);
            return createNew;
        }
    }
}
