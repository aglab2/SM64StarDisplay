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
        public LineDescriptionEx lind;
        public int starsShown;

        private byte oldMask;

        public Settings(LineDescriptionEx lind, int starsShown)
        {
            InitializeComponent();
            this.lind = lind;
            this.starsShown = starsShown;
            if (starsShown == 8)
                this.is8StarsCheckbox.Checked = true;
                    
            textOnlyCheckbox.Checked = lind is TextOnlyLineDescription;

            if (lind is TextOnlyLineDescription told)
            {
                stringTextBox.Text = told.text;
                oldMask = 0x7F;
            }
            if (lind is StarsLineDescription sld)
            {
                stringTextBox.Text = sld.text;
                if (sld.offset != 0)
                    offsetComboBox.SelectedIndex = sld.offset - 8;
                else
                    offsetComboBox.SelectedIndex = 0;

                byte mask = sld.highlightStarMask;
                if ((mask & (1 << 0)) != 0) checkBox1.Checked = true;
                if ((mask & (1 << 1)) != 0) checkBox2.Checked = true;
                if ((mask & (1 << 2)) != 0) checkBox3.Checked = true;
                if ((mask & (1 << 3)) != 0) checkBox4.Checked = true;
                if ((mask & (1 << 4)) != 0) checkBox5.Checked = true;
                if ((mask & (1 << 5)) != 0) checkBox6.Checked = true;
                if ((mask & (1 << 6)) != 0) checkBox7.Checked = true;
                if ((mask & (1 << 7)) != 0) checkBox8.Checked = true;

                oldMask = sld.starMask;
                highlightOffsetTextBox.Text = sld.highlightOffset.ToString();
            }

            highlightPresetComboBox.SelectedIndex = 0;

            stringTextBox.Select();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textOnlyCheckbox.Checked)
            {
                lind = new TextOnlyLineDescription(stringTextBox.Text);
            }
            else
            {
                byte mask = 0;
                int offset = 0;

                if (checkBox1.Checked) mask |= (1 << 0);
                if (checkBox2.Checked) mask |= (1 << 1);
                if (checkBox3.Checked) mask |= (1 << 2);
                if (checkBox4.Checked) mask |= (1 << 3);
                if (checkBox5.Checked) mask |= (1 << 4);
                if (checkBox6.Checked) mask |= (1 << 5);
                if (checkBox7.Checked) mask |= (1 << 6);
                if (checkBox8.Checked) mask |= (1 << 7);

                offset = offsetComboBox.SelectedIndex + 8;
                int.TryParse(highlightOffsetTextBox.Text, out int highlightOffset);

                lind = new StarsLineDescription(stringTextBox.Text, oldMask, offset, mask, highlightOffset);
            }

            starsShown = is8StarsCheckbox.Checked ? 8 : 7;

            this.Close();
        }

        private void highlightPresetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = highlightPresetComboBox.SelectedIndex;
            if (index == 1)
            {
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                checkBox4.Checked = false;
                checkBox5.Checked = false;
                checkBox6.Checked = false;
                checkBox7.Checked = false;
                checkBox8.Checked = false;
            }
            if (index == 2)
            {
                highlightOffsetTextBox.Text = "11";
                checkBox1.Checked = false;
                checkBox2.Checked = true;
                checkBox3.Checked = false;
                checkBox4.Checked = false;
                checkBox5.Checked = false;
                checkBox6.Checked = false;
                checkBox7.Checked = false;
                checkBox8.Checked = false;
            }

            if (index == 3)
            {
                highlightOffsetTextBox.Text = "11";
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox3.Checked = true;
                checkBox4.Checked = false;
                checkBox5.Checked = false;
                checkBox6.Checked = false;
                checkBox7.Checked = false;
                checkBox8.Checked = false;
            }

            if (index == 4)
            {
                highlightOffsetTextBox.Text = "11";
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                checkBox4.Checked = true;
                checkBox5.Checked = false;
                checkBox6.Checked = false;
                checkBox7.Checked = false;
                checkBox8.Checked = false;
            }

            if (index == 5)
            {
                highlightOffsetTextBox.Text = "11";
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                checkBox4.Checked = false;
                checkBox5.Checked = true;
                checkBox6.Checked = false;
                checkBox7.Checked = true;
                checkBox8.Checked = false;
            }

            if (index == 6)
            {
                highlightOffsetTextBox.Text = "11";
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                checkBox4.Checked = false;
                checkBox5.Checked = false;
                checkBox6.Checked = true;
                checkBox7.Checked = false;
                checkBox8.Checked = true;
            }
        }
    }
}
