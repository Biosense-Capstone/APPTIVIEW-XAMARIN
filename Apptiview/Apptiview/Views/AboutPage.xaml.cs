using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Apptiview.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        void OpenEEF(object sender, EventArgs e)
        {
            Uri uri = new Uri("http://eef.colorado.edu/");
            Device.OpenUri(uri);
        }
    }
}