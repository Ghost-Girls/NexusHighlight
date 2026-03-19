using BetterCommentsPlus.Options;
using Microsoft.VisualStudio.Text;

namespace BetterCommentsPlus.CommentsTagging
{
   internal class VBCommentParser : CommentParser
   {
      public override bool IsValidComment(SnapshotSpan span)
      {
         return span.GetText().Trim().StartsWith("'", OrdinalIgnoreCase);
      }

      protected override Comment SpecificParse(SnapshotSpan span, CommentCategory? commentCategory)
      {
         var spanText = span.GetText().ToLower();
         var startOffset = ParseHelper.SingleLineCommentStartIndex(spanText, "''", commentCategory);

         return new Comment(
             new[] { new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset) },
             commentCategory);
      }

      protected override CommentCategory? GetCommentType(SnapshotSpan span)
      {
         if (Settings.StrikethroughDoubleComments && span.GetText().StartsWith("''", OrdinalIgnoreCase))
            return CommentCategory.Remove;

         return base.GetCommentType(span);
      }

      protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
      {
         return span.GetText().Substring(1);
      }
   }
}