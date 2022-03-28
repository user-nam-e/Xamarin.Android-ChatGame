using App2.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace App2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]

    public partial class SelectPlayerPage : ContentPage
    {
        ObservableCollection<Player> players = new ObservableCollection<Player>();
        List<Player> suggestions = new List<Player>();
        public static HubConnection hubConnection;

        public SelectPlayerPage()
        {
            InitializeComponent();
            LoadGif();
        }

        async void LoadGif()
        {
            while (true)
            {
                if (players.Count == 0)
                {
                    labelLoad.IsVisible = true;
                    imageGif.IsVisible = true;
                }
                else
                {
                    labelLoad.IsVisible = false;
                    imageGif.IsVisible = false;
                }

                await Task.Delay(200);
            }

        }

        protected override void OnAppearing()
        {
            if (hubConnection != null)
            {

                countOfPlayers.Text = hubConnection.ConnectionId.Substring(0, 5);
            }
            StartConnection();
            UpdatePlayers();
            playersListView.ItemsSource = players;
            base.OnAppearing();
        }

        private async void UpdatePlayers()
        {
            try
            {
                while (true)
                {
                    await hubConnection.SendAsync("SendConnectedUsers");
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert($"Ошибка", $"Проверьте подключение к интернету\n{ex.Message}", "OK");
                await this.Navigation.PopAsync();
            }
        }

        async void StartConnection()
        {
            hubConnection = new HubConnectionBuilder().WithUrl("https://citieschatapi.azurewebsites.net/chat").Build();

            hubConnection.On("CloseLobby", () =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await this.Navigation.PopAsync(false);
                    await DisplayAlert("Внимание", "Пользователь завершил игру", "ок");
                });
            });

            hubConnection.On<string, string>("OpenGameLobby", (sendersUserId, name) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await hubConnection.SendAsync("RemoveUser");
                    await Navigation.PushAsync(new GameLobbyPage(hubConnection.ConnectionId, sendersUserId, name, true), false);
                });
            });

            hubConnection.On<string, string, string>("RequestToPlay", (targetUserId, sendersName, requestUserId) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    bool answer = await DisplayAlert("Запрос на игру", $"Игрок \"{sendersName}\" ({requestUserId.Substring(0, 5)}) хочет начать игру", "Да", "Нет");

                    if (answer)
                    {
                        await hubConnection.SendAsync("SendAnswerToPlay", requestUserId, targetUserId, Preferences.Get("playerName", ""));
                        await hubConnection.SendAsync("RemoveUser");
                        await Navigation.PushAsync(new GameLobbyPage(targetUserId, requestUserId, sendersName, false), false);
                    }
                    else
                    {
                        await DisplayAlert(answer.ToString(), answer.ToString(), answer.ToString());
                    }
                });

            });

            hubConnection.On<List<Player>>("GetConnectedUsers", (users) =>
            {
                players.Clear();

                foreach (var item in users)
                {
                    if (item.ConnectionId != hubConnection.ConnectionId && !players.Any(x => x.ConnectionId == item.ConnectionId))
                    {
                        players.Add(new Player() { PlayerName = $"{item.PlayerName} ({item.ConnectionId.Substring(0, 5)})", ConnectionId = item.ConnectionId });
                    }
                }
            });

            try
            {
                await hubConnection.StartAsync();

                await hubConnection.SendAsync("AddUser", Preferences.Get("playerName", ""));
                countOfPlayers.Text = hubConnection.ConnectionId.Substring(0, 5);
            }
            catch (Exception ex)
            {
                await DisplayAlert($"Error", $"Проверьте подключение к интернету\n{ex.Message}", "OK");
                await this.Navigation.PopAsync();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (hubConnection.State == HubConnectionState.Connected)
                {
                    await hubConnection.SendAsync("RemoveUser");
                    await this.Navigation.PopAsync();
                }
            });

            return true;
        }

        private void PlayersSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = PlayersSearchBar.Text;
            suggestions = players.Where(c => c.PlayerName.ToLower().Contains(keyword.ToLower())).ToList();
            playersListView.ItemsSource = suggestions;
        }

        private async void buttonPlayer_Clicked(object sender, EventArgs e)
        {
            Player selectedPlayer = (Player)((Xamarin.Forms.ViewCell)sender).BindingContext;

            try
            {
                if (selectedPlayer != null)
                {
                    await hubConnection.SendAsync("SendRequestToPlay", selectedPlayer.ConnectionId, Preferences.Get("playerName", ""), hubConnection.ConnectionId);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert($"Error", $"Проверьте подключение к интернету\n{ex.Message}", "OK");
                await this.Navigation.PopAsync();
            }
        }
    }
}