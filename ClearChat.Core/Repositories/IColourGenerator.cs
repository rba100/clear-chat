using System;

namespace ClearChat.Core.Repositories
{
    public interface IColourGenerator
    {
        string GenerateFromString(string input);
    }

    public class ColourGenerator : IColourGenerator
    {
        public string GenerateFromString(string input)
        {
            var random = new Random(input.GetHashCode());
            return colourFromHue(random.NextDouble());
        }

        private string colourFromHue(double hue)
        {
            var rgb = hslToRgb(hue, 0.9, 0.30);
            var value = ((int)rgb[0]).ToString("X").PadLeft(2, '0').ToUpperInvariant() +
                        ((int)rgb[1]).ToString("X").PadLeft(2, '0').ToUpperInvariant() +
                        ((int)rgb[2]).ToString("X").PadLeft(2, '0').ToUpperInvariant();
            return value;
        }

        private double[] hslToRgb(double h, double s, double l)
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
