using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components
{
    /// <summary>
    /// キーストロークを送る機能。
    /// </summary>
    /// <seealso cref="https://learn.microsoft.com/ja-jp/windows/win32/api/winuser/nf-winuser-sendinput"/>
    internal class KeyStroke
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public UInt16 wVK;
            public UInt16 sScan;
            public UInt32 dwFlags;
            public UInt32 time;
            public IntPtr dwExtraInfo;
            //MOUSEの構造体がLONG/LONG/DWORD/DWORD/DWORD/PTR になっていてDWORD 2つ分短いのでダミーで足しておく。HWのほうは短いので考慮しない。ないと動かないし多くても動かない。
            public UInt64 dmy;
        }

        /// <summary>
        /// winuser.hのINPUT構造体のうちunion DUMMYINPUTNAME.kiのみ実装したもの
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT_KI
        {
            public UInt32 type;
            public KEYBDINPUT ki;
        }


        [DllImport("user32.dll", SetLastError = true)]
        public static extern UInt32 SendInput(UInt32 numberOfInputs, INPUT_KI[] inputs, Int32 sizeOfInputStructure);

        public static UInt32 SendInput(params INPUT_KI[] keys)
        {
            return SendInput((uint)keys.Length, keys, Marshal.SizeOf(typeof(INPUT_KI)));
        }

        public static INPUT_KI KeyDown(ushort keyCode)
        {
            return new INPUT_KI { type = 1, ki = new KEYBDINPUT { wVK = keyCode, dwFlags = 0 } };
        }

        public static INPUT_KI KeyUp(ushort keyCode)
        {
            return new INPUT_KI { type = 1, ki = new KEYBDINPUT { wVK = keyCode, dwFlags = 2 } };
        }

        public const ushort VKEY_CTRL = 0x11;
        public const ushort VKEY_LWIN = 0x5B;

        public const ushort VKEY_O = 0x4F;
    }


}
