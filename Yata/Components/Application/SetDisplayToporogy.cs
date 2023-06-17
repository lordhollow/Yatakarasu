using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Yata.Components.Application
{
    internal class SetDisplayToporogy : IInternalApplication
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="numPathArrayElements"></param>
        /// <param name="pathArray"></param>
        /// <param name="numModeInfoArrayElements"></param>
        /// <param name="modeInfoArray"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        /// <seealso cref="https://learn.microsoft.com/ja-jp/windows/win32/api/winuser/nf-winuser-setdisplayconfig"/>
        [DllImport("User32.dll")]
        static extern Int32 SetDisplayConfig(
            UInt32 numPathArrayElements,
            IntPtr pathArray,
            UInt32 numModeInfoArrayElements,
            IntPtr modeInfoArray,
            UInt32 flags);

        /// <summary>
        /// 設定するトポロジー。この値はSDC_TOPOLOGY_XXXに合わせる。
        /// </summary>
        public enum Topology
        {
            Internal = 1,
            Clone = 2,
            Extend = 4,
            External = 8,
        }

        const UInt32 SDC_APPLY = 0x80;

        public void SetDisplayTopology(Topology t)
        {
            //SDC_TOPOLOGY_XXXとSDC_APPLYを合わせて渡す。その場合ほかのパラメータは全部不要なので0 or NULL を渡す。
            SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, (UInt32)t | SDC_APPLY);
        }

        public void Execute(Karasu karasu, string option)
        {
            switch (option.ToUpper())
            {
                case "INTERNAL":
                    SetDisplayTopology(Topology.Internal); break;
                case "CLONE":
                    SetDisplayTopology(Topology.Clone); break;
                case "EXTEND":
                    SetDisplayTopology(Topology.Extend); break;
                case "EXTERNAL":
                    SetDisplayTopology(Topology.External); break;
                default:
                    break;
            }
        }
    }
}
