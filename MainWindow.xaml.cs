﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
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

        private const int _idleTime = 2;

        private static int increment;

        private static readonly DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            videoplayer.Source = new Uri("C://video.mp4");  //put the video file path you have
            InitializedIdleTimerEvent();

            dispatcherTimer.Tick += GetStatus;

            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

        }

        private async void GetStatus(object sender, EventArgs e)
        {
            if (Process.GetProcessesByName("notepad").Length > 0) //for the demonstration , i put notepad
            {
                //just for validate, need to refactor.

                if (Grid2.Visibility == Visibility.Visible)
                {
                    await Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                    {
                        WindowState = WindowState.Normal;
                        WindowStyle = WindowStyle.SingleBorderWindow;

                        videoplayer.Stop();
                        Grid1.Visibility = Visibility.Visible;

                        Grid2.Visibility = Visibility.Hidden;
                    }));

                    _activityTimer.Stop();
                    _activityTimer.Start();
                }

            }
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

            if (Process.GetProcessesByName("notepad").Length > 0) //for the demonstration , i put notepad      
            {
                return;
            }
            Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fffffff") + " Inactivity");

            await Application.Current.Dispatcher.InvokeAsync(new Action(() =>
            {
                Grid1.Visibility = Visibility.Hidden;

                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;

                videoplayer.Play();
                Grid2.Visibility = Visibility.Visible;
            }));
        }

        private async void OnActivity(object sender, PreProcessInputEventArgs e)
        {
            var inputEventArgs = e.StagingItem.Input;

            switch (inputEventArgs.RoutedEvent.Name)
            {
                case "PreviewGotKeyboardFocus":
                case "PreviewKeyboardInputProviderAcquireFocus":
                case "KeyboardInputProviderAcquireFocus":
                case "LostKeyboardFocus":
                case "GotKeyboardFocus":
                    return;
            }

            if (inputEventArgs is MouseEventArgs || inputEventArgs is KeyboardEventArgs)
            {
                if (e.StagingItem.Input is MouseEventArgs)
                {
                    var mouseEventArgs = (MouseEventArgs)e.StagingItem.Input;

                    if (!(
                           mouseEventArgs.LeftButton == MouseButtonState.Pressed
                        || mouseEventArgs.RightButton == MouseButtonState.Pressed
                        || mouseEventArgs.MiddleButton == MouseButtonState.Pressed
                        || mouseEventArgs.XButton1 == MouseButtonState.Pressed
                        || mouseEventArgs.XButton2 == MouseButtonState.Pressed
                        //|| _inactiveMousePosition != mouseEventArgs.GetPosition(this)
                        ))
                    {
                        return;
                    }
                }
                
                Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss.fffffff") + " Activity " + inputEventArgs.RoutedEvent.ToString());
                
                // set UI on activity
                await Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                {
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;

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
            videoplayer.Stop();
        }

        private void Videoplayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            videoplayer.Position = TimeSpan.Zero;
            videoplayer.Stop();
        }
    }
}