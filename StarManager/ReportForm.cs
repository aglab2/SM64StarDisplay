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
    public partial class ReportForm : Form
    {
        Exception ex;

        public ReportForm(Exception ex)
        {
            this.ex = ex;
            InitializeComponent();
        }

        public void init()
        {
            errorTextBox.Text = ex.Message + "\n\n" + ex.StackTrace;
        }
    }
}
