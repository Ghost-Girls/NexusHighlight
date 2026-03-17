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
                        settings.StyleRules.Add(found);
                    }
                    else
                    {
                        var existingRule = settings.StyleRules.FirstOrDefault(r => r.Id == found.Id);
                        if (existingRule != null)
                        {
                            int index = settings.StyleRules.IndexOf(existingRule);
                            settings.StyleRules[index] = found;
                        }
                    }

                    settings.SyncStyleRulesToUnifiedConfig();
                    SettingsStore.SaveSettings(settings);
                    settings.OnConfigurationChanged();
                }
                else
                {
                    if (editor.delete)
                    {
                        var ruleToRemove = settings.StyleRules.FirstOrDefault(r => r.Id == found.Id);
                        if (ruleToRemove != null)
                        {
                            settings.StyleRules.Remove(ruleToRemove);
                            settings.SyncStyleRulesToUnifiedConfig();
                            SettingsStore.SaveSettings(settings);
                            settings.OnConfigurationChanged();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
