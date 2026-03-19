using System;

namespace BetterCommentsPlus.Options
{
    /// <summary>
    /// 配置迁移工具 - 用于将旧的数据结构迁移到新的 CommentRule
    /// </summary>
    public static class ConfigMigrator
    {
        /// <summary>
        /// 从 CommentToken 迁移到 CommentRule
        /// </summary>
        public static CommentRule MigrateFromCommentToken(CommentToken token)
        {
            if (token == null) return null;

            var rule = new CommentRule
            {
                Id = token.RuleId,
                Criteria = token.CurrentValue,  // CurrentValue → Criteria
                Category = (CommentCategory?)token.Type,  // 显式转换
                IsPredefined = !token.IsDynamic,
                IsActive = true,
                Order = 0,
                Scope = RuleScope.Global // 需要根据上下文判断
            };

            // 迁移前景样式
            if (rule.Foreground != null)
            {
                rule.Foreground.ColorHex = token.ColorHex;
                rule.Foreground.IsBold = token.IsBold;
                rule.Foreground.IsItalic = token.IsItalic;
                rule.Foreground.HasUnderline = token.HasUnderline;
                rule.Foreground.HasStrikethrough = token.HasStrikethrough;
                rule.Foreground.IsActive = token.IsForegroundActive;
            }

            // 迁移背景样式
            if (rule.Background != null)
            {
                rule.Background.ColorHex = token.BackgroundStyle?.ColorHex;
                rule.Background.Shape = token.BackgroundStyle?.Shape;
                rule.Background.Blur = token.BackgroundStyle?.Blur;
                rule.Background.Alpha = token.BackgroundStyle?.Alpha;
                rule.Background.IsActive = token.BackgroundStyle?.IsActive ?? false;
                rule.Background.IsCaseSensitive = token.BackgroundStyle?.IsCaseSensitive ?? true;
                rule.Background.AllowPartialMatch = token.BackgroundStyle?.AllowPartialMatch ?? false;
            }

            return rule;
        }

        /// <summary>
        /// 从 StyleRule 迁移到 CommentRule
        /// </summary>
        public static CommentRule MigrateFromStyleRule(StyleRule rule)
        {
            if (rule == null) return null;

            var newRule = new CommentRule
            {
                Id = rule.Id,
                Criteria = rule.Criteria,  // 保持不变
                IsPredefined = rule.IsPredefined,
                IsActive = rule.IsActive,
                Order = rule.Order,
                Scope = RuleScope.Global // 需要根据上下文判断
            };

            // 迁移前景样式
            if (newRule.Foreground != null && rule.Foreground != null)
            {
                newRule.Foreground.ColorHex = rule.Foreground.ColorHex;
                newRule.Foreground.IsBold = rule.Foreground.IsBold;
                newRule.Foreground.IsItalic = rule.Foreground.IsItalic;
                newRule.Foreground.HasUnderline = rule.Foreground.HasUnderline;
                newRule.Foreground.HasStrikethrough = rule.Foreground.HasStrikethrough;
                newRule.Foreground.IsActive = rule.Foreground.IsActive;
            }

            // 迁移背景样式
            if (newRule.Background != null && rule.Background != null)
            {
                newRule.Background.ColorHex = rule.Background.ColorHex;
                newRule.Background.Shape = rule.Background.Shape;
                newRule.Background.Blur = rule.Background.Blur;
                newRule.Background.Alpha = rule.Background.Alpha;
                newRule.Background.IsActive = rule.Background.IsActive;
                newRule.Background.IsCaseSensitive = rule.Background.IsCaseSensitive;
                newRule.Background.AllowPartialMatch = rule.Background.AllowPartialMatch;
            }

            return newRule;
        }

        /// <summary>
        /// 从 CommentRule 迁移回 CommentToken（向后兼容）
        /// </summary>
        public static CommentToken MigrateToCommentToken(CommentRule rule)
        {
            if (rule == null) return null;

            var token = new CommentToken(
                type: (CommentsTagging.CommentType)(rule.Category ?? Options.CommentCategory.Normal),  // 显式转换
                defaultValue: rule.Criteria,
                value: rule.Criteria,
                colorHex: rule.ColorHex
            )
            {
                RuleId = rule.Id,
                IsDynamic = !rule.IsPredefined,
                IsBold = rule.IsBold ?? false,
                IsItalic = rule.IsItalic ?? false,
                HasUnderline = rule.HasUnderline ?? false,
                HasStrikethrough = rule.HasStrikethrough ?? false,
                IsForegroundActive = rule.IsForegroundActive ?? true
            };

            // 迁移背景样式
            if (rule.Background != null)
            {
                token.BackgroundStyle = new BackgroundStyle
                {
                    ColorHex = rule.BackgroundColorHex,
                    Shape = rule.Shape,
                    Blur = rule.Blur,
                    Alpha = rule.Alpha,
                    IsActive = rule.IsBackgroundActive ?? false,
                    IsCaseSensitive = rule.IsCaseSensitive ?? true,
                    AllowPartialMatch = rule.AllowPartialMatch ?? false
                };
            }

            return token;
        }
    }
}
