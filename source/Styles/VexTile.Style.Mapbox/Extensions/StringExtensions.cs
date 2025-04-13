using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using VexTile.Common.Primitives;

namespace VexTile.Style.Mapbox.Extensions
{
    [SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity")]
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string in Mapbox GL format to a Color
        /// 
        /// This function assumes, that alpha is a float in range from 0.0 to 1.0.
        /// It converts this float the range of 0..255 without rounding.
        /// The following colors could be converted:
        /// - Named colors with known Html names 
        /// - Colors as Html color values with leading '#' and 6 or 3 numbers
        /// - Function rgb(r,g,b) with values for red, green and blue
        /// - Function rgba(r,g,b,a) with values for red, green, blue and alpha. Here alpha is between 0.0 and 1.0 like opacity.
        /// - Function hsl(h,s,l) with values hue (0.0 to 360.0), saturation (0.0% - 100.0%) and lightness (0.0% - 100.0%)
        /// - Function hsla(h,s,l,a) with values hue (0.0 to 360.0), saturation (0.0% - 100.0%), lightness (0.0% - 100.0%) and alpha. Here alpha is between 0.0 and 1.0 like opacity.
        /// </summary>
        /// <param name="from">String with HTML color representation or function like rgb() or hsl()</param>
        /// <returns>Converted Skia SKColor</returns>
        public static Color FromString(this string from)
        {
            Color result = Color.Empty;

            from = from.Trim().ToLowerInvariant();

            // Check, if it is a known color
            if (KnownColors.ContainsKey(from))
                from = KnownColors[from];

            if (from.StartsWith("#"))
            {
                if (from.Length == 7)
                {
                    var color = int.Parse(from.Substring(1), NumberStyles.AllowHexSpecifier,
                        CultureInfo.InvariantCulture);
                    result = new Color((byte)(color >> 16 & 0xFF), (byte)(color >> 8 & 0xFF), (byte)(color & 0xFF), 255);
                }
                else if (from.Length == 4)
                {
                    var color = int.Parse(from.Substring(1), NumberStyles.AllowHexSpecifier,
                        CultureInfo.InvariantCulture);
                    var r = (byte)((color >> 8 & 0xF) * 16 + (color >> 8 & 0xF));
                    var g = (byte)((color >> 4 & 0xF) * 16 + (color >> 4 & 0xF));
                    var b = (byte)((color & 0xF) * 16 + (color & 0xF));
                    result = new Color(r, g, b, 255);
                }
            }
            else if (from.StartsWith("rgba"))
            {
                var split = from.Substring(from.IndexOf('(') + 1).TrimEnd(')').Split(',');

                if (split.Length != 4)
                    throw new ArgumentException($"color {from} isn't a valid color");

                var r = byte.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
                var g = byte.Parse(split[1].Trim(), CultureInfo.InvariantCulture);
                var b = byte.Parse(split[2].Trim(), CultureInfo.InvariantCulture);
                var a = float.Parse(split[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

                result = new Color(r, g, b, (byte)(a * 255));
            }
            else if (from.StartsWith("rgb"))
            {
                var split = from.Substring(from.IndexOf('(') + 1).TrimEnd(')').Split(',');

                if (split.Length != 3)
                    throw new ArgumentException($"color {from} isn't a valid color");

                var r = byte.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
                var g = byte.Parse(split[1].Trim(), CultureInfo.InvariantCulture);
                var b = byte.Parse(split[2].Trim(), CultureInfo.InvariantCulture);

                result = new Color(r, g, b, 255);
            }
            else if (from.StartsWith("hsla"))
            {
                var split = from.Substring(from.IndexOf('(') + 1).TrimEnd(')').Split(',');

                if (split.Length != 4)
                    throw new ArgumentException($"color {from} isn't a valid color");

                var h = float.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
                var s = float.Parse(split[1].Trim().Replace("%", ""), CultureInfo.InvariantCulture);
                var l = float.Parse(split[2].Trim().Replace("%", ""), CultureInfo.InvariantCulture);
                var a = float.Parse(split[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

                result = FromHsl(h / 360.0f, s / 100.0f, l / 100.0f, (int)(a * 255));
            }
            else if (from.StartsWith("hsl"))
            {
                var split = from.Substring(from.IndexOf('(') + 1).TrimEnd(')').Split(',');

                if (split.Length != 3)
                    throw new ArgumentException($"color {from} isn't a valid color");

                var h = float.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
                var s = float.Parse(split[1].Trim().Replace("%", ""), CultureInfo.InvariantCulture);
                var l = float.Parse(split[2].Trim().Replace("%", ""), CultureInfo.InvariantCulture);

                result = FromHsl(h / 360.0f, s / 100.0f, l / 100.0f);
            }

            return result;
        }

        /// <summary>
        /// Found at http://james-ramsden.com/convert-from-hsl-to-rgb-colour-codes-in-c/
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Color FromHsl(float h, float s, float l, int a = 255)
        {
            double r = 0, g = 0, b = 0;
            // != 0
            if (l > float.Epsilon)
            {
                // == 0
                if (s < float.Epsilon)
                    r = g = b = l;
                else
                {
                    float temp2;

                    if (l < 0.5)
                        temp2 = l * (1.0f + s);
                    else
                        temp2 = l + s - (l * s);

                    float temp1 = 2.0f * l - temp2;

                    r = GetColorComponent(temp1, temp2, h + 1.0f / 3.0f);
                    g = GetColorComponent(temp1, temp2, h);
                    b = GetColorComponent(temp1, temp2, h - 1.0f / 3.0f);
                }
            }
            return new Color(
                (byte)Math.Round(r * 255.0f),
                (byte)Math.Round(g * 255.0f),
                (byte)Math.Round(b * 255.0f),
                (byte)a);

        }

        /// <summary>
        /// Helper function for FromHsl function
        /// </summary>
        /// <param name="temp1"></param>
        /// <param name="temp2"></param>
        /// <param name="temp3"></param>
        /// <returns></returns>
        private static double GetColorComponent(float temp1, float temp2, float temp3)
        {
            if (temp3 < 0.0f)
                temp3 += 1.0f;
            else if (temp3 > 1.0f)
                temp3 -= 1.0f;

            if (temp3 < 1.0f / 6.0f)
                return temp1 + (temp2 - temp1) * 6.0f * temp3;
            else if (temp3 < 0.5f)
                return temp2;
            else if (temp3 < 2.0f / 3.0f)
                return temp1 + ((temp2 - temp1) * ((2.0f / 3.0f) - temp3) * 6.0f);
            else
                return temp1;
        }

        /// <summary>
        /// Change alpha channel from given color to respect opacity
        /// </summary>
        /// <param name="color">Mapsui Color to change</param>
        /// <param name="opacity">Opacity of the new color</param>
        /// <returns>New color respecting old alpha and new opacity</returns>
        public static Color Opacity(Color color, float? opacity)
        {
            if (opacity == null)
                return color;

            return new Color(color.R, color.G, color.B, (byte)Math.Round(color.A * (float)opacity));
        }

        /// <summary>
        /// Known HTML color names and hex code for RGB color
        /// </summary>
        public static readonly Dictionary<string, string> KnownColors = new()
        {
            {"AliceBlue".ToLowerInvariant(), "#F0F8FF"},
            {"AntiqueWhite".ToLowerInvariant(), "#FAEBD7"},
            {"Aqua".ToLowerInvariant(), "#00FFFF"},
            {"Aquamarine".ToLowerInvariant(), "#7FFFD4"},
            {"Azure".ToLowerInvariant(), "#F0FFFF"},
            {"Beige".ToLowerInvariant(), "#F5F5DC"},
            {"Bisque".ToLowerInvariant(), "#FFE4C4"},
            {"Black".ToLowerInvariant(), "#000000"},
            {"BlanchedAlmond".ToLowerInvariant(), "#FFEBCD"},
            {"Blue".ToLowerInvariant(), "#0000FF"},
            {"BlueViolet".ToLowerInvariant(), "#8A2BE2"},
            {"Brown".ToLowerInvariant(), "#A52A2A"},
            {"BurlyWood".ToLowerInvariant(), "#DEB887"},
            {"CadetBlue".ToLowerInvariant(), "#5F9EA0"},
            {"Chartreuse".ToLowerInvariant(), "#7FFF00"},
            {"Chocolate".ToLowerInvariant(), "#D2691E"},
            {"Coral".ToLowerInvariant(), "#FF7F50"},
            {"CornflowerBlue".ToLowerInvariant(), "#6495ED"},
            {"Cornsilk".ToLowerInvariant(), "#FFF8DC"},
            {"Crimson".ToLowerInvariant(), "#DC143C"},
            {"Cyan".ToLowerInvariant(), "#00FFFF"},
            {"DarkBlue".ToLowerInvariant(), "#00008B"},
            {"DarkCyan".ToLowerInvariant(), "#008B8B"},
            {"DarkGoldenRod".ToLowerInvariant(), "#B8860B"},
            {"DarkGray".ToLowerInvariant(), "#A9A9A9"},
            {"DarkGrey".ToLowerInvariant(), "#A9A9A9"},
            {"DarkGreen".ToLowerInvariant(), "#006400"},
            {"DarkKhaki".ToLowerInvariant(), "#BDB76B"},
            {"DarkMagenta".ToLowerInvariant(), "#8B008B"},
            {"DarkOliveGreen".ToLowerInvariant(), "#556B2F"},
            {"DarkOrange".ToLowerInvariant(), "#FF8C00"},
            {"DarkOrchid".ToLowerInvariant(), "#9932CC"},
            {"DarkRed".ToLowerInvariant(), "#8B0000"},
            {"DarkSalmon".ToLowerInvariant(), "#E9967A"},
            {"DarkSeaGreen".ToLowerInvariant(), "#8FBC8F"},
            {"DarkSlateBlue".ToLowerInvariant(), "#483D8B"},
            {"DarkSlateGray".ToLowerInvariant(), "#2F4F4F"},
            {"DarkSlateGrey".ToLowerInvariant(), "#2F4F4F"},
            {"DarkTurquoise".ToLowerInvariant(), "#00CED1"},
            {"DarkViolet".ToLowerInvariant(), "#9400D3"},
            {"DeepPink".ToLowerInvariant(), "#FF1493"},
            {"DeepSkyBlue".ToLowerInvariant(), "#00BFFF"},
            {"DimGray".ToLowerInvariant(), "#696969"},
            {"DimGrey".ToLowerInvariant(), "#696969"},
            {"DodgerBlue".ToLowerInvariant(), "#1E90FF"},
            {"FireBrick".ToLowerInvariant(), "#B22222"},
            {"FloralWhite".ToLowerInvariant(), "#FFFAF0"},
            {"ForestGreen".ToLowerInvariant(), "#228B22"},
            {"Fuchsia".ToLowerInvariant(), "#FF00FF"},
            {"Gainsboro".ToLowerInvariant(), "#DCDCDC"},
            {"GhostWhite".ToLowerInvariant(), "#F8F8FF"},
            {"Gold".ToLowerInvariant(), "#FFD700"},
            {"GoldenRod".ToLowerInvariant(), "#DAA520"},
            {"Gray".ToLowerInvariant(), "#808080"},
            {"Grey".ToLowerInvariant(), "#808080"},
            {"Green".ToLowerInvariant(), "#008000"},
            {"GreenYellow".ToLowerInvariant(), "#ADFF2F"},
            {"HoneyDew".ToLowerInvariant(), "#F0FFF0"},
            {"HotPink".ToLowerInvariant(), "#FF69B4"},
            {"IndianRed ".ToLowerInvariant(), "#CD5C5C"},
            {"Indigo ".ToLowerInvariant(), "#4B0082"},
            {"Ivory".ToLowerInvariant(), "#FFFFF0"},
            {"Khaki".ToLowerInvariant(), "#F0E68C"},
            {"Lavender".ToLowerInvariant(), "#E6E6FA"},
            {"LavenderBlush".ToLowerInvariant(), "#FFF0F5"},
            {"LawnGreen".ToLowerInvariant(), "#7CFC00"},
            {"LemonChiffon".ToLowerInvariant(), "#FFFACD"},
            {"LightBlue".ToLowerInvariant(), "#ADD8E6"},
            {"LightCoral".ToLowerInvariant(), "#F08080"},
            {"LightCyan".ToLowerInvariant(), "#E0FFFF"},
            {"LightGoldenRodYellow".ToLowerInvariant(), "#FAFAD2"},
            {"LightGray".ToLowerInvariant(), "#D3D3D3"},
            {"LightGrey".ToLowerInvariant(), "#D3D3D3"},
            {"LightGreen".ToLowerInvariant(), "#90EE90"},
            {"LightPink".ToLowerInvariant(), "#FFB6C1"},
            {"LightSalmon".ToLowerInvariant(), "#FFA07A"},
            {"LightSeaGreen".ToLowerInvariant(), "#20B2AA"},
            {"LightSkyBlue".ToLowerInvariant(), "#87CEFA"},
            {"LightSlateGray".ToLowerInvariant(), "#778899"},
            {"LightSlateGrey".ToLowerInvariant(), "#778899"},
            {"LightSteelBlue".ToLowerInvariant(), "#B0C4DE"},
            {"LightYellow".ToLowerInvariant(), "#FFFFE0"},
            {"Lime".ToLowerInvariant(), "#00FF00"},
            {"LimeGreen".ToLowerInvariant(), "#32CD32"},
            {"Linen".ToLowerInvariant(), "#FAF0E6"},
            {"Magenta".ToLowerInvariant(), "#FF00FF"},
            {"Maroon".ToLowerInvariant(), "#800000"},
            {"MediumAquaMarine".ToLowerInvariant(), "#66CDAA"},
            {"MediumBlue".ToLowerInvariant(), "#0000CD"},
            {"MediumOrchid".ToLowerInvariant(), "#BA55D3"},
            {"MediumPurple".ToLowerInvariant(), "#9370DB"},
            {"MediumSeaGreen".ToLowerInvariant(), "#3CB371"},
            {"MediumSlateBlue".ToLowerInvariant(), "#7B68EE"},
            {"MediumSpringGreen".ToLowerInvariant(), "#00FA9A"},
            {"MediumTurquoise".ToLowerInvariant(), "#48D1CC"},
            {"MediumVioletRed".ToLowerInvariant(), "#C71585"},
            {"MidnightBlue".ToLowerInvariant(), "#191970"},
            {"MintCream".ToLowerInvariant(), "#F5FFFA"},
            {"MistyRose".ToLowerInvariant(), "#FFE4E1"},
            {"Moccasin".ToLowerInvariant(), "#FFE4B5"},
            {"NavajoWhite".ToLowerInvariant(), "#FFDEAD"},
            {"Navy".ToLowerInvariant(), "#000080"},
            {"OldLace".ToLowerInvariant(), "#FDF5E6"},
            {"Olive".ToLowerInvariant(), "#808000"},
            {"OliveDrab".ToLowerInvariant(), "#6B8E23"},
            {"Orange".ToLowerInvariant(), "#FFA500"},
            {"OrangeRed".ToLowerInvariant(), "#FF4500"},
            {"Orchid".ToLowerInvariant(), "#DA70D6"},
            {"PaleGoldenRod".ToLowerInvariant(), "#EEE8AA"},
            {"PaleGreen".ToLowerInvariant(), "#98FB98"},
            {"PaleTurquoise".ToLowerInvariant(), "#AFEEEE"},
            {"PaleVioletRed".ToLowerInvariant(), "#DB7093"},
            {"PapayaWhip".ToLowerInvariant(), "#FFEFD5"},
            {"PeachPuff".ToLowerInvariant(), "#FFDAB9"},
            {"Peru".ToLowerInvariant(), "#CD853F"},
            {"Pink".ToLowerInvariant(), "#FFC0CB"},
            {"Plum".ToLowerInvariant(), "#DDA0DD"},
            {"PowderBlue".ToLowerInvariant(), "#B0E0E6"},
            {"Purple".ToLowerInvariant(), "#800080"},
            {"RebeccaPurple".ToLowerInvariant(), "#663399"},
            {"Red".ToLowerInvariant(), "#FF0000"},
            {"RosyBrown".ToLowerInvariant(), "#BC8F8F"},
            {"RoyalBlue".ToLowerInvariant(), "#4169E1"},
            {"SaddleBrown".ToLowerInvariant(), "#8B4513"},
            {"Salmon".ToLowerInvariant(), "#FA8072"},
            {"SandyBrown".ToLowerInvariant(), "#F4A460"},
            {"SeaGreen".ToLowerInvariant(), "#2E8B57"},
            {"SeaShell".ToLowerInvariant(), "#FFF5EE"},
            {"Sienna".ToLowerInvariant(), "#A0522D"},
            {"Silver".ToLowerInvariant(), "#C0C0C0"},
            {"SkyBlue".ToLowerInvariant(), "#87CEEB"},
            {"SlateBlue".ToLowerInvariant(), "#6A5ACD"},
            {"SlateGray".ToLowerInvariant(), "#708090"},
            {"SlateGrey".ToLowerInvariant(), "#708090"},
            {"Snow".ToLowerInvariant(), "#FFFAFA"},
            {"SpringGreen".ToLowerInvariant(), "#00FF7F"},
            {"SteelBlue".ToLowerInvariant(), "#4682B4"},
            {"Tan".ToLowerInvariant(), "#D2B48C"},
            {"Teal".ToLowerInvariant(), "#008080"},
            {"Thistle".ToLowerInvariant(), "#D8BFD8"},
            {"Tomato".ToLowerInvariant(), "#FF6347"},
            {"Turquoise".ToLowerInvariant(), "#40E0D0"},
            {"Violet".ToLowerInvariant(), "#EE82EE"},
            {"Wheat".ToLowerInvariant(), "#F5DEB3"},
            {"White".ToLowerInvariant(), "#FFFFFF"},
            {"WhiteSmoke".ToLowerInvariant(), "#F5F5F5"},
            {"Yellow".ToLowerInvariant(), "#FFFF00"},
            {"YellowGreen".ToLowerInvariant(), "#9ACD32"}
        };
    }
}
