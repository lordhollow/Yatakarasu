using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Yata.Components.Application
{

    /// <summary>
    /// 物理ディスプレイの入力ポート(DP, HDMI)を切り替える
    /// </summary>
    internal class SetDisplayInput : IInternalApplication
    {
        const int ToggleInput1 = SOURCE_DP1;
        const int ToggleInput2 = SOURCE_HDMI1;

        /// <summary>
        /// 実施
        /// </summary>
        /// <param name="karasu"></param>
        /// <param name="option">ディスプレイID(0～)=入力ソース(TOGGLE/DP1/HDMI1)</param>
        /// <remarks>
        /// optionは左辺を省略可("DP1"のみ指定可)。その場合はID=0とする。
        /// DDC(DxVA2)上の管理IDなので不用意に変わるのかもしれないので本来はモデルIDなりなんなりを指定できたほうがよさそう。
        /// </remarks>
        public void Execute(Karasu karasu, string option)
        {

            var reg = new Regex(@"((?<ID>\d+)=)?(?<INPUT>[^\s]+)");
            var match = reg.Match(option);
            if (match.Success)
            {
                var dID = match.Groups["ID"].Value;
                var dInput = match.Groups["INPUT"].Value;
                SelectSouce(dID, dInput);
            }
        }

        /// <summary>
        /// パラメータからソースIDを選択
        /// </summary>
        /// <param name="dInput"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private void SelectSouce(string id, string dInput)
        {
            var physicalMonitors = new List<PhysicalMonitor>();
            try
            {
                //物理モニタ列挙
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                    (IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData) =>
                    {
                        if (!GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint arrSize)) return true;
                        var hPhysicalMonitors = new PhysicalMonitor[arrSize];
                        if (!GetPhysicalMonitorsFromHMONITOR(hMonitor, arrSize, hPhysicalMonitors)) return true;
                        physicalMonitors.AddRange(hPhysicalMonitors); return true;
                    }, IntPtr.Zero);

                //モニタ操作
                var monitor = Select(physicalMonitors, id);
                if (monitor != null)
                {
                    var dinputNo = SOURCE_DP1;  //とりあえずDP1
                    switch (dInput.ToUpper())
                    {
                        case "TOGGLE":  dinputNo = Toggle(monitor.Value.GetInputSource()); break;
                        case "DP1":     dinputNo = SOURCE_DP1; break;
                        case "DP2":     dinputNo = SOURCE_DP2; break;
                        case "HDMI1":   dinputNo = SOURCE_HDMI1; break;
                        case "HDMI2":   dinputNo = SOURCE_HDMI2; break;
                        default:
                            dinputNo = int.Parse(dInput);
                            break;
                    }
                    monitor.Value.SetInputSource(dinputNo);
                }
            }
            finally
            {
                //物理モニタ操作の終了
                DestroyPhysicalMonitors((uint)physicalMonitors.Count, physicalMonitors.ToArray());
            }
        }

        /// <summary>
        /// 物理モニターの選択
        /// </summary>
        /// <param name="physicalMonitors">物理モニターリスト</param>
        /// <param name="id">選択するもの(今はリストのインデックスのみ)</param>
        /// <returns>選択されたモニタまたはnull</returns>
        PhysicalMonitor? Select(List<PhysicalMonitor> physicalMonitors, string id)
        {
            var v = string.IsNullOrEmpty(id) ? 0 : int.Parse(id);
            if ((v>=0)&&(v< physicalMonitors.Count))
            {
                return physicalMonitors[v];
            }
            return null;
        }

        /// <summary>
        /// 今のinputをもとにToggleInput1か2のいずれか(currentとは異なる値)を返す
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        int Toggle(int current)
        {
            return current == ToggleInput1 ? ToggleInput2 : ToggleInput1;
        }

        #region DDC/CI

        //from VESA MCCS
        /// <summary>
        /// DisplayPort 1
        /// </summary>
        private const int SOURCE_DP1 = 15;
        /// <summary>
        /// DisplayPort 2
        /// </summary>
        private const int SOURCE_DP2 = 16;
        /// <summary>
        /// HDMI 1
        /// </summary>
        private const int SOURCE_HDMI1 = 17;
        /// <summary>
        /// HDMI 2
        /// </summary>
        private const int SOURCE_HDMI2 = 18;

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("Dxva2.dll")]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

        [DllImport("Dxva2.dll")]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PhysicalMonitor[] pPhysicalMonitorArray);

        [DllImport("Dxva2.dll")]
        private static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, PhysicalMonitor[] pPhysicalMonitorArray);

        /// <summary>
        /// (LP)PHYSICAL_MONITOR
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PhysicalMonitor
        {
            public IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;

            [DllImport("Dxva2.dll")]
            private static extern bool GetVCPFeatureAndVCPFeatureReply(IntPtr hMonitor, byte bVCPCode, IntPtr pvct, out uint pdwCurrentValue, out uint pdwMaximumValue);

            [DllImport("Dxva2.dll")]
            private static extern bool SetVCPFeature(IntPtr hMonitor, byte bVCPCode, uint dwNewValue);

            /// <summary>
            /// 入力ソース
            /// </summary>
            /// <seealso cref="https://milek7.pl/ddcbacklight/mccs.pdf"/>
            const byte VCP_INPUT_SOURCE_SELECT = 0x60;

            /// <summary>
            /// get input source of monitor
            /// </summary>
            /// <returns></returns>
            public int GetInputSource()
            {
                if (GetVCPFeatureAndVCPFeatureReply(hPhysicalMonitor, VCP_INPUT_SOURCE_SELECT, IntPtr.Zero, out uint current, out uint max))
                {
                    return (int)(current & 0x00FF);
                }
                return -1;
            }

            /// <summary>
            /// set input source of monitor
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            public bool SetInputSource(int source)
            {
                return SetVCPFeature(hPhysicalMonitor, VCP_INPUT_SOURCE_SELECT, (uint)source);
            }
        }

        #endregion
    }
}
