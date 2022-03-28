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
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace App2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GameLobbyPage : ContentPage
    {
        Player player1Right { get; set; }
        Player player2Left { get; set; }
        List<Chat> allСities = new List<Chat>();
        ObservableCollection<Chat> usedCities = new ObservableCollection<Chat>();
        bool accesToWrite;
        private int numberOfHints = 100;
        double time = 60;
        bool timerAlive;

        public GameLobbyPage(string right, string left, string leftName, bool acces)
        {
            InitializeComponent();
            hintButton.Text = $"Подсказка ({numberOfHints})";
            CheckAcces();
            HubHandler();

            XmlLoadAsync();
            allСities.RemoveAt(1);
            player1Right = new Player() { ConnectionId = right, PlayerName = Preferences.Get("playerName", "") };
            player2Left = new Player() { ConnectionId = left, PlayerName = leftName };
            accesToWrite = acces;
            timerAlive = acces;

            timerButton.Text = time.ToString();
            if (timerAlive)
            {
                StartTimer();
            }

            gameListView.ItemsSource = usedCities;
        }

        void StartTimer()
        {
            time = 60;
            timerButton.Text = time.ToString();

            Device.StartTimer(new TimeSpan(0, 0, 0, 0, 500), () =>
            {
                time -= 0.5;

                if (!time.ToString().Contains(","))
                {
                    timerButton.Text = time.ToString();
                }

                if (time <= 0)
                {
                    DisplayAlert("Вы проиграли", "Время на ответ исчерпано ¯\\_(ツ)_/¯", "ok");
                    SelectPlayerPage.hubConnection.SendAsync("SendCloseLobby", player2Left.ConnectionId);
                    Navigation.PopAsync(true);
                    return false;
                }

                return timerAlive;
            });
        }

        protected override void OnAppearing()
        {
            App.Current.On<Xamarin.Forms.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            App.Current.On<Xamarin.Forms.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Pan);
            base.OnDisappearing();
        }

        void RemoveUser(Chat city)
        {
            var remCity = allСities.Where(x => x.CityName.ToLower() == city.CityName.ToLower());
            allСities.RemoveAt(1);
        }

        async void HubHandler()
        {
            try
            {
                SelectPlayerPage.hubConnection.On<List<Chat>>("ReceiveCities", (cities) =>
                {
                    foreach (var item in cities)
                    {
                        this.allСities.Add(new Chat() { CityName = item.CityName, FlagId = $"a{item.FlagId}.png" });
                    }
                });

                SelectPlayerPage.hubConnection.On<Chat>("ReceiveMessageFromUser", (city) =>
                {
                    //var dd = new Chat() { CityName = city.CityName, FlagId = city.FlagId };
                    RemoveUser(new Chat() { CityName = city.CityName, FlagId = city.FlagId });
                    //var exist = allСities.Contains(city);

                    //allСities.Remove(city);

                    usedCities.Add(new Chat() { CityName = city.CityName, FlagId = city.FlagId, Status = "received" });
                    timerAlive = true;
                    StartTimer();
                    ScrollToLastElement();

                    accesToWrite = true;
                });

            }
            catch (Exception ex)
            {
                await DisplayAlert($"Error", $"Проверьте подключение к интернету\n{ex.Message}", "OK");
            }
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
                        allСities.Add(new Chat() { CityName = nameCityElement.Value, FlagId = $"a{idElement.Value}.png" });
                    }
                }
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

        private async void EntryText_Completed(object sender, EventArgs e)
        {
            if (accesToWrite)
            {
                try
                {
                    try
                    {
                        var playerCity = new Chat();

                        if (usedCities.Count > 0)
                        {
                            var lastCity = usedCities.Last();
                            lastCity.CityName = lastCity.CityName.Trim('ь', 'ы', 'ё', 'ъ', ')');

                            playerCity = allСities
                                .Where(x => x.CityName.ToLower().StartsWith(lastCity.CityName.Last().ToString()))
                                .Where(x => x.CityName.ToLower() == entryText.Text.ToLower().Trim())
                                .First();
                        }
                        else
                        {
                            playerCity = allСities
                                .Where(x => x.CityName.ToLower() == entryText.Text.ToLower().Trim())
                                .Select(x => x).First();
                        }
                        allСities.Remove(playerCity);
                        accesToWrite = false;
                        timerAlive = false;
                        usedCities.Add(new Chat() { CityName = playerCity.CityName, FlagId = playerCity.FlagId, Status = "sent" });

                        ScrollToLastElement();

                        await SelectPlayerPage.hubConnection.SendAsync("SendMessageToUser", player2Left.ConnectionId,
                            new GameCity() { CityName = playerCity.CityName, FlagId = playerCity.FlagId });
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

        private async void Hint_Clicked(object sender, EventArgs e)
        {
            if (accesToWrite)
            {
                if (numberOfHints == 0)
                {
                    await DisplayAlert("У вас закончились подсказки",
                        "Узнать новые города вы можете в нашей базе",
                        "ОК");
                }
                else if (await DisplayAlert("Вы действительно хотите использовать подсказку?", $"У вас их {numberOfHints}", "ОК", "ОТМЕНА") && numberOfHints > 0)
                {
                    try
                    {
                        var relevantCity = new Chat();

                        if (usedCities.Count > 0)
                        {
                            var relevantCities = allСities.Where(i => i.CityName.ToLower()
                                           .StartsWith(usedCities.Last().CityName.Trim('ь', 'ы', 'ё', 'ъ', ')')
                                           .Last().ToString().ToLower()));

                            relevantCity = relevantCities.ToList()[new Random().Next(0, relevantCities.Count())];
                        }
                        else
                        {
                            relevantCity = allСities[new Random().Next(0, allСities.Count())];
                        }
                        timerAlive = false;
                        allСities.Remove(relevantCity);
                        numberOfHints--;
                        hintButton.Text = $"Подсказка ({numberOfHints})";
                        usedCities.Add(new Chat() { CityName = relevantCity.CityName, FlagId = relevantCity.FlagId, Status = "sent" });
                        ScrollToLastElement();

                        await SelectPlayerPage.hubConnection.SendAsync("SendMessageToUser", player2Left.ConnectionId,
                            new GameCity() { CityName = relevantCity.CityName, FlagId = relevantCity.FlagId });
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert(ex.Message, "error", "error");
                    }
                    accesToWrite = false;
                    entryText.Text = "";
                }
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await this.DisplayAlert("ВНИМАНИЕ", $"Завершить игру и выйти в лобби?", "Да", "Нет");
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
                var result = await this.DisplayAlert("ВНИМАНИЕ", "Завершить игру и выйти в лобби?", "Да", "Нет");
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

        async void ScrollToLastElement()
        {
            gameListView.ScrollTo(usedCities.Last(), ScrollToPosition.Start, true);

            await Task.Delay(150);

            gameListView.ScrollTo(usedCities.Last(), ScrollToPosition.Start, true);
        }

        private void entryText_Focused(object sender, FocusEventArgs e)
        {
            if (usedCities.Count != 0)
            {
                ScrollToLastElement();
            }
        }
    }
}