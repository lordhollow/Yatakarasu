using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Yata.Settings
{
    internal static class EditorUi
    {
        public static Button Button(string text, EventHandler action)
        {
            var button = new Button { Text = text, AutoSize = true, MinimumSize = new Size(90, 30), Margin = new Padding(4) };
            button.Click += action;
            return button;
        }
        public static DataGridView Grid(bool editable = true)
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false,
                AllowUserToDeleteRows = false, ReadOnly = !editable, RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowTemplate = { Height = 27 },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 248, 252) }
            };
        }
        public static void Column(DataGridView grid, string name, string title, int weight = 100)
        {
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = name, DataPropertyName = name, HeaderText = title, FillWeight = weight, SortMode = DataGridViewColumnSortMode.NotSortable });
        }
        public static FlowLayoutPanel Toolbar(params Control[] controls)
        {
            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(4) };
            panel.Controls.AddRange(controls);
            return panel;
        }
        public static Label Label(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(5, 9, 5, 5) };
        public static void Error(IWin32Window owner, Exception ex) => MessageBox.Show(owner, ex.Message, "設定を確認してください", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    internal abstract class ConfigurationEditorForm : Form
    {
        readonly TextBox file = new TextBox { ReadOnly = true, TabStop = false, Dock = DockStyle.Fill, Margin = new Padding(5, 8, 5, 5) };
        protected Panel Body { get; } = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        protected bool Dirty { get; set; }
        public string SelectedPath { get; private set; }
        protected abstract void LoadDefinition(string path);
        protected abstract void SaveDefinition(string path);

        protected ConfigurationEditorForm(string title, string path)
        {
            Text = title;
            Font = SystemFonts.MessageBoxFont;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1080, 760);
            MinimumSize = new Size(900, 620);
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var header = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 4, Padding = new Padding(8) };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.Controls.Add(EditorUi.Label("設定ファイル"), 0, 0);
            header.Controls.Add(file, 1, 0);
            header.Controls.Add(EditorUi.Button("開く…", (s, e) => OpenDefinition()), 2, 0);
            header.Controls.Add(EditorUi.Button("保存先を選択…", (s, e) => SelectDestination()), 3, 0);
            var save = EditorUi.Button("保存して反映", (s, e) =>
            {
                try { SaveDefinition(SelectedPath); Dirty = false; DialogResult = DialogResult.OK; Close(); }
                catch (Exception ex) { EditorUi.Error(this, ex); }
            });
            var cancel = EditorUi.Button("キャンセル", (s, e) => Close());
            var footer = EditorUi.Toolbar(save, cancel);
            footer.Dock = DockStyle.Fill;
            root.Controls.Add(header, 0, 0);
            root.Controls.Add(Body, 0, 1);
            root.Controls.Add(footer, 0, 2);
            Controls.Add(root);
            CancelButton = cancel;
            SetPath(path);
            FormClosing += (s, e) => { if (Dirty && !DiscardChanges()) e.Cancel = true; };
        }
        protected void InitializeDefinition()
        {
            try { LoadDefinition(SelectedPath); }
            catch (Exception ex)
            {
                LoadDefinition("");
                Shown += (s, e) => MessageBox.Show(this, "選択中の設定を読み込めないため、新規設定を表示します。別のファイルを開くか、編集して保存してください。保存時に元ファイルを.bakへ退避します。\r\n" + ex.Message, "設定の読み込み", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            Dirty = false;
        }
        void SetPath(string path) { SelectedPath = Path.GetFullPath(path); file.Text = SelectedPath; }
        bool DiscardChanges() => MessageBox.Show(this, "保存していない編集を破棄しますか？", "未保存の変更", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        void OpenDefinition()
        {
            using (var dialog = new OpenFileDialog { Filter = "設定ファイル (*.txt)|*.txt|すべてのファイル|*.*", FileName = SelectedPath })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK || (Dirty && !DiscardChanges())) return;
                try { LoadDefinition(dialog.FileName); SetPath(dialog.FileName); Dirty = false; }
                catch (Exception ex) { EditorUi.Error(this, ex); }
            }
        }
        void SelectDestination()
        {
            using (var dialog = new SaveFileDialog { Filter = "設定ファイル (*.txt)|*.txt", FileName = SelectedPath, OverwritePrompt = false })
                if (dialog.ShowDialog(this) == DialogResult.OK) { SetPath(dialog.FileName); Dirty = true; }
        }
        protected void TrackChanges(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is TextBox text) text.TextChanged += (s, e) => Dirty = true;
                if (control is DataGridView grid) grid.CellValueChanged += (s, e) => Dirty = true;
                TrackChanges(control);
            }
        }
    }
}
