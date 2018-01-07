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
    public partial class SyncLoginForm : Form
    {
        public SyncManager sm;
        byte[] data;
        
        public SyncLoginForm(byte[] data)
        {
            this.data = data;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sm = new SyncManager(serverTextBox.Text, textBox2.Text, data);
        }
    }
}
