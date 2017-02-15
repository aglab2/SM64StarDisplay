using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using LiveSplit.ComponentUtil;
using System.IO;
using System.Drawing;

namespace StarDisplay
{
    public class LineEntry
    {
        public int Line;
        public byte StarByte;
        public int StarDiff;
        public bool Secret;
        public byte StarMask;

        public LineEntry(int line, byte starByte, int starDiff, bool secret, byte starMask)
        {
            this.Line = line;
            this.StarByte = starByte;
            this.StarDiff = starDiff;
            this.Secret = secret;
            this.StarMask = starMask;
        }
    }

    public class MemoryManager : IEnumerable<LineEntry>
    {
        public readonly Process Process;
        LayoutDescription ld;
        GraphicsManager gm;

        int previousTime;
        byte[] oldStars;
        byte[] defPicture;

        DeepPointer igt;
        DeepPointer[] files;

        DeepPointer romNamePtr;
        DeepPointer levelPtr;
        DeepPointer starPtr;
        
        private int[] courseLevels = { 0, 9, 24, 12, 5, 4, 7, 22, 8, 23, 10, 11, 36, 13, 14, 15 };
        private int[] secretLevels = { 0, 17, 19, 21, 27, 28, 29, 18, 31, 20, 25 };
        private int[] overworldLevels = { 6, 26, 16 };

        public int selectedFile;

        public MemoryManager(Process process, LayoutDescription ld, GraphicsManager gm)
        {
            this.Process = process;
            this.ld = ld;
            this.gm = gm;
            oldStars = new byte[32];

            igt = new DeepPointer("Project64.exe", 0xD6A1C, 0x32D580);
            files = new DeepPointer[4];
            files[0] = new DeepPointer("Project64.exe", 0xD6A1C, 0x207708);
            files[1] = new DeepPointer("Project64.exe", 0xD6A1C, 0x207778);
            files[2] = new DeepPointer("Project64.exe", 0xD6A1C, 0x2077E8);
            files[3] = new DeepPointer("Project64.exe", 0xD6A1C, 0x207858);

            romNamePtr = new DeepPointer("Project64.exe", 0xAF1F8);
            levelPtr = new DeepPointer("Project64.exe", 0xD6A1C, 0x32DDFA);
            starPtr = new DeepPointer("Project64.exe", 0xD6A1C, 0x064F80 + 0x04800);

            defPicture = File.ReadAllBytes("images/star.rgba16");
        }

        public bool ProcessActive()
        {
            return Process == null || Process.HasExited;
        }

        public void DeleteStars()
        {
            int curTime = igt.Deref<int>(Process);
            if (curTime > 200 || curTime < 60) return;
            
            previousTime = curTime;
            byte[] data = Enumerable.Repeat((byte)0x00, 0x70).ToArray();
            IntPtr ptr;

            DeepPointer file = files[selectedFile];
            if (!file.DerefOffsets(Process, out ptr))
            {
                Console.WriteLine("deref fail");
            }
            if (!Process.WriteBytes(ptr, data))
            {
                throw new IOException();
            }
        }

        public string GetROMName()
        {
            return romNamePtr.DerefString(Process, 32);
        }

        private int GetOffset()
        {
            int level = levelPtr.Deref<byte>(Process);
            if (level == 0) return -1;
            int courseLevel = Array.FindIndex(courseLevels, lvl => lvl == level);
            if (courseLevel != -1) return courseLevel + 3;
            int secretLevel = Array.FindIndex(secretLevels, lvl => lvl == level);
            if (secretLevel != -1) return secretLevel + 18;
            int owLevel = Array.FindIndex(overworldLevels, lvl => lvl == level);
            if (owLevel != -1) return 0;
            return -2;
        }

        public LineEntry GetLine()
        {
            int offset = GetOffset();
            
            int courseIndex = Array.FindIndex(ld.courseDescription, lind => lind != null && !lind.isTextOnly && lind.offset == offset);
            if (courseIndex != -1) return new LineEntry(courseIndex, 0, 0, false, 0);
            int secretIndex = Array.FindIndex(ld.secretDescription, lind => lind != null && !lind.isTextOnly && lind.offset == offset);
            if (secretIndex != -1) return new LineEntry(secretIndex, 0, 0, true, 0);

            return null;
        }

        static public int countStars(byte stars)
        {
            int answer = 0;
            for (int i = 1; i <= 7; i++)
                answer += ((stars & (1 << (i - 1))) == 0) ? 0 : 1;
            return answer;
        }

