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
                var rules = settings.GlobalRules.Concat(settings.SolutionRules).ToList();
                
                var found = rules.FirstOrDefault(x => x.Criteria == selection);
                bool isNew = found == null;

                if (isNew)
                {
                    found = new CommentRule
                    {
                        Criteria = selection,
                        Id = Guid.NewGuid().ToString(),
                        IsPredefined = false,
                        ColorHex = enableForeground ? "#FF0000" : "#57a64a",
                        IsBold = false,
                        IsItalic = false,
                        HasUnderline = false,
                        HasStrikethrough = false,
                        IsForegroundActive = enableForeground,
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
                            settings.GlobalRules.Add(found);
                        }
                        if (editor.SaveToSolution)
                        {
                            settings.SolutionRules.Add(found);
                        }
                    }
                    else
                    {
                        UpdateRuleInCollections(found, editor.SaveToGlobal, editor.SaveToSolution, settings);
                    }

                    settings.OnConfigurationChanged();
                }
                else
                {
                    if (editor.delete)
                    {
                        RemoveRuleFromCollections(found, settings);
                        settings.OnConfigurationChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void UpdateRuleInCollections(CommentRule rule, bool saveToGlobal, bool saveToSolution, Settings settings)
        {
            var globalRule = settings.GlobalRules.FirstOrDefault(r => r.Id == rule.Id);
            var solutionRule = settings.SolutionRules.FirstOrDefault(r => r.Id == rule.Id);

            if (saveToGlobal && globalRule == null)
            {
                settings.GlobalRules.Add(rule);
            }
            else if (!saveToGlobal && globalRule != null)
            {
                settings.GlobalRules.Remove(globalRule);
            }
            else if (saveToGlobal && globalRule != null)
            {
                globalRule.Criteria = rule.Criteria;
                globalRule.ColorHex = rule.ColorHex;
                globalRule.IsBold = rule.IsBold;
                globalRule.IsItalic = rule.IsItalic;
                globalRule.HasUnderline = rule.HasUnderline;
                globalRule.HasStrikethrough = rule.HasStrikethrough;
                globalRule.IsForegroundActive = rule.IsForegroundActive;
                if (globalRule.Background != null && rule.Background != null)
                {
                    globalRule.Background.IsActive = rule.Background.IsActive;
                    globalRule.Background.ColorHex = rule.Background.ColorHex;
                    globalRule.Background.Shape = rule.Background.Shape;
                    globalRule.Background.Blur = rule.Background.Blur;
                    globalRule.Background.Alpha = rule.Background.Alpha;
                    globalRule.Background.IsCaseSensitive = rule.Background.IsCaseSensitive;
                    globalRule.Background.AllowPartialMatch = rule.Background.AllowPartialMatch;
                }
            }

            if (saveToSolution && solutionRule == null)
            {
                settings.SolutionRules.Add(rule);
            }
            else if (!saveToSolution && solutionRule != null)
            {
                settings.SolutionRules.Remove(solutionRule);
            }
            else if (saveToSolution && solutionRule != null)
            {
                solutionRule.Criteria = rule.Criteria;
                solutionRule.ColorHex = rule.ColorHex;
                solutionRule.IsBold = rule.IsBold;
                solutionRule.IsItalic = rule.IsItalic;
                solutionRule.HasUnderline = rule.HasUnderline;
                solutionRule.HasStrikethrough = rule.HasStrikethrough;
                solutionRule.IsForegroundActive = rule.IsForegroundActive;
                if (solutionRule.Background != null && rule.Background != null)
                {
                    solutionRule.Background.IsActive = rule.Background.IsActive;
                    solutionRule.Background.ColorHex = rule.Background.ColorHex;
                    solutionRule.Background.Shape = rule.Background.Shape;
                    solutionRule.Background.Blur = rule.Background.Blur;
                    solutionRule.Background.Alpha = rule.Background.Alpha;
                    solutionRule.Background.IsCaseSensitive = rule.Background.IsCaseSensitive;
                    solutionRule.Background.AllowPartialMatch = rule.Background.AllowPartialMatch;
                }
            }
        }

        private static void RemoveRuleFromCollections(CommentRule rule, Settings settings)
        {
            var globalRule = settings.GlobalRules.FirstOrDefault(r => r.Id == rule.Id);
            if (globalRule != null)
            {
                settings.GlobalRules.Remove(globalRule);
            }

            var solutionRule = settings.SolutionRules.FirstOrDefault(r => r.Id == rule.Id);
            if (solutionRule != null)
            {
                settings.SolutionRules.Remove(solutionRule);
            }
        }
    }
}
