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
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.configureLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editComponentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadCustomFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceBackgroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripMenuItem();
            this.editFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.warpToLevelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editFlagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableAutoDeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pickFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iconsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem19 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem20 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem21 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem22 = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.resetHighlightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.killPJ64ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem26 = new System.Windows.Forms.ToolStripMenuItem();
            this.syncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.starPicture.Size = new System.Drawing.Size(346, 460);
            this.starPicture.TabIndex = 2;
            this.starPicture.TabStop = false;
            this.starPicture.Click += new System.EventHandler(this.starPicture_Click);
            this.starPicture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.starPicture_MouseMove);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.layoutToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.toolStripMenuItem13,
            this.iconsToolStripMenuItem,
            this.advancedToolStripMenuItem,
            this.toolStripMenuItem26,
            this.syncToolStripMenuItem});
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(153, 180);
            // 
            // layoutToolStripMenuItem
            // 
            this.layoutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem4,
            this.toolStripMenuItem6,
            this.toolStripMenuItem7,
            this.toolStripMenuItem5,
            this.toolStripMenuItem8,
            this.configureLayoutToolStripMenuItem});
            this.layoutToolStripMenuItem.Name = "layoutToolStripMenuItem";
            this.layoutToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.layoutToolStripMenuItem.Text = "Layout";
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(141, 22);
            this.toolStripMenuItem4.Text = "Load";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(141, 22);
            this.toolStripMenuItem6.Text = "Load Default";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.loadDefaultToolStripMenuItem_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(141, 22);
            this.toolStripMenuItem7.Text = "Load From...";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.loadFromToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(141, 22);
            this.toolStripMenuItem5.Text = "Save";
            this.toolStripMenuItem5.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(141, 22);
            this.toolStripMenuItem8.Text = "Save As...";
            this.toolStripMenuItem8.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // configureLayoutToolStripMenuItem
            // 
            this.configureLayoutToolStripMenuItem.CheckOnClick = true;
            this.configureLayoutToolStripMenuItem.Name = "configureLayoutToolStripMenuItem";
            this.configureLayoutToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.configureLayoutToolStripMenuItem.Text = "Edit Layout";
            this.configureLayoutToolStripMenuItem.Click += new System.EventHandler(this.configureDisplayToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.loadDefaultToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.editComponentsToolStripMenuItem,
            this.loadCustomFontToolStripMenuItem,
            this.replaceBackgroundToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.viewToolStripMenuItem.Text = "Settings";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click_1);
            // 
            // loadDefaultToolStripMenuItem
            // 
            this.loadDefaultToolStripMenuItem.Name = "loadDefaultToolStripMenuItem";
            this.loadDefaultToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.loadDefaultToolStripMenuItem.Text = "Load Default";
            this.loadDefaultToolStripMenuItem.Click += new System.EventHandler(this.loadDefaultToolStripMenuItem_Click_1);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click_1);
            // 
            // editComponentsToolStripMenuItem
            // 
            this.editComponentsToolStripMenuItem.Name = "editComponentsToolStripMenuItem";
            this.editComponentsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.editComponentsToolStripMenuItem.Text = "Edit Settings...";
            this.editComponentsToolStripMenuItem.Click += new System.EventHandler(this.editComponentsToolStripMenuItem_Click);
            // 
            // loadCustomFontToolStripMenuItem
            // 
            this.loadCustomFontToolStripMenuItem.Name = "loadCustomFontToolStripMenuItem";
            this.loadCustomFontToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.loadCustomFontToolStripMenuItem.Text = "Load Custom Font";
            this.loadCustomFontToolStripMenuItem.Click += new System.EventHandler(this.loadCustomFontToolStripMenuItem_Click);
            // 
            // replaceBackgroundToolStripMenuItem
            // 
            this.replaceBackgroundToolStripMenuItem.Name = "replaceBackgroundToolStripMenuItem";
            this.replaceBackgroundToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.replaceBackgroundToolStripMenuItem.Text = "Replace Background";
            this.replaceBackgroundToolStripMenuItem.Click += new System.EventHandler(this.replaceBackgroundToolStripMenuItem_Click);
            // 
            // toolStripMenuItem13
            // 
            this.toolStripMenuItem13.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editFileToolStripMenuItem,
            this.warpToLevelToolStripMenuItem,
            this.editFlagsToolStripMenuItem,
            this.enableAutoDeleteToolStripMenuItem,
            this.pickFileToolStripMenuItem});
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            this.toolStripMenuItem13.Size = new System.Drawing.Size(133, 22);
            this.toolStripMenuItem13.Text = "Game Edits";
            // 
            // editFileToolStripMenuItem
            // 
            this.editFileToolStripMenuItem.CheckOnClick = true;
            this.editFileToolStripMenuItem.Name = "editFileToolStripMenuItem";
            this.editFileToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.editFileToolStripMenuItem.Text = "Edit File";
            this.editFileToolStripMenuItem.Click += new System.EventHandler(this.editFileToolStripMenuItem_Click);
            // 
            // warpToLevelToolStripMenuItem
            // 
            this.warpToLevelToolStripMenuItem.CheckOnClick = true;
            this.warpToLevelToolStripMenuItem.Name = "warpToLevelToolStripMenuItem";
            this.warpToLevelToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.warpToLevelToolStripMenuItem.Text = "Warp to level";
            this.warpToLevelToolStripMenuItem.Click += new System.EventHandler(this.warpToLevelToolStripMenuItem_Click);
            // 
            // editFlagsToolStripMenuItem
            // 
            this.editFlagsToolStripMenuItem.Name = "editFlagsToolStripMenuItem";
            this.editFlagsToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.editFlagsToolStripMenuItem.Text = "Edit Flags...";
            this.editFlagsToolStripMenuItem.Click += new System.EventHandler(this.editFlagsToolStripMenuItem_Click);
            // 
            // enableAutoDeleteToolStripMenuItem
            // 
            this.enableAutoDeleteToolStripMenuItem.CheckOnClick = true;
            this.enableAutoDeleteToolStripMenuItem.Name = "enableAutoDeleteToolStripMenuItem";
            this.enableAutoDeleteToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.enableAutoDeleteToolStripMenuItem.Text = "Delete file on reset";
            // 
            // pickFileToolStripMenuItem
            // 
            this.pickFileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileAToolStripMenuItem,
            this.fileBToolStripMenuItem,
            this.fileCToolStripMenuItem,
            this.fileDToolStripMenuItem});
            this.pickFileToolStripMenuItem.Name = "pickFileToolStripMenuItem";
            this.pickFileToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.pickFileToolStripMenuItem.Text = "Pick File";
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
            // iconsToolStripMenuItem
            // 
            this.iconsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem19,
            this.toolStripMenuItem20,
            this.toolStripMenuItem21,
            this.toolStripMenuItem22});
            this.iconsToolStripMenuItem.Name = "iconsToolStripMenuItem";
            this.iconsToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.iconsToolStripMenuItem.Text = "Icons";
            // 
            // toolStripMenuItem19
            // 
            this.toolStripMenuItem19.Name = "toolStripMenuItem19";
            this.toolStripMenuItem19.Size = new System.Drawing.Size(168, 22);
            this.toolStripMenuItem19.Text = "Import from RAM";
            this.toolStripMenuItem19.Click += new System.EventHandler(this.importIconsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem20
            // 
            this.toolStripMenuItem20.Name = "toolStripMenuItem20";
            this.toolStripMenuItem20.Size = new System.Drawing.Size(168, 22);
            this.toolStripMenuItem20.Text = "Import from z64";
            this.toolStripMenuItem20.Click += new System.EventHandler(this.importIconsFromROMToolStripMenuItem_Click);
            // 
            // toolStripMenuItem21
            // 
            this.toolStripMenuItem21.Name = "toolStripMenuItem21";
            this.toolStripMenuItem21.Size = new System.Drawing.Size(168, 22);
            this.toolStripMenuItem21.Text = "Saturate";
            this.toolStripMenuItem21.Click += new System.EventHandler(this.saturateIconToolStripMenuItem_Click);
            // 
            // toolStripMenuItem22
            // 
            this.toolStripMenuItem22.Name = "toolStripMenuItem22";
            this.toolStripMenuItem22.Size = new System.Drawing.Size(168, 22);
            this.toolStripMenuItem22.Text = "Recolor";
            this.toolStripMenuItem22.Click += new System.EventHandler(this.recolorIconsToolStripMenuItem_Click);
            // 
            // advancedToolStripMenuItem
            // 
            this.advancedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.toolStripMenuItem12,
            this.toolStripMenuItem9,
            this.resetHighlightToolStripMenuItem,
            this.killPJ64ToolStripMenuItem});
            this.advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
            this.advancedToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
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
            // resetHighlightToolStripMenuItem
            // 
            this.resetHighlightToolStripMenuItem.Name = "resetHighlightToolStripMenuItem";
            this.resetHighlightToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.resetHighlightToolStripMenuItem.Text = "Reset Highlight";
            this.resetHighlightToolStripMenuItem.Click += new System.EventHandler(this.resetHighlightToolStripMenuItem_Click);
            // 
            // killPJ64ToolStripMenuItem
            // 
            this.killPJ64ToolStripMenuItem.Name = "killPJ64ToolStripMenuItem";
            this.killPJ64ToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.killPJ64ToolStripMenuItem.Text = "Kill PJ64";
            this.killPJ64ToolStripMenuItem.Click += new System.EventHandler(this.killPJ64ToolStripMenuItem_Click);
            // 
            // toolStripMenuItem26
            // 
            this.toolStripMenuItem26.Name = "toolStripMenuItem26";
            this.toolStripMenuItem26.Size = new System.Drawing.Size(133, 22);
            this.toolStripMenuItem26.Text = "?";
            this.toolStripMenuItem26.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // syncToolStripMenuItem
            // 
            this.syncToolStripMenuItem.Name = "syncToolStripMenuItem";
            this.syncToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.syncToolStripMenuItem.Text = "Sync";
            this.syncToolStripMenuItem.Click += new System.EventHandler(this.syncToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(345, 459);
            this.Controls.Add(this.starPicture);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Star Display";
            this.Resize += new System.EventHandler(this.MainWindow_Resize);
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
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem13;
        private System.Windows.Forms.ToolStripMenuItem enableAutoDeleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iconsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem19;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem20;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem21;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem22;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem26;
        private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem9;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem12;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editFlagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editComponentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadCustomFontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceBackgroundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pickFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileCToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetHighlightToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem configureLayoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadDefaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem warpToLevelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem killPJ64ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem syncToolStripMenuItem;
    }
}

