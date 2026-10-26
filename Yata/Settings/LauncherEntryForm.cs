using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Yata.Components.Configuration;

namespace Yata.Settings
{
    internal sealed partial class LauncherEntryForm : Form
    {
        readonly string definitionPath;
        readonly Func<LauncherEntry, bool> placementAvailable;
        readonly List<string> iconSources = new List<string>();
        Bitmap selectedIcon;
        public LauncherEntry Entry { get; private set; }

        public LauncherEntryForm() { InitializeComponent(); }
        public LauncherEntryForm(LauncherEntry entry, string path, Func<LauncherEntry, bool> placementAvailable = null) : this()
        {
            definitionPath = path;
            this.placementAvailable = placementAvailable;
            system.DataSource = new[] { new SystemApplicationChoice { Id = "", Caption = "（システムアプリから選択）" } }.Concat(SystemApplicationCatalog.All).ToList();
            system.SelectedItem = ((List<SystemApplicationChoice>)system.DataSource).FirstOrDefault(x => "internal::" + x.Id == entry.Command) ?? ((List<SystemApplicationChoice>)system.DataSource)[0];
            caption.Text = entry.Caption; command.Text = entry.Command; option.Text = entry.Option; icon.Text = entry.IconPath;
            page.Value = entry.Page; row.Value = entry.PageRow; column.Value = entry.Column;
            if (File.Exists(entry.Command)) iconSources.Add(entry.Command);
            TryPreview(entry.IconPath);
        }

        void System_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(system.SelectedItem is SystemApplicationChoice choice) || string.IsNullOrEmpty(choice.Id)) return;
            command.Text = "internal::" + choice.Id; caption.Text = choice.Caption; hint.Text = choice.Hint;
            option.Items.Clear();
            if (choice.Id == "DashboardBrightnessChanger") option.Items.AddRange(new object[] { "1", "0.75", "0.5", "0.25", "0" });
            if (choice.Id == "SetDisplayTopology") option.Items.AddRange(new object[] { "EXTEND", "INTERNAL", "EXTERNAL", "CLONE" });
            if (choice.Id == "SetDisplayInput") option.Items.AddRange(new object[] { "TOGGLE", "DP1", "DP2", "HDMI1", "HDMI2" });
            option.Text = option.Items.Count == 0 ? "" : option.Items[0].ToString();
        }
        void ChooseExeButton_Click(object sender, EventArgs e) => ChooseExe();
        void ChooseSteamButton_Click(object sender, EventArgs e) => ChooseSteam();
        void ChooseIconButton_Click(object sender, EventArgs e) => ChooseIcon();
        void ResetIconButton_Click(object sender, EventArgs e)
        {
            selectedIcon?.Dispose(); selectedIcon = null; icon.Text = ""; SetPreview(null);
        }
        void OkButton_Click(object sender, EventArgs e) => SaveEntry();
        void CancelButton_Click(object sender, EventArgs e) => DialogResult = DialogResult.Cancel;

        void ChooseExe()
        {
            using (var dialog = new OpenFileDialog { Filter = "実行ファイル|*.exe|すべてのファイル|*.*" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                command.Text = dialog.FileName; caption.Text = Path.GetFileNameWithoutExtension(dialog.FileName); option.Items.Clear(); option.Text = "";
                system.SelectedIndex = 0; iconSources.Clear(); iconSources.Add(dialog.FileName);
                ChooseIcon();
            }
        }
        void ChooseSteam()
        {
            using (var picker = new SteamPickerForm())
            {
                if (picker.ShowDialog(this) != DialogResult.OK) return;
                var game = picker.SelectedGame;
                var steamExe = Path.Combine(game.SteamDirectory, "steam.exe");
                if (!File.Exists(steamExe)) { MessageBox.Show(this, "Steam本体（steam.exe）のあるフォルダを指定してください。"); return; }
                command.Text = steamExe; option.Items.Clear(); option.Text = "-applaunch " + game.Id; caption.Text = game.Name; system.SelectedIndex = 0;
                iconSources.Clear();
                try { iconSources.AddRange(game.IconSources()); }
                catch (Exception ex) { hint.Text = ex.Message; }
                hint.Text = "-applaunch のAppIDは維持し、その後にゲームの起動オプションを追加してください。";
                ChooseIcon();
            }
        }
        void ChooseIcon()
        {
            var sources = iconSources.ToList();
            if (File.Exists(command.Text) && !sources.Contains(command.Text)) sources.Add(command.Text);
            using (var picker = new IconPickerForm(sources))
                if (picker.ShowDialog(this) == DialogResult.OK)
                {
                    selectedIcon?.Dispose(); selectedIcon = picker.SelectedImage;
                    icon.Text = "（選択したアイコンを保存します）"; SetPreview(selectedIcon);
                }
        }
        void TryPreview(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                if (path.StartsWith("InternalApplicationIconsImage::"))
                {
                    var parts = path.Substring("InternalApplicationIconsImage::".Length).Split(',');
                    using (var bitmap = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files", "SystemApp.png")))
                    using (var cropped = bitmap.Clone(new Rectangle(int.Parse(parts[0]) * 48, int.Parse(parts[1]) * 48, 48, 48), bitmap.PixelFormat)) SetPreview(cropped);
                }
                else
                {
                    if (!Path.IsPathRooted(path)) path = Path.Combine(Path.GetDirectoryName(definitionPath), path);
                    using (var image = Image.FromFile(path)) SetPreview(image);
                }
            }
            catch (Exception) { }
        }
        void SetPreview(Image image) { var previous = preview.Image; preview.Image = image == null ? null : new Bitmap(image); previous?.Dispose(); }
        void SaveEntry()
        {
            try
            {
                var entry = new LauncherEntry { Row = ((int)page.Value - 1) * 2 + (int)row.Value, Column = (int)column.Value, Caption = caption.Text, Command = command.Text, Option = option.Text, IconPath = selectedIcon == null ? icon.Text : "" };
                var check = new LauncherDefinition(); check.Entries.Add(entry); check.Validate();
                if (placementAvailable != null && !placementAvailable(entry)) throw new FormatException("その位置には既に項目があります。ページ・行・列を変更してください。");
                if (entry.Command.StartsWith("internal::") && !SystemApplicationCatalog.All.Any(x => "internal::" + x.Id == entry.Command) && entry.Command != "internal::SetDisplayToporogy") throw new FormatException("システムアプリを一覧から選択してください。");
                if (selectedIcon != null) entry.IconPath = IconLibrary.Save(selectedIcon, definitionPath);
                Entry = entry; DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { EditorUi.Error(this, ex); }
        }
        void DisposeImages() { selectedIcon?.Dispose(); preview.Image?.Dispose(); }
    }
}
