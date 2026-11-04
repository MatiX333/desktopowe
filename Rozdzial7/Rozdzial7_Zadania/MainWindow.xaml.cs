using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rozdzial7_Zadania;

public partial class MainWindow : Window
{
    private ObservableCollection<Produkt> ListaProduktow = null;

    public MainWindow()
    {
        InitializeComponent();
        PrzygotujWiazanie();
    }

    private void PrzygotujWiazanie()
    {
        string sciezka = "pack://application:,,,/rysunki/";

        ListaProduktow = new ObservableCollection<Produkt>();
        ListaProduktow.Add(new Produkt("O1-11", "ołówek", 8, "Katowice 1",
            new Uri(sciezka + "1.png"), "Ołówek z gumką HB"));
        ListaProduktow.Add(new Produkt("PW-20", "pióro wieczne", 75, "Katowice 2",
            new Uri(sciezka + "2.png"), "Pióro wieczne ze stalówką M"));
        ListaProduktow.Add(new Produkt("DZ-10", "długopis żelowy", 1121, "Katowice 1",
            new Uri(sciezka + "3.png"), "Długopis żelowy, wkład niebieski"));
        ListaProduktow.Add(new Produkt("DZ-12", "długopis kulkowy", 280, "Katowice 2",
            new Uri(sciezka + "4.png"), "Długopis kulkowy, wkład czarny"));
        gridProdukty.ItemsSource = ListaProduktow;

        ObservableCollection<string> ListaMagazynow =
            new ObservableCollection<string>() { "Katowice 1", "Katowice 2", "Gliwice 1" };
        nazwaMagazynu.ItemsSource = ListaMagazynow;

        ICollectionView widok = CollectionViewSource.GetDefaultView(gridProdukty.ItemsSource);
        widok.GroupDescriptions.Add(new PropertyGroupDescription("Magazyn"));
        widok.SortDescriptions.Add(new SortDescription("Magazyn", ListSortDirection.Ascending));
        widok.SortDescriptions.Add(new SortDescription("Nazwa", ListSortDirection.Ascending));

        ICollectionViewLiveShaping zywyWidok = widok as ICollectionViewLiveShaping;
        if (zywyWidok != null && zywyWidok.CanChangeLiveGrouping)
        {
            zywyWidok.LiveGroupingProperties.Add("Magazyn");
            zywyWidok.IsLiveGrouping = true;
        }
    }

    private void btnZdjecie_Click(object sender, RoutedEventArgs e)
    {
        Produkt produkt = ((FrameworkElement)sender).DataContext as Produkt;
        if (produkt == null) return;

        Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Title = "Wybierz zdjęcie";
        dialog.Filter = "Pliki graficzne (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Wszystkie pliki (*.*)|*.*";
        dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

        if (dialog.ShowDialog() == true)
            produkt.Zdjecie = new Uri(dialog.FileName);
    }
}
