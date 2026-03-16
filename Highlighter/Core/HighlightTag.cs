using System.Windows.Media;
namespace Highlighter.Core
{
    public class HighlightTag
    {
        public HighlightTag()
        { }
        public HighlightTag(string criteria)
        {
            Criteria = criteria;
        }
        public string Criteria { get; set; }
        public Color Color { get; set; } = Helper.GetRandomColor();
        public TagShape Shape { get; set; } = TagShape.TagUnder;
        public BlurIntensity Blur { get; set; } = BlurIntensity.None;
        public FillAlpha Alpha { get; set; } = FillAlpha.Alpha_10_10;
        public bool IsActive { get; set; } = true;
        public bool AllowPartialMatch { get; set; } = false;
        public bool IsCaseSensitive { get; set; } = true;
        internal bool IsTagUnder() => Shape is TagShape.TagUnder;
        internal bool IsLine() => Shape is TagShape.Line or TagShape.LineUnder;
        internal bool IsTag() => Shape is TagShape.Tag;
        internal bool IsLineUnder() => Shape is TagShape.LineUnder;
        public override string ToString() => Criteria;
    }
}