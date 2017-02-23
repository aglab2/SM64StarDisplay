using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    public class GraphicsManager
    {
        public readonly Image darkStar;
        public readonly Image goldStar;
        public readonly Image redOutline;
        public readonly Image greenOutline;

        Bitmap goldSquare;
        Bitmap blackSquare;

        public readonly LayoutDescription ld;
        Graphics _graphics;
        public StarHighlightAction lastSHA = null;

        public Graphics graphics
        {
            internal get { return _graphics; }
            set { _graphics.Dispose(); _graphics = value; }
        }

        public GraphicsManager(Graphics graphics, LayoutDescription ld)
        {
            this.darkStar = ld.darkStar;
            this.goldStar = ld.goldStar;
            this.redOutline = ld.redOutline;
            this.greenOutline = ld.greenOutline;

            this.ld = ld;
            this._graphics = graphics;

            goldSquare = new Bitmap(4, 4);
            for (int i = 0; i < goldSquare.Width; i++)
                for (int j = 0; j < goldSquare.Height; j++)
                    goldSquare.SetPixel(i, j, Color.Gold);

            blackSquare = new Bitmap(4, 4);
            for (int i = 0; i < blackSquare.Width; i++)
                for (int j = 0; j < blackSquare.Height; j++)
                    blackSquare.SetPixel(i, j, Color.Black);
        }

        public void PaintHUD()
        {
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            SolidBrush drawBrush = new SolidBrush(Color.LightGray);

            PrivateFontCollection collection = new PrivateFontCollection();
            collection.AddFontFile("font/CourierNew.ttf");
            FontFamily fontFamily = new FontFamily("Courier New", collection);

            Font bigFont = new Font(fontFamily, 15);

            graphics.Clear(Color.Black);

            int courseDescriptionLength = Array.FindLastIndex(ld.courseDescription, item => item != null) + 1;
            int secretDescriptionLength = Array.FindLastIndex(ld.secretDescription, item => item != null) + 1;

            for (int line = 0; line < courseDescriptionLength; line++)
            {
                if (ld.courseDescription[line] == null) continue;
                DrawLine(ld.courseDescription[line], line, false);
            }
            for (int line = 0; line < secretDescriptionLength; line++)
            {
                if (ld.secretDescription[line] == null) continue;
                DrawLine(ld.secretDescription[line], line, true);
            }
            int lastLine = Math.Max(courseDescriptionLength, secretDescriptionLength);
            graphics.DrawString("Savestateless Stars", bigFont, drawBrush, 45, lastLine * 23 + 2);

            blackBrush.Dispose();
            drawBrush.Dispose();
            bigFont.Dispose();
            fontFamily.Dispose();
            collection.Dispose();
        }

        public void DrawByte(byte stars, int lineNumber, bool isSecret, byte mask)
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

        public void DrawLine(LineDescription ld, int lineNumber, bool isSecret)
        {
            PrivateFontCollection collection = new PrivateFontCollection();
            collection.AddFontFile("font/CourierNew.ttf");
            FontFamily fontFamily = new FontFamily("Courier New", collection);

            Font drawFont = new Font(fontFamily, 10);

            SolidBrush drawBrush = new SolidBrush(Color.LightGray);
            if (ld.isTextOnly)
            {
                graphics.DrawString(ld.text, drawFont, drawBrush, (isSecret ? 180: 0) + 7, lineNumber * 23 + 2);
            }
            else
            {
                graphics.DrawString(ld.text, drawFont, drawBrush, isSecret ? 180 : 0, lineNumber * 23 + 2);
                DrawByte(0, lineNumber, isSecret, ld.starMask);
            }
            drawBrush.Dispose();
            drawFont.Dispose();
            fontFamily.Dispose();
            collection.Dispose();
        }

        public void AddLineHighlight(TextHighlightAction act)
        {
            if (act.Text != "")
            {
                int x = (act.IsSecret ? 180 : 0) + 1;
                int y = act.Line * 23 + 2;

                SolidBrush yellowBrush = new SolidBrush(Color.DarkGoldenrod);
                Pen yellowPen = new Pen(yellowBrush);
                graphics.DrawRectangle(yellowPen, new Rectangle(x, y, 17, 17));
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
        
        public void DrawGreenString(TextHighlightAction le, LineDescription lind)
        {
            int x = le.IsSecret ? 180 : 0;
            int y = le.Line * 23;

            graphics.DrawImage(blackSquare, x + 1, y + 1);
            SolidBrush drawBrush = new SolidBrush(Color.LightGreen);

            PrivateFontCollection collection = new PrivateFontCollection();
            collection.AddFontFile("font/CourierNew.ttf");
            FontFamily fontFamily = new FontFamily("Courier New", collection);

            Font drawFont = new Font(fontFamily, 10);

            graphics.DrawString(lind.text, drawFont, drawBrush, x, y + 2);

            drawBrush.Dispose();
            drawFont.Dispose();
            fontFamily.Dispose();
            collection.Dispose();
        }

        public void DrawStarNumber(string totalCount, int starCount)
        {
            string starLine = starCount.ToString().PadLeft(3) + "/" + totalCount.PadRight(3);

            int courseDescriptionLength = Array.FindLastIndex(ld.courseDescription, item => item != null) + 1;
            int secretDescriptionLength = Array.FindLastIndex(ld.secretDescription, item => item != null) + 1;
            int totalStarLine = Math.Max(courseDescriptionLength, secretDescriptionLength) + 1;

            SolidBrush blackBrush = new SolidBrush(Color.Black);
            SolidBrush drawBrush = new SolidBrush(Color.LightGray);

            PrivateFontCollection collection = new PrivateFontCollection();
            collection.AddFontFile("font/CourierNew.ttf");
            FontFamily fontFamily = new FontFamily("Courier New", collection);
            Font bigFont = new Font(fontFamily, 15);

            graphics.DrawString(starLine, bigFont, drawBrush, 120, totalStarLine * 23 + 2);

            blackBrush.Dispose();
            drawBrush.Dispose();
            fontFamily.Dispose();
            collection.Dispose();
        }

        public void DrawReds(int reds)
        {
            string starLine = reds.ToString().PadLeft(3);

            int courseDescriptionLength = Array.FindLastIndex(ld.courseDescription, item => item != null) + 1;
            int secretDescriptionLength = Array.FindLastIndex(ld.secretDescription, item => item != null) + 1;
            int totalStarLine = Math.Max(courseDescriptionLength, secretDescriptionLength) + 2;

            SolidBrush redBrush = new SolidBrush(Color.DarkRed);
            SolidBrush drawBrush = new SolidBrush(Color.LightGray);

            PrivateFontCollection collection = new PrivateFontCollection();
            collection.AddFontFile("font/CourierNew.ttf");
            FontFamily fontFamily = new FontFamily("Courier New", collection);
            Font bigFont = new Font(fontFamily, 15);

            graphics.DrawString(starLine, bigFont, redBrush, 0, totalStarLine * 23);

            redBrush.Dispose();
            drawBrush.Dispose();
            fontFamily.Dispose();
            collection.Dispose();
        }
    }
}
