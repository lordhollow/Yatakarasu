using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Yata.Components.Configuration;

namespace Yata.Settings
{
    internal sealed class IconPickerForm : Form
    {
        readonly List<IconCandidate> candidates = new List<IconCandidate>();
        readonly ImageList images = new ImageList { ImageSize = new Size(48, 48), ColorDepth = ColorDepth.Depth32Bit };
        readonly ListView list = new ListView { Dock = DockStyle.Fill, View = View.LargeIcon, MultiSelect = false, HideSelection = false, ShowItemToolTips = true };
        readonly Label status = EditorUi.Label("Exe・DLL内のアイコン、ico・png・jpg画像から選べます。");
        public Bitmap SelectedImage { get; private set; }
        public IconPickerForm(IEnumerable<string> sources)
        {
            Text = "アイコンを選択"; Size = new Size(760, 540); MinimumSize = new Size(540, 360); StartPosition = FormStartPosition.CenterParent; Font = SystemFonts.MessageBoxFont;
            list.LargeImageList = images;
            var select = EditorUi.Button("このアイコンを使う", (s, e) =>
            {
                if (list.SelectedIndices.Count == 0) return;
                SelectedImage = new Bitmap(candidates[list.SelectedIndices[0]].Image); DialogResult = DialogResult.OK;
            });
            list.DoubleClick += (s, e) => select.PerformClick();
            var cancel = EditorUi.Button("キャンセル", (s, e) => { DialogResult = DialogResult.Cancel; });
            Controls.Add(list);
            Controls.Add(EditorUi.Toolbar(EditorUi.Button("ファイルを追加…", (s, e) =>
            {
                using (var dialog = new OpenFileDialog { Filter = "アイコン・画像・アプリ|*.exe;*.dll;*.ico;*.png;*.jpg;*.jpeg;*.bmp|すべてのファイル|*.*" })
                    if (dialog.ShowDialog(this) == DialogResult.OK) AddSource(dialog.FileName);
            }), status));
            var footer = EditorUi.Toolbar(select, cancel); footer.Dock = DockStyle.Bottom; Controls.Add(footer);
            AcceptButton = select; CancelButton = cancel;
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
        protected override void Dispose(bool disposing)
        {
            if (disposing) { images.Dispose(); foreach (var candidate in candidates) candidate.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
