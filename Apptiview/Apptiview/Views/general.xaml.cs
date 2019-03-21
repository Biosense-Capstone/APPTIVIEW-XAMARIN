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

using Plugin.BLE;

namespace Apptiview.Views
{
    class Guids
    {
        public static readonly Guids _guids = new Guids();

        public Guid HeadbandGuid { get; set; }
        public Guid HeadsetGuid { get; set; }
        public bool HBConnected { get; set; } = false;
        public bool HSConnected { get; set; } = false;
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class general : ContentPage
    {
        public int packetLossCount = 0;

        public general()
        {
            InitializeComponent();

            FourierTransformer ft = new FourierTransformer(100, FourierSign.Negative, FourierNormalization.Unitary);
            List<Complex> sin = new List<Complex>();
            //Complex[] sin = new Complex[25];
            //int packetCount = 0;

            //Complex[] tri = new Complex[] { 0, 0, 0, 1000, 0, 0, 0, 0 };
            //Complex[] xt = ft.Transform(tri);

            List<Entry> entries = new List<Entry>();
            List<Entry> fourierTransform = new List<Entry>();
            bool firstpass = false;
            CrossBluetoothLE.Current.Adapter.DeviceAdvertised += (s, e) =>
            {
                // Check if we are connected to our devices and workout is running
                if ((Guids._guids.HBConnected == true || Guids._guids.HSConnected == true) && finishBtn.IsEnabled)
                {
                    // Check if device is headband, headset, or not one of our devices
                    if (Guids._guids.HeadbandGuid == e.Device.Id)
                    {
                        // Grab advertisements and process
                        Plugin.BLE.Abstractions.AdvertisementRecord[] advertisement = e.Device.AdvertisementRecords.ToArray();
                        var temp = advertisement[3].Data[0];
                        //sin[packetCount]
                        entries.Add(new Entry(temp) { Color = SKColor.FromHsl(1, 100, 100, 255) });

                        //System.Diagnostics.Debug.WriteLine(temp);
                        // Add value to our fourier list
                        sin.Add(temp);

                        var chart = new LineChart()
                        {
                            Entries = entries,
                            LabelTextSize = 100,
                            LineSize = 10,
                            LineMode = LineMode.Spline,
                            PointSize = 20,
                            BackgroundColor = SKColor.Parse("#393E46")
                        };
                        this.chartView.Chart = chart;

                        // Entries has reached max size
                        if (entries.Count > 100)
                        {
                            // Remove oldest values
                            sin.RemoveAt(0);
                            entries.RemoveAt(0);

                            // Perform fourier
                            Complex[] xt = ft.Transform(sin);
                            for (int i = 0; i < 100; i++)
                            {
                                if (firstpass == true)
                                {
                                    fourierTransform.RemoveAt(0);
                                }
                                fourierTransform.Add(new Entry((float)Meta.Numerics.ComplexMath.Abs(xt[i])) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            }
                            firstpass = true;
                            var fChart = new LineChart()
                            {
                                Entries = fourierTransform,
                                LabelTextSize = 100,
                                LineSize = 10,
                                LineMode = LineMode.Spline,
                                PointSize = 20,
                                BackgroundColor = SKColor.Parse("#393E46")
                            };
                            this.fourierChart.Chart = fChart;
                        }
                    }
                    else if (Guids._guids.HeadsetGuid == e.Device.Id)
                    {
                        //AdReceived(e.Device.Id);
                    }
                }
            };

            //Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            //{

            //});

            BindingContext = this;
        }

        void Start_Button_Clicked(object sender, System.EventArgs e)
        {
            if (Guids._guids.HBConnected == true || Guids._guids.HSConnected == true)
            {
                ((Button)sender).IsEnabled = false;
                finishBtn.IsEnabled = true;
                
            }
            else
            {
                DisplayAlert("Warning", "Head-set and/or headband should be connected (done through settings page) before starting workout.", "OK");
            }
            
        }

        void Finish_Button_Clicked(object sender, System.EventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            startBtn.IsEnabled = true;
        }
    }
}