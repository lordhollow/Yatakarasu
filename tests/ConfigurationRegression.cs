using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Yata.Components;
using Yata.Components.Configuration;
using Yata.Components.Widget;
using Yata.Settings;
using Yata;
using ISensor = LibreHardwareMonitor.Hardware.ISensor;
using IHardware = LibreHardwareMonitor.Hardware.IHardware;

static class ConfigurationRegression
{
    static string root;
    static int passed;
    static readonly List<string> images = new List<string>();
    [STAThread]
    static int Main()
    {
        try
        {
            Application.EnableVisualStyles();
            root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fixtures", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var assets = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files");
            Directory.CreateDirectory(assets);
            using (var icon = SystemIcons.Application.ToBitmap()) icon.Save(Path.Combine(assets, "SystemApp.png"), System.Drawing.Imaging.ImageFormat.Png);
            File.Copy(Path.Combine(assets, "SystemApp.png"), Path.Combine(root, "icon.png"));
            Run("Sensor round trip with six slots, two plots and invariant numbers", SensorRoundTrip);
            Run("Invalid sensor data cannot overwrite an existing file", SensorValidation);
            Run("Strict parser reports malformed sensor lines", SensorParsing);
            Run("Launcher preserves keys, comments, options and page positions", LauncherRoundTrip);
            Run("Launcher rejects duplicate and out-of-range positions", LauncherValidation);
            Run("Atomic saves keep a backup", Backups);
            Run("Selected configuration files persist with a safe fallback", SelectedFiles);
            Run("Pages wrap and reload clamps the current page", Pages);
            Run("Steam discovers modern and legacy library folders", SteamLibraries);
            Run("VDF parser rejects truncated files and handles escaping", VdfParsing);
            Run("Executable icons can be selected and persisted", Icons);
            Run("Karasu resolves the selected file and exact slot without launching", ResolvePosition);
            Run("Karasu handles fragmented UTF-8 and oversized messages", Protocol);
            Run("Configuration screens load, save and render", Forms);
            Run("Settings forms load in the WinForms designer without runtime data", DesignerForms);
            Run("Designer round trips retain editor content and grid columns", DesignerRoundTrips);
            Run("Design-time grids stay unbound until runtime definitions load", DesignerBindingLifecycle);
            Run("Replacing widgets preserves unrelated cached drawings", WidgetReplacement);
            Run("Premultiplied layers preserve transparent composition", PremultipliedLayers);
            Run("Window drawing buffers retain full-size frames without shrinking", DrawingBufferCapacity);
            Run("Loaded images preserve composition and release source files", DrawingImages);
            Run("Launcher focus fades and clears for every image source", LauncherFocus);
            Run("Page fault recording marks concurrency and waits for active scopes", PageFaultRecording);
            Run("NVIDIA optional queries restore selected sensors and aggregates", NvidiaQueries);
            Console.WriteLine("All " + passed + " configuration checks passed.");
            foreach (var image in images) Console.WriteLine("PREVIEW: " + image);
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        finally { Application.Exit(); }
    }
    static void Run(string name, Action action) { action(); passed++; Console.WriteLine("PASS: " + name); }
    static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    static void Reject(Action action)
    {
        try { action(); }
        catch (FormatException) { return; }
        throw new Exception("Invalid configuration was accepted.");
    }
    static SensorDefine Sensors()
    {
        var definition = new SensorDefine(); definition.EnsureFixedLayout();
        definition.PlotDefines[0].Name = "CPU / GPU";
        definition.PlotDefines[0].Items.Add(new PlotItemDefine { Name = "CPU", Id = "/amdcpu/0/load/1-2", Color = Color.FromArgb(180, 250, 80, 20) });
        definition.MeterDefines[0].Name = "CPU Fan"; definition.MeterDefines[0].Id = "/lpc/test/fan/1"; definition.MeterDefines[0].Unit = "rpm"; definition.MeterDefines[0].Max = 1800.5f;
        definition.MeterDefines[2].Name = "GPU Fan"; definition.MeterDefines[2].Id = "/gpu/0/fan/0";
        return definition;
    }
    static LauncherDefinition Launcher()
    {
        var definition = new LauncherDefinition { Key = "test-key" };
        definition.Comments.Add("# 保持するコメント");
        definition.Entries.Add(new LauncherEntry { Caption = "同じ名前", Command = "first.exe", Option = "--first \"日本語 file\"", Row = 0, Column = 0, IconPath = "icon.png" });
        definition.Entries.Add(new LauncherEntry { Caption = "同じ名前", Command = "second.exe", Option = "--second", Row = 2, Column = 0 });
        definition.Entries.Add(new LauncherEntry { Caption = "次のページ", Command = "internal::LauncherNextPage", Row = 1, Column = 15 });
        definition.Entries.Add(new LauncherEntry { Caption = "前のページ", Command = "internal::LauncherPreviousPage", Row = 3, Column = 14 });
        return definition;
    }
    static void SensorRoundTrip()
    {
        var path = Path.Combine(root, "sensor.txt");
        var culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Sensors().Save(path);
            var restored = new SensorDefine(); restored.ReadFromFile(path, true);
            Check(restored.PlotDefines.Count == 2 && restored.MeterDefines.Count == 6, "Fixed counts changed.");
            Check(restored.MeterDefines[1].Id == "-" && restored.MeterDefines[2].Id == "/gpu/0/fan/0", "An empty slot shifted subsequent meters.");
            Check(restored.MeterDefines[0].Max == 1800.5f, "Culture changed a number.");
            Check(restored.PlotDefines[0].Items[0].Color.A == 180, "Color alpha was lost.");
        }
        finally { CultureInfo.CurrentCulture = culture; }
    }
    static void SensorValidation()
    {
        var path = Path.Combine(root, "sensor.txt"); var before = File.ReadAllText(path);
        var definition = Sensors(); definition.MeterDefines[0].Min = definition.MeterDefines[0].Max;
        Reject(() => definition.Save(path));
        Check(before == File.ReadAllText(path), "Invalid data overwrote the file.");
        definition = Sensors(); definition.PlotDefines[0].Items[0].Name = "bad\tfield";
        Reject(() => definition.Save(path));
    }
    static void SensorParsing()
    {
        var path = Path.Combine(root, "invalid-sensor.txt"); File.WriteAllText(path, "# test\nnot-a-setting\n");
        try { new SensorDefine().ReadFromFile(path, true); }
        catch (FormatException ex) { Check(ex.Message.Contains("2行目"), "Line number is missing."); return; }
        throw new Exception("Malformed sensor configuration was ignored.");
    }
    static void LauncherRoundTrip()
    {
        var path = Path.Combine(root, "karasu.exec.txt"); Launcher().Save(path);
        var loaded = LauncherDefinition.Load(path);
        Check(loaded.Key == "test-key" && loaded.Comments.Contains("# 保持するコメント"), "Key or comments changed.");
        Check(loaded.Entries.Count == 4 && loaded.Entries[0].Option.Contains("日本語"), "Options were lost.");
        Check(loaded.Entries.Single(x => x.Command == "second.exe").Page == 2, "Page position changed.");
    }
    static void LauncherValidation()
    {
        var definition = Launcher(); definition.Entries.Add(definition.Entries[0].Clone()); Reject(definition.Validate);
        definition = Launcher(); definition.Entries[0].Column = 16; Reject(definition.Validate);
        definition = Launcher(); definition.Entries[0].Command = "evil\ncommand"; Reject(definition.Validate);
    }
    static void Backups()
    {
        var path = Path.Combine(root, "atomic.txt");
        ConfigurationFile.Save(path, "first"); ConfigurationFile.Save(path, "second");
        Check(File.ReadAllText(path) == "second" && File.ReadAllText(path + ".bak") == "first", "Atomic save backup failed.");
    }
    static void SelectedFiles()
    {
        var preferences = Path.Combine(root, "preferences.xml");
        var paths = new ConfigurationPaths { SensorFile = Path.Combine(root, "sensor.txt"), LauncherFile = Path.Combine(root, "karasu.exec.txt") };
        paths.Save(preferences);
        var loaded = ConfigurationPaths.Load(preferences);
        Check(loaded.SensorFile == paths.SensorFile && loaded.LauncherFile == paths.LauncherFile, "Selected files were not restored.");
        File.WriteAllText(preferences, "<broken");
        Check(ConfigurationPaths.Load(preferences).SensorFile == Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "files", "sensor.txt"), "Broken preferences did not fall back.");
    }
    static void Pages()
    {
        var path = Path.Combine(root, "karasu.exec.txt");
        using (var launcher = new CommandLauncher(CommandLauncher.ConstructionParameter.WXGA))
        {
            launcher.LoadFromFile(path, null);
            Check(launcher.PageCount == 2 && launcher.PageNumber == 1, "Wrong initial page.");
            launcher[1, 15].Execute(null); Check(launcher.PageNumber == 2, "Next page action failed.");
            launcher[3, 14].Execute(null); Check(launcher.PageNumber == 1, "Previous page action failed.");
            launcher.ChangePage(-1); Check(launcher.PageNumber == 2, "Previous page did not wrap.");
            launcher.ChangePage(1); Check(launcher.PageNumber == 1, "Next page did not wrap.");
            launcher.ChangePage(1);
            var single = new LauncherDefinition(); single.Entries.Add(new LauncherEntry { Caption = "One", Command = "one.exe" });
            var other = Path.Combine(root, "single.txt"); single.Save(other); launcher.LoadFromFile(other, null);
            Check(launcher.PageNumber == 1 && launcher.PageCount == 1, "Reload retained an invalid page.");
        }
    }
    static string Quote(string text) => "\"" + text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    static void Manifest(string steam, string id, string name, string flags, bool installed)
    {
        var apps = Path.Combine(steam, "steamapps"); Directory.CreateDirectory(apps);
        if (installed) Directory.CreateDirectory(Path.Combine(apps, "common", name));
        File.WriteAllText(Path.Combine(apps, "appmanifest_" + id + ".acf"), "\"AppState\" { \"appid\" " + Quote(id) + " \"name\" " + Quote(name) + " \"installdir\" " + Quote(name) + " \"StateFlags\" " + Quote(flags) + " }");
    }
    static void SteamLibraries()
    {
        var steam = Path.Combine(root, "Steam"); var other = Path.Combine(root, "別の Steam Library");
        Manifest(steam, "100", "Game A", "4", true); Manifest(other, "200", "日本語ゲーム", "4", true);
        Manifest(other, "300", "Uninstalled", "0", true); Manifest(other, "400", "Missing", "4", false);
        var folders = Path.Combine(steam, "steamapps", "libraryfolders.vdf");
        File.WriteAllText(folders, "// comment\n\"libraryfolders\" { \"0\" { \"path\" " + Quote(steam) + " } \"1\" { \"path\" " + Quote(other) + " } }");
        var warnings = new List<string>(); var games = SteamCatalog.FindInstalled(steam, warnings);
        Check(games.Count == 2 && games.Any(x => x.Id == "200"), "Modern multi-library discovery failed.");
        File.WriteAllText(folders, "\"LibraryFolders\" { \"1\" " + Quote(other) + " }");
        Check(SteamCatalog.FindInstalled(steam, warnings).Count == 2, "Legacy libraries failed.");
        File.WriteAllText(Path.Combine(other, "steamapps", "appmanifest_999.acf"), "\"AppState\" {");
        Check(SteamCatalog.FindInstalled(steam, warnings).Count == 2 && warnings.Count > 0, "Bad manifests stopped discovery.");
    }
    static void VdfParsing()
    {
        Check(VdfObject.Parse("\"root\" { \"name\" \"A \\\"quote\\\"\" }").Children["root"].Value("name") == "A \"quote\"", "VDF escapes failed.");
        Reject(() => VdfObject.Parse("\"root\" { \"name\" \"x\""));
    }
    static void Icons()
    {
        var icons = IconLibrary.Load(Path.Combine(Environment.SystemDirectory, "shell32.dll"));
        try
        {
            Check(icons.Count > 1, "Embedded icons were not enumerated.");
            var path = IconLibrary.Save(icons[0].Image, Path.Combine(root, "karasu.exec.txt"));
            using (var image = Image.FromFile(path)) Check(image.Width > 0 && image.Height > 0, "Saved icon is invalid.");
            Check(IconLibrary.Save(icons[0].Image, Path.Combine(root, "karasu.exec.txt")) == path, "Icon files are not deduplicated.");
        }
        finally { foreach (var icon in icons) icon.Dispose(); }
    }
    static object Karasu(string method, params object[] arguments)
    {
        var type = Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Karasu.exe")).GetType("Karasu.Program");
        return type.GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, arguments);
    }
    static void ResolvePosition()
    {
        var path = Path.Combine(root, "karasu.exec.txt");
        var first = (ProcessStartInfo)Karasu("ResolvePosition", "test-key", 0, 0, path);
        var second = (ProcessStartInfo)Karasu("ResolvePosition", "test-key", 2, 0, path);
        Check(first.FileName == "first.exe" && second.FileName == "second.exe" && second.Arguments == "--second", "Duplicate captions launched the wrong slot.");
        Check(Karasu("ResolvePosition", "wrong", 0, 0, path) == null, "Wrong key was accepted.");
        Check(Karasu("ResolvePosition", "test-key", 1, 15, path) == null, "An internal command reached Process.Start.");
    }
    static void Protocol()
    {
        var text = "EXEC2\tキー\t2\t0\t" + new string('a', 6000);
        using (var stream = new FragmentedStream(Encoding.UTF8.GetBytes(text))) Check((string)Karasu("ReadCommand", stream) == text, "Fragmented command was truncated.");
        try { using (var stream = new MemoryStream(new byte[65537])) Karasu("ReadCommand", stream); }
        catch (TargetInvocationException ex) { Check(ex.InnerException is InvalidDataException, "Wrong size-limit exception."); return; }
        throw new Exception("Oversized command was accepted.");
    }
    static void Forms()
    {
        var sensorPath = Path.Combine(root, "sensor.txt");
        var choices = new List<SensorChoice> { new SensorChoice { Id = "/amdcpu/0/load/0", Name = "CPU Total", Unit = "%", Description = "CPU Total | Load | /amdcpu/0/load/0" } };
        using (var form = new SensorSettingsForm(sensorPath, choices))
        {
            Preview(form, "sensor-meters.png");
            var tabs = Find<TabControl>(form); tabs.SelectedIndex = 1;
            Preview(form, "sensor-plot.png");
            var output = Path.Combine(root, "from-sensor-ui.txt");
            typeof(SensorSettingsForm).GetMethod("SaveDefinition", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(form, new object[] { output });
            Check(form.ResultDefinition.MeterDefines.Count == 6 && File.Exists(output), "Sensor screen save failed.");
        }
        using (var form = new LauncherSettingsForm(Path.Combine(root, "karasu.exec.txt")))
        {
            Preview(form, "launcher.png");
            typeof(LauncherSettingsForm).GetMethod("AddPage", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(form, null);
            var output = Path.Combine(root, "save-as", "from-launcher-ui.txt");
            typeof(LauncherSettingsForm).GetMethod("SaveDefinition", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(form, new object[] { output });
            var loaded = LauncherDefinition.Load(output);
            Check(loaded.Entries.Count == 6 && loaded.Entries.Max(x => x.Page) == 3, "Adding a page did not save navigation actions.");
            Check(loaded.Entries[0].IconPath == Path.Combine(root, "icon.png"), "Saving to another directory broke a relative icon path.");
        }
        using (var form = new LauncherSettingsForm())
        {
            var grid = Find<DataGridView>(form);
            grid.AutoGenerateColumns = true; // The hidden property is omitted by the VS designer.
            typeof(LauncherSettingsForm).GetMethod("LoadDefinition", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(form, new object[] { Path.Combine(root, "karasu.exec.txt") });
            Check(!grid.AutoGenerateColumns && grid.Columns.Count == 6, "Designer save introduced auto-generated launcher columns.");
        }
        using (var form = new LauncherEntryForm(Launcher().Entries[0], Path.Combine(root, "karasu.exec.txt")))
        {
            Preview(form, "launcher-entry.png");
            var system = (ComboBox)typeof(LauncherEntryForm).GetField("system", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
            Check(((SystemApplicationChoice)system.SelectedItem).Id == "", "An external app showed a selected system action.");
            system.SelectedItem = ((List<SystemApplicationChoice>)system.DataSource).Single(x => x.Id == "MonitorPowerOff");
            typeof(LauncherEntryForm).GetMethod("SaveEntry", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(form, null);
            Check(form.Entry.Command == "internal::MonitorPowerOff" && form.Entry.Option == "", "Selecting a system app retained external arguments.");
        }
        using (var form = new IconPickerForm(new[] { Path.Combine(Environment.SystemDirectory, "shell32.dll") })) Preview(form, "icon-picker.png");
    }
    static void DesignerForms()
    {
        foreach (var type in new[] { typeof(ConfigurationEditorForm), typeof(SensorSettingsForm), typeof(LauncherSettingsForm), typeof(LauncherEntryForm), typeof(SensorPickerForm), typeof(IconPickerForm), typeof(SteamPickerForm), typeof(PlotEditor) })
        {
            using (var surface = new System.ComponentModel.Design.DesignSurface())
            {
                surface.BeginLoad(type);
                var errors = string.Join("\n", surface.LoadErrors.Cast<object>().Select(x => x.ToString()));
                Check(surface.IsLoaded && surface.LoadErrors.Count == 0, type.Name + " failed to load in the designer: " + errors);
                var host = (System.ComponentModel.Design.IDesignerHost)surface.GetService(typeof(System.ComponentModel.Design.IDesignerHost));
                var control = (Control)host.RootComponent;
                Check(control.Controls.Count > 0 && host.GetDesigner(control) != null, type.Name + " has no editable design surface.");
                if (control is SensorSettingsForm)
                {
                    Check(Find<TabControl>(control).TabPages.Count == 3, "Sensor tabs require runtime setup.");
                    Check(Find<DataGridView>(control).Columns.Count == 5, "Sensor columns require runtime setup.");
                }
                if (control is LauncherSettingsForm)
                    Check(Find<DataGridView>(control).Columns.Count == 6, "Launcher columns require runtime setup.");
                var serializer = new System.ComponentModel.Design.Serialization.CodeDomComponentSerializationService(surface);
                using (var store = serializer.CreateStore())
                {
                    serializer.Serialize(store, control);
                    store.Close();
                    Check(store.Errors.Count == 0, type.Name + " failed designer serialization: " + string.Join("\n", store.Errors.Cast<object>()));
                }
            }
        }
    }
    static void DesignerRoundTrips()
    {
        var bodyField = typeof(ConfigurationEditorForm).GetField("Body", BindingFlags.Instance | BindingFlags.NonPublic);
        using (var surface = new System.ComponentModel.Design.DesignSurface(typeof(ConfigurationEditorForm)))
        {
            var host = (System.ComponentModel.Design.IDesignerHost)surface.GetService(typeof(System.ComponentModel.Design.IDesignerHost));
            var rootControl = (Control)host.RootComponent;
            var body = (Panel)bodyField.GetValue(rootControl);
            Check(ReferenceEquals(body.Parent, rootControl), "Editable inherited content is inside another inherited container.");
            var inheritance = (System.ComponentModel.InheritanceAttribute)System.ComponentModel.TypeDescriptor.GetAttributes(body)[typeof(System.ComponentModel.InheritanceAttribute)];
            Check(inheritance.InheritanceLevel != System.ComponentModel.InheritanceLevel.InheritedReadOnly, "Editor content is read-only in the designer.");
            foreach (var type in new[] { typeof(SensorSettingsForm), typeof(LauncherSettingsForm), typeof(PlotEditor) })
            using (var original = (Control)Activator.CreateInstance(type))
            {
                var originalBody = original is ConfigurationEditorForm ? (Control)bodyField.GetValue(original) : original;
                CheckDesignerColumnNames(original);
                var serializer = new System.ComponentModel.Design.Serialization.CodeDomComponentSerializationService(surface);
                using (var sourceComponents = new System.ComponentModel.Container())
                using (var store = serializer.CreateStore())
                {
                    foreach (Control part in originalBody.Controls) SerializeDesignerTree(serializer, store, sourceComponents, part);
                    store.Close();
                    Check(store.Errors.Count == 0, "Editor content could not be saved.");
                    using (var container = new System.ComponentModel.Container())
                    {
                        var restored = serializer.Deserialize(store, container).Cast<object>().OfType<Control>().ToList();
                        foreach (Control part in originalBody.Controls)
                        {
                            var copy = restored.Single(control => control.Name == part.Name);
                            Check(DesignerShape(part) == DesignerShape(copy), type.Name + " lost controls or columns after designer save/load.\n" + DesignerShape(copy));
                        }
                    }
                }
            }
        }
    }
    static void CheckDesignerColumnNames(Control control)
    {
        foreach (var field in control.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(field => field.DeclaringType.Assembly == typeof(ConfigurationEditorForm).Assembly))
        {
            if (!typeof(DataGridViewColumn).IsAssignableFrom(field.FieldType)) continue;
            var column = field.GetValue(control) as DataGridViewColumn;
            Check(column != null && column.Name == field.Name, control.GetType().Name + "." + field.Name + " has a conflicting designer component name.");
        }
        foreach (Control child in control.Controls) CheckDesignerColumnNames(child);
    }
    static void SerializeDesignerTree(System.ComponentModel.Design.Serialization.ComponentSerializationService serializer,
        System.ComponentModel.Design.Serialization.SerializationStore store, System.ComponentModel.IContainer sourceComponents, Control control)
    {
        if (control is DataGridView grid)
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                sourceComponents.Add(column, column.Name);
                serializer.Serialize(store, column);
            }
        }
        else if (!(control is PlotEditor))
        {
            foreach (Control child in control.Controls) SerializeDesignerTree(serializer, store, sourceComponents, child);
        }
        serializer.Serialize(store, control);
    }
    static string DesignerShape(Control control)
    {
        var result = control.GetType().Name + ":" + control.Name;
        if (control is DataGridView grid)
            result += "[" + string.Join(",", grid.Columns.Cast<DataGridViewColumn>().Select(column => column.Name + "=" + column.DataPropertyName + ":" + column.HeaderText)) + "]";
        return result + "(" + string.Join("|", control.Controls.Cast<Control>().Select(DesignerShape)) + ")";
    }
    static void DesignerBindingLifecycle()
    {
        using (var sensor = new SensorSettingsForm())
        using (var launcher = new LauncherSettingsForm())
        using (var plot = new PlotEditor())
        {
            CheckUnboundDesignerGrids(sensor); CheckUnboundDesignerGrids(launcher); CheckUnboundDesignerGrids(plot);
            var choices = new List<SensorChoice>();
            using (var runtime = new SensorSettingsForm(Path.Combine(root, "sensor.txt"), choices))
            {
                var grid = Find<DataGridView>(runtime);
                Check(grid.Rows.Count == 6 && grid.Columns.Cast<DataGridViewColumn>().Select(column => column.DataPropertyName).SequenceEqual(new[] { "Name", "Unit", "Min", "Max", "Id" }), "Runtime meter mapping was not restored.");
            }
            plot.LoadDefinition(Sensors().PlotDefines[0]);
            Check(plot.Series.Columns.Cast<DataGridViewColumn>().Select(column => column.DataPropertyName).SequenceEqual(new[] { "Name", "Id", "Color" }), "Runtime plot mapping was not restored.");
            Check(plot.Read().Items.Count == 1, "Runtime plot rows could not be read.");
            typeof(LauncherSettingsForm).GetMethod("LoadDefinition", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(launcher, new object[] { Path.Combine(root, "karasu.exec.txt") });
            Check(Find<DataGridView>(launcher).Columns.Cast<DataGridViewColumn>().Select(column => column.DataPropertyName).SequenceEqual(new[] { "Page", "PageRow", "Column", "Caption", "Command", "Option" }), "Runtime launcher mapping was not restored.");
        }
    }
    static void CheckUnboundDesignerGrids(Control control)
    {
        if (control is DataGridView grid)
            Check(grid.DataSource == null && grid.Columns.Count > 0 && grid.Columns.Cast<DataGridViewColumn>().All(column => string.IsNullOrEmpty(column.DataPropertyName)), "VS can remove bound columns without a design-time DataSource: " + grid.Name);
        foreach (Control child in control.Controls) CheckUnboundDesignerGrids(child);
    }
    static T Find<T>(Control parent) where T : Control
    {
        foreach (Control child in parent.Controls)
        {
            if (child is T match) return match;
            var nested = Find<T>(child); if (nested != null) return nested;
        }
        return null;
    }
    static void WidgetReplacement()
    {
        var container = new WidgetContainer(); container.Initialize(1280, 800);
        var old = new TestWidget(Color.Red); var unaffected = new TestWidget(Color.Blue);
        container.Add(old, Point.Empty); container.Add(unaffected, new Point(48, 0)); container.Update();
        var surface = (Bitmap)typeof(WidgetContainer).GetField("BackSurface", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(container);
        using (var retainedGraphics = Graphics.FromImage(surface))
        {
            container.Remove(old);
            Check(old.Disposed, "Removed widget resources were retained.");
            container.Add(new TestWidget(Color.Green), Point.Empty); container.Update();
            using (var image = new Bitmap(1280, 800))
            using (var graphics = Graphics.FromImage(image))
            {
                container.Draw(graphics);
                Check(image.GetPixel(10, 10).ToArgb() == Color.Green.ToArgb(), "Replacement widget was not drawn.");
                Check(image.GetPixel(60, 10).ToArgb() == Color.Blue.ToArgb(), "Removing a widget erased an unrelated widget.");
            }
        }
    }
    sealed class TestWidget : OwnerDrawWidget
    {
        readonly Color color;
        bool first = true;
        public bool Disposed;
        public TestWidget(Color color) : base(WidgetScaleMode.Fixed, 24, 24) { this.color = color; }
        public override bool Update() { var result = first; first = false; return result; }
        public override void Draw(Graphics graphics) { using (var brush = new SolidBrush(color)) graphics.FillRectangle(brush, 0, 0, 24, 24); }
        public override void Dispose() { Disposed = true; }
    }
    static void PremultipliedLayers()
    {
        foreach (var brightness in new[] { 1f, 0.45f, 0f })
        using (var expected = RenderLayers(false, brightness))
        using (var actual = RenderLayers(true, brightness))
        {
            for (int y = 0; y < 128; y++)
            for (int x = 0; x < 192; x++)
            {
                var a = expected.GetPixel(x, y); var b = actual.GetPixel(x, y);
                Check(Math.Abs(a.R - b.R) <= 2 && Math.Abs(a.G - b.G) <= 2 && Math.Abs(a.B - b.B) <= 2 && a.A == b.A,
                    "Premultiplied alpha changed the rendered result beyond rounding tolerance.");
            }
        }
    }
    static void DrawingBufferCapacity()
    {
        var context = BufferedGraphicsManager.Current;
        var original = context.MaximumBuffer;
        try
        {
            context.MaximumBuffer = new Size(225, 96);
            DrawingBuffer.EnsureCapacity(new Size(1280, 800));
            Check(context.MaximumBuffer == new Size(1281, 801), "Full-size drawing buffer is not retained.");
            DrawingBuffer.EnsureCapacity(new Size(320, 200));
            DrawingBuffer.EnsureCapacity(Size.Empty);
            Check(context.MaximumBuffer == new Size(1281, 801), "A smaller or minimized window shrank the buffer.");
            DrawingBuffer.EnsureCapacity(new Size(1600, 900));
            Check(context.MaximumBuffer == new Size(1601, 901), "Larger client dimensions were not handled.");
        }
        finally { context.MaximumBuffer = original; }
    }
    static void DrawingImages()
    {
        var path = Path.Combine(root, "transparent-resource.png");
        using (var original = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
        {
            using (var graphics = Graphics.FromImage(original))
            using (var brush = new SolidBrush(Color.FromArgb(77, 210, 73, 31))) graphics.FillRectangle(brush, 0, 0, 20, 24);
            original.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            using (var loaded = CommonResource.LoadImage(path))
            {
                Check(loaded.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb && loaded.Size == original.Size, "Image was not prepared for repeated drawing.");
                File.Move(path, path + ".moved");
                using (var before = new Bitmap(32, 32))
                using (var after = new Bitmap(32, 32))
                {
                    using (var graphics = Graphics.FromImage(before)) { graphics.Clear(Color.DarkBlue); graphics.DrawImageUnscaled(original, 0, 0); }
                    using (var graphics = Graphics.FromImage(after)) { graphics.Clear(Color.DarkBlue); graphics.DrawImageUnscaled(loaded, 0, 0); }
                    for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                    {
                        var a = before.GetPixel(x, y); var b = after.GetPixel(x, y);
                        Check(Math.Abs(a.R - b.R) <= 2 && Math.Abs(a.G - b.G) <= 2 && Math.Abs(a.B - b.B) <= 2 && a.A == b.A, "Image conversion changed visible transparency.");
                    }
                }
            }
        }
    }
    static void LauncherFocus()
    {
        var path = Path.Combine(root, "focus-launcher.txt");
        var definition = new LauncherDefinition();
        definition.Entries.Add(new LauncherEntry { Column = 0, Caption = "File", Command = "internal::EditLauncher", IconPath = Path.Combine(root, "icon.png") });
        definition.Entries.Add(new LauncherEntry { Column = 1, Caption = "Atlas", Command = "internal::EditLauncher", IconPath = "InternalApplicationIconsImage::0,0" });
        definition.Entries.Add(new LauncherEntry { Column = 2, Caption = "Fallback", Command = "internal::EditLauncher" });
        definition.Entries.Add(new LauncherEntry { Column = 3, Caption = "Next", Command = "internal::LauncherNextPage" });
        definition.Entries.Add(new LauncherEntry { Column = 4, Caption = "Red", Command = "internal::ProbeNoOperation" });
        definition.Entries.Add(new LauncherEntry { Row = 3, Column = 15, Caption = "Last slot", Command = "internal::EditLauncher", IconPath = Path.Combine(root, "icon.png") });
        definition.Entries.Add(new LauncherEntry { Row = 3, Column = 14, Caption = "Previous", Command = "internal::LauncherPreviousPage" });
        definition.Save(path);
        using (var launcher = new CommandLauncher(CommandLauncher.ConstructionParameter.WXGA))
        using (var bitmap = new Bitmap(1280, 180, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            launcher.LoadFromFile(path, null); launcher.Update(); launcher.Draw(graphics);
            foreach (var entry in definition.Entries)
                Check(((Bitmap)launcher[entry.Row, entry.Column].Image).PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb, "Launcher image source still needs alpha conversion.");
            foreach (var position in new[] { new[] { 0, 0 }, new[] { 0, 1 }, new[] { 0, 2 }, new[] { 0, 4 }, new[] { 3, 15 } })
            {
                int row = position[0], slot = position[1], y = row % 2 * 90;
                launcher.ChangePage(row / 2 - (launcher.PageNumber - 1)); launcher.Update(); launcher.Draw(graphics);
                var baseline = bitmap.GetPixel(slot * 80 + 10, y + 8);
                launcher.Click(slot * 80 + 40, y + 40);
                Check(launcher.Update(), "Focus did not start."); launcher.Draw(graphics);
                var focus = bitmap.GetPixel(slot * 80 + 10, y + 8);
                Check(slot == 4 ? focus.R > focus.G : focus.G > focus.R, "Focus has the wrong success/failure color.");
                for (int frame = 1; frame < 30; frame++)
                {
                    Check(launcher.Update(), "Focus animation stopped too soon."); launcher.Draw(graphics);
                    var fading = bitmap.GetPixel(slot * 80 + 10, y + 8);
                    Check(fading.A < focus.A, "Focus did not fade continuously."); focus = fading;
                }
                var cleared = bitmap.GetPixel(slot * 80 + 10, y + 8);
                Check(cleared.A == baseline.A && !launcher.Update(), "Focus indicator was not erased or idle kept redrawing.");
            }
            launcher.Click(14 * 80 + 40, 130);
            Check(launcher.PageNumber == 1 && launcher.Update(), "Previous-page navigation did not redraw."); launcher.Draw(graphics);
            Check(!launcher.Update(), "Previous-page navigation left a focus animation running.");
            launcher.Click(3 * 80 + 40, 40);
            Check(launcher.PageNumber == 2 && launcher.Update(), "Next-page navigation did not redraw."); launcher.Draw(graphics);
            Check(!launcher.Update(), "Page navigation left a focus animation running.");
        }
    }
    static void PageFaultRecording()
    {
        Check(!PageFaultDiagnostics.IsRunning, "Diagnostics were enabled by default.");
        using (PageFaultDiagnostics.Measure("Disabled")) { }
        Check(PageFaultDiagnostics.Difference(uint.MaxValue - 3, 2) == 6, "DWORD counter wrap was lost.");
        var directory = Path.Combine(root, "page-fault-recording");
        PageFaultDiagnostics.Start(TimeSpan.FromMinutes(1), directory);
        Check(PageFaultDiagnostics.Complete() == null, "Recording stopped before its deadline.");
        bool duplicateRejected = false;
        try { PageFaultDiagnostics.Start(TimeSpan.FromSeconds(30), directory); }
        catch (InvalidOperationException) { duplicateRejected = true; }
        Check(duplicateRejected, "A running recording was replaced.");
        for (int i = 0; i < 4; i++) using (PageFaultDiagnostics.Measure("Stable")) { }
        var container = new WidgetContainer(); container.Initialize(1280, 800);
        var widget = new TestWidget(Color.Red); container.Add(widget);
        try
        {
            container.Update();
            using (var bitmap = new Bitmap(1280, 800, System.Drawing.Imaging.PixelFormat.Format32bppPArgb))
            using (var graphics = Graphics.FromImage(bitmap)) container.Draw(graphics);
        }
        finally
        {
            container.Remove(widget);
            foreach (var field in new[] { "BacksurfaceGraphic", "Background", "Overlay", "BackSurface" })
                ((IDisposable)typeof(WidgetContainer).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(container)).Dispose();
        }
        using (var started = new System.Threading.ManualResetEventSlim())
        using (var release = new System.Threading.ManualResetEventSlim())
        {
            var worker = System.Threading.Tasks.Task.Run(() =>
            {
                using (PageFaultDiagnostics.Measure("Worker")) { started.Set(); Check(release.Wait(5000), "Worker was not released."); }
            });
            try
            {
                Check(started.Wait(5000), "Worker did not start.");
                using (PageFaultDiagnostics.Measure("UI")) { }
            }
            finally { release.Set(); worker.GetAwaiter().GetResult(); }
        }
        try { using (PageFaultDiagnostics.Measure("Failure")) throw new InvalidOperationException("Expected"); }
        catch (InvalidOperationException) { }
        var pending = PageFaultDiagnostics.Measure("Pending");
        Check(PageFaultDiagnostics.Complete(true) == null, "An active scope was exported prematurely.");
        using (PageFaultDiagnostics.Measure("AfterStop")) { }
        pending.Dispose(); pending.Dispose();
        var path = PageFaultDiagnostics.Complete(true);
        Check(path != null && !PageFaultDiagnostics.IsRunning && PageFaultDiagnostics.Complete(true) == null, "Recording did not stop exactly once.");
        var lines = File.ReadAllLines(path);
        Func<string, string[]> row = name => lines.Single(x => x.StartsWith("scope,\"" + name + "\",", StringComparison.Ordinal)).Split(',');
        Check(row("Stable")[2] == "4" && row("Stable")[6] == "2", "Warmup samples were not excluded from steady counts.");
        Check(row("Worker")[3] == "1" && row("UI")[3] == "1", "Concurrent scopes were presented as disjoint attribution.");
        Check(row("Pending")[2] == "1" && row("Failure")[2] == "1", "Pending or failed work was omitted.");
        Check(row("TestWidget.Update")[2] == "1" && row("TestWidget.Draw")[2] == "1" && row("Window.Composition")[2] == "1", "Production update and drawing scopes were not connected.");
        Check(!lines.Any(x => x.Contains("Disabled") || x.Contains("AfterStop")), "Inactive scopes were recorded.");
        PageFaultDiagnostics.Start(TimeSpan.FromMilliseconds(1), directory);
        System.Threading.Thread.Sleep(20);
        Check(PageFaultDiagnostics.Complete() != null, "Expired recording did not complete automatically.");
    }
    static void NvidiaQueries()
    {
        // Real library types and readonly fields, with no constructor, Update,
        // driver initialization, control setting or Close call.
        var assembly = typeof(ISensor).Assembly;
        var type = assembly.GetType("LibreHardwareMonitor.Hardware.Gpu.NvidiaGpu", true);
        var device = (IHardware)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
        var powerField = type.GetField("_powers", BindingFlags.Instance | BindingFlags.NonPublic);
        var voltageField = type.GetField("_coreVoltage", BindingFlags.Instance | BindingFlags.NonPublic);
        var sensorType = voltageField.FieldType;
        var owner = sensorType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Single(field => typeof(IHardware).IsAssignableFrom(field.FieldType));
        Func<ISensor> sensor = () =>
        {
            var result = (ISensor)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(sensorType);
            owner.SetValue(result, device); return result;
        };
        var first = sensor(); var second = sensor(); var voltage = sensor();
        var powers = Array.CreateInstance(sensorType, 2); powers.SetValue(first, 0); powers.SetValue(second, 1);
        powerField.SetValue(device, powers); voltageField.SetValue(device, voltage);
        var policy = new NvidiaOptionalQueries();
        policy.Configure(new[] { device }, new HashSet<ISensor>());
        Check(powerField.GetValue(device) == null && voltageField.GetValue(device) == null, "Unused expensive queries remained enabled.");
        policy.Configure(new[] { device }, new HashSet<ISensor> { second });
        Check(ReferenceEquals(powerField.GetValue(device), powers) && voltageField.GetValue(device) == null, "Selecting one power ratio did not restore its group.");
        policy.Configure(new[] { device }, new HashSet<ISensor> { voltage });
        Check(powerField.GetValue(device) == null && ReferenceEquals(voltageField.GetValue(device), voltage), "Selecting voltage restored the wrong query.");
        var aggregate = new ValueHistorySensor(new[] { first, second }, new LibreHardwareMonitor.Hardware.Identifier("test", "sum"), true);
        policy.Configure(new[] { device }, new HashSet<ISensor>(aggregate.SourceSensors));
        Check(ReferenceEquals(powerField.GetValue(device), powers), "An aggregate lost its underlying power sensors.");
        policy.Configure(new[] { device }, new HashSet<ISensor>());
        policy.Configure(new[] { device }, new HashSet<ISensor>(new ValueHistorySensor(aggregate).SourceSensors));
        Check(ReferenceEquals(powerField.GetValue(device), powers), "A wrapped aggregate lost its underlying power sensors.");
        policy.Configure(new IHardware[0], new HashSet<ISensor>());
        Check(ReferenceEquals(powerField.GetValue(device), powers) && ReferenceEquals(voltageField.GetValue(device), voltage), "Removing a device did not restore its original fields.");
        policy.Configure(new[] { device }, new HashSet<ISensor>()); policy.Restore();
        Check(ReferenceEquals(powerField.GetValue(device), powers) && ReferenceEquals(voltageField.GetValue(device), voltage), "Disposal restore changed sensor identity.");
        Check(OptionalSensorQuery.Create(device, typeof(string).GetField("Empty")) == null, "Unrecognized field shape was accepted.");
    }
    static Bitmap RenderLayers(bool premultiplied, float brightness)
    {
        var container = new WidgetContainer(); container.Initialize(1280, 800);
        var fields = new[] { "Background", "Overlay", "BackSurface" }.Select(name => typeof(WidgetContainer).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)).ToArray();
        var graphicsField = typeof(WidgetContainer).GetField("BacksurfaceGraphic", BindingFlags.Instance | BindingFlags.NonPublic);
        if (!premultiplied)
        {
            ((Graphics)graphicsField.GetValue(container)).Dispose();
            foreach (var field in fields)
            {
                var old = (Bitmap)field.GetValue(container);
                var replacement = new Bitmap(old.Width, old.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (var graphics = Graphics.FromImage(replacement)) graphics.DrawImageUnscaled(old, 0, 0);
                field.SetValue(container, replacement); old.Dispose();
            }
            graphicsField.SetValue(container, Graphics.FromImage((Bitmap)fields[2].GetValue(container)));
        }
        else foreach (var field in fields)
            Check(((Bitmap)field.GetValue(container)).PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb, "A compositing layer still requires conversion.");
        container.Add(new AlphaWidget(), Point.Empty); container.Brightness = brightness; container.Update();
        var output = new Bitmap(192, 128);
        using (var graphics = Graphics.FromImage(output)) container.Draw(graphics);
        ((Graphics)graphicsField.GetValue(container)).Dispose();
        foreach (var field in fields) ((Bitmap)field.GetValue(container)).Dispose();
        return output;
    }
    sealed class AlphaWidget : OwnerDrawWidget
    {
        public AlphaWidget() : base(WidgetScaleMode.Fixed, 192, 128) { }
        public override void Draw(Graphics graphics)
        {
            graphics.Clear(Color.Transparent);
            using (var brush = new SolidBrush(Color.FromArgb(77, 210, 73, 31))) graphics.FillRectangle(brush, 5, 5, 150, 60);
            using (var brush = new SolidBrush(Color.FromArgb(160, 23, 190, 240))) graphics.FillEllipse(brush, 30, 20, 100, 80);
            graphics.DrawString("Alpha / 半透明", SystemFonts.DefaultFont, Brushes.White, 5, 105);
        }
    }
    static void Preview(Form form, string name)
    {
        form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-20000, -20000); form.ShowInTaskbar = false;
        form.Show(); Application.DoEvents(); form.PerformLayout();
        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "previews"); Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, name);
        using (var bitmap = new Bitmap(form.Width, form.Height)) { form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size)); bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png); }
        form.Hide(); images.Add(path);
    }
    sealed class FragmentedStream : MemoryStream
    {
        public FragmentedStream(byte[] bytes) : base(bytes) { }
        public override int Read(byte[] buffer, int offset, int count) => base.Read(buffer, offset, Math.Min(count, 1));
    }
}
