using App2.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace App2
{
    public partial class GamePage : ContentPage
    {
        HubConnection hubConnection;
        ObservableCollection<City> usedСities = new ObservableCollection<City>();
        ObservableCollection<City> allСities = new ObservableCollection<City>();
        City currentCity = new City();
        int numberOfHints = 3;

        public GamePage()
        {
            InitializeComponent();

            if (Preferences.Get("localDataSwitch", true))
            {
                XmlLoadAsync();
            }
            else
            {
                AzureLoadAsync();
            }

            GameListView.ItemsSource = usedСities;
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

        async void AzureLoadAsync()
        {
            hubConnection = new HubConnectionBuilder().WithUrl("https://citieschatapi.azurewebsites.net/chat").Build();

            hubConnection.On<List<City>>("ReceiveCities", (cities) =>
            {
                foreach (var item in cities)
                {
                    this.allСities.Add(new City() { CityName = item.CityName, FlagId = $"a{item.FlagId}.png" });
                }
            });

            try
            {
                await hubConnection.StartAsync();
                await hubConnection.SendAsync("GetCities");
            }
            catch (Exception ex)
            {
                await DisplayAlert($"Error", $"Проверьте подключение к интернету\n{ex.Message}", "OK");
            }
        }

        private void EntryText_Completed(object sender, EventArgs e)
        {
            entryText.Focus();

            try
            {
                if (usedСities.Count > 0)
                {
                    City lastCity = usedСities.Last();
                    lastCity.SecondCityName = lastCity.SecondCityName.Trim('ь', 'ы');

                    currentCity = allСities
                        .Where(x => x.CityName.ToLower().StartsWith(lastCity.SecondCityName.Last().ToString()))
                        .Where(x => x.CityName.ToLower() == entryText.Text.ToLower().Trim())
                        .First();
                }
                else
                {
                    currentCity = allСities
                        .Where(x => x.CityName.ToLower() == entryText.Text.ToLower().Trim())
                        .Select(x => x).First();
                }
                //GameBot();
                // ну как-то так
                usedСities.Add(currentCity);
                allСities.Remove(currentCity);
                GameListView.ScrollTo(usedСities.Last(), ScrollToPosition.End, true);
            }
            catch (Exception)
            {
                entryText.Text = string.Empty;
                //DisplayAlert(ex.Message, "error", "error");
            }

            entryText.Text = string.Empty;
        }

        private void GameBot()
        {
            var botCities = allСities.Where(i => i.CityName.ToLower().StartsWith(currentCity.CityName.Trim('ь', 'ы').Last().ToString().ToLower()));

            var botCity = botCities.ToList()[new Random().Next(0, botCities.Count())];

            allСities.Remove(botCity);
            currentCity.SecondCityName = botCity.CityName;
            currentCity.SecondFlagId = botCity.FlagId;
        }

        private async void Hint_Clicked(object sender, EventArgs e)
        {
            if (numberOfHints == 0)
            {
                await DisplayAlert("У вас закончились подсказки",
                    "Узнать новые города вы можете в нашей базе",
                    "ОК");
            }
            else if (await DisplayAlert("Вы действительно хотите использовать подсказку?", $"У вас их {numberOfHints}", "ОК", "ОТМЕНА") && numberOfHints > 0)
            {
                var botCity = new City();
                try
                {
                    if (usedСities.Count > 0)
                    {
                        City lastCity = usedСities.Last();
                        lastCity.SecondCityName = lastCity.SecondCityName.Trim('ь', 'ы');

                        currentCity = allСities
                            .Where(x => x.CityName.ToLower().StartsWith(lastCity.SecondCityName.Last().ToString()))
                            .First();
                    }
                    else
                    {
                        currentCity = allСities[new Random().Next(0, allСities.Count())];
                    }
                    numberOfHints--;
                    GameBot();
                    usedСities.Add(currentCity);
                    allСities.Remove(currentCity);
                    GameListView.ScrollTo(usedСities.Last(), ScrollToPosition.End, true);

                }
                catch (Exception ex)
                {
                    //await DisplayAlert(ex.Message, "error", "error");
                }
                entryText.Text = "";
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () => {
                var result = await this.DisplayAlert("ВНИМАНИЕ", "Завершить игру и выйти в главное меню?", "Да", "Нет");
                //if (result) await this.Navigation.PopAsync(); // or anything else
                if (result) await this.Navigation.PushAsync(new MainPage(), false);
            });

            return true;
        }

        private void CloseGamePage(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await this.DisplayAlert("ВНИМАНИЕ", "Завершить игру и выйти в главное меню?", "Да", "Нет");
                //if (result) await this.Navigation.PopAsync();
                if (result) await this.Navigation.PushAsync(new MainPage(), false);
            });
        }
    }
}