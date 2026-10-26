using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Yata.Components.Configuration;

namespace Yata.Settings
{
    internal sealed partial class SteamPickerForm : Form
    {
        List<SteamGame> installed = new List<SteamGame>();
        int revision;
        public SteamGame SelectedGame { get; private set; }
        public SteamPickerForm() { InitializeComponent(); }
        void Picker_Shown(object sender, EventArgs e)
        {
            if (DesignMode || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime) return;
            root.Text = SteamCatalog.DefaultDirectory();
            Reload();
        }
        void Search_TextChanged(object sender, EventArgs e) => Filter();
        void Games_DoubleClick(object sender, EventArgs e) => selectButton.PerformClick();
        void SelectButton_Click(object sender, EventArgs e)
        {
            if (games.SelectedItem is SteamGame game) { SelectedGame = game; DialogResult = DialogResult.OK; }
        }
        void CancelButton_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;
        void ReloadButton_Click(object sender, EventArgs e) => Reload();
        void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog { SelectedPath = root.Text })
                if (dialog.ShowDialog(this) == DialogResult.OK) { root.Text = dialog.SelectedPath; Reload(); }
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
