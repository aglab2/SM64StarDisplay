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

    [Serializable]
    public class LayoutAdvanced
    {
        public List<LayoutAdvancedGroup> groups;
        [JsonConverter(typeof(ImageConverter))]
        public Bitmap collectedStarIcon;
        [JsonConverter(typeof(ImageConverter))]
        public Bitmap missingStarIcon;
    }
}
