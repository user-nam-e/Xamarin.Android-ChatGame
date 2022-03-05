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

        public CitiesPage()
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
                citiesListView.ItemsSource = cities;
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
            var suggestions = cities.Where(c => c.CityName.ToLower().Contains(keyword.ToLower()));
            citiesListView.ItemsSource = suggestions;
        }
    }
}