using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kalkulatorczasu
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(pod_min.Text, out int podaneMinuty) && int.TryParse(pod_godz.Text, out int podaneGodziny))
            {
                int wyliczoneGodz = podaneMinuty / 60;
                int wyliczoneMinuty = podaneGodziny * 60;

                wynik_1.Text = $"Podales tyle minut: {podaneMinuty} min.\ni jest to tyle godzin: {wyliczoneGodz} godz.\n" +
                    $"Podales tyle godzin: {podaneGodziny} godz.\ni jest to tyle minut: {wyliczoneMinuty} min.";
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(tb_wartosc.Text, out int podanaWartosc))
            {
                if (cb_wybor.Text == "Minuty")
                {
                    int wyliczoneGodz = podanaWartosc / 60;

                    wynik_2.Text = $"Podales tyle minut: {podanaWartosc} min.\n" +
                        $"i jest to tyle godzin: {wyliczoneGodz} godz. {podanaWartosc % 60} min.";
                } else
                {
                    int wyliczoneMinuty = podanaWartosc * 60;

                    wynik_2.Text = $"Podales tyle godzin: {podanaWartosc} godz.\n" +
                        $"i jest to tyle minut: {wyliczoneMinuty} min.";
                }
            } else
            {
                MessageBox.Show($"Wystapil blad podczas parsowania inputu: {tb_wartosc.Text}");
            }
        }

        private void pod_min_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
