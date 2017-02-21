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

            ld = LayoutDescription.GenerateDefault();
            Image randomImage = new Bitmap(1,1);
            gm = new GraphicsManager(Graphics.FromImage(randomImage), ld);
            starPicture.Image = randomImage;
            mm = new MemoryManager(null, ld, gm);

            timer = new Timer();
            timer.Tick += new EventHandler(updateStars);
            timer.Interval = 1000;

            oldLE = new LineEntry(0, 0, 0, false, 0);

        }

        //Wandows, your forms are broken
        protected override void WndProc(ref Message m)
        {
            // Suppress the WM_UPDATEUISTATE message
            if (m.Msg == 0x128) return;
            base.WndProc(ref m);
        }

        private void resetForm()
        {
            connectButton.Enabled = true;
            layoutToolStripMenuItem.Enabled = false;
            iconsToolStripMenuItem.Enabled = false;
            timer.Stop();
        }

        private void updateStars(object sender, EventArgs e)
        {
            if (radioButtonA.Checked) mm.selectedFile = 0;
            if (radioButtonB.Checked) mm.selectedFile = 1;
            if (radioButtonC.Checked) mm.selectedFile = 2;
            if (radioButtonD.Checked) mm.selectedFile = 3;

            if (mm.ProcessActive())
            {
                resetForm();
                return;
            }
            
            if (enableAutoDeleteToolStripMenuItem.Checked)
            {
                try
                {
                    mm.DeleteStars();
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
            Image baseImage = new Bitmap(starPicture.Width, starPicture.Height);
            gm.graphics = Graphics.FromImage(baseImage);
            gm.PaintHUD();
            LineEntry le = mm.GetLine();
            if (le != null)
            {
                LineDescription lind = le.Secret ? ld.secretDescription[le.Line] : ld.courseDescription[le.Line];
                gm.AddLineHighlight(le, lind);
                /*
                if (le.Secret != oldLE.Secret || le.Line != oldLE.Line)
                {
                    
                    /*if (oldLE.Line != 0)
                    {
                        LineDescription oldLind = oldLE.Secret ? ld.secretDescription[oldLE.Line] : ld.courseDescription[oldLE.Line];
                        gm.RemoveLineHighlight(oldLE, oldLind);
                    }
                    oldLE = le;
                }*/
            }

            try
            {
                int totalDiff = 0;
                foreach (var entry in mm)
                {
                    int line = entry.Line;
                    byte stars = entry.StarByte;
                    int diff = entry.StarDiff;
                    bool isSecret = entry.Secret;

                    totalDiff += diff;
                    gm.DrawByte(stars, line, isSecret, entry.StarMask);
                }

                int starCount = oldStarCount + totalDiff;
                if (oldTotalCount != totalCountText.Text)
                {
                    ld.starAmount = totalCountText.Text;
                }

                //if (starCount != oldStarCount || oldTotalCount != totalCountText.Text)
                //{
                    gm.DrawStarNumber(totalCountText.Text, starCount);
                //}
                gm.DrawLastOutline();
                oldStarCount = starCount;
                oldTotalCount = totalCountText.Text;
            }
            catch (Win32Exception)
            {
                resetForm();
                return;
            }
            starPicture.Image.Dispose();
            starPicture.Image = baseImage;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process process = Process.GetProcessesByName("project64").First();
                mm = new MemoryManager(process, ld, gm);
                connectButton.Enabled = false;
                layoutToolStripMenuItem.Enabled = true;
                iconsToolStripMenuItem.Enabled = true;
                timer.Start();
                updateStars(null, null);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Can not find Project64!", "Process Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void importIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap image = mm.GetImage();
            ld = new LayoutDescription(ld.courseDescription, ld.secretDescription, image, ld.starAmount);
            InvalidateCache();
        }

        private void InvalidateCache()
        {
            gm = new GraphicsManager(gm.graphics, ld);
            mm = new MemoryManager(mm.Process, ld, gm);
            totalCountText.Text = ld.starAmount;
            oldStarCount = 0;
            oldTotalCount = "";
            oldLE = new LineEntry(0, 0, 0, false, 0);
            timer.Stop();
            updateStars(null, null);
            timer.Start();
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
                    if (curld == null)
                    {
                        ld.secretDescription[line] = new LineDescription("", true, 0, 0);
                        curld = ld.secretDescription[line];
                    }
                }
                else
                {
                    curld = ld.courseDescription[line];
                    if (curld == null)
                    {
                        ld.courseDescription[line] = new LineDescription("", true, 0, 0);
                        curld = ld.courseDescription[line];
                    }
                }

                if (star == 0 || curld.isTextOnly)
                {
                    Settings settings = new Settings(curld);
                    settings.ShowDialog();
                    ld.Trim();
                    InvalidateCache();
                }
                else
                {
                    curld.starMask = (byte)(curld.starMask ^ (1 << star));
                    InvalidateCache();
                }
                return;
            }
            catch (Exception) { }
        }

        private void starPicture_MouseMove(object sender, MouseEventArgs e)
        {
            picX = e.X; picY = e.Y;
        }

        private void LoadLayout(string name)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read);
            ld = (LayoutDescription)formatter.Deserialize(stream);
            ld.Trim();
            stream.Close();
            InvalidateCache();
        }

        private void SaveLayout(string name)
        {
            ld.Trim();
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, ld);
            stream.Close();
        }

        public void LoadExternal(string name)
        {
            byte[] data = File.ReadAllBytes(name);
            ld = LayoutDescription.DeserializeExternal(data, mm.GetImage());
            InvalidateCache();
        }

        public void SaveExternal(string name)
        {
            byte[] data = ld.SerializeExternal();
            File.WriteAllBytes(name, data);
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                LoadLayout("layout/" + mm.GetROMName() + ".sml");
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
            ld.SaturateStar();
            InvalidateCache();
        }

        private void loadDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ld = LayoutDescription.GenerateDefault();
            InvalidateCache();
        }

        private void loadFromToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Star Manager Layout (*.sml)|*.sml|Unified Layout (*.smlx)|*.smlx|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = false;
            openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\layout";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (openFileDialog.FilterIndex == 2)
                        LoadExternal(openFileDialog.FileName);
                    else
                        LoadLayout(openFileDialog.FileName);
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
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "Star Manager Layout (*.sml)|*.sml|Unified Layout (*.smlx)|*.smlx|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = false;
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\layout";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {

                    if (saveFileDialog.FilterIndex == 2)
                        SaveExternal(saveFileDialog.FileName);
                    else
                        SaveLayout(saveFileDialog.FileName);
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

        private void compressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ld.Trim();
        }

        private void drawImageFromRAMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gm.graphics.DrawImage(mm.GetImage(), 0, 0);
        }

        private void importStarMasksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "ROM Files (*.z64)|*.z64";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ROMManager rm = new ROMManager(openFileDialog.FileName);
                    rm.Parse(ld);
                    rm.Dispose();
                }
                catch (IOException)
                {
                    MessageBox.Show("Failed to load layout!", "Layour Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void recolorIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorPicker cp = new ColorPicker(ld);
            cp.ShowDialog();
            ld = new LayoutDescription(ld.courseDescription, ld.secretDescription, cp.newImg, ld.starAmount);
            InvalidateCache();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string name = "layout/" + mm.GetROMName() + ".sml";
                if (File.Exists(name))
                {
                    var result = MessageBox.Show("Layout for this hack already exists! Do you want to overwrite it?", "Layour Error", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (result == DialogResult.No) throw new IOException();
                }
                SaveLayout(name);
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
