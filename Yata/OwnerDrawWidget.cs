using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Yata
{
    public class OwnerDrawWidget : IUpdatable, IWidget
    {
        public OwnerDrawWidget(WidgetScaleMode scaleMode, int initialWidth, int initialHeight)
        {
            ScaleMode = scaleMode;
            width = initialWidth;
            height = initialHeight;
            Ratio = (float)initialWidth / initialHeight;
        }

        /// <summary>
        /// スケーリングのモード
        /// </summary>
        public WidgetScaleMode ScaleMode { get; private set; }

        /// <summary>
        /// キャプション
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// 更新倍率
        /// </summary>
        public int UpdateRate { get; protected set; } = 10;

        /// <summary>
        /// 幅
        /// </summary>
        private int width;

        /// <summary>
        /// 高さ
        /// </summary>
        private int height;

        /// <summary>
        /// 幅
        /// </summary>
        public int Width
        {
            get => width;
            set
            {
                switch (ScaleMode)
                {
                    case WidgetScaleMode.Fixed:
                    case WidgetScaleMode.WidthFixed:
                        //変更不可
                        break;
                    case WidgetScaleMode.FixedRatio:
                        //比率固定。
                        height = (int)(value / Ratio);
                        width = value;
                        break;
                    case WidgetScaleMode.HeightFixed:
                    default: //variable
                        height = value;
                        break;
                }
            }
        }

        /// <summary>
        /// 高さ
        /// </summary>
        public int Height
        {
            get => height;
            set
            {
                switch(ScaleMode)
                {
                    case WidgetScaleMode.Fixed:
                    case WidgetScaleMode.HeightFixed:
                        //変更不可
                        break;
                    case WidgetScaleMode.FixedRatio:
                        //比率固定。
                        width = (int)(Ratio * value);
                        height = value;
                        break;
                    case WidgetScaleMode.WidthFixed:
                    default: //variable
                        height = value;
                        break;
                }
            }
        }

        /// <summary>
        /// 比率
        /// </summary>
        protected float Ratio { get; set; }
        
        /// <summary>
        /// 更新回数(デバッグ用)
        /// </summary>
        int cnt;

        /// <summary>
        /// 更新
        /// </summary>
        public virtual bool Update()
        {
            cnt++;
            return true;
        }

        /// <summary>
        /// 描画。デフォルトではプレースホルダとして赤枠とキャプションを描く。
        /// </summary>
        /// <param name="graphics"></param>
        public virtual void Draw(System.Drawing.Graphics graphics)
        {
            graphics.Clear(Color.Transparent);
            graphics.DrawRectangle(Pens.Red, 0, 0, width - 1, height - 1);
            graphics.DrawString(Caption + " " + cnt.ToString(), SystemFonts.DefaultFont, Brushes.Red, Point.Empty);
        }

        public virtual void Click(int x, int y) { }

    }
}
