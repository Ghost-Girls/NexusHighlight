using BetterCommentsPlus.Options;
using Microsoft.VisualStudio.Text;

namespace BetterCommentsPlus.CommentsTagging
{
   /// <summary>
   /// Handles HTML and XAML comments
   /// </summary>
   internal class MarkupCommentParser : CommentParser
   {
      public override bool IsValidComment(SnapshotSpan span)
      {
         var txt = span.GetText();
         return !txt.Contains("\r\n") && txt.Contains("<!--") && txt.Contains("-->");
      }

      protected override Comment SpecificParse(SnapshotSpan span, string criteria)
      {
         var spanText = span.GetText().ToLower();

         var rule = Settings.Instance.GetRuleValue(criteria);

         var startOffset = (criteria.ToUpper() == "#TASK")
                         ? spanText.IndexOf(rule, 3, OrdinalIgnoreCase)
                         : spanText.IndexOfFirstChar(spanText.IndexOf(rule, 3, OrdinalIgnoreCase) + rule.Length);

         var closerIndex = spanText.IndexOf("-->", OrdinalIgnoreCase);
         var spanLength = spanText.IndexOfFirstCharReverse(closerIndex - 1) - (startOffset - 1);

         return new Comment(
             new[] { new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength) },
             criteria);
      }

      protected override string GetCommentCriteria(SnapshotSpan span)
      {
         return base.GetCommentCriteria(span);
      }

      protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
      {
         return span.GetText().Substring(4);
      }
   }
}