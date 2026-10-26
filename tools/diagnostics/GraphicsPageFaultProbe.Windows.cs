using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

static partial class GraphicsPageFaultProbe
{
    static void RunWindowBuffers()
    {
        Application.EnableVisualStyles();
        using (var background = Layer(PixelFormat.Format32bppPArgb, Color.FromArgb(255, 30, 50, 80)))
        using (var widgets = Layer(PixelFormat.Format32bppPArgb, Color.FromArgb(120, 0, 0, 0)))
        using (var overlay = Layer(PixelFormat.Format32bppPArgb, Color.Transparent))
        using (var window = new ProbeWindow())
        {
            window.PaintFrame = graphics =>
            {
                graphics.Clear(Color.Transparent);
                graphics.DrawImage(background, 0, 0);
                graphics.DrawImage(widgets, 0, 0);
                graphics.DrawImage(overlay, 0, 0);
            };
            window.Show(); Application.DoEvents();
            var context = BufferedGraphicsManager.Current;
            var originalMaximum = context.MaximumBuffer;
            Console.WriteLine("Default MaximumBuffer: " + originalMaximum);
            Console.WriteLine("Window client: " + window.ClientSize);
            try
            {
                for (int round = 1; round <= 3; round++)
                {
                    window.UseDoubleBuffer = true;
                    context.MaximumBuffer = originalMaximum;
                    var before = window.Frames;
                    Measure("WinForms-default-buffer", round, 200, () => window.PaintOnce());
                    Console.WriteLine("Paint calls including warmup: " + (window.Frames - before));
                    if (window.Frames - before != 230) throw new InvalidOperationException("The WM_PAINT path was not exercised.");
                    context.MaximumBuffer = new Size(window.ClientSize.Width + 1, window.ClientSize.Height + 1);
                    Measure("WinForms-retained-buffer", round, 200, () => window.PaintOnce());
                    using (var target = window.CreateGraphics())
                    using (var direct = new BufferedGraphicsContext())
                    {
                        var rect = new Rectangle(Point.Empty, window.ClientSize);
                        direct.MaximumBuffer = originalMaximum;
                        Measure("BufferedGraphics-default-limit", round, 200, () =>
                        {
                            using (var frame = direct.Allocate(target, rect)) { window.PaintFrame(frame.Graphics); frame.Render(); }
                        });
                        direct.MaximumBuffer = new Size(rect.Width + 1, rect.Height + 1);
                        Measure("BufferedGraphics-retained-limit", round, 200, () =>
                        {
                            using (var frame = direct.Allocate(target, rect)) { window.PaintFrame(frame.Graphics); frame.Render(); }
                        });
                    }
                }
            }
            finally { context.MaximumBuffer = originalMaximum; }
        }
    }

    sealed class ProbeWindow : Form
    {
        public Action<Graphics> PaintFrame;
        public int Frames;
        public bool UseDoubleBuffer { set { DoubleBuffered = value; } }
        public ProbeWindow()
        {
            AutoScaleMode = AutoScaleMode.None;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(-20000, -20000);
            ShowInTaskbar = false;
            FormBorderStyle = FormBorderStyle.None;
            ClientSize = new Size(1280, 800);
            DoubleBuffered = true;
        }
        public void PaintOnce()
        {
            //An off-screen HWND has an empty BeginPaint clip. Supply its DC explicitly
            //so Control.WmPaint follows the real double-buffered path for the full client.
            using (var target = CreateGraphics())
            {
                var dc = target.GetHdc();
                try
                {
                    var message = Message.Create(Handle, 0x000f, dc, IntPtr.Zero);
                    base.WndProc(ref message);
                }
                finally { target.ReleaseHdc(dc); }
            }
        }
        protected override void OnPaint(PaintEventArgs e) { Frames++; PaintFrame?.Invoke(e.Graphics); }
    }
}
