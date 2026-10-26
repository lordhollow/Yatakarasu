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

        /// <summary>
        /// アプリケーションの実行
        /// </summary>
        public bool Execute(Karasu karasu)
        {
            if (Application != null)
            {
                Application.Execute(karasu, Option);
                return true;
            }
            else if (!string.IsNullOrEmpty(Command))
            {
                var path = Convert.ToBase64String(Encoding.UTF8.GetBytes(DefinitionPath));
                var msg = $"EXEC2\t{Command}\t{Row}\t{Column}\t{path}";
                //モニタリングのために管理者権限で動いているので
                //一般ユーザーで起動するようにしないとダメ。。。
                karasu.Send(msg);
                return karasu.Active;
            }
            return false;
        }

    }
}
