using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;

namespace Yata.Settings
{
    internal sealed class SensorChoice
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public override string ToString() => Description;
        public static List<SensorChoice> FromSensors(IEnumerable<ISensor> sensors)
        {
            var all = sensors.ToList();
            return all.Select(sensor => new SensorChoice
            {
                Id = all.Count(x => x.Identifier.ToString() == sensor.Identifier.ToString()) > 1 ? "(" + sensor.Name + ")" + sensor.Identifier : sensor.Identifier.ToString(),
                Name = sensor.Name, Unit = UnitFor(sensor.SensorType),
                Description = sensor.Name + "  |  " + sensor.SensorType + "  |  " + sensor.Identifier
            }).OrderBy(x => x.Description).ToList();
        }
        static string UnitFor(SensorType type)
        {
            switch (type)
            {
                case SensorType.Temperature: return "°C";
                case SensorType.Load: case SensorType.Control: return "%";
                case SensorType.Fan: return "rpm";
                case SensorType.Clock: return "MHz";
                case SensorType.Power: return "W";
                case SensorType.Voltage: return "V";
                case SensorType.Data: return "GB";
                case SensorType.SmallData: return "MB";
                case SensorType.Throughput: return "B/s";
                default: return " ";
            }
        }
    }

    internal sealed class SensorPickerForm : Form
    {
        public SensorChoice SelectedSensor { get; private set; }
        public SensorPickerForm(List<SensorChoice> sensors)
        {
            Text = "センサーを選択";
            Size = new Size(880, 540);
            MinimumSize = new Size(650, 400);
            StartPosition = FormStartPosition.CenterParent;
            Font = SystemFonts.MessageBoxFont;
            var list = new ListBox { Dock = DockStyle.Fill, HorizontalScrollbar = true, IntegralHeight = false };
            var filter = new TextBox { Width = 480 };
            Action refresh = () => { list.DataSource = sensors.Where(x => x.Description.IndexOf(filter.Text, StringComparison.OrdinalIgnoreCase) >= 0).ToList(); };
            filter.TextChanged += (s, e) => refresh();
            var select = EditorUi.Button("選択", (s, e) => { if (list.SelectedItem is SensorChoice choice) { SelectedSensor = choice; DialogResult = DialogResult.OK; } });
            list.DoubleClick += (s, e) => select.PerformClick();
            var cancel = EditorUi.Button("キャンセル", (s, e) => { DialogResult = DialogResult.Cancel; });
            Controls.Add(list);
            Controls.Add(EditorUi.Toolbar(EditorUi.Label("名前・種類・IDで検索"), filter));
            var footer = EditorUi.Toolbar(select, cancel); footer.Dock = DockStyle.Bottom; Controls.Add(footer);
            AcceptButton = select; CancelButton = cancel;
            refresh();
        }
    }
}
