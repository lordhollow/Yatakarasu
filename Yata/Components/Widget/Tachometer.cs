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
            internal bool LowIsWarning;
            internal Layer(float min, float max, ISensor sensor, string name = null, string unit = null, bool lowIsWarning = false)
            { Min = min; Max = max; Sensor = sensor; Name = name; Unit = unit; LowIsWarning = lowIsWarning; }
        }
        readonly Layer[] layers;
        readonly float[] fractions;
        readonly bool[] available;
        readonly RectangleF[] rings;
        readonly System.Drawing.Drawing2D.GraphicsPath[] outlines;
        readonly Pen[,] pens;
        readonly Font font;
        readonly bool captionOnly;
        bool needUpdate = true;
        string disp = "----";
        string displayText;
        string displayedCaption, displayedUnit;
        // -1 is the all-layer view. This state belongs only to the widget instance.
        int visibleLayer = -1;
        int SingleLayer => visibleLayer >= 0 ? visibleLayer : layers.Length == 1 && !captionOnly ? 0 : -1;
        internal int VisibleLayer => visibleLayer;
        internal string DisplayText => displayText;

        public Tachometer(float min, float max, ISensor sensor) : this(new[] { new Layer(min, max, sensor) }) { }
        internal Tachometer(IEnumerable<Layer> definitions, bool captionOnly = false) : base(WidgetScaleMode.Fixed, 72, 72)
        {
            this.captionOnly = captionOnly;
            layers = definitions.ToArray();
            if (layers.Length == 0 || layers.Any(layer => layer.Sensor == null || !Finite(layer.Min) || !Finite(layer.Max) || layer.Min >= layer.Max))
                throw new ArgumentException("Each meter layer needs a sensor and a finite increasing range.");
            fractions = new float[layers.Length]; available = new bool[layers.Length];
            rings = new RectangleF[layers.Length]; pens = new Pen[layers.Length, 3];
            outlines = new System.Drawing.Drawing2D.GraphicsPath[layers.Length];
            font = new Font("BIZ UDPゴシック", 7.5f);
            // Cache GDI resources and geometry; repeated redraws do not allocate pens/paths.
            var band = 30f / layers.Length;
            var gap = Math.Min(1.5f, band * 0.2f);
            for (int i = 0; i < layers.Length; i++)
            {
                fractions[i] = 1f;
                var radius = 34f - (i + 0.5f) * band;
                rings[i] = new RectangleF(36 - radius, 36 - radius, radius * 2, radius * 2);
                var colors = new[] { Color.DarkSlateBlue, Color.DarkOrange, Color.Red };
                for (int c = 0; c < colors.Length; c++) pens[i, c] = new Pen(colors[c], band - gap);
                var half = (band - gap) / 2;
                var outer = new RectangleF(rings[i].X - half, rings[i].Y - half, rings[i].Width + 2 * half, rings[i].Height + 2 * half);
                var inner = new RectangleF(rings[i].X + half, rings[i].Y + half, rings[i].Width - 2 * half, rings[i].Height - 2 * half);
                var outline = new System.Drawing.Drawing2D.GraphicsPath();
                outline.AddArc(outer, -90, 270);
                outline.AddArc(inner, 180, -270);
                outline.CloseFigure();
                outlines[i] = outline;
            }
        }
        public string Unit { get; set; }
        internal static Tachometer FromDefinition(MeterDefine definition, Func<string, ISensor> resolve)
            => new Tachometer(definition.DisplayLayers.Select(layer => new Layer(layer.Min, layer.Max, resolve(layer.Id), layer.Name, layer.Unit, layer.LowIsWarning)), definition.IsCaptionOnly)
            { Caption = definition.Name, Unit = definition.Unit };
        public override void Click(int x, int y)
        {
            if (layers.Length <= 1 && !captionOnly) return;
            visibleLayer++;
            if (visibleLayer >= layers.Length) visibleLayer = -1;
            needUpdate = true;
        }
        public override void Dispose()
        {
            font.Dispose();
            foreach (var pen in pens) pen.Dispose();
            foreach (var outline in outlines) outline.Dispose();
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
                    : AllLayersName + (available.All(value => value) ? "" : "\r\n----");
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
                var severity = (layers[i].LowIsWarning ? 1f - fractions[i] : fractions[i]) * 270f;
                var color = !available[i] ? 2 : severity > 250 ? 2 : severity > 180 ? 1 : 0;
                // At zero, retain a small warning mark so the empty arc cannot hide the warning.
                if (available[i] && layers[i].LowIsWarning && sweep == 0) sweep = 6f;
                if (SingleLayer >= 0)
                {
                    var brush = color == 2 ? (available[i] ? Brushes.DarkRed : Brushes.Red) : color == 1 ? Brushes.DarkOrange : Brushes.DarkSlateBlue;
                    if (sweep > 0) graphics.FillPie(brush, new Rectangle(2, 2, Width - 4, Height - 4), -90, sweep);
                    graphics.DrawPie(Pens.Gray, new Rectangle(2, 2, Width - 4, Height - 4), -90, 270);
                }
                else
                {
                    if (sweep > 0) graphics.DrawArc(pens[i, color], rings[i], -90, sweep);
                    graphics.DrawPath(Pens.Gray, outlines[i]);
                }
            }
            if (displayText == null) displayText = SingleLayer >= 0 ? $"{LayerName(SingleLayer)}\r\n----\r\n{layers[SingleLayer].Unit ?? Unit}" : AllLayersName + "\r\n----";
            graphics.DrawString(DisplayText, font, Brushes.White, 0, 2);
        }
        string LayerName(int index) => layers[index].Name ?? (index == 0 ? Caption : layers[index].Sensor.Name);
        string AllLayersName => captionOnly ? Caption : LayerName(0);
    }
}
