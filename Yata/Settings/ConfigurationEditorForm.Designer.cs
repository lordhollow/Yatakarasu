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
            this.components = new System.ComponentModel.Container();
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.headerLayout = new System.Windows.Forms.TableLayoutPanel();
            this.fileLabel = new System.Windows.Forms.Label();
            this.file = new System.Windows.Forms.TextBox();
            this.openButton = new System.Windows.Forms.Button();
            this.destinationButton = new System.Windows.Forms.Button();
            this.Body = new System.Windows.Forms.Panel();
            this.saveButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.footerPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.ClientSize = new System.Drawing.Size(1064, 721);
            this.MinimumSize = new System.Drawing.Size(900, 620);

            this.SuspendLayout();
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.RowCount = 3;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.headerLayout.Name = "headerLayout";
            this.headerLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.headerLayout.AutoSize = true;
            this.headerLayout.ColumnCount = 4;
            this.headerLayout.RowCount = 1;
            this.headerLayout.Padding = new System.Windows.Forms.Padding(8);
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.headerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.fileLabel.Name = "fileLabel";
            this.fileLabel.Text = "設定ファイル";
            this.fileLabel.AutoSize = true;
            this.fileLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.file.Name = "file";
            this.file.Dock = System.Windows.Forms.DockStyle.Fill;
            this.file.ReadOnly = true;
            this.file.TabStop = false;
            this.file.Margin = new System.Windows.Forms.Padding(5, 8, 5, 5);
            this.openButton.Name = "openButton";
            this.openButton.Text = "開く…";
            this.openButton.AutoSize = true;
            this.openButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.openButton.Margin = new System.Windows.Forms.Padding(4);
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Click += new System.EventHandler(this.OpenButton_Click);
            this.destinationButton.Name = "destinationButton";
            this.destinationButton.Text = "保存先を選択…";
            this.destinationButton.AutoSize = true;
            this.destinationButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.destinationButton.Margin = new System.Windows.Forms.Padding(4);
            this.destinationButton.UseVisualStyleBackColor = true;
            this.destinationButton.Click += new System.EventHandler(this.DestinationButton_Click);
            this.headerLayout.Controls.Add(this.fileLabel, 0, 0);
            this.headerLayout.Controls.Add(this.file, 1, 0);
            this.headerLayout.Controls.Add(this.openButton, 2, 0);
            this.headerLayout.Controls.Add(this.destinationButton, 3, 0);
            this.Body.Name = "Body";
            this.Body.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Body.Padding = new System.Windows.Forms.Padding(8);
            this.saveButton.Name = "saveButton";
            this.saveButton.Text = "保存して反映";
            this.saveButton.AutoSize = true;
            this.saveButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.saveButton.Margin = new System.Windows.Forms.Padding(4);
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.SaveButton_Click);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.AutoSize = true;
            this.cancelButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.footerPanel.AutoSize = true;
            this.footerPanel.WrapContents = true;
            this.footerPanel.Padding = new System.Windows.Forms.Padding(4);
            this.footerPanel.Controls.Add(this.saveButton);
            this.footerPanel.Controls.Add(this.cancelButton);
            this.rootLayout.Controls.Add(this.headerLayout, 0, 0);
            this.rootLayout.Controls.Add(this.Body, 0, 1);
            this.rootLayout.Controls.Add(this.footerPanel, 0, 2);
            this.Controls.Add(this.rootLayout);
            this.CancelButton = this.cancelButton;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Editor_FormClosing);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = System.Drawing.SystemFonts.MessageBoxFont;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Name = "ConfigurationEditorForm";
            this.Text = "設定ファイル";

            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.TableLayoutPanel rootLayout;
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
