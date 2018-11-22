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
        public bool Silent = false;
        byte[] data;
        public bool isClosed = false;

        public SyncLoginForm(byte[] data)
        {
            this.data = data;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (sm != null)
            {
                sm.isClosed = true;
                sm = null;
                button1.Text = "Login";
                return;
            }

            try
            {
                sm = new SyncManager("http://" + serverTextBox.Text + ":" + portTextBox.Text + "/", textBox2.Text, data);
                button1.Text = "Stop";
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Sync", MessageBoxButtons.OK, MessageBoxIcon.Error);
                sm = null;
                return;
            }
            MessageBox.Show("Login Finished", "Sync", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void SyncLoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && sm != null)
            {
                e.Cancel = true;
            }
            else
            {
                isClosed = true;
            }
        }
    }
}
