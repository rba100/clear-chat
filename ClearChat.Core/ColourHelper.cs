using System;

namespace ClearChat.Core
{
    internal static class ColourHelper
    {
        // Convert an RGB value into an HLS value.
        public static (double hue, double saturation, double luminance) RgbToHls(int r, int g, int b)
        {
            // Convert RGB to a 0.0 to 1.0 range.
            double doubleR = r / 255.0;
            double doubleG = g / 255.0;
            double doubleB = b / 255.0;

            // Get the maximum and minimum RGB components.
            double max = doubleR;
            if (max < doubleG) max = doubleG;
            if (max < doubleB) max = doubleB;

            double min = doubleR;
            if (min > doubleG) min = doubleG;
            if (min > doubleB) min = doubleB;

            double diff = max - min;
            double luminance = (max + min) / 2;
            double saturation, hue;
            if (Math.Abs(diff) < 0.00001)
            {
                saturation = 0;
                hue = 0;
            }
            else
            {
                if (luminance <= 0.5) saturation = diff / (max + min);
                else saturation = diff / (2 - max - min);

                double rDist = (max - doubleR) / diff;
                double gDist = (max - doubleG) / diff;
                double bDist = (max - doubleB) / diff;

                // Exact match is OK, because max is defined by one of these values.
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (doubleR == max) hue = bDist - gDist;
                else if (doubleG == max) hue = 2 + rDist - bDist;
                // ReSharper restore CompareOfFloatsByEqualityOperator
                else hue = 4 + gDist - rDist;

                hue = hue * 60;
                if (hue < 0) hue += 360;
            }

            return (hue, saturation, luminance);
        }

        // Convert an HLS value into an RGB value.
        public static (int red, int green, int blue) HlsToRgb(double h, double l, double s)
        {
            double p2;
            if (l <= 0.5) p2 = l * (1 + s);
            else p2 = l + s - l * s;

            double p1 = 2 * l - p2;
            double doubleR, doubleG, doubleB;
            if (s == 0)
            {
                doubleR = l;
                doubleG = l;
                doubleB = l;
            }
            else
            {
                doubleR = QqhToRgb(p1, p2, h + 120);
                doubleG = QqhToRgb(p1, p2, h);
                doubleB = QqhToRgb(p1, p2, h - 120);
            }

            // Convert RGB to the 0 to 255 range.
            var red = (int)(doubleR * 255.0);
            var green = (int)(doubleG * 255.0);
            var blue = (int)(doubleB * 255.0);

            return (red, green, blue);
        }

        private static double QqhToRgb(double q1, double q2, double hue)
        {
            if (hue > 360) hue -= 360;
            else if (hue < 0) hue += 360;

            if (hue < 60) return q1 + (q2 - q1) * hue / 60;
            if (hue < 180) return q2;
            if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
            return q1;
        }

        /// <summary>
        /// Get a random-looking number from a string.
        /// </summary>
        public static int GetStableHashCode(this string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + hash2 * 1566083941;
            }
        }
    }
}
