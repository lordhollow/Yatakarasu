namespace Yata.Settings
{
    partial class SensorSettingsForm
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
            this.meters = new System.Windows.Forms.DataGridView();
            this.metersNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.metersUnitColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.metersMinColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.metersMaxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.metersIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.selectMeterButton = new System.Windows.Forms.Button();
            this.clearMeterButton = new System.Windows.Forms.Button();
            this.layersButton = new System.Windows.Forms.Button();
            this.upMeterButton = new System.Windows.Forms.Button();
            this.downMeterButton = new System.Windows.Forms.Button();
            this.meterCommands = new System.Windows.Forms.FlowLayoutPanel();
            this.tabs = new System.Windows.Forms.TabControl();
            this.meterTab = new System.Windows.Forms.TabPage();
            this.plot1Tab = new System.Windows.Forms.TabPage();
            this.plot1 = new Yata.Settings.PlotEditor();
            this.plot2Tab = new System.Windows.Forms.TabPage();
            this.plot2 = new Yata.Settings.PlotEditor();
            this.note = new System.Windows.Forms.Label();
            this.Body.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.meters)).BeginInit();
            this.meterCommands.SuspendLayout();
            this.tabs.SuspendLayout();
            this.meterTab.SuspendLayout();
            this.plot1Tab.SuspendLayout();
            this.plot2Tab.SuspendLayout();
            this.SuspendLayout();
            // 
            // Body
            // 
            this.Body.Controls.Add(this.tabs);
            this.Body.Controls.Add(this.note);
            // 
            // meters
            // 
            this.meters.AllowUserToAddRows = false;
            this.meters.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.meters.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.meters.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.meters.BackgroundColor = System.Drawing.Color.White;
            this.meters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.metersNameColumn,
            this.metersUnitColumn,
            this.metersMinColumn,
            this.metersMaxColumn,
            this.metersIdColumn});
            this.meters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.meters.Location = new System.Drawing.Point(3, 49);
            this.meters.MultiSelect = false;
            this.meters.Name = "meters";
            this.meters.RowHeadersVisible = false;
            this.meters.RowTemplate.Height = 27;
            this.meters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.meters.Size = new System.Drawing.Size(1034, 510);
            this.meters.TabIndex = 0;
            // 
            // metersNameColumn
            // 
            this.metersNameColumn.FillWeight = 90F;
            this.metersNameColumn.HeaderText = "表示名（親項目）";
            this.metersNameColumn.Name = "metersNameColumn";
            this.metersNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // metersUnitColumn
            // 
            this.metersUnitColumn.FillWeight = 45F;
            this.metersUnitColumn.HeaderText = "単位";
            this.metersUnitColumn.Name = "metersUnitColumn";
            this.metersUnitColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // metersMinColumn
            // 
            this.metersMinColumn.FillWeight = 45F;
            this.metersMinColumn.HeaderText = "最小";
            this.metersMinColumn.Name = "metersMinColumn";
            this.metersMinColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // metersMaxColumn
            // 
            this.metersMaxColumn.FillWeight = 45F;
            this.metersMaxColumn.HeaderText = "最大";
            this.metersMaxColumn.Name = "metersMaxColumn";
            this.metersMaxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // metersIdColumn
            // 
            this.metersIdColumn.FillWeight = 240F;
            this.metersIdColumn.HeaderText = "センサーID";
            this.metersIdColumn.Name = "metersIdColumn";
            this.metersIdColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // selectMeterButton
            // 
            this.selectMeterButton.AutoSize = true;
            this.selectMeterButton.Location = new System.Drawing.Point(8, 8);
            this.selectMeterButton.Margin = new System.Windows.Forms.Padding(4);
            this.selectMeterButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.selectMeterButton.Name = "selectMeterButton";
            this.selectMeterButton.Size = new System.Drawing.Size(99, 30);
            this.selectMeterButton.TabIndex = 0;
            this.selectMeterButton.Text = "センサーを選択…";
            this.selectMeterButton.UseVisualStyleBackColor = true;
            this.selectMeterButton.Click += new System.EventHandler(this.SelectMeterButton_Click);
            // 
            // clearMeterButton
            // 
            this.clearMeterButton.AutoSize = true;
            this.clearMeterButton.Location = new System.Drawing.Point(115, 8);
            this.clearMeterButton.Margin = new System.Windows.Forms.Padding(4);
            this.clearMeterButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.clearMeterButton.Name = "clearMeterButton";
            this.clearMeterButton.Size = new System.Drawing.Size(90, 30);
            this.clearMeterButton.TabIndex = 1;
            this.clearMeterButton.Text = "選択を解除";
            this.clearMeterButton.UseVisualStyleBackColor = true;
            this.clearMeterButton.Click += new System.EventHandler(this.ClearMeterButton_Click);
            this.layersButton.AutoSize = true;
            this.layersButton.Name = "layersButton";
            this.layersButton.Size = new System.Drawing.Size(100, 30);
            this.layersButton.Text = "層を編集…";
            this.layersButton.Click += new System.EventHandler(this.LayersButton_Click);
            this.upMeterButton.AutoSize = true;
            this.upMeterButton.Name = "upMeterButton";
            this.upMeterButton.Size = new System.Drawing.Size(90, 30);
            this.upMeterButton.Text = "上へ";
            this.upMeterButton.Click += new System.EventHandler(this.UpMeterButton_Click);
            this.downMeterButton.AutoSize = true;
            this.downMeterButton.Name = "downMeterButton";
            this.downMeterButton.Size = new System.Drawing.Size(90, 30);
            this.downMeterButton.Text = "下へ";
            this.downMeterButton.Click += new System.EventHandler(this.DownMeterButton_Click);
            // 
            // meterCommands
            // 
            this.meterCommands.AutoSize = true;
            this.meterCommands.Controls.Add(this.selectMeterButton);
            this.meterCommands.Controls.Add(this.clearMeterButton);
            this.meterCommands.Controls.Add(this.layersButton);
            this.meterCommands.Controls.Add(this.upMeterButton);
            this.meterCommands.Controls.Add(this.downMeterButton);
            this.meterCommands.Dock = System.Windows.Forms.DockStyle.Top;
            this.meterCommands.Location = new System.Drawing.Point(3, 3);
            this.meterCommands.Name = "meterCommands";
            this.meterCommands.Padding = new System.Windows.Forms.Padding(4);
            this.meterCommands.Size = new System.Drawing.Size(1034, 46);
            this.meterCommands.TabIndex = 1;
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.meterTab);
            this.tabs.Controls.Add(this.plot1Tab);
            this.tabs.Controls.Add(this.plot2Tab);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Location = new System.Drawing.Point(8, 8);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(1048, 590);
            this.tabs.TabIndex = 0;
            // 
            // meterTab
            // 
            this.meterTab.Controls.Add(this.meters);
            this.meterTab.Controls.Add(this.meterCommands);
            this.meterTab.Location = new System.Drawing.Point(4, 24);
            this.meterTab.Name = "meterTab";
            this.meterTab.Padding = new System.Windows.Forms.Padding(3);
            this.meterTab.Size = new System.Drawing.Size(1040, 562);
            this.meterTab.TabIndex = 0;
            this.meterTab.Text = "タコメーター（6個）";
            this.meterTab.UseVisualStyleBackColor = true;
            // 
            // plot1Tab
            // 
            this.plot1Tab.Controls.Add(this.plot1);
            this.plot1Tab.Location = new System.Drawing.Point(4, 24);
            this.plot1Tab.Name = "plot1Tab";
            this.plot1Tab.Padding = new System.Windows.Forms.Padding(3);
            this.plot1Tab.Size = new System.Drawing.Size(1040, 562);
            this.plot1Tab.TabIndex = 1;
            this.plot1Tab.Text = "グラフ 1";
            this.plot1Tab.UseVisualStyleBackColor = true;
            // 
            // plot1
            // 
            this.plot1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plot1.Location = new System.Drawing.Point(3, 3);
            this.plot1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.plot1.Name = "plot1";
            this.plot1.Size = new System.Drawing.Size(1034, 556);
            this.plot1.TabIndex = 0;
            this.plot1.SelectRequested += new System.Action(this.Plot1_SelectRequested);
            this.plot1.Changed += new System.Action(this.Plot_Changed);
            // 
            // plot2Tab
            // 
            this.plot2Tab.Controls.Add(this.plot2);
            this.plot2Tab.Location = new System.Drawing.Point(4, 24);
            this.plot2Tab.Name = "plot2Tab";
            this.plot2Tab.Padding = new System.Windows.Forms.Padding(3);
            this.plot2Tab.Size = new System.Drawing.Size(1040, 562);
            this.plot2Tab.TabIndex = 2;
            this.plot2Tab.Text = "グラフ 2";
            this.plot2Tab.UseVisualStyleBackColor = true;
            // 
            // plot2
            // 
            this.plot2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plot2.Location = new System.Drawing.Point(3, 3);
            this.plot2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.plot2.Name = "plot2";
            this.plot2.Size = new System.Drawing.Size(1034, 556);
            this.plot2.TabIndex = 0;
            this.plot2.SelectRequested += new System.Action(this.Plot2_SelectRequested);
            this.plot2.Changed += new System.Action(this.Plot_Changed);
            // 
            // note
            // 
            this.note.AutoSize = true;
            this.note.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.note.Location = new System.Drawing.Point(8, 598);
            this.note.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.note.Name = "note";
            this.note.Size = new System.Drawing.Size(437, 15);
            this.note.TabIndex = 1;
            this.note.Text = "タコメーター6個・グラフ2個。未設定のIDは「-」。センサーIDは平均・合計の式も入力できます。";
            // 
            // SensorSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.ClientSize = new System.Drawing.Size(1064, 721);
            this.Name = "SensorSettingsForm";
            this.Text = "センサー選択・設定";
            this.Body.ResumeLayout(false);
            this.Body.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.meters)).EndInit();
            this.meterCommands.ResumeLayout(false);
            this.meterCommands.PerformLayout();
            this.tabs.ResumeLayout(false);
            this.meterTab.ResumeLayout(false);
            this.meterTab.PerformLayout();
            this.plot1Tab.ResumeLayout(false);
            this.plot2Tab.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.DataGridView meters;
        private System.Windows.Forms.DataGridViewTextBoxColumn metersNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn metersUnitColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn metersMinColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn metersMaxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn metersIdColumn;
        private System.Windows.Forms.Button selectMeterButton;
        private System.Windows.Forms.Button clearMeterButton;
        private System.Windows.Forms.Button layersButton;
        private System.Windows.Forms.Button upMeterButton;
        private System.Windows.Forms.Button downMeterButton;
        private System.Windows.Forms.FlowLayoutPanel meterCommands;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage meterTab;
        private System.Windows.Forms.TabPage plot1Tab;
        private System.Windows.Forms.TabPage plot2Tab;
        private Yata.Settings.PlotEditor plot1;
        private Yata.Settings.PlotEditor plot2;
        private System.Windows.Forms.Label note;
    }
}
