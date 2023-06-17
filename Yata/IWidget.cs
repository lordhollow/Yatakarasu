using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata
{
    /// <summary>
    /// ウィジェットの基本インターフェース
    /// </summary>
    public interface IWidget
    {
        /// <summary>
        /// スケールモード
        /// </summary>
        WidgetScaleMode ScaleMode { get; }

        /// <summary>
        /// キャプション
        /// </summary>
        string Caption { get; }

        /// <summary>
        /// 幅
        /// </summary>
        int Width { get; set; }
        
        /// <summary>
        /// 高さ
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// 描画
        /// </summary>
        /// <param name="graphics">描画対象グラフィック。0,0から書けばよい</param>
        void Draw(System.Drawing.Graphics graphics);

        /// <summary>
        /// クリック動作
        /// </summary>
        void Click(int x, int y);
    }

    public enum WidgetScaleMode
    {
        Fixed,
        FixedRatio,
        Variable,
        WidthFixed,
        HeightFixed,
    }

}
