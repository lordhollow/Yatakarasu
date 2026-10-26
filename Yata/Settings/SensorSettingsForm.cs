using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Yata.Components;

namespace Yata.Settings
{
    internal sealed partial class SensorSettingsForm : ConfigurationEditorForm
    {
        readonly List<SensorChoice> sensors = new List<SensorChoice>();
        PlotEditor[] plots => new[] { plot1, plot2 };
        public SensorDefine ResultDefinition { get; private set; }

        public SensorSettingsForm() { InitializeComponent(); }
        public SensorSettingsForm(string path, List<SensorChoice> choices) : this()
        {
            sensors = choices ?? new List<SensorChoice>();
            SetPath(path);
            TrackChanges(Body);
            InitializeDefinition();
        }
        void SelectMeterButton_Click(object sender, EventArgs e) => SelectSensor(meters);
        void ClearMeterButton_Click(object sender, EventArgs e)
        {
            if (meters.CurrentRow != null) { DataCell(meters.CurrentRow, "Id").Value = "-"; Dirty = true; }
        }
        void Plot1_SelectRequested() => SelectSensor(plot1.Series);
        void Plot2_SelectRequested() => SelectSensor(plot2.Series);
        void Plot_Changed() => Dirty = true;

        protected override void LoadDefinition(string path)
        {
            // VS removes bound columns when no design-time DataSource exists.
            // Define the UI in the designer, and map data only at runtime.
            metersNameColumn.DataPropertyName = "Name";
            metersUnitColumn.DataPropertyName = "Unit";
            metersMinColumn.DataPropertyName = "Min";
            metersMaxColumn.DataPropertyName = "Max";
            metersIdColumn.DataPropertyName = "Id";
            var definition = new SensorDefine();
            definition.ReadFromFile(path, true);
            bool trimmed = definition.MeterDefines.Count > 6 || definition.PlotDefines.Count > 2;
            definition.EnsureFixedLayout();
            meters.Rows.Clear();
            foreach (var meter in definition.MeterDefines) meters.Rows.Add(meter.Name, meter.Unit, Number(meter.Min), Number(meter.Max), meter.Id);
            for (int i = 0; i < 2; i++) plots[i].LoadDefinition(definition.PlotDefines[i]);
            note.Text = trimmed ? "このファイルには上限を超える設定があります。保存すると先頭6メーター・2グラフになります。元ファイルは.bakに保存します。" : "タコメーター6個・グラフ2個。未設定のIDは「-」。IDは範囲平均（/1-2）・合計（ID+ID）も入力できます。";
        }

        protected override void SaveDefinition(string path)
        {
            meters.EndEdit();
            var definition = new SensorDefine();
            definition.PlotDefines.AddRange(plots.Select(x => x.Read()));
            foreach (DataGridViewRow row in meters.Rows)
                definition.MeterDefines.Add(new MeterDefine { Name = Cell(row, "Name"), Unit = Cell(row, "Unit"), Min = Parse(Cell(row, "Min")), Max = Parse(Cell(row, "Max")), Id = Cell(row, "Id") });
            definition.Save(path);
            ResultDefinition = definition;
        }

        void SelectSensor(DataGridView grid)
        {
            if (grid.CurrentRow == null) return;
            using (var picker = new SensorPickerForm(sensors))
                if (picker.ShowDialog(this) == DialogResult.OK)
                {
                    DataCell(grid.CurrentRow, "Id").Value = picker.SelectedSensor.Id;
                    if (string.IsNullOrWhiteSpace(Cell(grid.CurrentRow, "Name")) || Cell(grid.CurrentRow, "Name") == "未設定" || Cell(grid.CurrentRow, "Name") == "項目") DataCell(grid.CurrentRow, "Name").Value = picker.SelectedSensor.Name;
                    if (grid.Columns.Cast<DataGridViewColumn>().Any(column => column.DataPropertyName == "Unit")) DataCell(grid.CurrentRow, "Unit").Value = picker.SelectedSensor.Unit;
                    Dirty = true;
                }
        }
        internal static DataGridViewCell DataCell(DataGridViewRow row, string propertyName)
            => row.Cells[row.DataGridView.Columns.Cast<DataGridViewColumn>().Single(column => column.DataPropertyName == propertyName).Index];
        internal static string Cell(DataGridViewRow row, string column) => Convert.ToString(DataCell(row, column).Value) ?? "";
        internal static string Number(float value) => value.ToString("R", CultureInfo.InvariantCulture);
        internal static float Parse(string value)
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) || float.IsNaN(number) || float.IsInfinity(number)) throw new FormatException("数値欄には有限の数値を入力してください（小数点は .）。");
            return number;
        }

    }
}
