using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    public class LevelOffsetsDescription
    {
        public int Level;
        public int EEPOffset;
        public int ROMOffset;
        public int NaturalIndex;

        public LevelOffsetsDescription(int Level, int EEPOffset, int ROMOffset, int NaturalIndex)
        {
            this.Level = Level;
            this.EEPOffset = EEPOffset;
            this.ROMOffset = ROMOffset;
            this.NaturalIndex = NaturalIndex;
        }
    }

    public class LevelInfo
    {
        // Level, EEPOffset, ROMOffset
        private static LevelOffsetsDescription[] Description = {
            new LevelOffsetsDescription(0x04, 11+5,   0,  4),  // Haunted House
            new LevelOffsetsDescription(0x05, 11+4,   1,  3),  // Cool Cool Mountain
            new LevelOffsetsDescription(0x06, 8,      2,  29), // Inside Castle
            new LevelOffsetsDescription(0x07, 11+6,   3,  5),  // Hazy Maze Cave
            new LevelOffsetsDescription(0x08, 11+8,   4,  7),  // Shifting Sand Land
            new LevelOffsetsDescription(0x09, 11+1,   5,  0),  // Bob-Omb's Battlefield
            new LevelOffsetsDescription(0x0A, 11+10,  6,  9), // Snow Man's land
            new LevelOffsetsDescription(0x0B, 11+11,  7,  10), // Wet Dry World
            new LevelOffsetsDescription(0x0C, 11+3,   8,  2),  // Jolly Roger Bay
            new LevelOffsetsDescription(0x0D, 11+13,  9,  12), // Tiny Huge Island
            new LevelOffsetsDescription(0x0E, 11+14,  10, 13), // Tick Tock Clock
            new LevelOffsetsDescription(0x0F, 11+15,  11, 14), // Rainbow Ride
            new LevelOffsetsDescription(0x10, 8,      12, 28), // Castle Grounds
            new LevelOffsetsDescription(0x11, 11+16,  13, 15), // Bowser First Course
            new LevelOffsetsDescription(0x12, 11+22,  14, 24), // Vanish Cap
            new LevelOffsetsDescription(0x13, 11+17,  15, 17), // Bowser's Fire Sea
            new LevelOffsetsDescription(0x14, 11+24,  16, 26), // Secret Aquarium
            new LevelOffsetsDescription(0x15, 11+18,  17, 19), // Bowser Third Course
            new LevelOffsetsDescription(0x16, 11+7,   18, 6),  // Lethal Lava Land
            new LevelOffsetsDescription(0x17, 11+9,   19, 8),  // Dire Dire Docks
            new LevelOffsetsDescription(0x18, 11+2,   20, 1),  // Whomp's Fortress
            new LevelOffsetsDescription(0x19, 11+25,  21, 29), // Picture at the end
            new LevelOffsetsDescription(0x1A, 8,      22, 30), // Castle Courtyard
            new LevelOffsetsDescription(0x1B, 11+19,  23, 21), // Peach's Secret Slide
            new LevelOffsetsDescription(0x1C, 11+20,  24, 22), // Metal Cap
            new LevelOffsetsDescription(0x1D, 11+21,  25, 23), // Wing Cap
            new LevelOffsetsDescription(0x1E, 11+16,  26, 16), // Bowser First Battle
            new LevelOffsetsDescription(0x1F, 11+23,  27, 25), // Rainbow Clouds
            new LevelOffsetsDescription(0x21, 11+17,  28, 18), // Bowser Second Battle
            new LevelOffsetsDescription(0x22, 11+18,  29, 20), // Bowser Third Battle
            new LevelOffsetsDescription(0x24, 11+12,  30, 11)  // Tall Tall Mountain
        };

        public static LevelOffsetsDescription FindByLevel(int level)
        {
            return Array.Find(Description, descr => descr.Level == level);
        }

        public static LevelOffsetsDescription FindByEEPOffset(int eepOffset)
        {
            return Array.Find(Description, descr => descr.EEPOffset == eepOffset);
        }

        public static LevelOffsetsDescription FindByROMOffset(int romOffset)
        {
            return Array.Find(Description, descr => descr.ROMOffset == romOffset);
        }

        public static LevelOffsetsDescription FindByNaturalIndex(int naturalIndex)
        {
            return Array.Find(Description, descr => descr.NaturalIndex == naturalIndex);
        }
    }



    public class SM64CmdHelpers
    {
        // for reading through behavior script commands and skipping the non-cmd bytes we don't need
        public static Dictionary<uint, int> behavCmdLengths = new Dictionary<uint, int>()
            {
                { 0x02, 0x8 }, { 0x04, 0x8 }, { 0x0C, 0x8 }, { 0x13, 0x8 },
                { 0x14, 0x8 }, { 0x15, 0x8 }, { 0x16, 0x8 }, { 0x17, 0x8 },
                { 0x23, 0x8 }, { 0x27, 0x8 }, { 0x2A, 0x8 }, { 0x2E, 0x8 },
                { 0x2F, 0x8 }, { 0x31, 0x8 }, { 0x33, 0x8 }, { 0x36, 0x8 },
                { 0x37, 0x8 },
                { 0x1C, 0xC }, { 0x29, 0xC }, { 0x2B, 0xC }, { 0x2C, 0xC },
                { 0x30, 0x14 }
            };
    }
}
