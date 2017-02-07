using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StarDisplay.HandlesInfo;
using LiveSplit.ComponentUtil;
using System.IO;

namespace StarDisplay
{
    public class LineEntry
    {
        public int line;
        public byte starByte;
        public int starDiff;
        public bool isSecret;
        public byte starMask;

        public LineEntry(int line, byte starsByte, int starsDiff, bool isSecret, byte starMask)
        {
            this.line = line;
            this.starByte = starsByte;
            this.starDiff = starsDiff;
            this.isSecret = isSecret;
            this.starMask = starMask;
        }
    }

    public class MemoryManager : IEnumerable<LineEntry> //course, starsByte, diff, isSecret
    {
        public readonly Process process;
        LayoutDescription ld;

        int prevTime;
        byte[] oldStars;

        DeepPointer igt;
        DeepPointer[] files;

        DeepPointer romNamePtr;
        DeepPointer levelPtr;

        private int[] courseLevels = { 0, 9, 24, 12, 5, 4, 7, 22, 8, 23, 10, 11, 36, 13, 14, 15 };
        private int[] secretLevels = { 0, 17, 19, 21, 27, 28, 29, 18, 31, 20, 25 };
        private int[] overworldLevels = { 6, 26, 16 };

        public int selectedFile;

        public MemoryManager(Process process, LayoutDescription ld)
        {
            this.process = process;
            this.ld = ld;
            oldStars = new byte[32];

            igt = new DeepPointer("Project64.exe", 0xD6A1C, 0x32D580);
            files = new DeepPointer[4];
            files[0] = new DeepPointer("Project64.exe", 0xD6A1C, 0x207708);
            files[1] = new DeepPointer("Project64.exe", 0xD6A1C, 0x207778);
            files[2] = new DeepPointer("Project64.exe", 0xD6A1C, 0x2077E8);
            files[3] = new DeepPointer("Project64.exe", 0xD6A1C, 0x207858);

            romNamePtr = new DeepPointer("Project64.exe", 0xAF1F8);
            levelPtr = new DeepPointer("Project64.exe", 0xD6A1C, 0x32DDFA);
        }

        public bool isProcessActive()
        {
            return process == null || process.HasExited;
        }

        public void deleteStars()
        {
            int curTime = igt.Deref<int>(process);
            if (curTime > 200 || curTime < 60) return;
            
            prevTime = curTime;
            byte[] data = Enumerable.Repeat((byte)0x00, 0x70).ToArray();
            IntPtr ptr;

            DeepPointer file = files[selectedFile];
            if (!file.DerefOffsets(process, out ptr))
            {
                Console.WriteLine("deref fail");
            }
            if (!process.WriteBytes(ptr, data))
            {
                throw new IOException();
            }
        }

        public string getROMName()
        {
            return romNamePtr.DerefString(process, 32);
        }

        private int getOffset()
        {
            int level = levelPtr.Deref<byte>(process);
            if (level == 0) return -1;
            int courseLevel = Array.FindIndex(courseLevels, lvl => lvl == level);
            if (courseLevel != -1) return courseLevel + 3;
            int secretLevel = Array.FindIndex(secretLevels, lvl => lvl == level);
            if (secretLevel != -1) return secretLevel + 18;
            int owLevel = Array.FindIndex(overworldLevels, lvl => lvl == level);
            if (owLevel != -1) return 0;
            return -2;
        }

        public LineEntry getLine()
        {
            int offset = getOffset();
            
            int courseIndex = Array.FindIndex(ld.courseDescription, lind => !lind.isTextOnly && lind.offset == offset);
            if (courseIndex != -1) return new LineEntry(courseIndex, 0, 0, false, 0);
            int secretIndex = Array.FindIndex(ld.secretDescription, lind => !lind.isTextOnly && lind.offset == offset);
            if (secretIndex != -1) return new LineEntry(secretIndex, 0, 0, true, 0);

            return null;
        }

        private int countStars(byte stars)
        {
            int answer = 0;
            for (int i = 1; i <= 7; i++)
                answer += ((stars & (1 << (i - 1))) == 0) ? 0 : 1;
            return answer;
        }

        public IEnumerator<LineEntry> GetEnumerator()
        {
            int length = 32;

            DeepPointer file = files[selectedFile];
            byte[] stars = file.DerefBytes(process, length);
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

        public void invalidateCache()
        {
            oldStars = new byte[32];
        }
    }
}
