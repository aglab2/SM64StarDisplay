using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Drawing.Text;

namespace StarDisplay
{
    public partial class MainWindow : Form
    {
        LayoutDescription ld;

        GraphicsManager gm;
        MemoryManager mm;
        ROMManager rm;

        string oldPath;

        Timer timer;

        int oldStarCount;
        string oldTotalCount;

        int picX, picY;

        Image baseImage;

        public MainWindow()
        {
            InitializeComponent();

            ld = LayoutDescription.GenerateDefault();
            Image randomImage = new Bitmap(1,1);
            gm = new GraphicsManager(Graphics.FromImage(randomImage), ld);
            starPicture.Image = randomImage;
            mm = new MemoryManager(null, ld, gm, null, null);

            timer = new Timer();
            timer.Tick += new EventHandler(updateStars);
            timer.Interval = 1000;

            baseImage = new Bitmap(starPicture.Width, starPicture.Height);

            oldPath = "";
        }

        private void resetForm()
        {
            connectButton.Enabled = true;
            layoutToolStripMenuItem.Enabled = false;
            iconsToolStripMenuItem.Enabled = false;
            timer.Stop();
            gm = new GraphicsManager(gm.graphics, ld);
            mm = new MemoryManager(null, ld, gm, null, null);
            rm = null;
            totalCountText.Text = "";
            oldStarCount = 0;
            oldTotalCount = "";
            try
            {
                connectToProcess();
            }
            catch (InvalidOperationException)
            { }
        }

        private void connectToProcess()
        {
            Process process = Process.GetProcessesByName("project64").First();
            mm = new MemoryManager(process, ld, gm, rm, mm.highlightPivot);
            connectButton.Enabled = false;
            layoutToolStripMenuItem.Enabled = true;
            iconsToolStripMenuItem.Enabled = true;
            timer.Start();
            updateStars(null, null);
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
            Graphics baseGraphics = Graphics.FromImage(baseImage);
            baseGraphics.Clear(Color.Black);
            baseGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            gm.graphics = Graphics.FromImage(baseImage);
            gm.PaintHUD();
            
            TextHighlightAction act = mm.GetCurrentLineAction();
            if (act != null)
            {
                gm.AddLineHighlight(act);
            }

            string currentROMPath = mm.GetAbsoluteROMPath();
            if (oldPath != currentROMPath)
            {
                oldPath = currentROMPath;
                try
                {
                    rm = new ROMManager(currentROMPath);
                    mm.rm = rm;
                }
                catch (IOException)
                {
                    oldPath = "";
                }
                InvalidateCache();
            }

            try
            {
                int totalDiff = 0;
                foreach (var entry in mm.GetDrawActions())
                {
                    if (entry is RedsSecretsDrawAction && !showRedsToolStripMenuItem.Checked) continue;
                    //if (entry is StarHighlightAction && !displayHighlightToolStripMenuItem.Checked) continue;
                    if (entry is LastStarHighlightAction && !displayHighlightToolStripMenuItem.Checked) continue;
                    if (entry is LastHighlight && !displayHighlightToolStripMenuItem.Checked) continue;
                    entry.execute(gm);
                    LineDrawAction lda = entry as LineDrawAction;
                    if (lda != null) totalDiff += lda.StarDiff;
                }

                int starCount = oldStarCount + totalDiff;
                if (oldTotalCount != totalCountText.Text)
                {
                    ld.starAmount = totalCountText.Text;
                }

                gm.DrawStarNumber(totalCountText.Text, starCount);
                oldStarCount = starCount;
                oldTotalCount = totalCountText.Text;
            }
            catch (Win32Exception)
            {
                resetForm();
                return;
            }
            catch (NullReferenceException error)
            {
                Console.WriteLine(error);
                resetForm();
                return;
            }

            baseGraphics.Dispose();
            //Image img = starPicture.Image;
            starPicture.Image = baseImage;
            //img.Dispose();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                connectToProcess();
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
            mm.ld = ld;
            gm.ld = ld;
            mm.InvalidateCache();
            gm.InvalidateCache();
            totalCountText.Text = ld.starAmount;
            oldStarCount = 0;
            oldTotalCount = "";
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
            if (ld.darkStar == null) ld.generateDarkStar();
            if (ld.redOutline == null) ld.generateOutline();
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
                    rm.ParseStars(ld);
                    rm.Dispose();
                    InvalidateCache();
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
            ld.goldStar = cp.newImg;
            ld.generateDarkStar();
            InvalidateCache();
        }

        private void resetHighlightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mm.resetHighlightPivot();
            gm.IsFirstCall = true;
        }

        private void loadCustomFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Font file (*.ttf)|*.ttf";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    PrivateFontCollection collection = new PrivateFontCollection();
                    collection.AddFontFile(openFileDialog.FileName);
                    FontFamily[] fontFamilies = collection.Families;
                    FontFamily fontFamily = fontFamilies.First();
                    if (fontFamily == null) return;

                    gm.collection = collection;
                    gm.fontFamily = fontFamily;
                    gm.fontName = fontFamily.Name;
                }
                catch (IOException)
                {
                    MessageBox.Show("Failed to load layout!", "Layour Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void changeStarTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeTextDialog ctd = new ChangeTextDialog(gm.StarText);
            ctd.ShowDialog();
            gm.StarText = ctd.text;
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
