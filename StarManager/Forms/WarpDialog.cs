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
            LevelOffsetsDescription lod = LevelInfo.FindByLevel(level);
            offsetComboBox.SelectedIndex = (lod == null) ? -1 : lod.NaturalIndex;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!Byte.TryParse(warpTextBox.Text, out warp))
                warp = 0x0A;

            if (!Byte.TryParse(areaTextBox.Text, out area))
                area = 0x01;
            ;
            LevelOffsetsDescription lod = LevelInfo.FindByNaturalIndex(offsetComboBox.SelectedIndex);
            level = (byte) ((lod == null) ? 1 : lod.Level);
            Close();
        }
    }
}
