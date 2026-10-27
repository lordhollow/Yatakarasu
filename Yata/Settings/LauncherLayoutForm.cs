using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Yata.Components.Configuration;
using Yata.Components.Widget;

namespace Yata.Settings
{
    internal sealed partial class LauncherLayoutForm : Form
    {
        readonly List<LauncherEntry> entries = new List<LauncherEntry>();
        readonly Dictionary<int, Bitmap> icons = new Dictionary<int, Bitmap>();
        int page, pageCount = 1, dragIndex = -1, hoverDirection;
        Point dragStart;
        readonly string dragToken = Guid.NewGuid().ToString("N") + ":";
        internal List<LauncherEntry> ResultEntries { get; private set; }
        internal int CurrentPage => page + 1;
        internal int PageCount => pageCount;
        public LauncherLayoutForm() { InitializeComponent(); }
        internal LauncherLayoutForm(IEnumerable<LauncherEntry> source, int initialPage = 1) : this()
        {
            entries.AddRange(source.Select(entry => entry.Clone()));
            var definition = new LauncherDefinition(); definition.Entries.AddRange(entries); definition.Validate();
            pageCount = entries.Count == 0 ? 1 : entries.Max(entry => entry.Page);
            page = Math.Max(0, Math.Min(pageCount - 1, initialPage - 1));
            grid.Rows.Add(2);
            for (int row = 0; row < 2; row++) { grid.Rows[row].Height = 118; grid.Rows[row].HeaderCell.Value = row.ToString(); }
            UpdatePage();
        }
        internal List<LauncherEntry> ReadEntries() => entries.Select(entry => entry.Clone()).ToList();
        internal bool MoveEntry(int index, int targetPage, int targetRow, int targetColumn)
        {
            if (index < 0 || index >= entries.Count || targetPage < 1 || targetPage > pageCount || targetRow < 0 || targetRow > 1 || targetColumn < 0 || targetColumn >= 16) return false;
            var source = entries[index]; int row = (targetPage - 1) * 2 + targetRow;
            if (source.Row == row && source.Column == targetColumn) return false;
            var occupied = entries.FirstOrDefault(entry => entry.Row == row && entry.Column == targetColumn);
            if (occupied != null) { occupied.Row = source.Row; occupied.Column = source.Column; }
            source.Row = row; source.Column = targetColumn;
            grid.Invalidate(); return true;
        }
        internal void ChangePage(int delta)
        {
            page = ((page + delta) % pageCount + pageCount) % pageCount;
            UpdatePage();
        }
        internal bool AddPage()
        {
            if (pageCount >= 1000) return false;
            entries.AddRange(LauncherDefinition.PageNavigation(pageCount));
            page = pageCount++; UpdatePage(); return true;
        }
        void UpdatePage()
        {
            pageLabel.Text = $"ページ {page + 1} / {pageCount}";
            previousButton.Enabled = nextButton.Enabled = pageCount > 1;
            grid.ClearSelection(); grid.Invalidate();
        }
        void PreviousButton_Click(object sender, EventArgs e) => ChangePage(-1);
        void NextButton_Click(object sender, EventArgs e) => ChangePage(1);
        void AddPageButton_Click(object sender, EventArgs e)
        {
            if (!AddPage()) MessageBox.Show(this, "1000ページまで追加できます。");
        }
        void OkButton_Click(object sender, EventArgs e)
        {
            ResultEntries = ReadEntries(); DialogResult = DialogResult.OK;
        }
        int IndexAt(int row, int column) => entries.FindIndex(entry => entry.Row == page * 2 + row && entry.Column == column);
        Bitmap IconFor(int index)
        {
            Bitmap icon;
            if (icons.TryGetValue(index, out icon)) return icon;
            var entry = entries[index];
            if (!string.IsNullOrWhiteSpace(entry.IconPath))
            {
                try
                {
                    Rectangle atlas;
                    var source = CommonResource.LoadImage(entry.IconPath, out atlas);
                    if (source != null)
                    {
                        try
                        {
                            icon = new Bitmap(48, 48, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                            using (var graphics = Graphics.FromImage(icon)) graphics.DrawImage(source, new Rectangle(0, 0, 48, 48), atlas.IsEmpty ? new Rectangle(0, 0, source.Width, source.Height) : atlas, GraphicsUnit.Pixel);
                        }
                        finally { if (!ReferenceEquals(source, CommonResource.InternalApplicationIconsImage)) source.Dispose(); }
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is ArgumentException || ex is UnauthorizedAccessException)
                { icon?.Dispose(); icon = null; }
            }
            if (icon == null)
            {
                if (entry.Command == "internal::LauncherPreviousPage" || entry.Command == "internal::LauncherNextPage") icon = IconLibrary.NavigationIcon(entry.Command.EndsWith("NextPage"));
                else using (var image = SystemIcons.Application.ToBitmap()) icon = CommonResource.CreateDrawingBitmap(image);
            }
            icons.Add(index, icon); return icon;
        }
        void Grid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            e.Paint(e.ClipBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground | DataGridViewPaintParts.Border);
            int index = IndexAt(e.RowIndex, e.ColumnIndex);
            if (index >= 0)
            {
                int size = Math.Min(48, e.CellBounds.Width - 12);
                e.Graphics.DrawImage(IconFor(index), new Rectangle(e.CellBounds.X + (e.CellBounds.Width - size) / 2, e.CellBounds.Y + 12, size, size));
                var text = new Rectangle(e.CellBounds.X + 3, e.CellBounds.Y + 66, e.CellBounds.Width - 6, 36);
                TextRenderer.DrawText(e.Graphics, entries[index].Caption, grid.Font, text, Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            }
            e.Handled = true;
        }
        void Grid_MouseDown(object sender, MouseEventArgs e)
        {
            dragIndex = -1;
            if (e.Button != MouseButtons.Left) return;
            var hit = grid.HitTest(e.X, e.Y);
            if (hit.RowIndex < 0 || hit.ColumnIndex < 0) return;
            dragIndex = IndexAt(hit.RowIndex, hit.ColumnIndex); dragStart = e.Location;
        }
        void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || dragIndex < 0) return;
            var threshold = new Rectangle(dragStart.X - SystemInformation.DragSize.Width / 2, dragStart.Y - SystemInformation.DragSize.Height / 2, SystemInformation.DragSize.Width, SystemInformation.DragSize.Height);
            if (threshold.Contains(e.Location)) return;
            try { grid.DoDragDrop(CreateDragData(dragIndex), DragDropEffects.Move); }
            finally { dragIndex = -1; pageHoverTimer.Stop(); }
        }
        internal IDataObject CreateDragData(int index) => new DataObject(DataFormats.UnicodeText, dragToken + index.ToString(System.Globalization.CultureInfo.InvariantCulture));
        int ReadDrag(IDataObject data)
        {
            var text = data?.GetData(DataFormats.UnicodeText) as string;
            int index;
            return text != null && text.StartsWith(dragToken, StringComparison.Ordinal) && int.TryParse(text.Substring(dragToken.Length), out index) && index >= 0 && index < entries.Count ? index : -1;
        }
        void Grid_DragOver(object sender, DragEventArgs e)
        {
            pageHoverTimer.Stop();
            var position = grid.PointToClient(new Point(e.X, e.Y)); var hit = grid.HitTest(position.X, position.Y);
            e.Effect = ReadDrag(e.Data) >= 0 && hit.RowIndex >= 0 && hit.ColumnIndex >= 0 ? DragDropEffects.Move : DragDropEffects.None;
        }
        void Grid_DragDrop(object sender, DragEventArgs e)
        {
            pageHoverTimer.Stop();
            var index = ReadDrag(e.Data); if (index < 0) return;
            var position = grid.PointToClient(new Point(e.X, e.Y)); var hit = grid.HitTest(position.X, position.Y);
            if (MoveEntry(index, CurrentPage, hit.RowIndex, hit.ColumnIndex)) grid.CurrentCell = grid.Rows[hit.RowIndex].Cells[hit.ColumnIndex];
        }
        void PageButton_DragEnter(object sender, DragEventArgs e)
        {
            if (ReadDrag(e.Data) < 0) { e.Effect = DragDropEffects.None; return; }
            e.Effect = DragDropEffects.Move;
            hoverDirection = ReferenceEquals(sender, previousButton) ? -1 : 1;
            pageHoverTimer.Start();
        }
        void PageButton_DragOver(object sender, DragEventArgs e) => e.Effect = ReadDrag(e.Data) >= 0 ? DragDropEffects.Move : DragDropEffects.None;
        void PageButton_DragLeave(object sender, EventArgs e) => pageHoverTimer.Stop();
        void PageHoverTimer_Tick(object sender, EventArgs e) => ChangePage(hoverDirection);
        void ReleaseIcons() { foreach (var icon in icons.Values) icon.Dispose(); icons.Clear(); }
    }
}
