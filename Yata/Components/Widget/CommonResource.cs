using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Yata.Components.Widget
{
    public static class CommonResource
    {

        /// <summary>
        /// ピッチ（グリッド幅）
        /// </summary>
        /// <remarks>
        /// ①横グリッド（42マス）
        /// ②縦グリッド１：公称ディスプレイ縦解像度の時。25マスで計算
        /// ③縦グリッド２：横解像度16としたときの9の長さで計算。
        /// ディスプレイに高さ②の画像を表示しようとしたとき、上か下が切れるようなら③にする必要がある。
        /// もともとはWSVGAディスプレイを検討していたのでそれでレイアウトしている。
        /// 製品によっては③の解像度しか出てないものがあるらしく試してみないとわからない。
        /// 実際に購入したのはWXGAディスプレイ。公称アスペクト比16:9と記載があるが物理解像度は16:10なので現物計測するしかない。
        /// 
        /// WSVGA ※Pitch=24の場合
        /// ①1024 = (42*24)+16
        /// ②600  = (25*24)+0
        /// ③576  = (24*24)+0
        /// 
        /// WXGA ※Pitch=30の場合
        /// ①1280 = (42*30)+20
        /// ②800  = (25*30)+50
        /// ③720  = (24*30)+0
        /// </remarks>
        public const float Pitch = 24;

        /// <summary>
        /// マージン。グリッドを右寄せするときに足す数字。
        /// </summary>
        public const float Margine = 16;

        /// <summary>
        /// アイコンアトラスのサイズ(アイコン画像１個分の元データの幅と高さ)
        /// </summary>
        public const int IconAtrasSize = 48;

        static CommonResource()
        {
            TransparentBackPanelBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0));
            InternalApplicationIconsImage = LoadYataImage("SystemApp.png");
        }

        /// <summary>
        /// 透明パネル
        /// </summary>
        public static Brush TransparentBackPanelBrush { get; private set; }

        /// <summary>
        /// 内部アプリのアイコン画像
        /// </summary>
        public static Image InternalApplicationIconsImage { get; private set; }

        public static Image LoadImage(string file, out Rectangle atras)
        {
            var ex = new Regex(@"^(.+)::(\d+),(\d+)$");
            atras = new Rectangle();
            var m = ex.Match(file);
            if (m.Success)
            {
                var x = int.Parse(m.Groups[2].Value);
                var y = int.Parse(m.Groups[3].Value);
                if (m.Groups[1].Value == "InternalApplicationIconsImage")
                {
                    atras = new Rectangle(x * IconAtrasSize, y * IconAtrasSize, IconAtrasSize, IconAtrasSize);
                    return InternalApplicationIconsImage;
                }
            }
            else
            {
                try
                {
                    return LoadImage(file);
                }
                finally { }
            }
            return null;
        }

        public static Image LoadYataImage(string file)
        {
            return LoadImage(YataFile(file));
        }

        public static Image LoadImage(string file)
        {
            //Image.FromFileはファイルをロックしてしまうのでストリームから読んで返す
            //return Image.FromFile(file);

            //本来↓でいいはずだが読めないファイルがある。
            //using (var stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            //{
            //    return Image.FromStream(stream);
            //}

            //ビットマップ作って返せばとりあえず安定。アニメーションありとか考慮しなければこれがいいかな
            using (var srcImg = Image.FromFile(file))
            {
                return  new Bitmap(srcImg);
            }
        }

        /// <summary>
        /// Application.StartupPath + p
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string YataFile(string p)
        {
            return System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "files", p);
        }

    }
}
