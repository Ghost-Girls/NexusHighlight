using BetterCommentsPlus.Options;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace BetterCommentsPlus.CommentsTagging
{
    internal class CSharpCommentParser : CommentParser //#TASK
    {
        public override bool IsValidComment(SnapshotSpan span)
        {
            var txt = span.GetText();

            return txt.StartsWith("//", OrdinalIgnoreCase)
                || txt.StartsWith("/*", OrdinalIgnoreCase);
        }

        protected override Comment SpecificParse(SnapshotSpan span, CommentCategory? commentCategory)
        {
            var spanText = span.GetText().ToLower();

            var commentSpans = new List<SnapshotSpan>();

            var firstLineNumber = span.Snapshot.GetLineFromPosition(span.Start).LineNumber;
            var lastLineNumber = span.Snapshot.GetLineFromPosition(span.End).LineNumber;

            if (firstLineNumber == lastLineNumber) //! The comment span consists of a single line.
            {
                var startOffset = ParseHelper.SingleLineCommentStartIndex(spanText, "////", commentCategory);
                var spanLength = 0;
                if (spanText.StartsWith("//", OrdinalIgnoreCase))
                {
                    spanLength = span.Length - startOffset;
                }
                else
                {
                    var closerIndex = spanText.IndexOf("*/", OrdinalIgnoreCase);
                    spanLength = spanText.IndexOfFirstCharReverse(closerIndex - 1) - (startOffset - 1);
                }

                if (spanLength > 0)
                    commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength));
            }
            else //! The comment spans multiple lines
            {
                var startOffset = ParseHelper.DelimitedCommentStartIndex(spanText, commentCategory);
                var token = Settings.Instance.GetTokenValue(commentCategory ?? CommentCategory.Normal);

                for (var curr = firstLineNumber; curr <= lastLineNumber; curr++)
                {
                    var line = span.Snapshot.GetLineFromLineNumber(curr);
                    var lineText = line.GetText().ToLower();

                    if (curr == firstLineNumber && lineText.Length > token.Length + 2) //! First line.
                    {
                        var index = lineText.IndexOf("/*", OrdinalIgnoreCase);
                        if (commentCategory == CommentCategory.Task)
                        {
                            startOffset = lineText.IndexOf(token, OrdinalIgnoreCase);
                        }
                        else
                        {
                            var indexOfToken = lineText.IndexOf(token, OrdinalIgnoreCase);
                            startOffset = lineText.IndexOfFirstChar(indexOfToken + token.Length);
                        }

                        commentSpans.Add(new SnapshotSpan(span.Snapshot, line.Start + startOffset, line.Length - startOffset));
                    }
                    else if (curr > firstLineNumber && curr < lastLineNumber) //! Line in the middle
                    {
                        if (!string.IsNullOrWhiteSpace(lineText))
                        {
                            startOffset = lineText.IndexOfFirstChar();
                            commentSpans.Add(new SnapshotSpan(span.Snapshot, line.Start + startOffset, line.Length - startOffset));
                        }
                    }
                    //! Last line . Handle it ONLY if it is more than just a comment ender.
                    else if (lineText.Contains("*/") && !lineText.Trim().StartsWith("*/", OrdinalIgnoreCase))
                    {
                        startOffset = lineText.IndexOfFirstChar();
                        var closerIndex = lineText.IndexOf("*/", OrdinalIgnoreCase);
                        var spanLength = lineText.IndexOfFirstCharReverse(closerIndex - 1) - startOffset + 1;

                        commentSpans.Add(new SnapshotSpan(span.Snapshot, line.Start + startOffset, spanLength));
                    }
                }
            }

            return new Comment(commentSpans, commentCategory);
        }

        protected override CommentCategory? GetCommentType(SnapshotSpan span)
        {
            if (Settings.StrikethroughDoubleComments && span.GetText().StartsWith("////", OrdinalIgnoreCase))
                return CommentCategory.Remove;

            return base.GetCommentType(span);
        }

        protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
        {
            return span.GetText().Substring(2);
        }
    }
}