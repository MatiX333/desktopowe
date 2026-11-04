using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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

        private void BtnDodaj_Click(object sender, RoutedEventArgs e)
        {
            Produkt nowy = new Produkt("", "", 0, "");
            OknoEdycja okno = new OknoEdycja(nowy);
            okno.Owner = this;

            if (okno.ShowDialog() == true)
            {
                ListaProduktow.Add(nowy);
            }
        }

        private void BtnEdytuj_Click(object sender, RoutedEventArgs e)
        {
            EdytujWybrany();
        }

        private void LstProdukty_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EdytujWybrany();
        }

        private void EdytujWybrany()
        {
            Produkt wybrany = lstProdukty.SelectedItem as Produkt;
            if (wybrany != null)
            {
                OknoEdycja okno = new OknoEdycja(wybrany);
                okno.Owner = this;
                okno.ShowDialog();
            }
            else
            {
                MessageBox.Show("Wybierz produkt z listy!", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnUsun_Click(object sender, RoutedEventArgs e)
        {
            Produkt wybrany = lstProdukty.SelectedItem as Produkt;
            if (wybrany != null)
            {
                var result = MessageBox.Show($"Czy na pewno chcesz usunąć produkt '{wybrany.Nazwa}'?",
                    "Potwierdzenie usunięcia", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ListaProduktow.Remove(wybrany);
                }
            }
            else
            {
                MessageBox.Show("Wybierz produkt do usunięcia!", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}