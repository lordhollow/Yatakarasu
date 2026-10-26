using System.Collections.Generic;

namespace Yata.Components.Configuration
{
    internal sealed class SystemApplicationChoice
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public string Hint { get; set; }
        public override string ToString() => Caption;
    }
    internal static class SystemApplicationCatalog
    {
        public static readonly List<SystemApplicationChoice> All = new List<SystemApplicationChoice>
        {
            Item("LauncherNextPage", "次のページ", "末尾のページでは先頭へ戻ります。"),
            Item("LauncherPreviousPage", "前のページ", "先頭のページでは末尾へ移動します。"),
            Item("MonitorPowerOff", "モニターの電源を切る", "接続中のモニターを消灯します。マウス・キーボード操作で復帰します。"),
            Item("EditSensors", "センサー設定", "センサー設定画面を開きます。"),
            Item("EditLauncher", "ランチャー編集", "ランチャー編集画面を開きます。"),
            Item("ReloadLauncher", "ランチャーを再読み込み", "現在選択中の定義ファイルを読み直します。"),
            Item("ScreenLock", "画面をロック", "Windowsをロックします。"),
            Item("Suspender", "休止状態", "PCを休止状態にします。"),
            Item("DashboardBrightnessChanger", "パネルの明るさ", "オプション: 0～1（例: 0.5）"),
            Item("SetDisplayInput", "モニター入力切替", "オプション: TOGGLE / DP1 / HDMI1 または 0=HDMI1"),
            Item("SetDisplayTopology", "ディスプレイ構成", "オプション: INTERNAL / EXTERNAL / EXTEND / CLONE"),
            Item("SendKeyStroke", "キーを送る", "オプション: SendKeys形式（例: ^c）"),
            Item("ToggleScreenKeyboard", "スクリーンキーボード", "スクリーンキーボードを切り替えます。"),
            Item("FanControl", "ファン制御", "オプション: 制御センサーID,0～100 または 制御センサーID,auto"),
            Item("CommandWithFile", "ファイル指定で実行", "オプション: コマンド。${infile}を選択ファイルに置換します。"),
            Item("CommandWithFile(K)", "ファイル指定で実行・結果を保持", "オプション: コマンド。${infile}を選択ファイルに置換します。"),
            Item("CommandWithFile(R)", "ファイル指定で実行・出力を保存", "オプション: コマンド。${infile}を選択ファイルに置換します。"),
            Item("CommandWithFile(P)", "ファイル指定で直接起動", "オプション: 実行ファイル。${infile}を選択ファイルに置換します。"),
            Item("CommandWithFile(A)", "管理者としてファイル指定で実行", "オプション: コマンド。${infile}を選択ファイルに置換します。"),
            Item("CommandWithFile(AK)", "管理者として実行・結果を保持", "オプション: コマンド。${infile}を選択ファイルに置換します。"),
            Item("CommandWithFile(AR)", "管理者として実行・出力を保存", "オプション: コマンド。${infile}を選択ファイルに置換します。"),
            Item("CommandWithFile(AP)", "管理者としてファイル指定で直接起動", "オプション: 実行ファイル。${infile}を選択ファイルに置換します。")
        };
        static SystemApplicationChoice Item(string id, string caption, string hint) => new SystemApplicationChoice { Id = id, Caption = caption, Hint = hint };
    }
}
