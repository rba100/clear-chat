using System;

namespace ClearChat.Core.Repositories
{
    public class ColourGenerator : IColourGenerator
    {
        private const int Red = 0;
        private const int Green = 1;
        private const int Blue = 2;

        public string GenerateFromString(string input)
        {
            if (input == "System") return "000000";
            var random = new Random(input.GetStableHashCode());
            return ColourFromHue(random.NextDouble());
        }

        public bool ValidColour(string colourStr, out string errorMessage)
        {
            if (colourStr.Length != 6)
            {
                errorMessage = "Must be a six character hex string representing RGB.";
                return false;
            }

            var rgb = new byte[3];
            rgb[Red]   = Convert.ToByte(colourStr.Substring(0, 2), 16);
            rgb[Green] = Convert.ToByte(colourStr.Substring(2, 2), 16);
            rgb[Blue]  = Convert.ToByte(colourStr.Substring(4, 2), 16);

            var hsl = ColourHelper.RgbToHls(rgb[Red], rgb[Green], rgb[Blue]);

            if (hsl.luminance > 0.8 && hsl.saturation < 0.2)
            {
                errorMessage = "Colour is too light. Go darker.";
                return false;
            }
            if (hsl.luminance < 0.2)
            {
                errorMessage = "Colour is too dark.";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }

        private string ColourFromHue(double hue)
        {
            var rgb = HslToRgb(hue, 0.9, 0.30);
            var value = ((int)rgb[0]).ToString("X").PadLeft(2, '0').ToUpperInvariant() +
                        ((int)rgb[1]).ToString("X").PadLeft(2, '0').ToUpperInvariant() +
                        ((int)rgb[2]).ToString("X").PadLeft(2, '0').ToUpperInvariant();
            return value;
        }

        private double[] HslToRgb(double h, double s, double l)
        {
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l; // achromatic
            }
            else
            {
                var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                var p = 2 * l - q;
                r = hue2rgb(p, q, h + 1.0 / 3.0);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - 1.0 / 3.0);
            }

            return new[] { Math.Round(r * 255), Math.Round(g * 255), Math.Round(b * 255) };
        }

        double hue2rgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
            return p;
        }
    }
}