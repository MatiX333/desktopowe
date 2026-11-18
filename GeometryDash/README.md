# Geometry Dash Clone (WPF, .NET 10)

Open `GeometryDash/GeometryDash.csproj` in Visual Studio 2022/2026 (or run `dotnet run --project GeometryDash`).

Requires the .NET 10 SDK and Windows. For an older SDK, change `TargetFramework` in the .csproj
(e.g. `net9.0-windows` or `net8.0-windows`).

## Controls
- SPACE / UP / W / left mouse: jump (hold to keep jumping)
- R: restart, ESC: quit

## Editing the level
All level layout is in `Level.cs` (`Level.Build()`): `Pillar(x, width, height)` and `Spike(x, onTop)`.
Physics constants are at the top of `GameWorld.cs`.
