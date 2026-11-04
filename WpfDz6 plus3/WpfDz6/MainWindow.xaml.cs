using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WpfDz6
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Produkt> ListaProduktow { get; set; }
        private ICollectionView view;

        public MainWindow()
        {
            InitializeComponent();
            PrzygotujDane();
        }

        private void PrzygotujDane()
        {
            ListaProduktow = new ObservableCollection<Produkt>
            {
                new Produkt("DZ-10", "długopis żelowy", 1121, "Katowice 1"),
                new Produkt("O1-11", "ołówki", 8, "Katowice 1"),
                new Produkt("DZ-12", "długopis kulkowy", 280, "Katowice 2"),
                new Produkt("PW-20", "pióro wieczne", 80, "Katowice 2")
            };

            lstProdukty.ItemsSource = ListaProduktow;
            view = CollectionViewSource.GetDefaultView(lstProdukty.ItemsSource);
        }

        private void TxtFiltr_TextChanged(object sender, TextChangedEventArgs e)
        {
            string fraza = txtFiltr.Text.ToLower();

            if (string.IsNullOrWhiteSpace(fraza))
            {
                view.Filter = null;
            }
            else
            {
                view.Filter = item =>
                {
                    Produkt p = item as Produkt;
                    return p != null && p.Nazwa.ToLower().Contains(fraza);
                };
            }
        }
        private void LstProdukty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Produkt wybrany = lstProdukty.SelectedItem as Produkt;
            if (wybrany != null)
            {
                txtSymbol.Text = wybrany.Symbol;
                txtNazwa.Text = wybrany.Nazwa;
                txtLiczbaSztuk.Text = wybrany.LiczbaSztuk.ToString();
                txtMagazyn.Text = wybrany.Magazyn;
            }
            else
            {
                WyczyscFormularz();
            }
        }

        private void BtnZapisz_Click(object sender, RoutedEventArgs e)
        {
            Produkt wybrany = lstProdukty.SelectedItem as Produkt;
            if (wybrany != null)
            {
                if (int.TryParse(txtLiczbaSztuk.Text, out int sztuki))
                {
                    wybrany.Symbol = txtSymbol.Text;
                    wybrany.Nazwa = txtNazwa.Text;
                    wybrany.LiczbaSztuk = sztuki;
                    wybrany.Magazyn = txtMagazyn.Text;
                }
                else
                {
                    MessageBox.Show("Liczba sztuk musi być liczbą całkowitą!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Wybierz produkt z listy, aby go edytować.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDodaj_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNazwa.Text) || string.IsNullOrWhiteSpace(txtSymbol.Text))
            {
                MessageBox.Show("Wpisz przynajmniej Symbol i Nazwę!", "Informacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtLiczbaSztuk.Text, out int sztuki))
            {
                sztuki = 0;
            }

            Produkt nowy = new Produkt(txtSymbol.Text, txtNazwa.Text, sztuki, txtMagazyn.Text);
            ListaProduktow.Add(nowy);
            lstProdukty.SelectedItem = nowy;
        }

        private void BtnUsun_Click(object sender, RoutedEventArgs e)
        {
            Produkt wybrany = lstProdukty.SelectedItem as Produkt;
            if (wybrany != null)
            {
                var wynik = MessageBox.Show($"Czy na pewno chcesz usunąć produkt '{wybrany.Nazwa}'?",
                    "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (wynik == MessageBoxResult.Yes)
                {
                    ListaProduktow.Remove(wybrany);
                    WyczyscFormularz();
                }
            }
            else
            {
                MessageBox.Show("Wybierz produkt do usunięcia!", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void WyczyscFormularz()
        {
            txtSymbol.Text = string.Empty;
            txtNazwa.Text = string.Empty;
            txtLiczbaSztuk.Text = string.Empty;
            txtMagazyn.Text = string.Empty;
        }
    }
}