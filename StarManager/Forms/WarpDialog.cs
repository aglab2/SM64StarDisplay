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
    public partial class WarpDialog : Form
    {
        public byte warp;
        public byte area;
        public byte level;

        public static byte[] offsetToLevels = { 0x09, 0x18, 0x0C, 0x05, 0x04, 0x07, 0x16, 0x08, 0x17, 0x0A,
            0x0B, 0x24, 0x0D, 0x0E, 0x0F, 0x11, 0x1E, 0x13, 0x21, 0x15, 0x22, 0x1B, 0x1C, 0x1D,
            0x12, 0x1F, 0x14, 0x19, 0x10, 0x06, 0x1A };

        public WarpDialog(byte level)
        {
            InitializeComponent();

            this.level = level;
            offsetComboBox.SelectedIndex = Array.FindIndex(offsetToLevels, el => el == level);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Byte.TryParse(warpTextBox.Text, out warp))
                warp = 0x0A;

            if (!Byte.TryParse(areaTextBox.Text, out area))
                area = 0x01;

            level = offsetToLevels[offsetComboBox.SelectedIndex];
            Close();
        }
    }
}
