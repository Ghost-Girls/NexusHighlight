using Microsoft.VisualStudio.Text;

namespace BetterCommentsPlus.CommentsTagging
{
   internal interface ICommentParser
   {
      Comment Parse(SnapshotSpan span);

      bool IsValidComment(SnapshotSpan span);
   }
}