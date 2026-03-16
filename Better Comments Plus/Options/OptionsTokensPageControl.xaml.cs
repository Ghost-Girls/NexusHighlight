using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System;

namespace BetterCommentsPlus.Options
{
   public partial class OptionsTokensPageControl
   {
      private Point _dragStartPoint;
      private CommentToken _draggedItem;
      private int _draggedIndex = -1;
      private bool _isDragging;
      private ListBoxItem _dragAdorner;

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

      private void TokensList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         _dragStartPoint = e.GetPosition(null);
         
         var listBox = sender as ListBox;
         var item = GetListBoxItemAtPosition(listBox, e.GetPosition(listBox));
         
         if (item != null)
         {
            _draggedItem = item.DataContext as CommentToken;
            if (_draggedItem != null)
            {
               _draggedIndex = Settings.Instance.CommentTokens.IndexOf(_draggedItem);
               listBox.CaptureMouse();
            }
         }
      }

      private void TokensList_MouseMove(object sender, MouseEventArgs e)
      {
         if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null && _draggedIndex >= 0)
         {
            Point currentPosition = e.GetPosition(null);
            Vector diff = _dragStartPoint - currentPosition;

            if (!_isDragging && (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
               _isDragging = true;
               Settings.Instance.IsDragging = true;
            }
         }
      }

      private void TokensList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         if (_isDragging && _draggedIndex >= 0)
         {
            var targetListBox = sender as ListBox;
            var settings = DataContext as Settings;

            if (targetListBox != null && settings != null)
            {
               var targetItem = GetListBoxItemAtPosition(targetListBox, e.GetPosition(targetListBox));
               var targetToken = targetItem?.DataContext as CommentToken;

               int newIndex = targetToken != null ? settings.CommentTokens.IndexOf(targetToken) : settings.CommentTokens.Count - 1;

               if (_draggedIndex >= 0 && newIndex >= 0 && _draggedIndex != newIndex)
               {
                  settings.CommentTokens.Move(_draggedIndex, newIndex);
                  TokensList.SelectedItem = _draggedItem;
               }
            }

            Settings.Instance.IsDragging = false;
            Settings.Instance.SyncCommentTokensToUnifiedConfig();
            Settings.Instance.OnConfigurationChanged();
         }

         _draggedItem = null;
         _draggedIndex = -1;
         _isDragging = false;
         (sender as ListBox)?.ReleaseMouseCapture();
      }

      private ListBoxItem GetListBoxItemAtPosition(ListBox listBox, Point position)
      {
         HitTestResult result = VisualTreeHelper.HitTest(listBox, position);
         if (result == null) return null;

         DependencyObject obj = result.VisualHit;
         while (obj != null && !(obj is ListBoxItem))
         {
            obj = VisualTreeHelper.GetParent(obj);
         }

         return obj as ListBoxItem;
      }

      private void AddButton_Click(object sender, RoutedEventArgs e)
      {
         MessageBox.Show("添加新规则的功能将在后续版本中实现（需要移除CommentType枚举限制）。", "提示",
             MessageBoxButton.OK, MessageBoxImage.Information);
      }

      private void DeleteButton_Click(object sender, RoutedEventArgs e)
      {
         if (sender is Button button && button.Tag is CommentToken token)
         {
            var result = MessageBox.Show($"确定要删除 \"{token.CurrentValue}\" 吗？", "确认删除",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
               var settings = DataContext as Settings;
               if (settings != null)
               {
                  settings.CommentTokens.Remove(token);
                  Settings.Instance.SyncCommentTokensToUnifiedConfig();
               }
            }
         }
      }
   }
}
