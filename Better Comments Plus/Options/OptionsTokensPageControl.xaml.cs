using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using BetterCommentsPlus.CommentsTagging;

namespace BetterCommentsPlus.Options
{
   public partial class OptionsTokensPageControl
   {
      private Point _dragStartPoint;
      private CommentToken _draggedItem;
      private int _draggedIndex = -1;
      private bool _isDragging;
      private ListBoxItem _dragSourceItem;
      private double _originalOpacity;

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

      private void BackgroundColorPreviewBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         if (sender is Border border && border.DataContext is CommentToken token && token.BackgroundStyle != null)
         {
            Color? initialColor = null;
            if (!string.IsNullOrEmpty(token.BackgroundStyle.ColorHex))
            {
               try
               {
                  initialColor = (Color)ColorConverter.ConvertFromString(token.BackgroundStyle.ColorHex);
               }
               catch { }
            }

            var dialog = new ColorPickerDialog(initialColor)
            {
               Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.SelectedColor.HasValue)
            {
               token.BackgroundStyle.ColorHex = $"#{dialog.SelectedColor.Value.A:X2}{dialog.SelectedColor.Value.R:X2}{dialog.SelectedColor.Value.G:X2}{dialog.SelectedColor.Value.B:X2}";
               Settings.Instance.SyncCommentTokensToUnifiedConfig();
               Settings.Instance.OnConfigurationChanged();
            }
         }
      }

      private void DragGrip_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         _dragStartPoint = e.GetPosition(null);
         
         if (sender is Border grip && grip.DataContext is CommentToken token)
         {
            _draggedItem = token;
            _draggedIndex = Settings.Instance.CommentTokens.IndexOf(token);
            
            var listBoxItem = GetListBoxItemAncestor(grip);
            if (listBoxItem != null)
            {
               _dragSourceItem = listBoxItem;
               _originalOpacity = listBoxItem.Opacity;
            }
            
            TokensList.CaptureMouse();
            e.Handled = true;
         }
      }

      private ListBoxItem GetListBoxItemAncestor(DependencyObject element)
      {
         DependencyObject obj = element;
         while (obj != null && !(obj is ListBoxItem))
         {
            obj = VisualTreeHelper.GetParent(obj);
         }
         return obj as ListBoxItem;
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
               if (_dragSourceItem != null)
               {
                  _dragSourceItem.Opacity = 0.4;
               }
               DropIndicator.Visibility = Visibility.Visible;
            }

            if (_isDragging)
            {
               UpdateDropIndicator(sender as ListBox, e.GetPosition(sender as ListBox));
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

         if (_dragSourceItem != null)
         {
            _dragSourceItem.Opacity = _originalOpacity;
         }
         DropIndicator.Visibility = Visibility.Collapsed;

         _draggedItem = null;
         _draggedIndex = -1;
         _isDragging = false;
         _dragSourceItem = null;
         (sender as ListBox)?.ReleaseMouseCapture();
      }

      private void UpdateDropIndicator(ListBox listBox, Point position)
      {
         var targetItem = GetListBoxItemAtPosition(listBox, position);
         
         if (targetItem != null)
         {
            var transform = targetItem.TransformToVisual(this);
            var itemPosition = transform.Transform(new Point(0, 0));
            
            DropIndicator.Margin = new Thickness(0, itemPosition.Y - 1, 0, 0);
         }
         else if (Settings.Instance.CommentTokens.Count > 0)
         {
            var lastItem = TokensList.ItemContainerGenerator.ContainerFromIndex(Settings.Instance.CommentTokens.Count - 1) as ListBoxItem;
            if (lastItem != null)
            {
               var transform = lastItem.TransformToVisual(this);
               var itemPosition = transform.Transform(new Point(0, lastItem.ActualHeight));
               
               DropIndicator.Margin = new Thickness(0, itemPosition.Y - 1, 0, 0);
            }
         }
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
         var settings = DataContext as Settings;
         if (settings == null)
            return;

         int counter = 1;
         string newCriteria;
         do
         {
            newCriteria = $"#NEW{counter}";
            counter++;
         } while (settings.CommentTokens.Any(t => t.CurrentValue == newCriteria));

         var newToken = new CommentToken(
             type: CommentType.Important,
             defaultValue: newCriteria,
             value: newCriteria,
             colorHex: "#FFFF0000");

         newToken.RuleId = Guid.NewGuid().ToString();
         newToken.IsDynamic = true;

         settings.CommentTokens.Add(newToken);
         TokensList.SelectedItem = newToken;
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

      private void MoveUpButton_Click(object sender, RoutedEventArgs e)
      {
         var settings = DataContext as Settings;
         if (settings == null || TokensList.SelectedItem == null)
            return;

         int selectedIndex = TokensList.SelectedIndex;
         if (selectedIndex > 0)
         {
            settings.CommentTokens.Move(selectedIndex, selectedIndex - 1);
            TokensList.SelectedIndex = selectedIndex - 1;
            Settings.Instance.SyncCommentTokensToUnifiedConfig();
            Settings.Instance.OnConfigurationChanged();
         }
      }

      private void MoveDownButton_Click(object sender, RoutedEventArgs e)
      {
         var settings = DataContext as Settings;
         if (settings == null || TokensList.SelectedItem == null)
            return;

         int selectedIndex = TokensList.SelectedIndex;
         if (selectedIndex >= 0 && selectedIndex < settings.CommentTokens.Count - 1)
         {
            settings.CommentTokens.Move(selectedIndex, selectedIndex + 1);
            TokensList.SelectedIndex = selectedIndex + 1;
            Settings.Instance.SyncCommentTokensToUnifiedConfig();
            Settings.Instance.OnConfigurationChanged();
         }
      }
   }
}
