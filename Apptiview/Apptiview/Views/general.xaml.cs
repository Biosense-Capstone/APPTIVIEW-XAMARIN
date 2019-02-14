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

using Meta.Numerics.SignalProcessing;
using Meta.Numerics;

using SkiaSharp;
using Microcharts;
using Entry = Microcharts.Entry;

namespace Apptiview.Views
{

    class Guids
    {
        public static readonly Guids _guids = new Guids();

        public Guid HeadbandGuid { get; set; }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class general : ContentPage
    {

        public general()
        {
            InitializeComponent();

            //FourierTransformer ft = new FourierTransformer(8);
            //Complex[] tri = new Complex[] { 0, 0, 0, 1000, 0, 0, 0, 0 };
            //Complex[] xt = ft.Transform(tri);

            List<Entry> entries = new List<Entry>();

            Random randNum = new Random();
            //int count = 0;

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                //entries.Add(new Entry(randNum.Next(1, 1000)) { Color = SKColor.FromHsl(count % 360, 100, 50, 255) });
                //count += 10;
                entries.Add(new Entry(randNum.Next(1, 1000)) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                
                var chart = new LineChart()
                {
                    Entries = entries,
                    LabelTextSize = 100,
                    LineSize = 10,
                    LineMode = LineMode.Straight,
                    PointSize = 20,
                    BackgroundColor = SKColor.Parse("#4286f4")
                };
                this.chartView.Chart = chart;

                // Entries has reached max size
                if (entries.Count > 25)
                {
                    // Remove oldest value
                    entries.RemoveAt(0);
                }
                return true; // True = Repeat again, False = Stop the timer
            });

            BindingContext = this;
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