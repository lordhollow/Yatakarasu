using System;
using System.Windows.Forms;

namespace Yata.Settings
{
    internal static class EditorUi
    {
        public static void Error(IWin32Window owner, Exception ex) => MessageBox.Show(owner, ex.Message, "設定を確認してください", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
