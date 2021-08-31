using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIPSInterpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            uint[] mem;
            {
                byte[] bytes = File.ReadAllBytes("C:\\Data\\SM64StarDisplay\\ram_dump\\ze.bin");
                int size = bytes.Count() / 4;
                mem = new uint[size];
                for (int idx = 0; idx < size; idx++)
                {
                    byte[] dataInt = new byte[4];
                    dataInt[0] = bytes[3 + 4 * idx];
                    dataInt[1] = bytes[2 + 4 * idx];
                    dataInt[2] = bytes[1 + 4 * idx];
                    dataInt[3] = bytes[0 + 4 * idx];
                    mem[idx] = BitConverter.ToUInt32(dataInt, 0);
                }
            }

            DecompManager dm = new DecompManager(mem);
            Console.WriteLine($"gSaveBuffer={dm.gSaveBuffer:X} gSaveBufferSize={dm.gSaveBufferSize:X}");
        }
    }
}
