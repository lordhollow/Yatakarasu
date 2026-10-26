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
                var listener = new TcpListener(System.Net.IPAddress.Loopback, 54892);
                listener.Start();

                bool living = true;
                var cnt = 0;
                while (living)
                {
                    if (listener.Pending())
                    {
                        //一回処理する。
                        try
                        {
                            using (var msg = listener.AcceptTcpClient())
                                DispatchCommand(ReadCommand(msg.GetStream()));
                        }
                        catch (Exception ex) { ErrorLog(ex); }
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
                listener.Stop();

            }
            catch (Exception e)
            {
                ErrorLog(e);
            }
        }

        private static bool ExitRequest = false;

        private static string ReadCommand(System.IO.Stream stream)
        {
            if (stream.CanTimeout) stream.ReadTimeout = 3000;
            using (var buffer = new MemoryStream())
            {
                var block = new byte[4096];
                int read;
                while ((read = stream.Read(block, 0, block.Length)) != 0)
                {
                    if (buffer.Length + read > 65536) throw new InvalidDataException("Command is too long.");
                    buffer.Write(block, 0, read);
                }
                return System.Text.Encoding.UTF8.GetString(buffer.ToArray());
            }
        }

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
            else if (cmd.StartsWith("EXEC2\t"))
            {
                var parts = cmd.Split('\t');
                int row, column;
                if (parts.Length != 5 || !int.TryParse(parts[2], out row) || !int.TryParse(parts[3], out column)) return;
                var definitionPath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parts[4]));
                ExecutePosition(parts[1], row, column, definitionPath);
            }
            else if (cmd.StartsWith("EXEC\t"))
            {
                Log(cmd);
                //実行コマンド。
                var parts = cmd.Split('\t');
                if (parts.Length < 4) return;
                Execute(parts[1], parts[2], parts[3]);

            }
            else if (cmd.StartsWith("COMMAND\t"))
            {
                Log(cmd);
                //実行モード
                var parts = cmd.Split('\t');
                if (parts.Length < 2) return;
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
            Process.Start(CreateStartInfo(cmd, param));
        }

        private static ProcessStartInfo CreateStartInfo(string cmd, string param)
        {
            var info = new ProcessStartInfo(cmd, param);
            if (File.Exists(cmd))
            {
                //コマンドがファイルの時はワーキングディレクトリをファイルの場所に指定
                //(ファイルじゃないのはsteam://とかそういうのを想定）
                info.WorkingDirectory = Path.GetDirectoryName(cmd);
            }
            return info;
        }

        private static void ExecutePosition(string key, int row, int column, string definitionPath)
        {
            var info = ResolvePosition(key, row, column, definitionPath);
            if (info != null) Process.Start(info);
        }

        private static ProcessStartInfo ResolvePosition(string key, int row, int column, string definitionPath)
        {
            var lines = File.ReadAllLines(definitionPath);
            if (lines.Length == 0 || lines[0] != key) return null;
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split('\t');
                int candidateRow, candidateColumn;
                if (parts.Length < 4 || !int.TryParse(parts[0], out candidateRow) || !int.TryParse(parts[1], out candidateColumn)) continue;
                if (candidateRow != row || candidateColumn != column || parts[3].StartsWith("internal::")) continue;
                return CreateStartInfo(parts[3], parts.Length > 5 ? parts[5] : "");
            }
            return null;
        }

        private static string YataFile(string name)
        {
            return Path.Combine(Application.StartupPath, "files", name);
        }
    }

}
