using Highlighter.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using MessageBox = System.Windows.MessageBox;

namespace Highlighter.Commands
{
    public partial class EditColor
    {
        public EditColor()
        {
            InitializeComponent();

            Loaded += EditColor_Loaded;

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

        private void EditColor_Loaded(object sender, RoutedEventArgs e)
        {
            string hexMatch = TagToEdit.Color.ColorToHex();

            foreach (Color color in Helper.colors)
            {
                ListBoxItem l = new()
                {
                    Foreground = new SolidColorBrush(color),
                    Background = new SolidColorBrush(color),
                    Tag = color,
                    IsSelected = hexMatch == color.ColorToHex()
                };

                lstColors.Items.Add(l);
            }

            loading = true;
            cboShape.SelectedIndex = (int)TagToEdit.Shape;
            cboBlur.SelectedIndex = (int)TagToEdit.Blur;
            cboAlpha.SelectedIndex = (int)TagToEdit.Alpha;
            //txtCriteria.Text = TagToEdit.Criteria;
            chkActive.IsChecked = TagToEdit.IsActive;

            //txtCriteria.TextChanged += (_, _) => CreatePreview();
            cboBlur.SelectionChanged += (_, _) => CreatePreview();
            cboShape.SelectionChanged += (_, _) => CreatePreview();
            cboBlur.SelectionChanged += (_, _) => CreatePreview();
            cboAlpha.SelectionChanged += (_, _) => CreatePreview();
            lstColors.SelectionChanged += (_, _) => CreatePreview();

            loading = false;

            CreatePreview(); //#INFO 创建预览效果
        }

        public HighlightTag TagToEdit { get; set; }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnModify_Click(object sender, RoutedEventArgs e)
        {
            TagToEdit.Shape = (TagShape)cboShape.SelectedIndex;
            TagToEdit.Blur = (BlurIntensity)cboBlur.SelectedIndex;
            TagToEdit.Alpha = (FillAlpha)cboAlpha.SelectedIndex;
            TagToEdit.IsActive = chkActive.IsChecked == true;
            TagToEdit.AllowPartialMatch = chkPartial.IsChecked == true;
            TagToEdit.IsCaseSensitive = chkCaseSensitive.IsChecked == true;
            TagToEdit.Color = lstColors.SelectedItem != null ? (Color)(lstColors.SelectedItem as ListBoxItem).Tag : TagToEdit.Color;
            DialogResult = true;
            Close();
        }

        internal void CreatePreview()
        {
            if (loading) return;

            try
            {
                if (TagToEdit == null)
                    return;

                HighlightTag tag = new()
                {
                    Shape = (TagShape)cboShape.SelectedIndex,
                    Blur = (BlurIntensity)cboBlur.SelectedIndex,
                    Alpha = (FillAlpha)cboAlpha.SelectedIndex,
                    Criteria = TagToEdit.Criteria,
                    IsActive = chkActive.IsChecked == true,
                    AllowPartialMatch = chkPartial.IsChecked == true,
                    Color = lstColors.SelectedItem != null ? (Color)(lstColors.SelectedItem as ListBoxItem).Tag : TagToEdit.Color
                };

                // UpdateLayout();

                bool isLine = tag.IsLine();

                r.Effect = null;

                t.Text = tag.Criteria;

                r.Fill = new SolidColorBrush(Color.FromArgb(60, tag.Color.R, tag.Color.G, tag.Color.B));
                r.Stroke = new SolidColorBrush(Color.FromArgb(200, tag.Color.R, tag.Color.G, tag.Color.B));

                if (tag.Blur != BlurIntensity.None)
                {
                    r.Effect = new BlurEffect
                    {
                        KernelType = KernelType.Gaussian,
                        RenderingBias = RenderingBias.Performance
                    };

                    switch (tag.Blur)
                    {
                        case BlurIntensity.Low:
                            //((SolidColorBrush)r.Fill).Color.ChangeAlpha(80);
                            ((BlurEffect)r.Effect).Radius = isLine ? 1 : 4.0;
                            break;

                        case BlurIntensity.Medium:
                            //((SolidColorBrush)r.Fill).Color.ChangeAlpha(120);
                            ((BlurEffect)r.Effect).Radius = isLine ? 2 : 7.0;
                            break;

                        case BlurIntensity.High:
                            //((SolidColorBrush)r.Fill).Color.ChangeAlpha(170);
                            ((BlurEffect)r.Effect).Radius = isLine ? 4 : 11.0;
                            break;

                        case BlurIntensity.Ultra:
                            //((SolidColorBrush)r.Fill).Color.ChangeAlpha(255);
                            ((BlurEffect)r.Effect).Radius = isLine ? 6 : 20.0;
                            break;
                    }

                    r.Stroke = null;
                }

                //修改Fill的透明度
                r.Fill = new SolidColorBrush(((SolidColorBrush)r.Fill).Color.ChangeAlpha((byte)((255 * (byte)tag.Alpha) / 10)));

                double Vert = tag.Blur != BlurIntensity.None ? 4 : 2;
                const double Horz = 2; //tag.isFullLine ? 1920 : 4;

                r.Width = isLine ? previewGrid.ActualWidth - 8 : t.ActualWidth + 2;
                t.Text = tag.Criteria;
                t.Padding = new Thickness(Horz, Vert, Horz, Vert);

                if (tag.IsTagUnder())
                {
                    r.Height = 4;
                    r.Margin = new Thickness(0, t.ActualHeight - 3, 0, 0);
                }
                else if (tag.IsLineUnder())
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
    }
}