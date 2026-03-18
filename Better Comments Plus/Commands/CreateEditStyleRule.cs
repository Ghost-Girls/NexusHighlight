using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using BetterCommentsPlus.Options;
using Task = System.Threading.Tasks.Task;

namespace BetterCommentsPlus.Commands
{
    internal static class CreateEditStyleRule
    {
        private static VsPackage package;
        private static IMenuCommandService commandService;

        public static void Initialize(VsPackage pkg, IMenuCommandService svc)
        {
            package = pkg;
            commandService = svc;

            if (commandService != null)
            {
                var foregroundCommandId = new CommandID(PackageGuids.guidBetterCommentsPlusPackageCmdSet, PackageIds.CreateEditForegroundStyleRule);
                var foregroundMenuItem = new MenuCommand(OnCreateEditForegroundStyleRule, foregroundCommandId);
                commandService.AddCommand(foregroundMenuItem);

                var backgroundCommandId = new CommandID(PackageGuids.guidBetterCommentsPlusPackageCmdSet, PackageIds.CreateEditBackgroundStyleRule);
                var backgroundMenuItem = new MenuCommand(OnCreateEditBackgroundStyleRule, backgroundCommandId);
                commandService.AddCommand(backgroundMenuItem);
            }
        }

        private static void OnCreateEditForegroundStyleRule(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ExecuteStyleRuleCommand(enableForeground: true);
        }

        private static void OnCreateEditBackgroundStyleRule(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ExecuteStyleRuleCommand(enableForeground: false);
        }

        private static void ExecuteStyleRuleCommand(bool enableForeground)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
                if (dte?.ActiveDocument == null)
                    return;

                string selection = (dte.ActiveDocument.Selection as TextSelection)?.Text;

