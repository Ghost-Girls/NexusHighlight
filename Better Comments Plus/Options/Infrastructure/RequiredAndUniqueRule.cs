using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace BetterCommentsPlus.Options
{
   public class RequiredAndUniqueRule : ValidationRule
   {
      public bool IsGlobalScope { get; set; } = false;
      public bool IsSolutionScope { get; set; } = false;
      
      public RequiredAndUniqueRule()
      {
      }

      public override ValidationResult Validate(object value, CultureInfo cultureInfo)
      {
         var str = value as string;
         if (str == null)
         {
            return new ValidationResult(false, "Value is not a string.");
         }

         if (str.IndexOfAny(new[] { '|', ',', '/' }) > -1)
         {
            return new ValidationResult(false, "Rules can't contain any of the following characters | , /");
         }

         if (string.IsNullOrWhiteSpace(str))
         {
            return new ValidationResult(false, "Value is required.");
         }

         // 根据作用域检查唯一性
         if (IsGlobalScope)
         {
            // 只检查 Global 集合内部是否唯一
            var count = Settings.Instance.GlobalRules.Count(t => t.Criteria.Equals(str));
            if (count > 1)
            {
               return new ValidationResult(false, "Value must be unique within Global Rules.");
            }
         }
         else if (IsSolutionScope)
         {
            // 只检查 Solution 集合内部是否唯一
            var count = Settings.Instance.SolutionRules.Count(t => t.Criteria.Equals(str));
            if (count > 1)
            {
               return new ValidationResult(false, "Value must be unique within Solution Rules.");
            }
         }
         else
        {
            // 默认行为：同时检查两个集合（用于 ValidateRules 方法）
            var globalCount = Settings.Instance.GlobalRules.Count(t => t.Criteria.Equals(str));
            var solutionCount = Settings.Instance.SolutionRules.Count(t => t.Criteria.Equals(str));
            
            if (globalCount > 1 || solutionCount > 1)
            {
               return new ValidationResult(false, "Value must be unique within each scope.");
            }
         }

         return ValidationResult.ValidResult;
      }

      private object GetBoundValue(object value)
      {
         if (value is BindingExpression be)
         {
            return be.DataItem
                     .GetType()
                     .GetProperty(be.ParentBinding.Path.Path)
                     .GetValue(be.DataItem, null);
         }
         else
         {
            return value;
         }
      }
   }
}