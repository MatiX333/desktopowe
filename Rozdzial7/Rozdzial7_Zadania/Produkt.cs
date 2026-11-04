using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Rozdzial7_Zadania;

public class Produkt : INotifyPropertyChanged
{
    private string symbol;
    private string nazwa;
    private int liczbaSztuk;
    private string magazyn;
    private Uri zdjecie;
    private string opis;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Symbol
    {
        get { return symbol; }
        set { symbol = value; Powiadom(); }
    }

    public string Nazwa
    {
        get { return nazwa; }
        set { nazwa = value; Powiadom(); }
    }

    public int LiczbaSztuk
    {
        get { return liczbaSztuk; }
        set { liczbaSztuk = value; Powiadom(); }
    }

    public string Magazyn
    {
        get { return magazyn; }
        set { magazyn = value; Powiadom(); }
    }

    public Uri Zdjecie
    {
        get { return zdjecie; }
        set { zdjecie = value; Powiadom(); }
    }

    public string Opis
    {
        get { return opis; }
        set { opis = value; Powiadom(); }
    }

    public Produkt(string sym, string naz, int lszt, string mag, Uri zdj, string op)
    {
        symbol = sym;
        nazwa = naz;
        liczbaSztuk = lszt;
        magazyn = mag;
        zdjecie = zdj;
        opis = op;
    }

    public override string ToString()
    {
        return String.Format("{0} {1} {2} {3}", Symbol, Nazwa, LiczbaSztuk, Magazyn);
    }

    private void Powiadom([CallerMemberName] string nazwaWlasciwosci = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nazwaWlasciwosci));
    }
}
