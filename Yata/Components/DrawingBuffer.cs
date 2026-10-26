using System;
using System.Drawing;

namespace Yata.Components
{
    internal static class DrawingBuffer
    {
        public static void EnsureCapacity(Size clientSize)
        {
            if (clientSize.Width <= 0 || clientSize.Height <= 0) return;
            var context = BufferedGraphicsManager.Current;
            var current = context.MaximumBuffer;
            //既定の225x96を超えると毎フレーム一時DIBが作られる。
            //他のウィンドウのために確保済みの上限は縮小しない。
            var required = new Size(Math.Max(current.Width, checked(clientSize.Width + 1)),
                Math.Max(current.Height, checked(clientSize.Height + 1)));
            if (current != required) context.MaximumBuffer = required;
        }
    }
}
