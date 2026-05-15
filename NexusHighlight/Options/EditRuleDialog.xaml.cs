using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using MessageBox = System.Windows.MessageBox;

namespace NexusHighlight.Options
{
    public partial class EditRuleDialog
    {
        public EditRuleDialog()
        {
            InitializeComponent();

            Loaded += EditRuleDialog_Loaded;

            btnModify.Click += BtnModify_Click;
            btnCancel.Click += BtnCancel_Click;
            btnDelete.Click += BtnDelete_Click;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this rule?", "Delete Rule", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                delete = true;
                Close();
            }
        }

        private bool loading;
        internal bool delete;
        public CommentRule RuleToEdit { get; set; }
        
        public bool SaveToGlobal => chkSaveToGlobal.IsChecked == true;
        public bool SaveToSolution => chkSaveToSolution.IsChecked == true;
        
        public Button ModifyButton => btnModify;
        public Button DeleteButton => btnDelete;

        private void EditRuleDialog_Loaded(object sender, RoutedEventArgs e)
        {
            Helper.InitDefaults();
            InitializeColorLists();

            txtCriteria.Text = RuleToEdit.Criteria;
            chkFgActive.IsChecked = RuleToEdit.IsForegroundActive;
            chkFgBold.IsChecked = RuleToEdit.IsBold;
            chkFgItalic.IsChecked = RuleToEdit.IsItalic;
            chkFgUnderline.IsChecked = RuleToEdit.HasUnderline;
            chkFgStrikethrough.IsChecked = RuleToEdit.HasStrikethrough;

            chkBgActive.IsChecked = RuleToEdit.Background?.IsActive ?? false;
            chkCaseSensitive.IsChecked = RuleToEdit.Background?.IsCaseSensitive ?? true;
            chkPartialMatch.IsChecked = RuleToEdit.Background?.AllowPartialMatch ?? false;

            loading = true;
            cboShape.SelectedIndex = GetShapeIndex(RuleToEdit.Background?.Shape ?? "Tag");
            cboBlur.SelectedIndex = GetBlurIndex(RuleToEdit.Background?.Blur ?? "None");
            cboAlpha.SelectedIndex = GetAlphaIndex(RuleToEdit.Background?.Alpha ?? "1/10");

            // 添加所有控件事件监听，包括 Active 复选框
            txtCriteria.TextChanged += (_, _) => CreatePreview();
            cboShape.SelectionChanged += (_, _) => CreatePreview();
            cboBlur.SelectionChanged += (_, _) => CreatePreview();
            cboAlpha.SelectionChanged += (_, _) => CreatePreview();
            
            chkFgActive.Checked += (_, _) => CreatePreview();
            chkFgActive.Unchecked += (_, _) => CreatePreview();
            chkBgActive.Checked += (_, _) => CreatePreview();
            chkBgActive.Unchecked += (_, _) => CreatePreview();
            
            chkFgBold.Checked += (_, _) => CreatePreview();
            chkFgBold.Unchecked += (_, _) => CreatePreview();
            chkFgItalic.Checked += (_, _) => CreatePreview();
            chkFgItalic.Unchecked += (_, _) => CreatePreview();
            chkFgUnderline.Checked += (_, _) => CreatePreview();
            chkFgUnderline.Unchecked += (_, _) => CreatePreview();
            chkFgStrikethrough.Checked += (_, _) => CreatePreview();
            chkFgStrikethrough.Unchecked += (_, _) => CreatePreview();

            loading = false;

            CreatePreview();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            RuleToEdit.Criteria = txtCriteria.Text;

            RuleToEdit.IsForegroundActive = chkFgActive.IsChecked == true;
            RuleToEdit.IsBold = chkFgBold.IsChecked == true;
            RuleToEdit.IsItalic = chkFgItalic.IsChecked == true;
            RuleToEdit.HasUnderline = chkFgUnderline.IsChecked == true;
            RuleToEdit.HasStrikethrough = chkFgStrikethrough.IsChecked == true;

            var fgColor = GetSelectedFgColor();
            if (fgColor.HasValue)
            {
                RuleToEdit.ColorHex = $"#{fgColor.Value.A:X2}{fgColor.Value.R:X2}{fgColor.Value.G:X2}{fgColor.Value.B:X2}";
            }

            if (RuleToEdit.Background == null)
                RuleToEdit.Background = new Background();
                
            RuleToEdit.Background.IsActive = chkBgActive.IsChecked == true;
            RuleToEdit.Background.IsCaseSensitive = chkCaseSensitive.IsChecked == true;
            RuleToEdit.Background.AllowPartialMatch = chkPartialMatch.IsChecked == true;

            var bgColor = GetSelectedBgColor();
            if (bgColor.HasValue)
            {
                RuleToEdit.Background.ColorHex = $"#{bgColor.Value.A:X2}{bgColor.Value.R:X2}{bgColor.Value.G:X2}{bgColor.Value.B:X2}";
            }

            RuleToEdit.Background.Shape = GetShapeString(cboShape.SelectedIndex);
            RuleToEdit.Background.Blur = GetBlurString(cboBlur.SelectedIndex);
            RuleToEdit.Background.Alpha = GetAlphaString(cboAlpha.SelectedIndex);

            DialogResult = true;
            Close();
        }

