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
        readonly List<List<MeterItemDefine>> innerLayers = new List<List<MeterItemDefine>>();
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
        void UpMeterButton_Click(object sender, EventArgs e) => MoveMeter(-1);
        void DownMeterButton_Click(object sender, EventArgs e) => MoveMeter(1);
        internal void MoveMeter(int delta)
        {
            if (meters.CurrentRow == null) return;
            var source = meters.CurrentRow.Index;
            if (!EditorUi.MoveRow(meters, delta)) return;
            var target = meters.CurrentRow.Index;
            var saved = innerLayers[source]; innerLayers[source] = innerLayers[target]; innerLayers[target] = saved;
            Dirty = true;
        }
        void LayersButton_Click(object sender, EventArgs e)
        {
            if (meters.CurrentRow == null) return;
            try
            {
                meters.EndEdit();
                var index = meters.CurrentRow.Index;
                using (var dialog = new MeterLayersForm(ReadMeter(meters.CurrentRow).Layers, sensors))
                    if (dialog.ShowDialog(this) == DialogResult.OK) { ApplyMeterLayers(index, dialog.ResultLayers); Dirty = true; }
            }
            catch (Exception ex) { EditorUi.Error(this, ex); }
        }
        MeterDefine ReadMeter(DataGridViewRow row)
        {
            var meter = new MeterDefine { Name = Cell(row, "Name"), Unit = Cell(row, "Unit"), Min = Parse(Cell(row, "Min")), Max = Parse(Cell(row, "Max")), Id = Cell(row, "Id") };
            meter.InnerLayers.AddRange(innerLayers[row.Index].Select(layer => layer.Clone()));
            return meter;
        }
        internal void ApplyMeterLayers(int index, IList<MeterItemDefine> layers)
        {
            if (layers.Count == 0) throw new FormatException("1項目以上を割り当ててください。");
            var first = layers[0]; var row = meters.Rows[index];
            DataCell(row, "Name").Value = first.Name; DataCell(row, "Unit").Value = first.Unit;
            DataCell(row, "Min").Value = Number(first.Min); DataCell(row, "Max").Value = Number(first.Max); DataCell(row, "Id").Value = first.Id;
            innerLayers[index] = layers.Skip(1).Select(layer => layer.Clone()).ToList();
        }
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
            innerLayers.Clear();
            foreach (var meter in definition.MeterDefines)
            {
                meters.Rows.Add(meter.Name, meter.Unit, Number(meter.Min), Number(meter.Max), meter.Id);
                innerLayers.Add(meter.InnerLayers.Select(layer => layer.Clone()).ToList());
            }
            for (int i = 0; i < 2; i++) plots[i].LoadDefinition(definition.PlotDefines[i]);
            note.Text = trimmed ? "このファイルには上限を超える設定があります。保存すると先頭6メーター・2グラフになります。元ファイルは.bakに保存します。" : "タコメーター6個・グラフ2個。親IDが未割当で子がある場合、親は名前のみ。「層を編集」で項目追加。上へ／下へで配置変更。";
        }

        protected override void SaveDefinition(string path)
        {
            meters.EndEdit();
            var definition = new SensorDefine();
            definition.PlotDefines.AddRange(plots.Select(x => x.Read()));
            foreach (DataGridViewRow row in meters.Rows)
                definition.MeterDefines.Add(ReadMeter(row));
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
