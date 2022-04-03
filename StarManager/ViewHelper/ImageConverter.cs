using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace StarDisplay
{
    public class ImageConverter : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string text = (string)reader.Value;
            if (string.IsNullOrWhiteSpace(text))
            {
                return Resource.gold_star;
            }
            return Image.FromStream(new MemoryStream(Convert.FromBase64String(text)));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Image obj = (Image)value;
            MemoryStream memoryStream = new MemoryStream();
            obj.Save(memoryStream, ImageFormat.Png);
            byte[] value2 = memoryStream.ToArray();
            writer.WriteValue(value2);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Image);
        }
    }
}
