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
            bool logInCheck = true;
            if (logInCheck)
            {
                //await Application.Current.MainPage.Navigation.PopAsync();
                await Navigation.PopModalAsync();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}