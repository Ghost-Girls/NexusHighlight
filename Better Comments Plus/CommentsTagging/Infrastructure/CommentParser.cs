using BetterCommentsPlus.Options;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterCommentsPlus.CommentsTagging
{
    internal abstract class CommentParser : ICommentParser
    {
        protected readonly StringComparison OrdinalIgnoreCase = StringComparison.OrdinalIgnoreCase;

        protected readonly Options.Settings Settings = Options.Settings.Instance;

        #region ICommentParser Members

        public virtual Comment Parse(SnapshotSpan span)
        {
            var commentInfo = GetCommentInfo(span);

            if (commentInfo.Rule == null)
                return new Comment(new List<SnapshotSpan> { span }, CommentCategory.Normal);

            if (!commentInfo.Rule.IsPredefined)
            {
                var commentSpans = new List<SnapshotSpan>();
                var firstLineNumber = span.Snapshot.GetLineFromPosition(span.Start).LineNumber;
                var lastLineNumber = span.Snapshot.GetLineFromPosition(span.End).LineNumber;
                var spanText = span.GetText().ToLower();
                var criteria = commentInfo.Rule.Criteria;
                var criteriaLower = criteria.ToLower();

                if (firstLineNumber == lastLineNumber)
                {
                    var startOffset = 0;
                    if (spanText.StartsWith("//", StringComparison.OrdinalIgnoreCase))
                    {
                        startOffset = spanText.IndexOf(criteriaLower, StringComparison.OrdinalIgnoreCase);
                        if (startOffset < 0) startOffset = 2;
                    }
                    else if (spanText.StartsWith("/*", StringComparison.OrdinalIgnoreCase))
                    {
                        startOffset = spanText.IndexOf(criteriaLower, StringComparison.OrdinalIgnoreCase);
                        if (startOffset < 0) startOffset = 2;
                    }
                    
                    int actualStartOffset = Settings.HighlightCriteriaItself 
                        ? startOffset 
                        : spanText.IndexOfFirstChar(startOffset + criteria.Length);
                    
                    var spanLength = span.Length - actualStartOffset;
                    if (spanLength > 0)
                        commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + actualStartOffset, spanLength));
                }
                else
                {
                    for (var curr = firstLineNumber; curr <= lastLineNumber; curr++)
                    {
                        var line = span.Snapshot.GetLineFromLineNumber(curr);
                        var lineText = line.GetText().ToLower();

                        if (curr == firstLineNumber)
                        {
                            var startOffset = lineText.IndexOf(criteriaLower, StringComparison.OrdinalIgnoreCase);
                            if (startOffset < 0) startOffset = lineText.IndexOfFirstChar();
                            
                            int actualStartOffset = Settings.HighlightCriteriaItself 
                                ? startOffset 
                                : lineText.IndexOfFirstChar(startOffset + criteria.Length);
                            
                            commentSpans.Add(new SnapshotSpan(span.Snapshot, line.Start + actualStartOffset, line.Length - actualStartOffset));
                        }
                        else if (curr > firstLineNumber && curr < lastLineNumber)
                        {
                            if (!string.IsNullOrWhiteSpace(lineText))
                            {
                                var startOffset = lineText.IndexOfFirstChar();
                                commentSpans.Add(new SnapshotSpan(span.Snapshot, line.Start + startOffset, line.Length - startOffset));
                            }
                        }
                        else if (lineText.Contains("*/") && !lineText.Trim().StartsWith("*/", StringComparison.OrdinalIgnoreCase))
                        {
                            var startOffset = lineText.IndexOfFirstChar();
                            var closerIndex = lineText.IndexOf("*/", StringComparison.OrdinalIgnoreCase);
                            var spanLength = lineText.IndexOfFirstCharReverse(closerIndex - 1) - startOffset + 1;

                            commentSpans.Add(new SnapshotSpan(span.Snapshot, line.Start + startOffset, spanLength));
                        }
                    }
                }

                return new Comment(commentSpans, commentInfo.Rule.Id, commentInfo.Rule.Criteria);
            }

            var commentCategory = commentInfo.Rule.Category;

            // Color only the "Todo" keyword.
            if (Settings.HighlightTaskKeywordOnly && commentCategory == CommentCategory.Task)
            {
                var spanText = span.GetText().ToLower();
                var token = Settings.GetTokenValue(CommentCategory.Task);
                var start = spanText.IndexOf(token, StringComparison.OrdinalIgnoreCase);

                return new Comment(
                    new List<SnapshotSpan> { new SnapshotSpan(span.Snapshot, span.Start + start, token.Length) },
                    CommentCategory.Task);
            }

            return SpecificParse(span, commentCategory);
        }

        public abstract bool IsValidComment(SnapshotSpan span);

        #endregion ICommentParser Members

        protected struct CommentInfo
        {
            public CommentRule Rule;
            public int MatchStart;
            public int MatchLength;
        }

        protected virtual CommentInfo GetCommentInfo(SnapshotSpan span)
        {
            var commentText = SpanTextWithoutCommentStarter(span).ToLower().Trim();
            
            // 优先查找 Solution Rules，然后是 Global Rules
            foreach (var rule in Settings.SolutionRules.Concat(Settings.GlobalRules).OrderBy(r => r.IsPredefined ? 1 : 0))
            {
                var criteria = rule.Criteria.ToLower();
                if (commentText.StartsWith(criteria, StringComparison.OrdinalIgnoreCase))
                {
                    return new CommentInfo
                    {
                        Rule = rule,
                        MatchStart = 0,
                        MatchLength = criteria.Length
                    };
                }
            }

            return new CommentInfo { Rule = null };
        }

        protected virtual CommentCategory? GetCommentType(SnapshotSpan span)
        {
            var commentInfo = GetCommentInfo(span);
            return commentInfo.Rule?.Category ?? CommentCategory.Normal;
        }

        protected abstract Comment SpecificParse(SnapshotSpan span, CommentCategory? commentCategory);

        protected abstract string SpanTextWithoutCommentStarter(SnapshotSpan span);
    }
}
