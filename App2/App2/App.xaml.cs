using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using App2.Views;

namespace App2
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage())
            {
                BarBackgroundColor = Color.FromHex("#6adea0"),
                BarTextColor = Color.White
            };
        }

        protected override void OnStart() { }

        protected override void OnSleep()
        {
            if (Views.SelectPlayerPage.hubConnection != null)
            {
                if (Views.SelectPlayerPage.hubConnection.State == HubConnectionState.Connected)
                {
                    Views.SelectPlayerPage.hubConnection.SendAsync("RemoveUser");
                }
            }

            base.OnSleep();
        }

        protected override void OnResume() { }
    }
}
