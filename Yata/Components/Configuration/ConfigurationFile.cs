using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Yata.Components.Configuration
{
    internal static class ConfigurationFile
    {
        public static void Save(string path, string text)
        {
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temporary, text, new UTF8Encoding(false));
                if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
                else File.Move(temporary, path);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        public static string Field(string value, string name)
        {
            value = value ?? "";
            if (value.IndexOfAny(new[] { '\t', '\r', '\n' }) >= 0)
                throw new FormatException(name + "にタブや改行は使用できません。");
            return value;
        }
    }

    internal sealed class ConfigurationPaths
    {
        public string SensorFile { get; set; }
        public string LauncherFile { get; set; }
        static string PreferencesFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Yatakarasu", "configuration.xml");

        public static ConfigurationPaths Load()
        {
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files");
            var paths = new ConfigurationPaths { SensorFile = Path.Combine(root, "sensor.txt"), LauncherFile = Path.Combine(root, "karasu.exec.txt") };
            try
            {
                if (File.Exists(PreferencesFile))
                {
                    var xml = XElement.Load(PreferencesFile);
                    paths.SensorFile = Existing((string)xml.Element("SensorFile"), paths.SensorFile);
                    paths.LauncherFile = Existing((string)xml.Element("LauncherFile"), paths.LauncherFile);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is System.Xml.XmlException || ex is UnauthorizedAccessException || ex is ArgumentException)
            { System.Diagnostics.Trace.TraceWarning(ex.ToString()); }
            return paths;
        }

        static string Existing(string selected, string fallback) => !string.IsNullOrWhiteSpace(selected) && File.Exists(selected) ? Path.GetFullPath(selected) : fallback;
        public void Save() => ConfigurationFile.Save(PreferencesFile, new XElement("Configuration", new XElement("SensorFile", SensorFile), new XElement("LauncherFile", LauncherFile)).ToString());
    }
}
