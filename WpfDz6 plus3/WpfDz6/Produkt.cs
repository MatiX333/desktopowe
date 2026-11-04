using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfDz6
{
    public class Produkt : INotifyPropertyChanged
    {
        private string symbol;
        private string nazwa;
        private int liczbaSztuk;
        private string magazyn;

        public string Symbol
        {
            get => symbol;
            set { symbol = value; OnPropertyChanged(); }
        }

        public string Nazwa
        {
            get => nazwa;
            set { nazwa = value; OnPropertyChanged(); }
        }

        public int LiczbaSztuk
        {
            get => liczbaSztuk;
            set { liczbaSztuk = value; OnPropertyChanged(); }
        }

        public string Magazyn
        {
            get => magazyn;
            set { magazyn = value; OnPropertyChanged(); }
        }

        public Produkt(string symbol, string nazwa, int liczbaSztuk, string magazyn)
        {
            Symbol = symbol;
            Nazwa = nazwa;
            LiczbaSztuk = liczbaSztuk;
            Magazyn = magazyn;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}