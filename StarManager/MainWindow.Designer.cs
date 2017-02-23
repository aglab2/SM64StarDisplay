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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.connectButton = new System.Windows.Forms.Button();
            this.starPicture = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.totalCountText = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.layoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadFromToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importStarMasksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.enableAutoDeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configureDisplayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.iconsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importIconsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saturateIconToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recolorIconsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonA = new System.Windows.Forms.RadioButton();
            this.radioButtonB = new System.Windows.Forms.RadioButton();
            this.radioButtonC = new System.Windows.Forms.RadioButton();
            this.radioButtonD = new System.Windows.Forms.RadioButton();
            this.resetHighlightToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.starPicture)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(12, 466);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 1;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // starPicture
            // 
            this.starPicture.BackColor = System.Drawing.Color.Black;
            this.starPicture.Location = new System.Drawing.Point(12, 27);
            this.starPicture.Name = "starPicture";
            this.starPicture.Size = new System.Drawing.Size(345, 431);
            this.starPicture.TabIndex = 2;
            this.starPicture.TabStop = false;
            this.starPicture.Click += new System.EventHandler(this.starPicture_Click);
            this.starPicture.MouseMove += new System.Windows.Forms.MouseEventHandler(this.starPicture_MouseMove);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(225, 469);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Stars in hack:";
            // 
            // totalCountText
            // 
            this.totalCountText.Location = new System.Drawing.Point(303, 466);
            this.totalCountText.MaxLength = 3;
            this.totalCountText.Name = "totalCountText";
            this.totalCountText.Size = new System.Drawing.Size(54, 20);
            this.totalCountText.TabIndex = 5;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.layoutToolStripMenuItem,
            this.toolStripMenuItem1,
            this.iconsToolStripMenuItem,
            this.toolStripMenuItem2});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(369, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip";
            // 
            // layoutToolStripMenuItem
            // 
            this.layoutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.loadDefaultToolStripMenuItem,
            this.loadFromToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.importStarMasksToolStripMenuItem});
            this.layoutToolStripMenuItem.Enabled = false;
            this.layoutToolStripMenuItem.Name = "layoutToolStripMenuItem";
            this.layoutToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.layoutToolStripMenuItem.Text = "Layout";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.loadToolStripMenuItem.Text = "Load";
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // loadDefaultToolStripMenuItem
            // 
            this.loadDefaultToolStripMenuItem.Name = "loadDefaultToolStripMenuItem";
            this.loadDefaultToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.loadDefaultToolStripMenuItem.Text = "Load Default";
            this.loadDefaultToolStripMenuItem.Click += new System.EventHandler(this.loadDefaultToolStripMenuItem_Click);
            // 
            // loadFromToolStripMenuItem
            // 
            this.loadFromToolStripMenuItem.Name = "loadFromToolStripMenuItem";
            this.loadFromToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.loadFromToolStripMenuItem.Text = "Load From...";
            this.loadFromToolStripMenuItem.Click += new System.EventHandler(this.loadFromToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.saveAsToolStripMenuItem.Text = "Save As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // importStarMasksToolStripMenuItem
            // 
            this.importStarMasksToolStripMenuItem.Name = "importStarMasksToolStripMenuItem";
            this.importStarMasksToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.importStarMasksToolStripMenuItem.Text = "Import Star Masks";
            this.importStarMasksToolStripMenuItem.Click += new System.EventHandler(this.importStarMasksToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableAutoDeleteToolStripMenuItem,
            this.configureDisplayToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(61, 20);
            this.toolStripMenuItem1.Text = "Settings";
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
            // 
            // iconsToolStripMenuItem
            // 
            this.iconsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importIconsToolStripMenuItem,
            this.saturateIconToolStripMenuItem,
            this.recolorIconsToolStripMenuItem,
            this.resetHighlightToolStripMenuItem});
            this.iconsToolStripMenuItem.Enabled = false;
            this.iconsToolStripMenuItem.Name = "iconsToolStripMenuItem";
            this.iconsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.iconsToolStripMenuItem.Text = "Icons";
            // 
            // importIconsToolStripMenuItem
            // 
            this.importIconsToolStripMenuItem.Name = "importIconsToolStripMenuItem";
            this.importIconsToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.importIconsToolStripMenuItem.Text = "Import Icons";
            this.importIconsToolStripMenuItem.Click += new System.EventHandler(this.importIconsToolStripMenuItem_Click);
            // 
            // saturateIconToolStripMenuItem
            // 
            this.saturateIconToolStripMenuItem.Name = "saturateIconToolStripMenuItem";
            this.saturateIconToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.saturateIconToolStripMenuItem.Text = "Saturate Icon";
            this.saturateIconToolStripMenuItem.Click += new System.EventHandler(this.saturateIconToolStripMenuItem_Click);
            // 
            // recolorIconsToolStripMenuItem
            // 
            this.recolorIconsToolStripMenuItem.Name = "recolorIconsToolStripMenuItem";
            this.recolorIconsToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.recolorIconsToolStripMenuItem.Text = "Recolor Icons";
            this.recolorIconsToolStripMenuItem.Click += new System.EventHandler(this.recolorIconsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(24, 20);
            this.toolStripMenuItem2.Text = "?";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonA);
            this.groupBox1.Controls.Add(this.radioButtonB);
            this.groupBox1.Controls.Add(this.radioButtonC);
            this.groupBox1.Controls.Add(this.radioButtonD);
            this.groupBox1.Location = new System.Drawing.Point(197, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(160, 24);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            // 
            // radioButtonA
            // 
            this.radioButtonA.AutoSize = true;
            this.radioButtonA.Checked = true;
            this.radioButtonA.Location = new System.Drawing.Point(6, 7);
            this.radioButtonA.Name = "radioButtonA";
            this.radioButtonA.Size = new System.Drawing.Size(32, 17);
            this.radioButtonA.TabIndex = 3;
            this.radioButtonA.TabStop = true;
            this.radioButtonA.Text = "A";
            this.radioButtonA.UseVisualStyleBackColor = true;
            // 
            // radioButtonB
            // 
            this.radioButtonB.AutoSize = true;
            this.radioButtonB.Location = new System.Drawing.Point(44, 7);
            this.radioButtonB.Name = "radioButtonB";
            this.radioButtonB.Size = new System.Drawing.Size(32, 17);
            this.radioButtonB.TabIndex = 2;
            this.radioButtonB.Text = "B";
            this.radioButtonB.UseVisualStyleBackColor = true;
            // 
            // radioButtonC
            // 
            this.radioButtonC.AutoSize = true;
            this.radioButtonC.Location = new System.Drawing.Point(82, 7);
            this.radioButtonC.Name = "radioButtonC";
            this.radioButtonC.Size = new System.Drawing.Size(32, 17);
            this.radioButtonC.TabIndex = 1;
            this.radioButtonC.Text = "C";
            this.radioButtonC.UseVisualStyleBackColor = true;
            // 
            // radioButtonD
            // 
            this.radioButtonD.AutoSize = true;
            this.radioButtonD.Location = new System.Drawing.Point(120, 7);
            this.radioButtonD.Name = "radioButtonD";
            this.radioButtonD.Size = new System.Drawing.Size(33, 17);
            this.radioButtonD.TabIndex = 0;
            this.radioButtonD.Text = "D";
            this.radioButtonD.UseVisualStyleBackColor = true;
            // 
            // resetHighlightToolStripMenuItem
            // 
            this.resetHighlightToolStripMenuItem.Name = "resetHighlightToolStripMenuItem";
            this.resetHighlightToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.resetHighlightToolStripMenuItem.Text = "Reset Highlight";
            this.resetHighlightToolStripMenuItem.Click += new System.EventHandler(this.resetHighlightToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(369, 498);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.totalCountText);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.starPicture);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Star Display";
            ((System.ComponentModel.ISupportInitialize)(this.starPicture)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.PictureBox starPicture;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox totalCountText;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem enableAutoDeleteToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonA;
        private System.Windows.Forms.RadioButton radioButtonB;
        private System.Windows.Forms.RadioButton radioButtonC;
        private System.Windows.Forms.RadioButton radioButtonD;
        private System.Windows.Forms.ToolStripMenuItem configureDisplayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem layoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadDefaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iconsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importIconsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saturateIconToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadFromToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem importStarMasksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recolorIconsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetHighlightToolStripMenuItem;
    }
}

