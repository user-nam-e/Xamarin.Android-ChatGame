using App2.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GameLobbyPage : ContentPage
    {
        Player player1Right { get; set; }
        Player player2Left { get; set; }
        ObservableCollection<City> usedСitiesRight = new ObservableCollection<City>();
        ObservableCollection<City> usedСitiesLeft = new ObservableCollection<City>();
        ObservableCollection<City> allСities = new ObservableCollection<City>();
        bool accesToWrite;

        public GameLobbyPage(string right, string left, string leftName, bool acces)
        {
            InitializeComponent();
            CheckAcces();
            HubHandler();

            //AzureLoadAsync();

            XmlLoadAsync();


            player1Right = new Player() { ConnectionId = right, PlayerName = Preferences.Get("playerName", "") };
            player2Left = new Player() { ConnectionId = left, PlayerName = leftName };
            accesToWrite = acces;

            GameListViewRight.ItemsSource = usedСitiesRight;
            GameListViewLeft.ItemsSource = usedСitiesLeft;
        }


        async void XmlLoadAsync()
        {
            using (var stream = await FileSystem.OpenAppPackageFileAsync("cityData.xml"))
            {
                XDocument xdoc = XDocument.Load(stream);

                foreach (XElement cityElement in xdoc.Element("rocid").Elements("city"))
                {
                    XElement nameCityElement = cityElement.Element("name");
                    XElement countryNameElement = cityElement.Element("country");
                    XElement idElement = cityElement.Element("ID");

                    if (nameCityElement != null && countryNameElement != null && idElement != null)
                    {
                        allСities.Add(new City() { CityName = nameCityElement.Value, FlagId = $"a{idElement.Value}.png" });
                    }
                }
            }
        }

        async void HubHandler()
        {
            try
            {
                SelectPlayerPage.hubConnection.On<List<City>>("ReceiveCities", (cities) =>
                {
                    foreach (var item in cities)
                    {
                        this.allСities.Add(new City() { CityName = item.CityName, FlagId = $"a{item.FlagId}.png" });
                    }
                });

                SelectPlayerPage.hubConnection.On<GameCity>("ReceiveMessageFromUser", (city) =>
                {
                    usedСitiesLeft.Add(new City() { CityName = city.CityName, FlagId = city.FlagId });

                    GameListViewLeft.ScrollTo(usedСitiesLeft.Last(), ScrollToPosition.End, true);

                    accesToWrite = true;
                });

            }
            catch (Exception ex)
            {
                await DisplayAlert($"Error", $"Проверьте подключение к интернету\n{ex.Message}", "OK");
            }
        }

        private async void EntryText_Completed(object sender, EventArgs e)
        {
            if (accesToWrite)
            {
                try
                {
                    try
                    {
                        var currentCity = new City();

                        if (usedСitiesRight.Count > 0)
                        {
                            City lastCity = usedСitiesLeft.Last();
                            lastCity.CityName = lastCity.CityName.Trim('ь', 'ы');

                            currentCity = allСities
                                .Where(x => x.CityName.ToLower().StartsWith(lastCity.CityName.Last().ToString()))
                                .Where(x => x.CityName.ToLower() == entryText.Text.ToLower().Trim())
                                .First();
                        }
                        else
                        {
                            currentCity = allСities
                                .Where(x => x.CityName.ToLower() == entryText.Text.ToLower().Trim())
                                .Select(x => x).First();
                        }

                        accesToWrite = false;
                        usedСitiesRight.Add(currentCity);

                        GameListViewRight.ScrollTo(usedСitiesRight.Last(), ScrollToPosition.End, true);

                        await SelectPlayerPage.hubConnection.SendAsync("SendMessageToUser", player2Left.ConnectionId,
                            new GameCity() { CityName = currentCity.CityName, FlagId = currentCity.FlagId });
                    }
                    catch (Exception)
                    {
                        entryText.Text = string.Empty;
                    }

                    entryText.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    await SelectPlayerPage.hubConnection.SendAsync("SendCloseLobby", player2Left.ConnectionId);
                    await DisplayAlert($"Error", $"Проверьте подключение к интернету\n{ex.Message}", "OK");
                    await this.Navigation.PopAsync();
                }
            }
            else
            {
                entryText.Text = string.Empty;
            }
        }

        async void AzureLoadAsync()
        {
            try
            {
                await SelectPlayerPage.hubConnection.SendAsync("GetCities");
            }
            catch (Exception ex)
            {
                await DisplayAlert($"Error", $"Проверьте подключение к интернету\n{ex.Message}", "OK");
            }
        }


        private void GameBot()
        {

        }

        private async void Hint_Clicked(object sender, EventArgs e)
        {

        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await this.DisplayAlert("ВНИМАНИЕ", $"Завершить игру и выйти в главное меню?", "Да", "Нет");
                //if (result) await this.Navigation.PopAsync(); // or anything else
                if (result)
                {
                    await SelectPlayerPage.hubConnection.SendAsync("SendCloseLobby", player2Left.ConnectionId);
                    await this.Navigation.PopAsync(false);
                }
            });

            return true;
        }

        private void CloseGamePage(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await this.DisplayAlert("ВНИМАНИЕ", "Завершить игру и выйти в главное меню?", "Да", "Нет");
                //if (result) await this.Navigation.PopAsync();
                if (result)
                {
                    await SelectPlayerPage.hubConnection.SendAsync("SendCloseLobby", player2Left.ConnectionId);
                    await this.Navigation.PopAsync(false);
                }
            });
        }

        private async void CheckAcces()
        {
            while (true)
            {
                if (accesToWrite)
                {
                    ButtonPlayer1Right.BorderColor = Color.Green;
                    ButtonPlayer2Left.BorderColor = Color.White;
                }
                else
                {
                    ButtonPlayer1Right.BorderColor = Color.White;
                    ButtonPlayer2Left.BorderColor = Color.Green;
                }

                await Task.Delay(1000);
            }
        }
    }
}