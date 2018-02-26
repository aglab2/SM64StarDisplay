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

        public WarpDialog(byte level)
        {
            InitializeComponent();

            this.level = level;
            offsetComboBox.SelectedIndex = LevelInfo.FindByLevel(level).NaturalIndex;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Byte.TryParse(warpTextBox.Text, out warp))
                warp = 0x0A;

            if (!Byte.TryParse(areaTextBox.Text, out area))
                area = 0x01;

            level = (byte) LevelInfo.FindByNaturalIndex(offsetComboBox.SelectedIndex).Level;
            Close();
        }
    }
}
