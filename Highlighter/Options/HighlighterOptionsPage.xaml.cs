using Highlighter.Core;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Highlighter.Options
{
    // XAML 使用的 ColorToBrushConverter 包装器
    public class ColorToBrushConverter : Highlighter.Core.ColorToBrushConverter
    {
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("A8B5C3D2-E4F5-6789-0123-456789ABCDEF")]
    public class HighlighterOptionsPage : UIElementDialogPage
    {
        private HighlighterOptionsPageControl control;

        protected override System.Windows.UIElement Child
        {
            get
            {
                if (control == null)
                {
                    // 确保设置已加载
                    Options.Instance.Load();

                    control = new HighlighterOptionsPageControl();
                    control.DataContext = Options.Instance;
                }
                return control;
            }
        }

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();
            Options.Instance.Save();
        }

        public override void LoadSettingsFromStorage()
        {
            base.LoadSettingsFromStorage();
            Options.Instance.Load();
        }
    }

    public partial class HighlighterOptionsPageControl : System.Windows.Controls.Grid
    {
        // 拖拽相关变量
        private Point _dragStartPoint;
        private object _draggedItem;
        private ListBox _sourceListBox;

        public HighlighterOptionsPageControl()
        {
            InitializeComponent();
        }

        #region Global Rules 拖拽事件

        private void GlobalRulesListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _sourceListBox = sender as ListBox;
            _draggedItem = _sourceListBox?.SelectedItem;
        }

        private void GlobalRulesListBox_MouseMove(object sender, MouseEventArgs e)
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

        private void GlobalRulesListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(HighlightTag)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void GlobalRulesListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void GlobalRulesListBox_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void GlobalRulesListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(HighlightTag)))
            {
                var droppedItem = e.Data.GetData(typeof(HighlightTag)) as HighlightTag;
                var targetListBox = sender as ListBox;
                var options = DataContext as Options;

                if (droppedItem != null && targetListBox != null && options != null)
                {
                    // 获取目标位置
                    var targetItem = GetListBoxItemAtPosition(targetListBox, e.GetPosition(targetListBox));
                    var targetTag = targetItem?.DataContext as HighlightTag;

                    var tags = options.ColorTags?.ToList();
                    if (tags != null)
                    {
                        int oldIndex = tags.IndexOf(droppedItem);
                        int newIndex = targetTag != null ? tags.IndexOf(targetTag) : tags.Count - 1;

                        if (oldIndex >= 0 && newIndex >= 0 && oldIndex != newIndex)
                        {
                            tags.RemoveAt(oldIndex);
                            tags.Insert(newIndex, droppedItem);
                            options.ColorTags = tags.ToArray();
                            GlobalRulesListBox.ItemsSource = options.ColorTags;
                            GlobalRulesListBox.SelectedItem = droppedItem;
                        }
                    }
                }
            }
            e.Handled = true;
            _draggedItem = null;
            _sourceListBox = null;
        }

        #endregion

        #region Solution Rules 拖拽事件

        private void SolutionRulesListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _sourceListBox = sender as ListBox;
            _draggedItem = _sourceListBox?.SelectedItem;
        }

        private void SolutionRulesListBox_MouseMove(object sender, MouseEventArgs e)
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

        private void SolutionRulesListBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(HighlightTag)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void SolutionRulesListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void SolutionRulesListBox_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void SolutionRulesListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(HighlightTag)))
            {
                var droppedItem = e.Data.GetData(typeof(HighlightTag)) as HighlightTag;
                var targetListBox = sender as ListBox;
                var options = DataContext as Options;

                if (droppedItem != null && targetListBox != null && options != null)
                {
                    // 获取目标位置
                    var targetItem = GetListBoxItemAtPosition(targetListBox, e.GetPosition(targetListBox));
                    var targetTag = targetItem?.DataContext as HighlightTag;

                    var tags = options.SolutionTags?.ToList();
                    if (tags != null)
                    {
                        int oldIndex = tags.IndexOf(droppedItem);
                        int newIndex = targetTag != null ? tags.IndexOf(targetTag) : tags.Count - 1;

                        if (oldIndex >= 0 && newIndex >= 0 && oldIndex != newIndex)
                        {
                            tags.RemoveAt(oldIndex);
                            tags.Insert(newIndex, droppedItem);
                            options.SolutionTags = tags.ToArray();
                            SolutionRulesListBox.ItemsSource = options.SolutionTags;
                            SolutionRulesListBox.SelectedItem = droppedItem;
                        }
                    }
                }
            }
            e.Handled = true;
            _draggedItem = null;
            _sourceListBox = null;
        }

        #endregion

        // 辅助方法：获取指定位置的 ListBoxItem
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

        #region 按钮事件处理

        private void AddGlobalRuleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var options = DataContext as Options;
            if (options != null)
            {
                var newTag = new HighlightTag("NewRule");
                var tags = options.ColorTags?.ToList() ?? new System.Collections.Generic.List<HighlightTag>();
                tags.Add(newTag);
                options.ColorTags = tags.ToArray();
                GlobalRulesListBox.ItemsSource = options.ColorTags;
                GlobalRulesListBox.SelectedItem = newTag;
                GlobalRulesListBox.ScrollIntoView(newTag);
            }
        }

        private void DeleteGlobalRuleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button?.Tag as HighlightTag;
            var options = DataContext as Options;
            if (tag != null && options != null)
            {
                var tags = options.ColorTags?.ToList() ?? new System.Collections.Generic.List<HighlightTag>();
                tags.Remove(tag);
                options.ColorTags = tags.ToArray();
                GlobalRulesListBox.ItemsSource = options.ColorTags;
            }
        }

        private void DeleteSolutionRuleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button?.Tag as HighlightTag;
            var options = DataContext as Options;
            if (tag != null && options != null)
            {
                var tags = options.SolutionTags?.ToList() ?? new System.Collections.Generic.List<HighlightTag>();
                tags.Remove(tag);
                options.SolutionTags = tags.ToArray();
                SolutionRulesListBox.ItemsSource = options.SolutionTags;
            }
        }

        private void CopyToSolutionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button?.Tag as HighlightTag;
            var options = DataContext as Options;
            if (tag != null && options != null)
            {
                var newTag = new HighlightTag(tag.Criteria)
                {
                    Color = tag.Color,
                    Shape = tag.Shape,
                    Blur = tag.Blur,
                    Alpha = tag.Alpha,
                    IsActive = tag.IsActive,
                    AllowPartialMatch = tag.AllowPartialMatch,
                    IsCaseSensitive = tag.IsCaseSensitive
                };
                var tags = options.SolutionTags?.ToList() ?? new System.Collections.Generic.List<HighlightTag>();
                tags.Add(newTag);
                options.SolutionTags = tags.ToArray();
                SolutionRulesListBox.ItemsSource = options.SolutionTags;
            }
        }

        private void CopyAllFromGlobalButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var options = DataContext as Options;
            if (options != null && options.ColorTags != null && options.ColorTags.Length > 0)
            {
                var result = System.Windows.MessageBox.Show(
                    "这将复制所有 Global Rules 到 Solution Rules，是否继续？",
                    "确认复制",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    var tags = options.SolutionTags?.ToList() ?? new System.Collections.Generic.List<HighlightTag>();
                    foreach (var globalTag in options.ColorTags)
                    {
                        var newTag = new HighlightTag(globalTag.Criteria)
                        {
                            Color = globalTag.Color,
                            Shape = globalTag.Shape,
                            Blur = globalTag.Blur,
                            Alpha = globalTag.Alpha,
                            IsActive = globalTag.IsActive,
                            AllowPartialMatch = globalTag.AllowPartialMatch,
                            IsCaseSensitive = globalTag.IsCaseSensitive
                        };
                        tags.Add(newTag);
                    }
                    options.SolutionTags = tags.ToArray();
                    SolutionRulesListBox.ItemsSource = options.SolutionTags;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("没有可用的 Global Rules", "提示",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void ImportFromGlobalButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var options = DataContext as Options;
            if (options != null && options.ColorTags != null && options.ColorTags.Length > 0)
            {
                var dialog = new System.Windows.Window
                {
                    Title = "从 Global Rules 选择规则（按住 Ctrl 或 Shift 多选）",
                    Width = 400,
                    Height = 350,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                };

                var grid = new System.Windows.Controls.Grid();
                grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
                grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
                grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });

                var instructionText = new System.Windows.Controls.TextBlock
                {
                    Text = "请选择要导入的规则（支持多选）：",
                    Margin = new System.Windows.Thickness(10)
                };
                System.Windows.Controls.Grid.SetRow(instructionText, 0);
                grid.Children.Add(instructionText);

                var listBox = new System.Windows.Controls.ListBox
                {
                    ItemsSource = options.ColorTags,
                    DisplayMemberPath = "Criteria",
                    SelectionMode = System.Windows.Controls.SelectionMode.Extended,
                    Margin = new System.Windows.Thickness(10)
                };
                System.Windows.Controls.Grid.SetRow(listBox, 1);
                grid.Children.Add(listBox);

                var buttonPanel = new System.Windows.Controls.StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    Margin = new System.Windows.Thickness(10)
                };

                var okButton = new System.Windows.Controls.Button
                {
                    Content = "导入选中项",
                    Width = 100,
                    Margin = new System.Windows.Thickness(5)
                };
                okButton.Click += (s, args) =>
                {
                    var selectedTags = listBox.SelectedItems.Cast<HighlightTag>().ToList();
                    if (selectedTags.Count > 0)
                    {
                        var tags = options.SolutionTags?.ToList() ?? new System.Collections.Generic.List<HighlightTag>();
                        foreach (var selectedTag in selectedTags)
                        {
                            var newTag = new HighlightTag(selectedTag.Criteria)
                            {
                                Color = selectedTag.Color,
                                Shape = selectedTag.Shape,
                                Blur = selectedTag.Blur,
                                Alpha = selectedTag.Alpha,
                                IsActive = selectedTag.IsActive,
                                AllowPartialMatch = selectedTag.AllowPartialMatch,
                                IsCaseSensitive = selectedTag.IsCaseSensitive
                            };
                            tags.Add(newTag);
                        }
                        options.SolutionTags = tags.ToArray();
                        SolutionRulesListBox.ItemsSource = options.SolutionTags;
                        dialog.Close();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("请至少选择一个规则", "提示",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                };

                var cancelButton = new System.Windows.Controls.Button
                {
                    Content = "取消",
                    Width = 80,
                    Margin = new System.Windows.Thickness(5)
                };
                cancelButton.Click += (s, args) => dialog.Close();

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);
                System.Windows.Controls.Grid.SetRow(buttonPanel, 2);
                grid.Children.Add(buttonPanel);

                dialog.Content = grid;
                dialog.ShowDialog();
            }
            else
            {
                System.Windows.MessageBox.Show("没有可用的 Global Rules", "提示",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void ClearSolutionRulesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var options = DataContext as Options;
            if (options != null && options.SolutionTags != null && options.SolutionTags.Length > 0)
            {
                var result = System.Windows.MessageBox.Show(
                    "确定要清除所有 Solution Rules 吗？",
                    "确认清除",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    options.SolutionTags = System.Array.Empty<HighlightTag>();
                    SolutionRulesListBox.ItemsSource = options.SolutionTags;
                }
            }
        }

        private void ExportGlobalRulesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var options = DataContext as Options;
            if (options != null && options.ColorTags != null && options.ColorTags.Length > 0)
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON 文件 (*.json)|*.json",
                    DefaultExt = ".json",
                    FileName = "Highlighter_GlobalRules.json",
                    Title = "导出 Global Rules"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    try
                    {
                        var config = new HighlighterConfig();
                        foreach (var tag in options.ColorTags)
                        {
                            config.GlobalRules.Add(HighlightTagData.FromHighlightTag(tag));
                        }
                        config.Performance = options.Performance.ToString();

                        System.IO.File.WriteAllText(saveDialog.FileName, config.ToJson());
                        System.Windows.MessageBox.Show("导出成功！", "提示",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.MessageBox.Show($"导出失败：{ex.Message}", "错误",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("没有可用的 Global Rules", "提示",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void ImportGlobalRulesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON 文件 (*.json)|*.json",
                Title = "导入 Global Rules"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    var json = System.IO.File.ReadAllText(openDialog.FileName);
                    var config = HighlighterConfig.FromJson(json);

                    if (config.GlobalRules != null && config.GlobalRules.Count > 0)
                    {
                        var result = System.Windows.MessageBox.Show(
                            "选择导入模式：\n\n是 - 覆盖现有规则\n否 - 合并现有规则",
                            "导入模式",
                            System.Windows.MessageBoxButton.YesNoCancel,
                            System.Windows.MessageBoxImage.Question);

                        if (result == System.Windows.MessageBoxResult.Cancel)
                            return;

                        var options = DataContext as Options;
                        if (options != null)
                        {
                            var tags = new System.Collections.Generic.List<HighlightTag>();

                            if (result == System.Windows.MessageBoxResult.No && options.ColorTags != null)
                            {
                                tags.AddRange(options.ColorTags);
                            }

                            foreach (var tagData in config.GlobalRules)
                            {
                                tags.Add(tagData.ToHighlightTag());
                            }

                            options.ColorTags = tags.ToArray();
                            GlobalRulesListBox.ItemsSource = options.ColorTags;

                            System.Windows.MessageBox.Show("导入成功！", "提示",
                                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("文件中没有 Global Rules", "提示",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show($"导入失败：{ex.Message}", "错误",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void ExportSolutionRulesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var options = DataContext as Options;
            if (options != null && options.SolutionTags != null && options.SolutionTags.Length > 0)
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON 文件 (*.json)|*.json",
                    DefaultExt = ".json",
                    FileName = "Highlighter_SolutionRules.json",
                    Title = "导出 Solution Rules"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    try
                    {
                        var config = new HighlighterConfig();
                        foreach (var tag in options.SolutionTags)
                        {
                            config.SolutionRules.Add(HighlightTagData.FromHighlightTag(tag));
                        }

                        System.IO.File.WriteAllText(saveDialog.FileName, config.ToJson());
                        System.Windows.MessageBox.Show("导出成功！", "提示",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.MessageBox.Show($"导出失败：{ex.Message}", "错误",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("没有可用的 Solution Rules", "提示",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        private void ImportSolutionRulesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON 文件 (*.json)|*.json",
                Title = "导入 Solution Rules"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    var json = System.IO.File.ReadAllText(openDialog.FileName);
                    var config = HighlighterConfig.FromJson(json);

                    if (config.SolutionRules != null && config.SolutionRules.Count > 0)
                    {
                        var result = System.Windows.MessageBox.Show(
                            "选择导入模式：\n\n是 - 覆盖现有规则\n否 - 合并现有规则",
                            "导入模式",
                            System.Windows.MessageBoxButton.YesNoCancel,
                            System.Windows.MessageBoxImage.Question);

                        if (result == System.Windows.MessageBoxResult.Cancel)
                            return;

                        var options = DataContext as Options;
                        if (options != null)
                        {
                            var tags = new System.Collections.Generic.List<HighlightTag>();

                            if (result == System.Windows.MessageBoxResult.No && options.SolutionTags != null)
                            {
                                tags.AddRange(options.SolutionTags);
                            }

                            foreach (var tagData in config.SolutionRules)
                            {
                                tags.Add(tagData.ToHighlightTag());
                            }

                            options.SolutionTags = tags.ToArray();
                            SolutionRulesListBox.ItemsSource = options.SolutionTags;

                            System.Windows.MessageBox.Show("导入成功！", "提示",
                                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("文件中没有 Solution Rules", "提示",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show($"导入失败：{ex.Message}", "错误",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void PerformanceComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var options = DataContext as Options;
            if (options != null && PerformanceComboBox.SelectedItem is System.Windows.Controls.ComboBoxItem item)
            {
                if (item.Tag?.ToString() == "Normal")
                    options.Performance = Core.Performance.Normal;
                else if (item.Tag?.ToString() == "Fast")
                    options.Performance = Core.Performance.Fast;
                else if (item.Tag?.ToString() == "NoEffects")
                    options.Performance = Core.Performance.NoEffects;
            }
        }

        #endregion
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("09A12CD2-205F-478E-A8C5-2678C7E1772B")]
    public class Options
    {
        private string path = null;

        #region Singleton

        private static volatile Options instance;
        private static readonly object syncLock = new object();

        public static Options Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                            instance = new Options();
                    }
                }

                return instance;
            }
        }

        #endregion Singleton

        #region Events

        public event EventHandler<Options> Saved;

        #endregion Events

        private Options()
        {
            Helper.InitDefaults();

            // 初始化为空，将在第一次访问时加载
            ColorTags = System.Array.Empty<HighlightTag>();
            SolutionTags = System.Array.Empty<HighlightTag>();

            VS.Events.SolutionEvents.OnBeforeOpenSolution += SolutionEvents_OnBeforeOpenSolution;
            VS.Events.SolutionEvents.OnAfterCloseSolution += SolutionEvents_OnAfterCloseSolution;
        }

        // 全局设置存储路径
        private static readonly string GlobalSettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Highlighter", "Settings.xml");

        private void SolutionEvents_OnAfterCloseSolution()
        {
            SolutionTags = System.Array.Empty<HighlightTag>();
            path = null;
        }

        public void Save()
        {
            // 保存全局设置
            SaveGlobalSettings();

            // 保存解决方案级别设置
            if (path != null)
            {
                try
                {
                    if (SolutionTags != null && SolutionTags.Any())
                    {
                        var xs = new XmlSerializer(SolutionTags.GetType());
                        using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                        xs.Serialize(fs, SolutionTags);
                    }
                }
                catch (System.Exception)
                {
                }
            }

            // 触发 Saved 事件
            Saved?.Invoke(this, this);
        }

        private void SaveGlobalSettings()
        {
            try
            {
                var settingsDir = Path.GetDirectoryName(GlobalSettingsPath);
                if (!Directory.Exists(settingsDir))
                {
                    Directory.CreateDirectory(settingsDir);
                }

                var xs = new XmlSerializer(typeof(GlobalSettings));
                var settings = new GlobalSettings
                {
                    ColorTags = this.ColorTags,
                    Performance = this.Performance
                };

                using var fs = new FileStream(GlobalSettingsPath, FileMode.Create, FileAccess.Write);
                xs.Serialize(fs, settings);
            }
            catch (System.Exception)
            {
            }
        }

        private bool isLoaded = false;

        public void Load()
        {
            if (isLoaded) return;

            LoadGlobalSettings();

            // 如果 ColorTags 为空，使用默认标签
            if (ColorTags == null || ColorTags.Length == 0)
            {
                ColorTags = Helper.GetFillerTags().ToArray();
            }

            isLoaded = true;
        }

        private void LoadGlobalSettings()
        {
            try
            {
                if (File.Exists(GlobalSettingsPath))
                {
                    var xs = new XmlSerializer(typeof(GlobalSettings));
                    using var fs = new FileStream(GlobalSettingsPath, FileMode.Open, FileAccess.Read);
                    var settings = xs.Deserialize(fs) as GlobalSettings;

                    if (settings != null)
                    {
                        this.ColorTags = settings.ColorTags ?? System.Array.Empty<HighlightTag>();
                        this.Performance = settings.Performance;
                    }
                }
            }
            catch (System.Exception)
            {
                // 加载失败时保持空数组，稍后会使用默认值
            }
        }

        private void SolutionEvents_OnBeforeOpenSolution(string obj)
        {
            try
            {
                path = $"{Path.GetDirectoryName(obj)}/.vs/Highlighter.xml";

                if (File.Exists(path))
                {
                    var xs = new XmlSerializer(SolutionTags.GetType());
                    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    SolutionTags = xs.Deserialize(fs) as HighlightTag[];
                }
                else
                {
                    SolutionTags = System.Array.Empty<HighlightTag>();
                }
            }
            catch (System.Exception)
            {
                SolutionTags = System.Array.Empty<HighlightTag>();
            }
        }

        // 用于序列化全局设置的辅助类
        public class GlobalSettings
        {
            public HighlightTag[] ColorTags { get; set; }
            public Core.Performance Performance { get; set; }
        }

        [Category("Tags")]
        [DisplayName("Global Rules")]
        [Description("These rules are applied across all projects.")]
        public HighlightTag[] ColorTags { get; set; }

        [Category("Tags")]
        [DisplayName("Solution Rules")]
        [Description("These rules are applied only to the current solution.")]
        public HighlightTag[] SolutionTags { get; set; }

        [Category("Appearance")]
        [DisplayName("Performance")]
        [Description("Choose the performance level.")]
        [DefaultValue(Core.Performance.Normal)]
        [TypeConverter(typeof(EnumConverter))]
        public Core.Performance Performance { get; set; } = Core.Performance.Normal;

        // 可用颜色列表
        public System.Collections.Generic.List<Color> AvailableColors => Helper.colors;

        // 可用形状列表
        public System.Collections.Generic.List<TagShape> AvailableShapes =>
            System.Enum.GetValues(typeof(TagShape)).Cast<TagShape>().ToList();

        // 可用模糊度列表
        public System.Collections.Generic.List<BlurIntensity> AvailableBlurLevels =>
            System.Enum.GetValues(typeof(BlurIntensity)).Cast<BlurIntensity>().ToList();

        // 可用透明度列表
        public System.Collections.Generic.List<FillAlpha> AvailableAlphaLevels =>
            System.Enum.GetValues(typeof(FillAlpha)).Cast<FillAlpha>().ToList();
    }
}
