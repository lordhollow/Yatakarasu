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
        const int interval = 100;

        /// <summary>
        /// タイムアウトまでのインターバル回数(100ms*1800=3分)
        /// </summary>
        const int timeout = 1800;

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
                            {
                                var stream = msg.GetStream();
                                stream.WriteTimeout = 3000;
                                var command = ReadCommand(stream);
                                if (command.StartsWith("REQ3\t", StringComparison.Ordinal))
                                {
                                    var response = System.Text.Encoding.UTF8.GetBytes(HandleRequest(command));
                                    stream.Write(response, 0, response.Length);
                                }
                                else DispatchCommand(command);
                            }
                        }
                        catch (Exception ex) { ErrorLog(ex); }
                        cnt = 0;
                    }
                    else
                    {
                        //応答確認を待たせないよう短い間隔で接続を確認する。
                        System.Threading.Thread.Sleep(interval);
                        cnt++;
                    }
                    //3分間何もなければ終了。
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

        static readonly object requestGate = new object();
        static readonly Dictionary<string, Tuple<string, string>> responses = new Dictionary<string, Tuple<string, string>>();
        static readonly Queue<string> responseOrder = new Queue<string>();
        static readonly Dictionary<string, long> recentCommands = new Dictionary<string, long>();
        static readonly Stopwatch requestWatch = Stopwatch.StartNew();
        static Func<long> requestClock = () => requestWatch.ElapsedMilliseconds;
        static IntPtr launchOrigin;
        static Action<ProcessStartInfo> startProcess = info => ForegroundLaunch.Start(info, launchOrigin);
        static readonly int processId = Process.GetCurrentProcess().Id;

        // The response is cached before writing to the socket. Replaying an ID cannot relaunch it.
        private static string HandleRequest(string request)
        {
            var parts = request.Split('\t');
            Guid id;
            long origin = 0;
            if ((parts.Length != 3 && parts.Length != 4) || !Guid.TryParseExact(parts[1], "N", out id) ||
                (parts.Length == 4 && !long.TryParse(parts[3], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out origin)))
                return Reply("-", "error", "要求の形式が不正です。");
            lock (requestGate)
            {
                Tuple<string, string> cached;
                if (responses.TryGetValue(parts[1], out cached))
                    return cached.Item1 == request ? cached.Item2 : Reply(parts[1], "error", "要求IDが重複しています。");
                string response;
                try
                {
                    var command = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parts[2]));
                    bool launch = command.StartsWith("EXEC\t") || command.StartsWith("EXEC2\t") || command.StartsWith("COMMAND\t");
                    var now = requestClock();
                    long previous;
                    if (launch && recentCommands.TryGetValue(command, out previous) && now - previous < 2000)
                    {
                        recentCommands[command] = now;
                        response = Reply(parts[1], "blocked", "連続した実行要求を抑制しました。");
                    }
                    else
                    {
                        launchOrigin = new IntPtr(origin);
                        try { DispatchCommand(command); }
                        finally { launchOrigin = IntPtr.Zero; }
                        if (launch)
                        {
                            if (recentCommands.Count >= 256) recentCommands.Remove(recentCommands.OrderBy(pair => pair.Value).First().Key);
                            recentCommands[command] = requestClock();
                        }
                        response = Reply(parts[1], "ok", launch ? "起動処理が成功しました。" : "処理が完了しました。");
                    }
                }
                catch (Exception ex) { response = Reply(parts[1], "error", ex.Message); }
                responses.Add(parts[1], Tuple.Create(request, response)); responseOrder.Enqueue(parts[1]);
                while (responseOrder.Count > 256) responses.Remove(responseOrder.Dequeue());
                return response;
            }
        }
        static string Reply(string id, string status, string message)
            => "RESULT\t" + id + "\t" + status + "\t" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(message.Length > 1024 ? message.Substring(0, 1024) : message)) + "\t" + processId;

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
            if (cmd == "HEARTBEAT")
            {
#if DEBUG
                Log(cmd);
#endif
                return;
            }
            if (cmd == "EXIT")
            {
                Log(cmd);
                ExitRequest = true;
            }
            else if (cmd.StartsWith("EXEC2\t"))
            {
                var parts = cmd.Split('\t');
                int row, column;
                if (parts.Length != 5 || !int.TryParse(parts[2], out row) || !int.TryParse(parts[3], out column) || row < 0 || column < 0 || column >= 16) throw new InvalidDataException("ランチャー位置が不正です。");
                var definitionPath = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parts[4]));
                ExecutePosition(parts[1], row, column, definitionPath);
            }
            else if (cmd.StartsWith("EXEC\t"))
            {
                Log(cmd);
                //実行コマンド。
                var parts = cmd.Split('\t');
                if (parts.Length < 4) throw new InvalidDataException("実行要求が不正です。");
                Execute(parts[1], parts[2], parts[3]);

            }
            else if (cmd.StartsWith("COMMAND\t"))
            {
                Log(cmd);
                //実行モード
                var parts = cmd.Split('\t');
                if (parts.Length != 2) throw new InvalidDataException("実行モードが不正です。");
                var fn = YataFile("karasu.exec.bat");
                if (parts[1] == "KarasProcessStart")
                {
                    var f = File.ReadAllLines(fn)[1];   //0はchcpなので無視する
                    startProcess(CreateStartInfo(f, ""));
                }
                else if (parts[1] == "KarasuCommandLine" || parts[1] == "KarasuCommandLineKeepAlive" || parts[1] == "KarasuCommandLineRedirect")
                {
                    startProcess(CreateStartInfo("Cmd.exe", "/C \"" + fn + "\""));
                }
                else throw new InvalidDataException("実行モードが不正です。");
            }
            else throw new InvalidDataException("未対応の要求です。YataとKarasuを両方更新してください。");
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
                                return;
                            }
                        }
                    }
                }
                throw new InvalidDataException("認証キーまたは起動項目が一致しません。");
            }
            catch (Exception e)
            {
                ErrorLog(e);
                throw;
            }
        }

        private static void Open(string cmd, string param)
        {
            startProcess(CreateStartInfo(cmd, param));
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
            if (info == null) throw new InvalidDataException("認証キーまたは起動項目が一致しません。");
            startProcess(info);
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
