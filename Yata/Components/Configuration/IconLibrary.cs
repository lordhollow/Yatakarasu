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
