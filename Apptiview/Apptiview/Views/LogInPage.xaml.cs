using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Microsoft.WindowsAzure.MobileServices;

namespace Apptiview.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LogInPage : ContentPage
	{
        public static MobileServiceClient MobileService = new MobileServiceClient("https://apptiviewdatabaseapp.azurewebsites.net");

        public class AccountInfo
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public LogInPage ()
		{
			InitializeComponent ();

            //AccountInfo item = new AccountInfo { Username = "Andrew" };
            //MobileService.GetTable<AccountInfo>().InsertAsync(item);
        }

        async void LogInCheck(object sender, System.EventArgs e)
        {
            if ((user.Text == null && pass.Text == null) || (user.Text == "anpa2782@colorado.edu" && pass.Text == "password"))
            {
                await Navigation.PopModalAsync();
            } else
            {
                await DisplayAlert("Not a valid login", "The login combo either doesn't exist or has not been created", "Ok");
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}