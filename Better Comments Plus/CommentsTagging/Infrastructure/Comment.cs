using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using BetterCommentsPlus.Options;

namespace BetterCommentsPlus.CommentsTagging
{
   internal class Comment
   {
      public string RuleId { get; private set; }
      public string Criteria { get; private set; }
      public IEnumerable<SnapshotSpan> Spans { get; private set; }

      public Comment(IEnumerable<SnapshotSpan> spans, string criteria)
      {
         Spans = spans;
         Criteria = criteria;
         RuleId = null;
      }

      public Comment(SnapshotSpan span, string criteria)
      {
         Spans = new List<SnapshotSpan>(new[] { span });
         Criteria = criteria;
         RuleId = null;
      }

      public Comment(IEnumerable<SnapshotSpan> spans, string ruleId, string criteria)
      {
         Spans = spans;
         RuleId = ruleId;
         Criteria = criteria;
      }

      public Comment(SnapshotSpan span, string ruleId, string criteria)
      {
         Spans = new List<SnapshotSpan>(new[] { span });
         RuleId = ruleId;
         Criteria = criteria;
      }
   }
}