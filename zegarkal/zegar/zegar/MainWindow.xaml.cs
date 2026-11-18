
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace zegar
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        private readonly string[] segmenty =
        {
            "ab cdef".Replace(" ", ""), 
            "bc",                       
            "abged",                    
            "abgcd",                    
            "fgbc",                     
            "afgcd",                    
            "afgecd",                   
            "abc",                      
            "abcdefg",                  
            "abfgcd"                    
        };

        private readonly string[] nazwySegmentow =
        {
            "a", "b", "c", "d", "e", "f", "g"
        };

        private void DodajCyfre(char cyfra)
        {
            Canvas canvas = new Canvas
            {
                Width = 34,
                Height = 49,
                Background = new SolidColorBrush(
                    Color.FromRgb(20, 20, 20))
            };

            string aktywne = cyfra.ToString();

            if (cyfra >= '0' && cyfra <= '9')
                aktywne = segmenty[cyfra - '0'];

            Point[][] punkty =
            {
                new Point[] { new Point(7, 2), new Point(27,2), new Point(30,5),
                              new Point(27,8), new Point(7,8), new Point(4,5) },

                new Point[] { new Point(27,4), new Point(30,7), new Point(30,21),
                              new Point(27,24), new Point(24,21), new Point(24,7) },

                new Point[] { new Point(27,25), new Point(30,28), new Point(30,42),
                              new Point(27,45), new Point(24,42), new Point(24,28) },

                new Point[] { new Point(7,41), new Point(27,41), new Point(30,44),
                              new Point(27,47), new Point(7,47), new Point(4,44) },

                new Point[] { new Point(4,28), new Point(7,25), new Point(10,28),
                              new Point(10,42), new Point(7,45), new Point(4,42) },

                new Point[] { new Point(4,7), new Point(7,4), new Point(10,7),
                              new Point(10,21), new Point(7,24), new Point(4,21) },

                new Point[] { new Point(7,21), new Point(27,21), new Point(30,24),
                              new Point(27,27), new Point(7,27), new Point(4,24) }
            };

            for (int i = 0; i < 7; i++)
            {
                Polygon segment = new Polygon
                {
                    Points = new PointCollection(punkty[i]),
                    Fill = aktywne.Contains(nazwySegmentow[i])
                        ? Brushes.Lime
                        : new SolidColorBrush(
                            Color.FromRgb(8, 12, 8))
                };

                canvas.Children.Add(segment);
            }

            Wyswietlacz.Children.Add(canvas);
        }

        private void DodajDwukropek()
        {
            Canvas canvas = new Canvas
            {
                Width = 12,
                Height = 49
            };

            for (int i = 0; i < 2; i++)
            {
                Ellipse kropka = new Ellipse
                {
                    Width = 5,
                    Height = 5,
                    Fill = Brushes.Lime
                };

                Canvas.SetLeft(kropka, 3);
                Canvas.SetTop(kropka, i == 0 ? 14 : 30);

                canvas.Children.Add(kropka);
            }

            Wyswietlacz.Children.Add(canvas);
        }

        public MainWindow()
        {
            InitializeComponent();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };

            timer.Tick += Timer_Tick;
            timer.Start();

            AktualizujZegar();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            AktualizujZegar();
        }

        private void AktualizujZegar()
        {
            DateTime teraz = DateTime.Now;

            Wyswietlacz.Children.Clear();

            string godzina = teraz.ToString("HHmmss");

            for (int i = 0; i < godzina.Length; i++)
            {
                DodajCyfre(godzina[i]);

                if (i == 1 || i == 3)
                    DodajDwukropek();
            }

            DolnyCzas.Text = teraz.ToString("HH:mm:ss");
        }
    }
}
