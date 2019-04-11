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

            // Scale chart to overlay the fourier graph
            List<Entry> scaleEntries = new List<Entry>();
            for (int j = 0; j < 125; j++)
            {
                if (j % 25 == 0 && j > 0)
                {
                   scaleEntries.Add(new Entry((int)(j / 2.5)) { ValueLabel = ((int)(j / 2.5)).ToString(), Color = SKColor.FromHsl(1, 100, 100, 255) });
                }
                else
                {
                    scaleEntries.Add(new Entry((int)(j / 2.5)));
                }
            }
            var scaleChart = new LineChart()
            {
                Entries = scaleEntries,
                LineMode = LineMode.None,
                PointMode = PointMode.None,
                LabelTextSize = 50,
                PointSize = 0,
                BackgroundColor = SKColor.FromHsl(243, 100, 50, 0)
            };
            this.fourierScaleChart.Chart = scaleChart;

            FourierTransformer ft = new FourierTransformer(250);
            List<Complex> headBandInput = new List<Complex>();
            //double Fs = 100;

            // Headband entries
            List<Entry> entries = new List<Entry>();
            List<Entry> chartEntries = new List<Entry>();
            // The list that will contain all the fourier data
            List<Entry> fourierTransform = new List<Entry>();

            // Headset entries
            List<Entry> temperatureEntries = new List<Entry>();

            // First pass to properly remove the correct value of fourier
            bool firstpass = false;

            int packetCount = 0;
            int temperatureCount = 0;

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

                        // Remove at 0 position for our chart
                        if (chartEntries.Count > 100)
                        {
                            chartEntries.RemoveAt(0);
                        }

                        if (packetCount == 10)
                        {
                            entries.Add(new Entry(temp) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = temp.ToString() });
                            chartEntries.Add(new Entry(temp) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = temp.ToString() });
                            packetCount = 0;
                        } else
                        {
                            entries.Add(new Entry(temp) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            chartEntries.Add(new Entry(temp) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            packetCount++;
                        }
                        //entries.Add(new Entry(temp) { Color = SKColor.FromHsl(1, 100, 100, 255) });

                        // Add value to our fourier list
                        headBandInput.Add(temp);

                        var chart = new LineChart()
                        {
                            Entries = chartEntries,
                            LabelTextSize = 100,
                            LineSize = 10,
                            LineMode = LineMode.Straight,
                            PointSize = 20,
                            BackgroundColor = SKColor.Parse("#393E46")
                        };
                        this.chartView.Chart = chart;

                        // Entries has reached max size
                        if (entries.Count > 250)
                        {
                            // Remove oldest values
                            headBandInput.RemoveAt(0);
                            entries.RemoveAt(0);

                            // Perform fourier
                            Complex[] xt = ft.Transform(headBandInput);
                            for (int i=1; i<125; i++)
                            {
                                xt[i] = Meta.Numerics.ComplexMath.Abs(xt[i] * 2 / 250);
                                // Needed to not stack up fourier graphs
                                if (firstpass == true)
                                {
                                    fourierTransform.RemoveAt(0);
                                }
                                fourierTransform.Add(new Entry((float)xt[i]) { Color = SKColor.FromHsl(1, 100, 100, 255)});
                            }
                            // Makes it so it doesn't remove the first value while it doesn't exsist.
                            firstpass = true;
                            var fChart = new LineChart()
                            {
                                Entries = fourierTransform,
                                LabelTextSize = 100,
                                LineSize = 10,
                                LineMode = LineMode.Straight,
                                PointSize = 20,
                                BackgroundColor = SKColor.Parse("#393E46")
                            };
                            this.fourierChart.Chart = fChart;
                        }

                        // Temperature data
                        if (temperatureEntries.Count > 100)
                        {
                            temperatureEntries.RemoveAt(0);
                        }
                        if (temperatureCount == 10)
                        {
                            temperatureEntries.Add(new Entry(temp / 20 + temperatureCount) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = (temp/20 + temperatureCount).ToString() });
                            temperatureCount = 0;
                        } else
                        {
                            temperatureEntries.Add(new Entry(temp / 20 + temperatureCount) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            temperatureCount++;
                        }
                        var tempChart = new LineChart()
                        {
                            Entries = temperatureEntries,
                            LabelTextSize = 100,
                            LineSize = 10,
                            LineMode = LineMode.Straight,
                            PointSize = 20,
                            BackgroundColor = SKColor.Parse("#393E46")
                        };
                        this.temperatureChart.Chart = tempChart;
                    }
                    else if (Guids._guids.HeadsetGuid == e.Device.Id)
                    {
                        //AdReceived(e.Device.Id);
                    }
                }
            };

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