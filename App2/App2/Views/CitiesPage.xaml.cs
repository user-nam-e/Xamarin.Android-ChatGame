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
        ObservableCollection<City> cities = new ObservableCollection<City>();
        List<City> suggestions = new List<City>();

        public CitiesPage()
        {
            InitializeComponent();

            CheckCityCount();
            if (Preferences.Get("localDataSwitch", true))
            {
                XmlLoadAsync();
            }
            else
            {
                labelLoad.IsVisible = true;
                imageGif.IsVisible = true;
                AzureLoadAsync();
                LoadGif();
            }
        }

        async void CheckCityCount()
        {
            while (true)
            {

                if (suggestions.Count != 0)
                {
                    countOfCities.Text = suggestions.Count.ToString();
                }
                else if (suggestions.Count == 0 && !string.IsNullOrEmpty(CitiesSearchBar.Text))
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
            await Task.Delay(1500);

            labelLoad.IsVisible = false;
            imageGif.IsVisible = false;
            citiesListView.ItemsSource = cities;
        }

        async void AzureLoadAsync()
        {
            hubConnection = new HubConnectionBuilder().WithUrl("https://citieschatapi.azurewebsites.net/chat").Build();

            hubConnection.On<List<City>>("ReceiveCities", (cities) =>
            {
                foreach (var item in cities)
                {
                    this.cities.Add(new City() { CityName = $"{item.CityName} ({item.CountryName})", FlagId = $"a{item.FlagId}.png" });
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

        public async void XmlLoadAsync()
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
                        cities.Add(new City() { CityName = $"{nameCityElement.Value} ({countryNameElement.Value})", FlagId = $"a{idElement.Value}.png" });
                    }
                }

                citiesListView.ItemsSource = cities;
            }
        }

        private void CitiesSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = CitiesSearchBar.Text;
            suggestions = cities.Where(c => c.CityName.ToLower().Contains(keyword.ToLower())).ToList();
            citiesListView.ItemsSource = suggestions;
        }
    }
}