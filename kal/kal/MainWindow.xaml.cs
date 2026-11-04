using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace kal
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            PreviewText.Text = InputTextBox.Text;
            PreviewText.FontSize = slider.Value;
            characterCount.Text = InputTextBox.Text.Length.ToString();

            PreviewText.Foreground = Brushes.Red;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PreviewText == null || TextColorComboBox == null)
                return;

            if (TextColorComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                switch (selectedItem.Content?.ToString())
                {
                    case "Czerwony":
                        PreviewText.Foreground = Brushes.Red;
                        break;

                    case "Zielony":
                        PreviewText.Foreground = Brushes.Green;
                        break;

                    case "Niebieski":
                        PreviewText.Foreground = Brushes.Blue;
                        break;
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (InputTextBox == null || characterCount == null || PreviewText == null)
                return;

            string text = InputTextBox.Text;

            characterCount.Text = text.Length.ToString();
            PreviewText.Text = text;
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PreviewText == null)
                return;

            PreviewText.FontSize = e.NewValue;
        }
    }
}
