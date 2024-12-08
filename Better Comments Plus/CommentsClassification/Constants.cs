using System.Windows.Media;

namespace BetterCommentsPlus.CommentsClassification
{
    internal static class CommentNames
    {
        public const string IMPORTANT_COMMENT = "Comment - IMPORTANT";
        public const string QUESTION_COMMENT = "Comment - QUESTION";
        public const string REMOVE_COMMENT = "Comment - REMOVE";
        public const string TASK_COMMENT = "Comment - TASK";

    }

    internal static class CommentColors
    {
        public static readonly Color ImportantColor = Colors.Red;   //#INFO YELLOW BACKGROUND
        public static readonly Color QuestionColor = Colors.Yellow; //#INFO RED BACKGROUND
        public static readonly Color RemoveColor = Colors.Gray;    //#INFO GRAY BACKGROUND
        public static readonly Color TaskColor = Color.FromRgb(235, 105, 10); //#INFO ORANGE BACKGROUND

    }
}
