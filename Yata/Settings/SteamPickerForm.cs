using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Yata.Components.Configuration;

namespace Yata.Settings
{
    internal sealed class SteamPickerForm : Form
    {
        readonly TextBox root = new TextBox { Width = 420 };
        readonly TextBox search = new TextBox { Width = 420 };
        readonly ListBox games = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false, HorizontalScrollbar = true };
        readonly Label status = EditorUi.Label("Steamライブラリを検索しています…");
        List<SteamGame> installed = new List<SteamGame>();
        int revision;
        public SteamGame SelectedGame { get; private set; }
        public SteamPickerForm()
        {
            Text = "インストール済みSteamソフトを選択"; Size = new Size(880, 560); MinimumSize = new Size(650, 420); Font = SystemFonts.MessageBoxFont; StartPosition = FormStartPosition.CenterParent;
            root.Text = SteamCatalog.DefaultDirectory();
            search.TextChanged += (s, e) => Filter();
            var select = EditorUi.Button("選択", (s, e) => { if (games.SelectedItem is SteamGame game) { SelectedGame = game; DialogResult = DialogResult.OK; } });
            games.DoubleClick += (s, e) => select.PerformClick();
            var cancel = EditorUi.Button("キャンセル", (s, e) => { DialogResult = DialogResult.Cancel; });
            Controls.Add(games);
            Controls.Add(EditorUi.Toolbar(EditorUi.Label("検索"), search, status));
            Controls.Add(EditorUi.Toolbar(EditorUi.Label("Steamフォルダ"), root, EditorUi.Button("参照…", (s, e) =>
            {
                using (var dialog = new FolderBrowserDialog { SelectedPath = root.Text })
                    if (dialog.ShowDialog(this) == DialogResult.OK) { root.Text = dialog.SelectedPath; Reload(); }
            }), EditorUi.Button("再検索", (s, e) => Reload())));
            var footer = EditorUi.Toolbar(select, cancel); footer.Dock = DockStyle.Bottom; Controls.Add(footer);
            AcceptButton = select; CancelButton = cancel;
            Shown += (s, e) => Reload();
        }
        async void Reload()
        {
            int request = ++revision;
            var directory = root.Text;
            status.Text = "検索中…";
            try
            {
                var warnings = new List<string>();
                var found = await Task.Run(() => SteamCatalog.FindInstalled(directory, warnings));
                if (IsDisposed || request != revision) return;
                installed = found; Filter();
                status.Text = $"{found.Count}件" + (warnings.Count > 0 ? $"（読み取り不可 {warnings.Count}件）" : "");
            }
            catch (Exception ex) { if (!IsDisposed && request == revision) status.Text = ex.Message; }
        }
        void Filter() => games.DataSource = installed.Where(x => x.ToString().IndexOf(search.Text, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
    }
}
