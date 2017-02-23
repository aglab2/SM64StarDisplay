using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    public abstract class Action
    {
        public abstract void execute(GraphicsManager gm);
    }

    public class LineDrawAction : Action
    {
        public int Line;
        public byte StarByte;
        public int StarDiff;
        public bool IsSecret;
        public byte StarMask;

        public LineDrawAction(int line, byte starByte, int starDiff, bool isSecret, byte starMask)
        {
            this.Line = line;
            this.StarByte = starByte;
            this.StarDiff = starDiff;
            this.IsSecret = isSecret;
            this.StarMask = starMask;
        }

        public override void execute(GraphicsManager gm)
        {
            for (int i = 1; i <= 7; i++)
            {
                if ((StarMask & (1 << i)) == 0) continue;
                int x = (IsSecret ? 180 : 0) + i * 20;
                int y = Line * 23;
                bool isAcquired = (StarByte & (1 << (i - 1))) != 0;
                Image img = isAcquired ? gm.goldStar : gm.darkStar;
                gm.graphics.DrawImage(img, x, y, 20, 20);
            }
        }
    }

    public class StarHighlightAction : Action
    {
        public int Line;
        public byte HighlightByte;
        public bool IsSecret;
        public byte StarMask;

        public StarHighlightAction(int line, byte highlightByte, bool isSecret, byte starMask)
        {
            this.Line = line;
            this.HighlightByte = highlightByte;
            this.IsSecret = isSecret;
            this.StarMask = starMask;
        }

        public override void execute(GraphicsManager gm)
        {
            for (int i = 1; i <= 7; i++)
            {
                if ((StarMask & (1 << i)) == 0) continue;
                int x = (IsSecret ? 180 : 0) + i * 20;
                int y = Line * 23;
                bool isAcquired = (HighlightByte & (1 << (i - 1))) != 0;
                if (isAcquired)
                    gm.graphics.DrawImage(gm.redOutline, x, y, 20, 20);
            }
        }
    }

    public class RedsDrawAction : Action
    {
        public int CurrentRedsCount;
        public int TotalRedsCount;

        public RedsDrawAction(int currentRedsCount, int totalRedsCount)
        {
            this.CurrentRedsCount = currentRedsCount;
            this.TotalRedsCount = totalRedsCount;
        }

        public override void execute(GraphicsManager gm)
        {
            int totalStarLine = gm.ld.GetLength() + 2;

            if (TotalRedsCount > 16)
            {
                string starLine = CurrentRedsCount.ToString() + "/" + TotalRedsCount.ToString();

                SolidBrush redBrush = new SolidBrush(Color.IndianRed);
                SolidBrush drawBrush = new SolidBrush(Color.LightGray);

                PrivateFontCollection collection = new PrivateFontCollection();
                collection.AddFontFile("font/CourierNew.ttf");
                FontFamily fontFamily = new FontFamily("Courier New", collection);
                Font bigFont = new Font(fontFamily, 13);

                gm.graphics.DrawImage(gm.reds, 20, totalStarLine * 23 + 10, 20, 20);
                gm.graphics.DrawString(starLine, bigFont, redBrush, 40, totalStarLine * 23 + 10);

                redBrush.Dispose();
                drawBrush.Dispose();
                fontFamily.Dispose();
                collection.Dispose();
            }
            else
            {
                for (int i = 0; i < CurrentRedsCount; i++)
                {
                    gm.graphics.DrawImage(gm.reds, 20 + i * 20, totalStarLine * 23 + 10, 20, 20);
                }
                for (int i = CurrentRedsCount; i < TotalRedsCount; i++)
                {
                    gm.graphics.DrawImage(gm.darkReds, 20 + i * 20, totalStarLine * 23 + 10, 20, 20);
                }
            }
        }
    }

    public class LastStarHighlightAction : Action
    {
        public LastStarHighlightAction() { }
        public override void execute(GraphicsManager gm)
        {
            if (gm.IsFirstCall)
            {
                gm.IsFirstCall = false;
                gm.lastSHA = null;
                return;
            }
            if (gm.lastSHA == null) return;
            for (int i = 1; i <= 7; i++)
            {
                if ((gm.lastSHA.StarMask & (1 << i)) == 0) continue;
                int x = (gm.lastSHA.IsSecret ? 180 : 0) + i * 20;
                int y = gm.lastSHA.Line * 23;
                bool isAcquired = (gm.lastSHA.HighlightByte & (1 << (i - 1))) != 0;
                if (isAcquired)
                {
                    gm.graphics.DrawImage(gm.greenOutline, x, y, 20, 20);
                }
            }
        }
    }

    public class TextHighlightAction : Action
    {
        public int Line;
        public bool IsSecret;
        public string Text;
        public TextHighlightAction(int line, bool isSecret, string text)
        {
            Line = line;
            IsSecret = isSecret;
            Text = text;
        }

        public override void execute(GraphicsManager gm)
        {
            int x = IsSecret ? 180 : 0;
            int y = Line * 23;

            SolidBrush drawBrush = new SolidBrush(Color.LightGreen);

            PrivateFontCollection collection = new PrivateFontCollection();
            collection.AddFontFile("font/CourierNew.ttf");
            FontFamily fontFamily = new FontFamily("Courier New", collection);

            Font drawFont = new Font(fontFamily, 10);

            gm.graphics.DrawString(Text, drawFont, drawBrush, x, y + 2);

            drawBrush.Dispose();
            drawFont.Dispose();
            fontFamily.Dispose();
            collection.Dispose();
        }
    }

    public class LastHighlight : Action
    {
        public int Line;
        public byte HighlightByte;
        public bool IsSecret;
        public byte StarMask;

        public LastHighlight(int line, byte highlightByte, bool isSecret, byte starMask)
        {
            this.Line = line;
            this.HighlightByte = highlightByte;
            this.IsSecret = isSecret;
            this.StarMask = starMask;
        }

        public override void execute(GraphicsManager gm)
        {
            gm.lastSHA = this;
        }
    }


    public class DrawActions : IEnumerable<Action>
    {
        LayoutDescription ld;
        byte[] stars;
        byte[] oldStars;
        byte[] highlightPivot;
        int reds;
        int totalReds;

        public DrawActions(LayoutDescription ld, byte[] stars, byte[] oldStars, byte[] highlightPivot, int reds, int totalReds)
        {
            this.ld = ld;
            this.stars = stars;
            this.oldStars = oldStars;
            this.highlightPivot = highlightPivot;
            this.reds = reds;
            this.totalReds = totalReds;
        }

        public IEnumerator<Action> GetEnumerator()
        {
            int index; bool isAcquired;
            index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "B1");
            isAcquired = ((stars[3] & (1 << 4)) != 0) || ((stars[3] & (1 << 6)) != 0);
            if (index != -1 && isAcquired)
                yield return new TextHighlightAction(index, true, "B1");
            index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "B2");
            isAcquired = ((stars[3] & (1 << 5)) != 0) || ((stars[3] & (1 << 7)) != 0);
            if (index != -1 && isAcquired)
                yield return new TextHighlightAction(index, true, "B2");
            index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "WC");
            isAcquired = ((stars[3] & (1 << 1)) != 0);
            if (index != -1 && isAcquired)
                yield return new TextHighlightAction(index, true, "WC");
            index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "MC");
            isAcquired = ((stars[3] & (1 << 2)) != 0);
            if (index != -1 && isAcquired)
                yield return new TextHighlightAction(index, true, "MC");
            index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "VC");
            isAcquired = ((stars[3] & (1 << 3)) != 0);
            if (index != -1 && isAcquired)
                yield return new TextHighlightAction(index, true, "VC");

            for (int line = 0; line < ld.courseDescription.Length; line++)
            {
                var descr = ld.courseDescription[line];
                if (descr == null || descr.isTextOnly) continue;

                byte oldStarByte = oldStars[descr.offset];
                byte newStarByte = stars[descr.offset];
                byte highlightByte = highlightPivot[descr.offset];
                byte starMask2 = (byte)(descr.starMask >> 1);
                
                byte diffByteFromPivot = (byte)(((highlightByte) ^ (newStarByte)) & newStarByte);
                yield return new StarHighlightAction(line, diffByteFromPivot, false, descr.starMask);
                if (oldStarByte != newStarByte)
                {
                    byte diffbyteFromOld = (byte)(((oldStarByte) ^ (newStarByte)) & newStarByte);
                    yield return new LastHighlight(line, diffbyteFromOld, false, descr.starMask);
                }
            }

            for (int line = 0; line < ld.secretDescription.Length; line++)
            {
                var descr = ld.secretDescription[line];
                if (descr == null || descr.isTextOnly) continue;

                byte oldStarByte = oldStars[descr.offset];
                byte newStarByte = stars[descr.offset];
                byte highlightByte = highlightPivot[descr.offset];
                byte starMask2 = (byte)(descr.starMask >> 1);
                
                byte diffByte = (byte)(((highlightByte) ^ (newStarByte)) & newStarByte);
                yield return new StarHighlightAction(line, diffByte, true, descr.starMask);
                if (oldStarByte != newStarByte)
                {
                    byte diffbyteFromOld = (byte)(((oldStarByte) ^ (newStarByte)) & newStarByte);
                    yield return new LastHighlight(line, diffbyteFromOld, true, descr.starMask);
                }
            }

            yield return new LastStarHighlightAction();

            for (int line = 0; line < ld.courseDescription.Length; line++)
            {
                var descr = ld.courseDescription[line];
                if (descr == null || descr.isTextOnly) continue;

                byte oldStarByte = oldStars[descr.offset];
                byte newStarByte = stars[descr.offset];
                byte highlightByte = highlightPivot[descr.offset];
                byte starMask2 = (byte)(descr.starMask >> 1);

                byte diffByte = (byte)(((highlightByte) ^ (newStarByte)) & newStarByte);
                yield return new LineDrawAction(line, newStarByte, MemoryManager.countStars((byte)(newStarByte & starMask2)) - MemoryManager.countStars((byte)(oldStarByte & starMask2)), false, descr.starMask);
            }

            for (int line = 0; line < ld.secretDescription.Length; line++)
            {
                var descr = ld.secretDescription[line];
                if (descr == null || descr.isTextOnly) continue;

                byte oldStarByte = oldStars[descr.offset];
                byte newStarByte = stars[descr.offset];
                byte highlightByte = highlightPivot[descr.offset];
                byte starMask2 = (byte)(descr.starMask >> 1);

                byte diffByte = (byte)(((highlightByte) ^ (newStarByte)) & newStarByte);
                yield return new LineDrawAction(line, newStarByte, MemoryManager.countStars((byte)(newStarByte & starMask2)) - MemoryManager.countStars((byte)(oldStarByte & starMask2)), true, descr.starMask);
            }

            yield return new RedsDrawAction(reds, totalReds);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
