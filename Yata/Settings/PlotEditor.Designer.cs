namespace Yata.Settings
{
    partial class PlotEditor
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
            this.series = new System.Windows.Forms.DataGridView();

            this.seriesNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.seriesIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.seriesColorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.captionLabel = new System.Windows.Forms.Label();
            this.caption = new System.Windows.Forms.TextBox();
            this.unitLabel = new System.Windows.Forms.Label();
            this.unit = new System.Windows.Forms.TextBox();
            this.minLabel = new System.Windows.Forms.Label();
            this.min = new System.Windows.Forms.TextBox();
            this.maxLabel = new System.Windows.Forms.Label();
            this.max = new System.Windows.Forms.TextBox();
            this.lowLabel = new System.Windows.Forms.Label();
            this.low = new System.Windows.Forms.TextBox();
            this.highLabel = new System.Windows.Forms.Label();
            this.high = new System.Windows.Forms.TextBox();
            this.propertiesPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.addButton = new System.Windows.Forms.Button();
            this.selectButton = new System.Windows.Forms.Button();
            this.colorButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.commandsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.Size = new System.Drawing.Size(1000, 550);
            ((System.ComponentModel.ISupportInitialize)(this.series)).BeginInit();
            this.SuspendLayout();
            this.series.Name = "series";
            this.series.Dock = System.Windows.Forms.DockStyle.Fill;
            this.series.AutoGenerateColumns = false;
            this.series.AllowUserToAddRows = false;
            this.series.AllowUserToDeleteRows = false;
            this.series.ReadOnly = false;
            this.series.RowHeadersVisible = false;
            this.series.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.series.MultiSelect = false;
            this.series.BackgroundColor = System.Drawing.Color.White;
            this.series.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.series.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.series.RowTemplate.Height = 27;
            System.Windows.Forms.DataGridViewCellStyle seriesAlternatingStyle = new System.Windows.Forms.DataGridViewCellStyle();
            seriesAlternatingStyle.BackColor = System.Drawing.Color.FromArgb(245, 248, 252);
            this.series.AlternatingRowsDefaultCellStyle = seriesAlternatingStyle;
            this.seriesNameColumn.Name = "seriesNameColumn";
            this.seriesNameColumn.DataPropertyName = "Name";
            this.seriesNameColumn.HeaderText = "表示名";
            this.seriesNameColumn.FillWeight = 90F;
            this.seriesNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.seriesIdColumn.Name = "seriesIdColumn";
            this.seriesIdColumn.DataPropertyName = "Id";
            this.seriesIdColumn.HeaderText = "センサーID";
            this.seriesIdColumn.FillWeight = 230F;
            this.seriesIdColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.seriesColorColumn.Name = "seriesColorColumn";
            this.seriesColorColumn.DataPropertyName = "Color";
            this.seriesColorColumn.HeaderText = "色 R,G,B[,A]";
            this.seriesColorColumn.FillWeight = 80F;
            this.seriesColorColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.series.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.seriesNameColumn, this.seriesIdColumn, this.seriesColorColumn });
            this.captionLabel.Name = "captionLabel";
            this.captionLabel.Text = "タイトル";
            this.captionLabel.AutoSize = true;
            this.captionLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.caption.Name = "caption";
            this.caption.Width = 120;
            this.unitLabel.Name = "unitLabel";
            this.unitLabel.Text = "単位";
            this.unitLabel.AutoSize = true;
            this.unitLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.unit.Name = "unit";
            this.unit.Width = 50;
            this.minLabel.Name = "minLabel";
            this.minLabel.Text = "最小";
            this.minLabel.AutoSize = true;
            this.minLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.min.Name = "min";
            this.min.Width = 65;
            this.maxLabel.Name = "maxLabel";
            this.maxLabel.Text = "最大";
            this.maxLabel.AutoSize = true;
            this.maxLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.max.Name = "max";
            this.max.Width = 65;
            this.lowLabel.Name = "lowLabel";
            this.lowLabel.Text = "下境界";
            this.lowLabel.AutoSize = true;
            this.lowLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.low.Name = "low";
            this.low.Width = 65;
            this.highLabel.Name = "highLabel";
            this.highLabel.Text = "上境界";
            this.highLabel.AutoSize = true;
            this.highLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.high.Name = "high";
            this.high.Width = 65;
            this.propertiesPanel.Name = "propertiesPanel";
            this.propertiesPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.propertiesPanel.AutoSize = true;
            this.propertiesPanel.WrapContents = true;
            this.propertiesPanel.Padding = new System.Windows.Forms.Padding(4);
            this.propertiesPanel.Controls.Add(this.captionLabel);
            this.propertiesPanel.Controls.Add(this.caption);
            this.propertiesPanel.Controls.Add(this.unitLabel);
            this.propertiesPanel.Controls.Add(this.unit);
            this.propertiesPanel.Controls.Add(this.minLabel);
            this.propertiesPanel.Controls.Add(this.min);
            this.propertiesPanel.Controls.Add(this.maxLabel);
            this.propertiesPanel.Controls.Add(this.max);
            this.propertiesPanel.Controls.Add(this.lowLabel);
            this.propertiesPanel.Controls.Add(this.low);
            this.propertiesPanel.Controls.Add(this.highLabel);
            this.propertiesPanel.Controls.Add(this.high);
            this.addButton.Name = "addButton";
            this.addButton.Text = "項目を追加";
            this.addButton.AutoSize = true;
            this.addButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.addButton.Margin = new System.Windows.Forms.Padding(4);
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.AddButton_Click);
            this.selectButton.Name = "selectButton";
            this.selectButton.Text = "センサーを選択…";
            this.selectButton.AutoSize = true;
            this.selectButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.selectButton.Margin = new System.Windows.Forms.Padding(4);
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.SelectButton_Click);
            this.colorButton.Name = "colorButton";
            this.colorButton.Text = "色を選択…";
            this.colorButton.AutoSize = true;
            this.colorButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.colorButton.Margin = new System.Windows.Forms.Padding(4);
            this.colorButton.UseVisualStyleBackColor = true;
            this.colorButton.Click += new System.EventHandler(this.ColorButton_Click);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Text = "項目を削除";
            this.deleteButton.AutoSize = true;
            this.deleteButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.deleteButton.Margin = new System.Windows.Forms.Padding(4);
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            this.commandsPanel.Name = "commandsPanel";
            this.commandsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.commandsPanel.AutoSize = true;
            this.commandsPanel.WrapContents = true;
            this.commandsPanel.Padding = new System.Windows.Forms.Padding(4);
            this.commandsPanel.Controls.Add(this.addButton);
            this.commandsPanel.Controls.Add(this.selectButton);
            this.commandsPanel.Controls.Add(this.colorButton);
            this.commandsPanel.Controls.Add(this.deleteButton);
            this.Controls.Add(this.series);
            this.Controls.Add(this.commandsPanel);
            this.Controls.Add(this.propertiesPanel);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "PlotEditor";
            ((System.ComponentModel.ISupportInitialize)(this.series)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.DataGridView series;
        private System.Windows.Forms.DataGridViewTextBoxColumn seriesNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn seriesIdColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn seriesColorColumn;
        private System.Windows.Forms.Label captionLabel;
        private System.Windows.Forms.TextBox caption;
        private System.Windows.Forms.Label unitLabel;
        private System.Windows.Forms.TextBox unit;
        private System.Windows.Forms.Label minLabel;
        private System.Windows.Forms.TextBox min;
        private System.Windows.Forms.Label maxLabel;
        private System.Windows.Forms.TextBox max;
        private System.Windows.Forms.Label lowLabel;
        private System.Windows.Forms.TextBox low;
        private System.Windows.Forms.Label highLabel;
        private System.Windows.Forms.TextBox high;
        private System.Windows.Forms.FlowLayoutPanel propertiesPanel;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Button colorButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.FlowLayoutPanel commandsPanel;
    }
}
