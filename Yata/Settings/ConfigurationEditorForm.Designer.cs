namespace Yata.Settings
{
    partial class ConfigurationEditorForm
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
            this.headerLayout = new System.Windows.Forms.TableLayoutPanel();
            this.fileLabel = new System.Windows.Forms.Label();
            this.file = new System.Windows.Forms.TextBox();
            this.openButton = new System.Windows.Forms.Button();
            this.destinationButton = new System.Windows.Forms.Button();
            this.Body = new System.Windows.Forms.Panel();
            this.footerPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.headerLayout.SuspendLayout();
            this.footerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // headerLayout
            // 
            this.headerLayout.AutoSize = true;
            this.headerLayout.ColumnCount = 4;
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.headerLayout.Controls.Add(this.fileLabel, 0, 0);
            this.headerLayout.Controls.Add(this.file, 1, 0);
            this.headerLayout.Controls.Add(this.openButton, 2, 0);
            this.headerLayout.Controls.Add(this.destinationButton, 3, 0);
            this.headerLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerLayout.Location = new System.Drawing.Point(0, 0);
            this.headerLayout.Name = "headerLayout";
            this.headerLayout.Padding = new System.Windows.Forms.Padding(8);
            this.headerLayout.RowCount = 1;
            this.headerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.headerLayout.Size = new System.Drawing.Size(1064, 54);
            this.headerLayout.TabIndex = 0;
            // 
            // fileLabel
            // 
            this.fileLabel.AutoSize = true;
            this.fileLabel.Location = new System.Drawing.Point(13, 17);
            this.fileLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.fileLabel.Name = "fileLabel";
            this.fileLabel.Size = new System.Drawing.Size(65, 15);
            this.fileLabel.TabIndex = 0;
            this.fileLabel.Text = "設定ファイル";
            // 
            // file
            // 
            this.file.Dock = System.Windows.Forms.DockStyle.Fill;
            this.file.Location = new System.Drawing.Point(88, 16);
            this.file.Margin = new System.Windows.Forms.Padding(5, 8, 5, 5);
            this.file.Name = "file";
            this.file.ReadOnly = true;
            this.file.Size = new System.Drawing.Size(761, 23);
            this.file.TabIndex = 1;
            this.file.TabStop = false;
            // 
            // openButton
            // 
            this.openButton.AutoSize = true;
            this.openButton.Location = new System.Drawing.Point(858, 12);
            this.openButton.Margin = new System.Windows.Forms.Padding(4);
            this.openButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(90, 30);
            this.openButton.TabIndex = 2;
            this.openButton.Text = "開く…";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // destinationButton
            // 
            this.destinationButton.AutoSize = true;
            this.destinationButton.Location = new System.Drawing.Point(956, 12);
            this.destinationButton.Margin = new System.Windows.Forms.Padding(4);
            this.destinationButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.destinationButton.Name = "destinationButton";
            this.destinationButton.Size = new System.Drawing.Size(96, 30);
            this.destinationButton.TabIndex = 3;
            this.destinationButton.Text = "保存先を選択…";
            this.destinationButton.UseVisualStyleBackColor = true;
            this.destinationButton.Click += new System.EventHandler(this.DestinationButton_Click);
            // 
            // Body
            // 
            this.Body.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Body.Location = new System.Drawing.Point(0, 54);
            this.Body.Name = "Body";
            this.Body.Padding = new System.Windows.Forms.Padding(8);
            this.Body.Size = new System.Drawing.Size(1064, 621);
            this.Body.TabIndex = 1;
            // 
            // footerPanel
            // 
            this.footerPanel.AutoSize = true;
            this.footerPanel.Controls.Add(this.saveButton);
            this.footerPanel.Controls.Add(this.cancelButton);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footerPanel.Location = new System.Drawing.Point(0, 675);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Padding = new System.Windows.Forms.Padding(4);
            this.footerPanel.Size = new System.Drawing.Size(1064, 46);
            this.footerPanel.TabIndex = 2;
            // 
            // saveButton
            // 
            this.saveButton.AutoSize = true;
            this.saveButton.Location = new System.Drawing.Point(8, 8);
            this.saveButton.Margin = new System.Windows.Forms.Padding(4);
            this.saveButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(90, 30);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "保存して反映";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.AutoSize = true;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(106, 8);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(90, 30);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ConfigurationEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(1064, 721);
            this.Controls.Add(this.Body);
            this.Controls.Add(this.headerLayout);
            this.Controls.Add(this.footerPanel);
            this.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            this.MinimumSize = new System.Drawing.Size(900, 620);
            this.Name = "ConfigurationEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "設定ファイル";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Editor_FormClosing);
            this.headerLayout.ResumeLayout(false);
            this.headerLayout.PerformLayout();
            this.footerPanel.ResumeLayout(false);
            this.footerPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.TableLayoutPanel headerLayout;
        private System.Windows.Forms.Label fileLabel;
        private System.Windows.Forms.TextBox file;
        private System.Windows.Forms.Button openButton;
        private System.Windows.Forms.Button destinationButton;
        protected System.Windows.Forms.Panel Body;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.FlowLayoutPanel footerPanel;
    }
}
