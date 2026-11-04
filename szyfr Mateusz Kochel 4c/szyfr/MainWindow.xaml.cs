using System.Windows;

namespace szyfr
{
    public partial class MainWindow
    {
        char[] litery = {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i',
            'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r',
            's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Zaszyfruj_Click(object sender, RoutedEventArgs e)
        {
            int klucz = int.Parse(txtKlucz.Text);
            string tekst = txtTekst.Text;

            string szyfr = "";

            foreach (char znak in tekst)
            {
                for (int i = 0; i < litery.Length; i++)
                {
                    if (znak == litery[i])
                    {
                        int nowaPozycja = (i + klucz) % litery.Length;

                        szyfr += litery[nowaPozycja];

                        break;
                    }
                }
            }

            txtWynik.Text = szyfr;
        }
    }
}