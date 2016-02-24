using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WorldHourBand.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WorldHourBand
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public async Task TestGetBandInfo()
        {
            await ((MainPageViewModel)this.Resources["mainPageViewModel"]).TestGetBandInfo();
        }

        public async Task TestGetBandLiveInfo()
        {
            await ((MainPageViewModel)this.Resources["mainPageViewModel"]).TestGetBandLiveInfo();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await TestGetBandInfo();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await TestGetBandLiveInfo();
        }

        private async void SendCustomTile_Click(object sender, RoutedEventArgs e)
        {
            //Clear old stuffs

            await ((MainPageViewModel)this.Resources["mainPageViewModel"]).TestPutTileBand();

            BandMessage.Text = "";
        }
    }
}
