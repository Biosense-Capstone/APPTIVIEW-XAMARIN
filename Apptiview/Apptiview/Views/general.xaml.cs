using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Apptiview.Models;
using Apptiview.Views;
using Apptiview.ViewModels;

namespace Apptiview.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class general : ContentPage
    {

        public general()
        {
            InitializeComponent();
        }

        //int count = 0;
        //void Button_Clicked(object sender, System.EventArgs e)
        //{
        //    count++;
        //    ((Button)sender).Text = $"You clicked {count} times.";
        //    if (count == 5)
        //    {
        //        btn.IsEnabled = false;
        //        this.DisplayAlert("No Access", "Need to buy more btnCoins to access more presses", "Ok");
        //    }
        //}
    }
}