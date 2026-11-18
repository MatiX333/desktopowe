using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace SpinImage
{
    public partial class MainWindow : Window
    {
        // ---------- tył karty: animacja napisu ----------
        private const string BackText = "ANDERS";
        private const double LetterDelayStart = 0.50;   // litery wskakują, gdy karta jest już odwrócona
        private const double LetterStagger = 0.06;
        private const double LetterDuration = 0.30;
        private const double BarsStart = 1.10;
        private const double SheenStart = 1.25;         // błysk światła przesuwający się po karcie
        private const double SheenDuration = 1.10;
        private const double BuildEnd = SheenStart + SheenDuration + 0.05;

        private const int TexW = 1920;
        private const double CardScale = 1.45;

        private static readonly Color Crimson = Color.FromRgb(220, 20, 60);

        private sealed class Item { public FormattedText Ft; public double X, Y, Delay; }
        private sealed class Particle { public Ellipse Shape; public double X, Y, Speed, Sway, Phase, Twinkle, Alpha; }

        private RenderTargetBitmap _backTarget;
        private BitmapSource _backBg;
        private readonly List<FormattedText> _letters = new List<FormattedText>();
        private readonly List<FormattedText> _letterShadows = new List<FormattedText>();
        private readonly List<double> _letterX = new List<double>();
        private readonly List<Item> _extras = new List<Item>();
        private double _letterY;
        private double _shadowOffset;
        private int _texW, _texH;

        private Brush _barWhite, _barRed, _sheenBrush;

        // ---------- stan ----------
        private readonly Stopwatch _life = new Stopwatch();
        private readonly Stopwatch _buildClock = new Stopwatch();
        private readonly Random _rng = new Random(7);
        private readonly List<Particle> _particles = new List<Particle>();
        private bool _hooked, _building, _hovering;
        private double _lastT;
        private double _tiltX, _tiltY, _targetTiltX, _targetTiltY;
        private double _hintAlpha;

        public MainWindow()
        {
            InitializeComponent();

            var front = new BitmapImage(new Uri("pack://application:,,,/obraz.png"));
            FrontBrush.ImageSource = BuildFront(front);

            double aspect = (double)front.PixelWidth / front.PixelHeight;
            ImageScale.ScaleX = CardScale;
            ImageScale.ScaleY = CardScale / aspect;
            ImageScale.ScaleZ = CardScale;

            PrepareBack(TexW, (int)Math.Round(TexW / aspect));
            BackBrush.ImageSource = _backTarget;
            DrawBack(0);

            Loaded += OnLoaded;
            Closed += (s, e) => { CompositionTarget.Rendering -= OnRendering; _hooked = false; };
        }

        // =====================================================================
        //  PRZÓD KARTY: zdjęcie + delikatna winieta, obwódka i czerwony pasek
        // =====================================================================
        private static BitmapSource BuildFront(BitmapSource img)
        {
            int w = img.PixelWidth, h = img.PixelHeight;
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawImage(img, new Rect(0, 0, w, h));

                var vig = new RadialGradientBrush
                {
                    Center = new Point(0.5, 0.5),
                    GradientOrigin = new Point(0.5, 0.5),
                    RadiusX = 0.78,
                    RadiusY = 0.78
                };
                vig.GradientStops.Add(new GradientStop(Color.FromArgb(0, 25, 35, 70), 0.55));
                vig.GradientStops.Add(new GradientStop(Color.FromArgb(52, 25, 35, 70), 1.0));
                dc.DrawRectangle(vig, null, new Rect(0, 0, w, h));

                dc.DrawRectangle(new SolidColorBrush(Crimson), null, new Rect(0, h - 14, w, 14));

                var pen = new Pen(new SolidColorBrush(Color.FromRgb(200, 207, 222)), 6);
                dc.DrawRectangle(null, pen, new Rect(3, 3, w - 6, h - 6));
            }
            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            rtb.Freeze();
            return rtb;
        }

        // =====================================================================
        //  TYŁ KARTY: przygotowanie
        // =====================================================================
        private void PrepareBack(int w, int h)
        {
            _texW = w;
            _texH = h;
            _backTarget = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            _backBg = BuildBackBackground(w, h);

            double ppd = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            var black = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Black, FontStretches.Normal);
            var semi = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
            var serif = new Typeface(new FontFamily("Georgia"), FontStyles.Italic, FontWeights.Normal, FontStretches.Normal);

            Brush fill = new LinearGradientBrush(Color.FromRgb(255, 255, 255), Color.FromRgb(150, 172, 222), 90);
            fill.Freeze();
            Brush shadow = new SolidColorBrush(Color.FromArgb(150, 0, 0, 0));
            shadow.Freeze();

            _barWhite = new SolidColorBrush(Color.FromRgb(232, 237, 247)); _barWhite.Freeze();
            _barRed = new SolidColorBrush(Crimson); _barRed.Freeze();

            var sheen = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
            sheen.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 0.0));
            sheen.GradientStops.Add(new GradientStop(Color.FromArgb(95, 255, 255, 255), 0.5));
            sheen.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 255, 255), 1.0));
            sheen.Freeze();
            _sheenBrush = sheen;

            FormattedText Make(char c, double s, Brush b) =>
                new FormattedText(c.ToString(), CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, black, s, b, ppd);

            // --- wielki napis ANDERS ---
            double trackingUnit = 0.04;
            double total100 = 0;
            foreach (char c in BackText)
                total100 += Make(c, 100, fill).Width + 100 * trackingUnit;
            total100 -= 100 * trackingUnit;

            double size = 100 * (w * 0.80) / total100;
            double x = (w - (total100 * size / 100)) / 2;

            double maxHeight = 0;
            foreach (char c in BackText)
            {
                var ft = Make(c, size, fill);
                _letters.Add(ft);
                _letterShadows.Add(Make(c, size, shadow));
                _letterX.Add(x);
                x += ft.Width + size * trackingUnit;
                maxHeight = Math.Max(maxHeight, ft.Height);
            }
            double centerY = h * 0.42;
            _letterY = centerY - maxHeight / 2 - size * 0.06;
            _shadowOffset = size * 0.035;

            // --- napisy dodatkowe (pojawiają się kolejno) ---
            Brush muted = new SolidColorBrush(Color.FromRgb(174, 187, 214)); muted.Freeze();
            Brush light = new SolidColorBrush(Color.FromRgb(201, 211, 230)); light.Freeze();
            Brush dim = new SolidColorBrush(Color.FromRgb(127, 144, 179)); dim.Freeze();

            AddTracked("GENERAŁ  WŁADYSŁAW", w / 2.0, h * 0.095, h * 0.043, muted, semi, 0.32, 0.45, ppd);
            AddTracked("1892 – 1970", w / 2.0, h * 0.635, h * 0.058, light, serif, 0.12, 0.90, ppd);
            AddTracked("ZESPÓŁ SZKÓŁ IM. GEN. WŁADYSŁAWA ANDERSA W CZĘSTOCHOWIE",
                       w / 2.0, h * 0.885, h * 0.026, dim, semi, 0.18, 1.20, ppd);
        }

        private void AddTracked(string text, double cx, double y, double size, Brush brush,
                                Typeface face, double trackingEm, double delay, double ppd)
        {
            var fts = new List<FormattedText>();
            double total = 0;
            foreach (char c in text)
            {
                var ft = new FormattedText(c.ToString(), CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight, face, size, brush, ppd);
                fts.Add(ft);
                total += ft.WidthIncludingTrailingWhitespace + size * trackingEm;
            }
            total -= size * trackingEm;

            double x = cx - total / 2;
            foreach (var ft in fts)
            {
                _extras.Add(new Item { Ft = ft, X = x, Y = y, Delay = delay });
                x += ft.WidthIncludingTrailingWhitespace + size * trackingEm;
            }
        }

        // statyczne tło tyłu karty: granat, poświata, pierścienie, ramka z czerwonymi narożnikami
        private static BitmapSource BuildBackBackground(int w, int h)
        {
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var rect = new Rect(0, 0, w, h);

                var bg = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1) };
                bg.GradientStops.Add(new GradientStop(Color.FromRgb(0x24, 0x37, 0x6A), 0.0));
                bg.GradientStops.Add(new GradientStop(Color.FromRgb(0x12, 0x1C, 0x3A), 0.55));
                bg.GradientStops.Add(new GradientStop(Color.FromRgb(0x07, 0x0B, 0x1A), 1.0));
                dc.DrawRectangle(bg, null, rect);

                var glow = new RadialGradientBrush
                {
                    Center = new Point(0.5, 0.42),
                    GradientOrigin = new Point(0.5, 0.42),
                    RadiusX = 0.55,
                    RadiusY = 0.7
                };
                glow.GradientStops.Add(new GradientStop(Color.FromArgb(120, 70, 110, 200), 0.0));
                glow.GradientStops.Add(new GradientStop(Color.FromArgb(0, 70, 110, 200), 1.0));
                dc.DrawRectangle(glow, null, rect);

                var ringPen = new Pen(new SolidColorBrush(Color.FromArgb(24, 255, 255, 255)), h * 0.003);
                var c = new Point(w / 2.0, h * 0.42);
                dc.DrawEllipse(null, ringPen, c, h * 0.36, h * 0.36);
                dc.DrawEllipse(null, ringPen, c, h * 0.47, h * 0.47);
                dc.DrawEllipse(null, ringPen, c, h * 0.60, h * 0.60);

                double inset = h * 0.034;
                var framePen = new Pen(new SolidColorBrush(Color.FromArgb(75, 255, 255, 255)), 2);
                dc.DrawRectangle(null, framePen, new Rect(inset, inset, w - 2 * inset, h - 2 * inset));

                var cp = new Pen(new SolidColorBrush(Crimson), h * 0.007)
                {
                    StartLineCap = PenLineCap.Square,
                    EndLineCap = PenLineCap.Square
                };
                double L = h * 0.07;
                double l = inset, r = w - inset, t = inset, b = h - inset;
                dc.DrawLine(cp, new Point(l, t), new Point(l + L, t));
                dc.DrawLine(cp, new Point(l, t), new Point(l, t + L));
                dc.DrawLine(cp, new Point(r, t), new Point(r - L, t));
                dc.DrawLine(cp, new Point(r, t), new Point(r, t + L));
                dc.DrawLine(cp, new Point(l, b), new Point(l + L, b));
                dc.DrawLine(cp, new Point(l, b), new Point(l, b - L));
                dc.DrawLine(cp, new Point(r, b), new Point(r - L, b));
                dc.DrawLine(cp, new Point(r, b), new Point(r, b - L));

                var borderPen = new Pen(new SolidColorBrush(Color.FromRgb(200, 207, 222)), 6);
                dc.DrawRectangle(null, borderPen, new Rect(3, 3, w - 6, h - 6));
            }
            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            rtb.Freeze();
            return rtb;
        }

        // =====================================================================
        //  ANIMACJA TYŁU (rysowana klatka po klatce)
        // =====================================================================
        private void DrawBack(double t)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                var full = new Rect(0, 0, _texW, _texH);
                dc.DrawImage(_backBg, full);

                // napisy dodatkowe
                foreach (var it in _extras)
                {
                    double p = Clamp01((t - it.Delay) / 0.7);
                    if (p <= 0) continue;
                    double eo = EaseOutCubic(p);
                    dc.PushOpacity(eo);
                    dc.DrawText(it.Ft, new Point(it.X, it.Y + (1 - eo) * _texH * 0.014));
                    dc.Pop();
                }

                // litery ANDERS
                for (int i = 0; i < _letters.Count; i++)
                {
                    double p = (t - LetterDelayStart - i * LetterStagger) / LetterDuration;
                    if (p <= 0) continue;
                    if (p > 1) p = 1;

                    double eb = EaseOutBack(p);
                    double scale = 0.35 + 0.65 * eb;
                    double dropY = (1 - eb) * _texH * 0.10;

                    var ft = _letters[i];
                    double cx = _letterX[i] + ft.Width / 2;
                    double cy = _letterY + ft.Height / 2 + dropY;

                    dc.PushOpacity(Math.Min(1, p * 2.5));
                    dc.PushTransform(new ScaleTransform(scale, scale, cx, cy));
                    dc.DrawText(_letterShadows[i], new Point(_letterX[i], _letterY + dropY + _shadowOffset));
                    dc.DrawText(ft, new Point(_letterX[i], _letterY + dropY));
                    dc.Pop();
                    dc.Pop();
                }

                // dwukolorowa kreska (biało-czerwona)
                double q = Clamp01((t - BarsStart) / 0.55);
                if (q > 0)
                {
                    double eq = EaseOutCubic(q);
                    double half = _texW * 0.12 * eq;
                    double by = _texH * 0.755;
                    double th = _texH * 0.0075;
                    double mid = _texW / 2.0;
                    dc.DrawRectangle(_barWhite, null, new Rect(mid - half, by, half, th));
                    dc.DrawRectangle(_barRed, null, new Rect(mid, by, half, th));
                }

                // błysk światła
                double sp = (t - SheenStart) / SheenDuration;
                if (sp > 0 && sp < 1)
                {
                    double es = EaseInOutCubic(sp);
                    double bandW = _texW * 0.22;
                    double pos = -bandW * 2 + es * (_texW + bandW * 4);
                    dc.PushClip(new RectangleGeometry(full));
                    dc.PushTransform(new RotateTransform(18, _texW / 2.0, _texH / 2.0));
                    dc.DrawRectangle(_sheenBrush, null, new Rect(pos, -_texH, bandW, _texH * 3));
                    dc.Pop();
                    dc.Pop();
                }
            }
            _backTarget.Clear();
            _backTarget.Render(visual);
        }

        // =====================================================================
        //  START / INTRO
        // =====================================================================
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_hooked) return;
            _hooked = true;
            _life.Start();
            CompositionTarget.Rendering += OnRendering;
            PlayIntro();
        }

        private void PlayIntro()
        {
            var spin = new DoubleAnimation(-100, 0, TimeSpan.FromMilliseconds(1500))
            {
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.4 }
            };
            Spin.BeginAnimation(AxisAngleRotation3D.AngleProperty, spin);

            var grow = new DoubleAnimation(0.45, 1, TimeSpan.FromMilliseconds(1300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            IntroScale.BeginAnimation(ScaleTransform3D.ScaleXProperty, grow);
            IntroScale.BeginAnimation(ScaleTransform3D.ScaleYProperty, grow);
            IntroScale.BeginAnimation(ScaleTransform3D.ScaleZProperty, grow);

            FadeSlide(Greeting, GreetingShift, 150);
            FadeSlide(TitleMain, TitleShift, 350);
            FadeSlide(TitleSub, SubShift, 550);

            AccentScale.BeginAnimation(ScaleTransform.ScaleXProperty,
                new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(800))
                {
                    BeginTime = TimeSpan.FromMilliseconds(950),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                });
        }

        private static void FadeSlide(UIElement el, TranslateTransform shift, int delayMs)
        {
            var dur = TimeSpan.FromMilliseconds(900);
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            el.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, dur)
            {
                BeginTime = TimeSpan.FromMilliseconds(delayMs),
                EasingFunction = ease
            });
            shift.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(-26, 0, dur)
            {
                BeginTime = TimeSpan.FromMilliseconds(delayMs),
                EasingFunction = ease
            });
        }

        // =====================================================================
        //  MYSZ
        // =====================================================================
        private void Root_MouseEnter(object sender, MouseEventArgs e)
        {
            _hovering = true;
            RotateTo(180);
            StartBuild();
        }

        private void Root_MouseLeave(object sender, MouseEventArgs e)
        {
            _hovering = false;
            _targetTiltX = _targetTiltY = 0;
            RotateTo(0, () => { if (!_hovering) { StopBuild(); DrawBack(0); } });
        }

        private void Root_MouseMove(object sender, MouseEventArgs e)
        {
            if (RootGrid.ActualWidth <= 0 || RootGrid.ActualHeight <= 0) return;
            var p = e.GetPosition(RootGrid);
            double nx = p.X / RootGrid.ActualWidth * 2 - 1;
            double ny = p.Y / RootGrid.ActualHeight * 2 - 1;
            _targetTiltY = nx * 9;   // karta "patrzy" na kursor
            _targetTiltX = ny * 6;
        }

        private void RotateTo(double angle, Action completed = null)
        {
            var animation = new DoubleAnimation(angle, TimeSpan.FromMilliseconds(800))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            if (completed != null) animation.Completed += (s, e) => completed();
            Spin.BeginAnimation(AxisAngleRotation3D.AngleProperty, animation);

            // karta lekko "wyskakuje" w stronę widza podczas obrotu
            var pulse = new DoubleAnimation(1.07, TimeSpan.FromMilliseconds(400))
            {
                AutoReverse = true,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            IntroScale.BeginAnimation(ScaleTransform3D.ScaleXProperty, pulse);
            IntroScale.BeginAnimation(ScaleTransform3D.ScaleYProperty, pulse);
            IntroScale.BeginAnimation(ScaleTransform3D.ScaleZProperty, pulse);
        }

        private void StartBuild()
        {
            _buildClock.Restart();
            _building = true;
            DrawBack(0);
        }

        private void StopBuild()
        {
            _building = false;
            _buildClock.Stop();
        }

        // =====================================================================
        //  GŁÓWNA PĘTLA ANIMACJI
        // =====================================================================
        private void OnRendering(object sender, EventArgs args)
        {
            double t = _life.Elapsed.TotalSeconds;
            double dt = Math.Min(0.05, Math.Max(0.0001, t - _lastT));
            _lastT = t;

            // unoszenie się + delikatne kołysanie + płynne nachylanie za myszką
            double bob = Math.Sin(t * 1.25);
            FloatMove.OffsetY = 0.05 * bob;

            double k = 1 - Math.Exp(-dt * 5.5);
            _tiltX += (_targetTiltX - _tiltX) * k;
            _tiltY += (_targetTiltY - _tiltY) * k;
            TiltY.Angle = _tiltY + 2.5 * Math.Sin(t * 0.6);
            TiltX.Angle = _tiltX + 1.5 * Math.Sin(t * 0.9 + 1.0);

            // poświata i "cień" pod kartą reagują na obrót i unoszenie
            double intro = Clamp01((t - 0.3) / 1.2);
            double face = Math.Abs(Math.Cos(Spin.Angle * Math.PI / 180.0));
            double lift = 1 - 0.07 * bob;
            FloorScale.ScaleX = (0.45 + 0.55 * face) * lift;
            FloorScale.ScaleY = lift;
            FloorGlow.Opacity = intro * (0.8 + 0.2 * Math.Sin(t * 1.25 + 3.14));
            HaloGlow.Opacity = intro * (0.85 + 0.15 * Math.Sin(t * 0.8));

            // podpowiedź
            double hintTarget = (_hovering || t < 2.2) ? 0 : 1;
            _hintAlpha += (hintTarget - _hintAlpha) * k;
            Hint.Opacity = _hintAlpha * (0.55 + 0.35 * Math.Sin(t * 2.4));

            // tło
            double W = ParticleLayer.ActualWidth, H = ParticleLayer.ActualHeight;
            if (W > 0 && H > 0)
            {
                if (_particles.Count == 0) InitParticles(W, H);
                UpdateParticles(t, dt, W, H);

                Canvas.SetLeft(Aurora1, W * 0.25 + Math.Sin(t * 0.13) * W * 0.12 - Aurora1.Width / 2);
                Canvas.SetTop(Aurora1, H * 0.60 + Math.Cos(t * 0.11) * H * 0.10 - Aurora1.Height / 2);
                Canvas.SetLeft(Aurora2, W * 0.78 + Math.Cos(t * 0.10) * W * 0.10 - Aurora2.Width / 2);
                Canvas.SetTop(Aurora2, H * 0.38 + Math.Sin(t * 0.15) * H * 0.12 - Aurora2.Height / 2);
            }

            // animacja napisu na tyle
            if (_building)
            {
                double bt = _buildClock.Elapsed.TotalSeconds;
                DrawBack(bt);
                if (bt >= BuildEnd) _building = false;
            }
        }

        private void InitParticles(double W, double H)
        {
            for (int i = 0; i < 80; i++)
            {
                double size = 1.2 + _rng.NextDouble() * 2.8;
                bool red = _rng.NextDouble() < 0.12;
                var shape = new Ellipse
                {
                    Width = size,
                    Height = size,
                    IsHitTestVisible = false,
                    Fill = new SolidColorBrush(red ? Color.FromRgb(255, 92, 120) : Color.FromRgb(214, 226, 255))
                };
                ParticleLayer.Children.Add(shape);
                _particles.Add(new Particle
                {
                    Shape = shape,
                    X = _rng.NextDouble() * W,
                    Y = _rng.NextDouble() * H,
                    Speed = 6 + size * 7 + _rng.NextDouble() * 6,
                    Sway = 6 + _rng.NextDouble() * 22,
                    Phase = _rng.NextDouble() * Math.PI * 2,
                    Twinkle = 0.8 + _rng.NextDouble() * 1.8,
                    Alpha = 0.25 + _rng.NextDouble() * 0.5
                });
            }
        }

        private void UpdateParticles(double t, double dt, double W, double H)
        {
            foreach (var p in _particles)
            {
                p.Y -= p.Speed * dt;
                if (p.Y < -10)
                {
                    p.Y = H + 10;
                    p.X = _rng.NextDouble() * W;
                }
                double x = p.X + Math.Sin(t * 0.4 + p.Phase) * p.Sway;
                p.Shape.Opacity = p.Alpha * (0.55 + 0.45 * Math.Sin(t * p.Twinkle + p.Phase));
                Canvas.SetLeft(p.Shape, x);
                Canvas.SetTop(p.Shape, p.Y);
            }
        }

        // =====================================================================
        //  FUNKCJE POMOCNICZE
        // =====================================================================
        private static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);

        private static double EaseOutCubic(double x) => 1 - Math.Pow(1 - x, 3);

        private static double EaseInOutCubic(double x) =>
            x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;

        private static double EaseOutBack(double x)
        {
            const double c1 = 1.70158;
            const double c3 = c1 + 1;
            return 1 + c3 * Math.Pow(x - 1, 3) + c1 * Math.Pow(x - 1, 2);
        }
    }
}
