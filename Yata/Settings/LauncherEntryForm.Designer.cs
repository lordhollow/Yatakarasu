namespace Yata.Settings
{
    partial class LauncherEntryForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeImages();
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.caption = new System.Windows.Forms.TextBox();
            this.command = new System.Windows.Forms.TextBox();
            this.icon = new System.Windows.Forms.TextBox();
            this.option = new System.Windows.Forms.ComboBox();
            this.page = new System.Windows.Forms.NumericUpDown();
            this.row = new System.Windows.Forms.NumericUpDown();
            this.column = new System.Windows.Forms.NumericUpDown();
            this.system = new System.Windows.Forms.ComboBox();
            this.hint = new System.Windows.Forms.Label();
            this.preview = new System.Windows.Forms.PictureBox();
            this.layout = new System.Windows.Forms.TableLayoutPanel();
            this.sourceLabel = new System.Windows.Forms.Label();
            this.systemLabel = new System.Windows.Forms.Label();
            this.positionLabel = new System.Windows.Forms.Label();
            this.captionLabel = new System.Windows.Forms.Label();
            this.commandLabel = new System.Windows.Forms.Label();
            this.optionLabel = new System.Windows.Forms.Label();
            this.iconLabel = new System.Windows.Forms.Label();
            this.sourcePanel = new System.Windows.Forms.FlowLayoutPanel();
            this.chooseExeButton = new System.Windows.Forms.Button();
            this.chooseSteamButton = new System.Windows.Forms.Button();
            this.positionPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.pageLabel = new System.Windows.Forms.Label();
            this.rowLabel = new System.Windows.Forms.Label();
            this.columnLabel = new System.Windows.Forms.Label();
            this.iconPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.chooseIconButton = new System.Windows.Forms.Button();
            this.resetIconButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.footerPanel = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.page)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.row)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.column)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.preview)).BeginInit();
            this.layout.SuspendLayout();
            this.sourcePanel.SuspendLayout();
            this.positionPanel.SuspendLayout();
            this.iconPanel.SuspendLayout();
            this.footerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // caption
            // 
            this.caption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.caption.Location = new System.Drawing.Point(120, 139);
            this.caption.Name = "caption";
            this.caption.Size = new System.Drawing.Size(749, 23);
            this.caption.TabIndex = 10;
            // 
            // command
            // 
            this.command.Dock = System.Windows.Forms.DockStyle.Fill;
            this.command.Location = new System.Drawing.Point(120, 168);
            this.command.Name = "command";
            this.command.Size = new System.Drawing.Size(749, 23);
            this.command.TabIndex = 11;
            // 
            // icon
            // 
            this.icon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.icon.Location = new System.Drawing.Point(120, 226);
            this.icon.Name = "icon";
            this.icon.Size = new System.Drawing.Size(749, 23);
            this.icon.TabIndex = 13;
            // 
            // option
            // 
            this.option.Dock = System.Windows.Forms.DockStyle.Fill;
            this.option.FormattingEnabled = true;
            this.option.Location = new System.Drawing.Point(120, 197);
            this.option.Name = "option";
            this.option.Size = new System.Drawing.Size(749, 23);
            this.option.TabIndex = 12;
            // 
            // page
            // 
            this.page.Location = new System.Drawing.Point(52, 7);
            this.page.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.page.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.page.Name = "page";
            this.page.Size = new System.Drawing.Size(75, 23);
            this.page.TabIndex = 1;
            this.page.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // row
            // 
            this.row.Location = new System.Drawing.Point(191, 7);
            this.row.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.row.Name = "row";
            this.row.Size = new System.Drawing.Size(60, 23);
            this.row.TabIndex = 3;
            // 
            // column
            // 
            this.column.Location = new System.Drawing.Point(321, 7);
            this.column.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.column.Name = "column";
            this.column.Size = new System.Drawing.Size(60, 23);
            this.column.TabIndex = 5;
            // 
            // system
            // 
            this.system.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.system.FormattingEnabled = true;
            this.system.Location = new System.Drawing.Point(120, 67);
            this.system.Name = "system";
            this.system.Size = new System.Drawing.Size(310, 23);
            this.system.TabIndex = 8;
            this.system.SelectedIndexChanged += new System.EventHandler(this.System_SelectedIndexChanged);
            // 
            // hint
            // 
            this.hint.AutoSize = true;
            this.hint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hint.ForeColor = System.Drawing.Color.DimGray;
            this.hint.Location = new System.Drawing.Point(120, 338);
            this.hint.Name = "hint";
            this.hint.Padding = new System.Windows.Forms.Padding(6);
            this.hint.Size = new System.Drawing.Size(749, 105);
            this.hint.TabIndex = 15;
            this.hint.Text = "1. アプリを選択 → 2. アイコンを選択 → 3. 起動オプションを入力。Exe/DLL内の複数アイコンにも対応します。";
            // 
            // preview
            // 
            this.preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.preview.Location = new System.Drawing.Point(15, 255);
            this.preview.Name = "preview";
            this.preview.Size = new System.Drawing.Size(80, 80);
            this.preview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.preview.TabIndex = 16;
            this.preview.TabStop = false;
            // 
            // layout
            // 
            this.layout.ColumnCount = 2;
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.Controls.Add(this.sourceLabel, 0, 0);
            this.layout.Controls.Add(this.systemLabel, 0, 1);
            this.layout.Controls.Add(this.positionLabel, 0, 2);
            this.layout.Controls.Add(this.captionLabel, 0, 3);
            this.layout.Controls.Add(this.commandLabel, 0, 4);
            this.layout.Controls.Add(this.optionLabel, 0, 5);
            this.layout.Controls.Add(this.iconLabel, 0, 6);
            this.layout.Controls.Add(this.sourcePanel, 1, 0);
            this.layout.Controls.Add(this.system, 1, 1);
            this.layout.Controls.Add(this.positionPanel, 1, 2);
            this.layout.Controls.Add(this.caption, 1, 3);
            this.layout.Controls.Add(this.command, 1, 4);
            this.layout.Controls.Add(this.option, 1, 5);
            this.layout.Controls.Add(this.icon, 1, 6);
            this.layout.Controls.Add(this.iconPanel, 1, 7);
            this.layout.Controls.Add(this.hint, 1, 8);
            this.layout.Controls.Add(this.preview, 0, 7);
            this.layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layout.Location = new System.Drawing.Point(0, 0);
            this.layout.Name = "layout";
            this.layout.Padding = new System.Windows.Forms.Padding(12);
            this.layout.RowCount = 9;
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.Size = new System.Drawing.Size(884, 455);
            this.layout.TabIndex = 0;
            // 
            // sourceLabel
            // 
            this.sourceLabel.AutoSize = true;
            this.sourceLabel.Location = new System.Drawing.Point(17, 21);
            this.sourceLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.sourceLabel.Name = "sourceLabel";
            this.sourceLabel.Size = new System.Drawing.Size(43, 15);
            this.sourceLabel.TabIndex = 0;
            this.sourceLabel.Text = "追加元";
            // 
            // systemLabel
            // 
            this.systemLabel.AutoSize = true;
            this.systemLabel.Location = new System.Drawing.Point(17, 73);
            this.systemLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.systemLabel.Name = "systemLabel";
            this.systemLabel.Size = new System.Drawing.Size(70, 15);
            this.systemLabel.TabIndex = 1;
            this.systemLabel.Text = "システムアプリ";
            // 
            // positionLabel
            // 
            this.positionLabel.AutoSize = true;
            this.positionLabel.Location = new System.Drawing.Point(17, 102);
            this.positionLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.positionLabel.Name = "positionLabel";
            this.positionLabel.Size = new System.Drawing.Size(31, 15);
            this.positionLabel.TabIndex = 2;
            this.positionLabel.Text = "配置";
            // 
            // captionLabel
            // 
            this.captionLabel.AutoSize = true;
            this.captionLabel.Location = new System.Drawing.Point(17, 145);
            this.captionLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.captionLabel.Name = "captionLabel";
            this.captionLabel.Size = new System.Drawing.Size(43, 15);
            this.captionLabel.TabIndex = 3;
            this.captionLabel.Text = "表示名";
            // 
            // commandLabel
            // 
            this.commandLabel.AutoSize = true;
            this.commandLabel.Location = new System.Drawing.Point(17, 174);
            this.commandLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.commandLabel.Name = "commandLabel";
            this.commandLabel.Size = new System.Drawing.Size(41, 15);
            this.commandLabel.TabIndex = 4;
            this.commandLabel.Text = "コマンド";
            // 
            // optionLabel
            // 
            this.optionLabel.AutoSize = true;
            this.optionLabel.Location = new System.Drawing.Point(17, 203);
            this.optionLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.optionLabel.Name = "optionLabel";
            this.optionLabel.Size = new System.Drawing.Size(74, 15);
            this.optionLabel.TabIndex = 5;
            this.optionLabel.Text = "起動オプション";
            // 
            // iconLabel
            // 
            this.iconLabel.AutoSize = true;
            this.iconLabel.Location = new System.Drawing.Point(17, 232);
            this.iconLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.iconLabel.Name = "iconLabel";
            this.iconLabel.Size = new System.Drawing.Size(42, 15);
            this.iconLabel.TabIndex = 6;
            this.iconLabel.Text = "アイコン";
            // 
            // sourcePanel
            // 
            this.sourcePanel.AutoSize = true;
            this.sourcePanel.Controls.Add(this.chooseExeButton);
            this.sourcePanel.Controls.Add(this.chooseSteamButton);
            this.sourcePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.sourcePanel.Location = new System.Drawing.Point(120, 15);
            this.sourcePanel.Name = "sourcePanel";
            this.sourcePanel.Padding = new System.Windows.Forms.Padding(4);
            this.sourcePanel.Size = new System.Drawing.Size(749, 46);
            this.sourcePanel.TabIndex = 7;
            // 
            // chooseExeButton
            // 
            this.chooseExeButton.AutoSize = true;
            this.chooseExeButton.Location = new System.Drawing.Point(8, 8);
            this.chooseExeButton.Margin = new System.Windows.Forms.Padding(4);
            this.chooseExeButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.chooseExeButton.Name = "chooseExeButton";
            this.chooseExeButton.Size = new System.Drawing.Size(90, 30);
            this.chooseExeButton.TabIndex = 0;
            this.chooseExeButton.Text = "Exeを選択…";
            this.chooseExeButton.UseVisualStyleBackColor = true;
            this.chooseExeButton.Click += new System.EventHandler(this.ChooseExeButton_Click);
            // 
            // chooseSteamButton
            // 
            this.chooseSteamButton.AutoSize = true;
            this.chooseSteamButton.Location = new System.Drawing.Point(106, 8);
            this.chooseSteamButton.Margin = new System.Windows.Forms.Padding(4);
            this.chooseSteamButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.chooseSteamButton.Name = "chooseSteamButton";
            this.chooseSteamButton.Size = new System.Drawing.Size(90, 30);
            this.chooseSteamButton.TabIndex = 1;
            this.chooseSteamButton.Text = "Steamから…";
            this.chooseSteamButton.UseVisualStyleBackColor = true;
            this.chooseSteamButton.Click += new System.EventHandler(this.ChooseSteamButton_Click);
            // 
            // positionPanel
            // 
            this.positionPanel.AutoSize = true;
            this.positionPanel.Controls.Add(this.pageLabel);
            this.positionPanel.Controls.Add(this.page);
            this.positionPanel.Controls.Add(this.rowLabel);
            this.positionPanel.Controls.Add(this.row);
            this.positionPanel.Controls.Add(this.columnLabel);
            this.positionPanel.Controls.Add(this.column);
            this.positionPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.positionPanel.Location = new System.Drawing.Point(120, 96);
            this.positionPanel.Name = "positionPanel";
            this.positionPanel.Padding = new System.Windows.Forms.Padding(4);
            this.positionPanel.Size = new System.Drawing.Size(749, 37);
            this.positionPanel.TabIndex = 9;
            // 
            // pageLabel
            // 
            this.pageLabel.AutoSize = true;
            this.pageLabel.Location = new System.Drawing.Point(9, 13);
            this.pageLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.pageLabel.Name = "pageLabel";
            this.pageLabel.Size = new System.Drawing.Size(35, 15);
            this.pageLabel.TabIndex = 0;
            this.pageLabel.Text = "ページ";
            // 
            // rowLabel
            // 
            this.rowLabel.AutoSize = true;
            this.rowLabel.Location = new System.Drawing.Point(135, 13);
            this.rowLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.rowLabel.Name = "rowLabel";
            this.rowLabel.Size = new System.Drawing.Size(48, 15);
            this.rowLabel.TabIndex = 2;
            this.rowLabel.Text = "行 (0–1)";
            // 
            // columnLabel
            // 
            this.columnLabel.AutoSize = true;
            this.columnLabel.Location = new System.Drawing.Point(259, 13);
            this.columnLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.columnLabel.Name = "columnLabel";
            this.columnLabel.Size = new System.Drawing.Size(54, 15);
            this.columnLabel.TabIndex = 4;
            this.columnLabel.Text = "列 (0–15)";
            // 
            // iconPanel
            // 
            this.iconPanel.AutoSize = true;
            this.iconPanel.Controls.Add(this.chooseIconButton);
            this.iconPanel.Controls.Add(this.resetIconButton);
            this.iconPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.iconPanel.Location = new System.Drawing.Point(120, 255);
            this.iconPanel.Name = "iconPanel";
            this.iconPanel.Padding = new System.Windows.Forms.Padding(4);
            this.iconPanel.Size = new System.Drawing.Size(749, 46);
            this.iconPanel.TabIndex = 14;
            // 
            // chooseIconButton
            // 
            this.chooseIconButton.AutoSize = true;
            this.chooseIconButton.Location = new System.Drawing.Point(8, 8);
            this.chooseIconButton.Margin = new System.Windows.Forms.Padding(4);
            this.chooseIconButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.chooseIconButton.Name = "chooseIconButton";
            this.chooseIconButton.Size = new System.Drawing.Size(95, 30);
            this.chooseIconButton.TabIndex = 0;
            this.chooseIconButton.Text = "アイコンを選択…";
            this.chooseIconButton.UseVisualStyleBackColor = true;
            this.chooseIconButton.Click += new System.EventHandler(this.ChooseIconButton_Click);
            // 
            // resetIconButton
            // 
            this.resetIconButton.AutoSize = true;
            this.resetIconButton.Location = new System.Drawing.Point(111, 8);
            this.resetIconButton.Margin = new System.Windows.Forms.Padding(4);
            this.resetIconButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.resetIconButton.Name = "resetIconButton";
            this.resetIconButton.Size = new System.Drawing.Size(90, 30);
            this.resetIconButton.TabIndex = 1;
            this.resetIconButton.Text = "標準に戻す";
            this.resetIconButton.UseVisualStyleBackColor = true;
            this.resetIconButton.Click += new System.EventHandler(this.ResetIconButton_Click);
            // 
            // okButton
            // 
            this.okButton.AutoSize = true;
            this.okButton.Location = new System.Drawing.Point(8, 8);
            this.okButton.Margin = new System.Windows.Forms.Padding(4);
            this.okButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(90, 30);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "項目を確定";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
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
            // footerPanel
            // 
            this.footerPanel.AutoSize = true;
            this.footerPanel.Controls.Add(this.okButton);
            this.footerPanel.Controls.Add(this.cancelButton);
            this.footerPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footerPanel.Location = new System.Drawing.Point(0, 455);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Padding = new System.Windows.Forms.Padding(4);
            this.footerPanel.Size = new System.Drawing.Size(884, 46);
            this.footerPanel.TabIndex = 1;
            // 
            // LauncherEntryForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(884, 501);
            this.Controls.Add(this.layout);
            this.Controls.Add(this.footerPanel);
            this.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            this.MinimumSize = new System.Drawing.Size(760, 500);
            this.Name = "LauncherEntryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ランチャー項目の編集";
            ((System.ComponentModel.ISupportInitialize)(this.page)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.row)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.column)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.preview)).EndInit();
            this.layout.ResumeLayout(false);
            this.layout.PerformLayout();
            this.sourcePanel.ResumeLayout(false);
            this.sourcePanel.PerformLayout();
            this.positionPanel.ResumeLayout(false);
            this.positionPanel.PerformLayout();
            this.iconPanel.ResumeLayout(false);
            this.iconPanel.PerformLayout();
            this.footerPanel.ResumeLayout(false);
            this.footerPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.TextBox caption;
        private System.Windows.Forms.TextBox command;
        private System.Windows.Forms.TextBox icon;
        private System.Windows.Forms.ComboBox option;
        private System.Windows.Forms.NumericUpDown page;
        private System.Windows.Forms.NumericUpDown row;
        private System.Windows.Forms.NumericUpDown column;
        private System.Windows.Forms.ComboBox system;
        private System.Windows.Forms.Label hint;
        private System.Windows.Forms.PictureBox preview;
        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.Button chooseExeButton;
        private System.Windows.Forms.Button chooseSteamButton;
        private System.Windows.Forms.FlowLayoutPanel sourcePanel;
        private System.Windows.Forms.Label pageLabel;
        private System.Windows.Forms.Label rowLabel;
        private System.Windows.Forms.Label columnLabel;
        private System.Windows.Forms.FlowLayoutPanel positionPanel;
        private System.Windows.Forms.Button chooseIconButton;
        private System.Windows.Forms.Button resetIconButton;
        private System.Windows.Forms.FlowLayoutPanel iconPanel;
        private System.Windows.Forms.Label sourceLabel;
        private System.Windows.Forms.Label systemLabel;
        private System.Windows.Forms.Label positionLabel;
        private System.Windows.Forms.Label captionLabel;
        private System.Windows.Forms.Label commandLabel;
        private System.Windows.Forms.Label optionLabel;
        private System.Windows.Forms.Label iconLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.FlowLayoutPanel footerPanel;
    }
}
