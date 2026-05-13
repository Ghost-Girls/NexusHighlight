﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Windows;

namespace BetterCommentsPlus.Options
{
   public abstract class OptionsPageBase : UIElementDialogPage
   {
      protected bool RulesValidated { get; private set; }

      protected override UIElement Child { get; }

      protected override void OnActivate(CancelEventArgs e)
      {
         RulesValidated = false;
         base.OnActivate(e);
      }

      protected override void OnApply(PageApplyEventArgs e)
      {
         RulesValidated = ValidateRules();

         if (RulesValidated)
         {
            e.ApplyBehavior = ApplyKind.Apply;
         }
         else
         {
            e.ApplyBehavior = ApplyKind.CancelNoNavigate;
            ShowInvalidRulesMessage();
         }

         base.OnApply(e);
      }

      protected override void OnClosed(EventArgs e)
      {
         if (RulesValidated)
         {
            SettingsStore.SaveSettings(Settings.Instance);
         }
         else
         {
            SettingsStore.LoadSettings(Settings.Instance);
         }

         base.OnClosed(e);
      }

      protected bool ValidateRules()
      {
         // 验证 Global 集合 - 确保没有重复
         var globalHasDuplicates = Settings.Instance.GlobalRules
             .GroupBy(r => r.Criteria)
             .Any(g => g.Count() > 1);
         
         if (globalHasDuplicates)
            return false;

         // 验证 Solution 集合 - 确保没有重复
         var solutionHasDuplicates = Settings.Instance.SolutionRules
             .GroupBy(r => r.Criteria)
             .Any(g => g.Count() > 1);
         
         if (solutionHasDuplicates)
            return false;

         // 验证所有规则的格式
         foreach (var rule in Settings.Instance.GlobalRules)
         {
            if (string.IsNullOrWhiteSpace(rule.Criteria))
               return false;
            if (rule.Criteria.IndexOfAny(new[] { '|', ',', '/' }) > -1)
               return false;
         }

         foreach (var rule in Settings.Instance.SolutionRules)
         {
            if (string.IsNullOrWhiteSpace(rule.Criteria))
               return false;
            if (rule.Criteria.IndexOfAny(new[] { '|', ',', '/' }) > -1)
               return false;
         }

         return true;
      }

      protected void ShowInvalidRulesMessage()
      {
         SystemSounds.Exclamation.Play();
         MessageBox.Show("Invalid rule!", "Better Comments Plus", MessageBoxButton.OK, MessageBoxImage.Error);
      }
   }
}