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
            var rgb = ColourHelper.HlsToRgb(random.NextDouble() * 360, 0.4, 0.8);
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
    }
}