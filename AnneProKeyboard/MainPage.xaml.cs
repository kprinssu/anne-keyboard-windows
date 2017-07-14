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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AnneProKeyboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            _frame.Navigate(typeof(LayoutPage));
            LayoutMenuButton.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            var colour = titleBar.BackgroundColor;
            if(colour.HasValue)
            {
                navPane.Background = new SolidColorBrush(colour.Value);
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
                LightingMenuButton.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            }
        }

        private void LayoutNav_Clicked(object sender, RoutedEventArgs e)
        {
            if (!(_frame.CurrentSourcePageType == typeof(LayoutPage)))
            {
                _frame.Navigate(typeof(LayoutPage));
                LayoutMenuButton.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            }
        }
    }
}
