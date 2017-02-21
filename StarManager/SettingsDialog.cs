using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarDisplay
{
    public partial class Settings : Form
    {
        LineDescription lind;

        public Settings(LineDescription lind)
        {
            InitializeComponent();
            this.lind = lind;
            
            textOnlyCheckbox.Checked = lind.isTextOnly;
            stringTextBox.Text = lind.text;
            if (lind.offset != 0)
                offsetComboBox.SelectedIndex = lind.offset - 4;
            else
                offsetComboBox.SelectedIndex = offsetComboBox.Items.Count - 1;
            byte mask = lind.starMask;
            if ((mask & (1 << 1)) != 0) checkBox1.Checked = true;
            if ((mask & (1 << 2)) != 0) checkBox2.Checked = true;
            if ((mask & (1 << 3)) != 0) checkBox3.Checked = true;
            if ((mask & (1 << 4)) != 0) checkBox4.Checked = true;
            if ((mask & (1 << 5)) != 0) checkBox5.Checked = true;
            if ((mask & (1 << 6)) != 0) checkBox6.Checked = true;
            if ((mask & (1 << 7)) != 0) checkBox7.Checked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lind.isTextOnly = textOnlyCheckbox.Checked;
            lind.text = stringTextBox.Text;
            if (offsetComboBox.SelectedIndex == offsetComboBox.Items.Count - 1)
                lind.offset = 0;
            else
                lind.offset = offsetComboBox.SelectedIndex + 4;

            byte mask = 0;
            if (checkBox1.Checked) mask |= (1 << 1);
            if (checkBox2.Checked) mask |= (1 << 2);
            if (checkBox3.Checked) mask |= (1 << 3);
            if (checkBox4.Checked) mask |= (1 << 4);
            if (checkBox5.Checked) mask |= (1 << 5);
            if (checkBox6.Checked) mask |= (1 << 6);
            if (checkBox7.Checked) mask |= (1 << 7);
            lind.starMask = mask;
            this.Close();
        }
    }
}
