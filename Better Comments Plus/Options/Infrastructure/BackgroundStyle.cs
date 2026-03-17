using System.Windows.Media;

namespace BetterCommentsPlus.Options
{
    public class BackgroundStyle : PropertyChangeNotifier
    {
        private bool isActive;
        private string colorHex;
        private string shape;
        private string blur;
        private string alpha;
        private bool isCaseSensitive;
        private bool allowPartialMatch;

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

        public string Shape
        {
            get { return shape; }
            set { SetField(ref shape, value); }
        }

        public string Blur
        {
            get { return blur; }
            set { SetField(ref blur, value); }
        }

        public string Alpha
        {
            get { return alpha; }
            set { SetField(ref alpha, value); }
        }

        public bool IsCaseSensitive
        {
            get { return isCaseSensitive; }
            set { SetField(ref isCaseSensitive, value); }
        }

        public bool AllowPartialMatch
        {
            get { return allowPartialMatch; }
            set { SetField(ref allowPartialMatch, value); }
        }

        public BackgroundStyle()
        {
            isActive = false;
            colorHex = null;
            shape = "Tag";
            blur = "None";
            alpha = "1/10";
            isCaseSensitive = true;
            allowPartialMatch = false;
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