        private void InitializeColorLists()
        {
            string fgHexMatch = RuleToEdit.ColorHex ?? "#57a64a";
            string bgHexMatch = RuleToEdit.Background?.ColorHex ?? "#FF0000";
            
            // 使用 ColorPickerDialog 的颜色矩阵
            var colors = new[]
            {
                "#FEF2ED","#FDECEF","#F7E9F7","#F3EDF9","#ECEFF8","#EAF5FF","#E9F7FD","#E5F7F8","#E4F7F4","#ECF7EC","#F3F8EC","#F2FAE6","#FFFDEA","#FEFBEB","#FFF8EA","#F9F9F9",
                "#FEDDD2","#FBCFD8","#EFCAF0","#E2D1F4","#D1D8F0","#CBE7FE","#C9ECFC","#C2EFF0","#C0F0E8","#D0F0D1","#E3F0D0","#E3F6C5","#FEFBCB","#FCF5CE","#FEEECC","#E6E8EA",
                "#FDB7A5","#F6A0B5","#DD9BE0","#C4A7E9","#A7B3E1","#98CDFD","#95D8F8","#8ADDE2","#87E0D3","#A4E0A7","#C8E2A5","#CBED8E","#FDF398","#F9E89E","#FED998","#C6CACD",
                "#FB9078","#F27396","#C96FD1","#A67FDD","#8090D3","#65B2FC","#62C3F5","#58CBD3","#54D1C1","#7DD182","#ADD37E","#B7E35B","#FCE865","#F6D86F","#FDC165","#A7ABB0",
                "#FA664C","#ED487B","#B449C2","#885BD2","#5E6FC4","#3295FB","#30ACF1","#2CB8C5","#27C2B0","#5AC262","#93C55B","#A7DA2C","#FBDA32","#F3C641","#FDA633","#888D92",
                "#F93920","#E91E63","#9E28B3","#6A3AC7","#3F51B5","#0064FA","#0095EE","#05A4B6","#00B3A1","#3BB346","#7BB63C","#9BD100","#FAC800","#F0B114","#FC8800","#6B7075",
                "#D52515","#C51356","#871E9E","#572FB3","#3342A1","#0062D6","#007BCA","#038698","#009589","#30953B","#649830","#7EAE00","#D0AA00","#C88A0F","#D26700","#555B61",
                "#B2140C","#A20B48","#71168A","#46259E","#28348C","#004FB3","#0063A7","#016979","#00776F","#25772F","#4E7926","#638B00","#A78B00","#A0660A","#A84A00","#41464C",
                "#8E0805","#7E053A","#5C0F75","#361C8A","#1F2878","#003D8F","#004B83","#004D5B","#005955","#1B5924","#395B1B","#486800","#7D6A00","#784606","#7E3100","#2E3238",
                "#6A0103","#5A012B","#490A61","#281475","#171D63","#002C6B","#00355F","#00323D","#003C3A","#113C18","#253D12","#2F4600","#534800","#502B03","#541D00","#1C1F23"
            };
            
            Border selectedFgBorder = null;
            Border selectedBgBorder = null;

            foreach (var colorHex in colors)
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorHex);
                    
                    // 创建前景色选择器
                    var fgBorder = new Border
                    {
                        Width = 30,
                        Height = 30,
                        Margin = new Thickness(2),
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(3),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Tag = colorHex,
                        Background = new SolidColorBrush(color)
                    };
                    
