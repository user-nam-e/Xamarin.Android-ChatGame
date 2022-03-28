using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Reflection;
using System.Xml.Serialization;
using Xamarin.Essentials;
using System.Threading;
using System.Xml.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using App2.Models;
using System.Collections.ObjectModel;

namespace App2
{
    public partial class CitiesPage : ContentPage
    {
        HubConnection hubConnection;
        ObservableCollection<Chat> cities = new ObservableCollection<Chat>();
        List<Chat> suggestions = new List<Chat>();

        public CitiesPage()
        {
            InitializeComponent();
            CheckCityCount();
            LoadData();
        }

        async void CheckCityCount()
        {
            while (true)
            {

                if (suggestions.Count != 0)
                {
                    countOfCities.Text = suggestions.Count.ToString();
                }
                else if (suggestions.Count == 0 && !string.IsNullOrEmpty(citiesSearchBar.Text))
                {
                    countOfCities.Text = "0";
                }
                else
                {
                    countOfCities.Text = cities.Count.ToString();
                }

                await Task.Delay(500);
            }
        }

        async void LoadGif()
        {
            while (cities.Count == 0)
            {
                await Task.Delay(500);
            }

            imageGif.IsVisible = false;
            labelLoad.IsVisible = false;
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
                            cities.Add(new Chat() { CityName = $"{nameCityElement.Value} ({countryNameElement.Value})", FlagId = $"a{idElement.Value}.png" });
                        }
                    }

                    //citiesListView.ItemsSource = cities;
                }
            }
            else
            {
                labelLoad.IsVisible = true;
                imageGif.IsVisible = true;
                LoadGif();

                hubConnection = new HubConnectionBuilder().WithUrl("https://citieschatapi.azurewebsites.net/chat").Build();

                hubConnection.On<List<Chat>>("ReceiveCities", (cities) =>
                {
                    foreach (var item in cities)
                    {
                        this.cities.Add(new Chat() { CityName = $"{item.CityName} ({item.CountryName})", FlagId = $"a{item.FlagId}.png" });
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

            citiesListView.ItemsSource = cities;
        }

        private void CitiesSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (citiesSearchBar.Text.StartsWith("*"))
            {
                var letter = citiesSearchBar.Text.Substring(1);
                suggestions = cities.Where(c => c.CityName.ToLower().StartsWith(letter.ToLower())).ToList();
            }
            else if (citiesSearchBar.Text.StartsWith("!") && citiesSearchBar.Text.Length > 3)
            {
                var letter = citiesSearchBar.Text.Substring(1, 1);
                var country = citiesSearchBar.Text.Substring(3);
                suggestions = cities.Where(c => c.CityName.ToLower().Contains(country.ToLower()))
                    .Where(c => c.CityName.ToLower().StartsWith(letter.ToLower())).ToList();
                //suggestions = cities.Where(c => c.CityName.ToLower().Contains("(Россия)").StartsWith(keyword.ToLower())).ToList();
            }
            else
            {
                var keyword = citiesSearchBar.Text;
                suggestions = cities.Where(c => c.CityName.ToLower().Contains(keyword.ToLower())).ToList();
            }

            citiesListView.ItemsSource = suggestions;
        }

        private void helpButton_Clicked(object sender, EventArgs e)
        {
            DisplayAlert("Поиск", "*А - поиск городов, начинающихся на букву 'А' \n\n!Г Беларусь -  поиск городов, начинающихся на букву 'Г' и расположенных в Беларуси", "OK");
        }

        private void updateButton_Clicked(object sender, EventArgs e)
        {
            cities.Clear();

            LoadData();
        }
    }
}