using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    //If isTextOnly is true, starMask is omitted
    [Serializable]
    public class LineDescription
    {
        public string text;
        public bool isTextOnly;
        public byte starMask;
        public int offset;

        public LineDescription(string text, bool isTextOnly, byte starMask, int offset)
        {
            this.text = text;
            this.isTextOnly = isTextOnly;
            this.starMask = starMask;
            this.offset = offset;
        }
    }

    [Serializable]
    public class LayoutDescription
    {
        public LineDescription[] courseDescription; //16 elements
        public LineDescription[] secretDescription;

        public Bitmap goldStar;
        public Bitmap darkStar;

        public string starAmount;

        public LayoutDescription(LineDescription[] courseDescription, LineDescription[] secretDescription, Bitmap star, string starAmount)
        {
            this.courseDescription = courseDescription;
            this.secretDescription = secretDescription;
            this.starAmount = starAmount;

            this.goldStar = star;
            Bitmap darkStar = new Bitmap(goldStar.Width, goldStar.Height);

            for (int i = 0; i < goldStar.Width; i++)
            {
                for (int j = 0; j < goldStar.Height; j++)
                {
                    double h; double s; double l;
                    Color c = star.GetPixel(i, j);
                    ColorRGB crgb = new ColorRGB(c);
                    ColorRGB.RGB2HSL(crgb, out h, out s, out l);

                    s = 0;

                    ColorRGB nrgb = ColorRGB.HSL2RGB(h, s, l);
                    Color n = Color.FromArgb(c.A, nrgb.R, nrgb.G, nrgb.B);
                    darkStar.SetPixel(i, j, n);
                }
            }

            this.darkStar = darkStar;
        }

        public void saturateStar()
        {
            for (int i = 0; i < goldStar.Width; i++)
            {
                for (int j = 0; j < goldStar.Height; j++)
                {
                    double h; double s; double l;
                    Color c = goldStar.GetPixel(i, j);
                    ColorRGB crgb = new ColorRGB(c);
                    ColorRGB.RGB2HSL(crgb, out h, out s, out l);

                    s = Math.Min(s + 0.1, 1);

                    ColorRGB nrgb = ColorRGB.HSL2RGB(h, s, l);
                    Color n = Color.FromArgb(c.A, nrgb.R, nrgb.G, nrgb.B);
                    goldStar.SetPixel(i, j, n);
                }
            }
        }

        static public LayoutDescription generateDefault()
        {
            LineDescription[] courseLD = new LineDescription[16];
            LineDescription[] secretLD = new LineDescription[16];

            int[] linesForSecrets = { 0, 1, 2, 3, 9, 5, 6, 7, 13, 14, 15, 11 };
            string[] namesForSecrets = { "--", "B1", "B2", "B3", "Sl", "MC", "WC", "VC", "S1", "S2", "S3", "OW" };

            courseLD[0] = new LineDescription("Main Courses", true, 0, 0);
            for (int course = 1; course <= 15; course++)
            {
                string drawString = course.ToString("D2");
                courseLD[course] = new LineDescription(drawString, false, 255, course + 3);
            }

            for (int course = 1; course <= 10; course++) //Secret course
            {
                secretLD[linesForSecrets[course]] = new LineDescription(namesForSecrets[course], false, 255, course + 18);
            }
            secretLD[linesForSecrets[11]] = new LineDescription(namesForSecrets[11], false, 255, 0);

            secretLD[0] = new LineDescription("Bowser Courses", true, 0, 0);
            secretLD[4] = new LineDescription("Cap Levels", true, 0, 0);
            secretLD[8] = new LineDescription("Slide", true, 0, 0);
            secretLD[10] = new LineDescription("OverWorld stars", true, 0, 0);
            secretLD[12] = new LineDescription("Secret Stars", true, 0, 0);

            return new LayoutDescription(courseLD, secretLD, new Bitmap("images/gold_star.png"), "");
        }
    }
}
