using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Yata.Components.Configuration;

namespace Yata.Settings
{
    internal sealed class LauncherEntryForm : Form
    {
        readonly TextBox caption = new TextBox { Dock = DockStyle.Fill };
        readonly TextBox command = new TextBox { Dock = DockStyle.Fill };
        readonly ComboBox option = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown };
        readonly TextBox icon = new TextBox { Dock = DockStyle.Fill };
        readonly NumericUpDown page = new NumericUpDown { Minimum = 1, Maximum = 1000, Width = 75 };
        readonly NumericUpDown row = new NumericUpDown { Minimum = 0, Maximum = 1, Width = 60 };
        readonly NumericUpDown column = new NumericUpDown { Minimum = 0, Maximum = 15, Width = 60 };
        readonly ComboBox system = new ComboBox { Width = 310, DropDownStyle = ComboBoxStyle.DropDownList };
        readonly Label hint = new Label { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6), ForeColor = Color.DimGray };
        readonly PictureBox preview = new PictureBox { Size = new Size(80, 80), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };
        readonly string definitionPath;
        readonly List<string> iconSources = new List<string>();
        Bitmap selectedIcon;
        public LauncherEntry Entry { get; private set; }

        public LauncherEntryForm(LauncherEntry entry, string path)
        {
            definitionPath = path;
            Text = "ランチャー項目の編集"; Size = new Size(900, 540); MinimumSize = new Size(760, 500); StartPosition = FormStartPosition.CenterParent; Font = SystemFonts.MessageBoxFont;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 9, Padding = new Padding(12) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 8; i++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(EditorUi.Label("追加元"), 0, 0);
            layout.Controls.Add(EditorUi.Toolbar(EditorUi.Button("Exeを選択…", (s, e) => ChooseExe()), EditorUi.Button("Steamから…", (s, e) => ChooseSteam())), 1, 0);
            system.DataSource = new[] { new SystemApplicationChoice { Id = "", Caption = "（システムアプリから選択）" } }.Concat(SystemApplicationCatalog.All).ToList();
            system.SelectedIndexChanged += (s, e) =>
            {
                if (!(system.SelectedItem is SystemApplicationChoice choice) || string.IsNullOrEmpty(choice.Id)) return;
                command.Text = "internal::" + choice.Id; caption.Text = choice.Caption; hint.Text = choice.Hint;
                option.Items.Clear();
                if (choice.Id == "DashboardBrightnessChanger") option.Items.AddRange(new object[] { "1", "0.75", "0.5", "0.25", "0" });
                if (choice.Id == "SetDisplayTopology") option.Items.AddRange(new object[] { "EXTEND", "INTERNAL", "EXTERNAL", "CLONE" });
                if (choice.Id == "SetDisplayInput") option.Items.AddRange(new object[] { "TOGGLE", "DP1", "DP2", "HDMI1", "HDMI2" });
                option.Text = option.Items.Count == 0 ? "" : option.Items[0].ToString();
            };
            layout.Controls.Add(EditorUi.Label("システムアプリ"), 0, 1); layout.Controls.Add(system, 1, 1);
            layout.Controls.Add(EditorUi.Label("配置"), 0, 2); layout.Controls.Add(EditorUi.Toolbar(EditorUi.Label("ページ"), page, EditorUi.Label("行 (0–1)"), row, EditorUi.Label("列 (0–15)"), column), 1, 2);
            layout.Controls.Add(EditorUi.Label("表示名"), 0, 3); layout.Controls.Add(caption, 1, 3);
            layout.Controls.Add(EditorUi.Label("コマンド"), 0, 4); layout.Controls.Add(command, 1, 4);
            layout.Controls.Add(EditorUi.Label("起動オプション"), 0, 5); layout.Controls.Add(option, 1, 5);
            layout.Controls.Add(EditorUi.Label("アイコン"), 0, 6); layout.Controls.Add(icon, 1, 6);
            layout.Controls.Add(preview, 0, 7); layout.Controls.Add(EditorUi.Toolbar(EditorUi.Button("アイコンを選択…", (s, e) => ChooseIcon()), EditorUi.Button("標準に戻す", (s, e) => { selectedIcon?.Dispose(); selectedIcon = null; icon.Text = ""; SetPreview(null); })), 1, 7);
            layout.Controls.Add(hint, 1, 8);
            var ok = EditorUi.Button("項目を確定", (s, e) => SaveEntry());
            var cancel = EditorUi.Button("キャンセル", (s, e) => { DialogResult = DialogResult.Cancel; });
            var footer = EditorUi.Toolbar(ok, cancel); footer.Dock = DockStyle.Bottom;
            Controls.Add(layout); Controls.Add(footer); AcceptButton = ok; CancelButton = cancel;
            system.SelectedItem = ((List<SystemApplicationChoice>)system.DataSource).FirstOrDefault(x => "internal::" + x.Id == entry.Command) ?? ((List<SystemApplicationChoice>)system.DataSource)[0];
            caption.Text = entry.Caption; command.Text = entry.Command; option.Text = entry.Option; icon.Text = entry.IconPath;
            page.Value = entry.Page; row.Value = entry.PageRow; column.Value = entry.Column;
            hint.Text = "1. アプリを選択 → 2. アイコンを選択 → 3. 起動オプションを入力。Exe/DLL内の複数アイコンにも対応します。";
            if (File.Exists(entry.Command)) iconSources.Add(entry.Command);
            TryPreview(entry.IconPath);
        }
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
                if (entry.Command.StartsWith("internal::") && !SystemApplicationCatalog.All.Any(x => "internal::" + x.Id == entry.Command) && entry.Command != "internal::SetDisplayToporogy") throw new FormatException("システムアプリを一覧から選択してください。");
                if (selectedIcon != null) entry.IconPath = IconLibrary.Save(selectedIcon, definitionPath);
                Entry = entry; DialogResult = DialogResult.OK;
            }
            catch (Exception ex) { EditorUi.Error(this, ex); }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { selectedIcon?.Dispose(); preview.Image?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
