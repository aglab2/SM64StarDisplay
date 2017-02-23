using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    public class ROMManager : IDisposable
    {
        BinaryReader reader;
        static int[] ROMOffsetforEEPOffset = { 0x10, 0x0F, 0x08, 0x11, 0x13, 0x0C, 0x15, 0x16,
                0x0E, 0x18, 0x19, 0x1A, 0x08, 0x1B, 0x21, 0x1C, 0x23, 0x1D, 0x12,
                0x14, 0x0D, 0x24, 0x08, 0x1E, 0x1F, 0x20, -1, 0x22, -1, -1, 0x17 };

        static int courseBaseAddress = 0x02AC094;
        static byte objectDescriptor = 0x24;

        static byte[] collectStarBehaviour = { 0x00, 0x3E, 0x3C };
        static byte[] redCoinStarBehaviour = { 0x00, 0x3E, 0x8C };
        static byte[] hiddenStarBehaviour = { 0x00, 0x3E, 0xFC };

        static byte[] boxBehaviour = { 0x00, 0x22, 0x50 };

        static byte[] bossWhompBehaviour = { 0x00, 0x2B, 0xB8 };
        static byte[] bossBobombBehaviour = { 0x00, 0x01, 0xF4 };
        static byte[] bossKoopaBehaviour = { 0x00, 0x45, 0x80 };
        static byte[] bossBullyBehaviour = { 0x00, 0x36, 0x60 };
        static byte[] bossBalconyBehaviour = { 0x00, 0x27, 0x68 };
        static byte[] bossPenguinBehaviour = { 0x00, 0x20, 0x88 };
        static byte[] bossBooBuddyBehaviour = { 0x00, 0x27, 0x90 };
        static byte[] bossWigglerBehaviour = { 0x00, 0x48, 0x98 };
        static byte[] bossBlizzardBehaviour = { 0x00, 0x4D, 0xBC };
        static byte[] bossPiranhaBehaviour = { 0x00, 0x51, 0x20 };

        static byte[] bossMIPSBehaviour = { 0x00, 0x44, 0xFC };

        static byte[] redsBehaviour = { 0x00, 0x3E, 0xAC };

        public ROMManager(string fileName)
        {
            if (fileName == "") throw new IOException("Bad name");
            byte[] data = File.ReadAllBytes(fileName);
            reader = new BinaryReader(new MemoryStream(data));
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        private int ReadInt32()
        {
            byte[] a32 = reader.ReadBytes(4);
            Array.Reverse(a32);
            return BitConverter.ToInt32(a32, 0);
        }

        private int ReadInt32(int offset)
        {
            reader.BaseStream.Position = offset;
            byte[] a32 = reader.ReadBytes(4);
            Array.Reverse(a32);
            return BitConverter.ToInt32(a32, 0);
        }

        private byte ReadByte()
        {
            return reader.ReadByte();
        }

        private byte ReadByte(int offset)
        {
            reader.BaseStream.Position = offset;
            return reader.ReadByte();
        }



        private byte[] ReadBehaviour(int offset)
        {
            reader.BaseStream.Position = offset + 0x15;
            return reader.ReadBytes(3);
        }

        private byte ReadBParam1(int offset)
        {
            reader.BaseStream.Position = offset + 0x10;
            return reader.ReadByte();
        }

        private byte ReadBParam2(int offset)
        {
            reader.BaseStream.Position = offset + 0x11;
            return reader.ReadByte();
        }

        public void ParseStars(LayoutDescription ld)
        {
            LineDescription[] descriptions = ld.courseDescription;
            for (int i = 0; i < descriptions.Length; i++)
            {
                int eeprom = descriptions[i].offset + 8;
                int index = Array.FindIndex(ROMOffsetforEEPOffset, el => el == eeprom);
                if (index == -1) continue;
                
                int levelAddressStart = ReadInt32(courseBaseAddress + index * 0x14 + 0x04); //base offset + offset for descriptor + address in descriptor
                int levelAddressEnd = ReadInt32();

                Console.WriteLine("{0}({1}) is address {2:x} -> {3:x} - {4:x}", eeprom, index, reader.BaseStream.Position - 0x08, levelAddressStart, levelAddressEnd);

                generateStarMask(levelAddressStart, levelAddressEnd);
            }
        }

        public int ParseReds(LayoutDescription ld, TextHighlightAction currentTHA)
        {
            if (currentTHA == null) return -1;
            int line = currentTHA.Line;
            int course = (currentTHA.IsSecret ? ld.secretDescription[line].offset : ld.courseDescription[line].offset) + 8;
            int index = Array.FindIndex(ROMOffsetforEEPOffset, el => el == course);

            if (index == -1) return -1;
            int levelAddressStart = ReadInt32(courseBaseAddress + index * 0x14 + 0x04); //base offset + offset for descriptor + address in descriptor
            int levelAddressEnd = ReadInt32();

            return getRedsAmount(levelAddressStart, levelAddressEnd);
        }

        private int getRedsAmount(int start, int end)
        {
            int counter = 0;
            for (int offset = start; offset < end; offset++)
            {
                reader.BaseStream.Position = offset;
                if (reader.ReadByte() != objectDescriptor) continue; //work with 3D object only
                byte[] behaviour = ReadBehaviour(offset);
                if (behaviour.SequenceEqual(redsBehaviour))
                {
                    counter++;
                    //Console.WriteLine("Red detected!");
                }
            }
            return counter;
        }

        private void generateStarMask(int start, int end) //Does not work :(
        {
            for (int offset = start; offset < end; offset ++)
            {
                reader.BaseStream.Position = offset;
                if (reader.ReadByte() != objectDescriptor) continue; //work with 3D object only
                byte[] behaviour = ReadBehaviour(offset);
                if (behaviour.SequenceEqual(collectStarBehaviour) || 
                    behaviour.SequenceEqual(redCoinStarBehaviour) || 
                    behaviour.SequenceEqual(hiddenStarBehaviour) ||
                    behaviour.SequenceEqual(bossWhompBehaviour) ||
                    behaviour.SequenceEqual(bossBobombBehaviour) ||
                    behaviour.SequenceEqual(bossBullyBehaviour) ||
                    behaviour.SequenceEqual(bossBalconyBehaviour) ||
                    behaviour.SequenceEqual(bossPenguinBehaviour) ||
                    behaviour.SequenceEqual(bossBooBuddyBehaviour) ||
                    behaviour.SequenceEqual(bossWigglerBehaviour) ||
                    behaviour.SequenceEqual(bossBlizzardBehaviour) //||
                    /*behaviour.SequenceEqual(bossPiranhaBehaviour)*/)
                {
                    byte starByte = ReadBParam1(offset);
                    if (starByte <= 0x06) //troll star
                    { 
                        Console.WriteLine("[S] '{0:x}' Star {1} detected!", offset, starByte);
                    }
                }

                if (behaviour.SequenceEqual(bossKoopaBehaviour))
                {
                    byte starByte = ReadBParam1(offset);
                    if (starByte == 1)
                    {
                        byte starByteParam = 1;
                        Console.WriteLine("[S] '{0:x}' Star {1} detected!", offset, starByteParam);
                    }
                }

                if (behaviour.SequenceEqual(boxBehaviour))
                {
                    byte starByte = ReadBParam2(offset);

                    if (starByte >= 0xA && starByte <= 0xE)  //not a star
                    {
                        Console.WriteLine("[B] '{0:x}' Star {1} detected!", offset, starByte - 0xA + 1);
                    }
                    if (starByte == 0x8) //star for random shit
                    {
                        byte starByteParam = ReadBParam1(offset);
                        Console.WriteLine("[8] '{0:x}' Star {1} detected!", offset, starByteParam);
                    }
                }
            }
        }
    }
}
