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

        public YataMain()
        {
            InitializeComponent();


            DoubleBuffered = true;

            FormBorderStyle = FormBorderStyle.None;
            screenParameter = new ScreenParameter();

            container = new WidgetContainer();
            container.Initialize(screenParameter.ScreenWidth, screenParameter.ScreenHeight);
            var bp = 24;    //bp=base pitch, 基本間隔(スナップするグリッドの幅)
            var rm = 16;    //右マージン。1024は24で割ると42あまり16なのでその16。右寄せするときこれを足す。

            //非表示コンポーネント
            var sensorDefine = new SensorDefine();
            sensorDefine.ReadFromFile(CommonResource.YataFile("sensor.txt"));
            var monitor = hwMonitorSetup(sensorDefine);
            container.Add(monitor);
            var karasu = new Karasu();
            container.Add(karasu);
            this.FormClosed += (s, a) => karasu.SendImmidiate("EXIT");
            var suspendChecker = new SuspendedChecker() { Expire = sensorTimeWindow };
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
            var launcher = new CommandLauncher(CommandLauncher.ConstructionParameter.WXGA);
            launcher.Karasu = karasu;
            launcher.LoadFromFile(CommonResource.YataFile("karasu.exec.txt"), container);
            //RegistorInternalApplications(launcher);
            container.Add(launcher, new Point(0, screenParameter.ScreenHeight - launcher.Height));

            //ダイアグ
            diagnostics = new IntervalDiagnostics(container.Update);
#if DEBUG
            //ダイアグ内容の表示はやるとしてもデバッグビルドだけ
            container.Add(new IntervalDiagnosticsDisplay(diagnostics, karasu), new Point(14 * bp, 0));
#endif

            ClientSize = new Size((int)(container.Width * screenParameter.ScreenScale), (int)(container.Height * screenParameter.ScreenScale));

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
                plotterBaseY += plotter.Height;
                if (--plotMax <= 0) break;
            }
        }

        private void SetupMeterWidgets(int bp, SensorDefine sensorDefine, HardwareMonitor monitor)
        {
            //5個までのメーター
            int meterMax = screenParameter.ScreenHeight >= 800 ? 7 : 5;
            var meterY = new int[] { 2 * bp + 12, 5 * bp + 12, 8 * bp + 12, 11 * bp + 12, 15 * bp, 18 * bp, 21 * bp };
            {
                foreach (var meterDef in sensorDefine.MeterDefines)
                {
                    var id = meterDef.Id;
                    var tacho = new Tachometer((int)meterDef.Min, (int)meterDef.Max, monitor[id])
                    {
                        Caption = meterDef.Name,
                        Unit = meterDef.Unit,
                    };
                    container.Add(tacho, new Point(screenParameter.ScreenWidth - 72 - 16, meterY[7 - meterMax]));
                    if (--meterMax <= 0) break;
                }
            }
        }

        private HardwareMonitor hwMonitorSetup(SensorDefine define)
        {
            var mon = new HardwareMonitor();
            var sensorUsage = define.SensorUsage();
            mon.Startup(sensorTimeWindow, sensorUsage);
            //var useSensor = mon.AllSensors.Where(x => sensorUsage.Contains(x.Identifier.ToString()));
            //mon.Startup(PlotLength, useSensor);
            return mon;
        }

        private void tmUpdate_Tick(object sender, EventArgs e)
        {
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

        private void YataMain_Click(object sender, EventArgs e)
        {
            if ((ClickPos.X < container.Width) && (ClickPos.Y < container.Height))
            {
                container.Click(ClickPos.X, ClickPos.Y);
            }
        }

        private void YataMain_MouseDown(object sender, MouseEventArgs e)
        {
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

    }
}
