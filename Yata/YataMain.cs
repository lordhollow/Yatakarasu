using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Yata.Components;
using Yata.Components.Widget;
using Yata.Components.Application;
using System.Threading;
using System.Runtime.InteropServices;
using Yata.Components.Configuration;
using Yata.Settings;

namespace Yata
{
    public partial class YataMain : Form
    {
        /// <summary>
        /// コンテナ
        /// </summary>
        WidgetContainer container;

        /// <summary>
        /// Update間隔の所要時間計測機
        /// </summary>
        IntervalDiagnostics diagnostics;

        /// <summary>
        /// センサーの最大履歴保存時間。
        /// </summary>
        TimeSpan sensorTimeWindow = TimeSpan.FromMinutes(12);

        /// <summary>
        /// パラメータ
        /// </summary>
        ScreenParameter screenParameter;
        HardwareMonitor monitor;
        Karasu karasu;
        CommandLauncher launcher;
        SuspendedChecker suspendChecker;
        ConfigurationPaths configurationPaths;
        readonly List<IUpdatable> sensorWidgets = new List<IUpdatable>();
        bool settingsOpen;

        public YataMain()
        {
            InitializeComponent();


            DoubleBuffered = true;

            FormBorderStyle = FormBorderStyle.None;
            screenParameter = new ScreenParameter();

            container = new WidgetContainer();
            container.Initialize(screenParameter.ScreenWidth, screenParameter.ScreenHeight);
            var bp = 24;    //bp=base pitch, 基本間隔(スナップするグリッドの幅)

            //非表示コンポーネント
            var sensorDefine = new SensorDefine();
            configurationPaths = ConfigurationPaths.Load();
            try { sensorDefine.ReadFromFile(configurationPaths.SensorFile); }
            catch (Exception ex)
            {
                Shown += (sender, args) => MessageBox.Show(this, "センサー定義を読み込めません。右クリック → センサー選択・設定から修正してください。\r\n" + ex.Message, "センサー定義", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            sensorDefine.EnsureFixedLayout();
            monitor = hwMonitorSetup(sensorDefine);
            container.Add(monitor);
            karasu = new Karasu();
            container.Add(karasu);
            this.FormClosed += (s, a) => karasu.SendImmidiate("EXIT");
            suspendChecker = new SuspendedChecker() { Expire = sensorTimeWindow };
            container.Add(suspendChecker);

            //ウィジェット
            //最大化・最小化・カラス
            container.Add(new KarasuIndicator(karasu), new Point(screenParameter.ScreenWidth - 48 * 3, 0));
            container.Add(new MinimizeButton(this), new Point(screenParameter.ScreenWidth - 48 * 2, 0));
            container.Add(new CloseButton(this), new Point(screenParameter.ScreenWidth - 48 * 1, 0));
            //時計
            container.Add(new DigitalClock(), new Point(0, 0));
            container.Add(new AnalogClock(), new Point(0, 2 * bp));
            //プロット
            SetupPlotWidgets(bp, sensorDefine, monitor, suspendChecker);
            //メーター
            SetupMeterWidgets(bp, sensorDefine, monitor);
            //音量
            var volumeCtrl = new Components.AudioVolumeController();    //これはIUpdatableではない
            var volumeCtrlY = 10 * bp + 200 - 32;   //200はWXSGA→WXGAの高さの増分。32はこの時のランチャの高さ変動分。
            container.Add(new VerticalBarMeter(0, 100, () => (int)volumeCtrl.Volume), new Point(1 * bp, volumeCtrlY));
            container.Add(new AudioMuteButton(volumeCtrl, false), new Point(1 * bp, volumeCtrlY + 6 * bp + 8));
            container.Add(new VerticalBarMeter(0, 100, () => (int)volumeCtrl.CaptureVolume), new Point(4 * bp, volumeCtrlY));
            container.Add(new AudioMuteButton(volumeCtrl, true), new Point(4 * bp, volumeCtrlY + 6 * bp + 8));

            //ランチャ
            launcher = new CommandLauncher(CommandLauncher.ConstructionParameter.WXGA);
            launcher.Karasu = karasu;
            try { launcher.LoadFromFile(configurationPaths.LauncherFile, container); }
            catch (Exception ex)
            {
                Shown += (sender, args) => MessageBox.Show(this, "ランチャー定義を読み込めません。右クリック → ランチャー編集で修正または別ファイルを選択してください。\r\n" + ex.Message, "ランチャー定義", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            launcher.SystemActionRequested += name => BeginInvoke((Action)(() => { if (name == "EditSensors") OpenSensorSettings(); else OpenLauncherSettings(); }));
            //RegistorInternalApplications(launcher);
            container.Add(launcher, new Point(0, screenParameter.ScreenHeight - launcher.Height));

            //ダイアグ
            diagnostics = new IntervalDiagnostics(container.Update);
#if DEBUG
            //ダイアグ内容の表示はやるとしてもデバッグビルドだけ
            container.Add(new IntervalDiagnosticsDisplay(diagnostics, karasu), new Point(14 * bp, 0));
#endif

            ClientSize = new Size((int)(container.Width * screenParameter.ScreenScale), (int)(container.Height * screenParameter.ScreenScale));
            SetupSettingsMenu();

            tmUpdate.Enabled = true;
        }

        private void SetupPlotWidgets(int bp, SensorDefine sensorDefine, HardwareMonitor monitor, SuspendedChecker suspendChecker)
        {
            //２個までの負荷情報
            int plotMax = 2;
            var plotterBaseY = 2 * bp;
            foreach (var plotDef in sensorDefine.PlotDefines)
            {
                var current = new SensorCurrentList
                {
                    Caption = plotDef.Name,
                    Unit = plotDef.Unit,
                    LowValueThreashold = plotDef.LowLevelThreshold,
                    HighValueThreashold = plotDef.HighLevelThreshold,
                    /* min / max */
                };

                foreach (var plot in plotDef.Items)
                {
                    current.Add(monitor[plot.Id], plot.Name, plot.Color);
                }
                var plotter = new SensorPlot(SensorPlot.ConstructionParameter.WXGA)
                {
                    Minimum = plotDef.Min,
                    Maximum = plotDef.Max,
                    LowLevelThreshold = plotDef.LowLevelThreshold,
                    HighLevelThreshold = plotDef.HighLevelThreshold,
                };
                plotter.SuspendChecker = suspendChecker;
                plotter.AddRange(current.Sensors);


                container.Add(current, new Point(8 * bp, plotterBaseY));
                container.Add(plotter, new Point(14 * bp, plotterBaseY + 4));
                sensorWidgets.Add(current);
                sensorWidgets.Add(plotter);
                plotterBaseY += plotter.Height;
                if (--plotMax <= 0) break;
            }
        }

        private void SetupMeterWidgets(int bp, SensorDefine sensorDefine, HardwareMonitor monitor)
        {
            //位置は既存のまま、タコメーターは6個に固定する。
            int meterIndex = 0;
            var meterY = new int[] { 2 * bp + 12, 5 * bp + 12, 8 * bp + 12, 11 * bp + 12, 15 * bp, 18 * bp, 21 * bp };
            {
                foreach (var meterDef in sensorDefine.MeterDefines.Take(6))
                {
                    var id = meterDef.Id;
                    var tacho = new Tachometer(meterDef.Min, meterDef.Max, monitor[id])
                    {
                        Caption = meterDef.Name,
                        Unit = meterDef.Unit,
                    };
                    container.Add(tacho, new Point(screenParameter.ScreenWidth - 72 - 16, meterY[meterIndex++]));
                    sensorWidgets.Add(tacho);
                }
            }
        }

        private HardwareMonitor hwMonitorSetup(SensorDefine define)
        {
            var mon = new HardwareMonitor();
            var sensorUsage = define.SensorUsage().Where(x => !string.IsNullOrWhiteSpace(x) && x != "-").Distinct();
            mon.Startup(sensorTimeWindow, sensorUsage);
            //var useSensor = mon.AllSensors.Where(x => sensorUsage.Contains(x.Identifier.ToString()));
            //mon.Startup(PlotLength, useSensor);
            return mon;
        }

        private void tmUpdate_Tick(object sender, EventArgs e)
        {
            if (!this.Focused)
            {
                GlobalWindowUtil.BackupForground();
            }

            if (diagnostics.Do())   //container.Update
            {
                CheckPosition();
                Invalidate();
            }
        }

        private void YataMain_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.ScaleTransform(screenParameter.ScreenScale, screenParameter.ScreenScale);
            container.Draw(e.Graphics);
            e.Graphics.ResetTransform();
        }

        Point ClickPos;
        MouseButtons clickButton;

        private void YataMain_Click(object sender, EventArgs e)
        {
            if (clickButton != MouseButtons.Left) return;
            if ((ClickPos.X < container.Width) && (ClickPos.Y < container.Height))
            {
                container.Click(ClickPos.X, ClickPos.Y);
            }
        }

        private void YataMain_MouseDown(object sender, MouseEventArgs e)
        {
            clickButton = e.Button;
            ClickPos.X = (int)(e.Location.X / screenParameter.ScreenScale);
            ClickPos.Y = (int)(e.Location.Y / screenParameter.ScreenScale);

        }

        private void YataMain_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = true;
            }
            else
            {
                ShowInTaskbar = false;
                //最小化から戻るとき、一瞬TopMostを設定しないとタスクバーの前に出てこないみたいなので。
                TopMost = true;
                Application.DoEvents();
                TopMost = false;
            }
        }

        public void CheckPosition()
        {
            var sc = Screen.AllScreens.Last();
            Location = new Point(sc.Bounds.Right - Width, sc.Bounds.Top);
        }

        void SetupSettingsMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("センサー選択・設定… (F2)", null, (s, e) => OpenSensorSettings());
            menu.Items.Add("ランチャー編集… (F3)", null, (s, e) => OpenLauncherSettings());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("前のページ", null, (s, e) => launcher.ChangePage(-1));
            menu.Items.Add("次のページ", null, (s, e) => launcher.ChangePage(1));
            var page = menu.Items.Add(""); page.Enabled = false;
            menu.Opening += (s, e) => page.Text = $"ページ {launcher.PageNumber} / {launcher.PageCount}";
            ContextMenuStrip = menu;
            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F2) { e.Handled = true; OpenSensorSettings(); }
                else if (e.KeyCode == Keys.F3) { e.Handled = true; OpenLauncherSettings(); }
            };
        }

        async void OpenSensorSettings()
        {
            if (settingsOpen) return;
            settingsOpen = true;
            try
            {
                using (var dialog = new SensorSettingsForm(configurationPaths.SensorFile, SensorChoice.FromSensors(monitor.AllSensors)))
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    tmUpdate.Enabled = false;
                    UseWaitCursor = true;
                    await monitor.ReconfigureAsync(sensorTimeWindow, dialog.ResultDefinition.SensorUsage().Where(x => !string.IsNullOrWhiteSpace(x) && x != "-").Distinct());
                    if (IsDisposed || Disposing) return;
                    foreach (var widget in sensorWidgets) container.Remove(widget);
                    sensorWidgets.Clear();
                    SetupPlotWidgets(24, dialog.ResultDefinition, monitor, suspendChecker);
                    SetupMeterWidgets(24, dialog.ResultDefinition, monitor);
                    configurationPaths.SensorFile = dialog.SelectedPath;
                    configurationPaths.Save();
                    container.Update(); Invalidate();
                }
            }
            catch (Exception ex) { if (!IsDisposed) EditorUi.Error(this, ex); }
            finally { if (!IsDisposed) { UseWaitCursor = false; tmUpdate.Enabled = true; } settingsOpen = false; }
        }

        void OpenLauncherSettings()
        {
            if (settingsOpen) return;
            settingsOpen = true;
            try
            {
                using (var dialog = new LauncherSettingsForm(configurationPaths.LauncherFile))
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    launcher.LoadFromFile(dialog.SelectedPath, container);
                    configurationPaths.LauncherFile = dialog.SelectedPath;
                    configurationPaths.Save();
                    container.Update(); Invalidate();
                }
            }
            catch (Exception ex) { EditorUi.Error(this, ex); }
            finally { settingsOpen = false; }
        }

    }

    class GlobalWindowUtil
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);


        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        static IntPtr prevWindow = IntPtr.Zero;

        public static void BackupForground()
        {
            prevWindow = GetForegroundWindow();
        }

        public static void RestoreForground()
        {
            SetForegroundWindow(prevWindow);
        }
    }

}
