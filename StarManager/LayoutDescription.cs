using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace StarDisplay
{
    //If isTextOnly is true, starMask is omitted and offset is omitted
    [Serializable]
    public class LineDescription
    {
        public string text;
        public bool isTextOnly;
        public byte starMask;
        public int offset;

        public LineDescription(string text, bool textOnly, byte starMask, int offset)
        {
            this.text = text;
            this.isTextOnly = textOnly;
            this.starMask = starMask;
            this.offset = offset;
        }

        public byte[] Serialize(byte control)
        {
            MemoryStream ms = new MemoryStream();

            if (isTextOnly)
            {
                control |= 1;
                byte[] txt = Encoding.UTF8.GetBytes(text.PadRight(20, '\0'));
                ms.WriteByte(control);
                ms.Write(txt, 0, text.Length);
            }
            else
            {
                byte[] txt = Encoding.UTF8.GetBytes(text.PadRight(4, '\0'));
                ms.WriteByte(control);
                ms.WriteByte((byte)(starMask >> 1));
                ms.WriteByte((byte)(offset));
                ms.Write(txt, 0, txt.Length);
            }

            return ms.ToArray();
        }

        static public LineDescription Deserialize(BinaryReader ms, out bool isSecret)
        {
            byte control = (byte)ms.ReadByte();
            if ((control & 1) == 0)
            {
                byte allStarsByte = (byte)ms.ReadByte();
                byte mask = (byte)(allStarsByte << 1);
                byte offset = (byte)ms.ReadByte();
                byte[] textByte = new byte[4];
                ms.Read(textByte, 0, 4);
                string text = new string(Encoding.UTF8.GetChars(textByte)).Trim('\0');

                isSecret = (control & 2) != 0;
                return new LineDescription(text, false, mask, offset);
            }
            else
            {
                byte[] textByte = new byte[20];
                ms.Read(textByte, 0, 20);
                string text = new string(Encoding.UTF8.GetChars(textByte)).Trim('\0');

                isSecret = (control & 2) != 0;
                return new LineDescription(text, true, 0, 0);
            }
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

        public void SaturateStar()
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

        public void Trim()
        {
            for (int i = courseDescription.Length - 1; i >= 0; i--)
            {
                LineDescription lind = courseDescription[i];
                if (lind == null) continue;
                if (lind.isTextOnly)
                {
                    if (lind.text != null)
                        break;
                    else
                        courseDescription[i] = null;
                }
                else
                {
                    if (lind.text != null || lind.starMask != 0)
                        break;
                    else
                        courseDescription[i] = null;
                }
            }

            for (int i = secretDescription.Length - 1; i >= 0; i--)
            {
                LineDescription lind = secretDescription[i];
                if (lind == null) continue;
                if (lind.isTextOnly)
                {
                    if (lind.text != null)
                        break;
                    else
                        secretDescription[i] = null;
                }
                else
                {
                    if (lind.text != null || lind.starMask != 0)
                        break;
                    else
                        secretDescription[i] = null;
                }
            }
        }

        public byte[] SerializeExternal()
        {
            Trim();
            MemoryStream ms = new MemoryStream();

            for (int i = 0; i < courseDescription.Length; i++)
            {
                LineDescription lind = courseDescription[i];
                if (lind == null) continue;
                byte[] data = lind.Serialize(0); //control&2==0 -> course
                ms.Write(data, 0, data.Length);
            }

            for (int i = 0; i < secretDescription.Length; i++)
            {
                LineDescription lind = secretDescription[i];
                if (lind == null) continue;
                byte[] data = lind.Serialize(2); //control&2!=0 -> secret
                ms.Write(data, 0, data.Length);
            }

            return ms.ToArray();
        }

        static public LayoutDescription DeserializeExternal(byte[] data, Bitmap img)
        {
            LineDescription[] courseLD = new LineDescription[16];
            LineDescription[] secretLD = new LineDescription[16];

            int courseCounter = 0;
            int secretCounter = 0;

            BinaryReader ms = new BinaryReader(new MemoryStream(data));
            int stars = 0;

            while (ms.BaseStream.Position != ms.BaseStream.Length)
            {
                bool isSecret;
                LineDescription lind = LineDescription.Deserialize(ms, out isSecret);
                if (!lind.isTextOnly) stars += MemoryManager.countStars((byte)(lind.starMask >> 1));

                if (isSecret)
                {
                    if (secretCounter == 16) break;
                    secretLD[secretCounter++] = lind;
                }
                else
                {
                    if (courseCounter == 16) break;
                    courseLD[courseCounter++] = lind;
                }
            }

            return new LayoutDescription(courseLD, secretLD, img, stars.ToString());
        }

        static public LayoutDescription GenerateDefault()
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
            secretLD[10] = new LineDescription("Overworld Stars", true, 0, 0);
            secretLD[12] = new LineDescription("Secret Stars", true, 0, 0);

            return new LayoutDescription(courseLD, secretLD, new Bitmap("images/gold_star.png"), "186");
        }
    }
}
