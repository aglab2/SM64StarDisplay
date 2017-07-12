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
        public FlagsEditForm(byte[] stars)
        {
            InitializeComponent();

            if ((stars[0x09] & (1 << 0)) != 0) checkBox1.Checked = true;
            if ((stars[0x09] & (1 << 1)) != 0) checkBox2.Checked = true;
            if ((stars[0x09] & (1 << 2)) != 0) checkBox3.Checked = true;
            if ((stars[0x09] & (1 << 3)) != 0) checkBox4.Checked = true;
            if ((stars[0x09] & (1 << 4)) != 0) checkBox5.Checked = true;
            if ((stars[0x09] & (1 << 5)) != 0) checkBox6.Checked = true;
            if ((stars[0x09] & (1 << 6)) != 0) checkBox7.Checked = true;
            if ((stars[0x09] & (1 << 7)) != 0) checkBox8.Checked = true;
        }
    }
}
