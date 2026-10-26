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
            this.components = new System.ComponentModel.Container();
            this.meters = new System.Windows.Forms.DataGridView();
            this.metersNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.metersUnitColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.metersMinColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.metersMaxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.metersIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.selectMeterButton = new System.Windows.Forms.Button();
            this.clearMeterButton = new System.Windows.Forms.Button();
            this.meterCommands = new System.Windows.Forms.FlowLayoutPanel();
            this.tabs = new System.Windows.Forms.TabControl();
            this.meterTab = new System.Windows.Forms.TabPage();
            this.plot1Tab = new System.Windows.Forms.TabPage();
            this.plot2Tab = new System.Windows.Forms.TabPage();
            this.plot1 = new Yata.Settings.PlotEditor();
            this.plot2 = new Yata.Settings.PlotEditor();
            this.note = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.meters)).BeginInit();
            this.SuspendLayout();
            this.meters.Name = "meters";
            this.meters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.meters.AutoGenerateColumns = false;
            this.meters.AllowUserToAddRows = false;
            this.meters.AllowUserToDeleteRows = false;
            this.meters.ReadOnly = false;
            this.meters.RowHeadersVisible = false;
            this.meters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.meters.MultiSelect = false;
            this.meters.BackgroundColor = System.Drawing.Color.White;
            this.meters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.meters.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.meters.RowTemplate.Height = 27;
            System.Windows.Forms.DataGridViewCellStyle metersAlternatingStyle = new System.Windows.Forms.DataGridViewCellStyle();
            metersAlternatingStyle.BackColor = System.Drawing.Color.FromArgb(245, 248, 252);
            this.meters.AlternatingRowsDefaultCellStyle = metersAlternatingStyle;
            this.metersNameColumn.Name = "Name";
            this.metersNameColumn.DataPropertyName = "Name";
            this.metersNameColumn.HeaderText = "表示名";
            this.metersNameColumn.FillWeight = 90F;
            this.metersNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.metersUnitColumn.Name = "Unit";
            this.metersUnitColumn.DataPropertyName = "Unit";
            this.metersUnitColumn.HeaderText = "単位";
            this.metersUnitColumn.FillWeight = 45F;
            this.metersUnitColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.metersMinColumn.Name = "Min";
            this.metersMinColumn.DataPropertyName = "Min";
            this.metersMinColumn.HeaderText = "最小";
            this.metersMinColumn.FillWeight = 45F;
            this.metersMinColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.metersMaxColumn.Name = "Max";
            this.metersMaxColumn.DataPropertyName = "Max";
            this.metersMaxColumn.HeaderText = "最大";
            this.metersMaxColumn.FillWeight = 45F;
            this.metersMaxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.metersIdColumn.Name = "Id";
            this.metersIdColumn.DataPropertyName = "Id";
            this.metersIdColumn.HeaderText = "センサーID";
            this.metersIdColumn.FillWeight = 240F;
            this.metersIdColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.meters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.metersNameColumn, this.metersUnitColumn, this.metersMinColumn, this.metersMaxColumn, this.metersIdColumn });
            this.selectMeterButton.Name = "selectMeterButton";
            this.selectMeterButton.Text = "センサーを選択…";
            this.selectMeterButton.AutoSize = true;
            this.selectMeterButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.selectMeterButton.Margin = new System.Windows.Forms.Padding(4);
            this.selectMeterButton.UseVisualStyleBackColor = true;
            this.selectMeterButton.Click += new System.EventHandler(this.SelectMeterButton_Click);
            this.clearMeterButton.Name = "clearMeterButton";
            this.clearMeterButton.Text = "選択を解除";
            this.clearMeterButton.AutoSize = true;
            this.clearMeterButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.clearMeterButton.Margin = new System.Windows.Forms.Padding(4);
            this.clearMeterButton.UseVisualStyleBackColor = true;
            this.clearMeterButton.Click += new System.EventHandler(this.ClearMeterButton_Click);
            this.meterCommands.Name = "meterCommands";
            this.meterCommands.Dock = System.Windows.Forms.DockStyle.Top;
            this.meterCommands.AutoSize = true;
            this.meterCommands.WrapContents = true;
            this.meterCommands.Padding = new System.Windows.Forms.Padding(4);
            this.meterCommands.Controls.Add(this.selectMeterButton);
            this.meterCommands.Controls.Add(this.clearMeterButton);
            this.tabs.Name = "tabs";
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.SelectedIndex = 0;
            this.meterTab.Name = "meterTab";
            this.meterTab.Text = "タコメーター（6個）";
            this.meterTab.UseVisualStyleBackColor = true;
            this.meterTab.Padding = new System.Windows.Forms.Padding(3);
            this.plot1Tab.Name = "plot1Tab";
            this.plot1Tab.Text = "グラフ 1";
            this.plot1Tab.UseVisualStyleBackColor = true;
            this.plot1Tab.Padding = new System.Windows.Forms.Padding(3);
            this.plot2Tab.Name = "plot2Tab";
            this.plot2Tab.Text = "グラフ 2";
            this.plot2Tab.UseVisualStyleBackColor = true;
            this.plot2Tab.Padding = new System.Windows.Forms.Padding(3);
            this.meterTab.Controls.Add(this.meters);
            this.meterTab.Controls.Add(this.meterCommands);
            this.plot1.Name = "plot1";
            this.plot1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plot1.SelectRequested += new System.Action(this.Plot1_SelectRequested);
            this.plot1.Changed += new System.Action(this.Plot_Changed);
            this.plot1Tab.Controls.Add(this.plot1);
            this.plot2.Name = "plot2";
            this.plot2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plot2.SelectRequested += new System.Action(this.Plot2_SelectRequested);
            this.plot2.Changed += new System.Action(this.Plot_Changed);
            this.plot2Tab.Controls.Add(this.plot2);
            this.tabs.Controls.Add(this.meterTab);
            this.tabs.Controls.Add(this.plot1Tab);
            this.tabs.Controls.Add(this.plot2Tab);
            this.note.Name = "note";
            this.note.Text = "タコメーター6個・グラフ2個。未設定のIDは「-」。センサーIDは平均・合計の式も入力できます。";
            this.note.AutoSize = true;
            this.note.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.note.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Body.Controls.Add(this.tabs);
            this.Body.Controls.Add(this.note);
            this.Text = "センサー選択・設定";
            this.Name = "SensorSettingsForm";
            ((System.ComponentModel.ISupportInitialize)(this.meters)).EndInit();
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
