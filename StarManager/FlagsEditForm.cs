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
    public partial class FlagsEditForm : Form
    {
        //Yeah, all this class is bad
        public byte[] stars;
        bool isInitialized = false;

        public FlagsEditForm(byte[] defstars)
        {
            InitializeComponent();

            this.stars = (byte[]) defstars.Clone();
            if ((stars[0x01] & (1 << 0)) != 0) checkBox1.Checked = true;
            if ((stars[0x01] & (1 << 1)) != 0) checkBox2.Checked = true;
            if ((stars[0x01] & (1 << 2)) != 0) checkBox3.Checked = true;
            if ((stars[0x01] & (1 << 3)) != 0) checkBox4.Checked = true;
            if ((stars[0x01] & (1 << 4)) != 0) checkBox5.Checked = true;
            if ((stars[0x01] & (1 << 5)) != 0) checkBox6.Checked = true;
            if ((stars[0x01] & (1 << 6)) != 0) checkBox7.Checked = true;
            if ((stars[0x01] & (1 << 7)) != 0) checkBox8.Checked = true;

            if ((stars[0x02] & (1 << 0)) != 0) checkBox16.Checked = true;
            if ((stars[0x02] & (1 << 1)) != 0) checkBox15.Checked = true;
            if ((stars[0x02] & (1 << 2)) != 0) checkBox14.Checked = true;
            if ((stars[0x02] & (1 << 3)) != 0) checkBox13.Checked = true;
            if ((stars[0x02] & (1 << 4)) != 0) checkBox12.Checked = true;
            if ((stars[0x02] & (1 << 5)) != 0) checkBox11.Checked = true;
            if ((stars[0x02] & (1 << 6)) != 0) checkBox10.Checked = true;
            if ((stars[0x02] & (1 << 7)) != 0) checkBox9.Checked = true;

            if ((stars[0x03] & (1 << 0)) != 0) checkBox24.Checked = true;
            if ((stars[0x03] & (1 << 1)) != 0) checkBox23.Checked = true;
            if ((stars[0x03] & (1 << 2)) != 0) checkBox22.Checked = true;
            if ((stars[0x03] & (1 << 3)) != 0) checkBox21.Checked = true;
            if ((stars[0x03] & (1 << 4)) != 0) checkBox20.Checked = true;
            if ((stars[0x03] & (1 << 5)) != 0) checkBox19.Checked = true;
            if ((stars[0x03] & (1 << 6)) != 0) checkBox18.Checked = true;
            if ((stars[0x03] & (1 << 7)) != 0) checkBox17.Checked = true;
            isInitialized = true;
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (!isInitialized) return;
            if (checkBox1.Checked == true) stars[0x01] |= 1 << 0; else stars[0x01] &= (0b11111111 ^ (1 << 0));
            if (checkBox2.Checked == true) stars[0x01] |= 1 << 1; else stars[0x01] &= (0b11111111 ^ (1 << 1));
            if (checkBox3.Checked == true) stars[0x01] |= 1 << 2; else stars[0x01] &= (0b11111111 ^ (1 << 2));
            if (checkBox4.Checked == true) stars[0x01] |= 1 << 3; else stars[0x01] &= (0b11111111 ^ (1 << 3));
            if (checkBox5.Checked == true) stars[0x01] |= 1 << 4; else stars[0x01] &= (0b11111111 ^ (1 << 4));
            if (checkBox6.Checked == true) stars[0x01] |= 1 << 5; else stars[0x01] &= (0b11111111 ^ (1 << 5));
            if (checkBox7.Checked == true) stars[0x01] |= 1 << 6; else stars[0x01] &= (0b11111111 ^ (1 << 6));
            if (checkBox8.Checked == true) stars[0x01] |= 1 << 7; else stars[0x01] &= (0b11111111 ^ (1 << 7));

            if (checkBox16.Checked == true) stars[0x02] |= 1 << 0; else stars[0x02] &= (0b11111111 ^ (1 << 0));
            if (checkBox15.Checked == true) stars[0x02] |= 1 << 1; else stars[0x02] &= (0b11111111 ^ (1 << 1));
            if (checkBox14.Checked == true) stars[0x02] |= 1 << 2; else stars[0x02] &= (0b11111111 ^ (1 << 2));
            if (checkBox13.Checked == true) stars[0x02] |= 1 << 3; else stars[0x02] &= (0b11111111 ^ (1 << 3));
            if (checkBox12.Checked == true) stars[0x02] |= 1 << 4; else stars[0x02] &= (0b11111111 ^ (1 << 4));
            if (checkBox11.Checked == true) stars[0x02] |= 1 << 5; else stars[0x02] &= (0b11111111 ^ (1 << 5));
            if (checkBox10.Checked == true) stars[0x02] |= 1 << 6; else stars[0x02] &= (0b11111111 ^ (1 << 6));
            if (checkBox9.Checked == true) stars[0x02] |= 1 << 7; else stars[0x02] &= (0b11111111 ^ (1 << 7));

            if (checkBox24.Checked == true) stars[0x03] |= 1 << 0; else stars[0x03] &= (0b11111111 ^ (1 << 0));
            if (checkBox23.Checked == true) stars[0x03] |= 1 << 1; else stars[0x03] &= (0b11111111 ^ (1 << 1));
            if (checkBox22.Checked == true) stars[0x03] |= 1 << 2; else stars[0x03] &= (0b11111111 ^ (1 << 2));
            if (checkBox21.Checked == true) stars[0x03] |= 1 << 3; else stars[0x03] &= (0b11111111 ^ (1 << 3));
            if (checkBox20.Checked == true) stars[0x03] |= 1 << 4; else stars[0x03] &= (0b11111111 ^ (1 << 4));
            if (checkBox19.Checked == true) stars[0x03] |= 1 << 5; else stars[0x03] &= (0b11111111 ^ (1 << 5));
            if (checkBox18.Checked == true) stars[0x03] |= 1 << 6; else stars[0x03] &= (0b11111111 ^ (1 << 6));
            if (checkBox17.Checked == true) stars[0x03] |= 1 << 7; else stars[0x03] &= (0b11111111 ^ (1 << 7));
        }
    }
}
