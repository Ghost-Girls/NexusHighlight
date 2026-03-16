using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BetterCommentsPlus.Options
{
   public partial class OptionsTokensPageControl
   {
      public OptionsTokensPageControl()
      {
         DataContext = Settings.Instance;

         InitializeComponent();
      }

      private void ColorPreviewBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         if (sender is Border border && border.DataContext is CommentToken token)
         {
            Color? initialColor = null;
            if (!string.IsNullOrEmpty(token.ColorHex))
            {
               try
               {
                  initialColor = (Color)ColorConverter.ConvertFromString(token.ColorHex);
               }
               catch { }
            }

            var dialog = new ColorPickerDialog(initialColor)
            {
               Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.SelectedColor.HasValue)
            {
               token.ColorHex = $"#{dialog.SelectedColor.Value.A:X2}{dialog.SelectedColor.Value.R:X2}{dialog.SelectedColor.Value.G:X2}{dialog.SelectedColor.Value.B:X2}";
               Settings.Instance.SyncCommentTokensToUnifiedConfig();
            }
         }
      }
   }
}