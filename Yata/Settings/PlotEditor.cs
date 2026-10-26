using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Yata.Components;
using static Yata.Settings.SensorSettingsForm;
namespace Yata.Settings
{
    public partial class PlotEditor : UserControl
    {
        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public DataGridView Series => series;
        public event Action SelectRequested;
        public event Action Changed;
        public PlotEditor() { InitializeComponent(); }
        void AddButton_Click(object sender, EventArgs e)
        {
            if (Series.Rows.Count >= 12) { MessageBox.Show(this, "現在の表示領域では1グラフ12項目までです。"); return; }
            var index = Series.Rows.Add("項目", "", "0,192,255");
            Series.CurrentCell = Series.Rows[index].Cells[0]; Changed?.Invoke();
        }
        void SelectButton_Click(object sender, EventArgs e) => SelectRequested?.Invoke();
        void ColorButton_Click(object sender, EventArgs e)
        {
            if (Series.CurrentRow == null) return;
            using (var dialog = new ColorDialog()) if (dialog.ShowDialog(this) == DialogResult.OK)
                DataCell(Series.CurrentRow, "Color").Value = $"{dialog.Color.R},{dialog.Color.G},{dialog.Color.B}";
        }
        void DeleteButton_Click(object sender, EventArgs e)
        {
            if (Series.CurrentRow != null) { Series.Rows.Remove(Series.CurrentRow); Changed?.Invoke(); }
        }
        internal void LoadDefinition(PlotDefine definition)
        {
            seriesNameColumn.DataPropertyName = "Name";
            seriesIdColumn.DataPropertyName = "Id";
            seriesColorColumn.DataPropertyName = "Color";
            caption.Text = definition.Name; unit.Text = definition.Unit; min.Text = Number(definition.Min); max.Text = Number(definition.Max); low.Text = Number(definition.LowLevelThreshold); high.Text = Number(definition.HighLevelThreshold);
            Series.Rows.Clear();
            foreach (var item in definition.Items) Series.Rows.Add(item.Name, item.Id, $"{item.Color.R},{item.Color.G},{item.Color.B},{item.Color.A}");
        }
        internal PlotDefine Read()
        {
            Series.EndEdit();
            if (string.IsNullOrWhiteSpace(caption.Text)) throw new FormatException("グラフのタイトルを入力してください。");
            var definition = new PlotDefine { Name = caption.Text, Unit = string.IsNullOrEmpty(unit.Text) ? " " : unit.Text, Min = Parse(min.Text), Max = Parse(max.Text), LowLevelThreshold = Parse(low.Text), HighLevelThreshold = Parse(high.Text) };
            foreach (DataGridViewRow row in Series.Rows)
            {
                if (string.IsNullOrWhiteSpace(Cell(row, "Name")) || string.IsNullOrWhiteSpace(Cell(row, "Id"))) throw new FormatException("グラフ項目の表示名とセンサーIDを入力してください。");
                var parts = Cell(row, "Color").Split(',');
                if ((parts.Length != 3 && parts.Length != 4) || parts.Any(x => !byte.TryParse(x, out _))) throw new FormatException("色はR,G,B[,A]形式で、各値0～255を指定してください。");
                definition.Items.Add(new PlotItemDefine { Name = Cell(row, "Name"), Id = Cell(row, "Id"), Color = Color.FromArgb(parts.Length == 4 ? byte.Parse(parts[3]) : 255, byte.Parse(parts[0]), byte.Parse(parts[1]), byte.Parse(parts[2])) });
            }
            return definition;
        }
    }
}
