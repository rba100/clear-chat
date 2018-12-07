using System;

using ClearChat.Core.Crypto;

namespace ClearChat.Core.Repositories
{
    public class ColourGenerator : IColourGenerator
    {
        private readonly IStringHasher m_StringHasher;

        public ColourGenerator(IStringHasher stringHasher)
        {
            m_StringHasher = stringHasher;
        }

        public string GenerateFromString(string input)
        {
            if (input == "System") return "000000";
            var hash = m_StringHasher.Hash(input);
            var seed = BitConverter.ToInt32(hash, 0);
            var random = new Random(seed);

            var rgb = ColourHelper.HlsToRgb(h: random.NextDouble() * 360,
                                            l: random.NextDouble() * 0.2 + 0.2,
                                            s: random.NextDouble() * 0.2 + 0.65);

            return rgb.red.ToString("X").PadLeft(2, '0').ToUpperInvariant() +
                   rgb.green.ToString("X").PadLeft(2, '0').ToUpperInvariant() +
                   rgb.blue.ToString("X").PadLeft(2, '0').ToUpperInvariant();
        }

        public bool ValidColour(string colourStr, out string errorMessage)
        {
            if (colourStr.Length != 6)
            {
                errorMessage = "Must be a six character hex string representing RGB.";
                return false;
            }

            var rgb = new byte[3];
            rgb[0] = Convert.ToByte(colourStr.Substring(0, 2), 16);
            rgb[1] = Convert.ToByte(colourStr.Substring(2, 2), 16);
            rgb[2] = Convert.ToByte(colourStr.Substring(4, 2), 16);

            var hsl = ColourHelper.RgbToHls(rgb[0], rgb[1], rgb[2]);

            if (hsl.luminance > 0.65)
            {
                errorMessage = "Colour is too light. Go darker.";
                return false;
            }
            if (hsl.luminance < 0.35)
            {
                errorMessage = "Colour is too dark.";
                return false;
            }

            if (hsl.hue > 55 && hsl.hue < 70) // Yellow
            {
                if (hsl.saturation > 0.75 || hsl.luminance > 0.45)
                {
                    errorMessage = "Colour is too yellow.";
                    return false;
                }
            }
            errorMessage = string.Empty;
            return true;
        }
    }
}