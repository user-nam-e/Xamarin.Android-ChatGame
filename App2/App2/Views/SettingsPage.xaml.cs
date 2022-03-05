using App2.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
        }

        protected override void OnAppearing()
        {
            //animationGradientSwitch.IsToggled = Preferences.Get("animationGradientSwitch", true);
            //localDataSwitch.IsToggled = Preferences.Get("localDataSwitch", true);

            //animationTurnOff.Text = Preferences.Get("animationGradientSwitch", true) ? "Вкл" : "Выкл";
            //localTurnOff.Text = Preferences.Get("localDataSwitch", true) ? "Да" : "Нет";

            currentName.Text = Preferences.Get("playerName", "");
            base.OnAppearing();
        }
        
        private async void EditPlayer_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditPlayerPage(), false);
        }

        //private void AnimationGradientSwitch_OnChanged(object sender, ToggledEventArgs e)
        //{
        //    Preferences.Set("animationGradientSwitch", animationGradientSwitch.IsToggled);

        //    animationTurnOff.Text = Preferences.Get("animationGradientSwitch", true) ? "Вкл" : "Выкл";
        //}

        //private void LocalDataSwitch_OnChanged(object sender, ToggledEventArgs e)
        //{
        //    Preferences.Set("localDataSwitch", localDataSwitch.IsToggled);

        //    localTurnOff.Text = Preferences.Get("localDataSwitch", true) ? "Да" : "Нет";
        //}
    }
}