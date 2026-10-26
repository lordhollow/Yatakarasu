using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Yata.Components.Configuration
{
    internal sealed class SteamGame
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string InstallDirectory { get; set; }
        public string SteamDirectory { get; set; }
        public override string ToString() => Name + " (" + Id + ")";
        public IEnumerable<string> IconSources()
        {
            var cache = Path.Combine(SteamDirectory, "appcache", "librarycache");
            if (Directory.Exists(cache))
            {
                foreach (var file in Directory.EnumerateFiles(cache, Id + "_*").Where(x => Path.GetFileName(x).IndexOf("icon", StringComparison.OrdinalIgnoreCase) >= 0).Take(10)) yield return file;
                var gameCache = Path.Combine(cache, Id);
                if (Directory.Exists(gameCache))
                    foreach (var file in Directory.EnumerateFiles(gameCache).Where(x => new[] { ".png", ".jpg", ".ico" }.Contains(Path.GetExtension(x).ToLowerInvariant())).Take(10)) yield return file;
            }
            if (Directory.Exists(InstallDirectory))
                foreach (var file in Directory.EnumerateFiles(InstallDirectory, "*.exe").Take(20)) yield return file;
            var steamExe = Path.Combine(SteamDirectory, "steam.exe");
            if (File.Exists(steamExe)) yield return steamExe;
        }
    }

    // Valve KeyValues: quoted/unquoted tokens, nested objects, escaping and // comments.
    internal sealed class VdfObject
    {
        public Dictionary<string, string> Values { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, VdfObject> Children { get; } = new Dictionary<string, VdfObject>(StringComparer.OrdinalIgnoreCase);
        public string Value(string key) => Values.TryGetValue(key, out var value) ? value : null;
        public static VdfObject Parse(string text)
        {
            int index = 0;
            return Read(text, ref index, false, 0);
        }
        static VdfObject Read(string text, ref int index, bool nested, int depth)
        {
            if (depth > 64) throw new FormatException("VDF nesting is too deep.");
            var result = new VdfObject();
            string key;
            while ((key = Token(text, ref index)) != null)
            {
                if (key == "}") { if (!nested) throw new FormatException("Unexpected VDF brace."); return result; }
                var value = Token(text, ref index);
                if (value == "{") result.Children[key] = Read(text, ref index, true, depth + 1);
                else if (value == null || value == "}") throw new FormatException("Missing VDF value.");
                else result.Values[key] = value;
            }
            if (nested) throw new FormatException("Unclosed VDF object.");
            return result;
        }
        static string Token(string text, ref int index)
        {
            while (index < text.Length)
            {
                if (char.IsWhiteSpace(text[index]) || text[index] == '\ufeff') { index++; continue; }
                if (text[index] == '/' && index + 1 < text.Length && text[index + 1] == '/')
                { while (index < text.Length && text[index] != '\n') index++; continue; }
                break;
            }
            if (index == text.Length) return null;
            var ch = text[index++];
            if (ch == '{' || ch == '}') return ch.ToString();
            var token = new StringBuilder();
            if (ch == '"')
            {
                while (index < text.Length)
                {
                    ch = text[index++];
                    if (ch == '"') return token.ToString();
                    if (ch == '\\' && index < text.Length && (text[index] == '\\' || text[index] == '"')) ch = text[index++];
                    token.Append(ch);
                }
                throw new FormatException("Unclosed VDF string.");
            }
            token.Append(ch);
            while (index < text.Length && !char.IsWhiteSpace(text[index]) && text[index] != '{' && text[index] != '}') token.Append(text[index++]);
            return token.ToString();
        }
    }

    internal static class SteamCatalog
    {
        public static string DefaultDirectory()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
            {
                var path = key?.GetValue("SteamPath") as string;
                if (!string.IsNullOrEmpty(path)) return path;
            }
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam");
        }

        public static List<SteamGame> FindInstalled(string steamDirectory, IList<string> warnings)
        {
            var roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { steamDirectory };
            var folders = Path.Combine(steamDirectory, "steamapps", "libraryfolders.vdf");
            if (File.Exists(folders))
            {
                try
                {
                    var vdf = VdfObject.Parse(File.ReadAllText(folders));
                    if (vdf.Children.TryGetValue("libraryfolders", out var libraries))
                    {
                        foreach (var library in libraries.Children.Values)
                            if (!string.IsNullOrWhiteSpace(library.Value("path"))) roots.Add(library.Value("path"));
                        foreach (var legacy in libraries.Values)
                            if (int.TryParse(legacy.Key, out _)) roots.Add(legacy.Value);
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is FormatException) { warnings.Add(ex.Message); }
            }
            var games = new Dictionary<string, SteamGame>();
            foreach (var root in roots)
            {
                try
                {
                    var apps = Path.Combine(root, "steamapps");
                    if (!Directory.Exists(apps)) continue;
                    foreach (var manifest in Directory.EnumerateFiles(apps, "appmanifest_*.acf"))
                    {
                        try
                        {
                            var parsed = VdfObject.Parse(File.ReadAllText(manifest));
                            if (!parsed.Children.TryGetValue("AppState", out var state)) continue;
                            uint id, flags;
                            if (!uint.TryParse(state.Value("appid"), out id) || !uint.TryParse(state.Value("StateFlags"), out flags) || (flags & 4) == 0) continue;
                            var directory = Path.GetFullPath(Path.Combine(apps, "common", state.Value("installdir") ?? ""));
                            var common = Path.GetFullPath(Path.Combine(apps, "common")) + Path.DirectorySeparatorChar;
                            if (!directory.StartsWith(common, StringComparison.OrdinalIgnoreCase) || !Directory.Exists(directory)) continue;
                            games[id.ToString()] = new SteamGame { Id = id.ToString(), Name = state.Value("name") ?? id.ToString(), InstallDirectory = directory, SteamDirectory = steamDirectory };
                        }
                        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is FormatException || ex is ArgumentException) { warnings.Add(Path.GetFileName(manifest) + ": " + ex.Message); }
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { warnings.Add(ex.Message); }
            }
            return games.Values.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        }
    }
}
