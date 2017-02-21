using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarDisplay
{
    public partial class ColorPicker : Form
    {
        LayoutDescription ld;
        int picX, picY;
        double midHue;
        double midVal;

        Bitmap img;
        public Bitmap newImg;

        public ColorPicker(LayoutDescription ld)
        {
            this.ld = ld;
            img = ld.goldStar;

            int r = 0, g = 0, b = 0;
            double h, s, v;

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    Color c = img.GetPixel(i, j);
                    r += c.R; g += c.G; b += c.B;
                }
            }

            r /= img.Width * img.Height;
            g /= img.Width * img.Height;
            b /= img.Width * img.Height;

            Color midColor = Color.FromArgb(r, g, b);
            ColorRGB.RGB2HSL(new ColorRGB(midColor), out h, out s, out v);

            midHue = h;
            midVal = v;

            newImg = new Bitmap(img); 
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        { 
            double expectedHue = (double)picX / pictureBox.Width;
            double expectedVal = (double)picY / pictureBox.Height;
            double h, s, v;

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    Color c = img.GetPixel(i, j);
                    ColorRGB.RGB2HSL(new ColorRGB(c), out h, out s, out v);
                    h += expectedHue - midHue;

                    if (h < 0) h += 1;
                    if (h > 1) h -= 1;
                    newImg.SetPixel(i, j, ColorRGB.HSL2RGB(h, s, v));
                }
            }

            Graphics graphics = starPicture.CreateGraphics();
            graphics.Clear(Color.Black);
            graphics.DrawImage(newImg, 0, 0, 220, 220);
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            picX = e.X; picY = e.Y;
            if (e.Button != MouseButtons.Left) return;

            Bitmap img = ld.goldStar;
            
            double expectedHue = (double) e.X / pictureBox.Width;
            double expectedVal = (double) e.Y / pictureBox.Height;

            double h, s, v;

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    Color c = img.GetPixel(i, j);
                    ColorRGB.RGB2HSL(new ColorRGB(c), out h, out s, out v);
                    h += expectedHue - midHue;
                    if (h < 0) h += 1;
                    if (h > 1) h -= 1;

                    //v += expectedVal - midVal;
                    //if (v < 0) v = 0;
                    //if (v > 1) v = 1;


                    newImg.SetPixel(i, j, ColorRGB.HSL2RGB(h, s, v));
                }
            }

            Graphics graphics = starPicture.CreateGraphics();
            graphics.Clear(Color.Black);
            graphics.DrawImage(newImg, 0, 0, 220, 220);
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            Bitmap bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
            for (int i = 0; i < pictureBox.Width; i++)
            {
                for (int j = 0; j < pictureBox.Height; j++)
                {
                    ColorRGB color = ColorRGB.HSL2RGB((double)i / pictureBox.Width, 0.4, (double)j / pictureBox.Height);
                    bitmap.SetPixel(i, j, color);
                }
            }
            e.Graphics.DrawImage(bitmap,0,0);
        }
    }
}
