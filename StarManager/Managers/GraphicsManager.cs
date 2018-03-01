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

        private LayoutDescriptionEx ld;

        public Graphics graphics;

        private LastHighlight lastSHA;
        public bool IsFirstCall = true;

        private PrivateFontCollection collection;
        private FontFamily fontFamily;
        private string fontName;

        private float drawFontSize = 10;
        private float medFontSize = 12;
        private float bigFontSize = 15;

        //public float scaleCoef = (float) 7 / 8;
        public float absoluteSHeight = 23;
        public float absoluteSWidth = 20;
        //public float absoluteSOffset = 2;

        private float width = 360;

        public LayoutDescriptionEx Ld { get => ld; set { if (ld != value) isInvalidated = true; ld = value; } }
        public LastHighlight LastSHA { get => lastSHA; set { if (lastSHA != value) isInvalidated = true; lastSHA = value; } }
        public PrivateFontCollection Collection { get => collection; set { if (collection != value) isInvalidated = true; collection = value; } }
        public FontFamily FontFamily { get => fontFamily; set { if (fontFamily != value) isInvalidated = true; fontFamily = value; } }
        public string FontName { get => fontName; set { if (fontName != value) isInvalidated = true; fontName = value; } }
        public float DrawFontSize { get => drawFontSize; set { if (drawFontSize != value) isInvalidated = true; drawFontSize = value; } }
        public float MedFontSize { get => medFontSize; set { if (medFontSize != value) isInvalidated = true; medFontSize = value; } }
        public float BigFontSize { get => bigFontSize; set { if (bigFontSize != value) isInvalidated = true; bigFontSize = value; } }
        public Image Background { get => background; set { if (background != value) isInvalidated = true; background = value; } }
        public float Width { get => width; set { if (width != value) { width = value; isInvalidated = true; TestFont(); }; width = value; } }
        public float SHeight { get { return Width / 360 * absoluteSHeight * scaleCoef; } } //23
        public float SWidth { get { return Width / 360 * absoluteSWidth * scaleCoef; } } //20
        public float SOffset { get { return (SHeight - SWidth) / 4; } } //2
        public float HalfWidth { get { return (Width - SWidth) / 2; } } //170
        public float scaleCoef { get { return ld == null ? 1 : 7 / (float)ld.starsShown; } }

        public GraphicsManager(Graphics graphics, LayoutDescriptionEx ld)
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
            
            int courseDescriptionLength = Ld.courseDescription.Count;
            int secretDescriptionLength = Ld.secretDescription.Count;
            int lastLine = Math.Max(courseDescriptionLength, secretDescriptionLength);

            if (Background != null)
                graphics.DrawImage(Background, new RectangleF(0, 0, Width, Width * 2));

            for (int line = 0; line < courseDescriptionLength; line++)
            {
                if (Ld.courseDescription[line] == null) continue;
                Ld.courseDescription[line].DrawBase(this, lineOffset + line, false);
            }
            for (int line = 0; line < secretDescriptionLength; line++)
            {
                if (Ld.secretDescription[line] == null) continue;
                Ld.secretDescription[line].DrawBase(this, lineOffset + line, true);
            }
            
            RectangleF drawRect = new RectangleF(0, (lineOffset + lastLine) * SHeight + 2, Width, SHeight);
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
            for (int i = 0; i < ld.starsShown; i++)
            {
                if ((mask & (1 << i)) == 0) continue;
                float x = (isSecret ? Width / 2 : 0) + (i + 1) * SWidth;
                float y = lineNumber * SHeight + SOffset;
                bool isAcquired = (stars & (1 << i)) != 0;
                Image img = isAcquired ? Ld.goldStar : Ld.darkStar;
                graphics.DrawImage(img, x, y, SWidth, SWidth);
            }
        }

        public void AddLineHighlight(TextHighlightAction act)
        {
            if (act.Text != "")
            {
                Font drawFont = new Font(FontFamily, DrawFontSize);

                float x = (act.IsSecret ? (Width / 2) : 0) + SOffset;
                float y = act.Line * SHeight + 4 * SOffset;

                SizeF size = graphics.MeasureString(act.Text, drawFont);
                drawFont.Dispose();

                SolidBrush yellowBrush = new SolidBrush(Color.DarkGoldenrod);
                Pen yellowPen = new Pen(yellowBrush);
                graphics.DrawRectangle(yellowPen, x, y, size.Width - 2 * SOffset, size.Height);
                yellowPen.Dispose();
                yellowBrush.Dispose();
            }
            else
            {
                float x = (act.IsSecret ? (Width / 2) : 0) + SWidth / 20 * 8;
                float y = act.Line * SHeight + SHeight / 20 * 8;
                
                graphics.DrawImage(goldSquare, x, y, SWidth / 5, SWidth / 5);
            }
        }

        public override void InvalidateCache()
        {
            base.InvalidateCache();
            IsFirstCall = true;
        }

        public void SetBackground(Image image)
        {
            double ratioX = (double)Width / image.Width;
            double ratioY = (double)Width * 2 / image.Height;
            double ratio = Math.Max(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);
            
            Background = new Bitmap((int)Width, (int)Width * 2);

            using (var graphics = Graphics.FromImage(Background))
                graphics.DrawImage(image, (Width - newWidth) / 2, (Width * 2 - newHeight) / 2, newWidth, newHeight);
        }

        public void DrawIntro(bool isEmulatorLoaded, bool isHackLoaded, bool isOffsetsInitialized)
        {
            Font drawFont = new Font(FontFamily, BigFontSize);
            SolidBrush whiteBrush = new SolidBrush(Color.White);
            SolidBrush greenBrush = new SolidBrush(Color.Green);

            RectangleF drawRect = new RectangleF(0, 0, Width, SHeight * (float)2.5);
            StringFormat drawFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            graphics.DrawString("Welcome to Star Display!", drawFont, whiteBrush, new RectangleF(0, 0, Width, SHeight * (float)2.5), drawFormat);

            bool success = isEmulatorLoaded && isHackLoaded && isOffsetsInitialized;

            drawFormat.Alignment = StringAlignment.Near;
            graphics.DrawString("1) Run Emulator", drawFont, isEmulatorLoaded ? greenBrush : whiteBrush, new RectangleF(SWidth, SHeight * 3, Width - SWidth, SHeight * 2), drawFormat);
            graphics.DrawString("2) Load your hack", drawFont, isHackLoaded ? greenBrush : whiteBrush, new RectangleF(SWidth, SHeight * 6, Width - SWidth, SHeight * 2), drawFormat);
            graphics.DrawString("3) Let Star Display init", drawFont, isOffsetsInitialized ? greenBrush : whiteBrush, new RectangleF(SWidth, SHeight * 9, Width - SWidth, SHeight * 2), drawFormat);
            graphics.DrawString("4) Enjoy!", drawFont, success ? greenBrush : whiteBrush, new RectangleF(SWidth, SHeight * 12, Width - SWidth, SHeight * 2), drawFormat);

        }

        public void TestFont()
        {
            Image randomImage = new Bitmap(300, 50);
            Graphics g = Graphics.FromImage(randomImage);

            // Setup draw font: 20 symbols in max 170 width
            float l = 0, r = 40, m = -1;
            String measureString = new string('W', 2);

            // 10 iterations is enough
            for (int iter = 0; iter < 10; iter++)
            {
                m = (l + r) / 2;

                Font drawFont = new Font(FontFamily, m);

                // Measure string.
                SizeF stringSize = new SizeF();
                stringSize = g.MeasureString(measureString, drawFont);

                float width = stringSize.Width; //should take 170
                if (width < SWidth)
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
                stringSize = g.MeasureString(measureString, drawFont);

                float height = stringSize.Height; //should take HalfWidth
                if (height < SWidth)
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
                stringSize = g.MeasureString(measureString, drawFont);

                float width = stringSize.Width; //should take 170
                float localScaleCoef = ld == null ? 1 : scaleCoef;

                if (width < HalfWidth * 2 * localScaleCoef)
                    l = m;
                else
                    r = m;
            }
            bigFontSize = m;
            Console.WriteLine("Big: {0}", m);

            g.Dispose();
            randomImage.Dispose();
        }
    }
}
