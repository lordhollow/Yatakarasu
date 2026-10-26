namespace Yata.Settings
{
    partial class SensorPickerForm
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
            this.list = new System.Windows.Forms.ListBox();
            this.filter = new System.Windows.Forms.TextBox();
            this.filterLabel = new System.Windows.Forms.Label();
            this.searchPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.selectButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.footerPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.ClientSize = new System.Drawing.Size(864, 501);
            this.MinimumSize = new System.Drawing.Size(650, 400);

            this.SuspendLayout();
            this.list.Name = "list";
            this.list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list.HorizontalScrollbar = true;
            this.list.IntegralHeight = false;
            this.list.DoubleClick += new System.EventHandler(this.List_DoubleClick);
            this.filter.Name = "filter";
            this.filter.Width = 480;
            this.filter.TextChanged += new System.EventHandler(this.Filter_TextChanged);
            this.filterLabel.Name = "filterLabel";
            this.filterLabel.Text = "名前・種類・IDで検索";
            this.filterLabel.AutoSize = true;
            this.filterLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchPanel.AutoSize = true;
            this.searchPanel.WrapContents = true;
            this.searchPanel.Padding = new System.Windows.Forms.Padding(4);
            this.searchPanel.Controls.Add(this.filterLabel);
            this.searchPanel.Controls.Add(this.filter);
            this.selectButton.Name = "selectButton";
            this.selectButton.Text = "選択";
            this.selectButton.AutoSize = true;
            this.selectButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.selectButton.Margin = new System.Windows.Forms.Padding(4);
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.SelectButton_Click);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.AutoSize = true;
            this.cancelButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footerPanel.AutoSize = true;
            this.footerPanel.WrapContents = true;
            this.footerPanel.Padding = new System.Windows.Forms.Padding(4);
            this.footerPanel.Controls.Add(this.selectButton);
            this.footerPanel.Controls.Add(this.cancelButton);
            this.Controls.Add(this.list);
            this.Controls.Add(this.searchPanel);
            this.Controls.Add(this.footerPanel);
            this.AcceptButton = this.selectButton;
            this.CancelButton = this.cancelButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = System.Drawing.SystemFonts.MessageBoxFont;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Name = "SensorPickerForm";
            this.Text = "センサーを選択";

            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.ListBox list;
        private System.Windows.Forms.TextBox filter;
        private System.Windows.Forms.Label filterLabel;
        private System.Windows.Forms.FlowLayoutPanel searchPanel;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.FlowLayoutPanel footerPanel;
    }
}
