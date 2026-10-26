using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

// Replays Yata's drawing operations without opening Computer or controlling hardware.
static partial class GraphicsPageFaultProbe
{
    [StructLayout(LayoutKind.Sequential)]
    struct MemoryCounters
    {
        public uint Size, PageFaultCount;
        public UIntPtr PeakWorkingSet, WorkingSet, PeakPagedPool, PagedPool;
        public UIntPtr PeakNonPagedPool, NonPagedPool, Pagefile, PeakPagefile, PrivateBytes;
    }
    [DllImport("psapi.dll", SetLastError = true)]
    static extern bool GetProcessMemoryInfo(IntPtr process, out MemoryCounters counters, uint size);
    static readonly Process process = Process.GetCurrentProcess();
    static readonly List<string> results = new List<string>();

    static MemoryCounters Memory()
    {
        MemoryCounters counters;
        if (!GetProcessMemoryInfo(process.Handle, out counters, (uint)Marshal.SizeOf(typeof(MemoryCounters))))
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        return counters;
    }

    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            var output = args.Length == 0 ? "graphics-page-faults.csv" : args[0];
            results.Add("case,round,iterations,elapsed_ms,total_page_faults,faults_per_iteration,private_bytes_delta,working_set_delta,gc0,gc1,gc2");
            if (args.Length > 1 && args[1] == "--window-buffers") RunWindowBuffers();
            else
            using (var background = Layer(PixelFormat.Format32bppArgb, Color.FromArgb(255, 30, 50, 80)))
            using (var widgets = Layer(PixelFormat.Format32bppArgb, Color.FromArgb(120, 0, 0, 0)))
            using (var overlay = Layer(PixelFormat.Format32bppArgb, Color.Transparent))
            using (var outputBitmap = new Bitmap(1280, 800, PixelFormat.Format32bppArgb))
            using (var graphics = Graphics.FromImage(outputBitmap))
            using (var backgroundP = Convert(background))
            using (var widgetsP = Convert(widgets))
            using (var overlayP = Convert(overlay))
            {
                for (int round = 1; round <= 3; round++)
                {
                    Measure("counter-only", round, 200, () => { });
                    Measure("current-DrawImage-three-layers", round, 200, () =>
                    {
                        graphics.Clear(Color.Transparent);
                        graphics.DrawImage(background, 0, 0);
                        graphics.DrawImage(widgets, 0, 0);
                        graphics.DrawImage(overlay, 0, 0);
                    });
                    Measure("DrawImage-background-only", round, 200, () => graphics.DrawImage(background, 0, 0));
                    Measure("DrawImage-transparent-overlay-only", round, 200, () => graphics.DrawImage(overlay, 0, 0));
                    Measure("DrawImageUnscaled-three-layers", round, 200, () =>
                    {
                        graphics.Clear(Color.Transparent);
                        graphics.DrawImageUnscaled(background, 0, 0);
                        graphics.DrawImageUnscaled(widgets, 0, 0);
                        graphics.DrawImageUnscaled(overlay, 0, 0);
                    });
                    Measure("explicit-pixel-rectangles-three-layers", round, 200, () =>
                    {
                        graphics.Clear(Color.Transparent);
                        var rect = new Rectangle(0, 0, 1280, 800);
                        graphics.DrawImage(background, rect, rect, GraphicsUnit.Pixel);
                        graphics.DrawImage(widgets, rect, rect, GraphicsUnit.Pixel);
                        graphics.DrawImage(overlay, rect, rect, GraphicsUnit.Pixel);
                    });
                    Measure("PArgb-DrawImage-three-layers", round, 200, () =>
                    {
                        graphics.Clear(Color.Transparent);
                        graphics.DrawImage(backgroundP, 0, 0);
                        graphics.DrawImage(widgetsP, 0, 0);
                        graphics.DrawImage(overlayP, 0, 0);
                    });
                    Measure("PArgb-DrawImageUnscaled-three-layers", round, 200, () =>
                    {
                        graphics.Clear(Color.Transparent);
                        graphics.DrawImageUnscaled(backgroundP, 0, 0);
                        graphics.DrawImageUnscaled(widgetsP, 0, 0);
                        graphics.DrawImageUnscaled(overlayP, 0, 0);
                    });
                    Measure("create-dispose-Graphics-only", round, 1000, () => { using (var temporary = Graphics.FromImage(outputBitmap)) { } });
                }
            }
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
            File.WriteAllLines(output, results);
            Console.WriteLine("CSV: " + Path.GetFullPath(output));
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    static Bitmap Layer(PixelFormat format, Color color)
    {
        var bitmap = new Bitmap(1280, 800, format);
        bitmap.SetResolution(96, 96);
        using (var graphics = Graphics.FromImage(bitmap)) graphics.Clear(color);
        return bitmap;
    }
    static Bitmap Convert(Bitmap source)
    {
        var bitmap = Layer(PixelFormat.Format32bppPArgb, Color.Transparent);
        using (var graphics = Graphics.FromImage(bitmap)) graphics.DrawImageUnscaled(source, 0, 0);
        return bitmap;
    }
    static void Measure(string name, int round, int iterations, Action action, int warmup = 30)
    {
        for (int i = 0; i < warmup; i++) action();
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        var before = Memory();
        var gc0 = GC.CollectionCount(0); var gc1 = GC.CollectionCount(1); var gc2 = GC.CollectionCount(2);
        var clock = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++) action();
        clock.Stop();
        var after = Memory();
        var faults = unchecked(after.PageFaultCount - before.PageFaultCount);
        var line = string.Join(",", name, round, iterations, clock.Elapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture), faults,
            ((double)faults / iterations).ToString("F3", CultureInfo.InvariantCulture),
            (long)after.PrivateBytes.ToUInt64() - (long)before.PrivateBytes.ToUInt64(),
            (long)after.WorkingSet.ToUInt64() - (long)before.WorkingSet.ToUInt64(),
            GC.CollectionCount(0) - gc0, GC.CollectionCount(1) - gc1, GC.CollectionCount(2) - gc2);
        results.Add(line);
        Console.WriteLine(line);
    }
}
