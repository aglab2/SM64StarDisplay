using LiveSplit.ComponentUtil;
using MIPSInterpreter;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    class MagicManager
    {
        const uint ramMagic = 0x3C1A8000;
        const uint ramMagicMask = 0xfffff000;
        const uint romMagic = 0x80371240;

        private Process process;
        public readonly ulong romPtrBase;
        public readonly ulong ramPtrBase;

        public readonly bool isDecomp;
        public readonly int saveBufferOffset = 0;
        public readonly int saveBufferSize = 0;
        public readonly int saveFileSize = 0;
        public readonly byte[] verificationBytes = null;
        public readonly uint verificationOffset = 0;

        [DllImport("kernel32.dll")]
        static extern int VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION64
        {
            public ulong BaseAddress;
            public ulong AllocationBase;
            public int AllocationProtect;
            public int __alignment1;
            public ulong RegionSize;
            public int State;
            public int Protect;
            public int Type;
            public int __alignment2;
        }

        public enum AllocationProtect : uint
        {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
        }

        // This only works for emulators that support Parallel RDP because they want a very aligned RDRAM
        void ScanForRAM(ulong address, ulong size, ulong delim, ref bool isRamFound, ref ulong ramPtrBase)
        {
            ulong addressAlignedStart = (address + delim - 1) / delim * delim;
            ulong addressAlignedEnd   = (address + size) / delim * delim;

            for (ulong probe = addressAlignedStart; probe <= addressAlignedEnd; probe += delim)
            {
                bool readSuccess = process.ReadValue(new IntPtr((long)probe), out uint value);
                if (readSuccess)
                {
                    if (!isRamFound && ((value & ramMagicMask) == ramMagic))
                    {
                        ramPtrBase = probe;
                        isRamFound = true;
                    }
                }

                if (isRamFound)
                    break;
            }
        }

        public MagicManager(Process process, long[] romPtrBaseSuggestions, long[] ramPtrBaseSuggestions, int offset, bool isAres)
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

            ulong parallelStart = 0;
            ulong parallelEnd = 0;
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName.Contains("parallel_n64"))
                {
                    parallelStart = (ulong)module.BaseAddress;
                    parallelEnd = parallelStart + (ulong) module.ModuleMemorySize;
                }
            }

            ulong MaxAddress = process.Is64Bit() ? 0x800000000000U : 0xffffffffU;
            ulong address = 0;
            if (isAres && !isRomFound)
                FindAresRomBlock(ref isRomFound, ref romPtrBase);
            do
            {
                if (isRomFound && isRamFound)
                    break;

                MEMORY_BASIC_INFORMATION m;
                int result = VirtualQueryEx(process.Handle, new UIntPtr(address), out m, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
                if (address == (ulong)m.BaseAddress + (ulong)m.RegionSize || result == 0)
                    break;

                AllocationProtect prot = (AllocationProtect) (m.Protect & 0xff);
                if (prot == AllocationProtect.PAGE_EXECUTE_READWRITE
                 || prot == AllocationProtect.PAGE_EXECUTE_WRITECOPY
                 || prot == AllocationProtect.PAGE_READWRITE
                 || prot == AllocationProtect.PAGE_WRITECOPY
                 || prot == AllocationProtect.PAGE_READONLY)
                {
                    uint value;
                    bool readSuccess = process.ReadValue(new IntPtr((long) (address + (ulong) offset)), out value);
                    if (readSuccess)
                    {
                        if (!isRamFound && ((value & ramMagicMask) == ramMagic))
                        {
                            ramPtrBase = address + (ulong)offset;
                            isRamFound = true;
                        }

                        if (!isRomFound && value == romMagic)
                        {
                            romPtrBase = address + (ulong)offset;
                            isRomFound = true;
                        }
                    }

                    // Parallel: scan only large regions - we want to find g_rdram
                    ulong regionSize = (ulong)m.RegionSize;
                    if (parallelStart <= address && address <= parallelEnd && regionSize >= 0x800000)
                    {
                        ScanForRAM(address, (ulong) m.RegionSize, 0x1000, ref isRamFound, ref ramPtrBase);
                    }

                    if (parallelStart != 0 && regionSize >= 0x800000)
                    {
                        ScanForRAM(address, (ulong)m.RegionSize, 0x10000, ref isRamFound, ref ramPtrBase);
                    }

                    // Modern mupen allocates a gigantic array with very strict alignment
                    if (regionSize >= 0x20000000)
                    {
                        ScanForRAM(address, 0x20000, 0x1000, ref isRamFound, ref ramPtrBase);

                        if (isRamFound)
                        {
                            // rom is shifted from ram at exactly MM_CART_ROM=UINT32_C(0x10000000)
                            if (!isRomFound)
                            {
                                // Uncompressed allocation
                                romPtrBase = ramPtrBase + 0x10000000;
                                if (IsRomBaseValid())
                                {
                                    isRomFound = true;
                                }
                            }
                            if (!isRomFound)
                            {
                                // Compressed allocation
                                romPtrBase = ramPtrBase + 0x800000;
                                if (IsRomBaseValid())
                                {
                                    isRomFound = true;
                                }
                            }
                        }
                    }

                    if (isAres && !isRamFound)
                    {
                        // filter for 64k aligned 4mb/8mb (expansion pak) virtual memory sections
                        if ((regionSize >= 0x800000 && regionSize <= 0x811000) || (regionSize >= 0x400000 && regionSize <= 0x411000))
                        {
                            // align the address
                            ulong alignedAddress = (address & 0xFFFFFFFFFFFF0000);
                            // round up
                            if ((address & 0xFFFF) > 0)
                                alignedAddress += 0x10000;
                            uint alignedValue;
                            bool alignedReadSuccess = process.ReadValue(new IntPtr((long)(alignedAddress)), out alignedValue);
                            if (alignedReadSuccess)
                            {
                                if ((alignedValue & ramMagicMask) == ramMagic)
                                {
                                    ramPtrBase = alignedAddress;
                                    isRamFound = true;
                                }
                            }
                        }
                    }
                }

                address = (ulong)m.BaseAddress + (ulong)m.RegionSize;
            }
            while (address <= MaxAddress);

            if (!isRomFound || !isRamFound)
                throw new ArgumentException("Failed to find rom and ram!");

            uint[] mem;
            {
                byte[] bytes = process.ReadBytes(new IntPtr((long) ramPtrBase), 0x400000);
                int size = bytes.Count() / 4;
                mem = new uint[size];
                for (int idx = 0; idx < size; idx++)
                {
                    mem[idx] = BitConverter.ToUInt32(bytes, 4 * idx);
                }
            }

            DecompManager dm = new DecompManager(mem);
            if (!dm.gSaveBuffer.HasValue)
                throw new ArgumentException("Failed to gSaveBuffer!");

            saveBufferOffset = dm.gSaveBuffer.Value & 0xffffff;
            saveFileSize = dm.gSaveFileSize.Value;
            verificationBytes = dm.VerificationBytes;
            verificationOffset = dm.VerificationOffset.Value;

            isDecomp = saveBufferOffset != 0x207700; // TODO: This is inaccurate
        }

        // ares stores the rom twice, once that is Big Endian and unaligned, and a 2nd time that is the little endian and aligned to 64k.
        // We find the biggest virtual memory allocation that matches the Big Endian rom magic and then use statistics about that block,
        // to find the other block which is what we actually use.
        void FindAresRomBlock(ref bool isRomFound, ref ulong romPtrBase)
        {
            const uint byteSwappedRomMagic = 0x40123780;
            MEMORY_BASIC_INFORMATION m;
            ulong MaxAddress = process.Is64Bit() ? 0x800000000000U : 0xffffffffU;
            ulong address = 0;
            ulong largestValidRegionSize = 0;
            // first pass to find big endian rom
            for (address = 0; address <= MaxAddress; address = (ulong)m.BaseAddress + (ulong)m.RegionSize)
            {

                int result = VirtualQueryEx(process.Handle, new UIntPtr(address), out m, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
                ulong regionSize = (ulong)m.RegionSize;
                ulong currentAddress = (ulong)m.BaseAddress;
                if (address == (ulong)m.BaseAddress + (ulong)m.RegionSize || result == 0)
                    break;
                // PAGE_READWRITE
                if ((m.Protect & 0xff) != 0x4)
                    continue;
                uint value;
                bool readSuccess = process.ReadValue(new IntPtr((long)(currentAddress + 0x40)), out value);
                if (readSuccess)
                    if (value == byteSwappedRomMagic && regionSize > largestValidRegionSize)
                        largestValidRegionSize = regionSize;
            }
            // 2nd pass to find the byteswapped rom
            // ares rounds up when allocating the byteswapped rom
            // round down a page before rounding up
            largestValidRegionSize = RoundUpToPowerOfTwo(largestValidRegionSize - 0x1000);

            for (address = 0; address <= MaxAddress; address = (ulong)m.BaseAddress + (ulong)m.RegionSize)
            {
                int result = VirtualQueryEx(process.Handle, new UIntPtr(address), out m, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
                ulong regionSize = (ulong)m.RegionSize;
                ulong currentAddress = (ulong)m.BaseAddress;
                if (address == (ulong)m.BaseAddress + (ulong)m.RegionSize || result == 0)
                    break;
                // align the address
                ulong alignedAddress = (currentAddress & 0xFFFFFFFFFFFF0000);

                // always round up even with no remainder because of padding
                alignedAddress += 0x10000;

                // only search allocations that are close to the byte swapped size
                if (regionSize < largestValidRegionSize || regionSize - 0x11000 > largestValidRegionSize)
                    continue;

                // PAGE_READWRITE
                if ((m.Protect & 0xff) != 0x4)
                    continue;
                uint value;
                bool readSuccess = process.ReadValue(new IntPtr((long)(alignedAddress)), out value);
                if (readSuccess)
                    if (value == romMagic)
                    {
                        isRomFound = true;
                        romPtrBase = alignedAddress;
                    }
            }
        }

        bool IsRamBaseValid()
        {
            uint value = 0;
            bool readSuccess = process.ReadValue(new IntPtr((long)ramPtrBase), out value);
            return readSuccess && ((value & ramMagicMask) == ramMagic);
        }

        bool IsRomBaseValid()
        {
            uint value = 0;
            bool readSuccess = process.ReadValue(new IntPtr((long)romPtrBase), out value);
            return readSuccess && (value == romMagic);
        }

        public bool isValid()
        {
            return IsRamBaseValid() && IsRomBaseValid() && saveBufferOffset != 0;
        }

        public ulong RoundUpToPowerOfTwo(ulong value)
        {
            if ((value & (value - 1)) == 0)
                return value;
            value--;
            for (int i = 1; i < 64; i *= 2)
            {
                value |= value >> i;
            }
            return value + 1;
        }
    }
}
