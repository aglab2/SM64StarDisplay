namespace StarDisplay
{
    partial class MainWindow
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.starPicture = new System.Windows.Forms.PictureBox();
            this.menuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.layoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripMenuItem();
            this.enableAutoDeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configureDisplayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showRedsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayHighlightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editFlagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iconsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem19 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem20 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem21 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem22 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem23 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem24 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem25 = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.starsInHackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.starTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem26 = new System.Windows.Forms.ToolStripMenuItem();
            this.showCollectablesOnlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.starPicture)).BeginInit();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // starPicture
            // 
            this.starPicture.BackColor = System.Drawing.Color.Black;
            this.starPicture.ContextMenuStrip = this.menuStrip;
            this.starPicture.Location = new System.Drawing.Point(0, 0);
            this.starPicture.Name = "starPicture";
            this.starPicture.Size = new System.Drawing.Size(345, 462);
            this.starPicture.TabIndex = 2;
            this.starPicture.TabStop = false;
            this.starPicture.Click += new System.EventHandler(this.starPicture_Click);
            this.starPicture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.starPicture_MouseMove);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.layoutToolStripMenuItem,
            this.toolStripMenuItem13,
            this.iconsToolStripMenuItem,
            this.fileToolStripMenuItem,
            this.textToolStripMenuItem,
            this.advancedToolStripMenuItem,
            this.toolStripMenuItem26});
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(153, 180);
            // 
            // layoutToolStripMenuItem
            // 
            this.layoutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem4,
            this.toolStripMenuItem5,
            this.toolStripMenuItem6,
            this.toolStripMenuItem7,
            this.toolStripMenuItem8,
            this.toolStripMenuItem10,
            this.showCollectablesOnlyToolStripMenuItem});
            this.layoutToolStripMenuItem.Enabled = false;
            this.layoutToolStripMenuItem.Name = "layoutToolStripMenuItem";
            this.layoutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.layoutToolStripMenuItem.Text = "Layout";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(198, 22);
            this.toolStripMenuItem4.Text = "Load";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(198, 22);
            this.toolStripMenuItem5.Text = "Save";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(198, 22);
            this.toolStripMenuItem6.Text = "Load Default";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.loadDefaultToolStripMenuItem_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(198, 22);
            this.toolStripMenuItem7.Text = "Load From...";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.loadFromToolStripMenuItem_Click);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(198, 22);
            this.toolStripMenuItem8.Text = "Save As...";
            this.toolStripMenuItem8.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(198, 22);
            this.toolStripMenuItem10.Text = "Load Custom Font";
            this.toolStripMenuItem10.Click += new System.EventHandler(this.loadCustomFontToolStripMenuItem_Click);
            // 
            // toolStripMenuItem13
            // 
            this.toolStripMenuItem13.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableAutoDeleteToolStripMenuItem,
            this.configureDisplayToolStripMenuItem,
            this.editFileToolStripMenuItem,
            this.showRedsToolStripMenuItem,
            this.displayHighlightToolStripMenuItem,
            this.editFlagsToolStripMenuItem});
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            this.toolStripMenuItem13.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem13.Text = "Settings";
            // 
            // enableAutoDeleteToolStripMenuItem
            // 
            this.enableAutoDeleteToolStripMenuItem.CheckOnClick = true;
            this.enableAutoDeleteToolStripMenuItem.Name = "enableAutoDeleteToolStripMenuItem";
            this.enableAutoDeleteToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.enableAutoDeleteToolStripMenuItem.Text = "Delete file on reset";
            // 
            // configureDisplayToolStripMenuItem
            // 
            this.configureDisplayToolStripMenuItem.CheckOnClick = true;
            this.configureDisplayToolStripMenuItem.Name = "configureDisplayToolStripMenuItem";
            this.configureDisplayToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.configureDisplayToolStripMenuItem.Text = "Configure Display";
            this.configureDisplayToolStripMenuItem.Click += new System.EventHandler(this.configureDisplayToolStripMenuItem_Click);
            // 
            // editFileToolStripMenuItem
            // 
            this.editFileToolStripMenuItem.CheckOnClick = true;
            this.editFileToolStripMenuItem.Name = "editFileToolStripMenuItem";
            this.editFileToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.editFileToolStripMenuItem.Text = "Edit File";
            this.editFileToolStripMenuItem.Click += new System.EventHandler(this.editFileToolStripMenuItem_Click);
            // 
            // showRedsToolStripMenuItem
            // 
            this.showRedsToolStripMenuItem.Checked = true;
            this.showRedsToolStripMenuItem.CheckOnClick = true;
            this.showRedsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showRedsToolStripMenuItem.Name = "showRedsToolStripMenuItem";
            this.showRedsToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.showRedsToolStripMenuItem.Text = "Show Collectables";
            // 
            // displayHighlightToolStripMenuItem
            // 
            this.displayHighlightToolStripMenuItem.Checked = true;
            this.displayHighlightToolStripMenuItem.CheckOnClick = true;
            this.displayHighlightToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.displayHighlightToolStripMenuItem.Name = "displayHighlightToolStripMenuItem";
            this.displayHighlightToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.displayHighlightToolStripMenuItem.Text = "Display Highlight";
            // 
            // editFlagsToolStripMenuItem
            // 
            this.editFlagsToolStripMenuItem.Name = "editFlagsToolStripMenuItem";
            this.editFlagsToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.editFlagsToolStripMenuItem.Text = "Edit Flags...";
            this.editFlagsToolStripMenuItem.Click += new System.EventHandler(this.editFlagsToolStripMenuItem_Click);
            // 
            // iconsToolStripMenuItem
            // 
            this.iconsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem19,
            this.toolStripMenuItem20,
            this.toolStripMenuItem21,
            this.toolStripMenuItem22,
            this.toolStripMenuItem23,
            this.toolStripMenuItem24,
            this.toolStripMenuItem25});
            this.iconsToolStripMenuItem.Enabled = false;
            this.iconsToolStripMenuItem.Name = "iconsToolStripMenuItem";
            this.iconsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.iconsToolStripMenuItem.Text = "Icons";
            // 
            // toolStripMenuItem19
            // 
            this.toolStripMenuItem19.Name = "toolStripMenuItem19";
            this.toolStripMenuItem19.Size = new System.Drawing.Size(182, 22);
            this.toolStripMenuItem19.Text = "Import from RAM";
            this.toolStripMenuItem19.Click += new System.EventHandler(this.importIconsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem20
            // 
            this.toolStripMenuItem20.Name = "toolStripMenuItem20";
            this.toolStripMenuItem20.Size = new System.Drawing.Size(182, 22);
            this.toolStripMenuItem20.Text = "Import from z64";
            this.toolStripMenuItem20.Click += new System.EventHandler(this.importIconsFromROMToolStripMenuItem_Click);
            // 
            // toolStripMenuItem21
            // 
            this.toolStripMenuItem21.Name = "toolStripMenuItem21";
            this.toolStripMenuItem21.Size = new System.Drawing.Size(182, 22);
            this.toolStripMenuItem21.Text = "Saturate";
            this.toolStripMenuItem21.Click += new System.EventHandler(this.saturateIconToolStripMenuItem_Click);
            // 
            // toolStripMenuItem22
            // 
            this.toolStripMenuItem22.Name = "toolStripMenuItem22";
            this.toolStripMenuItem22.Size = new System.Drawing.Size(182, 22);
            this.toolStripMenuItem22.Text = "Recolor";
            this.toolStripMenuItem22.Click += new System.EventHandler(this.recolorIconsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem23
            // 
            this.toolStripMenuItem23.Name = "toolStripMenuItem23";
            this.toolStripMenuItem23.Size = new System.Drawing.Size(182, 22);
            this.toolStripMenuItem23.Text = "Recolor Text WIP";
            this.toolStripMenuItem23.Click += new System.EventHandler(this.recolorTextToolStripMenuItem_Click);
            // 
            // toolStripMenuItem24
            // 
            this.toolStripMenuItem24.Name = "toolStripMenuItem24";
            this.toolStripMenuItem24.Size = new System.Drawing.Size(182, 22);
            this.toolStripMenuItem24.Text = "Reset Highlight";
            this.toolStripMenuItem24.Click += new System.EventHandler(this.resetHighlightToolStripMenuItem_Click);
            // 
            // toolStripMenuItem25
            // 
            this.toolStripMenuItem25.Name = "toolStripMenuItem25";
            this.toolStripMenuItem25.Size = new System.Drawing.Size(182, 22);
            this.toolStripMenuItem25.Text = "Replace background";
            this.toolStripMenuItem25.Click += new System.EventHandler(this.replaceBackgroundToolStripMenuItem_Click);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileAToolStripMenuItem,
            this.fileBToolStripMenuItem,
            this.fileCToolStripMenuItem,
            this.fileDToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.fileToolStripMenuItem.Text = "Game File";
            // 
            // fileAToolStripMenuItem
            // 
            this.fileAToolStripMenuItem.Checked = true;
            this.fileAToolStripMenuItem.CheckOnClick = true;
            this.fileAToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fileAToolStripMenuItem.Name = "fileAToolStripMenuItem";
            this.fileAToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.fileAToolStripMenuItem.Text = "File A";
            this.fileAToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // fileBToolStripMenuItem
            // 
            this.fileBToolStripMenuItem.CheckOnClick = true;
            this.fileBToolStripMenuItem.Name = "fileBToolStripMenuItem";
            this.fileBToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.fileBToolStripMenuItem.Text = "File B";
            this.fileBToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // fileCToolStripMenuItem
            // 
            this.fileCToolStripMenuItem.CheckOnClick = true;
            this.fileCToolStripMenuItem.Name = "fileCToolStripMenuItem";
            this.fileCToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.fileCToolStripMenuItem.Text = "File C";
            this.fileCToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // fileDToolStripMenuItem
            // 
            this.fileDToolStripMenuItem.CheckOnClick = true;
            this.fileDToolStripMenuItem.Name = "fileDToolStripMenuItem";
            this.fileDToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.fileDToolStripMenuItem.Text = "File D";
            this.fileDToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // textToolStripMenuItem
            // 
            this.textToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.starsInHackToolStripMenuItem,
            this.starTextToolStripMenuItem});
            this.textToolStripMenuItem.Name = "textToolStripMenuItem";
            this.textToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.textToolStripMenuItem.Text = "Edit Text";
            // 
            // starsInHackToolStripMenuItem
            // 
            this.starsInHackToolStripMenuItem.Name = "starsInHackToolStripMenuItem";
            this.starsInHackToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.starsInHackToolStripMenuItem.Text = "Stars in hack";
            this.starsInHackToolStripMenuItem.Click += new System.EventHandler(this.starsInHackToolStripMenuItem_Click);
            // 
            // starTextToolStripMenuItem
            // 
            this.starTextToolStripMenuItem.Name = "starTextToolStripMenuItem";
            this.starTextToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.starTextToolStripMenuItem.Text = "Star Text";
            this.starTextToolStripMenuItem.Click += new System.EventHandler(this.changeStarTextToolStripMenuItem_Click);
            // 
            // advancedToolStripMenuItem
            // 
            this.advancedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.toolStripMenuItem12,
            this.toolStripMenuItem9});
            this.advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
            this.advancedToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.advancedToolStripMenuItem.Text = "Advanced";
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.connectToolStripMenuItem.Text = "Connect to PJ64";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // toolStripMenuItem12
            // 
            this.toolStripMenuItem12.Name = "toolStripMenuItem12";
            this.toolStripMenuItem12.Size = new System.Drawing.Size(169, 22);
            this.toolStripMenuItem12.Text = "Load ROM";
            this.toolStripMenuItem12.Click += new System.EventHandler(this.loadROMToolStripMenuItem_Click);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new System.Drawing.Size(169, 22);
            this.toolStripMenuItem9.Text = "Import Star Masks";
            this.toolStripMenuItem9.Click += new System.EventHandler(this.importStarMasksToolStripMenuItem_Click);
            // 
            // toolStripMenuItem26
            // 
            this.toolStripMenuItem26.Name = "toolStripMenuItem26";
            this.toolStripMenuItem26.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem26.Text = "?";
            this.toolStripMenuItem26.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // showCollectablesOnlyToolStripMenuItem
            // 
            this.showCollectablesOnlyToolStripMenuItem.CheckOnClick = true;
            this.showCollectablesOnlyToolStripMenuItem.Name = "showCollectablesOnlyToolStripMenuItem";
            this.showCollectablesOnlyToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.showCollectablesOnlyToolStripMenuItem.Text = "Show Collectables Only";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(345, 459);
            this.Controls.Add(this.starPicture);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Star Display";
            ((System.ComponentModel.ISupportInitialize)(this.starPicture)).EndInit();
            this.menuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox starPicture;
        private System.Windows.Forms.ContextMenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem layoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem10;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem13;
        private System.Windows.Forms.ToolStripMenuItem enableAutoDeleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem configureDisplayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showRedsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayHighlightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iconsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem19;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem20;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem21;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem22;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem23;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem24;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem25;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem26;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileCToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem starsInHackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem starTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem9;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem12;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editFlagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showCollectablesOnlyToolStripMenuItem;
    }
}

