using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Widget
{
    class CommandLauncher : OwnerDrawWidget
    {

        Font capFont;
        Brush blankBrush;
        StringFormat center;
        bool needUpdate = true;


        public class ConstructionParameter
        {
            /// <summary>
            /// 基準幅
            /// </summary>
            public int Width = 1024;

            /// <summary>
            /// 基準高さ
            /// </summary>
            public int Height = 144;

            /// <summary>
            /// 縦に並ぶアイコンの数
            /// </summary>
            public int IconRows { get; private set; } = 2;

            /// <summary>
            /// 横に並ぶアイコンの数
            /// </summary>
            public int IconsColumns { get; private set; } = 16;

            /// <summary>
            /// アイコンのサイズ
            /// </summary>
            public int ButtonSize { get; private set; } = 48;

            /// <summary>
            /// キャプションの高さ
            /// </summary>
            public int CaptionHeight { get; private set; } = 12;

            /// <summary>
            /// 垂直パディング(分割領域からアイコン左上への距離Y）
            /// </summary>
            public int VPadding { get; private set; } = 6;

            /// <summary>
            /// 水平パディング(分割領域からアイコン左上への距離X）
            /// </summary>
            public int HPadding { get; private set; } = 8;

            /// <summary>
            /// 強調枠の幅
            /// </summary>
            public int IndicatorWidth { get; private set; } = 3;

            /// <summary>
            /// フォントサイズ
            /// </summary>
            public float FontSize { get; private set; } = 8.0f;

            /// <summary>
            /// フォント名
            /// </summary>
            public string FontName { get; private set; } = "BIZ UDゴシック";

            /// <summary>
            /// フォントスタイル
            /// </summary>
            public FontStyle FontStyle { get; private set; } = FontStyle.Bold;

            public ConstructionParameter() { }

            public static ConstructionParameter WSVGA { get => new ConstructionParameter(); }

            public static ConstructionParameter WXGA
            {
                get
                {
                    return new ConstructionParameter()
                    {
                        Width = 1280,
                        Height = 180,
                        ButtonSize = 64,
                        HPadding = 12,
                        VPadding = 9,
                    };
                }
            }
        }

        public ConstructionParameter Parameter { get; set; }

        /// <summary>
        /// クリックしたアイコンの強調期間
        /// </summary>
        const int indicationTime = 30;

        /// <summary>
        /// 1024=Width, 144=3pitch
        /// </summary>
        public CommandLauncher(ConstructionParameter p) : base(WidgetScaleMode.Fixed, p.Width, p.Height)
        {
            Parameter = p;

            capFont = new Font(Parameter.FontName, Parameter.FontSize, Parameter.FontStyle);
            blankBrush = new SolidBrush(Color.FromArgb(64, 0, 0, 32));
            center = new StringFormat { Alignment = StringAlignment.Center };
            UpdateRate = 1;
        }


        public override bool Update()
        {
            var ret = needUpdate;
            needUpdate = false;

            if (indicationLast > 0)
            {
                indicationLast -= UpdateRate;
                if (indicationLast < 0) indicationLast = 0;
                ret = true;
            }

            return ret;
        }

        public override void Draw(Graphics graphics)
        {
            graphics.Clear(Color.Transparent);

            //縦144を半分に割って「1行当たり72。うちアイコンが48あるので残りが24。
            //半分をキャプションに使って残り12の半分、6がオフセット。
            //横には16個等間隔で並べる。64幅にセンタリングして置いていけばよい。
            var hWidth = Width / Parameter.IconsColumns;

            for (var r = 0; r < Parameter.IconRows; r++)
            {
                var baseY = Height / Parameter.IconRows * r;
                for (var c = 0; c < Parameter.IconsColumns; c++)
                {
                    var baseX = c * hWidth;

                    var app = this[r + LineOffset, c];

                    if ((indicationLast > 0) && (r + LineOffset == lastClickAppY) && (c == lastClickAppX))
                    {
                        var indicatorColor = lastClickState ? Color.Lime : Color.Red;
                        using (var b = new SolidBrush(Color.FromArgb(255 / indicationTime * indicationLast, indicatorColor)))
                        {
                            graphics.FillRectangle(b, baseX + Parameter.HPadding - Parameter.IndicatorWidth, baseY + Parameter.VPadding - Parameter.IndicatorWidth,
                                Parameter.ButtonSize + (Parameter.IndicatorWidth * 2), Parameter.ButtonSize + (Parameter.IndicatorWidth * 2));
                        }
                    }

                    var iconRect = new Rectangle(baseX + Parameter.HPadding, baseY + Parameter.VPadding, Parameter.ButtonSize, Parameter.ButtonSize);
                    if (app != null)
                    {
                        if (app.Image != null)
                        {
                            var srcRect = new Rectangle(0, 0, app.Image.Width, app.Image.Height);
                            if (app.Atras != Rectangle.Empty)
                            {
                                srcRect = app.Atras;
                            }
                            graphics.DrawImage(app.Image, iconRect, srcRect, GraphicsUnit.Pixel);
                        }
                        else
                        {
                            graphics.FillRectangle(Brushes.DarkSlateBlue, iconRect);

                        }

                        var formatBox = new Rectangle(baseX + Parameter.HPadding / 2, baseY + Parameter.VPadding + Parameter.ButtonSize + 2, hWidth, Parameter.CaptionHeight);
                        //graphics.DrawRectangle(Pens.Red, formatBox);
                        graphics.DrawString(app.Caption, capFont, Brushes.LightGray, formatBox, center);
                    }
                    else
                    {
                        graphics.FillRectangle(blankBrush, iconRect);
                    }
                }
            }
        }

        public override void Click(int x, int y)
        {
            //分割領域の大きさ
            var vSize = Height / Parameter.IconRows;
            var hSize = Width / Parameter.IconsColumns;

            var ix = x / hSize;
            var iy = y / vSize;

            if (ix < 0) return;
            if (iy < 0) return;
            if (ix > Parameter.IconsColumns) return;
            if (iy > Parameter.IconRows) return;

            var app = this[iy + LineOffset, ix];

            if (app != null)
            {
                lastClickAppX = ix;
                lastClickAppY = iy + LineOffset;
                indicationLast = indicationTime;

                lastClickState = app.Execute(Karasu);
            }
        }

        int lastClickAppX = 0;
        int lastClickAppY = 0;
        int indicationLast = 0;
        bool lastClickState = false;

        /// <summary>
        /// 表示ラインオフセット
        /// </summary>
        public int LineOffset
        {
            get => _LineOffset;
            set
            {
                if (value < 0) value = 0;
                if (_LineOffset != value) needUpdate = true;
                _LineOffset = value;
            }
        }
        int _LineOffset;

        int posToApplicationIndex(int row, int col)
        {
            return row * 16 + col;
        }

        Dictionary<int, LauncherApplication> applications = new Dictionary<int, LauncherApplication>();

        public LauncherApplication this[int row, int col]
        {
            get
            {
                var i = posToApplicationIndex(row, col);
                if (applications.ContainsKey(i))
                {
                    return applications[i];
                }
                return null;
            }
            set
            {
                var i = posToApplicationIndex(row, col);
                applications[i] = value;
                needUpdate = true;
            }
        }

        /// <summary>
        /// カラスくん
        /// </summary>
        public Karasu Karasu { get; set; }

        /// <summary>
        /// 定義ファイル
        /// </summary>
        private string defFile;

        /// <summary>
        /// ファイルから参照
        /// </summary>
        /// <param name="defFile"></param>
        /// <param name="container"></param>
        public void LoadFromFile(string defFile, WidgetContainer container)
        {
            this.defFile = defFile;
            var lines = System.IO.File.ReadAllLines(defFile, Encoding.UTF8);
            KarasKey = lines[0];
            for (var i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split('\t');
                try
                {
                    if (parts.Length >= 4)
                    {
                        var r = int.Parse(parts[0]);
                        var c = int.Parse(parts[1]);
                        var app = new LauncherApplication();
                        app.Caption = parts[2];
                        if (parts[3].StartsWith("internal::"))
                        {
                            app.Application = GetInternalApplication(parts[3].Substring("internal::".Length), container);
                        }
                        else
                        {
                            app.Command = KarasKey;
                        }
                        this[r, c] = app;

                        //icon
                        if (parts.Length >= 5)
                        {
                            if (parts[4] != "")
                            {
                                Rectangle atras;
                                app.Image = CommonResource.LoadImage(parts[4], out atras);
                                app.Atras = atras;
                            }
                        }

                        //param
                        if (parts.Length >= 6)
                        {
                            app.Option = parts[5];
                        }
                    }
                }
                catch (FormatException)
                {
                    //r/cのパースに失敗。無視して次の行へ。(最初を数字以外で始めるとコメント行として使える。 *とか使う。
                }
                finally { }
            }
        }

        /// <summary>
        /// これ、コンテナ以外が必要になるときは全部まとめてクラスにするべき。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        Application.IInternalApplication GetInternalApplication(string name, WidgetContainer container)
        {
            //リロードはこいつの都合なのでここで生成。それ以外はプロバイダから取得する。
            switch (name)
            {
                case "ReloadLauncher":
                    return new Application.ActionInvoker(() => { applications.Clear(); LoadFromFile(defFile, container); needUpdate = true; });
                default:
                    return Application.ApplicationProvider.GetInternalApplication(name, container);
            }
        }

        string KarasKey = "";


    }
}

