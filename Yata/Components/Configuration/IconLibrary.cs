using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Yata.Components.Configuration
{
    internal sealed class IconCandidate : IDisposable
    {
        public string Name { get; set; }
        public Bitmap Image { get; set; }
        public void Dispose() => Image?.Dispose();
    }

    internal static class IconLibrary
    {
        internal static Bitmap CenteredNavigationIcon(bool next)
        {
            var image = new Bitmap(48, 48, PixelFormat.Format32bppPArgb);
            using (var graphics = Graphics.FromImage(image))
            using (var font = new Font("Segoe UI", 26, FontStyle.Bold))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center, FormatFlags = StringFormatFlags.NoWrap })
            {
                graphics.Clear(Color.FromArgb(35, 65, 100));
                graphics.DrawString(next ? "▼" : "▲", font, Brushes.White, new RectangleF(0, 0, 48, 48), format);
            }
            return image;
        }
        internal static Rectangle NavigationInkBounds(Bitmap image)
        {
            int left = image.Width, top = image.Height, right = -1, bottom = -1;
            for (int y = 0; y < image.Height; y++)
                for (int x = 0; x < image.Width; x++)
                {
                    var color = image.GetPixel(x, y);
                    if (color.R <= 160 || color.G <= 160 || color.B <= 160) continue;
                    left = Math.Min(left, x); top = Math.Min(top, y); right = Math.Max(right, x); bottom = Math.Max(bottom, y);
                }
            return right < left ? Rectangle.Empty : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }
        internal static Bitmap NavigationIcon(bool next)
        {
            // Try layout centering first. Glyph bearings can still leave ▲ one pixel too low.
            var centered = CenteredNavigationIcon(next);
            var ink = NavigationInkBounds(centered);
            if (ink.IsEmpty) return centered;
            int x = (centered.Width - ink.Width) / 2 - ink.X;
            int y = (centered.Height - ink.Height) / 2 - ink.Y;
            if (x == 0 && y == 0) return centered;
            var corrected = new Bitmap(48, 48, PixelFormat.Format32bppPArgb);
            using (var graphics = Graphics.FromImage(corrected))
            {
                graphics.Clear(Color.FromArgb(35, 65, 100));
                graphics.DrawImageUnscaled(centered, x, y);
            }
            centered.Dispose();
            return corrected;
        }
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern uint ExtractIconEx(string file, int index, [Out] IntPtr[] large, [Out] IntPtr[] small, uint count);
        [DllImport("user32.dll")]
        static extern bool DestroyIcon(IntPtr icon);

        public static List<IconCandidate> Load(string file)
        {
            var icons = new List<IconCandidate>();
            if (!File.Exists(file)) return icons;
            var extension = Path.GetExtension(file).ToLowerInvariant();
            if (extension != ".exe" && extension != ".dll" && extension != ".ico")
            {
                using (var image = Image.FromFile(file)) icons.Add(new IconCandidate { Name = Path.GetFileName(file), Image = new Bitmap(image) });
                return icons;
            }
            var count = ExtractIconEx(file, -1, null, null, 0);
            if (count == uint.MaxValue) return icons;
            for (int i = 0; i < Math.Min(count, 256u); i++)
            {
                var large = new IntPtr[1];
                var small = new IntPtr[1];
                try
                {
                    ExtractIconEx(file, i, large, small, 1);
                    var handle = large[0] != IntPtr.Zero ? large[0] : small[0];
                    if (handle == IntPtr.Zero) continue;
                    using (var icon = Icon.FromHandle(handle))
                        icons.Add(new IconCandidate { Name = Path.GetFileName(file) + " #" + i, Image = icon.ToBitmap() });
                }
                finally
                {
                    if (large[0] != IntPtr.Zero) DestroyIcon(large[0]);
                    if (small[0] != IntPtr.Zero && small[0] != large[0]) DestroyIcon(small[0]);
                }
            }
            return icons;
        }

        public static string Save(Image image, string definitionPath)
        {
            byte[] bytes;
            using (var memory = new MemoryStream()) { image.Save(memory, ImageFormat.Png); bytes = memory.ToArray(); }
            string name;
            using (var hash = SHA256.Create()) name = BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
            var directory = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(definitionPath)), "icons");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, name + ".png");
            if (!File.Exists(path)) File.WriteAllBytes(path, bytes);
            return path;
        }
    }
}
