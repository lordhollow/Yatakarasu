using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Yata.Components;

namespace Yata.Settings
{
    internal sealed class SensorSettingsForm : ConfigurationEditorForm
    {
        readonly DataGridView meters = EditorUi.Grid();
        readonly List<SensorChoice> sensors;
        readonly PlotEditor[] plots = { new PlotEditor(), new PlotEditor() };
        readonly Label note = EditorUi.Label("タコメーター6個・グラフ2個。未設定のIDは「-」。センサーIDは平均・合計の式も入力できます。");
        public SensorDefine ResultDefinition { get; private set; }

        public SensorSettingsForm(string path, List<SensorChoice> choices) : base("センサー選択・設定", path)
        {
            sensors = choices;
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var meterTab = new TabPage("タコメーター（6個）");
            EditorUi.Column(meters, "Name", "表示名", 90);
            EditorUi.Column(meters, "Unit", "単位", 45);
            EditorUi.Column(meters, "Min", "最小", 45);
            EditorUi.Column(meters, "Max", "最大", 45);
            EditorUi.Column(meters, "Id", "センサーID", 240);
            meterTab.Controls.Add(meters);
            meterTab.Controls.Add(EditorUi.Toolbar(EditorUi.Button("センサーを選択…", (s, e) => SelectSensor(meters)), EditorUi.Button("選択を解除", (s, e) => { if (meters.CurrentRow != null) { meters.CurrentRow.Cells["Id"].Value = "-"; Dirty = true; } })));
            tabs.TabPages.Add(meterTab);
            for (int i = 0; i < 2; i++)
            {
                var editor = plots[i];
                var page = new TabPage("グラフ " + (i + 1));
                page.Controls.Add(editor);
                editor.SelectRequested += () => SelectSensor(editor.Series);
                editor.Changed += () => Dirty = true;
                tabs.TabPages.Add(page);
            }
            Body.Controls.Add(tabs);
            note.Dock = DockStyle.Bottom;
            Body.Controls.Add(note);
            TrackChanges(Body);
            InitializeDefinition();
        }

        protected override void LoadDefinition(string path)
        {
            var definition = new SensorDefine();
            definition.ReadFromFile(path, true);
            bool trimmed = definition.MeterDefines.Count > 6 || definition.PlotDefines.Count > 2;
            definition.EnsureFixedLayout();
            meters.Rows.Clear();
            foreach (var meter in definition.MeterDefines) meters.Rows.Add(meter.Name, meter.Unit, Number(meter.Min), Number(meter.Max), meter.Id);
            for (int i = 0; i < 2; i++) plots[i].Load(definition.PlotDefines[i]);
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
                    grid.CurrentRow.Cells["Id"].Value = picker.SelectedSensor.Id;
                    if (string.IsNullOrWhiteSpace(Cell(grid.CurrentRow, "Name")) || Cell(grid.CurrentRow, "Name") == "未設定" || Cell(grid.CurrentRow, "Name") == "項目") grid.CurrentRow.Cells["Name"].Value = picker.SelectedSensor.Name;
                    if (grid.Columns.Contains("Unit")) grid.CurrentRow.Cells["Unit"].Value = picker.SelectedSensor.Unit;
                    Dirty = true;
                }
        }
        internal static string Cell(DataGridViewRow row, string column) => Convert.ToString(row.Cells[column].Value) ?? "";
        internal static string Number(float value) => value.ToString("R", CultureInfo.InvariantCulture);
        internal static float Parse(string value)
        {
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) || float.IsNaN(number) || float.IsInfinity(number)) throw new FormatException("数値欄には有限の数値を入力してください（小数点は .）。");
            return number;
        }

        sealed class PlotEditor : Panel
        {
            readonly TextBox caption = new TextBox { Width = 120 };
            readonly TextBox unit = new TextBox { Width = 50 };
            readonly TextBox min = new TextBox { Width = 65 };
            readonly TextBox max = new TextBox { Width = 65 };
            readonly TextBox low = new TextBox { Width = 65 };
            readonly TextBox high = new TextBox { Width = 65 };
            public DataGridView Series { get; } = EditorUi.Grid();
            public event Action SelectRequested;
            public event Action Changed;
            public PlotEditor()
            {
                Dock = DockStyle.Fill;
                EditorUi.Column(Series, "Name", "表示名", 90);
                EditorUi.Column(Series, "Id", "センサーID", 230);
                EditorUi.Column(Series, "Color", "色 R,G,B[,A]", 80);
                Controls.Add(Series);
                Controls.Add(EditorUi.Toolbar(EditorUi.Button("項目を追加", (s, e) =>
                {
                    if (Series.Rows.Count >= 12) { MessageBox.Show(this, "現在の表示領域では1グラフ12項目までです。"); return; }
                    var index = Series.Rows.Add("項目", "", "0,192,255");
                    Series.CurrentCell = Series.Rows[index].Cells[0]; Changed?.Invoke();
                }), EditorUi.Button("センサーを選択…", (s, e) => SelectRequested?.Invoke()), EditorUi.Button("色を選択…", (s, e) =>
                {
                    if (Series.CurrentRow == null) return;
                    using (var dialog = new ColorDialog()) if (dialog.ShowDialog(this) == DialogResult.OK)
                        Series.CurrentRow.Cells["Color"].Value = $"{dialog.Color.R},{dialog.Color.G},{dialog.Color.B}";
                }), EditorUi.Button("項目を削除", (s, e) => { if (Series.CurrentRow != null) { Series.Rows.Remove(Series.CurrentRow); Changed?.Invoke(); } })));
                Controls.Add(EditorUi.Toolbar(EditorUi.Label("タイトル"), caption, EditorUi.Label("単位"), unit, EditorUi.Label("最小"), min, EditorUi.Label("最大"), max, EditorUi.Label("下境界"), low, EditorUi.Label("上境界"), high));
            }
            public void Load(PlotDefine definition)
            {
                caption.Text = definition.Name; unit.Text = definition.Unit; min.Text = Number(definition.Min); max.Text = Number(definition.Max); low.Text = Number(definition.LowLevelThreshold); high.Text = Number(definition.HighLevelThreshold);
                Series.Rows.Clear();
                foreach (var item in definition.Items) Series.Rows.Add(item.Name, item.Id, $"{item.Color.R},{item.Color.G},{item.Color.B},{item.Color.A}");
            }
            public PlotDefine Read()
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
}
