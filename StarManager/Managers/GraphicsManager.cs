using System;
using System.Drawing;
using System.Drawing.Text;

namespace StarDisplay
{
    public class GraphicsManager : CachedManager
    {
        public readonly Image reds;
        public readonly Image darkReds;

        public readonly Image secrets;
        public readonly Image darkSecrets;

        public readonly Image flipswitchOn;
        public readonly Image flipswitchOff;
        public readonly Image flipswitchDone;

        private Image background;

        Bitmap goldSquare;
        Bitmap blackSquare;

        private LayoutDescription ld;

        public Graphics graphics;

        private LastHighlight lastSHA;
        public bool IsFirstCall = true;

        private PrivateFontCollection collection;
        private FontFamily fontFamily;
        private string fontName;

        private float drawFontSize = 10;
        private float medFontSize = 12;
        private float bigFontSize = 15;

        public LayoutDescription Ld { get => ld; set { if (ld != value) isInvalidated = true; ld = value; } }
        public LastHighlight LastSHA { get => lastSHA; set { if (lastSHA != value) isInvalidated = true; lastSHA = value; } }
        public PrivateFontCollection Collection { get => collection; set { if (collection != value) isInvalidated = true; collection = value; } }
        public FontFamily FontFamily { get => fontFamily; set { if (fontFamily != value) isInvalidated = true; fontFamily = value; } }
        public string FontName { get => fontName; set { if (fontName != value) isInvalidated = true; fontName = value; } }
        public float DrawFontSize { get => drawFontSize; set { if (drawFontSize != value) isInvalidated = true; drawFontSize = value; } }
        public float MedFontSize { get => medFontSize; set { if (medFontSize != value) isInvalidated = true; medFontSize = value; } }
        public float BigFontSize { get => bigFontSize; set { if (bigFontSize != value) isInvalidated = true; bigFontSize = value; } }
        public Image Background { get => background; set { if (background != value) isInvalidated = true; background = value; } }
        
        public GraphicsManager(Graphics graphics, LayoutDescription ld)
        {
            this.Ld = ld;
            this.graphics = graphics;

            goldSquare = new Bitmap(4, 4);
            for (int i = 0; i < goldSquare.Width; i++)
                for (int j = 0; j < goldSquare.Height; j++)
                    goldSquare.SetPixel(i, j, Color.Gold);

            blackSquare = new Bitmap(4, 4);
            for (int i = 0; i < blackSquare.Width; i++)
                for (int j = 0; j < blackSquare.Height; j++)
                    blackSquare.SetPixel(i, j, Color.Black);

            flipswitchOn = new Bitmap("images/flipswitch_on.png");
            flipswitchOff = new Bitmap("images/flipswitch_off.png");
            flipswitchDone = new Bitmap("images/flipswitch_done.png");

            Bitmap redsBitmap = new Bitmap("images/red.png");
            reds = redsBitmap;
            darkReds = ImageProcessing.Desaturate(redsBitmap);

            Bitmap secretsBitmap = new Bitmap("images/secret.png");
            secrets = secretsBitmap;
            darkSecrets = ImageProcessing.Desaturate(secretsBitmap);

            Collection = new PrivateFontCollection();
            Collection.AddFontFile("font/CourierNew.ttf");
            FontFamily = new FontFamily("Courier New", Collection);

            TestFont();
        }

        public void PaintHUD(int lineOffset)
        {
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            int courseDescriptionLength = Array.FindLastIndex(Ld.courseDescription, item => item != null) + 1;
            int secretDescriptionLength = Array.FindLastIndex(Ld.secretDescription, item => item != null) + 1;
            int lastLine = Math.Max(courseDescriptionLength, secretDescriptionLength);

            if (Background != null)
                graphics.DrawImage(Background, new Rectangle(0, 0, 345, 462));

            for (int line = 0; line < courseDescriptionLength; line++)
            {
                if (Ld.courseDescription[line] == null) continue;
                DrawLine(Ld.courseDescription[line], lineOffset + line, false);
            }
            for (int line = 0; line < secretDescriptionLength; line++)
            {
                if (Ld.secretDescription[line] == null) continue;
                DrawLine(Ld.secretDescription[line], lineOffset + line, true);
            }
            
            RectangleF drawRect = new RectangleF(0, (lineOffset + lastLine) * 23 + 2, 340, 23);
            StringFormat drawFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            blackBrush.Dispose();
            drawBrush.Dispose();
        }

        public void DrawByte(byte stars, int lineNumber, bool isSecret, byte mask)
        {
            for (int i = 1; i <= 7; i++)
            {
                if ((mask & (1 << i)) == 0) continue;
                int x = (isSecret ? 180 : 0) + i * 20;
                int y = lineNumber * 23;
                bool isAcquired = (stars & (1 << (i - 1))) != 0;
                Image img = isAcquired ? Ld.goldStar : Ld.darkStar;
                graphics.DrawImage(img, x, y, 20, 20);
            }
        }

