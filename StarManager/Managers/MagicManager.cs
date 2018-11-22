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
        public readonly int romPtrBase;
        public readonly int ramPtrBase;

        public MagicManager(Process process, int[] romPtrBaseSuggestions, int[] ramPtrBaseSuggestions, int offset)
        {
            this.process = process;

            bool isRomFound = false;
            bool isRamFound = false;

            foreach(int romPtrBaseSuggestion in romPtrBaseSuggestions)
            {
                romPtrBase = romPtrBaseSuggestion;
                if (IsRomBaseValid())
                {
                    isRomFound = true;
                    break;
                }
            }

            foreach (int ramPtrBaseSuggestion in ramPtrBaseSuggestions)
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
                bool readSuccess = process.ReadValue(addr, out value);

                if (readSuccess)
                {
                    if (!isRamFound && value == ramMagic)
                    {
                        ramPtrBase = addr.ToInt32();
                        isRamFound = true;
                    }

                    if (!isRomFound && value == romMagic)
                    {
                        romPtrBase = addr.ToInt32();
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
