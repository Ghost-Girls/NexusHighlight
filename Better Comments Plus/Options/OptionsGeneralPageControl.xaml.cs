using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Controls;

namespace BetterCommentsPlus.Options
{
   public partial class OptionsGeneralPageControl
   {
      public OptionsGeneralPageControl()
      {
         DataContext = Settings.Instance;

         InitializeComponent();
         FontsComboBox.ItemsSource = GetInstalledFonts();
         Loaded += OptionsGeneralPageControl_Loaded;
         UIStrings.LanguageChanged += OnLanguageChanged;
      }

      private void OptionsGeneralPageControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
      {
         UIStrings.CurrentLanguage = Settings.Instance.UILanguage;
         ApplyUIStrings();
         SelectLanguageItem(Settings.Instance.UILanguage);
      }

      private void OnLanguageChanged(object sender, System.EventArgs e)
      {
         Dispatcher.Invoke(() => ApplyUIStrings());
      }

      private void ApplyUIStrings()
      {
         grpLanguage.Header = UIStrings.GroupLanguage;
         lblLanguage.Content = UIStrings.LabelLanguage + " :";
         SetComboItemText(cmbLanguage, "Chinese", UIStrings.LanguageChinese);
         SetComboItemText(cmbLanguage, "English", UIStrings.LanguageEnglish);
         
         lblFont.Content = UIStrings.LabelFont + " :";
         lblSize.Content = UIStrings.LabelSize + " :";
         lblSizeNote.Content = UIStrings.LabelSizeNote;
         lblOpacity.Content = UIStrings.LabelOpacity + " :";
         lblItalic.Content = UIStrings.LabelItalic;
         lblHighlightKeywordsOnly.Content = UIStrings.LabelHighlightKeywordsOnly;
         lblUnderlineImportant.Content = UIStrings.LabelUnderlineImportant;
         lblStrikethrough.Content = UIStrings.LabelStrikethrough;
         lnkFeedback.Text = UIStrings.Feedback;
      }

      private static void SetComboItemText(ComboBox combo, string tag, string text)
      {
         foreach (ComboBoxItem item in combo.Items)
         {
            if (item.Tag as string == tag)
            {
               item.Content = text;
               return;
            }
         }
      }

      private void SelectLanguageItem(UILanguage language)
      {
         foreach (ComboBoxItem item in cmbLanguage.Items)
         {
            if (item.Tag as string == language.ToString())
            {
               cmbLanguage.SelectedItem = item;
               break;
            }
         }
      }

      private void cmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (!IsLoaded) return;
         var selectedItem = cmbLanguage.SelectedItem as ComboBoxItem;
         if (selectedItem != null && System.Enum.TryParse<UILanguage>(selectedItem.Tag as string, out var lang))
         {
            UIStrings.SwitchLanguage(lang);
            Settings.Instance.UILanguage = lang;
         }
      }

      private static IEnumerable<string> GetInstalledFonts()
      {
         IEnumerable<string> result;

         using (var fonts = new InstalledFontCollection())
         {
            result = fonts.Families.Select(f => f.Name);
         }

         return result;
      }

      private void Hyperlink_Click(object sender, System.Windows.RoutedEventArgs e)
      {
         System.Diagnostics.Process.Start("https://docs.google.com/forms/d/e/1FAIpQLScRNeHI2q4yiaAzfXtGOidp-Tu8E6TEaKNPWnE4Cos_osHX9w/viewform?usp=sf_link");
      }
   }
}