        public void DrawLine(LineDescription ld, int lineNumber, bool isSecret)
        {
            Font drawFont = new Font(FontFamily, DrawFontSize);

            SolidBrush drawBrush = new SolidBrush(Color.White);
            if (ld.isTextOnly)
            {
                RectangleF drawRect = new RectangleF((isSecret ? 180 : 0) + 7, lineNumber * 23, 170, 23);
                StringFormat drawFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };
                graphics.DrawString(ld.text, drawFont, drawBrush, drawRect, drawFormat);
            }
            else
            {
                RectangleF drawRect = new RectangleF((isSecret ? 180 : 0), lineNumber * 23, 170, 23);
                StringFormat drawFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };
                graphics.DrawString(ld.text, drawFont, drawBrush, drawRect, drawFormat);
                DrawByte(0, lineNumber, isSecret, ld.starMask);
            }
            drawBrush.Dispose();
            drawFont.Dispose();
        }

        public void AddLineHighlight(TextHighlightAction act)
        {
            if (act.Text != "")
            {
                int x = (act.IsSecret ? 180 : 0) + 2;
                int y = act.Line * 23 + 2;

                SolidBrush yellowBrush = new SolidBrush(Color.DarkGoldenrod);
                Pen yellowPen = new Pen(yellowBrush);
                graphics.DrawRectangle(yellowPen, new Rectangle(x, y, 8 * act.Text.Length, 16));
                yellowPen.Dispose();
                yellowBrush.Dispose();
            }
            else
            {
                int x = (act.IsSecret ? 180 : 0);
                int y = act.Line * 23;

                graphics.DrawImage(goldSquare, x + 8, y + 8);
            }
        }

        public override void InvalidateCache()
        {
            base.InvalidateCache();
            IsFirstCall = true;
        }

        public void SetBackground(Image image)
        {
            double ratioX = (double)345 / image.Width;
            double ratioY = (double)462 / image.Height;
            double ratio = Math.Max(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            Background = new Bitmap(345, 462);

            using (var graphics = Graphics.FromImage(Background))
                graphics.DrawImage(image, (345 - newWidth) / 2, (462 - newHeight) / 2, newWidth, newHeight);
        }

        public void DrawIntro(bool isEmulatorLoaded, bool isHackLoaded, bool isOffsetsInitialized)
        {
            Font drawFont = new Font(FontFamily, BigFontSize);
            SolidBrush whiteBrush = new SolidBrush(Color.White);
            SolidBrush greenBrush = new SolidBrush(Color.Green);

            RectangleF drawRect = new RectangleF(0, 0, 340, 50);
            StringFormat drawFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            graphics.DrawString("Welcome to Star Display!", drawFont, whiteBrush, new RectangleF(0, 0, 340, 50), drawFormat);

            bool success = isEmulatorLoaded && isHackLoaded && isOffsetsInitialized;

            drawFormat.Alignment = StringAlignment.Near;
            graphics.DrawString("1) Run Emulator", drawFont, isEmulatorLoaded ? greenBrush : whiteBrush, new RectangleF(20, 70, 340, 50), drawFormat);
            graphics.DrawString("2) Load your hack", drawFont, isHackLoaded ? greenBrush : whiteBrush, new RectangleF(20, 140, 340, 50), drawFormat);
            graphics.DrawString("3) Let Star Display init", drawFont, isOffsetsInitialized ? greenBrush : whiteBrush, new RectangleF(20, 210, 340, 50), drawFormat);
            graphics.DrawString("4) Enjoy!", drawFont, success ? greenBrush : whiteBrush, new RectangleF(20, 280, 340, 50), drawFormat);

        }

        public void TestFont()
        {
            // Setup draw font: 20 symbols in max 170 width
            float l = 0, r = 40, m = -1;
            String measureString = new string('W', 20);

            // 10 iterations is enough
            for (int iter = 0; iter < 10; iter++)
            {
                m = (l + r) / 2;

                Font drawFont = new Font(FontFamily, m);

                // Measure string.
                SizeF stringSize = new SizeF();
                stringSize = graphics.MeasureString(measureString, drawFont);

                float width = stringSize.Width; //should take 170
                if (width < 170)
                    l = m;
                else
                    r = m;
            }
            drawFontSize = m;
            Console.WriteLine("Draw: {0}", m);

            // Setup med font: 1 symbols in max 20 height
            l = 0; r = 40; m = -1;
            measureString = "W";

            // 10 iterations is enough
            for (int iter = 0; iter < 10; iter++)
            {
                m = (l + r) / 2;

                Font drawFont = new Font(FontFamily, m);
                
                // Measure string.
                SizeF stringSize = new SizeF();
                stringSize = graphics.MeasureString(measureString, drawFont);

                float height = stringSize.Height; //should take 170
                if (height < 20)
                    l = m;
                else
                    r = m;
            }
            medFontSize = m;
            Console.WriteLine("Med: {0}", m);

            // Setup med font: 1 symbols in max 20 height
            l = 0; r = 40; m = -1;
            measureString = new string('W', 27);

            // 10 iterations is enough
            for (int iter = 0; iter < 10; iter++)
            {
                m = (l + r) / 2;

                Font drawFont = new Font(FontFamily, m);

                // Measure string.
                SizeF stringSize = new SizeF();
                stringSize = graphics.MeasureString(measureString, drawFont);

                float width = stringSize.Width; //should take 170
                if (width < 340)
                    l = m;
                else
                    r = m;
            }
            bigFontSize = m;
            Console.WriteLine("Big: {0}", m);
        }
    }
}
