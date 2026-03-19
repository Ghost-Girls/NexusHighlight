using BetterCommentsPlus.Options;
using Microsoft.VisualStudio.Text;

namespace BetterCommentsPlus.CommentsTagging
{
   internal class PythonCommentParser : CommentParser
   {
      public override bool IsValidComment(SnapshotSpan span)
      {
         return span.GetText().Trim().StartsWith("#", OrdinalIgnoreCase);
      }

      protected override Comment SpecificParse(SnapshotSpan span, string criteria)
      {
         var spanText = span.GetText().ToLower();
         var startOffset = ParseHelper.SingleLineCommentStartIndex(spanText, "##", criteria);

         return new Comment(
             new[] { new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset) },
             criteria);
      }

      protected override string GetCommentCriteria(SnapshotSpan span)
      {
         if (Settings.StrikethroughDoubleComments && span.GetText().StartsWith("##", OrdinalIgnoreCase))
            return "#REMOVE";

         return base.GetCommentCriteria(span);
      }

      protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
      {
         return span.GetText().Substring(1);
      }
   }
}