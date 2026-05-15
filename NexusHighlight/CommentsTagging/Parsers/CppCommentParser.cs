using NexusHighlight.Options;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;

namespace NexusHighlight.CommentsTagging
{
    internal class CppCommentParser : CommentParser
    {
        public override bool IsValidComment(SnapshotSpan span) //#TASK: This method is used to check if the comment is valid or not.
        {
            var txt = span.GetText();

            return txt.StartsWith("//", OrdinalIgnoreCase) || txt.StartsWith("/*", OrdinalIgnoreCase);
        }

        protected override Comment SpecificParse(SnapshotSpan span, string criteria)
        {
            var spanText = span.GetText().ToLower();

            var commentSpans = new List<SnapshotSpan>();

            var startOffset = ParseHelper.SingleLineCommentStartIndex(spanText, "////", criteria);

            // single line comment
            if (spanText.StartsWith("//", OrdinalIgnoreCase))
            {
                commentSpans.Add(
                   new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset));
            }
            // delimited comment in a single line
            else if (spanText.StartsWith("/*", OrdinalIgnoreCase)
                  && spanText.EndsWith("*/", OrdinalIgnoreCase)
                  && spanText.Length > 5)
            {
                startOffset = ParseHelper.DelimitedCommentStartIndex(spanText, criteria);

                var indexOfStarter = spanText.IndexOf("*/", OrdinalIgnoreCase);
                var spanLength = spanText.IndexOfFirstCharReverse(indexOfStarter - 1) - (startOffset - 1);

                if (spanLength > 0)
                    commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength));
            }

            return new Comment(commentSpans, criteria);
        }

        protected override string GetCommentCriteria(SnapshotSpan span)
        {
			// #HACK 
			//if (Settings.StrikethroughDoubleComments && span.GetText().StartsWith("//x", OrdinalIgnoreCase))
			//	return "#X";

			if (Settings.StrikethroughDoubleComments && span.GetText().StartsWith("////", OrdinalIgnoreCase))
                return "#REMOVE";

            return base.GetCommentCriteria(span);
        }

        protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
        {
            return span.GetText().Substring(2);
        }
    }
}
