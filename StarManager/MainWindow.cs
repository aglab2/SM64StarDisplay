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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace StarDisplay
{
    public partial class MainWindow : Form
    {
        LayoutDescriptionEx ld;

        GraphicsManager gm;
        MemoryManager mm;
        ROMManager rm;
        UpdateManager um;
        SettingsManager sm;

        System.Threading.Timer timer;
        
        UInt16 oldCRC;

        int picX, picY;

        Image baseImage;

        bool isUpdateRequested = false;

        ToolStripMenuItem[] fileMenuItems;
        
        List<Type> componentsClasses;

        bool isEmulatorStarted = false;
        bool isHackLoaded = false;
        bool isOffsetsFound = false;

        Thread magicThread;

        const int period = 30;
        
        public MainWindow()
        {
            InitializeComponent();

            ld = LayoutDescriptionEx.GenerateDefault();

            // We need big enough picture to perform tests
            Image randomImage = new Bitmap(300, 50);
            gm = new GraphicsManager(Graphics.FromImage(randomImage), ld);
            starPicture.Image = randomImage;
            mm = new MemoryManager(null);
            um = new UpdateManager();

            try
            {
                LoadSettings();
            }
            catch(Exception)
            {
                sm = new SettingsManager();
            }

            if (sm == null || !sm.isValid())
            {
                sm = new SettingsManager();
            }

            timer = new System.Threading.Timer(UpdateStars, null, period, Timeout.Infinite);
            
            oldCRC = 0;

            fileMenuItems = new ToolStripMenuItem[4];
            fileMenuItems[0] = fileAToolStripMenuItem;
            fileMenuItems[1] = fileBToolStripMenuItem;
            fileMenuItems[2] = fileCToolStripMenuItem;
            fileMenuItems[3] = fileDToolStripMenuItem;
            
            IEnumerable<Type> types = Action.GetAllSubclasses();
            componentsClasses = new List<Type>();

            foreach (Type type in types)
            {
                MethodInfo info = type.GetMethod("DrawConfigs");
                if (info == null)
                    continue;

                componentsClasses.Add(type);
            }

            starPicture.Width = this.Width;
            starPicture.Height = this.Height;
        }

        ~MainWindow()
        {
            magicThread.Abort();
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            WaitHandle handle = new AutoResetEvent(false);
            timer.Dispose(handle);
            handle.WaitOne();
        }

        private void doMagicThread()
        {
            while (mm != null && !mm.isMagicDone())
                try
                {
                    mm.doMagic();
                }
                catch (Exception) { }
        }
        
        private void DrawIntro()
        {
            menuStrip.Enabled = false;

            //Steps to achieve success here!
            baseImage = new Bitmap(starPicture.Width, starPicture.Height);
            Graphics baseGraphics = Graphics.FromImage(baseImage);
            baseGraphics.Clear(Color.Black);
            baseGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            gm.graphics = Graphics.FromImage(baseImage);

            gm.DrawIntro(isEmulatorStarted, isHackLoaded, isOffsetsFound);
            baseGraphics.Dispose();
            starPicture.Image = baseImage;
        }

        private void ResetForm()
        {
            connectToolStripMenuItem.Enabled = true;
            layoutToolStripMenuItem.Enabled = false;
            iconsToolStripMenuItem.Enabled = false;
            rm = null;
            try
            {
                ConnectToProcess();
            }
            catch (Exception) { }
        }

        private void ConnectToProcess()
        {
            Process process = Process.GetProcessesByName("project64").FirstOrDefault();
            mm = new MemoryManager(process);
            connectToolStripMenuItem.Enabled = false;
            layoutToolStripMenuItem.Enabled = true;
            iconsToolStripMenuItem.Enabled = true;
        }

        // Call from other thread for safe UI invoke
        private void SafeInvoke(MethodInvoker updater, bool forceSynchronous = false)
        {
            if (InvokeRequired)
            {
                if (forceSynchronous)
                {
                    Invoke((MethodInvoker)delegate { SafeInvoke(updater, forceSynchronous); });
                }
                else
                {
                    BeginInvoke((MethodInvoker)delegate { SafeInvoke(updater, forceSynchronous); });
                }
            }
            else
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException("Control is already disposed.");
                }

                updater();
            }
        }

        private void UpdateStars(object sender)
        {
            isEmulatorStarted = false;
            isHackLoaded = false;
            isOffsetsFound = false;

            try
            {
                if (um.IsCompleted())
                {
                    if (!isUpdateRequested && !um.IsUpdated())
                    {
                        isUpdateRequested = true;
                        this.SafeInvoke((MethodInvoker)delegate
                        {
                            DialogResult result = MessageBox.Show(String.Format("Update for Star Display available!\n\n{0}\n\nDo you want to download it now? Press cancel to skip update", um.UpdateName()), "Update",
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk);
                            if (result == DialogResult.Yes)
                            {
                                Process.Start("https://github.com/aglab2/SM64StarDisplay/blob/master/StarDisplay.zip?raw=true");
                            }
                            if (result == DialogResult.Cancel)
                            {
                                um.WritebackUpdate();
                            }
                        });
                    }
                }
            }
            catch (Exception) { }

            try
            {

                if (mm.ProcessActive())
                {
                    this.SafeInvoke((MethodInvoker)delegate { DrawIntro(); ResetForm(); });
                    return;
                }
                
                isEmulatorStarted = true;

                // Well, that's just a minor magic but it works
                if (mm.GetTitle().Contains("-"))
                    isHackLoaded = true;

                if (!mm.isMagicDone())
                {
                    if (magicThread == null || !magicThread.IsAlive)
                    {
                        magicThread = new Thread(doMagicThread);
                        magicThread.Start();
                    }

                    this.SafeInvoke((MethodInvoker)delegate { DrawIntro(); });
                    return;
                }
                
                isOffsetsFound = true;


                try
                {
                    // Just do nothing
                    if (!mm.isReadyToRead())
                        return;

                    mm.PerformRead();
                }
                catch (Exception)
                {
                    this.SafeInvoke((MethodInvoker)delegate { DrawIntro(); ResetForm(); });
                    return;
                }

                bool mmIsInvalidated = mm.CheckInvalidated();
                bool gmIsInvalidated = gm.CheckInvalidated();

                if (mmIsInvalidated)
                    Console.WriteLine("MM Invalidated!");
                if (gmIsInvalidated)
                    Console.WriteLine("GM Invalidated!");
                
                // We do not draw anything!
                if (!mmIsInvalidated && !gmIsInvalidated)
                {
                    return;
                }

                gm.TestFont();

                if (enableAutoDeleteToolStripMenuItem.Checked)
                {
                    try
                    {
                        mm.DeleteStars();
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Can not modify savefiles. Trying launching with elevated rights!", "Write Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        enableAutoDeleteToolStripMenuItem.Checked = false;
                    }
                }

                //Display stars routine
                baseImage = new Bitmap(starPicture.Width, starPicture.Width * 4);

                Graphics baseGraphics = Graphics.FromImage(baseImage);
                baseGraphics.Clear(Color.Black);
                baseGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                gm.graphics = Graphics.FromImage(baseImage);

                if (!sm.GetConfig(MainWindowsSettingsAction.collectablesOnlyConfigureName, false))
                {
                    TextHighlightAction act = mm.GetCurrentLineAction(ld);
                    if (act != null)
                    {
                        gm.AddLineHighlight(act);
                    }
                }

                UInt16 currentCRC = mm.GetRomCRC();
                if (currentCRC != oldCRC || rm == null)
                {
                    oldCRC = currentCRC;
                    try
                    {
                        rm = new ROMManager(mm.GetROM());
                        try
                        {
                            LoadLayoutNoInvalidate("layout/" + rm.GetROMName() + ".sml");
                        }
                        catch (IOException)
                        {
                            LoadDefaultLayoutNoInvalidate();
                        }

                        gm.IsFirstCall = true;
                    }
                    catch (IndexOutOfRangeException) //can be generated by box reader
                    { }
                    catch (Exception)
                    { oldCRC = 0; }

                    InvalidateCacheNoResetRM();
                }

                var actions = sm.GetConfig(MainWindowsSettingsAction.collectablesOnlyConfigureName, false) ? mm.GetCollectablesOnlyDrawActions(ld, rm) : mm.GetDrawActions(ld, rm);
                if (actions == null) return;

                int lineOffset = 0;
                foreach (var entry in actions)
                {
                    lineOffset += entry.Execute(gm, lineOffset, sm);
                }
                
                baseGraphics.Dispose();
                this.SafeInvoke(delegate {
                    menuStrip.Enabled = true;
                    starPicture.Image = baseImage;
                    starPicture.Height = (int) (lineOffset * gm.SHeight) + 10;
                    this.Height = (int)(lineOffset * gm.SHeight) + 48;
                });
            }
            catch (Win32Exception)
            {
                this.SafeInvoke((MethodInvoker)delegate { ResetForm(); DrawIntro(); });
            }
            catch (NullReferenceException)
            {
                this.SafeInvoke((MethodInvoker)delegate { ResetForm(); DrawIntro(); });
            }
            // Not really important exception, just a placeholder basically
            catch (ObjectDisposedException) { }
            finally
            {
                timer.Change(period, Timeout.Infinite);
            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                ConnectToProcess();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Can not find Project64!", "Process Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void importIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap image = mm.GetImage();
            ld = new LayoutDescriptionEx(ld.courseDescription, ld.secretDescription, image, ld.starAmount, ld.starsShown);
            InvalidateCache();
        }

        private void InvalidateCache()
        {
            gm.Ld = ld;
            mm.InvalidateCache();
            gm.InvalidateCache();
        }

        // Should be called from UpdateStars not to cause recursion
        private void InvalidateCacheNoResetRM()
        {
            gm.Ld = ld;
            mm.InvalidateCache();
            gm.InvalidateCache();
        }

        private void EditDisplay()
        {
            int X = picX; int Y = picY;
            int line = (int) Math.Floor(Y / gm.SHeight);
            bool isSecret = ((int) Math.Floor(X / (gm.Width / 2))) == 1;
            int star = (int) Math.Floor((X - (isSecret ? (gm.Width / 2) : 0)) / gm.SWidth);
            if (star > ld.starsShown) return;

            try
            {
                LineDescriptionEx curld;
                if (isSecret)
                {
                    curld = ld.secretDescription[line];
                    if (curld == null)
                    {
                        ld.secretDescription[line] = new TextOnlyLineDescription("");
                        curld = ld.secretDescription[line];
                    }
                }
                else
                {
                    curld = ld.courseDescription[line];
                    if (curld == null)
                    {
                        ld.courseDescription[line] = new TextOnlyLineDescription("");
                        curld = ld.courseDescription[line];
                    }
                }

                if (star == 0 || curld is TextOnlyLineDescription)
                {
                    Settings settings = new Settings(curld, ld.starsShown);
                    settings.ShowDialog();
                    ld.starsShown = settings.starsShown;
                    if (isSecret)
                        ld.secretDescription[line] = settings.lind;
                    else
                        ld.courseDescription[line] = settings.lind;

                    ld.PrepareForEdit();
                    gm.TestFont();
                }
                if (star > 0 && curld is StarsLineDescription sld)
                {
                    sld.starMask = (byte)(sld.starMask ^ (1 << (star - 1)));
                }

                ld.RecountStars();
                InvalidateCache();
                return;
            }
            catch (Exception) { }
        }

        WarpDialog wd;

        private void EditWarps()
        {
            int X = picX; int Y = picY;
            int line = (int)Math.Floor(Y / gm.SHeight);
            bool isSecret = ((int)Math.Floor(X / (gm.Width / 2))) == 1;
            int star = (int)Math.Floor((X - (isSecret ? (gm.Width / 2) : 0)) / gm.SWidth);
            if (line > ld.GetLength()) return;

            try
            {
                LineDescriptionEx curld = null;
                do
                {

                    curld = isSecret ? ld.secretDescription[line] : ld.courseDescription[line];

                    if (line > ld.GetLength())
                        return;

                    line++;
                } while (curld == null || (curld is TextOnlyLineDescription));
                
                
                if (curld is StarsLineDescription sld)
                    mm.WriteWarp(wd.warp, (byte) LevelInfo.FindByEEPOffset(sld.offset).Level, wd.area);

                return;
            }
            catch (Exception) { }
        }

        private void EditFile()
        {
            int X = picX; int Y = picY;
            int line = (int)Math.Floor(Y / gm.SHeight);
            bool isSecret = ((int)Math.Floor(X / (gm.Width / 2))) == 1;
            int star = (int)Math.Floor((X - (isSecret ? (gm.Width / 2) : 0)) / gm.SWidth);
            if (star == 8) return;

            try
            {
                LineDescriptionEx curld;
                if (isSecret)
                {
                    curld = ld.secretDescription[line];
                    if (curld == null)
                    {
                        ld.secretDescription[line] = new TextOnlyLineDescription("");
                        curld = ld.secretDescription[line];
                    }
                }
                else
                {
                    curld = ld.courseDescription[line];
                    if (curld == null)
                    {
                        ld.courseDescription[line] = new TextOnlyLineDescription("");
                        curld = ld.courseDescription[line];
                    }
                }

                if (curld is StarsLineDescription sld)
                {
                    if (star == 0)
                    {
                        //caps editing, keys editing, maybe other stuff?
                        if (sld.starMask != 0)
                        {
                            for (int i = 0; i < ld.starsShown; i++)
                            {
                                if ((sld.starMask & (byte)(1 << i)) != 0)
                                {
                                    mm.WriteToFile(sld.offset, i);
                                }
                            }
                            InvalidateCache();
                        }
                    }
                    else
                    {
                        if ((sld.starMask & (byte)(1 << (star - 1))) != 0)
                        {
                            mm.WriteToFile(sld.offset, star - 1);
                            InvalidateCache();
                        }
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
                if (configureLayoutToolStripMenuItem.Checked)
                    EditDisplay();

                if (editFileToolStripMenuItem.Checked)
                    EditFile();

                if (warpToLevelToolStripMenuItem.Checked)
                    EditWarps();
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

            string ext = Path.GetExtension(name);

            Stream stream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read);
            object layout = formatter.Deserialize(stream);
            string layoutClassName = layout.GetType().Name;

            if (layoutClassName == "LayoutDescription")
                ld = new LayoutDescriptionEx((LayoutDescription) layout);
            else if (layoutClassName == "LayoutDescriptionEx")
                ld = (LayoutDescriptionEx) layout;
            else
                MessageBox.Show("Failed to load layout, unknown extension", "Layour Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            ld.RecountStars();
            if (ld.darkStar == null) ld.GenerateDarkStar();
            if (ld.redOutline == null) ld.GenerateOutline();
            ld.Trim();
        }

        private void LoadDefaultLayoutNoInvalidate()
        {
            ld = LayoutDescriptionEx.GenerateDefault();
            ld.Trim();
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
            throw new NotImplementedException();
            //byte[] data = File.ReadAllBytes(name);
            //ld = LayoutDescriptionEx.DeserializeExternal(data, mm.GetImage());
            //InvalidateCache();
        }

        public void SaveExternal(string name)
        {
            throw new NotImplementedException();
            //byte[] data = ld.SerializeExternal();
            //File.WriteAllBytes(name, data);
        }

        private Exception LoadLayoutExc(string path)
        {
            Exception ret = null;

            try
            {
                LoadLayout(path);
            }
            catch (Exception e)
            {
                ret = e;
            }

            return ret;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                do
                {
                    LoadLayout("layout/" + rm.GetROMName() + ".sml");
                } while (false);
            }
            catch (IOException){
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
            ld = LayoutDescriptionEx.GenerateDefault();
            InvalidateCache();
        }

        private void loadFromToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Star Manager Layout (*.sml)|*.sml|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = false,
                InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\layout"
            };

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
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Star Manager Layout (*.sml)|*.sml|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = false,
                InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\layout"
            };

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

        private void importStarMasksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "ROM Files (*.z64)|*.z64",
                FilterIndex = 1,
                RestoreDirectory = true
            };

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
            ld.GenerateDarkStar();
            InvalidateCache();
        }

        private void resetHighlightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gm.IsFirstCall = true;
            InvalidateCache();
        }

        private void loadCustomFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Font file (*.ttf)|*.ttf",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    PrivateFontCollection collection = new PrivateFontCollection();
                    collection.AddFontFile(openFileDialog.FileName);
                    FontFamily[] fontFamilies = collection.Families;
                    FontFamily fontFamily = fontFamilies.First();
                    if (fontFamily == null) return;

                    gm.Collection = collection;
                    gm.FontFamily = fontFamily;
                    gm.FontName = fontFamily.Name;
                }
                catch (IOException)
                {
                    MessageBox.Show("Failed to load layout!", "Layour Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            InvalidateCache();
        }

        private void loadROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "ROM Files (*.z64)|*.z64",
                FilterIndex = 1,
                RestoreDirectory = true
            };

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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "ROM Files (*.z64)|*.z64",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ROMManager rm = new ROMManager(openFileDialog.FileName);
                    if (rm == null) throw new IOException();
                    Bitmap image = rm.GetStarImage();
                    ld = new LayoutDescriptionEx(ld.courseDescription, ld.secretDescription, image, ld.starAmount, ld.starsShown);
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
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PNG Images (*.png)|*.png",
                FilterIndex = 1,
                RestoreDirectory = true
            };

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

            InvalidateCache();
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
            int counter = 0;
            foreach (ToolStripMenuItem item in fileMenuItems)
            {
                if (item == sender)
                {
                    if (mm != null)
                        mm.SelectedFile = counter;
                    item.Checked = true;
                }
                else
                {
                    item.Checked = false;
                }
                counter++;
            }
            InvalidateCache();
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
            warpToLevelToolStripMenuItem.Checked = false;

            if (configureLayoutToolStripMenuItem.Checked)
            {
                ld.PrepareForEdit();
            }
            else
            {
                ld.Trim();
            }
            gm.InvalidateCache();
        }

        private void editFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            configureLayoutToolStripMenuItem.Checked = false;
            warpToLevelToolStripMenuItem.Checked = false;
        }

        private void editFlagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FlagsEditForm fef = new FlagsEditForm(mm.GetStars());
            fef.ShowDialog();
            mm.WriteToFile(fef.stars);
            InvalidateCache();
        }

        private void editComponentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActionMaskForm amf = new ActionMaskForm(componentsClasses, sm);
            amf.ShowDialog();
            InvalidateCache();
        }

        private void LoadSettings()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("settings.cfg", FileMode.Open, FileAccess.Read, FileShare.None);
            sm = (SettingsManager)formatter.Deserialize(stream);
            stream.Close();
        }

        private void loadToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (!File.Exists("settings.cfg"))
            {
                sm = new SettingsManager();
                return;
            }

            try
            {
                LoadSettings();
                MessageBox.Show("Settings loaded successfully!", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception)
            {
                MessageBox.Show("Your settings seems to be unavailable or corrupted, loading defaults", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                sm = new SettingsManager();
            }

            InvalidateCache();
        }

        private void loadDefaultToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            sm = new SettingsManager();
            InvalidateCache();
            MessageBox.Show("Defaults loaded successfully!", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        void SaveSettings()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("settings.cfg", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, sm);
            stream.Close();
        }

        private void saveToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                SaveSettings();
                MessageBox.Show("Settings saved successfully!", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception)
            {
                MessageBox.Show("Can't save settings!", "Settings Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void warpToLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editFileToolStripMenuItem.Checked = false;
            configureLayoutToolStripMenuItem.Checked = false;

            wd = new WarpDialog(mm.GetCurrentLevel());
            wd.ShowDialog();
            mm.WriteWarp(wd.warp, wd.level, wd.area);
        }

        private void killPJ64ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mm != null)
            {
                mm.KillProcess();
            }
        }

        private void MainWindow_Resize(object sender, EventArgs e)
        {
            starPicture.Width = this.Width;
            gm.Width = Width;
            gm.InvalidateCache();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string name = "layout/" + rm.GetROMName() + ".sml";
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
