using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Yata.Components.Configuration
{
    internal sealed class LauncherEntry
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int Page { get => Row / 2 + 1; set => Row = (value - 1) * 2 + Row % 2; }
        public int PageRow { get => Row % 2; set => Row = (Page - 1) * 2 + value; }
        public string Caption { get; set; } = "";
        public string Command { get; set; } = "";
        public string IconPath { get; set; } = "";
        public string Option { get; set; } = "";
        public LauncherEntry Clone() => (LauncherEntry)MemberwiseClone();
    }

    internal sealed class LauncherDefinition
    {
        public string Key { get; set; } = Guid.NewGuid().ToString("N");
        public List<LauncherEntry> Entries { get; } = new List<LauncherEntry>();
        public List<string> Comments { get; } = new List<string>();
        public static LauncherDefinition Load(string path)
        {
            var result = new LauncherDefinition();
            if (!File.Exists(path)) return result;
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0])) throw new FormatException("ランチャー定義の先頭にキーが必要です。");
            result.Key = lines[0];
            for (var i = 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split('\t');
                int row, column;
                if (!int.TryParse(parts[0], out row)) { result.Comments.Add(lines[i]); continue; }
                if (parts.Length < 4 || !int.TryParse(parts[1], out column)) throw new FormatException($"{i + 1}行目: 行・列・表示名・コマンドを確認してください。");
                result.Entries.Add(new LauncherEntry { Row = row, Column = column, Caption = parts[2], Command = parts[3], IconPath = parts.Length > 4 ? parts[4] : "", Option = parts.Length > 5 ? parts[5] : "" });
            }
            result.Validate();
            return result;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConfigurationFile.Field(Key, "キー"))) throw new FormatException("キーは空にできません。");
            var positions = new HashSet<int>();
            foreach (var entry in Entries)
            {
                if (entry.Row < 0 || entry.Row >= 2000 || entry.Column < 0 || entry.Column >= 16) throw new FormatException("ページは1～1000、行は0～1、列は0～15で指定してください。");
                if (!positions.Add(entry.Row * 16 + entry.Column)) throw new FormatException($"ページ{entry.Page}、行{entry.PageRow}、列{entry.Column}が重複しています。");
                if (string.IsNullOrWhiteSpace(ConfigurationFile.Field(entry.Caption, "表示名")) || string.IsNullOrWhiteSpace(ConfigurationFile.Field(entry.Command, "コマンド"))) throw new FormatException("表示名とコマンドを入力してください。");
                ConfigurationFile.Field(entry.IconPath, "アイコン");
                ConfigurationFile.Field(entry.Option, "起動オプション");
            }
        }

        public void Save(string path)
        {
            Validate();
            var text = new StringBuilder().AppendLine(Key);
            foreach (var comment in Comments) text.AppendLine(comment);
            foreach (var entry in Entries.OrderBy(x => x.Row).ThenBy(x => x.Column))
                text.AppendLine(string.Join("\t", entry.Row.ToString(CultureInfo.InvariantCulture), entry.Column.ToString(CultureInfo.InvariantCulture), entry.Caption, entry.Command, entry.IconPath, entry.Option));
            ConfigurationFile.Save(path, text.ToString());
        }
    }
}
