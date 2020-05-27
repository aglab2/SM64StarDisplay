using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    class ImageProcessing
    {
        public static Bitmap Desaturate(Bitmap goldStar)
        {
            Bitmap darkStar = new Bitmap(goldStar);
            for (int i = 0; i < goldStar.Width; i++)
            {
                for (int j = 0; j < goldStar.Height; j++)
                {
                    double h; double s; double l;
                    Color c = goldStar.GetPixel(i, j);
                    ColorRGB crgb = new ColorRGB(c);
                    ColorRGB.RGB2HSL(crgb, out h, out s, out l);

                    ColorRGB nrgb = ColorRGB.HSL2RGB(h, 0, l);

                    Color n = Color.FromArgb(c.A, nrgb.R, nrgb.G, nrgb.B);

                    darkStar.SetPixel(i, j, n);
                }
            }
            return darkStar;
        }

        public static Bitmap GetEmptied(Bitmap goldStar, int size)
        {
            bool IsTupleValid(Tuple<int, int> t)
            {
                if (t.Item1 < 0 || t.Item1 >= goldStar.Width)
                    return false;

                if (t.Item2 < 0 || t.Item2 >= goldStar.Height)
                    return false;

                return true;
            }

            List<Tuple<int, int>> GetIndices(int x, int y)
            {
                List<Tuple<int, int>> indices = new List<Tuple<int, int>>();
                for (int i = -size; i <= size; i++)
                {
                    for (int j = -size; j <= size; j++)
                    {
                        if (i == 0 && j == 0)
                            continue;

                        var pos = new Tuple<int, int>(x - i, y - j);
                        if (!IsTupleValid(pos))
                            continue;

                        indices.Add(pos);
                    }
                }
                return indices;
            }

            Bitmap darkStar = new Bitmap(goldStar);
            for (int i = 0; i < goldStar.Width; i++)
            {
                for (int j = 0; j < goldStar.Height; j++)
                {
                    var indices = GetIndices(i, j);
                    var alphas = indices.Select(t => goldStar.GetPixel(t.Item1, t.Item2).A);
                    var visibleCount = alphas.Where(a => a == 0xff).Count();
                    var invisibleCount = alphas.Count() - visibleCount;
                    var totalCount = indices.Count();

                    Color c = goldStar.GetPixel(i, j);
                    if (invisibleCount == totalCount)
                    {
                        darkStar.SetPixel(i, j, c);
                    }
                    else
                    {
                        darkStar.SetPixel(i, j, Color.FromArgb((int) ((float) invisibleCount / (float) alphas.Count() * 255), c.R, c.G, c.B));
                    }
                }
            }
            return darkStar;
        }

        public static int[,] GetAlpha(Bitmap goldStar)
        {
            int[,] A = new int[goldStar.Width, goldStar.Height];
            for (int i = 0; i < goldStar.Width; i++)
            {
                for (int j = 0; j < goldStar.Height; j++)
                {
                    Color c = goldStar.GetPixel(i, j);
                    A[i, j] = c.A;
                }
            }
            return A;
        }

        //public static void GenerateOutline(Bitmap goldStar)
        //{
            //this.redOutline = new Bitmap(goldStar.Width, goldStar.Height);
            //this.greenOutline = new Bitmap(goldStar.Width, goldStar.Height);

            /*int[,] outlineAlpha = ImageProcessing.GetAlpha(goldStar);
            for (int i = 0; i < 10; i++)
            {
                outlineAlpha = OutlineAlpha(outlineAlpha, goldStar);
            }*/
            /*for (int i = 0; i < goldStar.Width; i++)
            {
                for (int j = 0; j < goldStar.Height; j++)
                {
                    redOutline.SetPixel(i, j, Color.FromArgb(outlineAlpha[i, j], 255, 0, 0));
                    greenOutline.SetPixel(i, j, Color.FromArgb(outlineAlpha[i, j], 0, 255, 0));
                }
            }*/
        //}

        public static int[,] OutlineAlpha(int[,] alpha, Bitmap image)
        {
            int[,] ret = new int[image.Width, image.Height];

            for (int i = 1; i < image.Width - 1; i++)
            {
                for (int j = 1; j < image.Height - 1; j++)
                {
                    int tl = alpha[i + 1, j + 1];
                    int tm = alpha[i + 1, j];
                    int tr = alpha[i + 1, j - 1];

                    int ml = alpha[i, j + 1];
                    int mm = alpha[i, j];
                    int mr = alpha[i, j - 1];

                    int bl = alpha[i - 1, j + 1];
                    int bm = alpha[i - 1, j];
                    int br = alpha[i - 1, j - 1];

                    int A = tl + tm + tr + ml + mm + mr + br + bm + br;
                    A /= 9;

                    ret[i, j] = A;
                }
            }

            ret[0, 0] = alpha[0, 0] + alpha[0, 1] + alpha[1, 0]; ret[0, 0] /= 3;
            ret[image.Width - 1, 0] = alpha[image.Width - 1, 0] + alpha[image.Width - 1, 1] + alpha[image.Width - 2, 0]; ret[image.Width - 1, 0] /= 3;
            ret[0, image.Height - 1] = alpha[0, image.Height - 1] + alpha[0, image.Height - 2] + alpha[1, image.Height - 1]; ret[0, image.Height - 1] /= 3;
            ret[image.Width - 1, image.Height - 1] = alpha[image.Width - 1, image.Height - 1] + alpha[image.Width - 1, image.Height - 2] + alpha[image.Width - 2, image.Height - 1]; ret[image.Width - 1, image.Height - 1] /= 3;

            for (int i = 1; i < image.Width - 1; i++)
            {
                ret[i, 0] = ret[i - 1, 0] + ret[i, 0] + ret[i + 1, 0] + ret[i - 1, 1] + ret[i, 1] + ret[i + 1, 1]; ret[i, 0] /= 6;
                ret[i, image.Height - 1] = ret[i - 1, image.Height - 1] + ret[i, image.Height - 1] + ret[i + 1, image.Height - 1] + ret[i - 1, image.Height - 2] + ret[i, image.Height - 2] + ret[i + 1, image.Height - 2]; ret[i, image.Height - 1] /= 6;
            }

            for (int j = 1; j < image.Height - 1; j++)
            {
                ret[0, j] = ret[0, j - 1] + ret[0, j] + ret[0, j + 1] + ret[1, j - 1] + ret[1, j] + ret[1, j + 1]; ret[0, j] /= 6;
                ret[image.Width - 1, j] = ret[image.Width - 1, j - 1] + ret[image.Width - 1, j] + ret[image.Width - 1, j + 1] + ret[image.Width - 2, j - 1] + ret[image.Width - 2, j] + ret[image.Width - 2, j + 1]; ret[image.Width - 1, j] /= 6;
            }

            return ret;
        }

        public static void Saturate(Bitmap image)
        {
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double h; double s; double l;
                    Color c = image.GetPixel(i, j);
                    ColorRGB crgb = new ColorRGB(c);
                    ColorRGB.RGB2HSL(crgb, out h, out s, out l);

                    s = Math.Min(s + 0.1, 1);

                    ColorRGB nrgb = ColorRGB.HSL2RGB(h, s, l);
                    Color n = Color.FromArgb(c.A, nrgb.R, nrgb.G, nrgb.B);
                    image.SetPixel(i, j, n);
                }
            }
        }
    }
}
