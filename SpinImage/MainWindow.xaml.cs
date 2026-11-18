using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace SpinImage
{
    public partial class MainWindow : Window
    {
        private const string BackText = "ANDERS";
        private const double LetterDelayStart = 0.30;   
        private const double LetterStagger = 0.06;      
        private const double LetterDuration = 0.26;     

        private static readonly Color TextColor = Color.FromRgb(232, 237, 247); 

        private RenderTargetBitmap _backTarget;
        private readonly List<FormattedText> _letters = new List<FormattedText>();
        private readonly List<double> _letterX = new List<double>();
        private double _letterY;
        private int _texW, _texH;
        private readonly Stopwatch _clock = new Stopwatch();
        private bool _renderHooked;
        private bool _hovering;

        public MainWindow()
        {
            InitializeComponent();

            var front = new BitmapImage(new Uri("pack://application:,,,/obraz.png"));
            FrontBrush.ImageSource = front;

            double aspect = (double)front.PixelWidth / front.PixelHeight;
            ImageScale.ScaleX = aspect > 1 ? 1 : aspect;
            ImageScale.ScaleY = aspect > 1 ? 1 / aspect : 1;

            PrepareBack(front.PixelWidth * 2, front.PixelHeight * 2);
            BackBrush.ImageSource = _backTarget;
            DrawBack(0); 
        }

        private void Viewport_MouseEnter(object sender, MouseEventArgs e)
        {
            _hovering = true;
            RotateTo(180);
            StartBuild();
        }

        private void Viewport_MouseLeave(object sender, MouseEventArgs e)
        {
            _hovering = false;
            RotateTo(0, () => { if (!_hovering) { StopBuild(); DrawBack(0); } });
        }

        private void RotateTo(double angle, Action completed = null)
        {
            var animation = new DoubleAnimation(angle, TimeSpan.FromMilliseconds(700))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            if (completed != null) animation.Completed += (s, e) => completed();
            Spin.BeginAnimation(AxisAngleRotation3D.AngleProperty, animation);
        }


        private void PrepareBack(int w, int h)
        {
            _texW = w;
            _texH = h;
            _backTarget = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);

            var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal,
                                        FontWeights.Black, FontStretches.Normal);
            double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            Brush brush = new SolidColorBrush(TextColor);
            brush.Freeze();

            double trackingUnit = 0.04;
            double total100 = 0;
            foreach (char c in BackText)
                total100 += Make(c, 100).Width + 100 * trackingUnit;
            total100 -= 100 * trackingUnit;

            double size = 100 * (w * 0.85) / total100;
            double x = (w - (total100 * size / 100)) / 2;

            double maxHeight = 0;
            foreach (char c in BackText)
            {
                var ft = Make(c, size);
                _letters.Add(ft);
                _letterX.Add(x);
                x += ft.Width + size * trackingUnit;
                maxHeight = Math.Max(maxHeight, ft.Height);
            }
            _letterY = (h - maxHeight) / 2;

            FormattedText Make(char c, double s) =>
                new FormattedText(c.ToString(), CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, typeface, s, brush, dpi);
        }

        private void StartBuild()
        {
            _clock.Restart();
            DrawBack(0);
            if (!_renderHooked)
            {
                CompositionTarget.Rendering += OnRendering;
                _renderHooked = true;
            }
        }

        private void StopBuild()
        {
            if (_renderHooked)
            {
                CompositionTarget.Rendering -= OnRendering;
                _renderHooked = false;
            }
        }

        private void OnRendering(object sender, EventArgs e)
        {
            double t = _clock.Elapsed.TotalSeconds;
            DrawBack(t);

            double end = LetterDelayStart + (_letters.Count - 1) * LetterStagger + LetterDuration;
            if (t >= end) StopBuild(); 
        }

        private void DrawBack(double t)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                for (int i = 0; i < _letters.Count; i++)
                {
                    double p = (t - LetterDelayStart - i * LetterStagger) / LetterDuration;
                    if (p <= 0) continue;
                    if (p > 1) p = 1;

                    double e = EaseOutBack(p);
                    double scale = 0.35 + 0.65 * e;
                    double dropY = (1 - e) * _texH * 0.10;     

                    var ft = _letters[i];
                    double cx = _letterX[i] + ft.Width / 2;
                    double cy = _letterY + ft.Height / 2 + dropY;

                    dc.PushOpacity(Math.Min(1, p * 2.5));
                    dc.PushTransform(new ScaleTransform(scale, scale, cx, cy));
                    dc.DrawText(ft, new Point(_letterX[i], _letterY + dropY));
                    dc.Pop();
                    dc.Pop();
                }
            }
            _backTarget.Clear();
            _backTarget.Render(visual);
        }

        private static double EaseOutBack(double x)
        {
            const double c1 = 1.70158;
            const double c3 = c1 + 1;
            return 1 + c3 * Math.Pow(x - 1, 3) + c1 * Math.Pow(x - 1, 2);
        }
    }
}
