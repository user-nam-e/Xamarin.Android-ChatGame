using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App2.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditPlayerPage : ContentPage
    {
        public EditPlayerPage()
        {
            InitializeComponent();

            EntryName.Text = Preferences.Get("playerName", "");
        }

        private async void EntryName_Completed(object sender, EventArgs e)
        {
            Preferences.Set("playerName", EntryName.Text);
            await Navigation.PopAsync();
        }
    }
}