        public void DrawSpecialString(int index, bool isAcquired)
        {
            LineDescription lind = ld.secretDescription[index];
            if (isAcquired)
            {
                gm.DrawGreenString(new LineEntry(index, 0, 0, true, 0), lind);
            }
            else
            {
                gm.DrawGrayString(new LineEntry(index, 0, 0, true, 0), lind);
            }
        }

        public IEnumerator<LineEntry> GetEnumerator()
        {
            int length = 32;

            DeepPointer file = files[selectedFile];
            byte[] stars = file.DerefBytes(Process, length);
            for (int i = 0; i < length; i += 4) //TODO: Better ending convert
            {
                byte[] copy = new byte[4];
                copy[0] = stars[i + 0];
                copy[1] = stars[i + 1];
                copy[2] = stars[i + 2];
                copy[3] = stars[i + 3];
                stars[i + 0] = copy[3];
                stars[i + 1] = copy[2];
                stars[i + 2] = copy[1];
                stars[i + 3] = copy[0];
            }

            /*DeepPointer badges = new DeepPointer("Project64.exe", file._base, file._offsets[1] - 2);
            byte[] badgeByte = badges.DerefBytes(process, 1);
            Console.WriteLine(string.Join(" ", badgeByte.Select(x => Convert.ToString(x, 2).PadLeft(8, '0'))));

            badgeByte[0] = 1 << 6;
            IntPtr ptr;
            if (!badges.DerefOffsets(process, out ptr))
            {
                Console.WriteLine("deref fail");
            }
            if (!process.WriteBytes(ptr, badgeByte))
            {
                throw new IOException();
            }*/

            if (stars[3] != oldStars[3])
            {
                int index; bool isAcquired;
                index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "B1");
                isAcquired = ((stars[3] & (1 << 4)) != 0) || ((stars[3] & (1 << 6)) != 0);
                if (index != -1)
                    DrawSpecialString(index, isAcquired);
                index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "B2");
                isAcquired = ((stars[3] & (1 << 5)) != 0) || ((stars[3] & (1 << 7)) != 0);
                if (index != -1)
                    DrawSpecialString(index, isAcquired);
                index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "WC");
                isAcquired = ((stars[3] & (1 << 1)) != 0);
                if (index != -1)
                    DrawSpecialString(index, isAcquired);
                index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "MC");
                isAcquired = ((stars[3] & (1 << 2)) != 0);
                if (index != -1)
                    DrawSpecialString(index, isAcquired);
                index = Array.FindIndex(ld.secretDescription, lind => lind != null && lind.text == "VC");
                isAcquired = ((stars[3] & (1 << 3)) != 0);
                if (index != -1)
                    DrawSpecialString(index, isAcquired);
            }

            for (int line = 0; line < ld.courseDescription.Length; line++)
            {
                var descr = ld.courseDescription[line];
                if (descr == null || descr.isTextOnly) continue;

                byte oldStarByte = oldStars[descr.offset];
                byte newStarByte = stars[descr.offset];
                byte starMask2 = (byte)(descr.starMask >> 1);

                if (oldStarByte != newStarByte)
                {
                    yield return new LineEntry(line, newStarByte, countStars((byte)(newStarByte & starMask2)) - countStars((byte)(oldStarByte & starMask2)), false, descr.starMask);
                }
            }

            for (int line = 0; line < ld.secretDescription.Length; line++)
            {
                var descr = ld.secretDescription[line];
                if (descr == null || descr.isTextOnly) continue;

                byte oldStarByte = oldStars[descr.offset];
                byte newStarByte = stars[descr.offset];
                byte starMask2 = (byte)(descr.starMask >> 1);

                if (oldStarByte != newStarByte)
                {
                    yield return new LineEntry(line, newStarByte, countStars((byte)(newStarByte & starMask2)) - countStars((byte)(oldStarByte & starMask2)), true, descr.starMask);
                }
            }
            oldStars = stars;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void InvalidateCache()
        {
            oldStars = new byte[32];
        }

        public Bitmap GetImage()
        {
            byte[] data = starPtr.DerefBytes(Process, 512);

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

        public Bitmap FromRGBA16(byte[] data)
        {
            Bitmap picture = new Bitmap(16, 16);
            for (int i = 0; i < 16; i++) {
                for (int j = 0; j < 16; j++) {
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
    }
}
