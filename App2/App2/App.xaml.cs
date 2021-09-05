using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace App2
{
    public class City
    {
        public string NameCity { get; set; }
        public string NameCountry { get; set; }
        public string Id { get; set; }
        public string BotNameCity { get; set; }
        public string BotId { get; set; }
    }
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage())
            {
                BarBackgroundColor = Color.FromHex("#6adea0"),
                BarTextColor = Color.White
            };
        }

        protected override void OnStart() { }

        protected override void OnSleep() { }

        protected override void OnResume() { }
    }
}
