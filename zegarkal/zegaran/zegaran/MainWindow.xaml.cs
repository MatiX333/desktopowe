
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ZegarWPF
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
            timer.Start();

            RysujZegar();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            RysujZegar();
        }

        private void ZegarCanvas_SizeChanged(
            object sender, SizeChangedEventArgs e)
        {
            RysujZegar();
        }

        private void RysujZegar()
        {
            double szerokosc = ZegarCanvas.ActualWidth;
            double wysokosc = ZegarCanvas.ActualHeight;

            if (szerokosc <= 0 || wysokosc <= 0)
                return;

            ZegarCanvas.Children.Clear();

            double srodekX = szerokosc / 2;
            double srodekY = wysokosc / 2;

            double promien = Math.Min(szerokosc, wysokosc) / 2 - 25;

            // Tarcza zegara
            Ellipse tarcza = new Ellipse
            {
                Width = promien * 2,
                Height = promien * 2,
                Fill = new SolidColorBrush(Color.FromRgb(35, 39, 48)),
                Stroke = Brushes.White,
                StrokeThickness = 3
            };

            Canvas.SetLeft(tarcza, srodekX - promien);
            Canvas.SetTop(tarcza, srodekY - promien);
            ZegarCanvas.Children.Add(tarcza);

            // Cyfry i kreski na tarczy
            for (int i = 0; i < 60; i++)
            {
                double kat = i * 6 * Math.PI / 180;

                double dlugosc = i % 5 == 0 ? 12 : 5;

                double x1 = srodekX +
                    (promien - 10) * Math.Sin(kat);

                double y1 = srodekY -
                    (promien - 10) * Math.Cos(kat);

                double x2 = srodekX +
                    (promien - 10 - dlugosc) * Math.Sin(kat);

                double y2 = srodekY -
                    (promien - 10 - dlugosc) * Math.Cos(kat);

                Line kreska = new Line
                {
                    X1 = x1,
                    Y1 = y1,
                    X2 = x2,
                    Y2 = y2,
                    Stroke = Brushes.White,
                    StrokeThickness = i % 5 == 0 ? 3 : 1
                };

                ZegarCanvas.Children.Add(kreska);
            }

            // Cyfry od 1 do 12
            for (int i = 1; i <= 12; i++)
            {
                double kat = i * 30 * Math.PI / 180;

                double x = srodekX +
                    (promien - 35) * Math.Sin(kat);

                double y = srodekY -
                    (promien - 35) * Math.Cos(kat);

                TextBlock cyfra = new TextBlock
                {
                    Text = i.ToString(),
                    FontSize = 22,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    Width = 35,
                    Height = 30,
                    TextAlignment = TextAlignment.Center
                };

                Canvas.SetLeft(cyfra, x - 17.5);
                Canvas.SetTop(cyfra, y - 15);

                ZegarCanvas.Children.Add(cyfra);
            }

            // Aktualny czas systemowy
            DateTime czas = DateTime.Now;

            double sekundy = czas.Second +
                czas.Millisecond / 1000.0;

            double minuty = czas.Minute + sekundy / 60.0;

            double godziny = (czas.Hour % 12) + minuty / 60.0;

            // Wskazówka godzinowa
            RysujWskazowke(
                srodekX, srodekY,
                promien * 0.45,
                godziny * 30,
                7,
                Brushes.White
            );

            // Wskazówka minutowa
            RysujWskazowke(
                srodekX, srodekY,
                promien * 0.65,
                minuty * 6,
                4,
                Brushes.White
            );

            // Wskazówka sekundowa
            RysujWskazowke(
                srodekX, srodekY,
                promien * 0.78,
                sekundy * 6,
                2,
                Brushes.OrangeRed
            );

            // Środek zegara
            Ellipse srodek = new Ellipse
            {
                Width = 14,
                Height = 14,
                Fill = Brushes.OrangeRed
            };

            Canvas.SetLeft(srodek, srodekX - 7);
            Canvas.SetTop(srodek, srodekY - 7);

            ZegarCanvas.Children.Add(srodek);
        }

        private void RysujWskazowke(
            double srodekX,
            double srodekY,
            double dlugosc,
            double kat,
            double grubosc,
            Brush kolor)
        {
            double radiany = kat * Math.PI / 180;

            double koniecX = srodekX +
                dlugosc * Math.Sin(radiany);

            double koniecY = srodekY -
                dlugosc * Math.Cos(radiany);

            Line wskazowka = new Line
            {
                X1 = srodekX,
                Y1 = srodekY,
                X2 = koniecX,
                Y2 = koniecY,
                Stroke = kolor,
                StrokeThickness = grubosc,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            ZegarCanvas.Children.Add(wskazowka);
        }
    }
}
