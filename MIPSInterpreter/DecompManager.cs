using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MIPSInterpreter
{
    public class DecompManager
    {
        public uint? VerificationOffset = null;
        public byte[] VerificationBytes = null;
        
        public int? gSaveBuffer = null;
        public int? gSaveBufferSize = null;
        public int? gSaveFileSize = null;

        // Magic regarding RAM dynamic decompiling
        static unsafe List<int> IndicesOf(uint[] arrayToSearchThrough, uint[] patternToFind)
        {
            List<int> ret = new List<int>();
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return ret;

            fixed (uint* arrayToSearchThroughPtr = arrayToSearchThrough, patternToFindPtr = patternToFind)
            {
                for (int i = 0; i <= arrayToSearchThrough.Length - patternToFind.Length; i++)
                {
                    bool found = true;
                    for (int j = 0; j < patternToFind.Length; j++)
                    {
                        if (arrayToSearchThroughPtr[i + j] != patternToFindPtr[j])
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                    {
                        ret.Add(i);
                    }
                }
            }
            
            return ret;
        }

        static unsafe List<int> FindAll(uint[] arrayToSearchThrough, uint val)
        {
            List<int> list = new List<int>();
            fixed(uint* ptr = arrayToSearchThrough)
            {
                for (int i = 0; i < arrayToSearchThrough.Length; i++)
                {
                    if (ptr[i] == val)
                        list.Add(i);
                }
            }

            return list;
        }

        static readonly uint[] BZero = new uint[]
        {
            0x28A1000C, 0x1420001D, 0x00041823, 0x30630003, 0x10600003, 0x00A32823, 0xA8800000, 0x00832021,
            0x2401FFE0, 0x00A13824, 0x10E0000C, 0x00A72823, 0x00E43821, 0x24840020, 0xAC80FFE0, 0xAC80FFE4,
            0xAC80FFE8, 0xAC80FFEC, 0xAC80FFF0, 0xAC80FFF4, 0xAC80FFF8, 0x1487FFF7, 0xAC80FFFC, 0x2401FFFC,
            0x00A13824, 0x10E00005, 0x00A72823, 0x00E43821, 0x24840004, 0x1487FFFE, 0xAC80FFFC, 0x18A00005,
            0x00000000, 0x00A42821, 0x24840001, 0x1485FFFE, 0xA080FFFF, 0x03E00008, 0x00000000,
        };

        static bool IsVAddr(uint addr)
        {
            if (0x80000000 != (0xff000000 & addr))
                return false;

            uint off = addr & 0x00ffffff;
            if (off > 0x800000)
                return false;

            return true;
        }

        static bool IsBZeroSize(int size)
        {
            return size > 0 && size <= 0x8000;
        }

        public DecompManager(uint[] mem)
        {
            uint instructionsToInterpretCount = 16;
            List<int> bzeroPositions = IndicesOf(mem, BZero);
            if (bzeroPositions.Count() == 0)
                throw new ArgumentException("Failed to find bzero!");

            foreach (int bzeroPos in bzeroPositions)
            {
                Instruction jmpInst = new Instruction
                {
                    cmd = Cmd.JAL,
                    jump = (uint)(4 * bzeroPos)
                };
                uint jmpInstVal = Converter.ToUInt(jmpInst);
                var bzeroJmpOffsets = FindAll(mem, jmpInstVal);

                SortedDictionary<int /*bzerodVAddr*/, SortedDictionary<int /*bzerodSize*/, uint /*off*/>> bzerodAddresses = new SortedDictionary<int, SortedDictionary<int, uint>>();

                foreach (uint bzeroJmpOffset in bzeroJmpOffsets)
                {
                    try
                    {
                        uint bzeroJmpVAddr = 0x80000000 | (bzeroJmpOffset << 2);
                        Interpreter interpreter = new Interpreter(mem);
                        uint bytesToInterpretCount = instructionsToInterpretCount << 2;
                        interpreter.pc = bzeroJmpVAddr - bytesToInterpretCount;

                        Instruction? inst;
                        for (int i = 0; i < instructionsToInterpretCount + 2 /*JAL + delay slot*/; i++)
                        {
                            inst = interpreter.GetInstruction();
                            if (inst.HasValue)
                                interpreter.Execute(inst.Value);
                        }

                        int addr = interpreter.gpr[(int)Register.A0];
                        int size = interpreter.gpr[(int)Register.A1];
                        if (IsVAddr((uint)addr) && IsBZeroSize(size))
                        {
                            uint interpretedBytesOffset = bzeroJmpOffset - instructionsToInterpretCount;
                            if (bzerodAddresses.TryGetValue(addr, out var descs))
                            {
                                descs[size] = interpretedBytesOffset;
                            }
                            else
                            {
                                SortedDictionary<int, uint> newDescs = new SortedDictionary<int, uint>() { { size, interpretedBytesOffset } };
                                bzerodAddresses[addr] = newDescs;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception happened when parsing PC 0x{bzeroJmpOffset:X}: {ex}");
                    }
                }

                foreach (var bzerodPair in bzerodAddresses)
                {
                    int bzerodVAddr = bzerodPair.Key;
                    var bzerodInfo = bzerodPair.Value;

                    if (bzerodInfo.Count != 2)
                        continue;

                    gSaveFileSize = bzerodInfo.Keys.ElementAt(0);
                    gSaveBufferSize = bzerodInfo.Keys.ElementAt(1);
                    if (0x70 == gSaveFileSize && 0x000c915c == bzeroPos && 0x34064442 == mem[0x9E6C5])
                    {
                        // workaround for decades later and any other binary hack that uses double file patch
                        gSaveFileSize = 0x38;
                    }
                    var interpretedOffset = bzerodInfo.Values.First();

                    gSaveBuffer = bzerodVAddr;
                    VerificationOffset = interpretedOffset << 2;

                    var interpetedSegment = new ArraySegment<uint>(mem, (int)interpretedOffset, (int)instructionsToInterpretCount);
                    VerificationBytes = new byte[instructionsToInterpretCount << 2];
                    var interpretedInstructionsIdx = 0;
                    foreach (uint num in interpetedSegment)
                    {
                        Array.Copy(BitConverter.GetBytes(num), 0, VerificationBytes, interpretedInstructionsIdx, 4);
                        interpretedInstructionsIdx += 4;
                    }

                    break;
                }

                if (gSaveBuffer.HasValue)
                    break;

                // workaround for Beyond The Cursed Mirror - start of the savefiles are a slightly offset
                foreach (var bzerodPair in bzerodAddresses)
                {
                    int bzerodVAddr = bzerodPair.Key;
                    var bzerodInfo = bzerodPair.Value;
                    if (bzerodInfo.Count != 1)
                        continue;

                    var bzerodBufferSize = bzerodInfo.First().Key;
                    for (int scanAmount = 0x4; scanAmount < 0x20; scanAmount += 4)
                    {
                        int bzerosFileVAddr = bzerodVAddr + scanAmount;
                        if (!bzerodAddresses.TryGetValue(bzerosFileVAddr, out var bzerosFileDescs))
                            continue;

                        if (bzerosFileDescs.Count != 1)
                            continue;

                        var bzerosFileDesc = bzerosFileDescs.First();
                        var bzerodFileSize = bzerosFileDesc.Key;

                        // bzeros file size cannot be larger than all the files
                        if (bzerodFileSize >= bzerodBufferSize)
                            continue;

                        gSaveFileSize = bzerodFileSize;
                        gSaveBufferSize = bzerodBufferSize;
                        var interpretedOffset = bzerodInfo.Values.First();

                        gSaveBuffer = bzerosFileVAddr;
                        VerificationOffset = interpretedOffset << 2;

                        var interpetedSegment = new ArraySegment<uint>(mem, (int)interpretedOffset, (int)instructionsToInterpretCount);
                        VerificationBytes = new byte[instructionsToInterpretCount << 2];
                        var interpretedInstructionsIdx = 0;
                        foreach (uint num in interpetedSegment)
                        {
                            Array.Copy(BitConverter.GetBytes(num), 0, VerificationBytes, interpretedInstructionsIdx, 4);
                            interpretedInstructionsIdx += 4;
                        }
                    }
                }

                if (gSaveBuffer.HasValue)
                    break;
            }
        }
    }
}
