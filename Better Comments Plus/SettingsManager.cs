using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Collections.ObjectModel;
using BetterCommentsPlus.Options;

namespace BetterCommentsPlus
{
    public class SettingsManager
    {
        private const string GlobalSettingsDir = "BetterCommentsPlus";
        private const string GlobalRulesFile = "globalRules.json";
        private const string SolutionRulesDir = "Better Comments Plus";
        private const string SolutionRulesFile = "rules.json";

        // 获取全局设置目录
        public static string GetGlobalSettingsDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var settingsDir = Path.Combine(appData, GlobalSettingsDir);
            Directory.CreateDirectory(settingsDir);
            return settingsDir;
        }

        // 获取全局规则文件路径
        public static string GetGlobalRulesFilePath()
        {
            var settingsDir = GetGlobalSettingsDirectory();
            return Path.Combine(settingsDir, GlobalRulesFile);
        }

        // 获取解决方案规则目录
        public static string GetSolutionRulesDirectory(string solutionPath)
        {
            if (string.IsNullOrEmpty(solutionPath))
                return null;

            try
            {
                var solutionDir = Path.GetDirectoryName(solutionPath);
                if (string.IsNullOrEmpty(solutionDir))
                    return null;

                var vsDir = Path.Combine(solutionDir, ".vs");
                var rulesDir = Path.Combine(vsDir, SolutionRulesDir);
                
                // 确保目录存在
                if (!Directory.Exists(rulesDir))
                {
                    Directory.CreateDirectory(rulesDir);
                }
                
                return rulesDir;
            }
            catch
            {
                return null;
            }
        }

        // 获取解决方案规则文件路径
        public static string GetSolutionRulesFilePath(string solutionPath)
        {
            var rulesDir = GetSolutionRulesDirectory(solutionPath);
            if (rulesDir == null)
                return null;
            return Path.Combine(rulesDir, SolutionRulesFile);
        }

        // 保存全局规则
        public static void SaveGlobalRules(ObservableCollection<CommentRule> rules)
        {
            SaveGlobalRules(new List<CommentRule>(rules));
        }

        // 保存全局规则（线程安全版本，接受 List）
        public static void SaveGlobalRules(List<CommentRule> rules)
        {
            try
            {
                var filePath = GetGlobalRulesFilePath();
                var json = JsonSerializer.Serialize(rules, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch
            {
                // 静默处理错误
            }
        }

        // 加载全局规则
        public static ObservableCollection<CommentRule> LoadGlobalRules()
        {
            try
            {
                var filePath = GetGlobalRulesFilePath();
                if (!File.Exists(filePath))
                {
                    return CreateDefaultGlobalRules();
                }

                var json = File.ReadAllText(filePath);
                var rulesList = JsonSerializer.Deserialize<List<CommentRule>>(json);
                return new ObservableCollection<CommentRule>(rulesList);
            }
            catch
            {
                return CreateDefaultGlobalRules();
            }
        }

        // 保存解决方案规则
        public static void SaveSolutionRules(string solutionPath, ObservableCollection<CommentRule> rules)
        {
            SaveSolutionRules(solutionPath, new List<CommentRule>(rules));
        }

        // 保存解决方案规则（线程安全版本，接受 List）
        public static void SaveSolutionRules(string solutionPath, List<CommentRule> rules)
        {
            try
            {
                var filePath = GetSolutionRulesFilePath(solutionPath);
                if (filePath == null)
                    return;

                var json = JsonSerializer.Serialize(rules, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch
            {
                // 静默处理错误
            }
        }

        // 加载解决方案规则
        public static ObservableCollection<CommentRule> LoadSolutionRules(string solutionPath)
        {
            var filePath = GetSolutionRulesFilePath(solutionPath);
            if (filePath == null || !File.Exists(filePath))
            {
                return new ObservableCollection<CommentRule>();
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var rulesList = JsonSerializer.Deserialize<List<CommentRule>>(json);
                return new ObservableCollection<CommentRule>(rulesList);
            }
            catch
            {
                return new ObservableCollection<CommentRule>();
            }
        }

        // 创建默认全局规则
        private static ObservableCollection<CommentRule> CreateDefaultGlobalRules()
        {
            var rules = new ObservableCollection<CommentRule>
            {
                new CommentRule
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Criteria = "#IMPORTANT",
                    ColorHex = "#FFFF0000",
                    IsBold = true,
                    IsItalic = false,
                    HasUnderline = false,
                    HasStrikethrough = false,
                    IsForegroundActive = true,
                    IsPredefined = false,
                    Background = new BetterCommentsPlus.Options.Background
                    {
                        IsActive = false,
                        ColorHex = null,
                        Shape = "Tag",
                        Blur = "None",
                        Alpha = "1/10",
                        IsCaseSensitive = true,
                        AllowPartialMatch = false
                    }
                },
                new CommentRule
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Criteria = "#REMOVE",
                    ColorHex = "#FF808080",
                    IsBold = false,
                    IsItalic = false,
                    HasUnderline = false,
                    HasStrikethrough = true,
                    IsForegroundActive = true,
                    IsPredefined = false,
                    Background = new BetterCommentsPlus.Options.Background
                    {
                        IsActive = false,
                        ColorHex = null,
                        Shape = "Tag",
                        Blur = "None",
                        Alpha = "1/10",
                        IsCaseSensitive = true,
                        AllowPartialMatch = false
                    }
                },
                new CommentRule
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Criteria = "#QUESTION",
                    ColorHex = "#FFFFFF00",
                    IsBold = false,
                    IsItalic = false,
                    HasUnderline = false,
                    HasStrikethrough = false,
                    IsForegroundActive = true,
                    IsPredefined = false,
                    Background = new BetterCommentsPlus.Options.Background
                    {
                        IsActive = false,
                        ColorHex = null,
                        Shape = "Tag",
                        Blur = "None",
                        Alpha = "1/10",
                        IsCaseSensitive = true,
                        AllowPartialMatch = false
                    }
                },
                new CommentRule
                {
                    Id = System.Guid.NewGuid().ToString(),
                    Criteria = "#TASK",
                    ColorHex = "#FFEB690A",
                    IsBold = false,
                    IsItalic = false,
                    HasUnderline = false,
                    HasStrikethrough = false,
                    IsForegroundActive = true,
                    IsPredefined = false,
                    Background = new BetterCommentsPlus.Options.Background
                    {
                        IsActive = false,
                        ColorHex = null,
                        Shape = "Tag",
                        Blur = "None",
                        Alpha = "1/10",
                        IsCaseSensitive = true,
                        AllowPartialMatch = false
                    }
                }
            };

            // 保存默认规则
            SaveGlobalRules(rules);
            return rules;
        }
    }
}