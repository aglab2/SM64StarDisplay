namespace StarDisplay
{
    partial class WarpDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.offsetComboBox = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.warpTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.areaTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // offsetComboBox
            // 
            this.offsetComboBox.FormattingEnabled = true;
            this.offsetComboBox.Items.AddRange(new object[] {
            "Course 1",
            "Course 2",
            "Course 3",
            "Course 4",
            "Course 5",
            "Course 6",
            "Course 7",
            "Course 8",
            "Course 9",
            "Course 10",
            "Course 11",
            "Course 12",
            "Course 13",
            "Course 14",
            "Course 15",
            "Bowser 1",
            "Fight 1",
            "Bowser 2",
            "Fight 2",
            "Bowser 3",
            "Fight 3",
            "Peach Slide",
            "Metal Cap",
            "Wing Cap",
            "Vanish Cap",
            "Rainbow",
            "Aquarium",
            "\"The End\"",
            "Castle Grounds",
            "Inside Castle",
            "Castle Courtyard"});
            this.offsetComboBox.Location = new System.Drawing.Point(12, 64);
            this.offsetComboBox.Name = "offsetComboBox";
            this.offsetComboBox.Size = new System.Drawing.Size(121, 21);
            this.offsetComboBox.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(140, 64);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(51, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // warpTextBox
            // 
            this.warpTextBox.Location = new System.Drawing.Point(73, 12);
            this.warpTextBox.Name = "warpTextBox";
            this.warpTextBox.Size = new System.Drawing.Size(117, 20);
            this.warpTextBox.TabIndex = 6;
            this.warpTextBox.Text = "10";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Warp ID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Area ID";
            // 
            // areaTextBox
            // 
            this.areaTextBox.Location = new System.Drawing.Point(73, 38);
            this.areaTextBox.Name = "areaTextBox";
            this.areaTextBox.Size = new System.Drawing.Size(117, 20);
            this.areaTextBox.TabIndex = 9;
            this.areaTextBox.Text = "1";
            // 
            // WarpDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(203, 95);
            this.Controls.Add(this.areaTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.warpTextBox);
            this.Controls.Add(this.offsetComboBox);
            this.Controls.Add(this.button1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WarpDialog";
            this.ShowIcon = false;
            this.Text = "Warp To";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox offsetComboBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox warpTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox areaTextBox;
    }
}