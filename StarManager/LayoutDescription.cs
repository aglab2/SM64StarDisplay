using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        public Bitmap redOutline;
        public Bitmap greenOutline;

        public string starAmount;

        public LayoutDescription(LineDescription[] courseDescription, LineDescription[] secretDescription, Bitmap star, string starAmount)
        {
            this.courseDescription = courseDescription;
            this.secretDescription = secretDescription;
            this.starAmount = starAmount;

            this.goldStar = star;
            this.darkStar = new Bitmap(goldStar.Width, goldStar.Height);
            if (goldStar.Width != 20 || goldStar.Height != 20)
                Compress();

            generateDarkStar();
            generateOutline();

            Trim();
        }

        public void generateDarkStar()
        {
            for (int i = 0; i < goldStar.Width; i++)
            {
                for (int j = 0; j < goldStar.Height; j++)
                {
                    double h; double s; double l;
                    Color c = goldStar.GetPixel(i, j);
                    ColorRGB crgb = new ColorRGB(c);
                    ColorRGB.RGB2HSL(crgb, out h, out s, out l);

                    ColorRGB nrgb = ColorRGB.HSL2RGB(h, 0, l);
                    ColorRGB orgb = ColorRGB.HSL2RGB(h + 0.05 > 1 ? h + 0.05 - 1 : h + 0.05, s, l);

                    Color n = Color.FromArgb(c.A, nrgb.R, nrgb.G, nrgb.B);

                    darkStar.SetPixel(i, j, n);
                }
            }
        }

        public void generateOutline()
        {
            this.redOutline = new Bitmap(goldStar.Width, goldStar.Height);
            this.greenOutline = new Bitmap(goldStar.Width, goldStar.Height);

            int[,] A = new int[goldStar.Width, goldStar.Height];
            for (int i = 0; i < goldStar.Width; i++)
            {
                for (int j = 0; j < goldStar.Height; j++)
                {
                    Color c = goldStar.GetPixel(i, j);
                    A[i, j] = c.A;
                }
            }

            int[,] outlineAlpha = A;
            for (int i = 0; i < 10; i++)
            {
                outlineAlpha = OutlineAlpha(outlineAlpha);
            }
            for (int i = 0; i < goldStar.Width; i++)
            {
                for (int j = 0; j < goldStar.Height; j++)
                {
                    redOutline.SetPixel(i, j, Color.FromArgb(outlineAlpha[i, j], 255, 0, 0));
                    greenOutline.SetPixel(i, j, Color.FromArgb(outlineAlpha[i, j], 0, 255, 0));
                }
            }
        }

        private int[,] OutlineAlpha(int[,] alpha)
        {
            int[,] ret = new int[goldStar.Width, goldStar.Height];

            for (int i = 1; i < goldStar.Width - 1; i++)
            {
                for (int j = 1; j < goldStar.Height - 1; j++)
                {
                    int tl = alpha[i + 1, j + 1];
                    int tm = alpha[i + 1, j];
                    int tr = alpha[i + 1, j - 1];

                    int ml = alpha[i, j + 1];
                    int mm = alpha[i, j];
                    int mr = alpha[i, j - 1];

                    int bl = alpha[i - 1, j + 1];
                    int bm = alpha[i - 1, j];
                    int br = alpha[i - 1, j - 1];

                    int A = tl+tm+tr+ml+mm+mr+br+bm+br;
                    A /= 9;

                    ret[i, j] = A;
                }
            }

            ret[0, 0] = alpha[0, 0] + alpha[0, 1] + alpha[1, 0]; ret[0,0] /= 3;
            ret[goldStar.Width-1, 0] = alpha[goldStar.Width-1, 0] + alpha[goldStar.Width-1, 1] + alpha[goldStar.Width-2, 0]; ret[goldStar.Width-1, 0] /= 3;
            ret[0, goldStar.Height - 1] = alpha[0, goldStar.Height-1] + alpha[0, goldStar.Height-2] + alpha[1, goldStar.Height-1]; ret[0, goldStar.Height-1] /= 3;
            ret[goldStar.Width-1, goldStar.Height-1] = alpha[goldStar.Width-1, goldStar.Height-1] + alpha[goldStar.Width-1, goldStar.Height - 2] + alpha[goldStar.Width - 2, goldStar.Height-1]; ret[goldStar.Width-1, goldStar.Height-1] /= 3;

            for (int i = 1; i < goldStar.Width - 1; i++)
            {
                ret[i, 0] = ret[i - 1, 0] + ret[i, 0] + ret[i + 1, 0] + ret[i - 1, 1] + ret[i, 1] + ret[i + 1, 1]; ret[i, 0] /= 6;
                ret[i, goldStar.Height-1] = ret[i - 1, goldStar.Height-1] + ret[i, goldStar.Height-1] + ret[i + 1, goldStar.Height-1] + ret[i - 1, goldStar.Height - 2] + ret[i, goldStar.Height - 2] + ret[i + 1, goldStar.Height - 2]; ret[i, goldStar.Height-1] /= 6;
            }

            for (int j = 1; j < goldStar.Height - 1; j++)
            {
                ret[0, j] = ret[0, j - 1] + ret[0, j] + ret[0, j + 1] + ret[1, j - 1] + ret[1, j] + ret[1, j + 1]; ret[0, j] /= 6;
                ret[goldStar.Width-1, j] = ret[goldStar.Width-1, j - 1] + ret[goldStar.Width-1, j] + ret[goldStar.Width-1, j + 1] + ret[goldStar.Width-2, j - 1] + ret[goldStar.Width-2, j] + ret[goldStar.Width-2, j + 1]; ret[goldStar.Width-1, j] /= 6;
            }

            return ret;
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
                    if (lind.text != "")
                        break;
                    else
                        courseDescription[i] = null;
                }
                else
                {
                    if (lind.text != "" || lind.starMask != 0)
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
                    if (lind.text != "")
                        break;
                    else
                        secretDescription[i] = null;
                }
                else
                {
                    if (lind.text != "" || lind.starMask != 0)
                        break;
                    else
                        secretDescription[i] = null;
                }
            }
        }

        public int GetLength()
        {
            int courseDescriptionLength = Array.FindLastIndex(courseDescription, item => item != null) + 1;
            int secretDescriptionLength = Array.FindLastIndex(secretDescription, item => item != null) + 1;
            return Math.Max(courseDescriptionLength, secretDescriptionLength);
        }

        public void Compress()
        {
            var goldCompressedImage = new Bitmap(20, 20);
            var darkCompressedImage = new Bitmap(20, 20);

            var destRect = new Rectangle(0, 0, 20, 20);

            using (var graphics = Graphics.FromImage(goldCompressedImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(goldStar, destRect, 0, 0, goldStar.Width, goldStar.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            goldStar.Dispose();
            goldStar = goldCompressedImage;

            using (var graphics = Graphics.FromImage(darkCompressedImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(darkStar, destRect, 0, 0, darkStar.Width, darkStar.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            darkStar.Dispose();
            darkStar = darkCompressedImage;
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
