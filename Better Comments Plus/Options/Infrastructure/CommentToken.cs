using BetterCommentsPlus.CommentsTagging;
using System.Windows.Media;

namespace BetterCommentsPlus.Options
{
   public class CommentToken : PropertyChangeNotifier
   {
      private CommentType type;
      private string defaultValue;
      private string currentValue;
      private string colorHex;

      public CommentType Type
      {
         get { return type; }
         set { SetField(ref type, value); }
      }

      public string CurrentValue
      {
         get { return currentValue; }
         set { SetField(ref currentValue, value); }
      }

      public string DefaultValue
      {
         get { return defaultValue; }
      }

      public string ColorHex
      {
         get { return colorHex; }
         set { SetField(ref colorHex, value); }
      }

      public CommentToken(CommentType type, string defaultValue, string value, string colorHex = null)
      {
         this.type = type;
         this.defaultValue = defaultValue;
         this.currentValue = value;
         this.colorHex = colorHex;
      }

      public bool IsOfType(string type)
      {
         return type == null ? false : Type.ToString().Equals(type.Trim());
      }

      public override string ToString()
      {
         return $"{type},{currentValue.Trim()},{colorHex}";
      }
   }
}