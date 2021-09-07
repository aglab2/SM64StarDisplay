using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    public class LineDescriptionExSpecifiedConcreteClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(LineDescriptionEx).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)

            return base.ResolveContractConverter(objectType);
        }
    }

    public class LineDescriptionExConverter : JsonConverter
    {
        static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings()
        { ContractResolver = new LineDescriptionExSpecifiedConcreteClassConverter() };

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LineDescriptionEx);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject jo = JObject.Load(reader);
            switch (jo["Type"].Value<int>())
            {
                case 1:
                    return JsonConvert.DeserializeObject<StarsLineDescription>(jo.ToString(), SpecifiedSubclassConversion);
                case 2:
                    return JsonConvert.DeserializeObject<TextOnlyLineDescription>(jo.ToString(), SpecifiedSubclassConversion);
                default:
                    throw new Exception();
            }
        }
        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // won't be called because CanWrite returns false
        }
    }

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

    [Serializable]
    [JsonConverter(typeof(LineDescriptionExConverter))]
    public abstract class LineDescriptionEx
    {
        public abstract void DrawBase(GraphicsManager gm, int lineNumber, bool isSecret);
        public abstract bool IsEmpty();
        public abstract void FixType();
    }

    [Serializable]
    public class TextOnlyLineDescription : LineDescriptionEx
    {
        public string text;
        
        public int Type = 2;

        public override void FixType()
        {
            Type = 2;
        }

        public TextOnlyLineDescription(string text)
        {
            this.text = text;
        }

        public override void DrawBase(GraphicsManager gm, int lineNumber, bool isSecret)
        {
            Font drawFont = new Font(gm.FontFamily, gm.DrawFontSize);

            SolidBrush drawBrush = new SolidBrush(Color.White);
            RectangleF drawRect = new RectangleF((isSecret ? (gm.Width / 2) : 0) + gm.SWidth / 2, lineNumber * gm.SHeight, gm.HalfWidth, gm.SHeight);
            StringFormat drawFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };
            gm.graphics.DrawStringWithOutline(text, gm.FontFamily, gm.DrawFontSize, drawBrush, Pens.Black, drawRect, drawFormat);
            drawBrush.Dispose();
            drawFont.Dispose();
        }

        public override bool IsEmpty()
        {
            return text == "";
        }
    }

    [Serializable]
    public class StarsLineDescription : LineDescriptionEx
    {
        public string text;

        public byte starMask;
        public int offset;

        public byte highlightStarMask;
        public int highlightOffset;

        public int Type = 1;

        public override void FixType()
        {
            Type = 1;
        }

        public StarsLineDescription(string text, byte starMask, int offset, byte highlightStarMask, int highlightOffset)
        {
            this.text = text;
            this.starMask = starMask;
            this.offset = offset;
            this.highlightStarMask = highlightStarMask;
            this.highlightOffset = highlightOffset;
        }

        public override void DrawBase(GraphicsManager gm, int lineNumber, bool isSecret)
        {
            SolidBrush drawBrush = new SolidBrush(Color.White);
            RectangleF drawRect = new RectangleF((isSecret ? (gm.Width / 2) : 0), lineNumber * gm.SHeight, gm.HalfWidth, gm.SHeight);
            StringFormat drawFormat = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            };
            gm.graphics.DrawStringWithOutline(text, gm.FontFamily, gm.DrawFontSize, drawBrush, Pens.Black, drawRect, drawFormat);
            gm.DrawByte(0, lineNumber, isSecret, starMask);

            drawBrush.Dispose();
        }

        public override bool IsEmpty()
        {
            return starMask == 0 && highlightStarMask == 0 && text == "";
        }
    }
    
    [Serializable]
    public class LayoutDescriptionEx
    {
        public List<LineDescriptionEx> courseDescription;
        public List<LineDescriptionEx> secretDescription;

        [JsonConverter(typeof(ImageConverter))]
        public Bitmap goldStar;
        [JsonConverter(typeof(ImageConverter))]
        public Bitmap darkStar;
        [JsonConverter(typeof(ImageConverter))]
        public Bitmap redOutline;
        [JsonConverter(typeof(ImageConverter))]
        public Bitmap greenOutline;
        [JsonConverter(typeof(ImageConverter))]
        public Bitmap invertedStar;

        public string starAmount;

        public int starsShown;

        public bool useEmptyStars;

        // JSON
        public LayoutDescriptionEx()
        {
        }

        public LayoutDescriptionEx(List<LineDescriptionEx> courseDescription, List<LineDescriptionEx> secretDescription, Bitmap star, string starAmount, int starsShown)
        {
            this.starsShown = starsShown;

            this.courseDescription = new List<LineDescriptionEx>(courseDescription);
            this.secretDescription = new List<LineDescriptionEx>(secretDescription);
            this.starAmount = starAmount;

            this.goldStar = star;
            this.darkStar = new Bitmap(goldStar.Width, goldStar.Height);
            if (goldStar.Width != 20 || goldStar.Height != 20)
                Compress();

            GenerateInvertedStar();
            GenerateDarkStar();
            GenerateOutline();

            Trim();
        }

        public bool isValid()
        {
            return courseDescription == null || secretDescription == null;
        }

        public void GenerateInvertedStar()
        {
            invertedStar = ImageProcessing.Invert(goldStar);
        }

        public void GenerateDarkStar()
        {
            darkStar = ImageProcessing.Desaturate(goldStar);
            if (useEmptyStars)
                darkStar = ImageProcessing.GetEmptied(darkStar, 2);
        }

        public void GenerateOutline()
        {
            this.redOutline = new Bitmap(goldStar.Width, goldStar.Height);
            this.greenOutline = new Bitmap(goldStar.Width, goldStar.Height);

            int[,] outlineAlpha = ImageProcessing.GetAlpha(goldStar);
            for (int i = 0; i < 10; i++)
            {
                outlineAlpha = ImageProcessing.OutlineAlpha(outlineAlpha, goldStar);
            }
            for (int i = 0; i < goldStar.Width; i++)
            {
                for (int j = 0; j < goldStar.Height; j++)
                {
                    redOutline.SetPixel(i, j, Color.FromArgb(outlineAlpha[i, j], 255, 0, 0));
                    greenOutline.SetPixel(i, j, Color.FromArgb(outlineAlpha[i, j], 0, 255, 0));
                }
            }
        }

        public void SaturateStar()
        {
            ImageProcessing.Saturate(goldStar);
        }

        public void Trim()
        {
            while (courseDescription.Count() != 0)
            {
                LineDescriptionEx lde = courseDescription.Last();

                if (lde == null || lde.IsEmpty())
                    courseDescription.RemoveAt(courseDescription.Count() - 1);
                else
                    break;
            }

            while (secretDescription.Count() != 0)
            {
                LineDescriptionEx lde = secretDescription.Last();

                if (lde == null || lde.IsEmpty())
                    secretDescription.RemoveAt(secretDescription.Count() - 1);
                else
                    break;
            }

            foreach (var ld in courseDescription)
            {
                if (ld is object)
                    ld.FixType();
            }
            foreach (var ld in secretDescription)
            {
                if (ld is object)
                    ld.FixType();
            }
        }

        public void PrepareForEdit()
        {
            Trim();
            int courseDescriptionLength = courseDescription.Count();
            int secretDescriptionLength = secretDescription.Count();
            int length = Math.Max(courseDescriptionLength, secretDescriptionLength);
            for (int currentIndex = courseDescriptionLength; currentIndex <= length; currentIndex++)
            {
                courseDescription.Add(new TextOnlyLineDescription(""));
            }

            for (int currentIndex = secretDescriptionLength; currentIndex <= length; currentIndex++)
            {
                secretDescription.Add(new TextOnlyLineDescription(""));
            }
        }
        
        public int GetLength()
        {
            int courseDescriptionLength = courseDescription.Count();
            int secretDescriptionLength = secretDescription.Count();
            return Math.Max(courseDescriptionLength, secretDescriptionLength);
        }

        public void Compress()
        {
            var goldCompressedImage = new Bitmap(20, 20);
            var darkCompressedImage = new Bitmap(20, 20);

            var destRect = new Rectangle(0, 0, 20, 20);

            using (var graphics = Graphics.FromImage(goldCompressedImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(goldStar, destRect, 0, 0, goldStar.Width, goldStar.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            goldStar.Dispose();
            goldStar = goldCompressedImage;

            using (var graphics = Graphics.FromImage(darkCompressedImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(darkStar, destRect, 0, 0, darkStar.Width, darkStar.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            darkStar.Dispose();
            darkStar = darkCompressedImage;
        }

        public void RecountStars()
        {
            int stars = 0;

            foreach (LineDescriptionEx lind in courseDescription)
            {
                if (lind is StarsLineDescription sld)
                {
                    stars += MemoryManager.countStars(sld.starMask, starsShown);
                }
            }

            foreach (LineDescriptionEx lind in secretDescription)
            {
                if (lind is StarsLineDescription sld)
                {
                    stars += MemoryManager.countStars(sld.starMask, starsShown);
                }
            }

            starAmount = stars.ToString();
        }

        //TODO: Store in file
        static public LayoutDescriptionEx GenerateDefault()
        {
            List<LineDescriptionEx> courseLD = new LineDescriptionEx[16].ToList();
            List<LineDescriptionEx> secretLD = new LineDescriptionEx[16].ToList();

            int[] linesForSecrets = { 0, 1, 2, 3, 9, 5, 6, 7, 13, 14, 15, 11 };
            int[] offsetForSecrets = { 0, 0xb, 0xb, 0, 0, 0xb, 0xb, 0xb, 0, 0, 0, 0 };
            byte[] highlightForSecrets = { 0, 1 << 4 | 1 << 6, 1 << 5 | 1 << 7, 0, 0, 1 << 2, 1 << 1, 1 << 3, 0, 0, 0, 0 };
            string[] namesForSecrets  = { "--", "B1", "B2", "B3", "Sl", "MC", "WC", "VC", "S1", "S2", "S3", "OW" };

            courseLD[0] = new TextOnlyLineDescription("Main Courses");
            for (int course = 1; course <= 15; course++)
            {
                string drawString = course.ToString("D2");
                courseLD[course] = new StarsLineDescription(drawString, 255, course + 11, 0, 0);
            }

            for (int course = 1; course <= 10; course++) //Secret course
            {
                secretLD[linesForSecrets[course]] = new StarsLineDescription(namesForSecrets[course], 255, course + 26, highlightForSecrets[course], offsetForSecrets[course]);
            }
            secretLD[linesForSecrets[11]] = new StarsLineDescription(namesForSecrets[11], 255, 8, 0, 0);

            secretLD[0] = new TextOnlyLineDescription("Bowser Courses");
            secretLD[4] = new TextOnlyLineDescription("Cap Levels");
            secretLD[8] = new TextOnlyLineDescription("Slide");
            secretLD[10] = new TextOnlyLineDescription("Overworld Stars");
            secretLD[12] = new TextOnlyLineDescription("Secret Stars");

            return new LayoutDescriptionEx(courseLD, secretLD, Resource.gold_star, "182", 7);
        }
    }
}
