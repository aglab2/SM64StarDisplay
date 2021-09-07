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
using Newtonsoft.Json;

namespace StarDisplay
{
    public partial class MainWindow : Form
    {
        LayoutDescriptionEx ld;

        GraphicsManager gm;
        MemoryManager mm;
        ROMManager rm;
        UpdateManager um;
        DownloadManager dm;
        SettingsManager sm;
        SyncLoginForm slf;

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

        const int scanPeriod = 1000;
        const int updatePeriod = 30;

        byte[] otherStars = new byte[MemoryManager.FileLength];
        
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

            DrawIntro(); 
            ResetForm();

            magicThread = new Thread(doMagicThread);
            magicThread.Start();

            timer = new System.Threading.Timer(UpdateStars, null, 1, Timeout.Infinite);
            
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
            bool isActive = !mm.ProcessActive();
            while (mm != null && isActive && !mm.isMagicDone())
            {
                try
                {
                    mm.doMagic();
                }
                catch (Exception) { }
            }
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

        static string[] processNames = { 
            "project64", "project64d",
            "mupen64-rerecording",
            "mupen64-pucrash",
            "mupen64_lua",
            "mupen64-wiivc",
            "mupen64-RTZ",
            "mupen64-rerecording-v2-reset",
            "mupen64-rrv8-avisplit",
            "mupen64-rerecording-v2-reset",
            // "retroarch" 
        };

        private Process FindEmulatorProcess()
        {
            foreach (string name in processNames)
            {
                Process process = Process.GetProcessesByName(name).Where(p => !p.HasExited).FirstOrDefault();
                if (process != null)
                    return process;
            }

            return null;
        }

