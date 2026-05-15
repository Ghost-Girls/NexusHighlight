using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace NexusHighlight.Options
{
   [ClassInterface(ClassInterfaceType.AutoDual)]
   [ComVisible(true)]
   [Guid("3F4564D7-3A70-4322-81FF-45E94C606D7B")]
   public class OptionsRulesPage : OptionsPageBase
   {
      private OptionsRulesPageControl pageControl;

      protected override UIElement Child
      {
         get { return pageControl ?? (pageControl = new OptionsRulesPageControl()); }
      }

      protected override void OnDeactivate(CancelEventArgs e)
      {
         if (!RulesValidated && !ValidateRules())
         {
            e.Cancel = true;
            ShowInvalidRulesMessage();
         }

         base.OnDeactivate(e);
      }
   }
}