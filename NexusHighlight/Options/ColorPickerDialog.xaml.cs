using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NexusHighlight.Options
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

            foreach (var colorHex in colors)
            {
                var border = new Border
                {
                    Width = 25,
                    Height = 25,
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
