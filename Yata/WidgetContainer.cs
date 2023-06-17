using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using Microsoft.Win32;
using System.Security.Principal;
using Yata.Components;

namespace Yata
{
    class WidgetContainer
    {
        /// <summary>
        /// スクリーンバッファの幅
        /// </summary>
        int width = 1024;
        /// <summary>
        /// スクリーンバッファの高さ
        /// </summary>
        int height = 600;

        /// <summary>
        /// 背景画像
        /// </summary>
        Bitmap Background;

        /// <summary>
        /// 明るさ調整用画像
        /// </summary>
        Bitmap Overlay;

        /// <summary>
        /// バックサーフェス
        /// </summary>
        Bitmap BackSurface;

        /// <summary>
        /// バックサーフェス描画用
        /// </summary>
        Graphics BacksurfaceGraphic;

        /// <summary>
        /// 全ウィジェット
        /// </summary>
        List<ComponentHolder> _Widgets = new List<ComponentHolder>();

        public WidgetContainer()
        {
            Brightness = 1f;
        }

        public void Initialize(int logicalWidth, int logicalHeight)
        {
            width = logicalWidth;
            height = logicalHeight;
            Background = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Overlay = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BackSurface = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            //毎回更新するわけではないものはここで作る
            using (var g = Graphics.FromImage(Background))
            {
                try
                {
                    var img = Bitmap.FromFile(Components.Widget.CommonResource.YataFile("background.jpg"));
                    g.DrawImage(img, new Rectangle(0, 0, Width, Height), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
                    var c = Color.FromArgb(160, 0, 0, 0);
                    using (var ovBrush = new SolidBrush(c))
                    {
                        g.FillRectangle(ovBrush, 0, 0, Width, Height);
                    }
                }
                catch
                {
                    g.Clear(Color.DarkBlue);
                }
            }
            using (var g = Graphics.FromImage(Overlay))
            {
                g.Clear(Color.FromArgb((int)(255 * (1 - Brightness)), Color.Black));
            }
            using (var g = Graphics.FromImage(BackSurface))
            {
                g.Clear(Color.Transparent);
            }

            BacksurfaceGraphic = Graphics.FromImage(BackSurface);

        }

        /// <summary>
        /// 明るさ。0～1。
        /// </summary>
        public float Brightness
        {
            get => _Brightness;
            set
            {
                if (value < 0) value = 0;
                if (value > 1) value = 1;
                BrightnessChanged |= (_Brightness != value);
                _Brightness = value;
            }
        }
        float _Brightness;
        bool BrightnessChanged;

        public int Width { get => width; }

        public int Height { get => height; }

        public bool Update()
        {
            //明るさ変更要求の実施
            var ret = BrightnessChanged;
            if (BrightnessChanged)
            {
                using (var overlay = Graphics.FromImage(Overlay))
                {
                    overlay.Clear(Color.FromArgb((int)(255 * (1 - Brightness)), Color.Black));
                }
                BrightnessChanged = false;
            }

            //全ウィジェット更新
            foreach (var holder in _Widgets)
            {
                if (holder.Counter <= 0)
                {
                    if (holder.Component.Update())
                    {
                        var widget = holder.Component as IWidget;
                        if (widget != null)
                        {
                            BacksurfaceGraphic.ResetTransform();
                            BacksurfaceGraphic.TranslateTransform(holder.Position.X, holder.Position.Y);
                            BacksurfaceGraphic.SetClip(new Rectangle(0, 0, widget.Width, widget.Height));
                            widget.Draw(BacksurfaceGraphic);
                            ret = true;
                        }
                    }
                    holder.Counter = holder.Component.UpdateRate;
                }
                holder.Counter--;
            }
            return ret;
        }

        public void Click(int x, int y)
        {
            foreach (var holder in _Widgets.Where(item => item.Component is IWidget))
            {
                var widget = holder.Component as IWidget;
                var rect = new Rectangle(holder.Position, new Size(widget.Width, widget.Height));
                if (rect.Contains(x, y))
                {
                    widget.Click(x - holder.Position.X, y - holder.Position.Y);
                }
            }
        }

        public void Draw(Graphics g)
        {
            g.Clear(Color.Transparent);
            //g.ScaleTransform(0.78f, 0.78f);
            g.DrawImage(Background, 0, 0);
            g.DrawImage(BackSurface, 0, 0);
            g.DrawImage(Overlay, 0, 0);
#if DEBUG
            g.DrawRectangle(Pens.Red, new Rectangle(0, 0, width-1, height-1));
#endif
            //g.ResetTransform();
        }

        public void Add(IUpdatable component)
        {
            Add(component, Point.Empty);
        }

        public void Add(IUpdatable component, Point position)
        {
            if (component != null)
            {
                _Widgets.Add(new ComponentHolder
                {
                    Component = component,
                    Position = position,
                    Counter = 0,
                });
            }
        }

        class ComponentHolder
        {
            public IUpdatable Component;
            public Point Position;
            public int Counter;
        }
    }
}
