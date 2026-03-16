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
      private object _draggedItem;
      private ListBox _sourceListBox;

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
         _sourceListBox = sender as ListBox;
         _draggedItem = _sourceListBox?.SelectedItem;
      }

      private void TokensList_MouseMove(object sender, MouseEventArgs e)
      {
         if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null)
         {
            Point currentPosition = e.GetPosition(null);
            Vector diff = _dragStartPoint - currentPosition;

            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
               DragDrop.DoDragDrop(_sourceListBox, _draggedItem, DragDropEffects.Move);
            }
         }
      }

      private void TokensList_DragEnter(object sender, DragEventArgs e)
      {
         if (e.Data.GetDataPresent(typeof(CommentToken)))
         {
            e.Effects = DragDropEffects.Move;
         }
         else
         {
            e.Effects = DragDropEffects.None;
         }
         e.Handled = true;
      }

      private void TokensList_DragOver(object sender, DragEventArgs e)
      {
         e.Handled = true;
      }

      private void TokensList_DragLeave(object sender, DragEventArgs e)
      {
         e.Handled = true;
      }

      private void TokensList_Drop(object sender, DragEventArgs e)
      {
         if (e.Data.GetDataPresent(typeof(CommentToken)))
         {
            var droppedItem = e.Data.GetData(typeof(CommentToken)) as CommentToken;
            var targetListBox = sender as ListBox;
            var settings = DataContext as Settings;

            if (droppedItem != null && targetListBox != null && settings != null)
            {
               var targetItem = GetListBoxItemAtPosition(targetListBox, e.GetPosition(targetListBox));
               var targetToken = targetItem?.DataContext as CommentToken;

               var tokens = settings.CommentTokens.ToList();
               if (tokens != null)
               {
                  int oldIndex = tokens.IndexOf(droppedItem);
                  int newIndex = targetToken != null ? tokens.IndexOf(targetToken) : tokens.Count - 1;

                  if (oldIndex >= 0 && newIndex >= 0 && oldIndex != newIndex)
                  {
                     tokens.RemoveAt(oldIndex);
                     tokens.Insert(newIndex, droppedItem);
                     
                     settings.CommentTokens.Clear();
                     foreach (var token in tokens)
                     {
                        settings.CommentTokens.Add(token);
                     }
                     
                     TokensList.ItemsSource = settings.CommentTokens;
                     TokensList.SelectedItem = droppedItem;
                     Settings.Instance.SyncCommentTokensToUnifiedConfig();
                  }
               }
            }
         }
         e.Handled = true;
         _draggedItem = null;
         _sourceListBox = null;
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