        private void ConnectToProcess()
        {
            Process process = FindEmulatorProcess();
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
                            DialogResult result = MessageBox.Show(String.Format("Update for Star Display available!\n\n{0}\nDo you want to download it now? Press cancel to skip update", um.UpdateName()), "Update",
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk);
                            if (result == DialogResult.Yes)
                            {
                                Process.Start(um.DownloadPath());
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
                //if (slf != null && slf.sm != null && slf.sm.isServer)
                //    goto bla;

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
 
                bool mmIsInvalidated = mm == null ? false : mm.CheckInvalidated();
                bool gmIsInvalidated = gm == null ? false : gm.CheckInvalidated();
                bool dmIsInvalidated = dm == null ? false : dm.CheckInvalidated();
                bool smIsInvalidated = false;
                if (slf != null && slf.sm != null)
                    smIsInvalidated = slf.sm.CheckInvalidated();

                bool nmIsInvalidated = false;
                if (slf != null && slf.nm != null)
                    nmIsInvalidated = slf.nm.CheckInvalidated();

                if (smIsInvalidated && slf.sm.dropFile)
                {
                    slf.sm.dropFile = false;
                    mm.Stars = new byte[MemoryManager.FileLength];
                    mm.WriteToFile(ld.starsShown);
                    mm.isStarsInvalidated = true;
                }

                if (mmIsInvalidated && mm.isStarsInvalidated)
                {
                    mm.isStarsInvalidated = false;
                    if (slf != null && slf.sm != null)
                    {
                        slf.sm.SendData(mm.Stars);
                    }

                    Console.WriteLine("MM Invalidated!");
                }

                if (gmIsInvalidated)
                    Console.WriteLine("GM Invalidated!");
                if (dmIsInvalidated)
                    Console.WriteLine("DM Invalidated!");

                if (smIsInvalidated)
                {
                    Console.WriteLine("SM Invalidated!");
                    {
                        byte[] stars = slf.sm.AcquiredData;

                        bool shouldSendHelp = false;
                        for (int i = 0; i < stars.Count(); i++)
                        {
                            byte diff = (byte)(mm.Stars[i] ^ stars[i]);
                            if ((mm.Stars[i] & diff) != 0)
                                shouldSendHelp = true;

                            otherStars[i] |= (byte)(diff & stars[i]);
                            mm.Stars[i] = (byte)(mm.Stars[i] | stars[i]);
                        }

                        if (shouldSendHelp)
                        {
                            slf.sm.SendData(mm.Stars);
                        }

                        mm.WriteToFile(ld.starsShown);
                    }
                    if (!mm.IsDecomp)
                    {
                        var location = mm.GetLocation();
                        var netData = slf.sm.getNetData();
                        foreach (var item in netData)
                        {
                            var player = item.Key;
#if DEBUG == false
                            if (player != slf.GetPlayerName())
#endif
                            {
                                var data = item.Value;

                                if (slf is object && slf.nm is object)
                                {
                                    var id = slf.nm.RegisterPlayer(player);
                                    if (data.location == location)
                                        mm.WriteNetState(id, data.state);
                                    else
                                        mm.WriteNetState(id, null);
                                }
                            }
                        }
                    }
                }

                if (nmIsInvalidated)
                {
                    if (!mm.IsDecomp)
                    {
                        mm.WriteNetPatch();
                        var state = mm.GetMarioState();
                        if (slf.sm is object)
                        {
                            slf.sm.SendNet64Data(slf.GetNet64Name(), state, mm.GetLocation());
                        }

                        if (slf.nm.mustReload)
                        {
                            for (int i = 0; i < 16; i++)
                            {
                                mm.WriteNetState(i, null);
                            }
                            slf.nm.mustReload = false;
                        }

                        this.SafeInvoke((MethodInvoker)delegate { slf.UpdatePlayers(slf.nm.GetPlayers()); });
                    }
                }

                // We do not draw anything!
                if (!mmIsInvalidated && !gmIsInvalidated && !smIsInvalidated && !dmIsInvalidated)
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
                if (currentCRC != oldCRC || rm == null || dmIsInvalidated)
                {
                    oldCRC = currentCRC;
                    try
                    {
                        rm = new ROMManager(mm.GetROM());
                        try
                        {
                            if (dm != null)
                            {
                                dm.GetData();
                            }

                            string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                            LoadLayoutNoInvalidate(exePath + "\\layout\\" + rm.GetROMName() + ".jsml", false);
                        }
                        catch (IOException)
                        {
                            try
                            {
                                dm = new DownloadManager(rm.GetROMName() + ".jsml");
                            }
                            catch(Exception) { }
                            LoadDefaultLayoutNoInvalidate();
                        }

                        gm.IsFirstCall = true;
                    }
                    catch (IndexOutOfRangeException) //can be generated by box reader
                    { }
                    catch (Exception)
                    { oldCRC = 0; }

                    InvalidateCache();
                }

                int lineOffset = 0;
                var actions = sm.GetConfig(MainWindowsSettingsAction.collectablesOnlyConfigureName, false) ?
                    mm.GetCollectablesOnlyDrawActions(ld, rm) : 
                    mm.GetDrawActions(ld, rm, enableLockoutToolStripMenuItem.Checked ? otherStars : null);

                if (actions == null) return;

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
            catch (ObjectDisposedException)
            {
            }
            finally
            {
                int period = (isEmulatorStarted && isHackLoaded && isOffsetsFound) ? updatePeriod : scanPeriod;
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
            if (mm.IsDecomp)
            {
                MessageBox.Show("Cannot extract assets from decomp ROM...", "Decomp Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Bitmap image = mm.GetImage();
            ld = new LayoutDescriptionEx(ld.courseDescription, ld.secretDescription, image, ld.starAmount, ld.starsShown);
            InvalidateCache();
        }

        string prevBackgroundPath = "";
        private void InvalidateCache()
        {
            Console.WriteLine("invalidated");
            gm.Ld = ld;
            mm.InvalidateCache();
            gm.InvalidateCache();

            if (sm.GetConfig(BackgroundSettingsAction.configureName, false))
            {

                string backgroundPath = sm.GetConfig(BackgroundSettingsAction.pathConfigureName, "");
                if (prevBackgroundPath != backgroundPath)
                {
                    prevBackgroundPath = backgroundPath;
                    try
                    {
                        Image img = Image.FromFile(sm.GetConfig(BackgroundSettingsAction.pathConfigureName, ""));
                        if (img == null)
                            throw new Exception();

                        gm.SetBackground(img, 0.5f);
                    }
                    catch (Exception)
                    {
                        sm.SetConfig(BackgroundSettingsAction.configureName, false);
                        prevBackgroundPath = "";
                    }
                }
            }
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
            if (mm.IsDecomp)
            {
                MessageBox.Show("No cheating on decomp ROMs allowed FUNgineer", "Decomp Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

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
                    mm.WriteWarp(wd is object ? wd.warp : (byte) 0xa, (byte) LevelInfo.FindByEEPOffset(sld.offset).Level, wd is object ? wd.area : (byte) 1);

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
                                    mm.WriteToFile(sld.offset, i, ld.starsShown);
                                }
                            }
                            InvalidateCache();
                        }
                    }
                    else
                    {
                        if ((sld.starMask & (byte)(1 << (star - 1))) != 0)
                        {
                            mm.WriteToFile(sld.offset, star - 1, ld.starsShown);
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

                if (enableClickToWarpToolStripMenuItem.Checked)
                    EditWarps();
            }
        }

        private void starPicture_MouseMove(object sender, MouseEventArgs e)
        {
            picX = e.X; picY = e.Y;
        }

        private void LoadLayout(string name, bool showFail)
        {
            LoadLayoutNoInvalidate(name, showFail);
            InvalidateCache();
        }

        private void LoadLayoutNoInvalidate(string name, bool showFail)
        {
            int TotalWidth = this.Width / ld.starsShown;

            try
            {
                string ext = Path.GetExtension(name);
                if (ext == ".sml")
                {
                    Stream stream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read);
                    IFormatter formatter = new BinaryFormatter();
                    object layout = formatter.Deserialize(stream);
                    string layoutClassName = layout.GetType().Name;

                    if (layoutClassName == "LayoutDescriptionEx")
                        ld = (LayoutDescriptionEx)layout;
                    else
                        MessageBox.Show("Failed to load layout, unknown extension", "Layour Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    var json = File.ReadAllText(name);
                    ld = JsonConvert.DeserializeObject<LayoutDescriptionEx>(json);
                }
            }
            catch(Exception e)
            {
                if (showFail)
                    MessageBox.Show($"Failed to load the layout {name}!");
                else
                    throw e;

                return;
            }

            ld.RecountStars();

            if (ld.darkStar == null) ld.GenerateDarkStar();
            if (ld.redOutline == null) ld.GenerateOutline();
            if (ld.invertedStar == null) ld.GenerateInvertedStar();
            ld.Trim();

            this.SafeInvoke((MethodInvoker)delegate { this.Width = TotalWidth * ld.starsShown; });
        }

        private void LoadDefaultLayoutNoInvalidate()
        {
            ld = LayoutDescriptionEx.GenerateDefault();
            ld.Trim();
        }
        
        private void SaveLayout(string name)
        {
            ld.Trim();

            var redOutline = ld.redOutline;
            var greenOutline = ld.greenOutline;
            var invertedStar = ld.invertedStar;
            ld.invertedStar = null;
            ld.greenOutline = null;
            ld.redOutline = null;
            var json = JsonConvert.SerializeObject(ld);
            ld.invertedStar = invertedStar;
            ld.greenOutline = greenOutline;
            ld.redOutline = redOutline;

            File.WriteAllText(name, json);
        }
        
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if XD
            foreach (var layoutPath in Directory.EnumerateFiles("C:\\Data\\layouts"))
            {
                var ext = Path.GetExtension(layoutPath);
                if (ext != ".sml")
                    continue;

                var jsmlPath = Path.ChangeExtension(layoutPath, ".jsml");
                if (File.Exists(jsmlPath))
                    continue;

                LoadLayout(layoutPath);
                SaveLayout(jsmlPath);
            }
#endif

            try
            {
                do
                {
                    string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    LoadLayout(exePath + "\\layout\\" + rm.GetROMName() + ".jsml", true);
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
                Filter = "Star Manager Layout (*.sml,*.jsml,*.txt)|*.sml;*.jsml;*.txt|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = false,
                InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\layout"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LoadLayout(openFileDialog.FileName, true);
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
                Filter = "Star Manager Layout (*.jsml)|*.jsml|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = false,
                InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath) + "\\layout"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
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
            if (mm.IsDecomp)
            {
                MessageBox.Show("Cannot parse decomp ROMs", "Decomp Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
            ld.GenerateInvertedStar();
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
            if (mm.IsDecomp)
            {
                MessageBox.Show("Cannot import assets from decomp ROMs", "Decomp Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
            enableClickToWarpToolStripMenuItem.Checked = false;

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
            enableClickToWarpToolStripMenuItem.Checked = false;
        }

        private void editFlagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FlagsEditForm fef = new FlagsEditForm(mm.GetStars());
            fef.ShowDialog();
            mm.WriteToFile(fef.stars, ld.starsShown);
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
            string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Stream stream = new FileStream(exePath + "\\settings.cfg", FileMode.Open, FileAccess.Read, FileShare.None);
            sm = (SettingsManager)formatter.Deserialize(stream);
            stream.Close();
        }

        private void loadToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (!File.Exists(exePath + "\\settings.cfg"))
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
            string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(exePath + "\\settings.cfg", FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
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
            if (mm.IsDecomp)
            {
                MessageBox.Show("No cheating on decomp ROMs allowed FUNgineer", "Decomp Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            wd = new WarpDialog(mm.GetCurrentLevel());
            DialogResult r = wd.ShowDialog();
            if (r == DialogResult.OK)
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
            Console.WriteLine(this.Width);

            if (starPicture != null)
                starPicture.Width = this.Width;

            if (gm != null)
            {
                gm.Width = Width;
                gm.InvalidateCache();
            }
		}
	
        private void syncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (slf == null || slf.isClosed)
            {
                slf = new SyncLoginForm();
                slf.Show();
            }
        }

        private void useEmptyStarsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ld.useEmptyStars = !ld.useEmptyStars;
            ld.GenerateDarkStar();
            gm.InvalidateCache();
        }

        private void clearOtherPlayerScoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            otherStars = new byte[MemoryManager.FileLength];
        }

        private void enableClickToWarpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editFileToolStripMenuItem.Checked = false;
            configureLayoutToolStripMenuItem.Checked = false;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string layoutDir = exePath + "\\layout\\";
                Directory.CreateDirectory(layoutDir);
                string name = layoutDir + rm.GetROMName() + ".jsml";
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
