using NexusHighlight.Options;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;

namespace NexusHighlight.CommentsTagging
{
   internal class JavaScriptCommentParser : CommentParser
   {
      public override bool IsValidComment(SnapshotSpan span)
      {
         var txt = span.GetText();

         return (txt.StartsWith("//", OrdinalIgnoreCase) || txt.StartsWith("/*", OrdinalIgnoreCase));
      }

      public override Comment Parse(SnapshotSpan span)
      {
         // Just get enough length for GetCommentType() to work.
         var len = Settings.AllRules.Any() ? Settings.AllRules.Max(r => r.Criteria.Length) * 2 : 100;

         return base.Parse(new SnapshotSpan(span.Snapshot, span.Start, len));
      }

      protected override Comment SpecificParse(SnapshotSpan span, string criteria)
      {
         var spanText = span.GetText().ToLower();

         if (spanText.StartsWith("//", OrdinalIgnoreCase)) //! The comment span consists of a single line.
         {
            var fullSpan = ParseHelper.CompleteSingleLineCommentSpan(span, "//");

            spanText = fullSpan.GetText().ToLower();

            var startOffset = ParseHelper.SingleLineCommentStartIndex(spanText, "////", criteria);
            var spanLength = spanText.Length - startOffset;

            if (spanLength > 0)
            {
               return new Comment(
                   new[] { new SnapshotSpan(fullSpan.Snapshot, fullSpan.Start + startOffset, spanLength) },
                   criteria);
            }
         }
         else if (spanText.Contains("/*"))
         {
            var fullSpans = ParseHelper.CompleteDelimitedCommentSpan(span, "/*", "*/");

            if (fullSpans.Count == 1)
            {
               spanText = fullSpans[0].GetText().ToLower();
               var startOffset = ParseHelper.DelimitedCommentStartIndex(spanText, criteria);
               var closerIndex = spanText.IndexOf("*/", OrdinalIgnoreCase);
               var spanLength = spanText.IndexOfFirstCharReverse(closerIndex - 1) - (startOffset - 1);

               if (spanLength > 0)
               {
                  return new Comment(
                      new[] { new SnapshotSpan(fullSpans[0].Snapshot, fullSpans[0].Start + startOffset, spanLength) },
                      criteria);
               }
            }
         }

         return new Comment(new List<SnapshotSpan>(), criteria);
      }

      protected override string GetCommentCriteria(SnapshotSpan span)
      {
         var fullSpan = span.GetText().Contains("//")
                      ? ParseHelper.CompleteSingleLineCommentSpan(span, "//")
                      : ParseHelper.CompleteDelimitedCommentSpan(span, "/*", "*/").First();

         if (Settings.StrikethroughDoubleComments && fullSpan.GetText().StartsWith("////", OrdinalIgnoreCase))
            return "#REMOVE";

         return base.GetCommentCriteria(fullSpan);
      }

      protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
      {
         return span.GetText().Substring(2);
      }
   }
}