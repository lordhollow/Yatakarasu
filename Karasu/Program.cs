using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Karasu
{
    static class Program
    {
        /// <summary>
        /// コマンド処理の間隔(ms)
        /// </summary>
        const int interval = 3000;

        /// <summary>
        /// タイムアウトまでのインターバル回数(3秒*60=3分)
        /// </summary>
        const int timeout = 60;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //カラスは起動するとYataをRunasした後TCP待機モードに入る。
            //一定時間接続がないとそのまま終了する（Yataの終了検知の代わり）

            try
            {
                var args = Environment.GetCommandLineArgs();
                if (args.Contains("+yata"))
                {
                    if (MutexCheck())
                    {
                        var pInfo = new ProcessStartInfo(Path.Combine(Application.StartupPath, "Yata.exe"));
                        pInfo.Verb = "RunAs";
                        Process.Start(pInfo);
                    }
                }

                //接続されるまで待つ。これはYataのReady待機の意味。一回データを受け取ったらもう切っていい。
                Log("WAIT");
                File.AppendAllText("log.txt", $"{DateTime.Now}\tWAIT YATA\r\n");
                var buffer = new byte[4096];
                var listener = new TcpListener(System.Net.IPAddress.Loopback, 54892);
                listener.Start();
                var client = listener.AcceptTcpClient();
                var sz = client.GetStream().Read(buffer, 0, 4096);
                client.Close();
                Log("CHECKED");
                listener.Start();

                bool living = true;
                var cnt = 0;
                while (living)
                {
                    if (listener.Pending())
                    {
                        //一回処理する。
                        var msg = listener.AcceptTcpClient();
                        Console.WriteLine(msg.Client.RemoteEndPoint.ToString());
                        var s = msg.GetStream().Read(buffer, 0, 4096);
                        var cmd = System.Text.Encoding.UTF8.GetString(buffer, 0, s);
                        client.Close();
                        if (msg.Client.RemoteEndPoint.ToString().StartsWith("127.0.0.1:"))
                        {   //リモートホストからのみ受け付ける
                            DispatchCommand(cmd);
                        }
                        //File.AppendAllText("log.txt", $"{DateTime.Now}\t{cmd}\r\n");
                        listener.Start();
                        cnt = 0;
                    }
                    else
                    {
                        //３秒寝ておく。
                        System.Threading.Thread.Sleep(interval);
                        cnt++;
                    }
                    //30秒何もなければ終了。
                    //DateTimeで30秒数えるとすると、途中にスリープ/復帰が入ると死ぬ気がするので
                    //スリープ中はカウントが止まるように自力でカウントしている。
                    if (cnt >= timeout)
                    {
                        living = false;
                    }

                    if (ExitRequest)
                    {
                        living = false;
                    }
                }
                Log("EXIT KARAS");

            }
            catch (Exception e)
            {
                ErrorLog(e);
            }
        }

        private static bool ExitRequest = false;

        private static void ErrorLog(Exception e)
        {
            File.WriteAllText("karasu.log.txt", e.Message + "\r\n" + e.StackTrace);

        }

        private static void Log(string msg)
        {
            Console.WriteLine($"{DateTime.Now}\t{msg}");
            File.AppendAllText("log.txt", $"{DateTime.Now}\t{msg}\r\n");
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
            var mutex = new Mutex(true, @"Global\YATAKARASU_198682546958649", out createNew, mutexSecurity);
            mutex.Dispose();
            return createNew;
        }

        /// <summary>
        /// コマンドを解釈する。
        /// </summary>
        /// <param name="cmd"></param>
        /// <remarks>
        /// カラスくんは言われたことを真面目に実行してしまうため、セキュリティホールになりえる。
        /// とりあえず、最低限のセキュリティとしてパスワード認証を備えておき、
        /// 第三者がカラスの存在を知っているだけでは任意のアプリの起動などはできないようにガードしておく。
        /// </remarks>
        private static void DispatchCommand(string cmd)
        {
            if (cmd.StartsWith("HEARTBEAT"))
            {
#if DEBUG
                Log(cmd);
#endif
                //なにもしない
            }
            if (cmd.StartsWith("EXIT"))
            {
                Log(cmd);
                ExitRequest = true;
            }
            else if (cmd.StartsWith("EXEC\t"))
            {
                Log(cmd);
                //実行コマンド。
                var parts = cmd.Split('\t');
                Execute(parts[1], parts[2], parts[3]);

            }
            else if (cmd.StartsWith("COMMAND\t"))
            {
                Log(cmd);
                //実行モード
                var parts = cmd.Split('\t');
                var fn = YataFile("karasu.exec.bat");
                if (parts[1] == "KarasProcessStart")
                {
                    try
                    {
                        var f = File.ReadAllLines(fn)[1];   //0はchcpなので無視する
                        Process.Start(f);
                    }
                    catch { }
                }
                else
                {
                    Process.Start("Cmd.exe", "/C " + fn);
                }
            }
        }

        /// <summary>
        /// 実行
        /// </summary>
        /// <param name="passCode"></param>
        /// <param name="cmdCode"></param>
        /// <param name="param"></param>
        /// <remarks>
        /// セキュリティ―の為、PASSCODEの一致をみるほか、
        /// CMDはExeの直接のパスなどではなくカラスに指示するためのコード。
        /// PARAMはそのまま起動引数。
        /// パスコード、および指示コードに対応する実際のコマンドラインはkarasu.exec.txtに記載する。
        /// </remarks>
        private static void Execute(string passCode, string cmdCode, string param)
        {
            var defFile = YataFile("karasu.exec.txt");
            try
            {
                var lines = File.ReadAllLines(defFile);
                if (lines[0] == passCode)
                {
                    for (var i = 1; i < lines.Length; i++)
                    {
                        var dt = lines[i].Split('\t');
                        if (dt.Length >= 4)
                        {
                            if (dt[2] == cmdCode)
                            {
                                Open(dt[3], param);
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                ErrorLog(e);
            }
        }

        private static void Open(string cmd, string param)
        {
            var info = new ProcessStartInfo(cmd, param);
            if (File.Exists(cmd))
            {
                //コマンドがファイルの時はワーキングディレクトリをファイルの場所に指定
                //(ファイルじゃないのはsteam://とかそういうのを想定）
                info.WorkingDirectory = Path.GetDirectoryName(cmd);
            }
            Process.Start(info);
        }

        private static string YataFile(string name)
        {
            return Path.Combine(Application.StartupPath, "files", name);
        }
    }

}
