using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Application
{
    /// <summary>
    /// 内部アプリケーションのファクトリ
    /// </summary>
    internal class ApplicationProvider
    {
        public static IInternalApplication GetInternalApplication(string name, WidgetContainer container)
        {
            switch (name)
            {
                case "ScreenLock":
                    return new Application.ScreenLock();
                case "Suspender":
                    return new Application.Suspender();
                case "DashboardBrightnessChanger":
                    return new Application.DashboardBrightnessChanger(container);
                case "SetDisplayInput":
                    return new Application.SetDisplayInput();
                case "SetDisplayToporogy":
                    return new Application.SetDisplayToporogy();
                case "SendKeyStroke":
                    return new Application.SendKeyStroke();
                case "ToggleScreenKeyboard":
                    return new Application.ToggleScreenKeyboard();

                case "FanControl":
                    return new Application.FanControl(HardwareMonitor.DefaultInstance);

                case "CommandWithFile(A)":
                    return new Application.CommandWithFile(Application.CommandExecutionType.CommandLine);
                case "CommandWithFile(AK)":
                    return new Application.CommandWithFile(Application.CommandExecutionType.CommandLineKeepAlive);
                case "CommandWithFile(AR)":
                    return new Application.CommandWithFile(Application.CommandExecutionType.CommandLineRedirect);
                case "CommandWithFile(AP)":
                    return new Application.CommandWithFile(Application.CommandExecutionType.ProcessStart);

                case "CommandWithFile":
                    return new Application.CommandWithFile(Application.CommandExecutionType.KarasuCommandLine);
                case "CommandWithFile(K)":
                    return new Application.CommandWithFile(Application.CommandExecutionType.KarasuCommandLineKeepAlive);
                case "CommandWithFile(R)":
                    return new Application.CommandWithFile(Application.CommandExecutionType.KarasuCommandLineRedirect);
                case "CommandWithFile(P)":
                    return new Application.CommandWithFile(Application.CommandExecutionType.KarasProcessStart);
            }
            return null;
        }
    }
}
