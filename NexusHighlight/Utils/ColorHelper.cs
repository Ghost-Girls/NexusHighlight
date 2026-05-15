using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;

namespace NexusHighlight.Utils
{
    public static class ColorHelper
    {
        public static List<Color> Colors { get; } = new List<Color>();

        public static Color Undefined { get; } = Color.FromArgb(0, 0, 0, 0);

        public static string ColorToHex(this Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

        public static Color HexToColor(string value)
        {
            value = value.Trim('#');
            switch (value.Length)
            {
                case 0:
                    return Undefined;
                case <= 6:
                    value = "FF" + value.PadLeft(6, '0');
                    break;
            }

            return uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint u)
                ? UIntToColor(u)
                : Undefined;
        }

        public static Color UIntToColor(uint color)
        {
            var a = (byte)(color >> 24);
            var r = (byte)(color >> 16);
            var g = (byte)(color >> 8);
            var b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }

        public static void InitializeColors()
        {
            if (Colors.Count > 0)
                return;

            Colors.Add(HexToColor("#FFB1B9"));
            Colors.Add(HexToColor("#EE1E34"));
            Colors.Add(HexToColor("#E8142A"));
            Colors.Add(HexToColor("#D2041A"));
            Colors.Add(HexToColor("#FFCEAE"));
            Colors.Add(HexToColor("#FE7A2B"));
            Colors.Add(HexToColor("#FA5C00"));
            Colors.Add(HexToColor("#EE5700"));
            Colors.Add(HexToColor("#A4E8C2"));
            Colors.Add(HexToColor("#1F9850"));
            Colors.Add(HexToColor("#0C823C"));
            Colors.Add(HexToColor("#006929"));
            Colors.Add(HexToColor("#D0E2FF"));
            Colors.Add(HexToColor("#5d93f8"));
            Colors.Add(HexToColor("#1062FF"));
            Colors.Add(HexToColor("#0c4dce"));
            Colors.Add(HexToColor("#FFDCF9"));
            Colors.Add(HexToColor("#DF2F9B"));
            Colors.Add(HexToColor("#BD0A78"));
            Colors.Add(HexToColor("#8E0157"));
            Colors.Add(HexToColor("#EFD1FF"));
            Colors.Add(HexToColor("#AF34FE"));
            Colors.Add(HexToColor("#9F1FF3"));
            Colors.Add(HexToColor("#8911D9"));
            Colors.Add(HexToColor("#BEB8E5"));
            Colors.Add(HexToColor("#473497"));
            Colors.Add(HexToColor("#2A1770"));
            Colors.Add(HexToColor("#1C0C56"));
            Colors.Add(HexToColor("#BCF2FF"));
            Colors.Add(HexToColor("#12B2F3"));
            Colors.Add(HexToColor("#019AD8"));
            Colors.Add(HexToColor("#01668F"));
            Colors.Add(HexToColor("#77F3E6"));
            Colors.Add(HexToColor("#15C1B0"));
            Colors.Add(HexToColor("#059183"));
            Colors.Add(HexToColor("#04685E"));
            Colors.Add(HexToColor("#D1F95F"));
            Colors.Add(HexToColor("#79A00B"));
            Colors.Add(HexToColor("#425900"));
            Colors.Add(HexToColor("#334500"));

            Colors.Add(HexToColor("#C00000"));
            Colors.Add(HexToColor("#FF0000"));
            Colors.Add(HexToColor("#FFC000"));
            Colors.Add(HexToColor("#FFFF00"));
            Colors.Add(HexToColor("#92D050"));
            Colors.Add(HexToColor("#00B050"));
            Colors.Add(HexToColor("#00B0F0"));
            Colors.Add(HexToColor("#0070C0"));
            Colors.Add(HexToColor("#002060"));
            Colors.Add(HexToColor("#7030A0"));
        }
    }
}
