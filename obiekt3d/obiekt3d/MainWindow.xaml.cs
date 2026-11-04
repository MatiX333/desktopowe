
using System.Windows;
using System.Windows.Controls;

namespace obiekt3d
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Horizontal 
        private void RotationSlider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (myAngleRotation != null)
            {
                myAngleRotation.Angle = e.NewValue;
            }
        }

        // Vertical 
        private void VerticalSlider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            if (verticalRotationXaml != null)
            {
                verticalRotationXaml.Angle = e.NewValue;
            }
        }
    }
}