                if (string.IsNullOrEmpty(selection) || string.IsNullOrWhiteSpace(selection))
                {
                    System.Windows.MessageBox.Show("Please select some text first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                selection = selection.Trim();

                var settings = Options.Settings.Instance;
                var rules = settings.StyleRules.ToList();
                
                var found = rules.FirstOrDefault(x => x.Criteria == selection);
                bool isNew = found == null;

                if (isNew)
                {
                    found = new StyleRule
                    {
                        Criteria = selection,
                        Id = Guid.NewGuid().ToString(),
                        Order = rules.Count + 1,
                        IsActive = true,
                        IsPredefined = false,
                        Foreground = new ForegroundStyle
                        {
                            IsActive = enableForeground,
                            ColorHex = enableForeground ? "#FF0000" : "#000000",
                            IsBold = false,
                            IsItalic = false,
                            HasUnderline = false,
                            HasStrikethrough = false
                        },
                        Background = new BackgroundStyle
                        {
                            IsActive = !enableForeground,
                            ColorHex = !enableForeground ? "#FF0000" : null,
                            Shape = "Tag",
                            Blur = "None",
                            Alpha = "1/10",
                            IsCaseSensitive = true,
                            AllowPartialMatch = false
                        }
                    };
                }

                var editor = new EditStyleRuleDialog
                {
                    RuleToEdit = found,
                    Title = isNew ? "Create Style Rule" : "Modify Style Rule"
                };

                editor.ModifyButton.Content = isNew ? "Create" : "Modify";
                editor.DeleteButton.Visibility = isNew ? Visibility.Collapsed : Visibility.Visible;

                bool? result = editor.ShowDialog();

                if (result == true)
                {
                    if (isNew)
                    {
                        if (editor.SaveToGlobal)
                        {
                            var token = ConvertStyleRuleToCommentToken(found, isGlobal: true);
                            settings.GlobalCommentTokens.Add(token);
                        }
                        if (editor.SaveToSolution)
                        {
                            var token = ConvertStyleRuleToCommentToken(found, isGlobal: false);
                            settings.SolutionCommentTokens.Add(token);
                        }
                    }
                    else
                    {
                        UpdateRuleInCollections(found, editor.SaveToGlobal, editor.SaveToSolution, settings);
                    }

                    settings.SyncCommentTokensToUnifiedConfig();
                    SettingsStore.SaveSettings(settings);
                    settings.OnConfigurationChanged();
                }
                else
                {
                    if (editor.delete)
                    {
                        RemoveRuleFromCollections(found, settings);
                        settings.SyncCommentTokensToUnifiedConfig();
                        SettingsStore.SaveSettings(settings);
                        settings.OnConfigurationChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static CommentToken ConvertStyleRuleToCommentToken(StyleRule rule, bool isGlobal)
        {
            var token = new CommentToken(
                type: CommentsTagging.CommentType.Important,
                defaultValue: rule.Criteria,
                value: rule.Criteria,
                colorHex: rule.Foreground.ColorHex ?? "#FFFF0000")
            {
                IsBold = rule.Foreground.IsBold,
                IsItalic = rule.Foreground.IsItalic,
                HasUnderline = rule.Foreground.HasUnderline,
                HasStrikethrough = rule.Foreground.HasStrikethrough,
                IsForegroundActive = rule.Foreground.IsActive,
                IsDynamic = true,
                RuleId = rule.Id,
                BackgroundStyle = new BackgroundStyle
                {
                    IsActive = rule.Background.IsActive,
                    ColorHex = rule.Background.ColorHex,
                    Shape = rule.Background.Shape ?? "Tag",
                    Blur = rule.Background.Blur ?? "None",
                    Alpha = rule.Background.Alpha ?? "1/10",
                    IsCaseSensitive = rule.Background.IsCaseSensitive,
                    AllowPartialMatch = rule.Background.AllowPartialMatch
                }
            };
            return token;
        }

        private static void UpdateRuleInCollections(StyleRule rule, bool saveToGlobal, bool saveToSolution, Settings settings)
        {
            var globalToken = settings.GlobalCommentTokens.FirstOrDefault(t => t.RuleId == rule.Id);
            var solutionToken = settings.SolutionCommentTokens.FirstOrDefault(t => t.RuleId == rule.Id);

            if (saveToGlobal && globalToken == null)
            {
                settings.GlobalCommentTokens.Add(ConvertStyleRuleToCommentToken(rule, isGlobal: true));
            }
            else if (!saveToGlobal && globalToken != null)
            {
                settings.GlobalCommentTokens.Remove(globalToken);
            }
            else if (saveToGlobal && globalToken != null)
            {
                UpdateCommentTokenFromStyleRule(globalToken, rule);
            }

            if (saveToSolution && solutionToken == null)
            {
                settings.SolutionCommentTokens.Add(ConvertStyleRuleToCommentToken(rule, isGlobal: false));
            }
            else if (!saveToSolution && solutionToken != null)
            {
                settings.SolutionCommentTokens.Remove(solutionToken);
            }
            else if (saveToSolution && solutionToken != null)
            {
                UpdateCommentTokenFromStyleRule(solutionToken, rule);
            }
        }

        private static void UpdateCommentTokenFromStyleRule(CommentToken token, StyleRule rule)
        {
            token.CurrentValue = rule.Criteria;
            token.ColorHex = rule.Foreground.ColorHex;
            token.IsBold = rule.Foreground.IsBold;
            token.IsItalic = rule.Foreground.IsItalic;
            token.HasUnderline = rule.Foreground.HasUnderline;
            token.HasStrikethrough = rule.Foreground.HasStrikethrough;
            token.IsForegroundActive = rule.Foreground.IsActive;
            
            if (token.BackgroundStyle != null)
            {
                token.BackgroundStyle.IsActive = rule.Background.IsActive;
                token.BackgroundStyle.ColorHex = rule.Background.ColorHex;
                token.BackgroundStyle.Shape = rule.Background.Shape;
                token.BackgroundStyle.Blur = rule.Background.Blur;
                token.BackgroundStyle.Alpha = rule.Background.Alpha;
                token.BackgroundStyle.IsCaseSensitive = rule.Background.IsCaseSensitive;
                token.BackgroundStyle.AllowPartialMatch = rule.Background.AllowPartialMatch;
            }
        }

        private static void RemoveRuleFromCollections(StyleRule rule, Settings settings)
        {
            var globalToken = settings.GlobalCommentTokens.FirstOrDefault(t => t.RuleId == rule.Id);
            if (globalToken != null)
            {
                settings.GlobalCommentTokens.Remove(globalToken);
            }

            var solutionToken = settings.SolutionCommentTokens.FirstOrDefault(t => t.RuleId == rule.Id);
            if (solutionToken != null)
            {
                settings.SolutionCommentTokens.Remove(solutionToken);
            }
        }
    }
}
