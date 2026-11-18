using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GeometryDash;

public sealed class GameView : FrameworkElement
{
    private const double ViewHeight = (Level.Rows + 1) * GameWorld.Tile; // 1 extra row for the floor
    private const double CameraLead = 6 * GameWorld.Tile;                // player sits 6 tiles from the left edge
    private const double Step = 1.0 / 240.0;                             // fixed physics step

    private readonly GameWorld _world = new(Level.Build());
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private double _last;
    private double _accumulator;

    private readonly BitmapImage _player = Load("player.png");
    private readonly BitmapImage[] _walls = { Load("wall1.png"), Load("wall2.png"), Load("wall3.png") };

    private static readonly Brush BgTop = Freeze(new LinearGradientBrush(
        Color.FromRgb(20, 60, 200), Color.FromRgb(60, 20, 140), 90));
    private static readonly Brush FloorBrush = Freeze(new SolidColorBrush(Color.FromRgb(16, 16, 40)));
    private static readonly Brush SpikeFill = Freeze(new SolidColorBrush(Color.FromRgb(20, 20, 20)));
    private static readonly Pen SpikePen = FreezePen(new Pen(Brushes.White, 1.5));
    private static readonly Pen LinePen = FreezePen(new Pen(Brushes.White, 2));
    private static readonly Brush FinishBrush = Freeze(new SolidColorBrush(Color.FromRgb(255, 215, 0)));
    private static readonly Brush ParticleBrush = Freeze(new SolidColorBrush(Color.FromRgb(190, 108, 226)));

    public bool JumpHeld { get; set; }

    public GameView()
    {
        ClipToBounds = true;
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        Loaded += (_, _) =>
        {
            _last = _clock.Elapsed.TotalSeconds;
            CompositionTarget.Rendering += OnFrame;
        };
        Unloaded += (_, _) => CompositionTarget.Rendering -= OnFrame;
    }

    public void PressedJump()
    {
        // Space after finishing the level restarts it
        if (_world.Won) _world.Reset();
    }

    public void Restart() => _world.Reset();

    private void OnFrame(object? sender, EventArgs e)
    {
        double now = _clock.Elapsed.TotalSeconds;
        double dt = Math.Min(now - _last, 0.05);
        _last = now;

        _accumulator += dt;
        while (_accumulator >= Step)
        {
            _world.Update(Step, JumpHeld);
            _accumulator -= Step;
        }

        if (_world.Dead && _world.DeadTime > 0.8) _world.Reset();

        InvalidateVisual();
    }

    // ------------------------------------------------------------------ rendering

    private static BitmapImage Load(string name)
    {
        var img = new BitmapImage(new Uri($"pack://application:,,,/Assets/{name}", UriKind.Absolute));
        img.Freeze();
        return img;
    }

    private static Brush Freeze(Brush b) { b.Freeze(); return b; }
    private static Pen FreezePen(Pen p) { p.Freeze(); return p; }

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth, h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        double s = h / ViewHeight;                       // world -> screen scale
        double camX = _world.X - CameraLead;
        double floorScreen = Math.Round(GameWorld.FloorY * s);

        // Background
        dc.DrawRectangle(BgTop, null, new Rect(0, 0, w, h));
        DrawBackgroundDecor(dc, w, h, s, camX);

        // Floor
        dc.DrawRectangle(FloorBrush, null, new Rect(0, floorScreen, w, h - floorScreen));
        int firstTile = (int)Math.Floor(camX / GameWorld.Tile) - 1;
        int lastTile = (int)Math.Ceiling((camX + w / s) / GameWorld.Tile) + 1;
        for (int tx = firstTile; tx <= lastTile; tx++)
            dc.DrawImage(_walls[1], TileRect(tx * GameWorld.Tile, GameWorld.FloorY, s, camX));
        dc.DrawLine(LinePen, new Point(0, floorScreen), new Point(w, floorScreen));

        // Level tiles
        var level = _world.Level;
        for (int x = Math.Max(0, firstTile); x <= Math.Min(level.Width - 1, lastTile); x++)
        {
            for (int r = 0; r < Level.Rows; r++)
            {
                var t = level.Grid[x, r];
                if (t == TileType.Empty) continue;
                double wx = x * GameWorld.Tile, wy = r * GameWorld.Tile;

                if (t == TileType.Spike) DrawSpike(dc, wx, wy, s, camX);
                else dc.DrawImage(PickWall(level, x, r), TileRect(wx, wy, s, camX));
            }
        }

        // Finish line
        double fx = (_world.Level.FinishX - camX) * s;
        dc.DrawRectangle(FinishBrush, null, new Rect(Math.Round(fx), 0, Math.Max(4, 6 * s), floorScreen));

