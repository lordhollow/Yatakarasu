using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Yata.Components
{
    /// <summary>
    /// ランチャーに登録するアプリケーションの定義
    /// </summary>
    class LauncherApplication
    {
        /// <summary>
        /// キャプション
        /// </summary>
        public string Caption;

        /// <summary>
        /// コマンドライン
        /// </summary>
        public string Command;

        /// <summary>
        /// 内部アプリ
        /// </summary>
        public Application.IInternalApplication Application;

        /// <summary>
        /// アイコン画像
        /// </summary>
        public Image Image;

        /// <summary>
        /// アイコン画像の利用領域。Emptyなら全域。
        /// </summary>
        public Rectangle Atras;

        /// <summary>
        /// コマンドラインオプション、または内部アプリのオプション
        /// </summary>
        public string Option;
        public string DefinitionPath;
        public int Row;
        public int Column;
        public bool IsNavigation;
        readonly object executionGate = new object();
        readonly System.Diagnostics.Stopwatch clickClock = System.Diagnostics.Stopwatch.StartNew();
        internal Func<long> ClickMilliseconds;
        long nextAllowedClick;
        Task<LaunchResult> execution;

        internal bool TryExecute(Karasu karasu, out Task<LaunchResult> completion)
        {
            lock (executionGate)
            {
                var now = ClickMilliseconds != null ? ClickMilliseconds() : clickClock.ElapsedMilliseconds;
                bool suppressRepeats = !IsNavigation;
                if (suppressRepeats && execution != null && (!execution.IsCompleted || now < nextAllowedClick))
                {
                    nextAllowedClick = now + 2000;
                    completion = execution;
                    return false;
                }
                nextAllowedClick = now + 2000;
                var reservation = new TaskCompletionSource<LaunchResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                execution = completion = reservation.Task;
                try
                {
                    if (Application != null)
                    {
                        var previous = karasu?.LastRequest;
                        Application.Execute(karasu, Option);
                        var submitted = karasu?.LastRequest;
                        _ = CompleteExecution(submitted != null && !ReferenceEquals(previous, submitted) ? submitted : Task.FromResult(new LaunchResult("ok", "処理が完了しました。")), reservation);
                    }
                    else if (!string.IsNullOrEmpty(Command) && karasu != null)
                    {
                        var path = Convert.ToBase64String(Encoding.UTF8.GetBytes(DefinitionPath));
                        _ = CompleteExecution(karasu.RequestAsync($"EXEC2\t{Command}\t{Row}\t{Column}\t{path}"), reservation);
                    }
                    else reservation.SetResult(new LaunchResult("error", "起動項目またはKarasuが利用できません。"));
                }
                catch (Exception ex) { reservation.TrySetResult(new LaunchResult("error", ex.Message)); }
                return true;
            }
        }
        static async Task CompleteExecution(Task<LaunchResult> operation, TaskCompletionSource<LaunchResult> completion)
        {
            try { completion.TrySetResult(await operation.ConfigureAwait(false)); }
            catch (Exception ex) { completion.TrySetResult(new LaunchResult("error", ex.Message)); }
        }

        /// <summary>
        /// アプリケーションの実行
        /// </summary>
        public bool Execute(Karasu karasu)
        {
            Task<LaunchResult> completion;
            return TryExecute(karasu, out completion) && completion.IsCompleted && completion.Result.Success;
        }

    }
}
