using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Media;

namespace NexusHighlight.CommentsViewCustomization
{
    public static class BackgroundHelper
    {
        public static char[] Escapes = { ' ', '!', '"', '@', '$', '(', ')', '{', '}', '[', ']', '*', '-', '.', '/', '>', '<', ':', ';', ',', '?', '\'', '\n', '\r', '\t', '=' };

        public static List<Color> Colors = new();

        public static Color Undefined { get; } = Color.FromArgb(0, 0, 0, 0);

        [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
        public static Color ChangeAlpha(this Color c, byte alpha) => Color.FromArgb(alpha, c.R, c.G, c.B);

        public static string ColorToHex(this Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

        public static List<Color> SetupColors(this List<Color> colors)
        {
            colors.Clear();
            colors.Add(HexToColor("#FFB1B9"));
            colors.Add(HexToColor("#EE1E34"));
            colors.Add(HexToColor("#E8142A"));
            colors.Add(HexToColor("#D2041A"));
            colors.Add(HexToColor("#FFCEAE"));
            colors.Add(HexToColor("#FE7A2B"));
            colors.Add(HexToColor("#FA5C00"));
            colors.Add(HexToColor("#EE5700"));
            colors.Add(HexToColor("#A4E8C2"));
            colors.Add(HexToColor("#1F9850"));
            colors.Add(HexToColor("#0C823C"));
            colors.Add(HexToColor("#006929"));
            colors.Add(HexToColor("#D0E2FF"));
            colors.Add(HexToColor("#5d93f8"));
            colors.Add(HexToColor("#1062FF"));
            colors.Add(HexToColor("#0c4dce"));
            colors.Add(HexToColor("#FFDCF9"));
            colors.Add(HexToColor("#DF2F9B"));
            colors.Add(HexToColor("#BD0A78"));
            colors.Add(HexToColor("#8E0157"));
            colors.Add(HexToColor("#EFD1FF"));
            colors.Add(HexToColor("#AF34FE"));
            colors.Add(HexToColor("#9F1FF3"));
            colors.Add(HexToColor("#8911D9"));
            colors.Add(HexToColor("#BEB8E5"));
            colors.Add(HexToColor("#473497"));
            colors.Add(HexToColor("#2A1770"));
            colors.Add(HexToColor("#1C0C56"));
            colors.Add(HexToColor("#BCF2FF"));
            colors.Add(HexToColor("#12B2F3"));
            colors.Add(HexToColor("#019AD8"));
            colors.Add(HexToColor("#01668F"));
            colors.Add(HexToColor("#77F3E6"));
            colors.Add(HexToColor("#15C1B0"));
            colors.Add(HexToColor("#059183"));
            colors.Add(HexToColor("#04685E"));
            colors.Add(HexToColor("#D1F95F"));
            colors.Add(HexToColor("#79A00B"));
            colors.Add(HexToColor("#425900"));
            colors.Add(HexToColor("#334500"));

            colors.Add(HexToColor("#C00000"));
            colors.Add(HexToColor("#FF0000"));
            colors.Add(HexToColor("#FFC000"));
            colors.Add(HexToColor("#FFFF00"));
            colors.Add(HexToColor("#92D050"));
            colors.Add(HexToColor("#00B050"));
            colors.Add(HexToColor("#00B0F0"));
            colors.Add(HexToColor("#0070C0"));
            colors.Add(HexToColor("#002060"));
            colors.Add(HexToColor("#7030A0"));

            colors.Add(HexToColor("#5E54C9"));
            colors.Add(HexToColor("#FF0F87"));
            colors.Add(HexToColor("#70AD47"));
            colors.Add(HexToColor("#F83605"));
            colors.Add(HexToColor("#FE350E"));
            colors.Add(HexToColor("#9B9B9B"));
            colors.Add(HexToColor("#77A88D"));
            colors.Add(HexToColor("#5759A8"));
            colors.Add(HexToColor("#07EAD3"));
            colors.Add(HexToColor("#FF410C"));
            colors.Add(HexToColor("#D379D8"));
            colors.Add(HexToColor("#ED1C15"));
            colors.Add(HexToColor("#343CD8"));
            colors.Add(HexToColor("#1C6BFF"));

            return colors;
        }

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

        public static Color GetRandomColor()
        {
            if (Colors.Count == 0)
                Colors.SetupColors();

            System.Random r = new(System.Environment.TickCount);
            return Colors[r.Next(0, Colors.Count - 1)];
        }

        public static void InitDefaults()
        {
            Colors.SetupColors();
        }
    }
}
