using App2.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace App2.Views
{
    public partial class GamePage : ContentPage
    {
        HubConnection hubConnection;
        ObservableCollection<Chat> allСities = new ObservableCollection<Chat>();
        ObservableCollection<Chat> usedCities = new ObservableCollection<Chat>();
        int numberOfHints = 333;
        bool accesToWrite = false;
        double time = Preferences.Get("timerValue", 60);
        bool timerAlive;

        public GamePage()
        {
            InitializeComponent();
            LoadData();
            hintButton.Text = $"Подсказка ({numberOfHints})";


            if (timerAlive = time != -1)
            {
                timerButton.Text = time.ToString();
                StartTimer();
            }
        }

        private void StartTimer()
        {
            time = Preferences.Get("timerValue", 60);
            timerButton.Text = time.ToString();

            Device.StartTimer(new TimeSpan(0, 0, 0, 0, 500), () =>
                {
                    time -= 0.5;

                    if (!time.ToString().Contains(",") && !time.ToString().Contains("."))
                    {
                        timerButton.Text = time.ToString();
                    }

                    if (time <= 0)
                    {
                        DisplayAlert("Вы проиграли", "Время на ответ исчерпано ¯\\_(ツ)_/¯", "ok");
                        Navigation.PopAsync(true);
                        return false;
                    }

                    return timerAlive;
                });
        }

        async void LoadGif()
        {
            while (allСities.Count == 0)
            {
                await Task.Delay(500);
            }

            imageGif.IsVisible = false;
            labelLoad.IsVisible = false;
            accesToWrite = true;
        }

        async void LoadData()
        {
            if (Preferences.Get("localDataSwitch", true))
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
            else
            {
                imageGif.IsVisible = true;
                labelLoad.IsVisible = true;
                hubConnection = new HubConnectionBuilder().WithUrl("https://citieschatapi.azurewebsites.net/chat").Build();

                hubConnection.On<List<Chat>>("ReceiveCities", (cities) =>
                {
                    foreach (var item in cities)
                    {
                        this.allСities.Add(new Chat() { CityName = item.CityName, FlagId = $"a{item.FlagId}.png" });
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

            LoadGif();

            gameListView.ItemsSource = usedCities;
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

        void EntryText_Completed(object sender, EventArgs e)
        {
            entryText.Focus();
            var playerCity = new Chat();
            try
            {
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
                timerAlive = false;

                allСities.Remove(playerCity);
                usedCities.Add(new Chat(playerCity) { Status = "sent" });
                GameBot();
                //////////
            }
            catch (Exception ex)
            {
                //entryText.Text = string.Empty;
                //DisplayAlert(ex.Message, "error", "error");
            }

            entryText.Text = string.Empty;
        }

        async void GameBot()
        {
            try
            {
                await Task.Delay(new Random().Next(500, 600));

                //await Task.Delay(1000);

                var botCities = allСities.Where(i => i.CityName.ToLower().StartsWith(usedCities.Last().CityName.Trim('ь', 'ы', 'ё', 'ъ', ')').Last().ToString().ToLower()));

                var botCity = botCities.ToList()[new Random().Next(0, botCities.Count())];

                allСities.Remove(botCity);
                usedCities.Add(new Chat(botCity) { Status = "received" });
                accesToWrite = true;
                timerAlive = true;
                if (timerAlive = time != -1)
                {
                    timerButton.Text = time.ToString();
                    StartTimer();
                }
                ScrollToLastElement();
                //StartTimer();
            }
            catch (Exception)
            {
                await DisplayAlert("Ошибка", "Попробуйте поменять используемую базу городов", "ок");
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
                    accesToWrite = false;
                    try
                    {
                        var relevantCity = new Chat();
                        timerAlive = false;

                        if (usedCities.Count > 0)
                        {
                            var relevantCities = allСities.Where(i => i.CityName.ToLower().StartsWith(usedCities.Last().CityName.Trim('ь', 'ы', 'ё', 'ъ', ')')
                                .Last().ToString().ToLower()));

                            relevantCity = relevantCities.ToList()[new Random().Next(0, relevantCities.Count())];
                        }
                        else
                        {
                            relevantCity = allСities[new Random().Next(0, allСities.Count())];
                        }
                        numberOfHints--;
                        hintButton.Text = $"Подсказка ({numberOfHints})";

                        allСities.Remove(relevantCity);
                        usedCities.Add(new Chat() { CityName = relevantCity.CityName, FlagId = relevantCity.FlagId, Status = "sent" });
                        gameListView.ScrollTo(usedCities.Last(), ScrollToPosition.Start, true);

                        GameBot();
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert(ex.Message, "error", "error");
                    }
                }
                entryText.Text = "";
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await this.DisplayAlert("ВНИМАНИЕ", "Завершить игру и выйти в главное меню?", "Да", "Нет");

                if (result)
                {
                    timerAlive = false;
                    await this.Navigation.PushAsync(new MainPage(), false);
                }
            });

            return true;
        }

        private void CloseGamePage(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await this.DisplayAlert("ВНИМАНИЕ", "Завершить игру и выйти в главное меню?", "Да", "Нет");
                if (result)
                {
                    timerAlive = false;
                    await this.Navigation.PushAsync(new MainPage(), false);
                }
            });
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