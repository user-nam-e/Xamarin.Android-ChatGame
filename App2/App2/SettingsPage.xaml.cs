using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Android.Renderscripts.Element;

namespace App2
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            animationGradientSwitch.On = Preferences.Get("animationGradientSwitch", true);
        }
        private async void animationGradientSwitch_OnChanged(object sender, ToggledEventArgs e)
        {
            Preferences.Set("animationGradientSwitch", animationGradientSwitch.On);
        }
    }
}