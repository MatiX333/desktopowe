namespace GeometryDash;

public sealed class Particle
{
    public double X, Y, VX, VY, Life;
}

public sealed class GameWorld
{
    // World units: 1 tile = 32 units (same as the sprites).
    public const double Tile = 32;
    public const double Size = 32;
    public const double Speed = 290;        // horizontal units / second
    public const double Gravity = 2600;
    public const double JumpVelocity = 640;
    public const double MaxFall = 1000;
    public const double FloorY = Level.Rows * Tile;
    private const double Eps = 0.01;

    private readonly Level _level;
    private readonly Random _rng = new();

    public double X, Y, VelY, Angle;
    public bool OnGround, Dead, Won;
    public double DeadTime;
    public int Attempt;
    public readonly List<Particle> Particles = new();

    public Level Level => _level;
    public double Progress => Math.Clamp(X / _level.FinishX, 0, 1);

    public GameWorld(Level level)
    {
        _level = level;
        Reset();
    }

    public void Reset()
    {
        Attempt++;
        X = 4 * Tile;
        Y = FloorY - Size;
        VelY = 0;
        Angle = 0;
        OnGround = true;
        Dead = false;
        Won = false;
        DeadTime = 0;
        Particles.Clear();
    }

    public void Update(double dt, bool jumpHeld)
    {
        // Particles always animate
        for (int i = Particles.Count - 1; i >= 0; i--)
        {
            var p = Particles[i];
            p.Life -= dt;
            if (p.Life <= 0) { Particles.RemoveAt(i); continue; }
            p.VY += 1800 * dt;
            p.X += p.VX * dt;
            p.Y += p.VY * dt;
        }

        if (Dead) { DeadTime += dt; return; }
        if (Won) return;

        // Jump
        if (OnGround && jumpHeld)
        {
            VelY = -JumpVelocity;
            OnGround = false;
        }

        // ---- Horizontal ----
        X += Speed * dt;

        if (X >= _level.FinishX)
        {
            Won = true;
            return;
        }

        if (TryGetSolidOverlap(X, Y, out double highestTop, out _))
        {
            // Tiny ledge forgiveness: if we only clipped the top few units, pop up onto it.
            if (Y + Size - highestTop <= 6 && VelY >= 0)
            {
                Y = highestTop - Size;
                VelY = 0;
                OnGround = true;
            }
            else
            {
                Die();
                return;
            }
        }

        // ---- Vertical ----
        double prevBottom = Y + Size;
        VelY = Math.Min(VelY + Gravity * dt, MaxFall);
        Y += VelY * dt;
        OnGround = false;

        if (Y + Size >= FloorY)
        {
            Y = FloorY - Size;
            VelY = 0;
            OnGround = true;
        }
        else if (TryGetSolidOverlap(X, Y, out double top, out _))
        {
            if (VelY >= 0 && prevBottom <= top + 0.5)
            {
                Y = top - Size;
                VelY = 0;
                OnGround = true;
            }
            else
            {
                Die(); // hit the underside of a block
                return;
            }
        }

        // ---- Rotation ----
        if (OnGround)
        {
            Angle = Math.Round(Angle / 90.0) * 90.0;
        }
        else
        {
            Angle += 440 * dt;
        }

        // ---- Spikes ----
        if (TouchesSpike())
        {
            Die();
        }
    }

    private bool TryGetSolidOverlap(double px, double py, out double highestTop, out int count)
    {
        highestTop = double.MaxValue;
        count = 0;
        int x0 = (int)Math.Floor((px + Eps) / Tile);
        int x1 = (int)Math.Floor((px + Size - Eps) / Tile);
        int r0 = (int)Math.Floor((py + Eps) / Tile);
        int r1 = (int)Math.Floor((py + Size - Eps) / Tile);

        for (int x = x0; x <= x1; x++)
            for (int r = r0; r <= r1; r++)
            {
                var t = _level.Get(x, r);
                if (t is TileType.Block1 or TileType.Block2 or TileType.Block3)
                {
                    count++;
                    double tileTop = r * Tile;
                    if (tileTop < highestTop) highestTop = tileTop;
                }
            }
        // we want the LOWEST top edge value among overlapped tiles to be the one we'd stand on,
        // but for "highest on screen" (smallest y) tile, landing logic uses the smallest y. Good.
        return count > 0;
    }

    private bool TouchesSpike()
    {
        // Player hit box is slightly smaller than the sprite.
        const double inset = 5;
        double pl = X + inset, pr = X + Size - inset, pt = Y + inset, pb = Y + Size - inset;

        int x0 = (int)Math.Floor(X / Tile);
        int x1 = (int)Math.Floor((X + Size) / Tile);
        int r0 = (int)Math.Floor(Y / Tile);
        int r1 = (int)Math.Floor((Y + Size) / Tile);

        for (int x = x0; x <= x1; x++)
            for (int r = r0; r <= r1; r++)
            {
                if (_level.Get(x, r) != TileType.Spike) continue;
                // Narrow hit box in the lower part of the spike tile
                double sl = x * Tile + 11, sr = x * Tile + 21;
                double st = r * Tile + 12, sb = r * Tile + Tile;
                if (pl < sr && pr > sl && pt < sb && pb > st) return true;
            }
        return false;
    }

    private void Die()
    {
        Dead = true;
        DeadTime = 0;
        for (int i = 0; i < 18; i++)
        {
            double a = _rng.NextDouble() * Math.PI * 2;
            double s = 150 + _rng.NextDouble() * 350;
            Particles.Add(new Particle
            {
                X = X + Size / 2,
                Y = Y + Size / 2,
                VX = Math.Cos(a) * s,
                VY = Math.Sin(a) * s - 150,
                Life = 0.5 + _rng.NextDouble() * 0.4
            });
        }
    }
}
