using System.Windows;
using System.Windows.Input;

namespace GeometryDash;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private static bool IsJumpKey(Key k) => k is Key.Space or Key.Up or Key.W;

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (IsJumpKey(e.Key))
        {
            Game.JumpHeld = true;
            Game.PressedJump();
            e.Handled = true;
        }
        else if (e.Key == Key.R)
        {
            Game.Restart();
        }
        else if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void Window_KeyUp(object sender, KeyEventArgs e)
    {
        if (IsJumpKey(e.Key)) Game.JumpHeld = false;
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        Game.JumpHeld = true;
        Game.PressedJump();
    }

    private void Window_MouseUp(object sender, MouseButtonEventArgs e)
    {
        Game.JumpHeld = false;
    }
}
