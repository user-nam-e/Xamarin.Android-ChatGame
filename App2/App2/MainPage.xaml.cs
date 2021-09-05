﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.IO;
using Xamarin.Essentials;

namespace App2
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            AnimateBackGround();
        }

        private async void AnimateBackGround()
        {
            if (Preferences.Get("animationGradientSwitch", false))
            {
                await Task.Delay(3000);
                Action<double> forward = input => bdGradient.AnchorY = input;
                Action<double> backward = input => bdGradient.AnchorY = input;
                while (Preferences.Get("animationGradientSwitch", true))
                {
                    bdGradient.Animate(name: "forward", callback: forward, start: 0, end: 1, length: 5000, easing: Easing.SinIn);
                    await Task.Delay(5000);
                    bdGradient.Animate(name: "backward", callback: backward, start: 1, end: 0, length: 5000, easing: Easing.SinIn);
                    await Task.Delay(5000);
                }
            }
        }
        private async void ButtonPlay_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GamePage(), false);
        }
        private async void ButtonCities_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CitiesPage(), false);
        }
        private async void ButtonSetting_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage(), false);
        }
    }
}
