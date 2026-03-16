using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BetterCommentsPlus.Options
{
    public partial class ColorPickerDialog : Window
    {
        public Color? SelectedColor { get; private set; }

        public ColorPickerDialog(Color? initialColor = null)
        {
            InitializeComponent();
            
            if (initialColor.HasValue)
            {
                SelectedColor = initialColor.Value;
                HexTextBox.Text = ColorToHex(initialColor.Value);
            }
            
            Loaded += ColorPickerDialog_Loaded;
        }

        private void ColorPickerDialog_Loaded(object sender, RoutedEventArgs e)
        {
            LoadColors();
        }

        private void LoadColors()
        {
            var colors = new[]
            {
                "#FFFF0000", "#FF00FF00", "#FF0000FF", "#FFFFFF00",
                "#FFFF00FF", "#FF00FFFF", "#FF800000", "#FF008000",
                "#FF000080", "#FF808000", "#FF800080", "#FF008080",
                "#FFC0C0C0", "#FF808080", "#FF000000", "#FFFFFFFF",
                "#FFEB690A", "#FFC00000", "#FF00C000", "#FF0000C0",
                "#FFFFC000", "#FFC000C0", "#FF00C0C0"
            };

            foreach (var colorHex in colors)
            {
                var border = new Border
                {
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(2),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(3),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = colorHex
                };

                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorHex);
                    border.Background = new SolidColorBrush(color);
                }
                catch { }

                border.MouseLeftButtonDown += (s, e) =>
                {
                    if (border.Tag is string hex)
                    {
                        try
                        {
                            SelectedColor = (Color)ColorConverter.ConvertFromString(hex);
                            HexTextBox.Text = hex;
                            DialogResult = true;
                            Close();
                        }
                        catch { }
                    }
                };

                ColorsWrapPanel.Children.Add(border);
            }
        }

        private void HexTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var hex = HexTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(hex))
            {
                PreviewBorder.Background = Brushes.Transparent;
                ConfirmHexButton.IsEnabled = false;
                return;
            }

            try
            {
                var color = ParseHexColor(hex);
                PreviewBorder.Background = new SolidColorBrush(color);
                ConfirmHexButton.IsEnabled = true;
            }
            catch
            {
                PreviewBorder.Background = Brushes.Transparent;
                ConfirmHexButton.IsEnabled = false;
            }
        }

        private void ConfirmHexButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var hex = HexTextBox.Text.Trim();
                SelectedColor = ParseHexColor(hex);
                DialogResult = true;
                Close();
            }
            catch
            {
                MessageBox.Show("请输入有效的HEX颜色值！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Color ParseHexColor(string hex)
        {
            if (!hex.StartsWith("#"))
            {
                hex = "#" + hex;
            }

            if (hex.Length == 7)
            {
                hex = "#FF" + hex.Substring(1);
            }

            return (Color)ColorConverter.ConvertFromString(hex);
        }

        private string ColorToHex(Color color)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
