using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.UI.Xaml.Controls;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Core;
using Windows.Devices.Bluetooth;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AnneProKeyboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;

        public MainPage()
        {
            this.InitializeComponent();
            _frame.Navigate(typeof(LayoutPage));
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonInactiveBackgroundColor = Colors.White;
            titleBar.ButtonInactiveForegroundColor = Color.FromArgb(1, 152, 152, 152);
            Window.Current.SetTitleBar(MainTitleBar);
            Window.Current.Activated += Current_Activated;
            Color systemAccentColor = (Color)App.Current.Resources["SystemAccentColor"];
        }

        private void hamburgerHover(object sender, RoutedEventArgs e)
        {
            //HamburgerButton.Background = new SolidColorBrush(Colors.Red);
        }

        private static Color lightenDarkenColor(Color color, double correctionFactor)
        {
            double red = (255 - color.R) * correctionFactor + color.R;
            double green = (255 - color.G) * correctionFactor + color.G;
            double blue = (255 - color.B) * correctionFactor + color.B;
            return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }

        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != CoreWindowActivationState.Deactivated)
            {
                MainTitleBar.Background = new SolidColorBrush((Color)this.Resources["SystemAccentColor"]);
                pageHeader.Foreground = new SolidColorBrush(Colors.White);
                HamburgerButton.Background = new SolidColorBrush(Color.FromArgb(51,255,255,255));
                HamburgerButton.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                pageHeader.Foreground = new SolidColorBrush(Color.FromArgb(255, 152, 152, 152));
                MainTitleBar.Background = new SolidColorBrush(Colors.White);
                HamburgerButton.Background = new SolidColorBrush(Colors.White);
                HamburgerButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 152, 152, 152));
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
        }

        private void LightingNav_Clicked(object sender, RoutedEventArgs e)
        {
            if (!(_frame.CurrentSourcePageType == typeof(LightingPage)))
            {
                _frame.Navigate(typeof(LightingPage));
                pageHeader.Text = "Lighting";
                LightingMenuButton.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            }
        }

        private void LayoutNav_Clicked(object sender, RoutedEventArgs e)
        {
            if (!(_frame.CurrentSourcePageType == typeof(LayoutPage)))
            {
                _frame.Navigate(typeof(LayoutPage));
                pageHeader.Text = "Layers";
                LayoutMenuButton.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            }
        }

        private void pageHeader_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