                    fgBorder.MouseLeftButtonDown += (s, e) =>
                    {
                        // 清除其他选中状态
                        foreach (var child in FgColorsWrapPanel.Children)
                        {
                            if (child is Border b) b.BorderThickness = new Thickness(1);
                        }
                        
                        // 设置当前选中
                        fgBorder.BorderThickness = new Thickness(3);
                        fgBorder.BorderBrush = Brushes.Black;
                        
                        // 更新预览
                        if (!loading) CreatePreview();
                    };
                    
                    FgColorsWrapPanel.Children.Add(fgBorder);
                    
                    // 检查是否匹配当前颜色
                    if (color.ToString() == fgHexMatch)
                    {
                        selectedFgBorder = fgBorder;
                    }
                    
                    // 创建背景色选择器
                    var bgBorder = new Border
                    {
                        Width = 30,
                        Height = 30,
                        Margin = new Thickness(2),
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(3),
                        Cursor = System.Windows.Input.Cursors.Hand,
                        Tag = colorHex,
                        Background = new SolidColorBrush(color)
                    };
                    
                    bgBorder.MouseLeftButtonDown += (s, e) =>
                    {
                        // 清除其他选中状态
                        foreach (var child in BgColorsWrapPanel.Children)
                        {
                            if (child is Border b) b.BorderThickness = new Thickness(1);
                        }
                        
                        // 设置当前选中
                        bgBorder.BorderThickness = new Thickness(3);
                        bgBorder.BorderBrush = Brushes.Black;
                        
                        // 更新预览
                        if (!loading) CreatePreview();
                    };
                    
                    BgColorsWrapPanel.Children.Add(bgBorder);
                    
