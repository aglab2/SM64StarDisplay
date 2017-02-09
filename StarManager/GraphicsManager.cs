using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    public class GraphicsManager
    {
        Image darkStar;
        Image goldStar;

        Bitmap goldSquare;
        Bitmap blackSquare;

        LayoutDescription ld;

        Graphics _graphics;
        public Graphics graphics {
            internal get { return _graphics; }
            set { _graphics.Dispose(); _graphics = value; }
        }

        SolidBrush blackBrush;
        SolidBrush drawBrush;
        SolidBrush yellowBrush;
        SolidBrush greenBrush;

        Font drawFont;
        Font bigFont;

        public GraphicsManager(Graphics graphics, LayoutDescription ld)
        {
            darkStar = ld.darkStar;
            goldStar = ld.goldStar;
            this.ld = ld;
            _graphics = graphics;
            blackBrush = new SolidBrush(Color.Black);
            drawBrush = new SolidBrush(Color.LightGray);
            yellowBrush = new SolidBrush(Color.Gold);

            greenBrush = new SolidBrush(Color.FromArgb(0, 177, 64));

            PrivateFontCollection collection = new PrivateFontCollection();
            collection.AddFontFile("font/CourierNew.ttf");
            FontFamily fontFamily = new FontFamily("Courier New", collection);
            
            drawFont = new Font(fontFamily, 10);
            bigFont = new Font(fontFamily, 15);

            goldSquare = new Bitmap(4, 4);
            for (int i = 0; i < goldSquare.Width; i++)
                for (int j = 0; j < goldSquare.Height; j++)
                    goldSquare.SetPixel(i, j, Color.Gold);

            blackSquare = new Bitmap(18, 18);
            for (int i = 0; i < blackSquare.Width; i++)
                for (int j = 0; j < blackSquare.Height; j++)
                    blackSquare.SetPixel(i, j, Color.Black);
        }

        public void paintHUD(int width, int height)
        {
            graphics.FillRectangle(blackBrush, new Rectangle(0, 0, width, height));
            for (int line = 0; line < ld.courseDescription.Length; line++)
            {
                drawLine(ld.courseDescription[line], line, false);
            }
            for (int line = 0; line < ld.secretDescription.Length; line++)
            {
                drawLine(ld.secretDescription[line], line, true);
            }
            int lastLine = Math.Max(ld.courseDescription.Length, ld.secretDescription.Length);
            graphics.DrawString("Savestateless Stars", bigFont, drawBrush, 45, lastLine * 23 + 2);
        }

        public void drawByte(byte stars, int lineNumber, bool isSecret, byte mask)
        {
            for (int i = 1; i <= 7; i++)
            {
                if ((mask & (1 << i)) == 0) continue;
                int x = (isSecret ? 180 : 0) + i * 20;
                int y = lineNumber * 23;
                bool isAcquired = (stars & (1 << (i - 1))) != 0;
                Image img = isAcquired ? goldStar : darkStar;
                graphics.DrawImage(img, x, y, 20, 20);
            }
        }

        public void drawLine(LineDescription ld, int lineNumber, bool isSecret)
        {
            if (ld.isTextOnly)
            {
                graphics.DrawString(ld.text, drawFont, drawBrush, (isSecret ? 180: 0) + 7, lineNumber * 23 + 2);
            }
            else
            {
                Console.WriteLine(ld.starMask);
                graphics.DrawString(ld.text, drawFont, drawBrush, isSecret ? 180 : 0, lineNumber * 23 + 2);
                drawByte(0, lineNumber, isSecret, ld.starMask);
            }
        }

        public void drawYellowString(LineEntry le, LineDescription lind)
        {
            int x = le.isSecret ? 180 : 0;
            int y = le.line * 23;

            if (lind.text != "")
            {
                SolidBrush yellowBrush = new SolidBrush(Color.Gold);

                PrivateFontCollection collection = new PrivateFontCollection();
                collection.AddFontFile("font/CourierNew.ttf");
                FontFamily fontFamily = new FontFamily("Courier New", collection);

                Font drawFont = new Font(fontFamily, 10);

                graphics.DrawString(lind.text, drawFont, yellowBrush, x, y + 2);
            }
            else
            {
                graphics.DrawImage(goldSquare, x + 8, y + 8);
            }

        }

        public void drawBlackString(LineEntry le, LineDescription lind)
        {
            int x = le.isSecret ? 180 : 0;
            int y = le.line * 23;

            graphics.DrawImage(blackSquare, x + 1, y + 1);
            SolidBrush drawBrush = new SolidBrush(Color.LightGray);

            PrivateFontCollection collection = new PrivateFontCollection();
            collection.AddFontFile("font/CourierNew.ttf");
            FontFamily fontFamily = new FontFamily("Courier New", collection);

            Font drawFont = new Font(fontFamily, 10);

            graphics.DrawString(lind.text, drawFont, drawBrush, x, y + 2);
        }

        public void drawStarNumber(string totalCount, int starCount)
        {
            string starLine = starCount.ToString().PadLeft(3) + "/" + totalCount.PadRight(3);
            
            int totalStarLine = Math.Max(ld.courseDescription.Length, ld.secretDescription.Length) + 1;

            SolidBrush blackBrush = new SolidBrush(Color.Black);
            SolidBrush drawBrush = new SolidBrush(Color.LightGray);

            PrivateFontCollection collection = new PrivateFontCollection();
            collection.AddFontFile("font/CourierNew.ttf");
            FontFamily fontFamily = new FontFamily("Courier New", collection);
            Font bigFont = new Font(fontFamily, 15);

            graphics.FillRectangle(blackBrush, new Rectangle(15, totalStarLine * 23 + 2, 200, 20));
            graphics.DrawString(starLine, bigFont, drawBrush, 120, totalStarLine * 23 + 2);
        }
    }
}
