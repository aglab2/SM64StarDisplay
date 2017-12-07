using System;
using System.Drawing;
using System.Drawing.Text;

namespace StarDisplay
{
    public class GraphicsManager
    {
        public Image reds;
        public Image darkReds;

        public Image secrets;
        public Image darkSecrets;

        public Image flipswitchOn;
        public Image flipswitchOff;
        public Image flipswitchDone;

        public Image background;

        Bitmap goldSquare;
        Bitmap blackSquare;

        public LayoutDescription ld;
        Graphics _graphics;

        public LastHighlight lastSHA;
        public bool IsFirstCall = true;

        public PrivateFontCollection collection;
        public FontFamily fontFamily;
        public string fontName;

        public int drawFontSize = 10;
        public int bigFontSize = 15;

        public string StarText = "Savestateless Stars";

        public bool areCollectablesMinimized;

        public Graphics graphics
        {
            get { return _graphics; }
            set { /*_graphics.Dispose();*/ _graphics = value; }
        }

        public GraphicsManager(Graphics graphics, LayoutDescription ld)
        {
            this.ld = ld;
            _graphics = graphics;

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

            collection = new PrivateFontCollection();
            collection.AddFontFile("font/CourierNew.ttf");
            fontFamily = new FontFamily("Courier New", collection);

            areCollectablesMinimized = false;
        }

        public void PaintHUD(int lineOffset)
        {
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            SolidBrush drawBrush = new SolidBrush(Color.White);
            
            Font bigFont = new Font(fontFamily, bigFontSize);

            int courseDescriptionLength = Array.FindLastIndex(ld.courseDescription, item => item != null) + 1;
            int secretDescriptionLength = Array.FindLastIndex(ld.secretDescription, item => item != null) + 1;
            int lastLine = Math.Max(courseDescriptionLength, secretDescriptionLength);

            if (background != null)
                graphics.DrawImage(background, new Rectangle(0, 0, 345, 462));

            for (int line = 0; line < courseDescriptionLength; line++)
            {
                if (ld.courseDescription[line] == null) continue;
                DrawLine(ld.courseDescription[line], lineOffset + line, false);
            }
            for (int line = 0; line < secretDescriptionLength; line++)
            {
                if (ld.secretDescription[line] == null) continue;
                DrawLine(ld.secretDescription[line], lineOffset + line, true);
            }
            
            RectangleF drawRect = new RectangleF(0, (lineOffset + lastLine) * 23 + 2, 340, 23);
            StringFormat drawFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            blackBrush.Dispose();
            drawBrush.Dispose();
            bigFont.Dispose();
        }

        public void DrawByte(byte stars, int lineNumber, bool isSecret, byte mask)
        {
            for (int i = 1; i <= 7; i++)
            {
                if ((mask & (1 << i)) == 0) continue;
                int x = (isSecret ? 180 : 0) + i * 20;
                int y = lineNumber * 23;
                bool isAcquired = (stars & (1 << (i - 1))) != 0;
                Image img = isAcquired ? ld.goldStar : ld.darkStar;
                graphics.DrawImage(img, x, y, 20, 20);
            }
        }

        public void DrawLine(LineDescription ld, int lineNumber, bool isSecret)
        {
            Font drawFont = new Font(fontFamily, drawFontSize);

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

        public void InvalidateCache()
        {
            IsFirstCall = true;
        }

        public void SetBackground(Image image)
        {
            double ratioX = (double)345 / image.Width;
            double ratioY = (double)462 / image.Height;
            double ratio = Math.Max(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            background = new Bitmap(345, 462);

            using (var graphics = Graphics.FromImage(background))
                graphics.DrawImage(image, (345 - newWidth) / 2, (462 - newHeight) / 2, newWidth, newHeight);
        }

        public void DrawIntro(bool isEmulatorLoaded, bool isHackLoaded, bool isOffsetsInitialized)
        {
            Font drawFont = new Font(fontFamily, bigFontSize);
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
    }
}
