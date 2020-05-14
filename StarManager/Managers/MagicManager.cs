using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    class MagicManager
    {
        const int PageSize = 4096;
        const long MaxMem = (long) 4 * 1024 * 1024 * 1024;

        const uint ramMagic = 0x3C1A8032;
        const uint romMagic = 0x80371240;

        private Process process;
        public readonly uint romPtrBase;
        public readonly uint ramPtrBase;

        public MagicManager(Process process, int[] romPtrBaseSuggestions, int[] ramPtrBaseSuggestions, int offset)
        {
            this.process = process;

            bool isRomFound = false;
            bool isRamFound = false;

            foreach(uint romPtrBaseSuggestion in romPtrBaseSuggestions)
            {
                romPtrBase = romPtrBaseSuggestion;
                if (IsRomBaseValid())
                {
                    isRomFound = true;
                    break;
                }
            }

            foreach (uint ramPtrBaseSuggestion in ramPtrBaseSuggestions)
            {
                ramPtrBase = ramPtrBaseSuggestion;
                if (IsRamBaseValid())
                {
                    isRamFound = true;
                    break;
                }
            }

            for (IntPtr addr = new IntPtr(0x0); addr.ToInt64() < MaxMem; addr = IntPtr.Add(addr, PageSize))
            {
                if (isRomFound && isRamFound)
                    break;

                uint value = 0;
                bool readSuccess = process.ReadValue(IntPtr.Add(addr, offset), out value);

                if (readSuccess)
                {
                    if (!isRamFound && value == ramMagic)
                    {
                        ramPtrBase = (uint) (addr.ToInt64() + offset);
                        isRamFound = true;
                    }

                    if (!isRomFound && value == romMagic)
                    {
                        romPtrBase = (uint) (addr.ToInt64() + offset);
                        isRomFound = true;
                    }
                }
            }

            if (!isRomFound && !isRamFound)
                throw new ArgumentException("Failed to find rom and ram!");
        }

        bool IsRamBaseValid()
        {
            uint value = 0;
            bool readSuccess = process.ReadValue(new IntPtr(ramPtrBase), out value);
            return readSuccess && (value == ramMagic);
        }

        bool IsRomBaseValid()
        {
            uint value = 0;
            bool readSuccess = process.ReadValue(new IntPtr(romPtrBase), out value);
            return readSuccess && (value == romMagic);
        }

        public bool isValid()
        {
            return IsRamBaseValid() && IsRomBaseValid();
        }
    }
}
