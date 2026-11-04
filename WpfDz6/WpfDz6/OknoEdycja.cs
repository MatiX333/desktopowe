using System.Windows;

namespace WpfDz6
{
    public partial class OknoEdycja : Window
    {
        public OknoEdycja(Produkt produkt)
        {
            InitializeComponent();
            DataContext = produkt;
        }

        private void BtnPotwierdz_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}