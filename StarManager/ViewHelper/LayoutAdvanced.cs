using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Side { left, right, top, bottom }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SaveType { EEPROM, SRAM, FlashRAM, MemPak, Multi }

    [Serializable]
    public class LayoutAdvancedData
    {
        public int offset;
        public int mask;
    };

    [Serializable]
    public class LayoutAdvancedCourse
    {
        public string name;
        public List<LayoutAdvancedData> data;
    }

    [Serializable]
    public class LayoutAdvancedGroup
    {
        public string name;
        public Side side;
        public List<LayoutAdvancedCourse> courses;
    }

    public class LayoutAdvancedFormat
    {
        public SaveType save_type = SaveType.EEPROM;
        public int num_slots = 4;
        public int slots_start = 0;
        public int slot_size = 112;
        public int active_bit = 95;
        public int checksum_offset = 54;
    }

    [Serializable]
    public class LayoutAdvanced
    {
        public LayoutAdvancedFormat format;
        public List<LayoutAdvancedGroup> groups;
        [JsonConverter(typeof(ImageConverter))]
        public Bitmap collectedStarIcon;
        [JsonConverter(typeof(ImageConverter))]
        public Bitmap missingStarIcon;

        public LayoutAdvanced()
        { }

        public LayoutAdvanced(LayoutDescriptionEx layout)
        {
            collectedStarIcon = layout.goldStar;
            missingStarIcon = layout.darkStar;
            groups = new List<LayoutAdvancedGroup>();

            LayoutAdvancedGroup group = null;
            foreach (var ld in layout.courseDescription)
            {

            }

            foreach (var ld in layout.secretDescription)
            {

            }
        }
    }
}
