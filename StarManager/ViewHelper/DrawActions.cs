using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarDisplay
{
    public abstract class Action
    {
        public abstract int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm); //returns taken size

        // This method is needed to perform draw of config, should be defined in base class on need
        // C# does nto allow abstract static methods :[
        //public abstract static int DrawConfigs();

        public static IEnumerable<Type> GetAllSubclasses()
        {
            var baseType = typeof(Action);
            var assembly = baseType.Assembly;

            return assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));
        }
    }

    public class LineDrawAction : Action
    {
        public int Line;
        public byte StarByte;
        public int StarDiff;
        public bool IsSecret;
        public byte StarMask;
        public Image FilledStar;

        public LineDrawAction(int line, byte starByte, int starDiff, bool isSecret, byte starMask, Image filledStar)
        {
            this.Line = line;
            this.StarByte = starByte;
            this.StarDiff = starDiff;
            this.IsSecret = isSecret;
            this.StarMask = starMask;
            this.FilledStar = filledStar;
        }
        
        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            for (int i = 0; i < gm.Ld.starsShown; i++)
            {
                if ((StarMask & (1 << i)) == 0) continue;
                float x = (IsSecret ? gm.Width / 2 : 0) + (i + 1) * gm.SWidth;
                float y = (lineOffset + Line) * gm.SHeight + gm.SOffset;
                bool isAcquired = (StarByte & (1 << i)) != 0;
                if (isAcquired)
                    gm.graphics.DrawImage(FilledStar, x, y, gm.SWidth, gm.SWidth);
            }
            return 0; //Line Actions should not increase global offset
        }
    }
   
    public class RedsSecretsDrawAction : Action
    {
        public int CurrentRedsCount;
        public int TotalRedsCount;
        public int CurrentSecretsCount;
        public int TotalSecretsCount;
        public int ActivePanelsCount;
        public int TotalPanelsCount;

        static string configureReadableName = "Show collectables";
        static string configureName = "RedsSecretsDrawAction_isShown";

        static string collectablesMinimizedReadableName = "Draw collectables minimized";
        static string collectablesMinimizedConfigureName = "RedsSecretsDrawAction_areCollectablesMinimized";


        public static int DrawConfigs(int height, ActionMaskForm amf)
        {
            CheckBox cb = new CheckBox
            {
                Name = configureName,
                Text = configureReadableName,
                Location = new Point(10, height),
                Checked = amf.sm.GetConfig(configureName, true),
                AutoSize = true
            };

            cb.CheckedChanged += (sender, e) => {
                CheckBox cb_local = sender as CheckBox;
                amf.sm.SetConfig(cb_local.Name, cb_local.Checked);
            };
            amf.Controls.Add(cb);

            height += cb.Height + 5;

            cb = new CheckBox
            {
                Name = collectablesMinimizedConfigureName,
                Text = collectablesMinimizedReadableName,
                Location = new Point(10, height),
                Checked = amf.sm.GetConfig(collectablesMinimizedConfigureName, false),
                AutoSize = true
            };

            cb.CheckedChanged += (sender, e) => {
                CheckBox cb_local = sender as CheckBox;
                amf.sm.SetConfig(cb_local.Name, cb_local.Checked);
            };
            amf.Controls.Add(cb);

            return cb.Height * 2 + 5;
        }

        public RedsSecretsDrawAction(int currentRedsCount, int totalRedsCount, int currentSecretsCount, int totalSecretsCount, int activePanelsCount, int totalPanelsCount)
        {
            this.CurrentRedsCount = currentRedsCount < 0 ? 0 : currentRedsCount;
            this.TotalRedsCount = totalRedsCount < 0 ? 0 : totalRedsCount;
            this.CurrentSecretsCount = currentSecretsCount < 0 ? 0 : currentSecretsCount;
            this.TotalSecretsCount = totalSecretsCount;
            this.ActivePanelsCount = activePanelsCount;
            this.TotalPanelsCount = totalPanelsCount;
        }

        static int totalSize = 30;

        int GetFullSize(int elementsCount)
        {
            return elementsCount * 2;
        }

        int GetTextSize(int elementsCount)
        {
            if (elementsCount <= 2) return 100;
            return elementsCount.ToString().Length * 2 + 3; //3 = space for icon + space for /
        }

        int GetSpaceSize()
        {
            if (TotalPanelsCount == 0 && TotalSecretsCount == 0 && TotalPanelsCount == 0) return 0;
            return (TotalRedsCount != 0 ? 2 : 0) + (TotalSecretsCount != 0 ? 2 : 0) - 2;
        }

        int DrawFullReds(GraphicsManager gm, int totalStarLine)
        {
            //int totalStarLine = gm.ld.GetLength() + 2;
            for (int i = 0; i < CurrentRedsCount; i++)
            {
                gm.graphics.DrawImage(gm.reds, gm.SWidth + i * gm.SWidth, totalStarLine * gm.SHeight + gm.SHeight / 2, gm.SWidth, gm.SWidth);
            }
            for (int i = CurrentRedsCount; i < TotalRedsCount; i++)
            {
                gm.graphics.DrawImage(gm.darkReds, gm.SWidth + i * gm.SWidth, totalStarLine * gm.SHeight + gm.SHeight / 2, gm.SWidth, gm.SWidth);
            }
            return TotalRedsCount * 2 + (TotalRedsCount == 0 ? 0 : 2);
        }

        int DrawFullSecrets(GraphicsManager gm, int totalStarLine)
        {
            //int totalStarLine = gm.ld.GetLength() + 2;
            for (int i = 0; i < CurrentSecretsCount; i++)
            {
                gm.graphics.DrawImage(gm.secrets, (gm.SWidth / 2) * totalSize - i * gm.SWidth, totalStarLine * gm.SHeight + gm.SHeight / 2, gm.SWidth, gm.SWidth);
            }
            for (int i = CurrentSecretsCount; i < TotalSecretsCount; i++)
            {
                gm.graphics.DrawImage(gm.darkSecrets, (gm.SWidth / 2) * totalSize - i * gm.SWidth, totalStarLine * gm.SHeight + gm.SHeight / 2, gm.SWidth, gm.SWidth);
            }
            return TotalSecretsCount * 2 + (TotalSecretsCount == 0 ? 0 : 2);
        }

        int DrawFullFlipswitches(GraphicsManager gm, int offset, int totalStarLine)
        {
            //int totalStarLine = gm.ld.GetLength() + 2;
            for (int i = 0; i < ActivePanelsCount; i++)
            {
                gm.graphics.DrawImage(gm.flipswitchOn, i * gm.SWidth + (gm.SWidth / 2) * offset + 1, totalStarLine * gm.SHeight + gm.SHeight / 2 + 1, gm.SWidth - 2, gm.SWidth - 2);
            }
            for (int i = ActivePanelsCount; i < TotalPanelsCount; i++)
            {
                gm.graphics.DrawImage(gm.flipswitchOff, i * gm.SWidth + (gm.SWidth / 2) * offset + 1, totalStarLine * gm.SHeight + gm.SHeight / 2 + 1, gm.SWidth - 2, gm.SWidth - 2);
            }
            return TotalPanelsCount * 2 + (TotalPanelsCount == 0 ? 0 : 2);
        }

        int DrawTextReds(GraphicsManager gm, int totalStarLine)
        {
            //int totalStarLine = gm.ld.GetLength() + 2;
            string starLine = CurrentRedsCount.ToString() + "/" + TotalRedsCount.ToString();

            SolidBrush redBrush = new SolidBrush(Color.IndianRed);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            gm.graphics.DrawImage(gm.reds, gm.SWidth, totalStarLine * gm.SHeight + gm.SHeight / 2, gm.SWidth, gm.SWidth);
            gm.graphics.DrawStringWithOutline(starLine, gm.FontFamily, gm.MedFontSize, redBrush, Pens.Black, gm.SWidth * 2, totalStarLine * gm.SHeight + gm.SHeight / 2);

            redBrush.Dispose();
            drawBrush.Dispose();
            
            return starLine.Length + 4;
        }

        int DrawTextSecrets(GraphicsManager gm, int totalStarLine)
        {
            //int totalStarLine = gm.ld.GetLength() + 2;
            string starLine = CurrentSecretsCount.ToString() + "/" + TotalSecretsCount.ToString();

            SolidBrush blueBrush = new SolidBrush(Color.LightBlue);
            SolidBrush drawBrush = new SolidBrush(Color.White);
            
            gm.graphics.DrawStringWithOutline(starLine, gm.FontFamily, gm.MedFontSize, blueBrush, Pens.Black, gm.SWidth + (gm.SWidth / 2) * totalSize - starLine.Length * (gm.SWidth / 2), totalStarLine * gm.SHeight + (gm.SHeight / 2));
            gm.graphics.DrawImage(gm.secrets, (gm.SWidth / 2) * totalSize - starLine.Length * (gm.SWidth / 2), totalStarLine * gm.SHeight + (gm.SHeight / 2), gm.SWidth, gm.SWidth);
            
            blueBrush.Dispose();
            drawBrush.Dispose();

            return starLine.Length + 4;
        }

        int DrawTextFlipswitches(GraphicsManager gm, int offset, int totalStarLine)
        {
            //int totalStarLine = gm.ld.GetLength() + 2;
            string starLine = ActivePanelsCount.ToString() + "/" + TotalPanelsCount.ToString();

            SolidBrush redBrush = new SolidBrush(Color.LightGreen);
            SolidBrush drawBrush = new SolidBrush(Color.White);
            
            gm.graphics.DrawImage(gm.flipswitchOff, offset * (gm.SWidth / 2), totalStarLine * gm.SHeight + (gm.SHeight / 2), gm.SWidth, gm.SWidth);
            gm.graphics.DrawStringWithOutline(starLine, gm.FontFamily, gm.MedFontSize, redBrush, Pens.Black, gm.SWidth + offset * (gm.SWidth / 2), totalStarLine * gm.SHeight + (gm.SHeight / 2));

            redBrush.Dispose();
            drawBrush.Dispose();
            
            return starLine.Length + 4;
        }

        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            bool isShown = sm.GetConfig(configureName, true);
            if (!isShown)
                return 0;

            if (!sm.GetConfig(collectablesMinimizedConfigureName, false))
            {
                int[] textSizes = new int[8];
                for (int i = 0; i < 8; i++)
                {
                    int count = 0;
                    count += (i & 1) == 0 ? GetTextSize(TotalSecretsCount) : GetFullSize(TotalSecretsCount);
                    count += (i & 2) == 0 ? GetTextSize(TotalRedsCount) : GetFullSize(TotalRedsCount);
                    count += (i & 4) == 0 ? GetTextSize(TotalPanelsCount) : GetFullSize(TotalPanelsCount);
                    count += GetSpaceSize();
                    if (count > totalSize) count = -1;
                    textSizes[i] = count;
                }
                int maxSize = textSizes.Max();
                int index = Array.FindIndex(textSizes, a => a == maxSize);

                int offset = 2;
                if ((index & 2) == 0)
                    offset += DrawTextReds(gm, lineOffset);
                else
                    offset += DrawFullReds(gm, lineOffset);

                if (maxSize == totalSize) offset -= 1;
                if ((index & 4) == 0)
                    DrawTextFlipswitches(gm, offset, lineOffset);
                else
                    DrawFullFlipswitches(gm, offset, lineOffset);

                if ((index & 1) == 0)
                    DrawTextSecrets(gm, lineOffset);
                else
                    DrawFullSecrets(gm, lineOffset);
            }
            else
            {
                if (TotalRedsCount != 0)
                    DrawTextReds(gm, lineOffset);
                if (TotalSecretsCount != 0)
                    DrawTextSecrets(gm, lineOffset);
            }
            return 2;
        }
    }

    public class LastStarHighlightAction : Action
    {
        static string configureReadableName = "Highlight last collected star";
        static string configureName = "LastStarHighlightAction_isShown";

        public static int DrawConfigs(int height, ActionMaskForm amf)
        {
            CheckBox cb = new CheckBox
            {
                Name = configureName,
                Text = configureReadableName,
                Location = new Point(10, height),
                Checked = amf.sm.GetConfig(configureName, true),
                AutoSize = true
            };

            cb.CheckedChanged += (sender, e) => {
                CheckBox cb_local = sender as CheckBox;
                amf.sm.SetConfig(cb_local.Name, cb_local.Checked);
            };

            amf.Controls.Add(cb);

            return cb.Height;
        }

        public LastStarHighlightAction() { }
        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            bool isShown = sm.GetConfig(configureName, true);
            if (!isShown)
                return 0;

            if (gm.IsFirstCall)
            {
                gm.IsFirstCall = false;
                gm.LastSHA = null;
                return 0;
            }
            if (gm.LastSHA == null) return 0;
            for (int i = 0; i < gm.Ld.starsShown; i++)
            {
                if ((gm.LastSHA.StarMask & (1 << i)) == 0) continue;
                float x = (gm.LastSHA.IsSecret ? (gm.Width/2) : 0) + (i + 1) * gm.SWidth;
                float y = (lineOffset + gm.LastSHA.Line) * gm.SHeight;
                bool isAcquired = (gm.LastSHA.HighlightByte & (1 << i)) != 0;
                if (isAcquired)
                {
                    gm.graphics.DrawImage(gm.Ld.redOutline, x, y, gm.SWidth, gm.SWidth);
                }
            }
            return 0;
        }
    }

    public class TextHighlightAction : Action
    {
        static string configureReadableName = "Highlight text for collected items";
        static string configureName = "TextHighlightAction_isShown";

        public static int DrawConfigs(int height, ActionMaskForm amf)
        {
            CheckBox cb = new CheckBox
            {
                Name = configureName,
                Text = configureReadableName,
                Location = new Point(10, height),
                Checked = amf.sm.GetConfig(configureName, true),
                AutoSize = true
            };

            cb.CheckedChanged += (sender, e) => {
                CheckBox cb_local = sender as CheckBox;
                amf.sm.SetConfig(cb_local.Name, cb_local.Checked);
            };
            amf.Controls.Add(cb);

            return cb.Height;
        }

        public int Line;
        public bool IsSecret;
        public string Text;
        public TextHighlightAction(int line, bool isSecret, string text)
        {
            Line = line;
            IsSecret = isSecret;
            Text = text;
        }

        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            bool isShown = sm.GetConfig(configureName, true);
            if (!isShown)
                return 0;

            SolidBrush drawBrush = new SolidBrush(Color.LightGreen);

            RectangleF drawRect = new RectangleF((IsSecret ? (gm.Width / 2) : 0), Line * gm.SHeight, gm.HalfWidth, gm.SHeight);
            StringFormat drawFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };
            gm.graphics.DrawStringWithOutline(Text, gm.FontFamily, gm.DrawFontSize, drawBrush, Pens.Black, drawRect, drawFormat);

            drawBrush.Dispose();

            return 0;
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

        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            gm.LastSHA = this;
            return 0;
        }
    }

    public class DrawCollectablesAction : Action
    {
        public int CurrentRedsCount;
        public int TotalRedsCount;
        public int Line;
        public bool IsText;

        public DrawCollectablesAction(int currentRedsCount, int totalRedsCount, int line, bool isText)
        {
            this.CurrentRedsCount = currentRedsCount < 0 ? 0 : currentRedsCount;
            this.TotalRedsCount = totalRedsCount;
            this.Line = line;
            this.IsText = isText;
        }
        

        void DrawFullReds(GraphicsManager gm, int lineOffset)
        {
            for (int i = 0; i < CurrentRedsCount; i++)
            {
                gm.graphics.DrawImage(gm.reds, gm.SWidth + i * gm.SWidth, (lineOffset + Line) * gm.SHeight + gm.SWidth / 2, gm.SWidth, gm.SWidth);
            }
            for (int i = CurrentRedsCount; i < TotalRedsCount; i++)
            {
                gm.graphics.DrawImage(gm.darkReds, gm.SWidth + i * gm.SWidth, (lineOffset + Line) * gm.SHeight + gm.SWidth / 2, gm.SWidth, gm.SWidth);
            }
        }

        void DrawTextReds(GraphicsManager gm, int lineOffset)
        {
            string starLine = CurrentRedsCount.ToString() + "/" + TotalRedsCount.ToString();

            SolidBrush redBrush = new SolidBrush(Color.IndianRed);
            SolidBrush drawBrush = new SolidBrush(Color.White);
            
            gm.graphics.DrawImage(gm.reds, gm.SWidth, (lineOffset + Line) * gm.SHeight + gm.SWidth / 2, gm.SWidth, gm.SWidth);
            gm.graphics.DrawStringWithOutline(starLine, gm.FontFamily, gm.MedFontSize, redBrush, Pens.Black, gm.SWidth * 2, (lineOffset + Line) * gm.SHeight + gm.SWidth / 2);

            redBrush.Dispose();
            drawBrush.Dispose();
        }
        
        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            if (IsText)
                DrawTextReds(gm, lineOffset);
            else
                DrawFullReds(gm, lineOffset);
            return 0;
        }
    }

    public class SecretsDrawAction : Action
    {
        public int CurrentSecretsCount;
        public int TotalSecretsCount;
        public int Line;
        public bool IsText;

        public SecretsDrawAction(int currentRedsCount, int totalRedsCount, int line, bool isText)
        {
            this.CurrentSecretsCount = currentRedsCount < 0 ? 0 : currentRedsCount;
            this.TotalSecretsCount = totalRedsCount;
            this.Line = line;
            this.IsText = isText;
        }


        void DrawFullSecrets(GraphicsManager gm, int lineOffset)
        {
            for (int i = 0; i < CurrentSecretsCount; i++)
            {
                gm.graphics.DrawImage(gm.secrets, gm.Width / 2 + gm.SWidth + i * gm.SWidth, (lineOffset + Line) * gm.SHeight + gm.SWidth / 2, gm.SWidth, gm.SWidth);
            }
            for (int i = CurrentSecretsCount; i < TotalSecretsCount; i++)
            {
                gm.graphics.DrawImage(gm.darkSecrets, gm.Width / 2 + gm.SWidth + i * gm.SWidth, (lineOffset + Line) * gm.SHeight + gm.SWidth / 2, gm.SWidth, gm.SWidth);
            }
        }

        void DrawTextSecrets(GraphicsManager gm, int lineOffset)
        {
            string starLine = CurrentSecretsCount.ToString() + "/" + TotalSecretsCount.ToString();

            SolidBrush blueBrush = new SolidBrush(Color.LightBlue);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            gm.graphics.DrawImage(gm.secrets, gm.Width / 2 + gm.SWidth, (lineOffset + Line) * gm.SHeight + gm.SWidth / 2, gm.SWidth, gm.SWidth);
            gm.graphics.DrawStringWithOutline(starLine, gm.FontFamily, gm.MedFontSize, blueBrush, Pens.Black, gm.Width / 2 + gm.SWidth * 2, (lineOffset + Line) * gm.SHeight + gm.SWidth / 2);

            blueBrush.Dispose();
            drawBrush.Dispose();
        }

        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            if (IsText)
                DrawTextSecrets(gm, lineOffset);
            else
                DrawFullSecrets(gm, lineOffset);
            return 0;
        }
    }

    public class StarLayoutFiniAction : Action
    {
        public int Length;

        public StarLayoutFiniAction(int length)
        {
            this.Length = length;
        }

        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            return Length;
        }
    }

    public class StringLineDrawAction : Action
    {
        public string Line;

        static string configureReadableName = "Show collected stars line";
        static string configureName = "StringLineDrawAction_isShown";

        public static int DrawConfigs(int height, ActionMaskForm amf)
        {
             CheckBox cb = new CheckBox
            {
                Name = configureName,
                Text = configureReadableName,
                Location = new Point(10, height),
                Checked = amf.sm.GetConfig(configureName, true),
                AutoSize = true
            };

            cb.CheckedChanged += (sender, e) => {
                CheckBox cb_local = sender as CheckBox;
                amf.sm.SetConfig(cb_local.Name, cb_local.Checked);
            };
            amf.Controls.Add(cb);

            return cb.Height;
        }

        public StringLineDrawAction(string line)
        {
            this.Line = line;
        }

        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            bool isShown = sm.GetConfig(configureName, true);
            if (!isShown)
                return 0;

            SolidBrush blackBrush = new SolidBrush(Color.Black);
            SolidBrush drawBrush = new SolidBrush(Color.White);
            
            RectangleF drawRect = new RectangleF(0, lineOffset * gm.SHeight, gm.Width, gm.SHeight);
            StringFormat drawFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.MeasureTrailingSpaces
            };
            gm.graphics.DrawStringWithOutline(Line, gm.FontFamily, gm.BigFontSize, drawBrush, Pens.Black, drawRect, drawFormat);

            blackBrush.Dispose();
            drawBrush.Dispose();
            return 1;
        }
    }

    public class StarTextLineDrawAction : Action
    {
        static string configureReadableName = "Show text line after display";
        static string configureName = "StarTextLineDrawAction_isShown";
        
        static string textlineConfigureName = "StarTextLineDrawAction_textLine";

        public static int DrawConfigs(int height, ActionMaskForm amf)
        {
            CheckBox cb = new CheckBox
            {
                Name = configureName,
                Text = configureReadableName,
                Location = new Point(10, height),
                Checked = amf.sm.GetConfig(configureName, true),
                AutoSize = true
            };
            height += cb.Height;
            
            TextBox tb = new TextBox
            {
                Name = textlineConfigureName,
                Text = amf.sm.GetConfig(textlineConfigureName, "Stars Collected"),
                Location = new Point(30, height),
                AutoSize = true,
                Width = 150
            };

            cb.CheckedChanged += (sender, e) => {
                CheckBox cb_local = sender as CheckBox;
                amf.sm.SetConfig(cb_local.Name, cb_local.Checked);
            };
            tb.TextChanged += (sender, e) => {
                TextBox tb_local = sender as TextBox;
                amf.sm.SetConfig(tb_local.Name, tb_local.Text);
            };

            amf.Controls.Add(cb);
            amf.Controls.Add(tb);

            return cb.Height + tb.Height + 10;
        }

        public StarTextLineDrawAction() { }

        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            String textLine = sm.GetConfig(textlineConfigureName, "Stars Collected");
            bool isShown = sm.GetConfig(configureName, true);

            if (!isShown)
                return 0;
            

            SolidBrush blackBrush = new SolidBrush(Color.Black);
            SolidBrush drawBrush = new SolidBrush(Color.White);
            
            RectangleF drawRect = new RectangleF(0, lineOffset * gm.SHeight, gm.Width, gm.SHeight);
            StringFormat drawFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.MeasureTrailingSpaces
            };
            gm.graphics.DrawStringWithOutline(textLine, gm.FontFamily, gm.BigFontSize, drawBrush, Pens.Black, drawRect, drawFormat);

            blackBrush.Dispose();
            drawBrush.Dispose();
            return 1;
        }
    }

    public class StarLayoutInitAction : Action
    {
        public StarLayoutInitAction()
        {
        }

        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            gm.PaintHUD(lineOffset);
            return 0;
        }
    }

    public class MainWindowsSettingsAction : Action
    {
        static string collectablesOnlyConfigureReadableName = "Show only collectables";
        public static string collectablesOnlyConfigureName = "MainWindow_areCollectablesOnly";

        public static int DrawConfigs(int height, ActionMaskForm amf)
        {
            CheckBox cb = new CheckBox
            {
                Name = collectablesOnlyConfigureName,
                Text = collectablesOnlyConfigureReadableName,
                Location = new Point(10, height),
                Checked = amf.sm.GetConfig(collectablesOnlyConfigureName, true),
                AutoSize = true
            };

            cb.CheckedChanged += (sender, e) =>
            {
                CheckBox cb_local = sender as CheckBox;
                amf.sm.SetConfig(cb_local.Name, cb_local.Checked);
            };
            amf.Controls.Add(cb);

            return cb.Height;
        }

        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            throw new NotSupportedException();
        }
    }

    public class BackgroundSettingsAction : Action
    {
        static string configureReadableName = "Use custom background";
        public static string configureName = "Background_isEnabled";
        public static string pathConfigureName = "background_path";

        public static int DrawConfigs(int height, ActionMaskForm amf)
        {
            CheckBox cb = new CheckBox
            {
                Name = configureName,
                Text = configureReadableName,
                Location = new Point(10, height),
                Checked = amf.sm.GetConfig(configureName, false),
                AutoSize = true
            };
            height += cb.Height;

            TextBox tb = new TextBox
            {
                Name = pathConfigureName,
                Text = amf.sm.GetConfig(pathConfigureName, ""),
                Location = new Point(30, height),
                AutoSize = true,
                Width = 120
            };

            Button b = new Button
            {
                Name = "backgroundPathButton",
                Text = ".",
                Location = new Point(160, height - 1),
                AutoSize = true,
                Width = 20,
                Height = tb.Height
            };

            cb.CheckedChanged += (sender, e) => {
                CheckBox cb_local = sender as CheckBox;
                amf.sm.SetConfig(cb_local.Name, cb_local.Checked);
            };
            tb.TextChanged += (sender, e) => {
                TextBox tb_local = sender as TextBox;
                amf.sm.SetConfig(tb_local.Name, tb_local.Text);
            };
            b.Click += (sender, e) =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "PNG Images (*.png)|*.png",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    tb.Text = openFileDialog.FileName;
                }
            };

            amf.Controls.Add(cb);
            amf.Controls.Add(tb);
            amf.Controls.Add(b);

            return cb.Height + tb.Height + 10;
        }

        public override int Execute(GraphicsManager gm, int lineOffset, SettingsManager sm)
        {
            throw new NotSupportedException();
        }
    }

    public class DrawActions : IEnumerable<Action>
    {
        LayoutDescriptionEx ld;
        byte[] stars;
        byte[] oldStars;
        byte[] otherStars;
        int reds;
        int totalReds;
        int secrets;
        int totalSecrets;
        int activePanels;
        int totalPanels;

        public DrawActions() { }

        public DrawActions(LayoutDescriptionEx ld, byte[] stars, byte[] oldStars, byte[] otherStars, int reds, int totalReds, int secrets, int totalSecrets, int activePanels, int totalPanels)
        {
            this.ld = ld;
            this.stars = stars;
            this.oldStars = oldStars;
            this.otherStars = otherStars;
            this.reds = reds;
            this.totalReds = totalReds;
            this.secrets = secrets;
            this.totalSecrets = totalSecrets;
            this.activePanels = activePanels;
            this.totalPanels = totalPanels;
        }

        virtual public IEnumerator<Action> GetEnumerator()
        {
            yield return new StarLayoutInitAction();

            for (int line = 0; line < ld.courseDescription.Count; line++)
            {
                var descr = ld.courseDescription[line];
                StarsLineDescription sld = descr as StarsLineDescription;
                if (sld == null) continue;

                byte oldStarByte = oldStars[sld.offset];
                byte newStarByte = stars[sld.offset];
                
                if (oldStarByte != newStarByte)
                {
                    byte diffbyteFromOld = (byte)(((oldStarByte) ^ (newStarByte)) & newStarByte);
                    yield return new LastHighlight(line, diffbyteFromOld, false, sld.starMask);
                }

                if ((stars[sld.highlightOffset] & sld.highlightStarMask) != 0)
                {
                    yield return new TextHighlightAction(line, false, sld.text);
                }
            }

            for (int line = 0; line < ld.secretDescription.Count; line++)
            {
                var descr = ld.secretDescription[line];
                StarsLineDescription sld = descr as StarsLineDescription;
                if (sld == null) continue;

                byte oldStarByte = oldStars[sld.offset];
                byte newStarByte = stars[sld.offset];

                if (oldStarByte != newStarByte)
                {
                    byte diffbyteFromOld = (byte)(((oldStarByte) ^ (newStarByte)) & newStarByte);
                    yield return new LastHighlight(line, diffbyteFromOld, true, sld.starMask);
                }

                if ((stars[sld.highlightOffset] & sld.highlightStarMask) != 0)
                {
                    yield return new TextHighlightAction(line, true, sld.text);
                }
            }

            yield return new LastStarHighlightAction();

            int starCount = 0;
            for (int line = 0; line < ld.courseDescription.Count; line++)
            {
                var descr = ld.courseDescription[line];
                StarsLineDescription sld = descr as StarsLineDescription;
                if (sld == null) continue;
                
                byte oldStarByte = oldStars[sld.offset];
                byte newStarByte = stars[sld.offset];

                starCount += MemoryManager.countStars((byte)(newStarByte & sld.starMask), ld.starsShown);
                
                yield return new LineDrawAction(line, newStarByte, MemoryManager.countStars((byte)(newStarByte & sld.starMask), ld.starsShown) - MemoryManager.countStars((byte)(oldStarByte & sld.starMask), ld.starsShown), false, sld.starMask, ld.goldStar);
            }

            for (int line = 0; line < ld.secretDescription.Count; line++)
            {
                var descr = ld.secretDescription[line];
                StarsLineDescription sld = descr as StarsLineDescription;
                if (sld == null) continue;
                
                byte oldStarByte = oldStars[sld.offset];
                byte newStarByte = stars[sld.offset];

                starCount += MemoryManager.countStars((byte)(newStarByte & sld.starMask), ld.starsShown);
                
                yield return new LineDrawAction(line, newStarByte, MemoryManager.countStars((byte)(newStarByte & sld.starMask), ld.starsShown) - MemoryManager.countStars((byte)(oldStarByte & sld.starMask), ld.starsShown), true, sld.starMask, ld.goldStar);
            }

            if (otherStars is object)
            {
                for (int line = 0; line < ld.courseDescription.Count; line++)
                {
                    var descr = ld.courseDescription[line];
                    StarsLineDescription sld = descr as StarsLineDescription;
                    if (sld == null) continue;

                    byte starByte = otherStars[sld.offset];
                    yield return new LineDrawAction(line, starByte, 0, false, sld.starMask, ld.invertedStar);
                }

                for (int line = 0; line < ld.secretDescription.Count; line++)
                {
                    var descr = ld.secretDescription[line];
                    StarsLineDescription sld = descr as StarsLineDescription;
                    if (sld == null) continue;

                    byte starByte = otherStars[sld.offset];
                    yield return new LineDrawAction(line, starByte, 0, true, sld.starMask, ld.invertedStar);
                }
            }

            yield return new StarLayoutFiniAction(ld.GetLength());
            yield return new StarTextLineDrawAction();
            yield return new StringLineDrawAction(starCount.ToString().PadLeft(3) + "/" + ld.starAmount.PadRight(3));
            yield return new RedsSecretsDrawAction(reds, totalReds, secrets, totalSecrets, activePanels, totalPanels);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CollectablesOnlyDrawActions : DrawActions
    {
        LayoutDescriptionEx ld;
        byte[] stars;
        byte[] oldStars;
        int reds;
        int totalReds;
        int secrets;
        int totalSecrets;
        int activePanels;
        int totalPanels;

        public CollectablesOnlyDrawActions(LayoutDescriptionEx ld, byte[] stars, byte[] oldStars, int reds, int totalReds, int secrets, int totalSecrets, int activePanels, int totalPanels)
        {
            this.ld = ld;
            this.stars = stars;
            this.oldStars = oldStars;
            this.reds = reds;
            this.totalReds = totalReds;
            this.secrets = secrets;
            this.totalSecrets = totalSecrets;
            this.activePanels = activePanels;
            this.totalPanels = totalPanels;
        }

        override public IEnumerator<Action> GetEnumerator()
        {
            yield return new DrawCollectablesAction(reds, totalReds, 0, true);

            int maxLine = 0;

            int line = 1;
            while (totalReds > 0)
            {
                int localTotalReds = totalReds > 8 ? 8 : totalReds;
                int localReds = reds > 8 ? 8 : reds;
                
                yield return new DrawCollectablesAction(localReds, localTotalReds, line, false);

                totalReds -= 8;
                reds -= 8;
                line++;
                if (reds < 0) reds = 0;
            }

            maxLine = (maxLine > line) ? maxLine : line;

            yield return new SecretsDrawAction(secrets, totalSecrets, 0, true);
            line = 1;
            while (totalSecrets > 0)
            {
                int localTotalSecrets = totalSecrets > 8 ? 8 : totalSecrets;
                int localSecrets = secrets > 8 ? 8 : secrets;

                yield return new SecretsDrawAction(localSecrets, localTotalSecrets, line, false);

                totalSecrets -= 8;
                secrets -= 8;
                line++;
                if (secrets < 0) secrets = 0;
            }

            maxLine = (maxLine > line) ? maxLine : line;

            yield return new StarLayoutFiniAction(maxLine + 1);
        }
    }
}
