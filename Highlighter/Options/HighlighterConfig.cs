using Highlighter.Core;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Highlighter.Options
{
    public class HighlighterConfig
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0";

        [JsonPropertyName("exportDate")]
        public DateTime ExportDate { get; set; } = DateTime.Now;

        [JsonPropertyName("globalRules")]
        public List<HighlightTagData> GlobalRules { get; set; } = new List<HighlightTagData>();

        [JsonPropertyName("solutionRules")]
        public List<HighlightTagData> SolutionRules { get; set; } = new List<HighlightTagData>();

        [JsonPropertyName("performance")]
        public string Performance { get; set; } = "Normal";

        public static HighlighterConfig FromOptions(Options options)
        {
            var config = new HighlighterConfig
            {
                ExportDate = DateTime.Now,
                Performance = options.Performance.ToString()
            };

            if (options.ColorTags != null)
            {
                foreach (var tag in options.ColorTags)
                {
                    config.GlobalRules.Add(HighlightTagData.FromHighlightTag(tag));
                }
            }

            if (options.SolutionTags != null)
            {
                foreach (var tag in options.SolutionTags)
                {
                    config.SolutionRules.Add(HighlightTagData.FromHighlightTag(tag));
                }
            }

            return config;
        }

        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(this, options);
        }

        public static HighlighterConfig FromJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<HighlighterConfig>(json, options);
        }
    }

    public class HighlightTagData
    {
        [JsonPropertyName("criteria")]
        public string Criteria { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("shape")]
        public string Shape { get; set; }

        [JsonPropertyName("blur")]
        public string Blur { get; set; }

        [JsonPropertyName("alpha")]
        public string Alpha { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("allowPartialMatch")]
        public bool AllowPartialMatch { get; set; }

        [JsonPropertyName("isCaseSensitive")]
        public bool IsCaseSensitive { get; set; }

        public static HighlightTagData FromHighlightTag(HighlightTag tag)
        {
            return new HighlightTagData
            {
                Criteria = tag.Criteria,
                Color = tag.Color.ColorToHex(),
                Shape = tag.Shape.ToString(),
                Blur = tag.Blur.ToString(),
                Alpha = tag.Alpha.ToString(),
                IsActive = tag.IsActive,
                AllowPartialMatch = tag.AllowPartialMatch,
                IsCaseSensitive = tag.IsCaseSensitive
            };
        }

        public HighlightTag ToHighlightTag()
        {
            return new HighlightTag(Criteria)
            {
                Color = Helper.HexToColor(Color),
                Shape = (TagShape)System.Enum.Parse(typeof(TagShape), Shape),
                Blur = (BlurIntensity)System.Enum.Parse(typeof(BlurIntensity), Blur),
                Alpha = (FillAlpha)System.Enum.Parse(typeof(FillAlpha), Alpha),
                IsActive = IsActive,
                AllowPartialMatch = AllowPartialMatch,
                IsCaseSensitive = IsCaseSensitive
            };
        }
    }
}
