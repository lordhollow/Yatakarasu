namespace Yata.Settings
{
    partial class SteamPickerForm
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
            this.root = new System.Windows.Forms.TextBox();
            this.search = new System.Windows.Forms.TextBox();
            this.games = new System.Windows.Forms.ListBox();
            this.status = new System.Windows.Forms.Label();
            this.searchLabel = new System.Windows.Forms.Label();
            this.rootLabel = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.reloadButton = new System.Windows.Forms.Button();
            this.searchPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.rootPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.selectButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.footerPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.ClientSize = new System.Drawing.Size(864, 521);
            this.MinimumSize = new System.Drawing.Size(650, 420);

            this.SuspendLayout();
            this.root.Name = "root";
            this.root.Width = 420;
            this.search.Name = "search";
            this.search.Width = 420;
            this.search.TextChanged += new System.EventHandler(this.Search_TextChanged);
            this.games.Name = "games";
            this.games.Dock = System.Windows.Forms.DockStyle.Fill;
            this.games.IntegralHeight = false;
            this.games.HorizontalScrollbar = true;
            this.games.DoubleClick += new System.EventHandler(this.Games_DoubleClick);
            this.status.Name = "status";
            this.status.Text = "Steamライブラリを検索しています…";
            this.status.AutoSize = true;
            this.status.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Text = "検索";
            this.searchLabel.AutoSize = true;
            this.searchLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.rootLabel.Name = "rootLabel";
            this.rootLabel.Text = "Steamフォルダ";
            this.rootLabel.AutoSize = true;
            this.rootLabel.Margin = new System.Windows.Forms.Padding(5, 9, 5, 5);
            this.browseButton.Name = "browseButton";
            this.browseButton.Text = "参照…";
            this.browseButton.AutoSize = true;
            this.browseButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.browseButton.Margin = new System.Windows.Forms.Padding(4);
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            this.reloadButton.Name = "reloadButton";
            this.reloadButton.Text = "再検索";
            this.reloadButton.AutoSize = true;
            this.reloadButton.MinimumSize = new System.Drawing.Size(90, 30);
            this.reloadButton.Margin = new System.Windows.Forms.Padding(4);
            this.reloadButton.UseVisualStyleBackColor = true;
            this.reloadButton.Click += new System.EventHandler(this.ReloadButton_Click);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchPanel.AutoSize = true;
            this.searchPanel.WrapContents = true;
            this.searchPanel.Padding = new System.Windows.Forms.Padding(4);
            this.searchPanel.Controls.Add(this.searchLabel);
            this.searchPanel.Controls.Add(this.search);
            this.searchPanel.Controls.Add(this.status);
            this.rootPanel.Name = "rootPanel";
            this.rootPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.rootPanel.AutoSize = true;
            this.rootPanel.WrapContents = true;
            this.rootPanel.Padding = new System.Windows.Forms.Padding(4);
            this.rootPanel.Controls.Add(this.rootLabel);
            this.rootPanel.Controls.Add(this.root);
            this.rootPanel.Controls.Add(this.browseButton);
            this.rootPanel.Controls.Add(this.reloadButton);
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
            this.Controls.Add(this.games);
            this.Controls.Add(this.searchPanel);
            this.Controls.Add(this.rootPanel);
            this.Controls.Add(this.footerPanel);
            this.AcceptButton = this.selectButton;
            this.CancelButton = this.cancelButton;
            this.Shown += new System.EventHandler(this.Picker_Shown);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Font = System.Drawing.SystemFonts.MessageBoxFont;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Name = "SteamPickerForm";
            this.Text = "インストール済みSteamソフトを選択";

            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.TextBox root;
        private System.Windows.Forms.TextBox search;
        private System.Windows.Forms.ListBox games;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.Label rootLabel;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button reloadButton;
        private System.Windows.Forms.FlowLayoutPanel searchPanel;
        private System.Windows.Forms.FlowLayoutPanel rootPanel;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.FlowLayoutPanel footerPanel;
    }
}
