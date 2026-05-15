using System.Windows.Media;

namespace NexusHighlight.Options
{
    public class Foreground : PropertyChangeNotifier
    {
        private bool isActive;
        private string colorHex;
        private bool isBold;
        private bool isItalic;
        private bool hasUnderline;
        private bool hasStrikethrough;

        public bool IsActive
        {
            get { return isActive; }
            set { SetField(ref isActive, value); }
        }

        public string ColorHex
        {
            get { return colorHex; }
            set { SetField(ref colorHex, value); }
        }

        public bool IsBold
        {
            get { return isBold; }
            set { SetField(ref isBold, value); }
        }

        public bool IsItalic
        {
            get { return isItalic; }
            set { SetField(ref isItalic, value); }
        }

        public bool HasUnderline
        {
            get { return hasUnderline; }
            set { SetField(ref hasUnderline, value); }
        }

        public bool HasStrikethrough
        {
            get { return hasStrikethrough; }
            set { SetField(ref hasStrikethrough, value); }
        }

        public Foreground()
        {
            isActive = true;
            colorHex = "#000000";
            isBold = false;
            isItalic = false;
            hasUnderline = false;
            hasStrikethrough = false;
        }

        public Color? GetColor()
        {
            if (string.IsNullOrEmpty(colorHex))
                return null;

            try
            {
                return ColorConverter.ConvertFromString(colorHex) as Color?;
            }
            catch
            {
                return null;
            }
        }

        public void SetColor(Color color)
        {
            ColorHex = color.ToString();
        }
    }
}
