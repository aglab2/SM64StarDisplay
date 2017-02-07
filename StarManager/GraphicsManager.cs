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
        public Image darkStar;
        public Image goldStar;

        LayoutDescription ld;
        public readonly Graphics graphics;

        SolidBrush blackBrush;
        SolidBrush drawBrush;
        SolidBrush yellowBrush;

        Font drawFont;
        Font bigFont;

        public GraphicsManager(Graphics graphics, LayoutDescription ld)
        {
            darkStar = ld.darkStar;
            goldStar = ld.goldStar;
            this.ld = ld;
            this.graphics = graphics;
            blackBrush = new SolidBrush(Color.Black);
            drawBrush = new SolidBrush(Color.LightGray);
            yellowBrush = new SolidBrush(Color.Gold);

            PrivateFontCollection collection = new PrivateFontCollection();
            collection.AddFontFile("font/CourierNew.ttf");
            FontFamily fontFamily = new FontFamily("Courier New", collection);
            
            drawFont = new Font(fontFamily, 10);
            bigFont = new Font(fontFamily, 15);
        }

        public void paintHUD(Graphics graphics)
        {
            graphics.FillRectangle(blackBrush, new Rectangle(0,0,1000,1000));
            for (int line = 0; line < ld.courseDescription.Length; line++)
            {
                drawLine(graphics, ld.courseDescription[line], line, false);
            }
            for (int line = 0; line < ld.secretDescription.Length; line++)
            {
                drawLine(graphics, ld.secretDescription[line], line, true);
            }
            int lastLine = Math.Max(ld.courseDescription.Length, ld.secretDescription.Length);
            graphics.DrawString("Savestateless Stars", bigFont, drawBrush, 45, lastLine * 23 + 2);
        }

        public void drawByte(Graphics graphics, byte stars, int lineNumber, bool isSecret, byte mask)
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

        public void drawLine(Graphics graphics, LineDescription ld, int lineNumber, bool isSecret)
        {
            if (ld.isTextOnly)
            {
                graphics.DrawString(ld.text, drawFont, drawBrush, (isSecret ? 180: 0) + 7, lineNumber * 23 + 2);
            }
            else
            {
                Console.WriteLine(ld.starMask);
                graphics.DrawString(ld.text, drawFont, drawBrush, isSecret ? 180 : 0, lineNumber * 23 + 2);
                drawByte(graphics, 0, lineNumber, isSecret, ld.starMask);
            }
        }

        public void drawYellowString(Graphics graphics, LineEntry le, LineDescription lind)
        {
            int x = le.isSecret ? 180 : 0;
            int y = le.line * 23;

            if (lind.text != "")
            {
                graphics.DrawString(lind.text, drawFont, yellowBrush, x, y + 2);
            }
            else
            {
                graphics.FillRectangle(yellowBrush, x + 8, y + 8, 4, 4);
            }
        }

        public void drawBlackString(Graphics graphics, LineEntry le, LineDescription lind)
        {
            int x = le.isSecret ? 180 : 0;
            int y = le.line * 23;

            graphics.FillRectangle(blackBrush, x+1, y+1, 18, 18);
            graphics.DrawString(lind.text, drawFont, drawBrush, x, y + 2);
        }
    }
}
