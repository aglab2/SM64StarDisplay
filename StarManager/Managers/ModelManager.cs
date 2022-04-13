using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarDisplay
{
    class ModelManager
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

        bool isUpdateRequested = false;


        bool isEmulatorStarted = false;
        bool isHackLoaded = false;
        bool isOffsetsFound = false;

        Thread magicThread;

        const int scanPeriod = 1000;
        const int updatePeriod = 30;

        byte[] otherStars = new byte[MemoryManager.FileLength];
        MainWindow form;

        public ModelManager(MainWindow form)
        {
            this.form = form;
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
                        form.SafeInvoke((MethodInvoker)delegate
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
                            catch (Exception) { }
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
                    starPicture.Height = (int)(lineOffset * gm.SHeight) + 10;
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

    }
}
