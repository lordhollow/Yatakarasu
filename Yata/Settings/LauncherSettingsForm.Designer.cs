namespace Yata.Settings
{
    partial class LauncherSettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.grid = new System.Windows.Forms.DataGridView();
            this.gridPageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridPageRowColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumnColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridCaptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridCommandColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridOptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.addButton = new System.Windows.Forms.Button();
            this.editButton = new System.Windows.Forms.Button();
            this.duplicateButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.addPageButton = new System.Windows.Forms.Button();
            this.commandsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.status = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            this.grid.Name = "grid";
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.AutoGenerateColumns = false;
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.ReadOnly = true;
            this.grid.RowHeadersVisible = false;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.MultiSelect = false;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.grid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.RowTemplate.Height = 27;
            System.Windows.Forms.DataGridViewCellStyle gridAlternatingStyle = new System.Windows.Forms.DataGridViewCellStyle();
            gridAlternatingStyle.BackColor = System.Drawing.Color.FromArgb(245, 248, 252);
            this.grid.AlternatingRowsDefaultCellStyle = gridAlternatingStyle;
            this.gridPageColumn.Name = "Page";
            this.gridPageColumn.DataPropertyName = "Page";
            this.gridPageColumn.HeaderText = "ページ";
            this.gridPageColumn.FillWeight = 35F;
            this.gridPageColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.gridPageRowColumn.Name = "PageRow";
            this.gridPageRowColumn.DataPropertyName = "PageRow";
            this.gridPageRowColumn.HeaderText = "行 (0/1)";
            this.gridPageRowColumn.FillWeight = 35F;
            this.gridPageRowColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.gridColumnColumn.Name = "Column";
            this.gridColumnColumn.DataPropertyName = "Column";
            this.gridColumnColumn.HeaderText = "列";
            this.gridColumnColumn.FillWeight = 30F;
            this.gridColumnColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.gridCaptionColumn.Name = "Caption";
            this.gridCaptionColumn.DataPropertyName = "Caption";
            this.gridCaptionColumn.HeaderText = "表示名";
            this.gridCaptionColumn.FillWeight = 95F;
            this.gridCaptionColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.gridCommandColumn.Name = "Command";
            this.gridCommandColumn.DataPropertyName = "Command";
            this.gridCommandColumn.HeaderText = "コマンド";
            this.gridCommandColumn.FillWeight = 180F;
            this.gridCommandColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.gridOptionColumn.Name = "Option";
            this.gridOptionColumn.DataPropertyName = "Option";
            this.gridOptionColumn.HeaderText = "起動オプション";
            this.gridOptionColumn.FillWeight = 130F;
            this.gridOptionColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.gridPageColumn, this.gridPageRowColumn, this.gridColumnColumn, this.gridCaptionColumn, this.gridCommandColumn, this.gridOptionColumn });
            this.grid.DoubleClick += new System.EventHandler(this.Grid_DoubleClick);
            this.addButton.Name = "addButton";
            this.addButton.Text = "項目を追加…";
            this.addButton.AutoSize = true;
            this.addButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.addButton.Margin = new System.Windows.Forms.Padding(4);
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.AddButton_Click);
            this.editButton.Name = "editButton";
            this.editButton.Text = "編集…";
            this.editButton.AutoSize = true;
            this.editButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.editButton.Margin = new System.Windows.Forms.Padding(4);
            this.editButton.UseVisualStyleBackColor = true;
            this.editButton.Click += new System.EventHandler(this.EditButton_Click);
            this.duplicateButton.Name = "duplicateButton";
            this.duplicateButton.Text = "複製…";
            this.duplicateButton.AutoSize = true;
            this.duplicateButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.duplicateButton.Margin = new System.Windows.Forms.Padding(4);
            this.duplicateButton.UseVisualStyleBackColor = true;
            this.duplicateButton.Click += new System.EventHandler(this.DuplicateButton_Click);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Text = "削除";
            this.deleteButton.AutoSize = true;
            this.deleteButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.deleteButton.Margin = new System.Windows.Forms.Padding(4);
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            this.addPageButton.Name = "addPageButton";
            this.addPageButton.Text = "ページを追加";
            this.addPageButton.AutoSize = true;
            this.addPageButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.addPageButton.Margin = new System.Windows.Forms.Padding(4);
            this.addPageButton.UseVisualStyleBackColor = true;
            this.addPageButton.Click += new System.EventHandler(this.AddPageButton_Click);
            this.commandsPanel.Name = "commandsPanel";
            this.commandsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.commandsPanel.AutoSize = true;
            this.commandsPanel.WrapContents = true;
            this.commandsPanel.Padding = new System.Windows.Forms.Padding(4);
            this.commandsPanel.Controls.Add(this.addButton);
            this.commandsPanel.Controls.Add(this.editButton);
            this.commandsPanel.Controls.Add(this.duplicateButton);
            this.commandsPanel.Controls.Add(this.deleteButton);
            this.commandsPanel.Controls.Add(this.addPageButton);
            this.status.Name = "status";
            this.status.Text = "1ページ = 2行 × 16列。右クリックメニューからもページを切り替えられます。";
            this.status.AutoSize = true;
            this.status.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.status.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Body.Controls.Add(this.grid);
            this.Body.Controls.Add(this.commandsPanel);
            this.Body.Controls.Add(this.status);
            this.Text = "ランチャー編集";
            this.Name = "LauncherSettingsForm";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridPageColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridPageRowColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumnColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridCaptionColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridCommandColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridOptionColumn;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.Button duplicateButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button addPageButton;
        private System.Windows.Forms.FlowLayoutPanel commandsPanel;
        private System.Windows.Forms.Label status;
    }
}
