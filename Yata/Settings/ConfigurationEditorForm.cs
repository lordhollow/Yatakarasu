using System;
using System.IO;
using System.Windows.Forms;
namespace Yata.Settings
{
    public partial class ConfigurationEditorForm : Form
    {
        protected bool Dirty { get; set; }
        public string SelectedPath { get; private set; } = "";
        protected virtual void LoadDefinition(string path) { }
        protected virtual void SaveDefinition(string path) { }
        public ConfigurationEditorForm() { InitializeComponent(); }
        void OpenButton_Click(object sender, EventArgs e) => OpenDefinition();
        void DestinationButton_Click(object sender, EventArgs e) => SelectDestination();
        void SaveButton_Click(object sender, EventArgs e)
        {
            try { SaveDefinition(SelectedPath); Dirty = false; DialogResult = DialogResult.OK; Close(); }
            catch (Exception ex) { EditorUi.Error(this, ex); }
        }
        void CancelButton_Click(object sender, EventArgs e) => Close();
        void Editor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Dirty && !DiscardChanges()) e.Cancel = true;
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
        protected void SetPath(string path) { SelectedPath = Path.GetFullPath(path); file.Text = SelectedPath; }
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
