using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yata.Components.Application
{
    /// <summary>
    /// スクリーンキーボードの表示を切り替える
    /// </summary>
    /// <remarks>
    /// キーボード表示ショートカットである CTRL+WIN+O を送る。
    /// </remarks>
    internal class ToggleScreenKeyboard : KeyStroke, IInternalApplication
    {
        public void Execute(Karasu karasu, string option)
        {
            SendInput(KeyDown(VKEY_CTRL), KeyDown(VKEY_LWIN), KeyDown(VKEY_O),
                      KeyUp(VKEY_CTRL), KeyUp(VKEY_LWIN), KeyUp(VKEY_O));
        }
    }
}
