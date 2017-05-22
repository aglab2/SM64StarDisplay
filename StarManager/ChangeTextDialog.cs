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
    public partial class ChangeTextDialog : Form
    {
        public string text;

        public ChangeTextDialog(string text)
        {
            InitializeComponent();
            this.text = text;
            textBox.Text = text;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            text = textBox.Text;
            Close();
        }
    }
}
