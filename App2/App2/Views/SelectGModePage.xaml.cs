using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectGModePage : ContentPage
    {
        public SelectGModePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            if (Views.SelectPlayerPage.hubConnection != null)
            {
                await Views.SelectPlayerPage.hubConnection.SendAsync("RemoveUser");
            }

            base.OnAppearing();
        }

        private async void ButtonPlayWithBot_Clicked(object sender, EventArgs e)
        {
            if (!Preferences.Get("localDataSwitch", true))
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    await Navigation.PushAsync(new GamePage(), false);
                }
                else
                {
                    await DisplayAlert("Ошибка", "Проверьте подключение к интернету или смените базу данных", "OK");
                }
            }
            else
            {
                await Navigation.PushAsync(new GamePage(), false);
            }
        }

        private async void ButtonPlayWithPlayer_Clicked(object sender, EventArgs e)
        {
            string playerName = Preferences.Get("playerName", "");

            if (string.IsNullOrEmpty(playerName))
            {
                await Navigation.PushAsync(new EditPlayerPage(), false);
            }
            else
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    await Navigation.PushAsync(new SelectPlayerPage(), false);
                }
                else
                {
                    await DisplayAlert("Ошибка", "Проверьте подключение к интернету", "OK");
                }
            }
        }
    }
}