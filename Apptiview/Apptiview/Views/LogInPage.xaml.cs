using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Apptiview.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LogInPage : ContentPage
	{
		public LogInPage ()
		{
			InitializeComponent ();
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