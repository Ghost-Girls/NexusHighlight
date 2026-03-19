using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using BetterCommentsPlus.Options;

namespace BetterCommentsPlus.CommentsTagging
{
   internal class Comment
   {
      public CommentCategory? Category { get; private set; }
      public string RuleId { get; private set; }
      public string Criteria { get; private set; }
      public IEnumerable<SnapshotSpan> Spans { get; private set; }

      public Comment(IEnumerable<SnapshotSpan> spans, CommentCategory? category)
      {
         Spans = spans;
         Category = category;
         RuleId = null;
         Criteria = null;
      }

      public Comment(SnapshotSpan span, CommentCategory? category)
      {
         Spans = new List<SnapshotSpan>(new[] { span });
         Category = category;
         RuleId = null;
         Criteria = null;
      }

      public Comment(IEnumerable<SnapshotSpan> spans, string ruleId, string criteria)
      {
         Spans = spans;
         Category = null;
         RuleId = ruleId;
         Criteria = criteria;
      }

      public Comment(SnapshotSpan span, string ruleId, string criteria)
      {
         Spans = new List<SnapshotSpan>(new[] { span });
         Category = null;
         RuleId = ruleId;
         Criteria = criteria;
      }
   }
}