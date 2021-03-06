﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPSInterpreter
{
    public class DecompManager
    {
        public uint? gSaveBufferBZeroJmpVAddr = null;
        public int? gSaveBuffer = null;
        public int? gSaveBufferSize = null;

        // Magic regarding RAM dynamic decompiling
        static List<int> IndicesOf(uint[] arrayToSearchThrough, uint[] patternToFind)
        {
            List<int> ret = new List<int>();
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return ret;

            for (int i = 0; i <= arrayToSearchThrough.Length - patternToFind.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < patternToFind.Length; j++)
                {
                    if (arrayToSearchThrough[i + j] != patternToFind[j])
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
            
            return ret;
        }

        static List<int> FindAll(uint[] arrayToSearchThrough, uint val)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < arrayToSearchThrough.Length; i++)
            {
                if (arrayToSearchThrough[i] == val)
                    list.Add(i);
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
            return size > 0 && size < 0x1000;
        }

        public DecompManager(uint[] mem)
        {
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

                SortedList<int /*bzerodVAddr*/, Dictionary<int /*bzerodSize*/, uint /*bzeroJmpVAddr*/>> bzerodAddresses = new SortedList<int, Dictionary<int, uint>>();

                foreach (uint bzeroJmpOffset in bzeroJmpOffsets)
                {
                    try
                    {
                        uint bzeroJmpVAddr = 0x80000000 | (bzeroJmpOffset << 2);
                        Interpreter interpreter = new Interpreter(mem);
                        uint instructionsToInterpretCount = 16;
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
                            if (bzerodAddresses.TryGetValue(addr, out var descs))
                            {
                                descs[size] = bzeroJmpVAddr;
                            }
                            else
                            {
                                Dictionary<int, uint> newDescs = new Dictionary<int, uint>() { { size, bzeroJmpVAddr } };
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

                    uint bzeroJmpVAddr = 0;
                    int? eepSize = null;
                    if (!eepSize.HasValue && bzerodInfo.TryGetValue(0x200, out bzeroJmpVAddr))
                    {
                        eepSize = 0x200;
                    }
                    if (!eepSize.HasValue && bzerodInfo.TryGetValue(0x400, out bzeroJmpVAddr))
                    {
                        eepSize = 0x400;
                    }

                    if (!eepSize.HasValue)
                        continue;

                    int bzerodVAddrIdx = bzerodAddresses.IndexOfKey(bzerodVAddr);
                    int bzerodNextVAddr = bzerodAddresses.ElementAt(bzerodVAddrIdx + 1).Key;
                    if (bzerodNextVAddr - bzerodVAddr > eepSize)
                        continue;

                    gSaveBuffer = bzerodVAddr;
                    gSaveBufferBZeroJmpVAddr = bzeroJmpVAddr;
                    gSaveBufferSize = eepSize;
                    break;
                }

                if (gSaveBuffer.HasValue)
                    break;
            }
        }
    }
}
