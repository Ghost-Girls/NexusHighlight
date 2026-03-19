using BetterCommentsPlus.CommentsClassification;
using BetterCommentsPlus.CommentsTagging;
using BetterCommentsPlus.Options;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TextFormattingRunProperties = Microsoft.VisualStudio.Text.Formatting.TextFormattingRunProperties;

namespace BetterCommentsPlus.CommentsViewCustomization
{
   internal sealed class CommentViewDecorator
   {
      private bool isDecorating;
      private readonly IClassificationFormatMap formatMap;
      private readonly IClassificationTypeRegistryService regService;

      private readonly Options.Settings settings = Options.Settings.Instance;

      private static readonly List<string> commentTypes = new List<string>()
            {
                "comment",
                "xml doc comment",
                "vb xml doc comment",
                "xml comment",
                "html comment",
                "xaml comment",
            };

      public static CommentViewDecorator Create(ITextView view, IClassificationFormatMap map,
                                                IClassificationTypeRegistryService service)
      {
         return view.Properties.GetOrCreateSingletonProperty(() => new CommentViewDecorator(view, map, service));
      }

      private CommentViewDecorator(ITextView view, IClassificationFormatMap map,
                                   IClassificationTypeRegistryService service)
      {
         view.GotAggregateFocus += TextView_GotAggregateFocus;

         SettingsStore.SettingsSaved += OnSettingsSaved;
         settings.ConfigurationChanged += OnConfigurationChanged;

         formatMap = map;
         regService = service;

         Decorate();
      }

      private void OnSettingsSaved()
      {
         if (!isDecorating)
            Decorate();
      }

      private void OnConfigurationChanged(object sender, EventArgs e)
      {
         if (!isDecorating)
            Decorate();
      }

      private void TextView_GotAggregateFocus(object sender, EventArgs e)
      {
         if (sender is ITextView view)
            view.GotAggregateFocus -= TextView_GotAggregateFocus;

         if (!isDecorating)
            Decorate();
      }

      private void Decorate()
      {
         try
         {
            isDecorating = true;
            DecorateKnownClassificationTypes();
            DecorateUnknowClassificationTypes();
            DecorateDynamicRules();
         }
         catch (Exception ex)
         {
            //TODO: Handle the exception gracefully.
            Debug.Assert(false, "Exception while formatting! \n", ex.Message);
         }
         finally
         {
            isDecorating = false;
         }
      }

      private void DecorateDynamicRules()
      {
         // 创建一个字典来跟踪已经应用过的 Criteria
         // Solution Rules 优先级高于 Global Rules，所以先记录 Solution 的 Criteria
         var appliedCriteria = new HashSet<string>();
         
         // 先应用 Solution Rules
         foreach (var rule in settings.SolutionRules)
         {
            if (!string.IsNullOrEmpty(rule.Id))
            {
               appliedCriteria.Add(rule.Criteria);
               
               var classificationType = regService.GetClassificationType(rule.Id);
               if (classificationType == null)
               {
                  classificationType = regService.CreateClassificationType(
                      rule.Id,
                      new[] { regService.GetClassificationType("comment") });
               }
               SetProperties(classificationType);
            }
         }
         
         // 再应用 Global Rules，但跳过已经在 Solution 中应用过的 Criteria
         foreach (var rule in settings.GlobalRules)
         {
            if (!string.IsNullOrEmpty(rule.Id) && !appliedCriteria.Contains(rule.Criteria))
            {
               var classificationType = regService.GetClassificationType(rule.Id);
               if (classificationType == null)
               {
                  classificationType = regService.CreateClassificationType(
                      rule.Id,
                      new[] { regService.GetClassificationType("comment") });
               }
               SetProperties(classificationType);
            }
         }
      }

      private void DecorateKnownClassificationTypes()
      {
         var knowns = commentTypes.Select(type => regService.GetClassificationType(type))
                                  .Where(type => type != null);

         foreach (var classificationType in knowns)
            SetProperties(classificationType);
      }

      private void DecorateUnknowClassificationTypes()
      {
         var unknowns = from type in formatMap.CurrentPriorityOrder.Where(type => type != null)
                        let name = type.Classification.ToLowerInvariant()
                        where name.Contains("comment") && !commentTypes.Contains(name)
                        select type;

         foreach (var classificationType in unknowns)
            SetProperties(classificationType);
      }

      private void SetProperties(IClassificationType classificationType)
      {
         //? Might need to benchmark this function for performance.

         var properties = formatMap.GetTextProperties(classificationType);
         var fontSize = GetEditorTextSize() + settings.Size;

         if (!string.IsNullOrWhiteSpace(settings.Font))
            properties = properties.SetTypeface(new Typeface(settings.Font));

         if (Math.Abs(fontSize - properties.FontRenderingEmSize) > 0)
            properties = properties.SetFontRenderingEmSize(fontSize);

         if (properties.Italic != settings.Italic)
            properties = properties.SetItalic(settings.Italic);

         if (classificationType.IsOfType(CommentNames.IMPORTANT_COMMENT)) //#INFO
            properties = properties.SetTextDecorations(GetTextDecoration(settings));

         // 添加动态颜色设置
         properties = SetDynamicColor(classificationType, properties);

         formatMap.SetTextProperties(classificationType, properties);
      }

