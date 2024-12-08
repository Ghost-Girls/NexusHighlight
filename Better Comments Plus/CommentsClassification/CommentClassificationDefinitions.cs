using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace BetterCommentsPlus.CommentsClassification
{
    public static class CommentClassificationDefinitions
    {
#pragma warning disable 0649

        [Export]
        [Name(CommentNames.IMPORTANT_COMMENT)]
        [BaseDefinition("Comment")]
        internal static ClassificationTypeDefinition ImportantCommentDefinition;

        [Export]
        [Name(CommentNames.QUESTION_COMMENT)]
        [BaseDefinition("Comment")]
        internal static ClassificationTypeDefinition QuestionCommentDefinition;

        [Export]
        [Name(CommentNames.REMOVE_COMMENT)]
        [BaseDefinition("Comment")]
        internal static ClassificationTypeDefinition RemoveCommentDefinition;

        [Export]
        [Name(CommentNames.TASK_COMMENT)]
        [BaseDefinition("Comment")]
        internal static ClassificationTypeDefinition TaskCommentDefinition;



#pragma warning restore 0649
    }
}
