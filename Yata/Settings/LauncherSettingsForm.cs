using System;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Yata.Components.Configuration;

namespace Yata.Settings
{
    internal sealed partial class LauncherSettingsForm : ConfigurationEditorForm
    {
        LauncherDefinition definition;
        BindingList<LauncherEntry> entries;
        public LauncherSettingsForm() { InitializeComponent(); }
        public LauncherSettingsForm(string path) : this()
        {
            SetPath(path);
            InitializeDefinition();
        }
        void AddButton_Click(object sender, EventArgs e) => Add();
        void EditButton_Click(object sender, EventArgs e) => Edit();
        void DuplicateButton_Click(object sender, EventArgs e) => Duplicate();
        void DeleteButton_Click(object sender, EventArgs e) => Delete();
        void AddPageButton_Click(object sender, EventArgs e) => AddPage();
        void Grid_DoubleClick(object sender, EventArgs e) => Edit();

        protected override void LoadDefinition(string path)
        {
            var loaded = LauncherDefinition.Load(path);
            definition = loaded;
            entries = new BindingList<LauncherEntry>(loaded.Entries.Select(x => x.Clone()).ToList());
            foreach (var entry in entries)
                if (!string.IsNullOrWhiteSpace(entry.IconPath) && !entry.IconPath.Contains("::") && !Path.IsPathRooted(entry.IconPath))
                    entry.IconPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path)), entry.IconPath));
            // AutoGenerateColumns is hidden from designer serialization, so set
            // it at the data-binding boundary as well as defining the UI columns.
            grid.AutoGenerateColumns = false;
            gridPageColumn.DataPropertyName = "Page";
            gridPageRowColumn.DataPropertyName = "PageRow";
            gridColumnColumn.DataPropertyName = "Column";
            gridCaptionColumn.DataPropertyName = "Caption";
            gridCommandColumn.DataPropertyName = "Command";
            gridOptionColumn.DataPropertyName = "Option";
            grid.DataSource = entries; UpdateStatus();
        }
        protected override void SaveDefinition(string path)
        {
            var result = new LauncherDefinition { Key = definition.Key };
            result.Comments.AddRange(definition.Comments);
            result.Entries.AddRange(entries.Select(x => x.Clone()));
            result.Save(path);
        }
        LauncherEntry Current => grid.CurrentRow?.DataBoundItem as LauncherEntry;
        LauncherEntry FreePosition()
        {
            var used = entries.Select(x => x.Row * 16 + x.Column).ToHashSetCompat();
            var index = Enumerable.Range(0, 32000).FirstOrDefault(x => !used.Contains(x));
            if (used.Contains(index)) throw new InvalidOperationException("配置できる空きがありません。");
            return new LauncherEntry { Row = index / 16, Column = index % 16 };
        }
        void Add() { try { EditEntry(FreePosition(), null); } catch (Exception ex) { EditorUi.Error(this, ex); } }
        void Edit() { if (Current != null) EditEntry(Current.Clone(), Current); }
        void Duplicate()
        {
            if (Current == null) return;
            try { var copy = Current.Clone(); var position = FreePosition(); copy.Row = position.Row; copy.Column = position.Column; EditEntry(copy, null); }
            catch (Exception ex) { EditorUi.Error(this, ex); }
        }
        void EditEntry(LauncherEntry entry, LauncherEntry original)
        {
            using (var dialog = new LauncherEntryForm(entry, SelectedPath, candidate => !entries.Any(x => !ReferenceEquals(x, original) && x.Row == candidate.Row && x.Column == candidate.Column)))
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var updated = dialog.Entry;
                    if (entries.Any(x => !ReferenceEquals(x, original) && x.Row == updated.Row && x.Column == updated.Column))
                    { MessageBox.Show(this, "その位置には既に項目があります。配置を変更してください。"); return; }
                    if (original == null) entries.Add(updated); else entries[entries.IndexOf(original)] = updated;
                    Dirty = true; UpdateStatus();
                }
        }
        void Delete()
        {
            if (Current == null) return;
            if (MessageBox.Show(this, Current.Caption + "を一覧から削除しますか？", "項目の削除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            entries.Remove(Current); Dirty = true; UpdateStatus();
        }
        void AddPage()
        {
            var row = entries.Count == 0 ? 0 : (entries.Max(x => x.Row) / 2 + 1) * 2;
            if (row >= 2000) { MessageBox.Show(this, "1000ページまで追加できます。"); return; }
            entries.Add(new LauncherEntry { Row = row + 1, Column = 14, Caption = "前のページ", Command = "internal::LauncherPreviousPage" });
            entries.Add(new LauncherEntry { Row = row + 1, Column = 15, Caption = "次のページ", Command = "internal::LauncherNextPage" });
            Dirty = true; UpdateStatus();
        }
        void UpdateStatus() => status.Text = $"{entries.Count}項目 / {(entries.Count == 0 ? 1 : entries.Max(x => x.Row) / 2 + 1)}ページ。1ページ = 2行 × 16列。";
    }

    internal static class EnumerableCompatibility
    {
        public static System.Collections.Generic.HashSet<T> ToHashSetCompat<T>(this System.Collections.Generic.IEnumerable<T> values) => new System.Collections.Generic.HashSet<T>(values);
    }
}
