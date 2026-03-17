using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using TextFormattingRunProperties = Microsoft.VisualStudio.Text.Formatting.TextFormattingRunProperties;

namespace BetterCommentsPlus.CommentsClassification
{
    [Export(typeof(DynamicClassificationManager))]
    internal class DynamicClassificationManager
    {
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry { get; set; }

        [Import]
        internal IClassificationFormatMapService FormatMapService { get; set; }

        private readonly Dictionary<string, IClassificationType> _classificationTypes 
            = new Dictionary<string, IClassificationType>();

        private const string TextClassification = "text";

        public IClassificationType GetOrCreateClassification(string classificationName)
        {
            if (_classificationTypes.TryGetValue(classificationName, out var existingType))
            {
                return existingType;
            }

            var baseType = ClassificationRegistry.GetClassificationType(TextClassification);
            var newType = ClassificationRegistry.CreateClassificationType(
                classificationName, 
                new[] { baseType });

            _classificationTypes[classificationName] = newType;
            return newType;
        }

        public void ApplyClassificationFormat(string classificationName, Color? foregroundColor = null, 
            bool? isItalic = null, bool? isBold = null, double? opacity = null)
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

                if (isItalic.HasValue)
                {
                    properties = properties.SetItalic(isItalic.Value);
                }

                if (opacity.HasValue && opacity.Value >= 0.1 && opacity.Value <= 1.0)
                {
                    properties = properties.SetForegroundOpacity(opacity.Value);
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
            if (!_classificationTypes.ContainsKey(classificationName))
                return;

            var classificationType = _classificationTypes[classificationName];
            var formatMap = FormatMapService.GetClassificationFormatMap("text");

            try
            {
                formatMap.BeginBatchUpdate();
                formatMap.SetTextProperties(classificationType, TextFormattingRunProperties.Empty);
            }
            finally
            {
                formatMap.EndBatchUpdate();
            }
        }

        public bool ClassificationExists(string classificationName)
        {
            return _classificationTypes.ContainsKey(classificationName) ||
                   ClassificationRegistry.GetClassificationType(classificationName) != null;
        }
    }
}
