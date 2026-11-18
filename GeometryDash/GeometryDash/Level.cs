namespace GeometryDash;

public enum TileType { Empty, Block1, Block2, Block3, Spike }

/// <summary>
/// Tile grid. Grid[x, r]: r = 0 is the top row, r = Rows-1 is the row sitting on the floor.
/// Builder methods take "height" counted upwards from the floor (0 = first tile above floor).
/// </summary>
public sealed class Level
{
    public const int Rows = 10;
    public int Width { get; }
    public TileType[,] Grid { get; }

    public Level(int width)
    {
        Width = width;
        Grid = new TileType[width, Rows];
    }

    public TileType Get(int x, int r)
    {
        if (x < 0 || x >= Width || r < 0 || r >= Rows) return TileType.Empty;
        return Grid[x, r];
    }

    private void Set(int x, int h, TileType t)
    {
        int r = Rows - 1 - h;
        if (x >= 0 && x < Width && r >= 0 && r < Rows) Grid[x, r] = t;
    }

    private static TileType BlockFor(int x, int h) => TileType.Block1;

    /// <summary>A solid pillar, 'count' tiles tall, 'width' tiles wide.</summary>
    public void Pillar(int x, int width, int count)
    {
        for (int i = 0; i < width; i++)
            for (int h = 0; h < count; h++)
                Set(x + i, h, BlockFor(x + i, h));
    }

    /// <summary>One spike, standing on top of a stack that is 'onTop' tiles tall (0 = on the floor).</summary>
    public void Spike(int x, int onTop = 0) => Set(x, onTop, TileType.Spike);

    public static Level Build()
    {
        var l = new Level(108);

        // Generous spacing, one obstacle at a time.
        l.Spike(18);                       // single spike
        l.Spike(28);                       // single spike
        l.Spike(37); l.Spike(38);          // double spike

        l.Pillar(48, 3, 1);                // hop on a low block

        l.Spike(58);
        l.Spike(66); l.Spike(67);

        l.Pillar(76, 4, 1);                // wide stairs up
        l.Pillar(80, 4, 2);

        l.Spike(90);
        l.Spike(98); l.Spike(99);

        return l;
    }

    /// <summary>X (in world units) of the finish line.</summary>
    public double FinishX => (Width - 2) * GameWorld.Tile;
}
