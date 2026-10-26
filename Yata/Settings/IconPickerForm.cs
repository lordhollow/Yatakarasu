using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Yata.Components.Configuration;

namespace Yata.Settings
{
    internal sealed partial class IconPickerForm : Form
    {
        readonly List<IconCandidate> candidates = new List<IconCandidate>();
        public Bitmap SelectedImage { get; private set; }
        public IconPickerForm() { InitializeComponent(); }
        public IconPickerForm(IEnumerable<string> sources) : this()
        {
            foreach (var source in sources) AddSource(source);
            if (candidates.Count == 0)
                AddSource(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files", "SystemApp.png"));
            if (candidates.Count == 0)
            {
                var candidate = new IconCandidate { Name = "アプリケーション", Image = SystemIcons.Application.ToBitmap() };
                candidates.Add(candidate); images.Images.Add(candidate.Image);
                list.Items.Add(new ListViewItem(candidate.Name, 0) { Selected = true });
            }
        }
        void AddSource(string path)
        {
            try
            {
                List<IconCandidate> loaded;
                if (string.Equals(Path.GetFileName(path), "SystemApp.png", StringComparison.OrdinalIgnoreCase) && File.Exists(path))
                {
                    loaded = new List<IconCandidate>();
                    using (var atlas = new Bitmap(path))
                        for (int y = 0; y < atlas.Height / 48; y++)
                            for (int x = 0; x < atlas.Width / 48; x++)
                                loaded.Add(new IconCandidate { Name = $"システム {x},{y}", Image = atlas.Clone(new Rectangle(x * 48, y * 48, 48, 48), atlas.PixelFormat) });
                }
                else loaded = IconLibrary.Load(path);
                foreach (var candidate in loaded)
                {
                    var index = candidates.Count;
                    candidates.Add(candidate); images.Images.Add(candidate.Image);
                    list.Items.Add(new ListViewItem(candidate.Name, index) { ToolTipText = path + " / " + candidate.Name });
                }
                status.Text = $"{candidates.Count}個のアイコン。Exe/DLLは先頭256個まで表示します。";
                if (list.Items.Count > 0 && list.SelectedItems.Count == 0) list.Items[0].Selected = true;
            }
            catch (Exception ex) when (ex is IOException || ex is ArgumentException || ex is UnauthorizedAccessException || ex is OutOfMemoryException) { status.Text = "読み込めません: " + Path.GetFileName(path); }
        }
        void SelectButton_Click(object sender, EventArgs e)
        {
            if (list.SelectedIndices.Count == 0) return;
            SelectedImage = new Bitmap(candidates[list.SelectedIndices[0]].Image); DialogResult = DialogResult.OK;
        }
        void List_DoubleClick(object sender, EventArgs e) => selectButton.PerformClick();
        void CancelButton_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;
        void AddFileButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "アイコン・画像・アプリ|*.exe;*.dll;*.ico;*.png;*.jpg;*.jpeg;*.bmp|すべてのファイル|*.*" })
                if (dialog.ShowDialog(this) == DialogResult.OK) AddSource(dialog.FileName);
        }
        void DisposeCandidates() { foreach (var candidate in candidates) candidate.Dispose(); }
    }
}
