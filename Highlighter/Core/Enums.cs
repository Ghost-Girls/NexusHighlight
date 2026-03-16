namespace Highlighter.Core
{
    [Serializable]
    public enum TagShape
    {
        Tag,
        TagUnder,
        Line,
        LineUnder
    }

    [Serializable]
    public enum BlurIntensity
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Ultra = 4
    }

    [Serializable]
    public enum FillAlpha
    {
        Alpha_0_10 = 0,
        Alpha_1_10 = 1,
        Alpha_2_10 = 2,
        Alpha_3_10 = 3,
        Alpha_4_10 = 4,
        Alpha_5_10 = 5,
        Alpha_6_10 = 6,
        Alpha_7_10 = 7,
        Alpha_8_10 = 8,
        Alpha_9_10 = 9,
        Alpha_10_10 = 10
    }

    public enum Performance
    {
        Normal,
        Fast,
        NoEffects
    }
}