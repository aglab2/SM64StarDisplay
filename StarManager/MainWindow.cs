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
        UpdateManager um;

        Timer timer;
        
        UInt16 oldCRC;
        int oldStarCount;

        int picX, picY;

        Image baseImage;

        bool isUpdateRequested = false;

        ToolStripMenuItem[] fileMenuItems;

        public MainWindow()
        {
            InitializeComponent();

            ld = LayoutDescription.GenerateDefault();
            Image randomImage = new Bitmap(1,1);
            gm = new GraphicsManager(Graphics.FromImage(randomImage), ld);
            starPicture.Image = randomImage;
            mm = new MemoryManager(null, ld, gm, null, null);
            um = new UpdateManager();

            timer = new Timer();
            timer.Tick += new EventHandler(updateStars);
            timer.Interval = 1000;

            baseImage = new Bitmap(starPicture.Width, starPicture.Height);

            oldCRC = 0;

            fileMenuItems = new ToolStripMenuItem[4];
            fileMenuItems[0] = fileAToolStripMenuItem;
            fileMenuItems[1] = fileBToolStripMenuItem;
            fileMenuItems[2] = fileCToolStripMenuItem;
            fileMenuItems[3] = fileDToolStripMenuItem;

            timer.Start();
        }

        private void resetForm()
        {
            connectToolStripMenuItem.Enabled = true;
            layoutToolStripMenuItem.Enabled = false;
            iconsToolStripMenuItem.Enabled = false;
            timer.Stop();
            gm = new GraphicsManager(gm.graphics, ld);
            mm = new MemoryManager(null, ld, gm, null, null);
            rm = null;
            oldStarCount = 0;
            try
            {
                connectToProcess();
            }
            catch (InvalidOperationException)
            {
                timer.Start();
            }
        }

        private void connectToProcess()
        {
            Process process = Process.GetProcessesByName("project64").First();
            mm = new MemoryManager(process, ld, gm, rm, mm.highlightPivot);
            connectToolStripMenuItem.Enabled = false;
            layoutToolStripMenuItem.Enabled = true;
            iconsToolStripMenuItem.Enabled = true;
            timer.Start();
            updateStars(null, null); //calls resetForm automatically!
        }

        private void updateStars(object sender, EventArgs e)
        {
            if (fileAToolStripMenuItem.Checked) mm.selectedFile = 0;
            if (fileBToolStripMenuItem.Checked) mm.selectedFile = 1;
            if (fileCToolStripMenuItem.Checked) mm.selectedFile = 2;
            if (fileDToolStripMenuItem.Checked) mm.selectedFile = 3;

            try
            {
                if (um.IsCompleted())
                {
                    if (!isUpdateRequested && !um.IsUpdated())
                    {
                        isUpdateRequested = true;
                        if (MessageBox.Show(String.Format("Update for Star Display available!\n\n{0}\n\nDo you want to download it now?", um.UpdateName()), "Update",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                        {
                            Process.Start("https://github.com/aglab2/SM64StarDisplay/blob/master/StarDisplay.zip?raw=true");
                        }
                    }
                }
            }catch(Exception) { }

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

            try
            {
                //Display stars routine
                Graphics baseGraphics = Graphics.FromImage(baseImage);
                baseGraphics.Clear(Color.Black);
                baseGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                gm.graphics = Graphics.FromImage(baseImage);
                if (!showCollectablesOnlyToolStripMenuItem.Checked)
                {

                    gm.PaintHUD();

                    TextHighlightAction act = mm.GetCurrentLineAction();
                    if (act != null)
                    {
                        gm.AddLineHighlight(act);
                    }
                }

                UInt16 currentCRC = mm.GetRomCRC();
                if (currentCRC != oldCRC)
                {
                    oldCRC = currentCRC;
                    try
                    {
                        rm = new ROMManager(mm.GetROM());
                        mm.rm = rm;
                        try
                        {
                            LoadLayoutNoInvalidate("layout/" + rm.GetROMName() + ".sml");
                        }
                        catch (IOException)
                        {
                            LoadDefaultLayoutNoInvalidate();
                        }

                        mm.resetHighlightPivot();
                        gm.IsFirstCall = true;
                    }
                    catch (IndexOutOfRangeException) //can be generated by box reader
                    { }
                    catch (IOException)
                    { oldCRC = 0; }

                    InvalidateCacheNoResetRM();
                }

                int totalDiff = 0;
                var actions = showCollectablesOnlyToolStripMenuItem.Checked ? mm.GetCollectablesOnlyDrawActions() : mm.GetDrawActions();
                if (actions == null) return;
                foreach (var entry in actions)
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

                if (!showCollectablesOnlyToolStripMenuItem.Checked) gm.DrawStarNumber(ld.starAmount, starCount);
                oldStarCount = starCount;

                baseGraphics.Dispose();
                starPicture.Image = baseImage;
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
            mm.rm = rm;
            gm.InvalidateCache();
            oldStarCount = 0;
            timer.Stop();
            updateStars(null, null);
            timer.Start();
        }

        private void InvalidateCacheNoResetRM()
        {
            mm.ld = ld;
            gm.ld = ld;
            mm.InvalidateCache();
            mm.rm = rm;
            gm.InvalidateCache();
            oldStarCount = 0;
        }

        private void EditDisplay()
        {
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

        private void EditFile()
        {
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
                    //caps editing, keys editing, maybe other stuff?
                }
                else
                {
                    if ((curld.starMask & (byte)(1 << star)) != 0)
                    {
                        mm.WriteToFile(curld.offset, star - 1);
                        //InvalidateCache();
                    }
                }
                return;
            }
            catch (Exception) { }
        }

        private void starPicture_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;

            if (me.Button == MouseButtons.Left)
            {
                if (configureDisplayToolStripMenuItem.Checked)
                    EditDisplay();

                if (editFileToolStripMenuItem.Checked)
                    EditFile();
            }
        }

        private void starPicture_MouseMove(object sender, MouseEventArgs e)
        {
            picX = e.X; picY = e.Y;
        }

        private void LoadLayout(string name)
        {
            LoadLayoutNoInvalidate(name);
            InvalidateCache();
        }

        private void LoadLayoutNoInvalidate(string name)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read);
            ld = (LayoutDescription)formatter.Deserialize(stream);
            if (ld.darkStar == null) ld.generateDarkStar();
            if (ld.redOutline == null) ld.generateOutline();
            ld.Trim();
            stream.Close();
        }

        private void LoadDefaultLayoutNoInvalidate()
        {
            ld = LayoutDescription.GenerateDefault();
            ld.Trim();
        }

        private void LoadDefaultLayout()
        {
            LoadDefaultLayoutNoInvalidate();
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
                    MessageBox.Show("Failed to import star masks!", "Layour Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void loadROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "ROM Files (*.z64)|*.z64";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    rm = new ROMManager(openFileDialog.FileName);
                    InvalidateCache();
                }
                catch (IOException)
                {
                    MessageBox.Show("Failed to load rom!", "Layour Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void importIconsFromROMToolStripMenuItem_Click(object sender, EventArgs e)
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
                    if (rm == null) throw new IOException();
                    Bitmap image = rm.GetStarImage();
                    ld = new LayoutDescription(ld.courseDescription, ld.secretDescription, image, ld.starAmount);
                    InvalidateCache();
                }
                catch (IOException)
                {
                    MessageBox.Show("Failed to import star icons!", "Layour Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void replaceBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "PNG Images (*.png)|*.png";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Image img = Image.FromFile(openFileDialog.FileName);
                    if (img == null) throw new IOException();
                    gm.SetBackground(img);
                    
                    InvalidateCache();
                }
                catch (IOException)
                {
                    MessageBox.Show("Failed to import star icons!", "Layour Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void recolorTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorPicker cp = new ColorPicker(ld);
            cp.ShowDialog();
            Color c = cp.pickedColor;
            InvalidateCache();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

            foreach (ToolStripMenuItem item in fileMenuItems)
            {
                if (item != sender)
                {
                    item.Checked = false;
                }
            }
        }

        private void starsInHackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeTextDialog ctd = new ChangeTextDialog(ld.starAmount);
            ctd.ShowDialog();
            ld.starAmount = ctd.text;
            InvalidateCache();
        }

        private void configureDisplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editFileToolStripMenuItem.Checked = false;
        }

        private void editFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            configureDisplayToolStripMenuItem.Checked = false;
        }

        private void editFlagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FlagsEditForm fef = new FlagsEditForm(mm.GetStars());
            fef.ShowDialog();
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
