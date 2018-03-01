using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    [Serializable]
    public abstract class LineDescriptionEx
    {
        public abstract void DrawBase(GraphicsManager gm, int lineNumber, bool isSecret);
        public abstract bool IsEmpty();
    }

    [Serializable]
    public class TextOnlyLineDescription : LineDescriptionEx
    {
        public string text;
        public TextOnlyLineDescription(string text)
        {
            this.text = text;
        }

        public override void DrawBase(GraphicsManager gm, int lineNumber, bool isSecret)
        {
            Font drawFont = new Font(gm.FontFamily, gm.DrawFontSize);

            SolidBrush drawBrush = new SolidBrush(Color.White);
            RectangleF drawRect = new RectangleF((isSecret ? (gm.Width / 2) : 0) + gm.SWidth / 2, lineNumber * gm.SHeight, gm.HalfWidth, gm.SHeight);
            StringFormat drawFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };
            gm.graphics.DrawString(text, drawFont, drawBrush, drawRect, drawFormat);
            drawBrush.Dispose();
            drawFont.Dispose();
        }

        public override bool IsEmpty()
        {
            return text == "";
        }
    }

    [Serializable]
    public class StarsLineDescription : LineDescriptionEx
    {
        public string text;

        public byte starMask;
        public int offset;

        public byte highlightStarMask;
        public int highlightOffset;

        public StarsLineDescription(string text, byte starMask, int offset, byte highlightStarMask, int highlightOffset)
        {
            this.text = text;
            this.starMask = starMask;
            this.offset = offset;
            this.highlightStarMask = highlightStarMask;
            this.highlightOffset = highlightOffset;
        }

        public override void DrawBase(GraphicsManager gm, int lineNumber, bool isSecret)
        {
            Font drawFont = new Font(gm.FontFamily, gm.DrawFontSize);

            SolidBrush drawBrush = new SolidBrush(Color.White);
            RectangleF drawRect = new RectangleF((isSecret ? (gm.Width / 2) : 0), lineNumber * gm.SHeight, gm.HalfWidth, gm.SHeight);
            StringFormat drawFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };
            gm.graphics.DrawString(text, drawFont, drawBrush, drawRect, drawFormat);
            gm.DrawByte(0, lineNumber, isSecret, starMask);

            drawBrush.Dispose();
            drawFont.Dispose();
        }

        public override bool IsEmpty()
        {
            return starMask == 0 && highlightStarMask == 0 && text == "";
        }
    }
    
    [Serializable]
    public class LayoutDescriptionEx
    {
        public List<LineDescriptionEx> courseDescription;
        public List<LineDescriptionEx> secretDescription;

        public Bitmap goldStar;
        public Bitmap darkStar;
        public Bitmap redOutline;
        public Bitmap greenOutline;

        public string starAmount;

        public int starsShown;

        // Method for converting from old to new Layouts
        public LayoutDescriptionEx(LayoutDescription ld)
        {
            starsShown = 7;

            courseDescription = new List<LineDescriptionEx>();
            secretDescription = new List<LineDescriptionEx>();

            foreach (LineDescription lined in ld.courseDescription)
            {
                LineDescriptionEx lde = null;

                if (lined != null)
                {
                    if (lined.isTextOnly)
                    {
                        lde = new TextOnlyLineDescription(lined.text);
                    }
                    else
                    {
                        byte highlightStarMask = 0;
                        int highlightStarOffset = 0;

                        if (lined.text == "WC")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 1;
                        }

                        if (lined.text == "MC")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 2;
                        }

                        if (lined.text == "VC")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 3;
                        }

                        if (lined.text == "B1")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 4 | 1 << 6;
                        }

                        if (lined.text == "B2")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 5 | 1 << 7;
                        }

                        lde = new StarsLineDescription(lined.text, (byte)(lined.starMask >> 1 | (1 << 7)), lined.offset + 8, highlightStarMask, highlightStarOffset);
                    }
                }

                courseDescription.Add(lde);
            }

            foreach (LineDescription lined in ld.secretDescription)
            {
                LineDescriptionEx lde = null;

                if (lined != null)
                {
                    if (lined.isTextOnly)
                    {
                        lde = new TextOnlyLineDescription(lined.text);
                    }
                    else
                    {
                        byte highlightStarMask = 0;
                        int highlightStarOffset = 0;

                        if (lined.text == "WC")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 1;
                        }

                        if (lined.text == "MC")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 2;
                        }

                        if (lined.text == "VC")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 3;
                        }

                        if (lined.text == "B1")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 4 | 1 << 6;
                        }

                        if (lined.text == "B2")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 5 | 1 << 7;
                        }

                        lde = new StarsLineDescription(lined.text, (byte)(lined.starMask >> 1 | (1 << 7)), lined.offset + 8, highlightStarMask, highlightStarOffset);
                    }
                }

                secretDescription.Add(lde);
            }

            this.goldStar = ld.goldStar;
            this.darkStar = ld.darkStar;
            this.redOutline = ld.redOutline;
            this.greenOutline = ld.greenOutline;
            this.starAmount = ld.starAmount;

            Trim();
        }

        public LayoutDescriptionEx(LineDescription[] courseDescriptionOut, LineDescription[] secretDescriptionOut, Bitmap star, string starAmount)
        {
            starsShown = 7;

            courseDescription = new List<LineDescriptionEx>();
            secretDescription = new List<LineDescriptionEx>();

            foreach (LineDescription lined in courseDescriptionOut)
            {
                LineDescriptionEx lde = null;

                if (lined != null)
                {
                    if (lined.isTextOnly)
                    {
                        lde = new TextOnlyLineDescription(lined.text);
                    }
                    else
                    {
                        byte highlightStarMask = 0;
                        int highlightStarOffset = 0;

                        if (lined.text == "WC")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 1;
                        }

                        if (lined.text == "MC")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 2;
                        }

                        if (lined.text == "VC")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 3;
                        }

                        if (lined.text == "B1")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 4 | 1 << 6;
                        }

                        if (lined.text == "B2")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 5 | 1 << 7;
                        }

                        lde = new StarsLineDescription(lined.text, (byte)((lined.starMask >> 1) | 1 << 7), lined.offset + 8, highlightStarMask, highlightStarOffset);
                    }
                }

                courseDescription.Add(lde);
            }

            foreach (LineDescription lined in secretDescriptionOut)
            {
                LineDescriptionEx lde = null;

                if (lined != null)
                {
                    if (lined.isTextOnly)
                    {
                        lde = new TextOnlyLineDescription(lined.text);
                    }
                    else
                    {
                        byte highlightStarMask = 0;
                        int highlightStarOffset = 0;

                        if (lined.text == "WC")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 1;
                        }

                        if (lined.text == "MC")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 2;
                        }

                        if (lined.text == "VC")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 3;
                        }

                        if (lined.text == "B1")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 4 | 1 << 6;
                        }

                        if (lined.text == "B2")
                        {
                            highlightStarOffset = 0xB;
                            highlightStarMask = 1 << 5 | 1 << 7;
                        }

                        lde = new StarsLineDescription(lined.text, (byte)(lined.starMask >> 1 | (1 << 7)), lined.offset + 8, highlightStarMask, highlightStarOffset);
                    }
                }

                secretDescription.Add(lde);
            }
            
            this.starAmount = starAmount;

            Trim();

            goldStar = star;
            darkStar = new Bitmap(goldStar.Width, goldStar.Height);
            if (goldStar.Width != 20 || goldStar.Height != 20)
                Compress();

            GenerateDarkStar();
            GenerateOutline();
        }
        
        public LayoutDescriptionEx(List<LineDescriptionEx> courseDescription, List<LineDescriptionEx> secretDescription, Bitmap star, string starAmount, int starsShown)
        {
            this.starsShown = starsShown;

            this.courseDescription = new List<LineDescriptionEx>(courseDescription);
            this.secretDescription = new List<LineDescriptionEx>(secretDescription);
            this.starAmount = starAmount;

            this.goldStar = star;
            this.darkStar = new Bitmap(goldStar.Width, goldStar.Height);
            if (goldStar.Width != 20 || goldStar.Height != 20)
                Compress();

            GenerateDarkStar();
            GenerateOutline();

            Trim();
        }

        public bool isValid()
        {
            return courseDescription == null || secretDescription == null;
        }

        public void GenerateDarkStar()
        {
            darkStar = ImageProcessing.Desaturate(goldStar);
        }

        public void GenerateOutline()
        {
            this.redOutline = new Bitmap(goldStar.Width, goldStar.Height);
            this.greenOutline = new Bitmap(goldStar.Width, goldStar.Height);

            int[,] outlineAlpha = ImageProcessing.GetAlpha(goldStar);
            for (int i = 0; i < 10; i++)
            {
                outlineAlpha = ImageProcessing.OutlineAlpha(outlineAlpha, goldStar);
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

        public void SaturateStar()
        {
            ImageProcessing.Saturate(goldStar);
        }

        public void Trim()
        {
            for (int i = courseDescription.Count() - 1; i >= 0; i--)
            {
                LineDescriptionEx lind = courseDescription[i];
                if (lind == null) continue;

                if (lind.IsEmpty())
                    courseDescription.RemoveAt(i);
            }

            for (int i = secretDescription.Count() - 1; i >= 0; i--)
            {
                LineDescriptionEx lind = secretDescription[i];
                if (lind == null) continue;

                if (lind.IsEmpty())
                    secretDescription.RemoveAt(i);
            }
        }

        public void PrepareForEdit()
        {
            Trim();
            int courseDescriptionLength = courseDescription.Count();
            int secretDescriptionLength = secretDescription.Count();
            int length = Math.Max(courseDescriptionLength, secretDescriptionLength);
            for (int currentIndex = courseDescriptionLength; currentIndex <= length; currentIndex++)
            {
                courseDescription.Add(new TextOnlyLineDescription(""));
            }

            for (int currentIndex = secretDescriptionLength; currentIndex <= length; currentIndex++)
            {
                secretDescription.Add(new TextOnlyLineDescription(""));
            }
        }
        
        public int GetLength()
        {
            int courseDescriptionLength = courseDescription.Count();
            int secretDescriptionLength = secretDescription.Count();
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

        /*
        public byte[] SerializeExternal()
        {
            Trim();
            MemoryStream ms = new MemoryStream();

            foreach (LineDescriptionEx lind in courseDescription)
            {
                if (lind == null) continue;
                byte[] data = lind.Serialize(0); //control&2==0 -> course
                ms.Write(data, 0, data.Length);
            }

            foreach (LineDescriptionEx lind in secretDescription)
            {
                if (lind == null) continue;
                byte[] data = lind.Serialize(2); //control&2!=0 -> secret
                ms.Write(data, 0, data.Length);
            }

            return ms.ToArray();
        }

        static public LayoutDescriptionEx DeserializeExternal(byte[] data, Bitmap img)
        {
            List<LineDescription> courseLD = new List<LineDescription>();
            List<LineDescription> secretLD = new List<LineDescription>();

            int courseCounter = 0;
            int secretCounter = 0;

            BinaryReader ms = new BinaryReader(new MemoryStream(data));
            int stars = 0;

            while (ms.BaseStream.Position != ms.BaseStream.Length)
            {
                LineDescription lind = LineDescription.Deserialize(ms, out bool isSecret);
                if (!lind.isTextOnly) stars += MemoryManager.countStars((byte)(lind.starMask));

                if (isSecret)
                {
                    secretLD[secretCounter++] = lind;
                }
                else
                {
                    courseLD[courseCounter++] = lind;
                }
            }

            return new LayoutDescriptionEx(courseLD, secretLD, img, stars.ToString());
        }
        */

        public void RecountStars()
        {
            int stars = 0;

            foreach (LineDescriptionEx lind in courseDescription)
            {
                if (lind is StarsLineDescription sld)
                {
                    stars += MemoryManager.countStars(sld.starMask, starsShown);
                }
            }

            foreach (LineDescriptionEx lind in secretDescription)
            {
                if (lind is StarsLineDescription sld)
                {
                    stars += MemoryManager.countStars(sld.starMask, starsShown);
                }
            }

            starAmount = stars.ToString();
        }

        //TODO: Store in file
        static public LayoutDescriptionEx GenerateDefault()
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

            return new LayoutDescriptionEx(courseLD, secretLD, new Bitmap("images/gold_star.png"), "182");
        }
    }
}
