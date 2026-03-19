﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Media;
using System.Windows;

namespace BetterCommentsPlus.Options
{
   public abstract class OptionsPageBase : UIElementDialogPage
   {
      protected bool TokensValidated { get; private set; }

      protected override UIElement Child { get; }

      protected override void OnActivate(CancelEventArgs e)
      {
         TokensValidated = false;
         base.OnActivate(e);
      }

      protected override void OnApply(PageApplyEventArgs e)
      {
         TokensValidated = ValidateTokens();

         if (TokensValidated)
         {
            e.ApplyBehavior = ApplyKind.Apply;
         }
         else
         {
            e.ApplyBehavior = ApplyKind.CancelNoNavigate;
            ShowInvalidTokenMessage();
         }

         base.OnApply(e);
      }

      protected override void OnClosed(EventArgs e)
      {
         if (TokensValidated)
         {
            SettingsStore.SaveSettings(Settings.Instance);
         }
         else
         {
            SettingsStore.LoadSettings(Settings.Instance);
         }

         base.OnClosed(e);
      }

      protected bool ValidateTokens()
      {
         // 验证 Global 集合
         var globalRule = new RequiredAndUniqueRule { IsGlobalScope = true };
         foreach (var rule in Settings.Instance.GlobalRules)
         {
            if (!globalRule.Validate(rule.Criteria, CultureInfo.InvariantCulture).IsValid)
               return false;
         }

         // 验证 Solution 集合
         var solutionRule = new RequiredAndUniqueRule { IsSolutionScope = true };
         foreach (var rule in Settings.Instance.SolutionRules)
         {
            if (!solutionRule.Validate(rule.Criteria, CultureInfo.InvariantCulture).IsValid)
               return false;
         }

         return true;
      }

      protected void ShowInvalidTokenMessage()
      {
         SystemSounds.Exclamation.Play();
         MessageBox.Show("Invalid token!", "Better Comments Plus", MessageBoxButton.OK, MessageBoxImage.Error);
      }
   }
}