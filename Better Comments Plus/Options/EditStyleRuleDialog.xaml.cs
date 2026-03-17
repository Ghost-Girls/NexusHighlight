using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using MessageBox = System.Windows.MessageBox;

namespace BetterCommentsPlus.Options
{
    public partial class EditStyleRuleDialog
    {
        public EditStyleRuleDialog()
        {
            InitializeComponent();

            Loaded += EditStyleRuleDialog_Loaded;

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
        public StyleRule RuleToEdit { get; set; }
        
        public Button ModifyButton => btnModify;
        public Button DeleteButton => btnDelete;

        private void EditStyleRuleDialog_Loaded(object sender, RoutedEventArgs e)
        {
            BetterCommentsPlus.Options.Helper.InitDefaults();
            InitializeColorLists();

            txtCriteria.Text = RuleToEdit.Criteria;
            chkFgActive.IsChecked = RuleToEdit.Foreground.IsActive;
            chkFgBold.IsChecked = RuleToEdit.Foreground.IsBold;
            chkFgItalic.IsChecked = RuleToEdit.Foreground.IsItalic;
            chkFgUnderline.IsChecked = RuleToEdit.Foreground.HasUnderline;
            chkFgStrikethrough.IsChecked = RuleToEdit.Foreground.HasStrikethrough;

            chkBgActive.IsChecked = RuleToEdit.Background.IsActive;
            chkCaseSensitive.IsChecked = RuleToEdit.Background.IsCaseSensitive;
            chkPartialMatch.IsChecked = RuleToEdit.Background.AllowPartialMatch;

            loading = true;
            cboShape.SelectedIndex = GetShapeIndex(RuleToEdit.Background.Shape);
            cboBlur.SelectedIndex = GetBlurIndex(RuleToEdit.Background.Blur);
            cboAlpha.SelectedIndex = GetAlphaIndex(RuleToEdit.Background.Alpha);

            txtCriteria.TextChanged += (_, _) => CreatePreview();
            cboShape.SelectionChanged += (_, _) => CreatePreview();
            cboBlur.SelectionChanged += (_, _) => CreatePreview();
            cboAlpha.SelectionChanged += (_, _) => CreatePreview();
            lstFgColors.SelectionChanged += (_, _) => CreatePreview();
            lstBgColors.SelectionChanged += (_, _) => CreatePreview();
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

            RuleToEdit.Foreground.IsActive = chkFgActive.IsChecked == true;
            RuleToEdit.Foreground.IsBold = chkFgBold.IsChecked == true;
            RuleToEdit.Foreground.IsItalic = chkFgItalic.IsChecked == true;
            RuleToEdit.Foreground.HasUnderline = chkFgUnderline.IsChecked == true;
            RuleToEdit.Foreground.HasStrikethrough = chkFgStrikethrough.IsChecked == true;

            if (lstFgColors.SelectedItem != null)
            {
                Color fgColor = (Color)(lstFgColors.SelectedItem as ListBoxItem).Tag;
                RuleToEdit.Foreground.SetColor(fgColor);
            }

            RuleToEdit.Background.IsActive = chkBgActive.IsChecked == true;
            RuleToEdit.Background.IsCaseSensitive = chkCaseSensitive.IsChecked == true;
            RuleToEdit.Background.AllowPartialMatch = chkPartialMatch.IsChecked == true;

            if (lstBgColors.SelectedItem != null)
            {
                Color bgColor = (Color)(lstBgColors.SelectedItem as ListBoxItem).Tag;
                RuleToEdit.Background.SetColor(bgColor);
            }

            RuleToEdit.Background.Shape = GetShapeString(cboShape.SelectedIndex);
            RuleToEdit.Background.Blur = GetBlurString(cboBlur.SelectedIndex);
            RuleToEdit.Background.Alpha = GetAlphaString(cboAlpha.SelectedIndex);

            DialogResult = true;
            Close();
        }

        private void InitializeColorLists()
        {
            string fgHexMatch = RuleToEdit.Foreground.GetColor()?.ToString() ?? "#000000";
            string bgHexMatch = RuleToEdit.Background.GetColor()?.ToString() ?? "#FF0000";

            foreach (Color color in Helper.colors)
            {
                ListBoxItem fgItem = new()
                {
                    Foreground = new SolidColorBrush(color),
                    Background = new SolidColorBrush(color),
                    Tag = color,
                    IsSelected = fgHexMatch == color.ToString()
                };
                lstFgColors.Items.Add(fgItem);

                ListBoxItem bgItem = new()
                {
                    Foreground = new SolidColorBrush(color),
                    Background = new SolidColorBrush(color),
                    Tag = color,
                    IsSelected = bgHexMatch == color.ToString()
                };
                lstBgColors.Items.Add(bgItem);
            }
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

                Color fgColor = lstFgColors.SelectedItem != null ? (Color)(lstFgColors.SelectedItem as ListBoxItem).Tag : RuleToEdit.Foreground.GetColor() ?? Colors.Black;
                Color bgColor = lstBgColors.SelectedItem != null ? (Color)(lstBgColors.SelectedItem as ListBoxItem).Tag : RuleToEdit.Background.GetColor() ?? Colors.Transparent;

                string shape = GetShapeString(cboShape.SelectedIndex);
                string blur = GetBlurString(cboBlur.SelectedIndex);
                string alpha = GetAlphaString(cboAlpha.SelectedIndex);

                t.Text = string.IsNullOrEmpty(txtCriteria.Text) ? "Sample Text" : txtCriteria.Text;

                bool isLine = shape == "Line" || shape == "LineUnder";

                r.Effect = null;

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

                double Vert = blur != "None" ? 4 : 2;
                const double Horz = 2;

                r.Width = isLine ? previewGrid.ActualWidth - 8 : t.ActualWidth + 2;
                t.Text = string.IsNullOrEmpty(txtCriteria.Text) ? "Sample Text" : txtCriteria.Text;
                t.Padding = new Thickness(Horz, Vert, Horz, Vert);

                t.Foreground = new SolidColorBrush(fgColor);

                if (chkFgBold.IsChecked == true)
                    t.FontWeight = FontWeights.Bold;
                else
                    t.FontWeight = FontWeights.Normal;

                if (chkFgItalic.IsChecked == true)
                    t.FontStyle = FontStyles.Italic;
                else
                    t.FontStyle = FontStyles.Normal;

                t.TextDecorations = null;
                if (chkFgUnderline.IsChecked == true)
                    t.TextDecorations = TextDecorations.Underline;
                if (chkFgStrikethrough.IsChecked == true)
                    t.TextDecorations = TextDecorations.Strikethrough;

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
