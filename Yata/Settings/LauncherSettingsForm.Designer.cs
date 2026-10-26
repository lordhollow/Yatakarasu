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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.Body.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.commandsPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // Body
            //
            this.Body.Controls.Add(this.grid);
            this.Body.Controls.Add(this.commandsPanel);
            this.Body.Controls.Add(this.status);
            //
            // grid
            //
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.grid.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.Color.White;
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(8, 54);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.gridPageColumn,
            this.gridPageRowColumn,
            this.gridColumnColumn,
            this.gridCaptionColumn,
            this.gridCommandColumn,
            this.gridOptionColumn});
            this.gridPageColumn.HeaderText = "ページ";
            this.gridPageColumn.Name = "gridPageColumn";
            this.gridPageColumn.FillWeight = 35F;
            this.gridPageColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.gridPageRowColumn.HeaderText = "行 (0/1)";
            this.gridPageRowColumn.Name = "gridPageRowColumn";
            this.gridPageRowColumn.FillWeight = 35F;
            this.gridPageRowColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.gridColumnColumn.HeaderText = "列";
            this.gridColumnColumn.Name = "gridColumnColumn";
            this.gridColumnColumn.FillWeight = 30F;
            this.gridColumnColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.gridCaptionColumn.HeaderText = "表示名";
            this.gridCaptionColumn.Name = "gridCaptionColumn";
            this.gridCaptionColumn.FillWeight = 95F;
            this.gridCaptionColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.gridCommandColumn.HeaderText = "コマンド";
            this.gridCommandColumn.Name = "gridCommandColumn";
            this.gridCommandColumn.FillWeight = 180F;
            this.gridCommandColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.gridOptionColumn.HeaderText = "起動オプション";
            this.gridOptionColumn.Name = "gridOptionColumn";
            this.gridOptionColumn.FillWeight = 130F;
            this.gridOptionColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.grid.ReadOnly = true;
            this.grid.RowHeadersVisible = false;
            this.grid.RowTemplate.Height = 27;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(1048, 544);
            this.grid.TabIndex = 0;
            this.grid.DoubleClick += new System.EventHandler(this.Grid_DoubleClick);
            //
            // addButton
            //
            this.addButton.AutoSize = true;
            this.addButton.Location = new System.Drawing.Point(8, 8);
            this.addButton.Margin = new System.Windows.Forms.Padding(4);
            this.addButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(90, 30);
            this.addButton.TabIndex = 0;
            this.addButton.Text = "項目を追加…";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.AddButton_Click);
            //
            // editButton
            //
            this.editButton.AutoSize = true;
            this.editButton.Location = new System.Drawing.Point(106, 8);
            this.editButton.Margin = new System.Windows.Forms.Padding(4);
            this.editButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(90, 30);
            this.editButton.TabIndex = 1;
            this.editButton.Text = "編集…";
            this.editButton.UseVisualStyleBackColor = true;
            this.editButton.Click += new System.EventHandler(this.EditButton_Click);
            //
            // duplicateButton
            //
            this.duplicateButton.AutoSize = true;
            this.duplicateButton.Location = new System.Drawing.Point(204, 8);
            this.duplicateButton.Margin = new System.Windows.Forms.Padding(4);
            this.duplicateButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.duplicateButton.Name = "duplicateButton";
            this.duplicateButton.Size = new System.Drawing.Size(90, 30);
            this.duplicateButton.TabIndex = 2;
            this.duplicateButton.Text = "複製…";
            this.duplicateButton.UseVisualStyleBackColor = true;
            this.duplicateButton.Click += new System.EventHandler(this.DuplicateButton_Click);
            //
            // deleteButton
            //
            this.deleteButton.AutoSize = true;
            this.deleteButton.Location = new System.Drawing.Point(302, 8);
            this.deleteButton.Margin = new System.Windows.Forms.Padding(4);
            this.deleteButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(90, 30);
            this.deleteButton.TabIndex = 3;
            this.deleteButton.Text = "削除";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            //
            // addPageButton
            //
            this.addPageButton.AutoSize = true;
            this.addPageButton.Location = new System.Drawing.Point(400, 8);
            this.addPageButton.Margin = new System.Windows.Forms.Padding(4);
            this.addPageButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.addPageButton.Name = "addPageButton";
            this.addPageButton.Size = new System.Drawing.Size(90, 30);
            this.addPageButton.TabIndex = 4;
            this.addPageButton.Text = "ページを追加";
            this.addPageButton.UseVisualStyleBackColor = true;
            this.addPageButton.Click += new System.EventHandler(this.AddPageButton_Click);
            //
            // commandsPanel
            //
            this.commandsPanel.AutoSize = true;
            this.commandsPanel.Controls.Add(this.addButton);
            this.commandsPanel.Controls.Add(this.editButton);
            this.commandsPanel.Controls.Add(this.duplicateButton);
            this.commandsPanel.Controls.Add(this.deleteButton);
            this.commandsPanel.Controls.Add(this.addPageButton);
            this.commandsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.commandsPanel.Location = new System.Drawing.Point(8, 8);
            this.commandsPanel.Name = "commandsPanel";
            this.commandsPanel.Padding = new System.Windows.Forms.Padding(4);
            this.commandsPanel.Size = new System.Drawing.Size(1048, 46);
            this.commandsPanel.TabIndex = 1;
            //
            // status
            //
            this.status.AutoSize = true;
            this.status.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.status.Location = new System.Drawing.Point(8, 598);
            this.status.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(351, 15);
            this.status.TabIndex = 2;
            this.status.Text = "1ページ = 2行 × 16列。右クリックメニューからもページを切り替えられます。";
            //
            // LauncherSettingsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.ClientSize = new System.Drawing.Size(1064, 721);
            this.Name = "LauncherSettingsForm";
            this.Text = "ランチャー編集";
            this.Body.ResumeLayout(false);
            this.Body.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.commandsPanel.ResumeLayout(false);
            this.commandsPanel.PerformLayout();
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
