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
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            string url = "http://www.github.com/aglab2/SM64StarDisplay";

            System.Diagnostics.Process.Start(url);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            string url = "";

            string business = "denis.kopyrin@phystech.edu";
            string description = "Star%20Display";
            string country = "US";
            string currency = "USD";

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            System.Diagnostics.Process.Start(url);
        }
    }
}
