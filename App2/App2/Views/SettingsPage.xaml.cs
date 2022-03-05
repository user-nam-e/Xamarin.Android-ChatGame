using App2.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            animationGradientSwitch.IsToggled = Preferences.Get("animationGradientSwitch", true);

            animationTurnOff.Text = Preferences.Get("animationGradientSwitch", true) ? "Вкл" : "Выкл";
        }
        //    localDataSwitch.On = Preferences.Get("localDataSwitch", true);
        //}

        private void AnimationGradientSwitch_OnChanged(object sender, ToggledEventArgs e)
        {
            Preferences.Set("animationGradientSwitch", animationGradientSwitch.IsToggled);

            animationTurnOff.Text = Preferences.Get("animationGradientSwitch", true) ? "Вкл" : "Выкл";
        }

        //private void LocalDataSwitch_OnChanged(object sender, ToggledEventArgs e)
        //{
        //    Preferences.Set("localDataSwitch", localDataSwitch.On);
        //}

        //private async void editPlayer_Clicked(object sender, EventArgs e)
        //{
        //    await Navigation.PushAsync(new EditPlayerPage(), false);
        //}
    }
}