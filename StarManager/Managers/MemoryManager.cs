using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace StarDisplay
{
    public class MemoryManager : CachedManager
    {
        private const int FileLength = 0x70;

        Process Process;
        MagicManager mm;

        private int previousTime;
        private byte[] oldStars;
        
        IntPtr igtPtr; int igt;
        IntPtr[] filesPtr; IntPtr filePtr; byte[] stars;

        IntPtr levelPtr; byte level;
        IntPtr areaPtr; byte area;
        IntPtr redsPtr; sbyte reds;

        int restSecrets;
        int activePanels;

        IntPtr selectedStarPtr; byte selectedStar;

        IntPtr romCRCPtr; UInt16 romCRC;

        IntPtr romPtr; // should be read on demand
        IntPtr starImagePtr; // should be read on user demand 
        
        IntPtr spawnPointPtr; //byte
        IntPtr hpPtr; //short
        IntPtr menuModifierPtr; //short
        IntPtr spawnStatusPtr; //byte
        IntPtr igtigtPtr; //short
        IntPtr levelSpawnPtr; //byte

        private int selectedFile;

        public int SelectedFile { get => selectedFile; set { if (selectedFile != value) isInvalidated = true; selectedFile = value; } }
        public int Igt { get => igt; set => igt = value; }
        public byte[] Stars { get => stars; set { if (stars == null || value == null || !stars.SequenceEqual(value)) isInvalidated = true; stars = value; } }
        public byte Level { get => level; set { if (level != value) isInvalidated = true; level = value; } }
        public byte Area { get => area; set { if (area != value) isInvalidated = true; area = value; } }
        public sbyte Reds { get => reds; set { if (reds != value) isInvalidated = true; reds = value; } }
        public byte SelectedStar { get => selectedStar; set { if (selectedStar != value) isInvalidated = true; selectedStar = value; } }
        public ushort RomCRC { get => romCRC; set { if (romCRC != value) isInvalidated = true; romCRC = value; } }
        public int RestSecrets { get => restSecrets; set { if (restSecrets != value) isInvalidated = true; restSecrets = value; } }
        public int ActivePanels { get => activePanels; set { if (activePanels != value) isInvalidated = true; activePanels = value; } }

        public MemoryManager(Process process)
        {
            this.Process = process;
            oldStars = new byte[FileLength];
        }

        public bool ProcessActive()
        {
            return Process == null || Process.HasExited;
        }

        public bool isReadyToRead()
        {
            // We can read mem now, let's read Igt and check if it is big enough
            int igt = Process.ReadValue<int>(igtPtr);
            if (igt < 30)
                isInvalidated = true;

            return (igt > 30);
        }

        public bool isMagicDone()
        {
            return mm != null && mm.isValid();
        }

        public void doMagic()
        {
            List<int> romPtrBaseSuggestions = new List<int>();
            List<int> ramPtrBaseSuggestions = new List<int>();

            DeepPointer[] ramPtrBaseSuggestionsDPtrs = { new DeepPointer("Project64.exe", 0xD6A1C),     //1.6
                    new DeepPointer("RSP 1.7.dll", 0x4C054), new DeepPointer("RSP 1.7.dll", 0x44B5C)        //2.3.2; 2.4
                };

            DeepPointer[] romPtrBaseSuggestionsDPtrs = { new DeepPointer("Project64.exe", 0xD6A2C),     //1.6
                    new DeepPointer("RSP 1.7.dll", 0x4C050), new DeepPointer("RSP 1.7.dll", 0x44B58)        //2.3.2; 2.4
                };

            // Time to generate some addesses for magic check
            foreach (DeepPointer romSuggestionPtr in romPtrBaseSuggestionsDPtrs)
            {
                int ptr = -1;
                try
                {
                    ptr = romSuggestionPtr.Deref<int>(Process);
                    romPtrBaseSuggestions.Add(ptr);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            foreach (DeepPointer ramSuggestionPtr in ramPtrBaseSuggestionsDPtrs)
            {
                int ptr = -1;
                try
                {
                    ptr = ramSuggestionPtr.Deref<int>(Process);
                    ramPtrBaseSuggestions.Add(ptr);
                }
                catch (Exception)
                {
                    continue;
                }
            }

            mm = new MagicManager(Process, romPtrBaseSuggestions.ToArray(), ramPtrBaseSuggestions.ToArray());

            igtPtr = new IntPtr(mm.ramPtrBase + 0x32D580);
            filesPtr = new IntPtr[4];
            filesPtr[0] = new IntPtr(mm.ramPtrBase + 0x207700);
            filesPtr[1] = new IntPtr(mm.ramPtrBase + 0x207770);
            filesPtr[2] = new IntPtr(mm.ramPtrBase + 0x2077E0);
            filesPtr[3] = new IntPtr(mm.ramPtrBase + 0x207850);

            levelPtr = new IntPtr(mm.ramPtrBase + 0x32DDFA);
            areaPtr = new IntPtr(mm.ramPtrBase + 0x33B249);
            starImagePtr = new IntPtr(mm.ramPtrBase + 0x064F80 + 0x04800);
            redsPtr = new IntPtr(mm.ramPtrBase + 0x3613FD);

            selectedStarPtr = new IntPtr(mm.ramPtrBase + 0x1A81A3);

            romPtr = new IntPtr(mm.romPtrBase + 0);
            romCRCPtr = new IntPtr(mm.romPtrBase + 0x10);

            spawnPointPtr = new IntPtr(mm.ramPtrBase + 0x33B248);
            hpPtr = new IntPtr(mm.ramPtrBase + 0x33B21C);
            menuModifierPtr = new IntPtr(mm.ramPtrBase + 0x33B23A);
            spawnStatusPtr = new IntPtr(mm.ramPtrBase + 0x33B24B);
            igtigtPtr = new IntPtr(mm.ramPtrBase + 0x33B26A);
            levelSpawnPtr = new IntPtr(mm.ramPtrBase + 0x33B24A);
        }

        public void PerformRead()
        {
            Igt = Process.ReadValue<int>(igtPtr);
            
            filePtr = filesPtr[SelectedFile];
            byte[] stars = Process.ReadBytes(filePtr, FileLength);
            for (int i = 0; i < FileLength; i += 4)
                Array.Reverse(stars, i, 4);
            Stars = stars;

            Level = Process.ReadValue<byte>(levelPtr);
            Area = Process.ReadValue<byte>(areaPtr);
            Reds = Process.ReadValue<sbyte>(redsPtr);
            
            RestSecrets = GetSecrets();
            ActivePanels = GetActivePanels();
    
            SelectedStar = Process.ReadValue<byte>(selectedStarPtr);

            RomCRC = Process.ReadValue<UInt16>(romCRCPtr);
        }

        public void DeleteStars()
        {
            if (Igt > 200 || Igt < 60) return;

            previousTime = Igt;
            byte[] data = Enumerable.Repeat((byte)0x00, FileLength).ToArray();
            IntPtr file = filesPtr[SelectedFile];
            if (!Process.WriteBytes(file, data))
            {
                throw new IOException();
            }
        }

        public byte GetCurrentStar()
        {
            return SelectedStar;
        }

        public byte GetCurrentArea()
        {
            return Area;
        }

        public byte GetCurrentLevel()
        {
            return Level;
        }

        private int GetCurrentOffset()
        {
            LevelOffsetsDescription lod = LevelInfo.FindByLevel(Level);
            if (lod == null)
                return -1;

            return lod.EEPOffset;
        }

        public TextHighlightAction GetCurrentLineAction(LayoutDescriptionEx ld)
        {
            int offset = GetCurrentOffset();

            int courseIndex = ld.courseDescription.FindIndex(lind => (lind is StarsLineDescription sld) ? sld.offset == offset : false);
            if (courseIndex != -1)
            {
                StarsLineDescription sld = (StarsLineDescription)ld.courseDescription[courseIndex];
                return new TextHighlightAction(courseIndex, false, sld.text);
            }

            int secretIndex = ld.secretDescription.FindIndex(lind => (lind is StarsLineDescription sld) ? sld.offset == offset : false);
            if (secretIndex != -1)
            {
                StarsLineDescription sld = (StarsLineDescription)ld.secretDescription[secretIndex];
                return new TextHighlightAction(secretIndex, true, sld.text);
            }

            return null;
        }

        static public int countStars(byte stars)
        {
            int answer = 0;
            for (int i = 1; i <= 7; i++)
                answer += ((stars & (1 << (i - 1))) == 0) ? 0 : 1;
            return answer;
        }

        public sbyte GetReds()
        {
            return Reds;
        }

        public int GetSecrets()
        {
            return SearchObjects(0x800EF0B4);
        }

        public int GetActivePanels()
        {
            return SearchObjects(0x800EB770, 1) + SearchObjects(0x800EB770, 2); //1 - active, 2 - finalized
        }

        public int GetAllPanels()
        {
            return SearchObjects(0x800EB770);
        }

        public Bitmap GetImage()
        {
            byte[] data = Process.ReadBytes(starImagePtr, 512);

            for (int i = 0; i < 512; i += 4) //TODO: Better ending convert
            {
                byte[] copy = new byte[4];
                copy[0] = data[i + 0];
                copy[1] = data[i + 1];
                copy[2] = data[i + 2];
                copy[3] = data[i + 3];
                data[i + 0] = copy[3];
                data[i + 1] = copy[2];
                data[i + 2] = copy[1];
                data[i + 3] = copy[0];
            }
            return FromRGBA16(data);
        }

        public byte[] GetROM()
        {
            int[] romSizesMB = new int[] { 64, 48, 32, 24, 16, 8 };
            byte[] rom = null;
            int romSize = 0;
            foreach (int sizeMB in romSizesMB)
            {
                romSize = 1024 * 1024 * sizeMB;
                rom = Process.ReadBytes(romPtr, romSize);
                if (rom != null) break;
            }
            if (rom == null) return null;
            for (int i = 0; i < romSize; i += 4)
            {
                Array.Reverse(rom, i, 4);
            }
            return rom;
        }

        public UInt16 GetRomCRC()
        {
            return RomCRC;
        }

        public byte[] GetStars()
        {
            return Stars;
        }

        public static Bitmap FromRGBA16(byte[] data)
        {
            Bitmap picture = new Bitmap(16, 16);
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    int offset = (16 * j + i) * 2;
                    int colorARGB = (data[offset + 1] & 0x01) * 255 << 24
                        | (data[offset] & 0xF8) << 16 | (data[offset] & 0xE0) << 11
                        | (data[offset] & 0x07) << 13 | (data[offset] & 0x07) << 8
                        | (data[offset + 1] & 0xC0) << 5
                        | (data[offset + 1] & 0x3E) << 2 | (data[offset + 1] & 0x38) >> 3;

                    Color c = Color.FromArgb(colorARGB);
                    picture.SetPixel(i, j, c);
                }
            }
            return picture;
        }

        public DrawActions GetDrawActions(LayoutDescriptionEx ld, ROMManager rm)
        {
            int totalReds = 0, reds = 0;
            try
            {
                totalReds = rm != null ? rm.ParseReds(Level, GetCurrentStar(), GetCurrentArea()) : 0;
                reds = GetReds();
            }
            catch (Exception) { }
            if (totalReds != 0) //Fix reds amount -- intended total amount is 8, so we should switch maximum to totalReds
                reds += totalReds - 8;
            else //If we got any reds we might not be able to read total amount properly, so we set total amount to current reds to display only them
                totalReds = reds;


            //Operations are the same as with regular reds
            int totalSecrets = 0, secrets = 0;
            try
            {
                totalSecrets = rm != null ? rm.ParseSecrets(Level, GetCurrentStar(), GetCurrentArea()) : 0;
                secrets = totalSecrets - restSecrets;
            }
            catch (Exception) { }

            //Operations are the same as with regular reds
            int totalPanels = 0, activePanels = 0;
            try
            {
                totalPanels = rm != null ? rm.ParseFlipswitches(Level, GetCurrentStar(), GetCurrentArea()) : 0;
                activePanels = ActivePanels;
            }
            catch (Exception) { }

            DrawActions da = new DrawActions(ld, Stars, oldStars, reds, totalReds, secrets, totalSecrets, activePanels, totalPanels);
            oldStars = Stars;
            return da;
        }

        public DrawActions GetCollectablesOnlyDrawActions(LayoutDescriptionEx ld, ROMManager rm)
        {
            IntPtr file = filesPtr[SelectedFile];
            byte[] stars = Process.ReadBytes(file, FileLength);
            if (stars == null) return null;

            for (int i = 0; i < FileLength; i += 4)
                Array.Reverse(stars, i, 4);

            int totalReds = 0, reds = 0;
            try
            {
                totalReds = rm != null ? rm.ParseReds(level, GetCurrentStar(), GetCurrentArea()) : 0;
                reds = Reds;
            }
            catch (Exception) { }
            if (totalReds != 0) //Fix reds amount -- intended total amount is 8, so we should switch maximum to totalReds
                reds += totalReds - 8;
            else //If we got any reds we might not be able to read total amount properly, so we set total amount to current reds to display only them
                totalReds = reds;


            //Operations are the same as with regular reds
            int totalSecrets = 0, secrets = 0;
            try
            {
                totalSecrets = rm != null ? rm.ParseSecrets(level, GetCurrentStar(), GetCurrentArea()) : 0;
                secrets = totalSecrets - RestSecrets;
            }
            catch (Exception) { }

            //Operations are the same as with regular reds
            int totalPanels = 0, activePanels = 0;
            try
            {
                totalPanels = rm != null ? rm.ParseFlipswitches(level, GetCurrentStar(), GetCurrentArea()) : 0;
                activePanels = ActivePanels;
            }
            catch (Exception) { }

            DrawActions da = new CollectablesOnlyDrawActions(ld, stars, oldStars, reds, totalReds, secrets, totalSecrets, activePanels, totalPanels);
            oldStars = stars;
            return da;
        }

        public int SearchObjects(UInt32 searchBehaviour)
        {
            int count = 0;

            UInt32 address = 0x33D488;

           do
            {
                IntPtr currentObjectPtr = new IntPtr(mm.ramPtrBase + (int)address);
                byte[] data = Process.ReadBytes(currentObjectPtr, 0x260);

                UInt32 intparam = BitConverter.ToUInt32(data, 0x180);
                UInt32 behaviourActive1 = BitConverter.ToUInt32(data, 0x1CC);
                UInt32 behaviourActive2 = BitConverter.ToUInt32(data, 0x1D0);
                UInt32 initialBehaviour = BitConverter.ToUInt32(data, 0x20C);
                UInt32 scriptParameter = BitConverter.ToUInt32(data, 0x0F0);
                
                if (behaviourActive1 == searchBehaviour)
                {
                    count++;
                }

                address = BitConverter.ToUInt32(data, 0x8) & 0x7FFFFFFF;
            } while (address != 0x33D488 && address != 0);
            return count;
        }

        public int SearchObjects(UInt32 searchBehaviour, UInt32 state)
        {
            int count = 0;

            UInt32 address = 0x33D488;
            do
            {
                IntPtr currentObjectPtr = new IntPtr(mm.ramPtrBase + (int)address);
                byte[] data = Process.ReadBytes(currentObjectPtr, 0x260);

                UInt32 intparam = BitConverter.ToUInt32(data, 0x180);
                UInt32 behaviourActive1 = BitConverter.ToUInt32(data, 0x1CC);
                UInt32 behaviourActive2 = BitConverter.ToUInt32(data, 0x1D0);
                UInt32 initialBehaviour = BitConverter.ToUInt32(data, 0x20C);
                UInt32 scriptParameter = BitConverter.ToUInt32(data, 0x0F0);

                //Console.Write("{0:X8}({1:X8}) ", behaviourActive1, scriptParameter);

                if (behaviourActive1 == searchBehaviour && scriptParameter == state)
                {
                    count++;
                }

                address = BitConverter.ToUInt32(data, 0x8) & 0x7FFFFFFF;
            } while (address != 0x33D488 && address != 0);
            //Console.WriteLine();
            return count;
        }

        public override void InvalidateCache()
        {
            oldStars = new byte[FileLength];
            base.InvalidateCache();
        }

        public void WriteToFile(int offset, int bit)
        {
            byte[] stars = new byte[FileLength];
            Stars.CopyTo(stars, 0);

            //fix stuff here!!!
            stars[offset] = (byte) (stars[offset] ^ (byte)(1 << bit)); //???

            for (int i = 0; i < FileLength; i += 4)
                Array.Reverse(stars, i, 4);

            Process.WriteBytes(filePtr, stars);

            for (int i = 0; i < FileLength; i += 4)
                Array.Reverse(stars, i, 4);

            stars.CopyTo(Stars, 0);


            isInvalidated = true;
        }

        public void WriteToFile(byte[] data)
        {
            byte[] stars = data;
            if (stars == null) return;
            
            for (int i = 0; i < FileLength; i += 4)
                Array.Reverse(stars, i, 4);

            Process.WriteBytes(filePtr, stars);
        }

        public string GetTitle()
        {
            Process.Refresh();
            return Process.MainWindowTitle;
        }

        public void WriteWarp(byte warpID, byte levelID, byte areaID)
        {
            Process.WriteBytes(areaPtr, new byte[] { areaID });
            Process.WriteBytes(spawnPointPtr, new byte[] { warpID });
            Process.WriteBytes(levelSpawnPtr, new byte[] { levelID });
            Process.WriteBytes(hpPtr, new byte[] { 0x00, 0x08 });
            Process.WriteBytes(menuModifierPtr, new byte[] { 0x04, 0x00 });
            Process.WriteBytes(spawnStatusPtr, new byte[] { 0x02 });
            Process.WriteBytes(igtigtPtr, new byte[] { 0x00, 0x00 });
        }

        public void KillProcess()
        {
            Process.Kill();
        }
    }
}