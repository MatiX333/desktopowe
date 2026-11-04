using System.Windows;
using System.Windows.Controls;

namespace Projektv2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string textFromInput;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Add_todo(object sender, RoutedEventArgs e)
        {
            textFromInput = input.Text;

            if (string.IsNullOrWhiteSpace(textFromInput))
                return;

            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5)
            };

            var todo = new TextBlock
            {
                Text = textFromInput,
                FontSize = 18,
                Width = 200
            };

            var deleteButton = new Button
            {
                Content = "Usuń",
                Width = 70,
                Margin = new Thickness(10, 0, 0, 0)
            };

            deleteButton.Click += (s, args) =>
            {
                TodoPanel.Children.Remove(row);
            };

            row.Children.Add(todo);
            row.Children.Add(deleteButton);

            TodoPanel.Children.Add(row);

            input.Clear();
        }
    }
}