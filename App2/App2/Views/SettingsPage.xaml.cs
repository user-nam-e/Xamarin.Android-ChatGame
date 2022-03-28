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

        private async void EditPlayer_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EditPlayerPage(), false);
        }

        protected override void OnAppearing()
        {
            animationGradientSwitch.IsToggled = Preferences.Get("animationGradientSwitch", true);
            localDataSwitch.IsToggled = Preferences.Get("localDataSwitch", true);

            animationStateLabel.Text = Preferences.Get("animationGradientSwitch", true) ? "Вкл" : "Выкл";
            dataStateLabel.Text = Preferences.Get("localDataSwitch", true) ? "Да" : "Нет";

            currentNameLabel.Text = Preferences.Get("playerName", "");

            var timer = Preferences.Get("timerValue", 60);
            {
                if (timer == 60)
                {
                    currentTimer.Text = "1 мин";
                }
                else if (timer == 120)
                {
                    currentTimer.Text = "2 мин";
                }
                else if (timer == 300)
                {
                    currentTimer.Text = "5 мин";
                }
                else if (timer == -1)
                {
                    currentTimer.Text = "Выкл";
                }
            }

            base.OnAppearing();
        }

        private void AnimationGradientSwitch_OnChanged(object sender, ToggledEventArgs e)
        {
            Preferences.Set("animationGradientSwitch", animationGradientSwitch.IsToggled);

            animationStateLabel.Text = Preferences.Get("animationGradientSwitch", true) ? "Вкл" : "Выкл";
        }

        private void LocalDataSwitch_OnChanged(object sender, ToggledEventArgs e)
        {
            Preferences.Set("localDataSwitch", localDataSwitch.IsToggled);

            dataStateLabel.Text = Preferences.Get("localDataSwitch", true) ? "Да" : "Нет";
        }

        private async void EditTimer_Tapped(object sender, EventArgs e)
        {
            var action = await DisplayActionSheet("Таймер хода", "Отмена", "", "Выкл", "1 мин", "2 мин", "5 мин");

            if (action != null && action != "Отмена")
            {
                if (action == "Выкл")
                {
                    Preferences.Set("timerValue", -1);
                }
                else if (action == "1 мин")
                {
                    Preferences.Set("timerValue", 60);
                }
                else if (action == "2 мин")
                {
                    Preferences.Set("timerValue", 120);
                }
                else if (action == "5 мин")
                {
                    Preferences.Set("timerValue", 300);
                }

                currentTimer.Text = action;
            }
        }
    }
}