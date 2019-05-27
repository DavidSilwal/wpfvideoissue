using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfVideoLoopback
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _activityTimer;
        private Point _inactiveMousePosition = new Point(0, 0);

        private const int _idleTime = 5;

        private static int increment;

        public MainWindow()
        {
            InitializeComponent();

            videoplayer.Source = new Uri("C://video.mp4");  //put the video file path you have
            InitializedIdleTimerEvent();
        }

        private void InitializedIdleTimerEvent()
        {
            InputManager.Current.PreProcessInput += OnActivity;

            _activityTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_idleTime),
                IsEnabled = true
            };

            _activityTimer.Tick += OnInactivity;
        }

        private async void OnInactivity(object sender, EventArgs e)
        {
            // remember mouse position
            _inactiveMousePosition = Mouse.GetPosition(this);
            // set UI on inactivity

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(new Action(() =>
            {
                Grid1.Visibility = Visibility.Hidden;
                videoplayer.Play();
                Grid2.Visibility = Visibility.Visible;
            }));
        }

        private async void OnActivity(object sender, PreProcessInputEventArgs e)
        {
            var inputEventArgs = e.StagingItem.Input;

            if (inputEventArgs is System.Windows.Input.MouseEventArgs || inputEventArgs is KeyboardEventArgs)
            {
                if (e.StagingItem.Input is System.Windows.Input.MouseEventArgs)
                {
                    var mouseEventArgs = (System.Windows.Input.MouseEventArgs)e.StagingItem.Input;
                    
                    if (!(
                        mouseEventArgs.LeftButton == MouseButtonState.Pressed ||
                        mouseEventArgs.RightButton == MouseButtonState.Pressed ||
                        mouseEventArgs.MiddleButton == MouseButtonState.Pressed ||
                        mouseEventArgs.XButton1 == MouseButtonState.Pressed ||
                        mouseEventArgs.XButton2 == MouseButtonState.Pressed
                        || _inactiveMousePosition != mouseEventArgs.GetPosition(this)
                        ))
                    {
                        return;
                    }
                }

                // set UI on activity
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                {
                    videoplayer.Stop();
                    Grid1.Visibility = Visibility.Visible;
                    
                    Grid2.Visibility = Visibility.Hidden;
                }));

                _activityTimer.Stop();
                _activityTimer.Start();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            lblCount.Content = "You click" + increment;
            increment++;
        }

        private void Videoplayer_Unloaded(object sender, RoutedEventArgs e)
        {
            videoplayer.Position = TimeSpan.Zero;
            videoplayer.Play();
        }

        private void Videoplayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            videoplayer.Position = TimeSpan.Zero;
            videoplayer.Play();
        }
    }
}