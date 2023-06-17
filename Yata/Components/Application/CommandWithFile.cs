using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Yata.Components.Application
{
    /// <summary>
    /// ファイル名を指定してコマンドを実行するアプリ。
    /// </summary>
    /// <remarks>
    /// いくつかの実行モードがある。デフォルトはKarasuCommandLineKeepAlive(karasu経由(ユーザー権限)でcmd.exeを終了しないように起動))。
    /// Optionには実行するプログラムを記述し、"${infile}"箇所が選択したファイル(単一）のフルパスに置き換わる。
    /// ファイルの指定方法には以下の3つがある。
    /// ・エクスプローラーからファイルを「コピー」した状態で起動
    /// ・クリップボードにダブルクオートで囲まれたファイルパスを設定した状態(エクスプローラーから「パスのコピー」）で起動
    /// ・上記いずれでもない場合、ファイルダイアログを開いて選択
    /// </remarks>
    class CommandWithFile : IInternalApplication
    {
        string Option;
        Karasu Karasu;

        CommandExecutionType mode;

        const string redirectFile = "karasu.console.txt";
        const string batchFile = "karasu.exec.bat";

        public CommandWithFile() : this(CommandExecutionType.KarasuCommandLineKeepAlive)
        {

        }

        public CommandWithFile(CommandExecutionType mode)
        {
            this.mode = mode;
        }

        public void Execute(Karasu karas, string option)
        {
            Option = option;
            Karasu = karas;
            //↓をAsyncにすると複数のダイアログを開けるようになるが操作が面倒なだけなのでやめておく。
            //画面の更新自体も停止しない(なぜ?)
            exec();
        }

        private async void execAsync()
        {
            var th = new System.Threading.Thread(exec);
            th.SetApartmentState(System.Threading.ApartmentState.STA);
            th.Start();
            await Task.Run(() => th.Join());
            Console.WriteLine("AfterAwait");

        }

        private void exec()
        {
            //クリップボードに"～"の形式で単一のファイル名が入っていたら
            //(=エクスプローラーシフト右クリックから「パスのコピーしたら」）
            //そのファイルを使う。なければダイアログで選択する。
            var file = GetFile();
            if (!string.IsNullOrEmpty(file))
            {
                var msg = Option.Replace("${infile}", file);
                Console.WriteLine($"CommandWithFile : {msg}");

                switch (mode)
                {
                    case CommandExecutionType.CommandLineKeepAlive:
                        //Pause/exitつきでバッチファイル生成→実行
                        CreateBatch(msg, false, true);
                        System.Diagnostics.Process.Start("cmd.exe", "/C " + Widget.CommonResource.YataFile(batchFile));
                        break;
                    case CommandExecutionType.KarasuCommandLineKeepAlive:
                        //Pause/exitつきでバッチファイル生成
                        CreateBatch(msg, false, true);
                        break;
                    case CommandExecutionType.KarasuCommandLineRedirect:
                        //リダイレクト付きでバッチファイル生成
                        CreateBatch(msg, true, false);
                        break;
                    case CommandExecutionType.KarasuCommandLine:
                        //そのままバッチファイル生成
                        CreateBatch(msg, false, false);
                        break;
                    case CommandExecutionType.KarasProcessStart:
                        //そのままバッチファイル生成
                        CreateBatch(msg, false, false);
                        break;
                    case CommandExecutionType.CommandLine:
                        //直接コール1
                        System.Diagnostics.Process.Start("cmd.exe", "/C " + msg);
                        return;
                    case CommandExecutionType.CommandLineRedirect:
                        //直接コール2
                        System.Diagnostics.Process.Start("cmd.exe", "/C " + msg + " >> " + Widget.CommonResource.YataFile(redirectFile));
                        return;
                    case CommandExecutionType.ProcessStart:
                        //直接コール3
                        System.Diagnostics.Process.Start(msg);
                        return;
                    default:
                        return;
                }
                //ここに来た時はkarasuで実行
                Karasu.Send($"COMMAND\t{mode}");
            }
        }

        private string GetFile()
        {
            if (Clipboard.ContainsFileDropList())
            {
                return Clipboard.GetFileDropList()[0];
            }

            else if (Clipboard.ContainsText())
            {
                var txt = Clipboard.GetText();
                var reg = new Regex(@"^""(.+)""$");
                var m = reg.Match(txt);
                if (m.Success)
                {
                    var fn = m.Groups[1].Value;
                    if (System.IO.File.Exists(fn))
                    {
                        return fn;
                    }
                }
            }

            var f = new OpenFileDialog();
            f.Filter = "all files|*.*";
            if (f.ShowDialog() == DialogResult.OK)
            {
                return f.FileName;
            }
            return null;
        }

        private void CreateBatch(string cmd, bool withRedirect, bool withPause)
        {
            using (var bat = new System.IO.StreamWriter(Widget.CommonResource.YataFile(batchFile)))
            {
                bat.WriteLine("chcp 65001");    //batchfile is utf-8(65001)
                bat.WriteLine(cmd + (withRedirect ? " >> " + redirectFile : ""));
                if (withPause)
                {
                    bat.WriteLine("@echo off");
                    bat.WriteLine("pause");
                    bat.WriteLine("exit");
                }
            }
        }
    }

    public enum CommandExecutionType
    {
        /// <summary>
        /// 直接ProcessStartする。管理者権限で動く。
        /// </summary>
        ProcessStart,
        /// <summary>
        /// コマンドラインで叩く。管理者権限で動く。実行終了したらcmd.exeはそのまま終了(/C)
        /// </summary>
        CommandLine,
        /// <summary>
        /// コマンドラインでたたく。管理者権限で動く。PAUSE/exitつきのバッチファイルを/Cで動かすのでキーを押すと終了する(標準出力が確認できる）
        /// </summary>
        CommandLineKeepAlive,
        /// <summary>
        /// コマンドラインでたたき、管理者権限で動き標準出力はkarasu.console.txtにリダイレクト。/Cで実行。
        /// </summary>
        CommandLineRedirect,

        /// <summary>
        /// Karasu経由でProcessStart(一般権限)
        /// </summary>
        KarasProcessStart,
        /// <summary>
        /// Karasu経由でcmd /C
        /// </summary>
        KarasuCommandLine,
        /// <summary>
        /// Karasu経由でcmd /C + PAUSE + Exec
        /// </summary>
        KarasuCommandLineKeepAlive,
        /// <summary>
        /// Karasu経由でcmd /C >> karasu.console.txt
        /// </summary>
        KarasuCommandLineRedirect,

    }
}
