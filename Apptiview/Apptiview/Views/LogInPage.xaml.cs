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
        public static List<Account> accounts = new List<Account>();

        public class Account
        {
            //public string Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public LogInPage()
        {
            InitializeComponent();

            Account defaultAccount = new Account();
            defaultAccount.Username = null;
            defaultAccount.Password = null;
            accounts.Add(defaultAccount);
            //AccountInfo item = new AccountInfo { Username = "Andrew" };
            //MobileService.GetTable<AccountInfo>().InsertAsync(item);
        }

        async void LogInCheck(object sender, System.EventArgs e)
        {
            for (int i=0; i<accounts.Count; i++)
            {
                if (user.Text == accounts.ElementAt(i).Username && pass.Text == accounts.ElementAt(i).Password)
                {
                    await Navigation.PopModalAsync();
                    return;
                }
            }
            await DisplayAlert("Not a valid login", "The login combo either doesn't exist or has not been created", "Ok");
            return;

            //if ((user.Text == null && pass.Text == null) || (user.Text == "anpa2782@colorado.edu" && pass.Text == "password"))
            //{
            //    await Navigation.PopModalAsync();
            //} else
            //{
            //    await DisplayAlert("Not a valid login", "The login combo either doesn't exist or has not been created", "Ok");
            //}
        }

        async void createAccount(object sender, System.EventArgs e)
        {
            Account newAccount = new Account();
            if (user.Text == null || pass.Text == null || user.Text == "" || pass.Text == "" || pass.Text.Length < 6)
            {
                await DisplayAlert("Not valid username/password", "Please input a username and password to create account. Password must be at least 6 characters long.", "Ok");
                return;
            } else
            {
                newAccount.Username = user.Text;
                newAccount.Password = pass.Text;
                accounts.Add(newAccount);
                await DisplayAlert("Success", "Account created!", "Ok");
                await Navigation.PopModalAsync();
                return;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}