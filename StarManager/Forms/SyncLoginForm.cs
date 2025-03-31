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
        public NetManager nm;
        public MemoryManager mm;
        public bool Silent = false;
        public bool isClosed = false;

        public SyncLoginForm(MemoryManager mm)
        {
            InitializeComponent();
        }

        public string GetNet64Name()
        {
            return textBoxName.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (sm != null)
            {
                sm.isClosed = true;
                sm = null;
                button1.Text = "Login";
                checkBox1.Enabled = true;
                textBoxName.Enabled = !checkBox1.Checked;
                return;
            }

            try
            {
                sm = new SyncManager("http://" + serverTextBox.Text + ":" + portTextBox.Text + "/", textBox2.Text, new byte[mm.FileLength], checkBox1.Checked, textBoxCategory.Text);
                button1.Text = "Stop";
                checkBox1.Enabled = false;
                textBoxName.Enabled = false;

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Sync", MessageBoxButtons.OK, MessageBoxIcon.Error);
                sm = null;
                return;
            }
            if (checkBox1.Checked)
            {
                MessageBox.Show("Login Finished. Reset the game (F1) for Net64 to be enabled", "Sync", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            {
                MessageBox.Show("Login Finished", "Sync", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
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

        private void netCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (nm is object)
            {
                textBoxName.Enabled = true;
                if (sm is object)
                    sm.listenNet = false;
                
                nm = null;
            }
            else
            {
                textBoxName.Enabled = false;
                if (sm is object)
                {
                    sm.listenNet = true;
                    sm.RegisterNetListener();
                }
                nm = new NetManager();
                nm.isInvalidated = true;
            }
        }

        public void UpdatePlayers(List<string> players)
        {
            var text = "";
            foreach (var player in players)
            {
                if (text.Length == 0)
                {
                    text = player;
                }
                else
                {
                    text += "/n" + player;
                }
            }
            richTextBox1.Text = text;
        }

        public string GetPlayerName()
        {
            return textBoxName.Text;
        }
    }
}
