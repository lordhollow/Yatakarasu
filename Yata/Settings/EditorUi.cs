using System;
using System.Windows.Forms;

namespace Yata.Settings
{
    internal static class EditorUi
    {
        public static void Error(IWin32Window owner, Exception ex) => MessageBox.Show(owner, ex.Message, "設定を確認してください", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        internal static bool MoveRow(DataGridView grid, int delta)
        {
            if (!grid.EndEdit() || grid.CurrentRow == null) return false;
            var row = grid.CurrentRow.Index; var target = row + delta;
            if (target < 0 || target >= grid.Rows.Count) return false;
            for (int column = 0; column < grid.Columns.Count; column++)
            {
                var value = grid.Rows[row].Cells[column].Value;
                grid.Rows[row].Cells[column].Value = grid.Rows[target].Cells[column].Value;
                grid.Rows[target].Cells[column].Value = value;
            }
            grid.CurrentCell = grid.Rows[target].Cells[0];
            return true;
        }
    }
}
