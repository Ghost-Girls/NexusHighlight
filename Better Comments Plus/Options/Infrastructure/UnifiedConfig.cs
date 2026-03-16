using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BetterCommentsPlus.Options
{
    public class UnifiedConfig : PropertyChangeNotifier
    {
        private string version;
        private DateTime exportDate;
        private ObservableCollection<StyleRule> comments;

        [JsonPropertyName("Version")]
        public string Version
        {
            get { return version; }
            set { SetField(ref version, value); }
        }

        [JsonPropertyName("ExportDate")]
        public DateTime ExportDate
        {
            get { return exportDate; }
            set { SetField(ref exportDate, value); }
        }

        [JsonPropertyName("Comments")]
        public ObservableCollection<StyleRule> Comments
        {
            get { return comments; }
            set { SetField(ref comments, value); }
        }

        public UnifiedConfig()
        {
            version = "2.0.0";
            exportDate = DateTime.UtcNow;
            comments = new ObservableCollection<StyleRule>();
        }

        public static UnifiedConfig CreateDefault()
        {
            var config = new UnifiedConfig();
            
            config.Comments.Add(new StyleRule
            {
                Order = 1,
                IsActive = true,
                Id = "comment-important",
                Criteria = "#IMPORTANT",
                IsPredefined = true,
                Foreground = new ForegroundStyle
                {
                    IsActive = true,
                    ColorHex = "#FFFF0000",
                    IsBold = true,
                    IsItalic = false,
                    HasUnderline = false,
                    HasStrikethrough = false
                },
                Background = new BackgroundStyle
                {
                    IsActive = false,
                    ColorHex = null,
                    Shape = "Tag",
                    Blur = "None",
                    Alpha = "Alpha_1_10",
                    IsCaseSensitive = true,
                    AllowPartialMatch = false
                }
            });

            config.Comments.Add(new StyleRule
            {
                Order = 2,
                IsActive = true,
                Id = "comment-question",
                Criteria = "#QUESTION",
                IsPredefined = true,
                Foreground = new ForegroundStyle
                {
                    IsActive = true,
                    ColorHex = "#FFFFFF00",
                    IsBold = false,
                    IsItalic = false,
                    HasUnderline = false,
                    HasStrikethrough = false
                },
                Background = new BackgroundStyle
                {
                    IsActive = false,
                    ColorHex = null,
                    Shape = "Tag",
                    Blur = "None",
                    Alpha = "Alpha_1_10",
                    IsCaseSensitive = true,
                    AllowPartialMatch = false
                }
            });

            config.Comments.Add(new StyleRule
            {
                Order = 3,
                IsActive = true,
                Id = "comment-remove",
                Criteria = "#REMOVE",
                IsPredefined = true,
                Foreground = new ForegroundStyle
                {
                    IsActive = true,
                    ColorHex = "#FF808080",
                    IsBold = false,
                    IsItalic = false,
                    HasUnderline = false,
                    HasStrikethrough = true
                },
                Background = new BackgroundStyle
                {
                    IsActive = false,
                    ColorHex = null,
                    Shape = "Tag",
                    Blur = "None",
                    Alpha = "Alpha_1_10",
                    IsCaseSensitive = true,
                    AllowPartialMatch = false
                }
            });

            config.Comments.Add(new StyleRule
            {
                Order = 4,
                IsActive = true,
                Id = "comment-task",
                Criteria = "#TASK",
                IsPredefined = true,
                Foreground = new ForegroundStyle
                {
                    IsActive = true,
                    ColorHex = "#FFEB690A",
                    IsBold = false,
                    IsItalic = false,
                    HasUnderline = false,
                    HasStrikethrough = false
                },
                Background = new BackgroundStyle
                {
                    IsActive = false,
                    ColorHex = null,
                    Shape = "Tag",
                    Blur = "None",
                    Alpha = "Alpha_1_10",
                    IsCaseSensitive = true,
                    AllowPartialMatch = false
                }
            });

            return config;
        }

        public string ToJson()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null
            };
            return JsonSerializer.Serialize(this, options);
        }

        public static UnifiedConfig FromJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<UnifiedConfig>(json, options);
        }
    }
}
