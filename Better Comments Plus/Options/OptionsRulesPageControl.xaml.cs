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
using Microsoft.VisualStudio.Shell;
using DTE = EnvDTE.DTE;
using DTEWindow = EnvDTE.Window;

namespace BetterCommentsPlus.Options
{
   public partial class OptionsRulesPageControl
   {
      private Point _dragStartPoint;
      private CommentRule _draggedItem;
      private int _draggedIndex = -1;
      private bool _isDragging;
      private ListBoxItem _dragSourceItem;
      private double _originalOpacity;
      private ListBox _currentDraggingListBox;
      private AdornerLayer _adornerLayer;
      private InsertionAdorner _insertionAdorner;

      public OptionsRulesPageControl()
      {
         DataContext = Settings.Instance;
         InitializeComponent();
         Loaded += OptionsRulesPageControl_Loaded;
      }

      private void OptionsRulesPageControl_Loaded(object sender, RoutedEventArgs e)
      {
         // 尝试通过 Visual Studio 服务获取解决方案路径
         try
         {
            // 如果还没有设置解决方案路径，尝试通过 VS 服务获取
            // 注意：这里我们需要通过包来获取服务，但在这个控件中不太容易
            // 不过，VSPackage 已经在初始化时尝试获取了
            // 这里我们主要是确保用户知道问题所在
         }
         catch
         {
            // 静默处理错误
         }
      }

      #region 颜色选择器相关

      private void ColorPreviewBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         if (sender is Border border && border.DataContext is CommentRule rule)
         {
            Color? initialColor = null;
            if (!string.IsNullOrEmpty(rule.ColorHex))
            {
               try
               {
                  initialColor = (Color)ColorConverter.ConvertFromString(rule.ColorHex);
               }
               catch { }
            }

            var dialog = new ColorPickerDialog(initialColor)
            {
               Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.SelectedColor.HasValue)
            {
               rule.ColorHex = $"#{dialog.SelectedColor.Value.A:X2}{dialog.SelectedColor.Value.R:X2}{dialog.SelectedColor.Value.G:X2}{dialog.SelectedColor.Value.B:X2}";
               Settings.Instance.OnConfigurationChanged();
            }
         }
      }

