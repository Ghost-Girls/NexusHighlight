using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace NexusHighlight.Options
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

         // 尝试获取绑定的 CommentRule 对象，以便在检查唯一性时排除当前正在编辑的规则
         var boundValue = GetBoundValue(value);
         CommentRule currentRule = null;
         if (boundValue is CommentRule rule)
         {
            currentRule = rule;
         }

         // 根据作用域检查唯一性
         if (IsGlobalScope)
         {
            // 只检查 Global 集合内部是否唯一，排除当前正在编辑的规则
            var count = Settings.Instance.GlobalRules.Count(t => t.Criteria.Equals(str) && t != currentRule);
            if (count > 0)
            {
               return new ValidationResult(false, "Value must be unique within Global Rules.");
            }
         }
         else if (IsSolutionScope)
         {
            // 只检查 Solution 集合内部是否唯一，排除当前正在编辑的规则
            var count = Settings.Instance.SolutionRules.Count(t => t.Criteria.Equals(str) && t != currentRule);
            if (count > 0)
            {
               return new ValidationResult(false, "Value must be unique within Solution Rules.");
            }
         }
         else
         {
            // 默认行为：同时检查两个集合（用于 ValidateRules 方法）
            // 在这种情况下，我们需要以不同方式检查：
            // 对于每个集合，检查是否有多个相同的规则（不排除当前规则，因为我们不知道哪一个是当前规则）
            var globalDuplicates = Settings.Instance.GlobalRules
                .GroupBy(t => t.Criteria)
                .Any(g => g.Key.Equals(str) && g.Count() > 1);
            
            var solutionDuplicates = Settings.Instance.SolutionRules
                .GroupBy(t => t.Criteria)
                .Any(g => g.Key.Equals(str) && g.Count() > 1);
            
            if (globalDuplicates || solutionDuplicates)
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
            // 返回 DataItem 本身，而不是其属性值
            // 这样我们就能获取到当前正在编辑的 CommentRule 对象
            return be.DataItem;
         }
         else
         {
            return value;
         }
      }
   }
}