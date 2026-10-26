namespace Yata.Settings
{
    partial class IconPickerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeCandidates();
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.images = new System.Windows.Forms.ImageList(this.components);
            this.list = new System.Windows.Forms.ListView();
            this.status = new System.Windows.Forms.Label();
            this.addFileButton = new System.Windows.Forms.Button();
            this.commandsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.selectButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.footerPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.ClientSize = new System.Drawing.Size(744, 501);
            this.MinimumSize = new System.Drawing.Size(540, 360);

            this.SuspendLayout();
            this.images.ImageSize = new System.Drawing.Size(48, 48);
            this.images.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.list.Name = "list";
            this.list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list.View = System.Windows.Forms.View.LargeIcon;
            this.list.MultiSelect = false;
            this.list.HideSelection = false;
            this.list.ShowItemToolTips = true;
            this.list.UseCompatibleStateImageBehavior = false;
            this.list.LargeImageList = this.images;
            this.list.DoubleClick += new System.EventHandler(this.List_DoubleClick);
            this.status.Name = "status";
            this.status.Text = "Exe・DLL内のアイコン、ico・png・jpg画像から選べます。";
            this.status.AutoSize = true;
            this.status.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.addFileButton.Name = "addFileButton";
            this.addFileButton.Text = "ファイルを追加…";
            this.addFileButton.AutoSize = true;
            this.addFileButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.addFileButton.Margin = new System.Windows.Forms.Padding(4);
            this.addFileButton.UseVisualStyleBackColor = true;
            this.addFileButton.Click += new System.EventHandler(this.AddFileButton_Click);
            this.commandsPanel.Name = "commandsPanel";
            this.commandsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.commandsPanel.AutoSize = true;
            this.commandsPanel.WrapContents = true;
            this.commandsPanel.Padding = new System.Windows.Forms.Padding(4);
            this.commandsPanel.Controls.Add(this.addFileButton);
            this.commandsPanel.Controls.Add(this.status);
            this.selectButton.Name = "selectButton";
            this.selectButton.Text = "このアイコンを使う";
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
            this.Controls.Add(this.commandsPanel);
            this.Controls.Add(this.footerPanel);
            this.AcceptButton = this.selectButton;
            this.CancelButton = this.cancelButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = System.Drawing.SystemFonts.MessageBoxFont;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Name = "IconPickerForm";
            this.Text = "アイコンを選択";

            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.ImageList images;
        private System.Windows.Forms.ListView list;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.Button addFileButton;
        private System.Windows.Forms.FlowLayoutPanel commandsPanel;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.FlowLayoutPanel footerPanel;
    }
}