      private void BackgroundColorPreviewBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         if (sender is Border border && border.DataContext is CommentRule rule && rule.Background != null)
         {
            Color? initialColor = null;
            if (!string.IsNullOrEmpty(rule.Background.ColorHex))
            {
               try
               {
                  initialColor = (Color)ColorConverter.ConvertFromString(rule.Background.ColorHex);
               }
               catch { }
            }

            var dialog = new ColorPickerDialog(initialColor)
            {
               Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.SelectedColor.HasValue)
            {
               rule.Background.ColorHex = $"#{dialog.SelectedColor.Value.A:X2}{dialog.SelectedColor.Value.R:X2}{dialog.SelectedColor.Value.G:X2}{dialog.SelectedColor.Value.B:X2}";
               Settings.Instance.OnConfigurationChanged();
            }
         }
      }

      #endregion

      #region 拖拽相关

      private void DragGrip_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         _dragStartPoint = e.GetPosition(null);
         
         if (sender is Border grip && grip.DataContext is CommentRule rule)
         {
            _draggedItem = rule;
            _currentDraggingListBox = FindParentListBox(grip);
            
            if (_currentDraggingListBox == GlobalRulesList)
                _draggedIndex = Settings.Instance.GlobalRules.IndexOf(rule);
            else if (_currentDraggingListBox == SolutionRulesList)
                _draggedIndex = Settings.Instance.SolutionRules.IndexOf(rule);
            
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

      private void GlobalRulesList_MouseMove(object sender, MouseEventArgs e)
      {
         HandleListBoxMouseMove(sender as ListBox, e, Settings.Instance.GlobalRules);
      }

      private void SolutionRulesList_MouseMove(object sender, MouseEventArgs e)
      {
         HandleListBoxMouseMove(sender as ListBox, e, Settings.Instance.SolutionRules);
      }

      private void HandleListBoxMouseMove(ListBox listBox, MouseEventArgs e, ObservableCollection<CommentRule> rules)
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

      private void GlobalRulesList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         HandleListBoxMouseUp(sender as ListBox, e, Settings.Instance.GlobalRules, GlobalRulesList);
      }

      private void SolutionRulesList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         HandleListBoxMouseUp(sender as ListBox, e, Settings.Instance.SolutionRules, SolutionRulesList);
      }

      private void HandleListBoxMouseUp(ListBox listBox, MouseButtonEventArgs e, ObservableCollection<CommentRule> rules, ListBox targetListBox)
      {
         if (_isDragging && _draggedIndex >= 0)
         {
            var settings = DataContext as Settings;
            var targetRules = listBox == GlobalRulesList ? settings.GlobalRules : settings.SolutionRules;

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
                  targetRules.Move(_draggedIndex, newIndex);
                  targetListBox.SelectedItem = _draggedItem;
               }
            }

            Settings.Instance.IsDragging = false;
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
         AddRule(Settings.Instance.GlobalRules, GlobalRulesList);
      }

      private void AddSolutionButton_Click(object sender, RoutedEventArgs e)
      {
         // 确保我们有解决方案路径
         TryGetSolutionPath();
         AddRule(Settings.Instance.SolutionRules, SolutionRulesList);
      }
      
      private void TryGetSolutionPath()
      {
         try
         {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            if (dte != null && dte.Solution != null && dte.Solution.IsOpen)
            {
               string solutionPath = dte.Solution.FullName;
               if (!string.IsNullOrEmpty(solutionPath))
               {
                  Settings.Instance.SetCurrentSolutionPath(solutionPath);
               }
            }
         }
         catch
         {
            // 静默处理错误
         }
      }

      private void AddRule(ObservableCollection<CommentRule> rules, ListBox listBox)
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
         } while (settings.GlobalRules.Any(t => t.Criteria == newCriteria) ||
                  settings.SolutionRules.Any(t => t.Criteria == newCriteria));

         var newRule = new CommentRule
         {
            Id = Guid.NewGuid().ToString(),
            Criteria = newCriteria,
            ColorHex = "#FFFF0000",
            IsPredefined = false
         };

         rules.Add(newRule);
         listBox.SelectedItem = newRule;
      }

      #endregion

      #region 删除按钮

      private void DeleteButton_Click(object sender, RoutedEventArgs e)
      {
         if (sender is Button button && button.Tag is CommentRule rule)
         {
            var settings = DataContext as Settings;
            if (settings != null)
            {
               if (settings.GlobalRules.Contains(rule))
                  settings.GlobalRules.Remove(rule);
               else if (settings.SolutionRules.Contains(rule))
                  settings.SolutionRules.Remove(rule);
            }
         }
      }

      #endregion

      #region 上下移动按钮

      private void MoveUpGlobalButton_Click(object sender, RoutedEventArgs e)
      {
         MoveUp(Settings.Instance.GlobalRules, GlobalRulesList);
      }

      private void MoveDownGlobalButton_Click(object sender, RoutedEventArgs e)
      {
         MoveDown(Settings.Instance.GlobalRules, GlobalRulesList);
      }

      private void MoveUpSolutionButton_Click(object sender, RoutedEventArgs e)
      {
         MoveUp(Settings.Instance.SolutionRules, SolutionRulesList);
      }

      private void MoveDownSolutionButton_Click(object sender, RoutedEventArgs e)
      {
         MoveDown(Settings.Instance.SolutionRules, SolutionRulesList);
      }

      private void MoveUp(ObservableCollection<CommentRule> rules, ListBox listBox)
      {
         var settings = DataContext as Settings;
         if (settings == null || listBox.SelectedItem == null)
            return;

         int selectedIndex = listBox.SelectedIndex;
         if (selectedIndex > 0)
         {
            rules.Move(selectedIndex, selectedIndex - 1);
            listBox.SelectedIndex = selectedIndex - 1;
            Settings.Instance.OnConfigurationChanged();
         }
      }

      private void MoveDown(ObservableCollection<CommentRule> rules, ListBox listBox)
      {
         var settings = DataContext as Settings;
         if (settings == null || listBox.SelectedItem == null)
            return;

         int selectedIndex = listBox.SelectedIndex;
         if (selectedIndex >= 0 && selectedIndex < rules.Count - 1)
         {
            rules.Move(selectedIndex, selectedIndex + 1);
            listBox.SelectedIndex = selectedIndex + 1;
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
            Settings.Instance.CopyAllFromGlobalToSolutionRules();
         }
      }

      private void ImportFromGlobalButton_Click(object sender, RoutedEventArgs e)
      {
         if (Settings.Instance.GlobalRules.Count > 0)
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
               ItemsSource = Settings.Instance.GlobalRules,
               DisplayMemberPath = "Criteria",
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
               var selectedRules = listBox.SelectedItems.Cast<CommentRule>().ToList();
               if (selectedRules.Count > 0)
               {
                  foreach (var selectedRule in selectedRules)
                  {
                     var newRule = new CommentRule
                     {
                        Id = Guid.NewGuid().ToString(),
                        Criteria = selectedRule.Criteria,
                        ColorHex = selectedRule.ColorHex,
                        IsBold = selectedRule.IsBold,
                        IsItalic = selectedRule.IsItalic,
                        HasUnderline = selectedRule.HasUnderline,
                        HasStrikethrough = selectedRule.HasStrikethrough,
                        IsForegroundActive = selectedRule.IsForegroundActive,
                        IsPredefined = false,
                        Background = new Background
                        {
                           IsActive = selectedRule.Background?.IsActive ?? false,
                           ColorHex = selectedRule.Background?.ColorHex,
                           Shape = selectedRule.Background?.Shape ?? "Tag",
                           Blur = selectedRule.Background?.Blur ?? "None",
                           Alpha = selectedRule.Background?.Alpha ?? "1/10",
                           IsCaseSensitive = selectedRule.Background?.IsCaseSensitive ?? true,
                           AllowPartialMatch = selectedRule.Background?.AllowPartialMatch ?? false
                        }
                     };
                     Settings.Instance.SolutionRules.Add(newRule);
                  }
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
            Settings.Instance.ClearSolutionRules();
         }
      }

      #endregion

      #region 导入导出

      private void ExportGlobalButton_Click(object sender, RoutedEventArgs e)
      {
         ExportRules(Settings.Instance.GlobalRules, "Global", "BetterCommentsPlus_GlobalRules.json");
      }

      private void ImportGlobalButton_Click(object sender, RoutedEventArgs e)
      {
         ImportRules(Settings.Instance.GlobalRules, "Global");
      }

      private void ExportSolutionButton_Click(object sender, RoutedEventArgs e)
      {
         ExportRules(Settings.Instance.SolutionRules, "Solution", "BetterCommentsPlus_SolutionRules.json");
      }

      private void ImportSolutionButton_Click(object sender, RoutedEventArgs e)
      {
         ImportRules(Settings.Instance.SolutionRules, "Solution");
      }

      private void ExportRules(ObservableCollection<CommentRule> rules, string type, string defaultFileName)
      {
         if (rules.Count == 0)
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
                  Rules = rules.Select(r => CommentRuleData.FromCommentRule(r)).ToList()
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

      private void ImportRules(ObservableCollection<CommentRule> rules, string type)
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
                     foreach (var ruleData in config.Rules)
                     {
                        var newRule = ruleData.ToCommentRule();
                        rules.Add(newRule);
                     }
                  }
                  else
                  {
                     rules.Clear();
                     foreach (var ruleData in config.Rules)
                     {
                        var newRule = ruleData.ToCommentRule();
                        rules.Add(newRule);
                     }
                  }

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
      public System.Collections.Generic.List<CommentRuleData> Rules { get; set; } = new System.Collections.Generic.List<CommentRuleData>();
   }

   public class CommentRuleData
   {
      public string Criteria { get; set; }
      public string ColorHex { get; set; }
      public bool? IsBold { get; set; }
      public bool? IsItalic { get; set; }
      public bool? HasUnderline { get; set; }
      public bool? HasStrikethrough { get; set; }
      public bool? IsForegroundActive { get; set; }
      public BackgroundData Background { get; set; }

      public static CommentRuleData FromCommentRule(CommentRule rule)
      {
         return new CommentRuleData
         {
            Criteria = rule.Criteria,
            ColorHex = rule.ColorHex,
            IsBold = rule.IsBold,
            IsItalic = rule.IsItalic,
            HasUnderline = rule.HasUnderline,
            HasStrikethrough = rule.HasStrikethrough,
            IsForegroundActive = rule.IsForegroundActive,
            Background = BackgroundData.FromBackground(rule.Background)
         };
      }

      public CommentRule ToCommentRule()
      {
         var rule = new CommentRule
         {
            Id = Guid.NewGuid().ToString(),
            Criteria = Criteria ?? string.Empty,
            ColorHex = ColorHex ?? "#FFFF0000",
            IsBold = IsBold,
            IsItalic = IsItalic,
            HasUnderline = HasUnderline,
            HasStrikethrough = HasStrikethrough,
            IsForegroundActive = IsForegroundActive,
            IsPredefined = false,
            Background = Background?.ToBackground() ?? new Background()
         };
         return rule;
      }
   }

   public class BackgroundData
   {
      public bool IsActive { get; set; }
      public string ColorHex { get; set; }
      public string Shape { get; set; }
      public string Blur { get; set; }
      public string Alpha { get; set; }
      public bool IsCaseSensitive { get; set; } = true;
      public bool AllowPartialMatch { get; set; }

      public BackgroundData()
      {
         Shape = "Tag";
         Blur = "None";
         Alpha = "1/10";
      }

      public static BackgroundData FromBackground(Background style)
      {
         if (style == null) return new BackgroundData();
         return new BackgroundData
         {
            IsActive = style.IsActive,
            ColorHex = style.ColorHex,
            Shape = style.Shape ?? "Tag",
            Blur = style.Blur ?? "None",
            Alpha = style.Alpha ?? "1/10",
            IsCaseSensitive = style.IsCaseSensitive,
            AllowPartialMatch = style.AllowPartialMatch
         };
      }

      public Background ToBackground()
      {
         return new Background
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