        // Player
        if (!_world.Dead)
        {
            double cx = (_world.X + GameWorld.Size / 2 - camX) * s;
            double cy = (_world.Y + GameWorld.Size / 2) * s;
            double ps = GameWorld.Size * s;
            dc.PushTransform(new RotateTransform(_world.Angle, cx, cy));
            dc.DrawImage(_player, new Rect(cx - ps / 2, cy - ps / 2, ps, ps));
            dc.Pop();
        }

        // Particles
        foreach (var p in _world.Particles)
        {
            double px = (p.X - camX) * s, py = p.Y * s, ps = 8 * s;
            dc.DrawRectangle(ParticleBrush, null, new Rect(px - ps / 2, py - ps / 2, ps, ps));
        }

        DrawHud(dc, w, h);
    }

    private void DrawBackgroundDecor(DrawingContext dc, double w, double h, double s, double camX)
    {
        // Simple parallax squares
        var brush = new SolidColorBrush(Color.FromArgb(28, 255, 255, 255));
        brush.Freeze();
        double spacing = 160;
        double offset = -(camX * 0.3) % spacing;
        if (offset > 0) offset -= spacing;
        for (double x = offset; x < w / s + spacing; x += spacing)
        {
            int idx = (int)Math.Round((x - offset) / spacing);
            double size = 40 + (idx * 37 % 5) * 12;
            double y = 20 + (idx * 53 % 7) * 30;
            dc.DrawRectangle(brush, null, new Rect(x * s, y * s, size * s, size * s));
        }
    }

    private static bool IsSolid(Level level, int x, int r)
        => level.Get(x, r) is TileType.Block1 or TileType.Block2 or TileType.Block3;

    /// wall1 = lone block (full outline), wall2 = top cap, wall3 = body (sides only).
    private BitmapImage PickWall(Level level, int x, int r)
    {
        if (IsSolid(level, x, r - 1)) return _walls[2];            // something above -> body
        bool below = r + 1 >= Level.Rows || IsSolid(level, x, r + 1); // floor counts as solid
        return below ? _walls[1] : _walls[0];                       // top cap, or floating block
    }

    private static Rect TileRect(double wx, double wy, double s, double camX)
    {
        // Snap to whole pixels, sizing from the next tile's edge so there are no seams.
        double x0 = Math.Floor((wx - camX) * s);
        double x1 = Math.Floor((wx + GameWorld.Tile - camX) * s);
        double y0 = Math.Floor(wy * s);
        double y1 = Math.Floor((wy + GameWorld.Tile) * s);
        return new Rect(x0, y0, Math.Max(1, x1 - x0), Math.Max(1, y1 - y0));
    }

    private static void DrawSpike(DrawingContext dc, double wx, double wy, double s, double camX)
    {
        double x = (wx - camX) * s, y = wy * s, t = GameWorld.Tile * s;
        var geo = new StreamGeometry();
        using (var g = geo.Open())
        {
            g.BeginFigure(new Point(x + t * 0.5, y + t * 0.04), true, true);
            g.LineTo(new Point(x + t * 0.96, y + t), true, false);
            g.LineTo(new Point(x + t * 0.04, y + t), true, false);
        }
        geo.Freeze();
        dc.DrawGeometry(SpikeFill, SpikePen, geo);
    }

    private void DrawHud(DrawingContext dc, double w, double h)
    {
        double ppd = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        // Progress bar
        double barW = Math.Min(400, w * 0.5), barX = (w - barW) / 2, barY = 14;
        dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)), null, new Rect(barX, barY, barW, 10));
        dc.DrawRectangle(Brushes.LimeGreen, null, new Rect(barX, barY, barW * _world.Progress, 10));

        Text(dc, $"{(int)(_world.Progress * 100)}%", 18, barX + barW + 10, 6, Brushes.White, ppd);
        Text(dc, $"Attempt {_world.Attempt}", 18, 14, 6, Brushes.White, ppd);

        if (_world.Won)
        {
            CenterText(dc, "LEVEL COMPLETE!", 54, w, h * 0.30, FinishBrush, ppd);
            CenterText(dc, "Press SPACE to play again", 22, w, h * 0.30 + 70, Brushes.White, ppd);
        }

        if (_world.Attempt == 1 && _world.X < 12 * GameWorld.Tile && !_world.Dead)
        {
            CenterText(dc, "SPACE / UP / W / Click = jump (hold to keep jumping)   R = restart   ESC = quit",
                16, w, h - 34, Brushes.White, ppd);
        }
    }

    private static void Text(DrawingContext dc, string text, double size, double x, double y, Brush brush, double ppd)
    {
        var ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
            size, brush, ppd);
        dc.DrawText(ft, new Point(x, y));
    }

    private static void CenterText(DrawingContext dc, string text, double size, double w, double y, Brush brush, double ppd)
    {
        var ft = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
            size, brush, ppd);
        dc.DrawText(ft, new Point((w - ft.Width) / 2, y));
    }
}