      private TextFormattingRunProperties SetDynamicColor(IClassificationType classificationType, TextFormattingRunProperties properties)
      {
         try
         {
            CommentCategory? commentCategory = null;
            string criteria = null;
            string ruleId = classificationType.Classification;

            if (classificationType.IsOfType(CommentNames.IMPORTANT_COMMENT))
            {
               commentCategory = CommentCategory.Important;
               criteria = "#IMPORTANT";
            }
            else if (classificationType.IsOfType(CommentNames.QUESTION_COMMENT))
            {
               commentCategory = CommentCategory.Question;
               criteria = "#QUESTION";
            }
            else if (classificationType.IsOfType(CommentNames.REMOVE_COMMENT))
            {
               commentCategory = CommentCategory.Remove;
               criteria = "#REMOVE";
            }
            else if (classificationType.IsOfType(CommentNames.TASK_COMMENT))
            {
               commentCategory = CommentCategory.Task;
               criteria = "#TASK";
            }

            string colorHex = null;
            bool? isBold = null;
            bool? isItalic = null;
            bool? hasUnderline = null;
            bool? hasStrikethrough = null;
            bool? isForegroundActive = null;
            BackgroundStyle background = null;

            // 优先查找 Solution Rules 中是否有匹配的 Criteria
            // Solution Rules 优先级高于 Global Rules
            CommentRule matchingRule = null;
            
            // 先在 Solution Rules 中查找
            if (!string.IsNullOrEmpty(criteria))
            {
               matchingRule = settings.SolutionRules.FirstOrDefault(r => r.Criteria == criteria);
            }
            
            // 如果 Solution Rules 中没有，再在 Global Rules 中查找
            if (matchingRule == null && !string.IsNullOrEmpty(criteria))
            {
               matchingRule = settings.GlobalRules.FirstOrDefault(r => r.Criteria == criteria);
            }
            
            // 如果还是没有找到，尝试通过 ruleId 查找（向后兼容）
            if (matchingRule == null)
            {
               matchingRule = settings.AllRules.FirstOrDefault(r => r.Id == ruleId);
            }
            
            // 如果还是没找到，尝试通过 commentCategory 查找预设规则
            if (matchingRule == null && commentCategory.HasValue)
            {
               matchingRule = settings.GetRule(commentCategory.Value);
            }

            // 应用找到的 Rule 的样式
            if (matchingRule != null)
            {
               if (!string.IsNullOrEmpty(matchingRule.ColorHex))
               {
                  colorHex = matchingRule.ColorHex;
               }
               isBold = matchingRule.IsBold;
               isItalic = matchingRule.IsItalic;
               hasUnderline = matchingRule.HasUnderline;
               hasStrikethrough = matchingRule.HasStrikethrough;
               isForegroundActive = matchingRule.IsForegroundActive;
               background = matchingRule.Background;
            }

            bool shouldApplyForeground = isForegroundActive.GetValueOrDefault(true);

            if (shouldApplyForeground)
            {
               if (!string.IsNullOrEmpty(colorHex))
               {
                  try
                  {
                     var color = (Color)ColorConverter.ConvertFromString(colorHex);
                     properties = properties.SetForeground(color);
                  }
                  catch { }
               }

               if (settings.Opacity >= 0.1 && settings.Opacity <= 1)
               {
                  properties = properties.SetForegroundOpacity(settings.Opacity);
               }

               if (isBold.HasValue)
               {
                  properties = properties.SetBold(isBold.Value);
               }

               if (isItalic.HasValue)
               {
                  properties = properties.SetItalic(isItalic.Value);
               }

               var decorations = new TextDecorationCollection();
               if (hasUnderline.GetValueOrDefault())
               {
                  decorations.Add(TextDecorations.Underline);
               }
               if (hasStrikethrough.GetValueOrDefault())
               {
                  decorations.Add(TextDecorations.Strikethrough);
               }
               if (decorations.Count > 0)
               {
                  properties = properties.SetTextDecorations(decorations);
               }
            }
            else
            {
               var defaultCommentType = regService.GetClassificationType("comment");
               if (defaultCommentType != null)
               {
                  var defaultProperties = formatMap.GetTextProperties(defaultCommentType);
                  if (defaultProperties.ForegroundBrush is SolidColorBrush solidColorBrush)
                  {
                     properties = properties.SetForeground(solidColorBrush.Color);
                  }
               }

               properties = properties.SetForegroundOpacity(1.0);

               if (isBold.HasValue)
               {
                  properties = properties.SetBold(false);
               }

               if (isItalic.HasValue)
               {
                  properties = properties.SetItalic(false);
               }

               properties = properties.SetTextDecorations(new TextDecorationCollection());
            }
         }
         catch { }

         return properties;
      }

      private double GetEditorTextSize()
      {
         return formatMap.GetTextProperties(regService.GetClassificationType("text"))
                         .FontRenderingEmSize;
      }

      private TextDecorationCollection GetTextDecoration(Options.Settings settings)
      {
         return settings.UnderlineImportantComments
                    ? new TextDecorationCollection { TextDecorations.Underline }
                    : new TextDecorationCollection();
      }
   }
}