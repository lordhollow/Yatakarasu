using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace Yata.Components.Widget
{
    class Tachometer : OwnerDrawWidget
    {
        internal sealed class Layer
        {
            internal float Min, Max;
            internal ISensor Sensor;
            internal string Name, Unit;
            internal Layer(float min, float max, ISensor sensor, string name = null, string unit = null)
            { Min = min; Max = max; Sensor = sensor; Name = name; Unit = unit; }
        }
        readonly Layer[] layers;
        readonly float[] fractions;
        readonly bool[] available;
        readonly RectangleF[] rings;
        readonly Pen[,] pens;
        readonly Font font;
        bool needUpdate = true;
        string disp = "----";
        string displayText;
        string displayedCaption, displayedUnit;
        // -1 is the all-layer view. This state belongs only to the widget instance.
        int visibleLayer = -1;
        int SingleLayer => visibleLayer >= 0 ? visibleLayer : layers.Length == 1 ? 0 : -1;
        internal int VisibleLayer => visibleLayer;
        internal string DisplayText => displayText;

        public Tachometer(float min, float max, ISensor sensor) : this(new[] { new Layer(min, max, sensor) }) { }
        internal Tachometer(IEnumerable<Layer> definitions) : base(WidgetScaleMode.Fixed, 72, 72)
        {
            layers = definitions.ToArray();
            if (layers.Length == 0 || layers.Any(layer => layer.Sensor == null || !Finite(layer.Min) || !Finite(layer.Max) || layer.Min >= layer.Max))
                throw new ArgumentException("Each meter layer needs a sensor and a finite increasing range.");
            fractions = new float[layers.Length]; available = new bool[layers.Length];
            rings = new RectangleF[layers.Length]; pens = new Pen[layers.Length, 4];
            font = new Font("BIZ UDPゴシック", 7.5f);
            // Cache GDI resources and geometry; repeated redraws do not allocate pens/paths.
            var band = 30f / layers.Length;
            var gap = Math.Min(1.5f, band * 0.2f);
            for (int i = 0; i < layers.Length; i++)
            {
                fractions[i] = 1f;
                var radius = 34f - (i + 0.5f) * band;
                rings[i] = new RectangleF(36 - radius, 36 - radius, radius * 2, radius * 2);
                var colors = new[] { Color.DarkSlateBlue, Color.DarkOrange, Color.Red, Color.FromArgb(65, 65, 65) };
                for (int c = 0; c < colors.Length; c++) pens[i, c] = new Pen(colors[c], band - gap);
            }
        }
        public string Unit { get; set; }
        public override void Click(int x, int y)
        {
            if (layers.Length <= 1) return;
            visibleLayer++;
            if (visibleLayer >= layers.Length) visibleLayer = -1;
            needUpdate = true;
        }
        public override void Dispose()
        {
            font.Dispose();
            foreach (var pen in pens) pen.Dispose();
        }
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        public override bool Update()
        {
            var changed = needUpdate || Caption != displayedCaption || Unit != displayedUnit;
            needUpdate = false;
            for (int i = 0; i < layers.Length; i++)
            {
                var sample = layers[i].Sensor.Value;
                var valid = sample.HasValue && Finite(sample.Value);
                var fraction = valid ? (float)Math.Max(0, Math.Min(1, ((double)sample.Value - layers[i].Min) / ((double)layers[i].Max - layers[i].Min))) : 1f;
                if ((SingleLayer < 0 || i == SingleLayer) && (available[i] != valid || fractions[i] != fraction)) changed = true;
                available[i] = valid; fractions[i] = fraction;
                if (i == SingleLayer)
                {
                    var text = valid ? Math.Max(layers[i].Min, Math.Min(layers[i].Max, sample.Value)).ToString(layers[i].Sensor.SensorType == SensorType.Fan ? "0" : "0.0") : "----";
                    if (disp != text) changed = true;
                    disp = text;
                }
            }
            if (changed)
            {
                displayedCaption = Caption; displayedUnit = Unit;
                displayText = SingleLayer >= 0
                    ? $"{LayerName(SingleLayer)}\r\n{disp}\r\n{layers[SingleLayer].Unit ?? Unit}"
                    : LayerName(0) + (available.All(value => value) ? "" : "\r\n----");
            }
            return changed;
        }
        public override void Draw(Graphics graphics)
        {
            graphics.Clear(Color.Transparent);
            for (int i = 0; i < layers.Length; i++)
            {
                if (SingleLayer >= 0 && i != SingleLayer) continue;
                var sweep = fractions[i] * 270f;
                var color = !available[i] ? 2 : sweep > 250 ? 2 : sweep > 180 ? 1 : 0;
                if (SingleLayer >= 0)
                {
                    var brush = color == 2 ? (available[i] ? Brushes.DarkRed : Brushes.Red) : color == 1 ? Brushes.DarkOrange : Brushes.DarkSlateBlue;
                    if (sweep > 0) graphics.FillPie(brush, new Rectangle(2, 2, Width - 4, Height - 4), -90, sweep);
                    graphics.DrawPie(Pens.Gray, new Rectangle(2, 2, Width - 4, Height - 4), -90, 270);
                }
                else
                {
                    graphics.DrawArc(pens[i, 3], rings[i], -90, 270);
                    if (sweep > 0) graphics.DrawArc(pens[i, color], rings[i], -90, sweep);
                }
            }
            if (displayText == null) displayText = SingleLayer >= 0 ? $"{LayerName(SingleLayer)}\r\n----\r\n{layers[SingleLayer].Unit ?? Unit}" : LayerName(0) + "\r\n----";
            graphics.DrawString(DisplayText, font, Brushes.White, 0, 2);
        }
        string LayerName(int index) => layers[index].Name ?? (index == 0 ? Caption : layers[index].Sensor.Name);
    }
}
