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
        ObservableCollection<City> usedСities = new ObservableCollection<City>();
        List<City> allСities = new List<City>();
        Random rnd = new Random();
        City playerCity = new City();
        int numberOfHints = 3;

        public GamePage()
        {
            InitializeComponent();
            LoadXml();

            GameListView.ItemsSource = usedСities;
        }
        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () => {
                var result = await this.DisplayAlert("ВНИМАНИЕ", "Завершить игру и выйти в главное меню?", "Да", "Нет");
                if (result) await this.Navigation.PopAsync(); // or anything else
            });

            return true;
        }
        public async void LoadXml()
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
                        allСities.Add(new City() { NameCity = $"{nameCityElement.Value}", Id = $"a{idElement.Value}.png" }); ;
                    }
                }
            }
        }

        private void EntryText_Completed(object sender, EventArgs e)
        {
            var botCity = new City();
            try
            {
                if (usedСities.Count > 0)
                {
                    City lastCity = usedСities.Last();
                    lastCity.BotNameCity = lastCity.BotNameCity.Trim('ь', 'ы');

                    playerCity = allСities
                        .Where(x => x.NameCity.ToLower().StartsWith(lastCity.BotNameCity.Last().ToString()))
                        .Where(x => x.NameCity.ToLower() == entryText.Text.ToLower().Trim())
                        .First();
                }
                else
                {
                    playerCity = allСities
                        .Where(x => x.NameCity.ToLower() == entryText.Text.ToLower().Trim())
                        .Select(x => x).First();
                }
                GameBot(playerCity);
                usedСities.Add(playerCity);
                allСities.Remove(playerCity);
                GameListView.ScrollTo(usedСities.Last(), ScrollToPosition.End, true);
            }
            catch (Exception)
            {
                //DisplayAlert(ex.Message, "error", "error");
            }
            entryText.Text = " ";
        }

        private void GameBot(City obj)
        {
            var selectedCities = from t in allСities
                                 where t.NameCity.ToLower().StartsWith(obj.NameCity.Trim('ь', 'ы').Last().ToString().ToLower())
                                 select t;

            var selectedCity = selectedCities.ToList()[rnd.Next(0, selectedCities.Count())];

            allСities.Remove(selectedCity);
            playerCity.BotNameCity = selectedCity.NameCity;
            playerCity.BotId = selectedCity.Id;
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
                        lastCity.BotNameCity = lastCity.BotNameCity.Trim('ь', 'ы');

                        playerCity = allСities
                            .Where(x => x.NameCity.ToLower().StartsWith(lastCity.BotNameCity.Last().ToString()))
                            .First();
                    }
                    else
                    {
                        playerCity = allСities[rnd.Next(0, allСities.Count())];
                    }
                    numberOfHints--;
                    GameBot(playerCity);
                    usedСities.Add(playerCity);
                    allСities.Remove(playerCity);
                    GameListView.ScrollTo(usedСities.Last(), ScrollToPosition.End, true);

                }
                catch (Exception ex)
                {
                    //await DisplayAlert(ex.Message, "error", "error");
                }
                entryText.Text = "";
            }
        }

        private void test_Clicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => {
                var result = await this.DisplayAlert("ВНИМАНИЕ", "Завершить игру и выйти в главное меню?", "Да", "Нет");
                if (result) await this.Navigation.PopAsync(); // or anything else
            });
        }
    }
}