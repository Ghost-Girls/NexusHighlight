using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Windows.Documents;
using BetterCommentsPlus.CommentsTagging;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;

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
      private ListBox _currentDraggingListBox;
      private AdornerLayer _adornerLayer;
      private InsertionAdorner _insertionAdorner;

      public OptionsTokensPageControl()
      {
         DataContext = Settings.Instance;
         InitializeComponent();
      }

      #region 颜色选择器相关

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

      #endregion

      #region 拖拽相关

      private void DragGrip_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         _dragStartPoint = e.GetPosition(null);
         
         if (sender is Border grip && grip.DataContext is CommentToken token)
         {
            _draggedItem = token;
            _currentDraggingListBox = FindParentListBox(grip);
            
            if (_currentDraggingListBox == GlobalTokensList)
                _draggedIndex = Settings.Instance.GlobalCommentTokens.IndexOf(token);
            else if (_currentDraggingListBox == SolutionTokensList)
                _draggedIndex = Settings.Instance.SolutionCommentTokens.IndexOf(token);
            
            var listBoxItem = GetListBoxItemAncestor(grip);
            if (listBoxItem != null)
            {
               _dragSourceItem = listBoxItem;
               _originalOpacity = listBoxItem.Opacity;
            }
            
            _currentDraggingListBox?.CaptureMouse();
            e.Handled = true;
         }
      }

      private ListBox FindParentListBox(DependencyObject element)
      {
         DependencyObject obj = element;
         while (obj != null)
         {
            if (obj is ListBox listBox)
               return listBox;
            obj = VisualTreeHelper.GetParent(obj);
         }
         return null;
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

      private void GlobalTokensList_MouseMove(object sender, MouseEventArgs e)
      {
         HandleListBoxMouseMove(sender as ListBox, e, Settings.Instance.GlobalCommentTokens);
      }

      private void SolutionTokensList_MouseMove(object sender, MouseEventArgs e)
      {
         HandleListBoxMouseMove(sender as ListBox, e, Settings.Instance.SolutionCommentTokens);
      }

      private void HandleListBoxMouseMove(ListBox listBox, MouseEventArgs e, ObservableCollection<CommentToken> tokens)
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
            }
            
            // 显示插入指示器
            if (_isDragging && listBox != null)
            {
               var mousePos = e.GetPosition(listBox);
               int targetIndex = GetInsertionIndex(listBox, mousePos);
               
               if (targetIndex >= 0 && targetIndex != _draggedIndex)
               {
                  ShowInsertionAdorner(listBox, targetIndex);
               }
            }
         }
      }
      
      private int GetInsertionIndex(ListBox listBox, Point mousePos)
      {
         for (int i = 0; i < listBox.Items.Count; i++)
         {
            var container = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
            if (container == null) continue;
            
            var itemRect = new Rect(
                container.TranslatePoint(new Point(0, 0), listBox),
                container.RenderSize);
            
            // 如果鼠标在项的上半部分，插入到该项之前
            // 如果鼠标在项的下半部分，插入到该项之后
            if (mousePos.Y < itemRect.Top + itemRect.Height / 2)
            {
               return i;
            }
         }
         
         // 如果鼠标在所有项的下方，插入到末尾
         return listBox.Items.Count;
      }

      private void GlobalTokensList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         HandleListBoxMouseUp(sender as ListBox, e, Settings.Instance.GlobalCommentTokens, GlobalTokensList);
      }

      private void SolutionTokensList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         HandleListBoxMouseUp(sender as ListBox, e, Settings.Instance.SolutionCommentTokens, SolutionTokensList);
      }

      private void HandleListBoxMouseUp(ListBox listBox, MouseButtonEventArgs e, ObservableCollection<CommentToken> tokens, ListBox targetListBox)
      {
         if (_isDragging && _draggedIndex >= 0)
         {
            var settings = DataContext as Settings;
            var targetTokens = listBox == GlobalTokensList ? settings.GlobalCommentTokens : settings.SolutionCommentTokens;

            if (targetListBox != null && settings != null)
            {
               // 使用相同的算法计算放置位置
               var mousePos = e.GetPosition(targetListBox);
               int newIndex = GetInsertionIndex(targetListBox, mousePos);
               
               // 调整索引，因为拖拽项本身也占一个位置
               if (newIndex > _draggedIndex)
                  newIndex--;
               
               if (_draggedIndex >= 0 && newIndex >= 0 && _draggedIndex != newIndex)
               {
                  targetTokens.Move(_draggedIndex, newIndex);
                  targetListBox.SelectedItem = _draggedItem;
               }
            }

            Settings.Instance.IsDragging = false;
            Settings.Instance.SyncCommentTokensToUnifiedConfig();
            Settings.Instance.OnConfigurationChanged();
         }

         // 移除插入指示器
         RemoveInsertionAdorner();

         if (_dragSourceItem != null)
         {
            _dragSourceItem.Opacity = _originalOpacity;
         }

         _draggedItem = null;
         _draggedIndex = -1;
         _isDragging = false;
         _dragSourceItem = null;
         _currentDraggingListBox?.ReleaseMouseCapture();
         _currentDraggingListBox = null;
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

      #endregion

      #region 添加按钮

      private void AddGlobalButton_Click(object sender, RoutedEventArgs e)
      {
         AddToken(Settings.Instance.GlobalCommentTokens, GlobalTokensList);
      }

      private void AddToken(ObservableCollection<CommentToken> tokens, ListBox listBox)
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
         } while (settings.GlobalCommentTokens.Any(t => t.CurrentValue == newCriteria) ||
                  settings.SolutionCommentTokens.Any(t => t.CurrentValue == newCriteria));

         var newToken = new CommentToken(
             type: CommentType.Important,
             defaultValue: newCriteria,
             value: newCriteria,
             colorHex: "#FFFF0000");

         newToken.RuleId = Guid.NewGuid().ToString();
         newToken.IsDynamic = true;

         tokens.Add(newToken);
         listBox.SelectedItem = newToken;
      }

      #endregion

      #region 删除按钮

      private void DeleteButton_Click(object sender, RoutedEventArgs e)
      {
         if (sender is Button button && button.Tag is CommentToken token)
         {
            var settings = DataContext as Settings;
            if (settings != null)
            {
               if (settings.GlobalCommentTokens.Contains(token))
                  settings.GlobalCommentTokens.Remove(token);
               else if (settings.SolutionCommentTokens.Contains(token))
                  settings.SolutionCommentTokens.Remove(token);
               
               Settings.Instance.SyncCommentTokensToUnifiedConfig();
            }
         }
      }

      #endregion

      #region 上下移动按钮

      private void MoveUpGlobalButton_Click(object sender, RoutedEventArgs e)
      {
         MoveUp(Settings.Instance.GlobalCommentTokens, GlobalTokensList);
      }

      private void MoveDownGlobalButton_Click(object sender, RoutedEventArgs e)
      {
         MoveDown(Settings.Instance.GlobalCommentTokens, GlobalTokensList);
      }

      private void MoveUpSolutionButton_Click(object sender, RoutedEventArgs e)
      {
         MoveUp(Settings.Instance.SolutionCommentTokens, SolutionTokensList);
      }

      private void MoveDownSolutionButton_Click(object sender, RoutedEventArgs e)
      {
         MoveDown(Settings.Instance.SolutionCommentTokens, SolutionTokensList);
      }

      private void MoveUp(ObservableCollection<CommentToken> tokens, ListBox listBox)
      {
         var settings = DataContext as Settings;
         if (settings == null || listBox.SelectedItem == null)
            return;

         int selectedIndex = listBox.SelectedIndex;
         if (selectedIndex > 0)
         {
            tokens.Move(selectedIndex, selectedIndex - 1);
            listBox.SelectedIndex = selectedIndex - 1;
            Settings.Instance.SyncCommentTokensToUnifiedConfig();
            Settings.Instance.OnConfigurationChanged();
         }
      }

      private void MoveDown(ObservableCollection<CommentToken> tokens, ListBox listBox)
      {
         var settings = DataContext as Settings;
         if (settings == null || listBox.SelectedItem == null)
            return;

         int selectedIndex = listBox.SelectedIndex;
         if (selectedIndex >= 0 && selectedIndex < tokens.Count - 1)
         {
            tokens.Move(selectedIndex, selectedIndex + 1);
            listBox.SelectedIndex = selectedIndex + 1;
            Settings.Instance.SyncCommentTokensToUnifiedConfig();
            Settings.Instance.OnConfigurationChanged();
         }
      }

      #endregion

      #region Solution 相关按钮

      private void CopyAllFromGlobalButton_Click(object sender, RoutedEventArgs e)
      {
         var result = MessageBox.Show(
             "这将复制所有 Global Rules 到 Solution Rules，是否继续？",
             "确认复制",
             MessageBoxButton.YesNo,
             MessageBoxImage.Question);

         if (result == MessageBoxResult.Yes)
         {
            Settings.Instance.CopyAllFromGlobalToSolution();
         }
      }

      private void ImportFromGlobalButton_Click(object sender, RoutedEventArgs e)
      {
         if (Settings.Instance.GlobalCommentTokens.Count > 0)
         {
            var dialog = new Window
            {
               Title = "从 Global Rules 选择规则（按住 Ctrl 或 Shift 多选）",
               Width = 400,
               Height = 350,
               WindowStartupLocation = WindowStartupLocation.CenterOwner,
               Owner = Window.GetWindow(this)
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var instructionText = new TextBlock
            {
               Text = "请选择要导入的规则（支持多选）：",
               Margin = new Thickness(10)
            };
            Grid.SetRow(instructionText, 0);
            grid.Children.Add(instructionText);

            var listBox = new ListBox
            {
               ItemsSource = Settings.Instance.GlobalCommentTokens,
               DisplayMemberPath = "CurrentValue",
               SelectionMode = SelectionMode.Extended,
               Margin = new Thickness(10)
            };
            Grid.SetRow(listBox, 1);
            grid.Children.Add(listBox);

            var buttonPanel = new StackPanel
            {
               Orientation = Orientation.Horizontal,
               HorizontalAlignment = HorizontalAlignment.Right,
               Margin = new Thickness(10)
            };

            var okButton = new Button
            {
               Content = "导入选中项",
               Width = 100,
               Margin = new Thickness(5)
            };
            okButton.Click += (s, args) =>
            {
               var selectedTags = listBox.SelectedItems.Cast<CommentToken>().ToList();
               if (selectedTags.Count > 0)
               {
                  foreach (var selectedTag in selectedTags)
                  {
                     var newToken = new CommentToken(
                         type: selectedTag.Type,
                         defaultValue: selectedTag.DefaultValue,
                         value: selectedTag.CurrentValue,
                         colorHex: selectedTag.ColorHex)
                     {
                        IsBold = selectedTag.IsBold,
                        IsItalic = selectedTag.IsItalic,
                        HasUnderline = selectedTag.HasUnderline,
                        HasStrikethrough = selectedTag.HasStrikethrough,
                        IsForegroundActive = selectedTag.IsForegroundActive,
                        IsDynamic = true,
                        RuleId = Guid.NewGuid().ToString(),
                        BackgroundStyle = new BackgroundStyle
                        {
                           IsActive = selectedTag.BackgroundStyle?.IsActive ?? false,
                           ColorHex = selectedTag.BackgroundStyle?.ColorHex,
                           Shape = selectedTag.BackgroundStyle?.Shape ?? "Tag",
                           Blur = selectedTag.BackgroundStyle?.Blur ?? "None",
                           Alpha = selectedTag.BackgroundStyle?.Alpha ?? "1/10",
                           IsCaseSensitive = selectedTag.BackgroundStyle?.IsCaseSensitive ?? true,
                           AllowPartialMatch = selectedTag.BackgroundStyle?.AllowPartialMatch ?? false
                        }
                     };
                     Settings.Instance.SolutionCommentTokens.Add(newToken);
                  }
                  Settings.Instance.SyncCommentTokensToUnifiedConfig();
                  Settings.Instance.OnConfigurationChanged();
                  dialog.Close();
               }
               else
               {
                  MessageBox.Show("请至少选择一个规则", "提示",
                      MessageBoxButton.OK, MessageBoxImage.Information);
               }
            };

            var cancelButton = new Button
            {
               Content = "取消",
               Width = 80,
               Margin = new Thickness(5)
            };
            cancelButton.Click += (s, args) => dialog.Close();

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            dialog.Content = grid;
            dialog.ShowDialog();
         }
         else
         {
            MessageBox.Show("没有可用的 Global Rules", "提示",
                MessageBoxButton.OK, MessageBoxImage.Information);
         }
      }

      private void ClearSolutionButton_Click(object sender, RoutedEventArgs e)
      {
         var result = MessageBox.Show(
             "确定要清除所有 Solution Rules 吗？",
             "确认清除",
             MessageBoxButton.YesNo,
             MessageBoxImage.Warning);

         if (result == MessageBoxResult.Yes)
         {
            Settings.Instance.ClearSolutionTokens();
         }
      }

      #endregion

      #region 导入导出

      private void ExportGlobalButton_Click(object sender, RoutedEventArgs e)
      {
         ExportRules(Settings.Instance.GlobalCommentTokens, "Global", "BetterCommentsPlus_GlobalRules.json");
      }

      private void ImportGlobalButton_Click(object sender, RoutedEventArgs e)
      {
         ImportRules(Settings.Instance.GlobalCommentTokens, "Global");
      }

      private void ExportSolutionButton_Click(object sender, RoutedEventArgs e)
      {
         ExportRules(Settings.Instance.SolutionCommentTokens, "Solution", "BetterCommentsPlus_SolutionRules.json");
      }

      private void ImportSolutionButton_Click(object sender, RoutedEventArgs e)
      {
         ImportRules(Settings.Instance.SolutionCommentTokens, "Solution");
      }

      private void ExportRules(ObservableCollection<CommentToken> tokens, string type, string defaultFileName)
      {
         if (tokens.Count == 0)
         {
            MessageBox.Show($"没有可用的 {type} Rules", "提示",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
         }

         var saveDialog = new SaveFileDialog
         {
            Filter = "JSON 文件 (*.json)|*.json",
            DefaultExt = ".json",
            FileName = defaultFileName,
            Title = $"导出 {type} Rules"
         };

         if (saveDialog.ShowDialog() == true)
         {
            try
            {
               var config = new CommentsPlusConfig
               {
                  ExportDate = DateTime.Now,
                  Rules = tokens.Select(t => CommentTokenData.FromCommentToken(t)).ToList()
               };

               var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
               {
                  WriteIndented = true,
                  PropertyNamingPolicy = JsonNamingPolicy.CamelCase
               });

               File.WriteAllText(saveDialog.FileName, json);
               MessageBox.Show("导出成功！", "提示",
                   MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
               MessageBox.Show($"导出失败：{ex.Message}", "错误",
                   MessageBoxButton.OK, MessageBoxImage.Error);
            }
         }
      }

      private void ImportRules(ObservableCollection<CommentToken> tokens, string type)
      {
         var openDialog = new OpenFileDialog
         {
            Filter = "JSON 文件 (*.json)|*.json",
            Title = $"导入 {type} Rules"
         };

         if (openDialog.ShowDialog() == true)
         {
            try
            {
               var json = File.ReadAllText(openDialog.FileName);
               var config = JsonSerializer.Deserialize<CommentsPlusConfig>(json, new JsonSerializerOptions
               {
                  PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                  PropertyNameCaseInsensitive = true
               });

               if (config?.Rules != null && config.Rules.Count > 0)
               {
                  var result = MessageBox.Show(
                      "选择导入模式：\n\n是 - 覆盖现有规则\n否 - 合并现有规则",
                      "导入模式",
                      MessageBoxButton.YesNoCancel,
                      MessageBoxImage.Question);

                  if (result == MessageBoxResult.Cancel)
                     return;

                  if (result == MessageBoxResult.No)
                  {
                     foreach (var tokenData in config.Rules)
                     {
                        var newToken = tokenData.ToCommentToken();
                        newToken.PropertyChanged += CommentToken_PropertyChanged;
                        tokens.Add(newToken);
                     }
                  }
                  else
                  {
                     tokens.Clear();
                     foreach (var tokenData in config.Rules)
                     {
                        var newToken = tokenData.ToCommentToken();
                        newToken.PropertyChanged += CommentToken_PropertyChanged;
                        tokens.Add(newToken);
                     }
                  }

                  Settings.Instance.SyncCommentTokensToUnifiedConfig();
                  Settings.Instance.OnConfigurationChanged();
                  MessageBox.Show("导入成功！", "提示",
                      MessageBoxButton.OK, MessageBoxImage.Information);
               }
               else
               {
                  MessageBox.Show("文件中没有 Rules", "提示",
                      MessageBoxButton.OK, MessageBoxImage.Information);
               }
            }
            catch (Exception ex)
            {
               MessageBox.Show($"导入失败：{ex.Message}", "错误",
                   MessageBoxButton.OK, MessageBoxImage.Error);
            }
         }
      }

      #endregion

      private void CommentToken_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
      }
      
      #region 插入指示器 Adorner

      private void ShowInsertionAdorner(ListBox listBox, int index)
      {
         if (listBox == null) return;
         
         _adornerLayer = AdornerLayer.GetAdornerLayer(listBox);
         if (_adornerLayer == null) return;
         
         // 移除旧的指示器
         RemoveInsertionAdorner();
         
         // 计算插入位置的 Y 坐标（相对于 ListBox）
         double y;
         if (index <= 0)
         {
            y = 0;
         }
         else if (index >= listBox.Items.Count)
         {
            // 在最后位置
            var lastContainer = listBox.ItemContainerGenerator.ContainerFromIndex(listBox.Items.Count - 1) as ListBoxItem;
            if (lastContainer != null)
            {
               y = lastContainer.TranslatePoint(new Point(0, lastContainer.RenderSize.Height), listBox).Y;
            }
            else
            {
               y = listBox.RenderSize.Height;
            }
         }
         else
         {
            var container = listBox.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
            if (container != null)
            {
               y = container.TranslatePoint(new Point(0, 0), listBox).Y;
            }
            else
            {
               y = index * 40; // 估算值
            }
         }
         
         // 在 ListBox 上创建 Adorner
         _insertionAdorner = new InsertionAdorner(listBox, y, listBox.RenderSize.Width);
         _adornerLayer.Add(_insertionAdorner);
      }
      
      private void RemoveInsertionAdorner()
      {
         if (_insertionAdorner != null && _adornerLayer != null)
         {
            _adornerLayer.Remove(_insertionAdorner);
            _insertionAdorner = null;
         }
      }
      
      #endregion
   }
   
   /// <summary>
   /// 插入位置指示器 Adorner（蓝色线条）
   /// </summary>
   public class InsertionAdorner : Adorner
   {
      private readonly double _y;
      private readonly double _width;
      private readonly Pen _pen;
      
      public InsertionAdorner(UIElement adornedElement, double y, double width)
          : base(adornedElement)
      {
         _y = y;
         _width = width;
         _pen = new Pen(Brushes.Blue, 2);
      }
      
      protected override void OnRender(DrawingContext drawingContext)
      {
         base.OnRender(drawingContext);
         
         // 绘制蓝色水平线
         drawingContext.DrawLine(_pen, new Point(0, _y), new Point(_width, _y));
      }
   }

   public class CommentsPlusConfig
   {
      public string Version { get; set; } = "1.0";
      public DateTime ExportDate { get; set; }
      public System.Collections.Generic.List<CommentTokenData> Rules { get; set; } = new System.Collections.Generic.List<CommentTokenData>();
   }

   public class CommentTokenData
   {
      public string CurrentValue { get; set; }
      public string DefaultValue { get; set; }
      public string ColorHex { get; set; }
      public bool IsBold { get; set; }
      public bool IsItalic { get; set; }
      public bool HasUnderline { get; set; }
      public bool HasStrikethrough { get; set; }
      public bool IsForegroundActive { get; set; }
      public BackgroundStyleData BackgroundStyle { get; set; }

      public static CommentTokenData FromCommentToken(CommentToken token)
      {
         return new CommentTokenData
         {
            CurrentValue = token.CurrentValue,
            DefaultValue = token.DefaultValue,
            ColorHex = token.ColorHex,
            IsBold = token.IsBold,
            IsItalic = token.IsItalic,
            HasUnderline = token.HasUnderline,
            HasStrikethrough = token.HasStrikethrough,
            IsForegroundActive = token.IsForegroundActive,
            BackgroundStyle = BackgroundStyleData.FromBackgroundStyle(token.BackgroundStyle)
         };
      }

      public CommentToken ToCommentToken()
      {
         var token = new CommentToken(
             type: CommentType.Important,
             defaultValue: DefaultValue ?? CurrentValue,
             value: CurrentValue,
             colorHex: ColorHex ?? "#FFFF0000")
         {
            IsBold = IsBold,
            IsItalic = IsItalic,
            HasUnderline = HasUnderline,
            HasStrikethrough = HasStrikethrough,
            IsForegroundActive = IsForegroundActive,
            IsDynamic = true,
            RuleId = Guid.NewGuid().ToString(),
            BackgroundStyle = BackgroundStyle?.ToBackgroundStyle() ?? new BackgroundStyle()
         };
         return token;
      }
   }

   public class BackgroundStyleData
   {
      public bool IsActive { get; set; }
      public string ColorHex { get; set; }
      public string Shape { get; set; }
      public string Blur { get; set; }
      public string Alpha { get; set; }
      public bool IsCaseSensitive { get; set; }
      public bool AllowPartialMatch { get; set; }

      public static BackgroundStyleData FromBackgroundStyle(BackgroundStyle style)
      {
         if (style == null) return null;
         return new BackgroundStyleData
         {
            IsActive = style.IsActive,
            ColorHex = style.ColorHex,
            Shape = style.Shape,
            Blur = style.Blur,
            Alpha = style.Alpha,
            IsCaseSensitive = style.IsCaseSensitive,
            AllowPartialMatch = style.AllowPartialMatch
         };
      }

      public BackgroundStyle ToBackgroundStyle()
      {
         return new BackgroundStyle
         {
            IsActive = IsActive,
            ColorHex = ColorHex,
            Shape = Shape ?? "Tag",
            Blur = Blur ?? "None",
            Alpha = Alpha ?? "1/10",
            IsCaseSensitive = IsCaseSensitive,
            AllowPartialMatch = AllowPartialMatch
         };
      }
   }
}
