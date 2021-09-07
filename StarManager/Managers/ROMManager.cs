using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Net;
using System.Drawing;

namespace StarDisplay
{
    public class ROMManager : CachedManager, IDisposable
    {
        BinaryReader reader;

        static int courseBaseAddress = 0x02AC094;

        static byte levelscriptEndDescriptor = 0x02;
        static byte jumpDescriptor = 0x05;
        static byte callDescriptor = 0x06;
        static byte returnDescriptor = 0x07;
        static byte objectDescriptor = 0x24;
        static byte areaStartDescriptor = 0x1F;
        static byte areaEndDescriptor = 0x20;
        static byte loadSegmentDescriptor = 0x17;

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
        static byte[] bossEyerockBehaviour = { 0x00, 0x52, 0xB4 };

        static byte[] bossMIPSBehaviour = { 0x00, 0x44, 0xFC };

        static byte[] redsBehaviour = { 0x00, 0x3E, 0xAC };
        static byte[] secretsBehaviour = { 0x00, 0x3F, 0x1C };
        static byte[] flipswitchBehaviour = { 0x00, 0x05, 0xD8 };

        Object[] boxObjects;

        public ROMManager(string fileName)
        {
            if (fileName == "") throw new IOException("Bad name");

            byte[] data = null;
            if (fileName.EndsWith(".zip")) //unpack
            {
                using (FileStream zipToOpen = new FileStream(fileName, FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.EndsWith(".z64") || entry.FullName.EndsWith(".n64") || entry.FullName.EndsWith(".v64"))
                            {
                                fileName = entry.FullName;
                                using (Stream stream = entry.Open())
                                {
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        stream.CopyTo(memoryStream);
                                        data = memoryStream.ToArray();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                data = File.ReadAllBytes(fileName);
            }

            if (fileName.EndsWith(".n64")) //big-endian
            {
                byte[] stuff = new byte[4];
                for (int i = 0; i < data.Length; i += 4)
                {
                    stuff[0] = data[i + 3];
                    stuff[1] = data[i + 2];
                    stuff[2] = data[i + 1];
                    stuff[3] = data[i + 0];

                    data[i + 0] = stuff[0];
                    data[i + 1] = stuff[1];
                    data[i + 2] = stuff[2];
                    data[i + 3] = stuff[3];
                }
            }
            else if (fileName.EndsWith(".v64")) //byte swapped
            {
                byte[] stuff = new byte[2];
                for (int i = 0; i < data.Length; i += 2)
                {
                    stuff[0] = data[i + 1];
                    stuff[1] = data[i + 0];

                    data[i + 0] = stuff[0];
                    data[i + 1] = stuff[1];
                }
            }

            reader = new BinaryReader(new MemoryStream(data));
            boxObjects = ReadBoxBehaviours();
        }

        public ROMManager(byte[] data)
        {
            if (data == null) throw new IOException("Data is null");
            reader = new BinaryReader(new MemoryStream(data));
            boxObjects = ReadBoxBehaviours();
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

        private byte ReadAct(int offset)
        {
            reader.BaseStream.Position = offset + 0x02;
            return reader.ReadByte();
        }

        private byte ReadLength(int offset)
        {
            reader.BaseStream.Position = offset + 0x01;
            return reader.ReadByte();
        }

        public void ParseStars(LayoutDescriptionEx ld)
        {
            int totalStars = 0;
            totalStars += UpdateLineDescriptionsWithStars(2 << 6, ld.courseDescription, ld.starsShown);
            totalStars += UpdateLineDescriptionsWithStars(0, ld.secretDescription, ld.starsShown);
            ld.RecountStars();
        }

        private int UpdateLineDescriptionsWithStars(byte initMask, List<LineDescriptionEx> descriptions, int maxShown)
        {
            int totalStars = 0;
            for (int i = 0; i < descriptions.Count; i++)
            {
                if (descriptions[i] is StarsLineDescription sld)
                {
                    int eeprom = sld.offset;

                    LevelOffsetsDescription levelOffsets = LevelInfo.FindByEEPOffset(eeprom);
                    if (levelOffsets == null)
                        continue;

                    int romOffset = levelOffsets.ROMOffset;

                    int levelAddressStart = ReadInt32(courseBaseAddress + romOffset * 0x14 + 0x04); //base offset + offset for descriptor + address in descriptor
                    int levelAddressEnd = ReadInt32();

                    sld.starMask = GenerateStarMask(initMask, levelAddressStart, levelAddressEnd);
                    totalStars += MemoryManager.countStars(sld.starMask, maxShown);
                }
            }
            return totalStars;
        }

        private int PrepareAddresses(int level, out int levelAddressStart, out int levelAddressEnd, out int levelOffset)
        {
            LevelOffsetsDescription levelOffsets = LevelInfo.FindByLevel(level);
            if (levelOffsets == null)
            {
                levelAddressStart = 0; levelAddressEnd = 0; levelOffset = 0; return -1;
            }

            int romOffset = levelOffsets.ROMOffset;
            
            levelAddressStart = ReadInt32(courseBaseAddress + romOffset * 0x14 + 0x04); //base offset + offset for descriptor + address in descriptor
            levelAddressEnd = ReadInt32(courseBaseAddress + romOffset * 0x14 + 0x08);
            levelOffset = ReadInt32(courseBaseAddress + romOffset * 0x14 + 0x0C) & 0x0000FFFF;
            return 0;
        }

        public int ParseReds(int level, int currentStar, int currentArea)
        {
            int result = PrepareAddresses(level, out int levelAddressStart, out int levelAddressEnd, out int levelOffset);
            if (result != 0) return 0;

            return GetAmountOfObjects(levelAddressStart, levelAddressEnd, levelOffset, redsBehaviour, currentStar, currentArea);
        }

        public int ParseSecrets(int level, int currentStar, int currentArea)
        {
            int result = PrepareAddresses(level, out int levelAddressStart, out int levelAddressEnd, out int levelOffset);
            if (result != 0) return 0;

            return GetAmountOfObjects(levelAddressStart, levelAddressEnd, levelOffset, secretsBehaviour, currentStar, currentArea);
        }
    
        public int ParseFlipswitches(int level, int currentStar, int currentArea)
        {
            int result = PrepareAddresses(level, out int levelAddressStart, out int levelAddressEnd, out int levelOffset);
            if (result != 0) return 0;

            return GetAmountOfObjects(levelAddressStart, levelAddressEnd, levelOffset, flipswitchBehaviour, currentStar, currentArea);
        }

        private int GetAmountOfObjects(int start, int end, int offset, byte[] searchBehaviour, int currentStar, int currentArea)
        {
            int area = 0;
            return GetAmountOfObjectsInternal(start, end, offset, searchBehaviour, currentStar, currentArea, ref area);
        }

        public int SwapBytes(int x)
        {
            return (int) (((x & 0x000000ff) << 24) +
                   ((x & 0x0000ff00) << 8) +
                   ((x & 0x00ff0000) >> 8) +
                   ((x & 0xff000000) >> 24));
        }


        private int GetAmountOfObjectsInternal (int start, int end, int Loffset, byte[] searchBehaviour, int currentStar, int currentArea, ref int area)
        {
            if (currentArea == 0) currentArea = 1;
            byte currentStarMask = (byte) (1 << currentStar);
            int counter = 0;
            if (start < 0) return counter;

            int temporaryBankEStart = 0;
            int temporaryBankEEnd   = 0;

            try
            {
                int length = 0;
                for (int offset = start + Loffset; offset < end; offset += length)
                {
                    reader.BaseStream.Position = offset;

                    byte command = reader.ReadByte();
                    length = reader.ReadByte();
                    if ((length == 0) || 
                        (command == levelscriptEndDescriptor) || (command == returnDescriptor))
                        return counter;

                    if (command == jumpDescriptor || command == callDescriptor)
                    {
                        reader.BaseStream.Position = offset + 0x4;
                        int bank = reader.ReadByte();
                        if (bank != 0x13 && bank != 0xe)
                            continue;

                        reader.BaseStream.Position = offset + 0x6;
                        byte[] jumpOffsetBytes = reader.ReadBytes(2);
                        byte x = jumpOffsetBytes[0];
                        jumpOffsetBytes[0] = jumpOffsetBytes[1];
                        jumpOffsetBytes[1] = x;

                        int jumpOffset = BitConverter.ToInt16(jumpOffsetBytes, 0);

                        var newStart = start;
                        var newEnd = end;
                        if (bank == 0xE)
                        {
                            newStart = temporaryBankEStart;
                            newEnd = temporaryBankEEnd;
                        }

                        counter += GetAmountOfObjectsInternal(newStart, newEnd, jumpOffset, searchBehaviour, currentStar, currentArea, ref area);
                        if (command == jumpDescriptor)
                            return counter;
                    }
                    else if (command == areaStartDescriptor)
                    {
                        area = reader.ReadByte();
                    }
                    else if (command == areaEndDescriptor)
                    {
                        area = 0;
                    }
                    else if (command == objectDescriptor) 
                    {
                        if (area != currentArea) continue;
                        int act = ReadAct(offset);
                        if ((act & 31) != 31)
                        {
                            if ((ReadAct(offset) & currentStarMask) == 0)
                                continue;
                        }

                        byte[] behaviour = ReadBehaviour(offset);
                        if (behaviour.SequenceEqual(searchBehaviour))
                        {
                            counter++;
                        }
                    }
                    else if (command == loadSegmentDescriptor)
                    {
                        reader.BaseStream.Position = offset + 0x3;
                        int bank = reader.ReadByte();
                        if (bank != 0xE)
                            continue;

                        reader.BaseStream.Position = offset + 0x4;
                        temporaryBankEStart = SwapBytes(reader.ReadInt32());
                        reader.BaseStream.Position = offset + 0x8;
                        temporaryBankEEnd = SwapBytes(reader.ReadInt32());
                    }
                }
            }
            catch (IOException) { }
            return counter;
        }

        //TODO: This does not work too well
        private byte GenerateStarMask(byte initMask, int start, int end) //Does not work :(
        {
            int mask = initMask;
            for (int offset = start; offset < end; offset++)
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
                    behaviour.SequenceEqual(bossBlizzardBehaviour) ||
                    behaviour.SequenceEqual(bossEyerockBehaviour))
                {
                    byte starByte = ReadBParam1(offset);
                    if (starByte <= 0x06) //troll star
                    {
                        mask |= 1 << starByte;
                        Console.WriteLine("[S] '{0:x}' Star {1} detected!", offset, starByte);
                    }
                }

                if (behaviour.SequenceEqual(bossKoopaBehaviour))
                {
                    byte starByte = ReadBParam1(offset);
                    if (starByte == 1)
                    {
                        byte starByteParam = 1;
                        mask |= 1 << starByteParam;
                        Console.WriteLine("[K] '{0:x}' Star {1} detected!", offset, starByteParam);
                    }
                    if (starByte == 2)
                    {
                        byte starByteParam = 2;
                        mask |= 1 << starByteParam;
                        Console.WriteLine("[K] '{0:x}' Star {1} detected!", offset, starByteParam);
                    }
                }

                if (behaviour.SequenceEqual(boxBehaviour))
                {
                    byte boxObjectByte = ReadBParam2(offset);
                    Object currentObject = boxObjects[boxObjectByte];
                    //Console.WriteLine(String.Format("{1:X}: {0:X8}", currentObject.Behaviour, boxObjectByte));
                    if (boxObjectByte == 0x08)
                    {
                        byte starByte = ReadBParam1(offset);
                        if (starByte <= 0x6)
                        {
                            mask |= 1 << starByte;
                            Console.WriteLine("[B] '{0:x}' Star {1} in box detected 8!", offset, starByte);
                        }
                    }
                    else if (currentObject != null && currentObject.Behaviour == 0x130007F8)
                    {
                        if (currentObject.BParam1 <= 0x6)
                        {
                            mask |= 1 << currentObject.BParam2;
                            Console.WriteLine("[B] '{0:x}' Star {1} in box detected!", offset, currentObject.BParam2);
                        }
                    }
                }
            }
            return (byte) mask;
        }

        static byte maxBehaviour = 0x63;

        public class Object
        {
            public byte BParam1;
            public byte BParam2;
            public byte Model;
            public int Behaviour;

            public Object(byte BParam1, byte BParam2, byte Model, int Behaviour)
            {
                this.BParam1 = BParam1;
                this.BParam2 = BParam2;
                this.Model = Model;
                this.Behaviour = Behaviour;
            }
        }

        public Object[] ReadBoxBehaviours()
        {
            Object[] ret = new Object[maxBehaviour];
            
            byte magic = ReadByte(0x7C8E3);
            int boxParamDescriptorsAddress = magic == 0x40 ? 0x01204000 : 0xEBBA0;
            
            reader.BaseStream.Position = boxParamDescriptorsAddress;
            while (true)
            {
                byte[] data = reader.ReadBytes(8);
                if (data[0] >= maxBehaviour) return ret;
                Object obj = new Object(data[1], data[2], data[3], IPAddress.HostToNetworkOrder(BitConverter.ToInt32(data, 4)));
                ret[data[0]] = obj;
            }
        }

        public Bitmap GetStarImage()
        {
            reader.BaseStream.Position = 0x807956; //0x803156 + 0x4800
            byte[] data = reader.ReadBytes(512);
            return MemoryManager.FromRGBA16(data);
        }

        public string GetROMName()
        {
            reader.BaseStream.Position = 0x20;
            return Encoding.UTF8.GetString(reader.ReadBytes(20), 0, 20).Trim().TrimEnd('\0').Trim();
        }
    }
}
