using Microsoft.VisualStudio.Text;

namespace NexusHighlight.CommentsTagging
{
   internal interface ICommentParser
   {
      Comment Parse(SnapshotSpan span);

      bool IsValidComment(SnapshotSpan span);
   }
}