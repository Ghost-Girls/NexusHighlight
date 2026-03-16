using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace BetterCommentsPlus.CommentsClassification
{
    [Export]
    public class DynamicClassificationManager
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry { get; set; }

        [Import]
        internal IClassificationFormatMapService FormatMapService { get; set; }

        private readonly Dictionary<string, IClassificationType> registeredClassifications = new Dictionary<string, IClassificationType>();

        public IClassificationType GetOrCreateClassification(string classificationName)
        {
            if (registeredClassifications.TryGetValue(classificationName, out var existingType))
            {
                return existingType;
            }

            var classificationType = ClassificationRegistry.GetClassificationType(classificationName);
            if (classificationType == null)
            {
                classificationType = ClassificationRegistry.CreateClassificationType(
                    classificationName,
                    new[] { ClassificationRegistry.GetClassificationType("comment") });
            }

            registeredClassifications[classificationName] = classificationType;
            return classificationType;
        }

        public void ApplyClassificationFormat(string classificationName, Color? foregroundColor, bool isBold = false, bool isItalic = false, bool hasUnderline = false, bool hasStrikethrough = false)
        {
            var classificationType = GetOrCreateClassification(classificationName);
            var formatMap = FormatMapService.GetClassificationFormatMap("text");

            try
            {
                formatMap.BeginBatchUpdate();

                var properties = formatMap.GetTextProperties(classificationType);

                if (foregroundColor.HasValue)
                {
                    properties = properties.SetForeground(foregroundColor.Value);
                }

                if (isBold)
                {
                    properties = properties.SetBold(true);
                }

                if (isItalic)
                {
                    properties = properties.SetItalic(true);
                }

                if (hasUnderline || hasStrikethrough)
                {
                    var textDecorations = new TextDecorationCollection();
                    if (hasUnderline)
                    {
                        textDecorations.Add(TextDecorations.Underline);
                    }
                    if (hasStrikethrough)
                    {
                        textDecorations.Add(TextDecorations.Strikethrough);
                    }
                    properties = properties.SetTextDecorations(textDecorations);
                }

                formatMap.SetTextProperties(classificationType, properties);
            }
            finally
            {
                formatMap.EndBatchUpdate();
            }
        }

        public void ClearClassificationFormat(string classificationName)
        {
            var classificationType = GetOrCreateClassification(classificationName);
            var formatMap = FormatMapService.GetClassificationFormatMap("text");

            try
            {
                formatMap.BeginBatchUpdate();
                var defaultProperties = formatMap.DefaultTextProperties;
                formatMap.SetTextProperties(classificationType, defaultProperties);
            }
            finally
            {
                formatMap.EndBatchUpdate();
            }
        }
    }
}
