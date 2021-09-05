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

namespace App2
{
    public partial class CitiesPage : ContentPage
    {
        public CitiesPage()
        {
            InitializeComponent();
            XmlLoad();
        }
        List<City> cities = new List<City>();
        public async void XmlLoad()
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
                        cities.Add(new City() { NameCity = $"{nameCityElement.Value} ({countryNameElement.Value})", Id = $"a{idElement.Value}.png" });
                    }
                }
                citiesListView.ItemsSource = cities;
            }
        }
        private void CitiesSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var keyword = CitiesSearchBar.Text;
            var suggestions = cities.Where(c => c.NameCity.ToLower().Contains(keyword.ToLower()));
            citiesListView.ItemsSource = suggestions;
        }
    }
}