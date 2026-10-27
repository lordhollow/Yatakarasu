using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Yata.Components;
using static Yata.Settings.SensorSettingsForm;

namespace Yata.Settings
{
    internal sealed partial class MeterLayersForm : Form
    {
        readonly List<SensorChoice> sensors = new List<SensorChoice>();
        internal List<MeterItemDefine> ResultLayers { get; private set; }
        public MeterLayersForm() { InitializeComponent(); }
        internal MeterLayersForm(IEnumerable<MeterItemDefine> layers, List<SensorChoice> choices) : this()
        {
            sensors = choices ?? new List<SensorChoice>();
            LoadLayers(layers);
        }
        internal void LoadLayers(IEnumerable<MeterItemDefine> layers)
        {
            nameColumn.DataPropertyName = "Name"; unitColumn.DataPropertyName = "Unit";
            minColumn.DataPropertyName = "Min"; maxColumn.DataPropertyName = "Max"; idColumn.DataPropertyName = "Id";
            grid.Rows.Clear();
            foreach (var layer in layers) grid.Rows.Add(layer.Name, layer.Unit, Number(layer.Min), Number(layer.Max), layer.Id);
        }
        internal List<MeterItemDefine> ReadLayers()
        {
            grid.EndEdit();
            if (grid.Rows.Count == 0) throw new FormatException("1項目以上を割り当ててください。");
            var result = new List<MeterItemDefine>();
            foreach (DataGridViewRow row in grid.Rows)
            {
                var item = new MeterItemDefine { Name = Cell(row, "Name"), Unit = Cell(row, "Unit"), Min = Parse(Cell(row, "Min")), Max = Parse(Cell(row, "Max")), Id = Cell(row, "Id") };
                if (item.Min >= item.Max) throw new FormatException($"{row.Index + 1}行目: 最小値より大きい最大値を指定してください。");
                result.Add(item);
            }
            return result;
        }
        void AddButton_Click(object sender, EventArgs e)
        {
            var row = grid.Rows.Add("項目", " ", "0", "100", "-");
            grid.CurrentCell = grid.Rows[row].Cells[0];
        }
        void RemoveButton_Click(object sender, EventArgs e)
        {
            if (grid.CurrentRow != null && grid.Rows.Count > 1) grid.Rows.Remove(grid.CurrentRow);
        }
        void UpButton_Click(object sender, EventArgs e) => MoveLayer(-1);
        void DownButton_Click(object sender, EventArgs e) => MoveLayer(1);
        internal void MoveLayer(int delta)
        {
            grid.EndEdit();
            if (grid.CurrentRow == null) return;
            var row = grid.CurrentRow.Index; var target = row + delta;
            if (target < 0 || target >= grid.Rows.Count) return;
            for (int column = 0; column < grid.Columns.Count; column++)
            {
                var value = grid.Rows[row].Cells[column].Value;
                grid.Rows[row].Cells[column].Value = grid.Rows[target].Cells[column].Value;
                grid.Rows[target].Cells[column].Value = value;
            }
            grid.CurrentCell = grid.Rows[target].Cells[0];
        }
        void SelectButton_Click(object sender, EventArgs e)
        {
            if (grid.CurrentRow == null) return;
            using (var picker = new SensorPickerForm(sensors))
                if (picker.ShowDialog(this) == DialogResult.OK)
                {
                    var row = grid.CurrentRow;
                    DataCell(row, "Id").Value = picker.SelectedSensor.Id;
                    if (string.IsNullOrWhiteSpace(Cell(row, "Name")) || Cell(row, "Name") == "項目" || Cell(row, "Name") == "未設定") DataCell(row, "Name").Value = picker.SelectedSensor.Name;
                    DataCell(row, "Unit").Value = picker.SelectedSensor.Unit;
                }
        }
        void SaveButton_Click(object sender, EventArgs e)
        {
            try { ResultLayers = ReadLayers(); DialogResult = DialogResult.OK; }
            catch (Exception ex) { EditorUi.Error(this, ex); }
        }
    }
}
