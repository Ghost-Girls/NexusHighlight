using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace BetterCommentsPlus.CommentsTagging
{
   internal class Comment
   {
      public CommentType Type { get; private set; }
      public string RuleId { get; private set; }
      public string Criteria { get; private set; }
      public IEnumerable<SnapshotSpan> Spans { get; private set; }

      public Comment(IEnumerable<SnapshotSpan> spans, CommentType type)
      {
         Spans = spans;
         Type = type;
         RuleId = null;
         Criteria = null;
      }

      public Comment(SnapshotSpan span, CommentType type)
      {
         Spans = new List<SnapshotSpan>(new[] { span });
         Type = type;
         RuleId = null;
         Criteria = null;
      }

      public Comment(IEnumerable<SnapshotSpan> spans, string ruleId, string criteria)
      {
         Spans = spans;
         Type = CommentType.Normal;
         RuleId = ruleId;
         Criteria = criteria;
      }

      public Comment(SnapshotSpan span, string ruleId, string criteria)
      {
         Spans = new List<SnapshotSpan>(new[] { span });
         Type = CommentType.Normal;
         RuleId = ruleId;
         Criteria = criteria;
      }
   }
}