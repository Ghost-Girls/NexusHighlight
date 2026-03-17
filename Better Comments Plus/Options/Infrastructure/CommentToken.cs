using BetterCommentsPlus.CommentsTagging;
using System.Windows.Media;

namespace BetterCommentsPlus.Options
{
   public class CommentToken : PropertyChangeNotifier
   {
      private CommentType type;
      private string ruleId;
      private bool isDynamic;
      private string defaultValue;
      private string currentValue;
      private string colorHex;
      private bool isBold;
      private bool isItalic;
      private bool hasUnderline;
      private bool hasStrikethrough;

      public CommentType Type
      {
         get { return type; }
         set { SetField(ref type, value); }
      }

      public string RuleId
      {
         get { return ruleId; }
         set { SetField(ref ruleId, value); }
      }

      public bool IsDynamic
      {
         get { return isDynamic; }
         set { SetField(ref isDynamic, value); }
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

      public bool IsBold
      {
         get { return isBold; }
         set { SetField(ref isBold, value); }
      }

      public bool IsItalic
      {
         get { return isItalic; }
         set { SetField(ref isItalic, value); }
      }

      public bool HasUnderline
      {
         get { return hasUnderline; }
         set { SetField(ref hasUnderline, value); }
      }

      public bool HasStrikethrough
      {
         get { return hasStrikethrough; }
         set { SetField(ref hasStrikethrough, value); }
      }

      public CommentToken(CommentType type, string defaultValue, string value, string colorHex = null)
      {
         this.type = type;
         this.ruleId = $"comment-{type.ToString().ToLower()}";
         this.isDynamic = false;
         this.defaultValue = defaultValue;
         this.currentValue = value;
         this.colorHex = colorHex;
         this.isBold = false;
         this.isItalic = true;
         this.hasUnderline = false;
         this.hasStrikethrough = false;
      }

      public bool IsOfType(string type)
      {
         return type == null ? false : Type.ToString().Equals(type.Trim());
      }

      public override string ToString()
      {
         return $"{type},{currentValue.Trim()},{colorHex},{isBold},{isItalic},{hasUnderline},{hasStrikethrough}";
      }
   }
}