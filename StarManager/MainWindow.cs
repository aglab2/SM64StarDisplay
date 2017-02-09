using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace StarDisplay
{
    public partial class MainWindow : Form
    {
        LayoutDescription ld;

        GraphicsManager gm;
        MemoryManager mm;

        Timer timer;

        int oldStarCount;
        string oldTotalCount;

        int picX, picY;

        LineEntry oldLE;

        public MainWindow()
        {
            InitializeComponent();
            ld = LayoutDescription.generateDefault();
            gm = new GraphicsManager(starPicture.CreateGraphics(), ld);
            mm = new MemoryManager(null, ld);

            timer = new Timer();
            timer.Tick += new EventHandler(updateStars);
            timer.Interval = 1000;

            oldLE = new LineEntry(0, 0, 0, false, 0);
        }

        private void resetForm()
        {
            connectButton.Enabled = true;
            layoutToolStripMenuItem.Enabled = false;
            timer.Stop();
        }

        private void updateStars(object sender, EventArgs e)
        {
            if (radioButtonA.Checked) mm.selectedFile = 0;
            if (radioButtonB.Checked) mm.selectedFile = 1;
            if (radioButtonC.Checked) mm.selectedFile = 2;
            if (radioButtonD.Checked) mm.selectedFile = 3;

            if (mm.isProcessActive())
            {
                resetForm();
                return;
            }
            
            if (enableAutoDeleteToolStripMenuItem.Checked)
            {
                try
                {
                    mm.deleteStars();
                }
                catch (Win32Exception)
                {
                    resetForm();
                    return;
                }
                catch (IOException)
                {
                    MessageBox.Show("Can not modify savefiles. Trying launching with elevated rights!", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    enableAutoDeleteToolStripMenuItem.Checked = false;
                }
            }

            //Display stars routine
            gm.graphics = starPicture.CreateGraphics();
            LineEntry le = mm.getLine();
            if (le != null)
            {
                LineDescription lind = le.isSecret ? ld.secretDescription[le.line] : ld.courseDescription[le.line];
                if (le.isSecret != oldLE.isSecret || le.line != oldLE.line)
                {
                    gm.drawYellowString(le, lind);
                    if (oldLE.line != 0)
                    {
                        LineDescription oldLind = oldLE.isSecret ? ld.secretDescription[oldLE.line] : ld.courseDescription[oldLE.line];
                        gm.drawBlackString(oldLE, oldLind);
                    }
                    oldLE = le;
                }
            }

            try
            {
                int totalDiff = 0;
                foreach (var entry in mm)
                {
                    int line = entry.line;
                    byte stars = entry.starByte;
                    int diff = entry.starDiff;
                    bool isSecret = entry.isSecret;

                    totalDiff += diff;
                    gm.drawByte(stars, line, isSecret, entry.starMask);
                }

                int starCount = oldStarCount + totalDiff;
                if (oldTotalCount != totalCountText.Text)
                {
                    ld.starAmount = totalCountText.Text;
                }

                if (starCount != oldStarCount || oldTotalCount != totalCountText.Text)
                {
                    gm.drawStarNumber(totalCountText.Text, starCount);
                    //TODO: Move to graphics
                    /*string totalCount = totalCountText.Text;
                    string starLine = starCount.ToString().PadLeft(3) + "/" + totalCount.PadRight(3);

                    Font drawFont = new Font("Courier", 15);
                    SolidBrush drawBrush = new SolidBrush(System.Drawing.Color.Black);
                    int totalStarLine = Math.Max(ld.courseDescription.Length, ld.secretDescription.Length) + 1;
                    gm.graphics.FillRectangle(drawBrush, new Rectangle(15, totalStarLine * 23 + 2, 1000, 20));
                    drawBrush.Dispose();
                    drawBrush = new SolidBrush(System.Drawing.Color.LightGray);
                    gm.graphics.DrawString(starLine, drawFont, drawBrush, 120, totalStarLine * 23 + 2);
                    drawFont.Dispose();
                    drawBrush.Dispose();*/
                }
                oldStarCount = starCount;
                oldTotalCount = totalCountText.Text;
            }
            catch (Win32Exception)
            {
                resetForm();
                return;
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            gm.paintHUD(starPicture.Width, starPicture.Height);
            try
            {
                Process process = Process.GetProcessesByName("project64").First();
                mm = new MemoryManager(process, ld);
                connectButton.Enabled = false;
                layoutToolStripMenuItem.Enabled = true;
                timer.Start();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Can not find Project64!", "Process Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void importIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            
            openFileDialog.Filter = "ROM Files (*.z64)|*.z64|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.Delete(Path.GetDirectoryName(Application.ExecutablePath) + "/star.png");
                }
                catch (Exception) { }
                string romName = openFileDialog.FileName;
                Process pictureLoadProc = new Process();
                pictureLoadProc.StartInfo.UseShellExecute = false;
                pictureLoadProc.StartInfo.RedirectStandardOutput = true;
                pictureLoadProc.StartInfo.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                pictureLoadProc.StartInfo.FileName = "externals/n64rawgfx.exe";
                pictureLoadProc.StartInfo.Arguments = "-r \"" + romName + "\" -b star.bmp -m export -f RGBA -d16 -a 0x00807956 -x 16 -y 16";
                pictureLoadProc.StartInfo.CreateNoWindow = true;
                pictureLoadProc.Start();
                pictureLoadProc.WaitForExit();

                if (!File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "/star.bmp"))
                {
                    MessageBox.Show("Can not extract data from ROM!", "ROM Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool timerEnabled = timer.Enabled;
                if (timerEnabled) timer.Stop();
                
                Process pictureTransformProc = new Process();
                pictureTransformProc.StartInfo.UseShellExecute = false;
                pictureTransformProc.StartInfo.RedirectStandardOutput = true;
                pictureTransformProc.StartInfo.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                pictureTransformProc.StartInfo.FileName = "externals/nconvert.exe";
                pictureTransformProc.StartInfo.Arguments = "-out png star.bmp";
                pictureTransformProc.StartInfo.CreateNoWindow = true;
                pictureTransformProc.Start();
                pictureTransformProc.WaitForExit();
                
                try
                {
                    File.Delete(Path.GetDirectoryName(Application.ExecutablePath) + "/star.bmp");
                }
                catch (Exception) { }

                Bitmap goldStar;
                using (var bmpTemp = new Bitmap(Path.GetDirectoryName(Application.ExecutablePath) + "/star.png"))
                {
                    goldStar = new Bitmap(bmpTemp);
                }
                Bitmap darkStar = new Bitmap(goldStar.Width, goldStar.Height);

                for (int i = 0; i < goldStar.Height; i++) {
                    for (int j = 0; j < goldStar.Width; j++)
                    {
                        double h; double s; double l;
                        Color c = goldStar.GetPixel(i, j);
                        ColorRGB crgb = new ColorRGB(c);
                        ColorRGB.RGB2HSL(crgb, out h, out s, out l);

                        s = 0;

                        ColorRGB nrgb = ColorRGB.HSL2RGB(h, s, l);
                        Color n = Color.FromArgb(c.A, nrgb.R, nrgb.G, nrgb.B);
                        darkStar.SetPixel(i, j, n);
                    }
                }

                ld.darkStar = darkStar;
                ld.goldStar = goldStar;
                invalidateCache();
                if (timerEnabled) timer.Start();
            }
        }

        private void invalidateCache()
        {
            mm = new MemoryManager(mm.process, ld);
            gm = new GraphicsManager(gm.graphics, ld);
            totalCountText.Text = ld.starAmount;
            oldStarCount = 0;
            oldTotalCount = "";
            oldLE = new LineEntry(0, 0, 0, false, 0);
            gm.paintHUD(starPicture.Width, starPicture.Height);
        }

        private void starPicture_Click(object sender, EventArgs e)
        {
            if (!configureDisplayToolStripMenuItem.Checked) return;
            int X = picX; int Y = picY;
            int line = Y / 23;
            bool isSecret = (X / 180) == 1;
            int star = (X - (isSecret ? 180 : 0)) / 20;
            if (star == 8) return;

            try
            {
                LineDescription curld;
                if (isSecret)
                {
                    curld = ld.secretDescription[line];
                }
                else
                {
                    curld = ld.courseDescription[line];
                }

                if (star == 0 || curld.isTextOnly)
                {
                    Settings settings = new Settings(curld);
                    settings.ShowDialog();
                    invalidateCache();
                }
                else
                {
                    curld.starMask = (byte)(curld.starMask ^ (1 << star));
                    invalidateCache();
                }
                return;
            }
            catch (Exception) { }
        }

        private void starPicture_MouseMove(object sender, MouseEventArgs e)
        {
            picX = e.X; picY = e.Y;
        }

        private void loadLayout(string name)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read);
            ld = (LayoutDescription)formatter.Deserialize(stream);
            stream.Close();
            invalidateCache();
        }

        private void saveLayout(string name)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, ld);
            stream.Close();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                loadLayout("layout/" + mm.getROMName() + ".sml");
            }catch (IOException){
                var result = MessageBox.Show("Cannot find layout for this hack. Do you want to load layout from file?", "Layour Error", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Yes)
                {
                    loadFromToolStripMenuItem_Click(sender, e);
                }
            }
        }

        private void saturateIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ld.saturateStar();
            invalidateCache();
        }

        private void loadDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ld = LayoutDescription.generateDefault();
            invalidateCache();
        }

        private void loadFromToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Star Manager Layout (*.sml)|*.sml|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = false;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\layout";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    loadLayout(openFileDialog.FileName);
                }
                catch (IOException)
                {
                    MessageBox.Show("Failed to load layout!", "Layour Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Star Manager Layout (*.sml)|*.sml|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = false;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\layout";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    saveLayout(openFileDialog.FileName);
                }
                catch (IOException)
                {
                    MessageBox.Show("Failed to save layout!", "Layour Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            AboutDialog ad = new AboutDialog();
            ad.ShowDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string name = "layout/" + mm.getROMName() + ".sml";
                if (File.Exists(name))
                {
                    var result = MessageBox.Show("Layout for this hack already exists! Do you want to overwrite it?", "Layour Error", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (result == DialogResult.No) throw new IOException();
                }
                saveLayout(name);
            }
            catch (IOException)
            {
                var result = MessageBox.Show("Cannot save layout for this hack. Do you want to save layout to different file?", "Layour Error", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Yes)
                {
                    saveAsToolStripMenuItem_Click(sender, e);
                }
            }
        }
    }
}