                    // 检查是否匹配当前颜色
                    if (color.ToString() == bgHexMatch)
                    {
                        selectedBgBorder = bgBorder;
                    }
                }
                catch { }
            }
            
            // 设置初始选中状态
            if (selectedFgBorder != null)
            {
                selectedFgBorder.BorderThickness = new Thickness(3);
                selectedFgBorder.BorderBrush = Brushes.Black;
            }
            
            if (selectedBgBorder != null)
            {
                selectedBgBorder.BorderThickness = new Thickness(3);
                selectedBgBorder.BorderBrush = Brushes.Black;
            }
        }
        
        private Color? GetSelectedFgColor()
        {
            foreach (var child in FgColorsWrapPanel.Children)
            {
                if (child is Border border && border.BorderThickness == new Thickness(3) && border.Tag is string hex)
                {
                    try
                    {
                        return (Color)ColorConverter.ConvertFromString(hex);
                    }
                    catch { }
                }
            }
            return null;
        }
        
        private Color? GetSelectedBgColor()
        {
            foreach (var child in BgColorsWrapPanel.Children)
            {
                if (child is Border border && border.BorderThickness == new Thickness(3) && border.Tag is string hex)
                {
                    try
                    {
                        return (Color)ColorConverter.ConvertFromString(hex);
                    }
                    catch { }
                }
            }
            return null;
        }

        private int GetShapeIndex(string shape)
        {
            return shape switch
            {
                "Tag" => 0,
                "TagUnder" => 1,
                "Line" => 2,
                "LineUnder" => 3,
                _ => 0
            };
        }

        private string GetShapeString(int index)
        {
            return index switch
            {
                0 => "Tag",
                1 => "TagUnder",
                2 => "Line",
                3 => "LineUnder",
                _ => "Tag"
            };
        }

        private int GetBlurIndex(string blur)
        {
            return blur switch
            {
                "None" => 0,
                "Low" => 1,
                "Medium" => 2,
                "High" => 3,
                "Ultra" => 4,
                _ => 0
            };
        }

        private string GetBlurString(int index)
        {
            return index switch
            {
                0 => "None",
                1 => "Low",
                2 => "Medium",
                3 => "High",
                4 => "Ultra",
                _ => "None"
            };
        }

        private int GetAlphaIndex(string alpha)
        {
            if (alpha.EndsWith("/10"))
            {
                if (int.TryParse(alpha.Substring(0, alpha.IndexOf('/')), out int value))
                {
                    return value;
                }
            }
            return 1;
        }

        private string GetAlphaString(int index)
        {
            return $"{index}/10";
        }

        internal void CreatePreview()
        {
            if (loading) return;

            try
            {
                if (RuleToEdit == null)
                    return;

                // 获取选中的颜色，如果没有选中则使用默认颜色（绿色 #57a64a）
                Color fgColor = GetSelectedFgColor() ?? Color.FromRgb(87, 166, 74); // #57a64a
                Color bgColor = GetSelectedBgColor() ?? Colors.Transparent;

                string shape = GetShapeString(cboShape.SelectedIndex);
                string blur = GetBlurString(cboBlur.SelectedIndex);
                string alpha = GetAlphaString(cboAlpha.SelectedIndex);

                t.Text = string.IsNullOrEmpty(txtCriteria.Text) ? "Sample Text" : txtCriteria.Text;

                bool isLine = shape == "Line" || shape == "LineUnder";
                
                // 检查 Active 状态
                bool isFgActive = chkFgActive.IsChecked == true;
                bool isBgActive = chkBgActive.IsChecked == true;

                r.Effect = null;

                // 背景样式
                if (isBgActive)
                {
                    r.Fill = new SolidColorBrush(Color.FromArgb(60, bgColor.R, bgColor.G, bgColor.B));
                    r.Stroke = new SolidColorBrush(Color.FromArgb(200, bgColor.R, bgColor.G, bgColor.B));

                    if (blur != "None")
                    {
                        r.Effect = new BlurEffect
                        {
                            KernelType = KernelType.Gaussian,
                            RenderingBias = RenderingBias.Performance
                        };

                        switch (blur)
                        {
                            case "Low":
                                ((BlurEffect)r.Effect).Radius = isLine ? 1 : 4.0;
                                break;

                            case "Medium":
                                ((BlurEffect)r.Effect).Radius = isLine ? 2 : 7.0;
                                break;

                            case "High":
                                ((BlurEffect)r.Effect).Radius = isLine ? 4 : 11.0;
                                break;

                            case "Ultra":
                                ((BlurEffect)r.Effect).Radius = isLine ? 6 : 20.0;
                                break;
                        }

                        r.Stroke = null;
                    }

                    int alphaValue = GetAlphaValue(alpha);
                    r.Fill = new SolidColorBrush(Color.FromArgb((byte)(255 * alphaValue / 10), bgColor.R, bgColor.G, bgColor.B));
                }
                else
                {
                    // Background 未激活时，使用透明背景
                    r.Fill = Brushes.Transparent;
                    r.Stroke = Brushes.Transparent;
                }

                double Vert = blur != "None" ? 4 : 2;
                const double Horz = 2;

                r.Width = isLine ? previewGrid.ActualWidth - 8 : t.ActualWidth + 2;
                t.Text = string.IsNullOrEmpty(txtCriteria.Text) ? "Sample Text" : txtCriteria.Text;
                t.Padding = new Thickness(Horz, Vert, Horz, Vert);

                // 前景样式
                if (isFgActive)
                {
                    t.Foreground = new SolidColorBrush(fgColor);
                    
                    // Active 启用时，应用所有样式
                    if (chkFgBold.IsChecked == true)
                        t.FontWeight = FontWeights.Bold;
                    else
                        t.FontWeight = FontWeights.Normal;

                    if (chkFgItalic.IsChecked == true)
                        t.FontStyle = FontStyles.Italic;
                    else
                        t.FontStyle = FontStyles.Normal;

                    // Underline 和 Strikethrough 可以共存
                    t.TextDecorations = new TextDecorationCollection();
                    if (chkFgUnderline.IsChecked == true)
                    {
                        t.TextDecorations.Add(TextDecorations.Underline);
                    }
                    if (chkFgStrikethrough.IsChecked == true)
                    {
                        t.TextDecorations.Add(TextDecorations.Strikethrough);
                    }
                }
                else
                {
                    // Active 禁用时，使用默认注释颜色（灰色），并禁用所有样式效果
                    t.Foreground = Brushes.Gray;
                    t.FontWeight = FontWeights.Normal;
                    t.FontStyle = FontStyles.Normal;
                    t.TextDecorations = null;
                }

                if (shape == "TagUnder" || shape == "LineUnder")
                {
                    r.Height = 4;
                    r.Margin = new Thickness(0, t.ActualHeight - 3, 0, 0);
                }
                else
                {
                    r.Height = t.ActualHeight + 2.0;
                    r.Margin = new Thickness(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace);
            }
        }

        private int GetAlphaValue(string alpha)
        {
            if (alpha.EndsWith("/10"))
            {
                if (int.TryParse(alpha.Substring(0, alpha.IndexOf('/')), out int value))
                {
                    return value;
                }
            }
            return 1;
        }
    }
}
