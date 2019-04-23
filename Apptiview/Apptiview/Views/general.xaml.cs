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
            List<Entry> accelXEntries = new List<Entry>();
            List<Entry> accelYEntries = new List<Entry>();
            List<Entry> accelZEntries = new List<Entry>();
            List<Entry> gyXEntries = new List<Entry>();
            List<Entry> gyYEntries = new List<Entry>();
            List<Entry> gyZEntries = new List<Entry>();
            List<Entry> presEntries = new List<Entry>();

            // First pass to properly remove the correct value of fourier
            bool firstpass = false;

            int packetCount = 0;
            int packetHSCount = 0;

            int elect1_0 = 0;
            int elect2_0 = 0;
            int elect1_1 = 0;
            int elect2_1 = 0;
            int elect1_2 = 0;
            int elect2_2 = 0;
            int elect1_3 = 0;
            int elect2_3 = 0;

            CrossBluetoothLE.Current.Adapter.DeviceAdvertised += (s, e) =>
            {
                // Check if we are connected to our devices and workout is running
                if ((Guids._guids.HBConnected == true || Guids._guids.HSConnected == true) && finishBtn.IsEnabled)
                {
                    // Check if device is headband, headset, or not one of our devices
                    if (Guids._guids.HBConnected == true && Guids._guids.HeadbandGuid == e.Device.Id)
                    {
                        // Grab advertisements and process
                        Plugin.BLE.Abstractions.AdvertisementRecord[] advertisement = e.Device.AdvertisementRecords.ToArray();
                        //var temp = advertisement[3].Data[0];

                        if (advertisement.Length == 4)
                        {
                            elect1_0 = advertisement[3].Data[0] | (advertisement[3].Data[1] << 8);
                            elect2_0 = advertisement[3].Data[2] | (advertisement[3].Data[3] << 8);
                            elect1_1 = advertisement[3].Data[4] | (advertisement[3].Data[5] << 8);
                            elect2_1 = advertisement[3].Data[6] | (advertisement[3].Data[7] << 8);
                            elect1_2 = advertisement[3].Data[8] | (advertisement[3].Data[9] << 8);
                            elect2_2 = advertisement[3].Data[10] | (advertisement[3].Data[11] << 8);
                            elect1_3 = advertisement[3].Data[12] | (advertisement[3].Data[13] << 8);
                            elect2_3 = advertisement[3].Data[14] | (advertisement[3].Data[15] << 8);
                        } else
                        {
                            elect1_0 = advertisement[1].Data[0] | (advertisement[1].Data[1] << 8);
                            elect2_0 = advertisement[1].Data[2] | (advertisement[1].Data[3] << 8);
                            elect1_1 = advertisement[1].Data[4] | (advertisement[1].Data[5] << 8);
                            elect2_1 = advertisement[1].Data[6] | (advertisement[1].Data[7] << 8);
                            elect1_2 = advertisement[1].Data[8] | (advertisement[1].Data[9] << 8);
                            elect2_2 = advertisement[1].Data[10] | (advertisement[1].Data[11] << 8);
                            elect1_3 = advertisement[1].Data[12] | (advertisement[1].Data[13] << 8);
                            elect2_3 = advertisement[1].Data[14] | (advertisement[1].Data[15] << 8);
                        }

                        int input0 = elect1_0 + elect2_0;
                        int input1 = elect1_1 + elect2_1;
                        int input2 = elect1_2 + elect2_2;
                        int input3 = elect1_3 + elect2_3;

                        // Remove at 0 position for our chart
                        if (chartEntries.Count > 100)
                        {
                            chartEntries.RemoveAt(0);
                        }

                        if (packetCount == 10)
                        {

                            // Add an entry with a title
                            entries.Add(new Entry(input0) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = input0.ToString() });
                            chartEntries.Add(new Entry(input0) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = input0.ToString() });

                            // Add the rest of the entries
                            entries.Add(new Entry(input1) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            chartEntries.Add(new Entry(input1) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            entries.Add(new Entry(input2) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            chartEntries.Add(new Entry(input2) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            entries.Add(new Entry(input3) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            chartEntries.Add(new Entry(input3) { Color = SKColor.FromHsl(1, 100, 100, 255) });

                            //// Add an entry with a title
                            //entries.Add(new Entry(elect1_0) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = elect1_0.ToString() });
                            //chartEntries.Add(new Entry(elect1_0) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = elect1_0.ToString() });

                            //// Add the rest of the entries
                            //entries.Add(new Entry(elect1_1) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            //chartEntries.Add(new Entry(elect1_1) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            //entries.Add(new Entry(elect1_2) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            //chartEntries.Add(new Entry(elect1_2) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            //entries.Add(new Entry(elect1_3) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            //chartEntries.Add(new Entry(elect1_3) { Color = SKColor.FromHsl(1, 100, 100, 255) });

                            packetCount = 0;
                        } else
                        {

                            entries.Add(new Entry(input0) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            chartEntries.Add(new Entry(input0) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            entries.Add(new Entry(input1) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            chartEntries.Add(new Entry(input1) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            entries.Add(new Entry(input2) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            chartEntries.Add(new Entry(input2) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            entries.Add(new Entry(input3) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            chartEntries.Add(new Entry(input3) { Color = SKColor.FromHsl(1, 100, 100, 255) });

                            // Add all the entries
                            //entries.Add(new Entry(elect1_0) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            //chartEntries.Add(new Entry(elect1_0) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            //entries.Add(new Entry(elect1_1) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            //chartEntries.Add(new Entry(elect1_1) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            //entries.Add(new Entry(elect1_2) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            //chartEntries.Add(new Entry(elect1_2) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            //entries.Add(new Entry(elect1_3) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            //chartEntries.Add(new Entry(elect1_3) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            packetCount++;
                        }

                        // Add values to our fourier list
                        headBandInput.Add(input0);
                        headBandInput.Add(input1);
                        headBandInput.Add(input2);
                        headBandInput.Add(input3);
                        //headBandInput.Add(elect1_0);
                        //headBandInput.Add(elect1_1);
                        //headBandInput.Add(elect1_2);
                        //headBandInput.Add(elect1_3);

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
                            while (headBandInput.Count > 250)
                            {
                                headBandInput.RemoveAt(0);
                                entries.RemoveAt(0);
                                chartEntries.RemoveAt(0);
                            }

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
                    }
                    else if (Guids._guids.HSConnected == true && Guids._guids.HeadsetGuid == e.Device.Id)
                    {
                        Plugin.BLE.Abstractions.AdvertisementRecord[] advertisementHS = e.Device.AdvertisementRecords.ToArray();

                        // Data variables
                        int tempHS = 0;
                        //int presHS = 0;
                        float accelX = 0;
                        float accelY = 0;
                        float accelZ = 0;
                        float gyX = 0;
                        float gyY = 0;
                        float gyZ = 0;

                        if (advertisementHS.Length == 3)
                        {
                            tempHS = advertisementHS[2].Data[3] | (advertisementHS[2].Data[4] << 8);
                            accelX = advertisementHS[2].Data[5] | (advertisementHS[2].Data[6] << 8) | (advertisementHS[2].Data[7] << 16) | (advertisementHS[2].Data[8] << 24);
                            accelY = advertisementHS[2].Data[9] | (advertisementHS[2].Data[10] << 8) | (advertisementHS[2].Data[11] << 16) | (advertisementHS[2].Data[12] << 24);
                            accelZ = advertisementHS[2].Data[13] | (advertisementHS[2].Data[14] << 8) | (advertisementHS[2].Data[15] << 16) | (advertisementHS[2].Data[16] << 24);
                            gyX = advertisementHS[2].Data[17] | (advertisementHS[2].Data[18] << 8) | (advertisementHS[2].Data[18] << 16) | (advertisementHS[2].Data[19] << 24);
                            gyY = advertisementHS[2].Data[21] | (advertisementHS[2].Data[22] << 8) | (advertisementHS[2].Data[23] << 16) | (advertisementHS[2].Data[24] << 24);
                            gyZ = advertisementHS[2].Data[25] | (advertisementHS[2].Data[26] << 8) | (advertisementHS[2].Data[27] << 16) | (advertisementHS[2].Data[28] << 24);
                            // presHS = advertisementHS[2].Data[29] | (advertisementHS[2].Data[30] << 8);
                        } else
                        {
                            tempHS = advertisementHS[0].Data[3] | (advertisementHS[0].Data[4] << 8);
                            accelX = advertisementHS[0].Data[5] | (advertisementHS[0].Data[6] << 8) | (advertisementHS[0].Data[7] << 16) | (advertisementHS[0].Data[8] << 24);
                            accelY = advertisementHS[0].Data[9] | (advertisementHS[0].Data[10] << 8) | (advertisementHS[0].Data[11] << 16) | (advertisementHS[0].Data[12] << 24);
                            accelZ = advertisementHS[0].Data[13] | (advertisementHS[0].Data[14] << 8) | (advertisementHS[0].Data[15] << 16) | (advertisementHS[0].Data[16] << 24);
                            gyX = advertisementHS[0].Data[17] | (advertisementHS[0].Data[18] << 8) | (advertisementHS[0].Data[18] << 16) | (advertisementHS[0].Data[19] << 24);
                            gyY = advertisementHS[0].Data[21] | (advertisementHS[0].Data[22] << 8) | (advertisementHS[0].Data[23] << 16) | (advertisementHS[0].Data[24] << 24);
                            gyZ = advertisementHS[0].Data[25] | (advertisementHS[0].Data[26] << 8) | (advertisementHS[0].Data[27] << 16) | (advertisementHS[0].Data[28] << 24);
                            // presHS = advertisementHS[0].Data[29] | (advertisementHS[0].Data[30] << 8);
                        }

                        // Bit parsing
                        tempHS = tempHS/100;
                        // presHS = ((0x0142EE << 8) | presHS)/256;
                        accelX = accelX / 1000;
                        accelY = accelY / 1000;
                        accelZ = accelZ / 1000;
                        gyX = gyX / 1000;
                        gyY = gyY / 1000;
                        gyZ = gyZ / 1000;

                        // When temperatureEntries is > 50 every HS data is >50 so remove first value
                        if (temperatureEntries.Count > 50)
                        {
                            temperatureEntries.RemoveAt(0);
                            accelXEntries.RemoveAt(0);
                            accelYEntries.RemoveAt(0);
                            accelZEntries.RemoveAt(0);
                            gyXEntries.RemoveAt(0);
                            gyYEntries.RemoveAt(0);
                            gyZEntries.RemoveAt(0);
                        }

                        // Adds data to each entry. If it is the 10th entry it adds a display value
                        if (packetHSCount == 10)
                        {
                            temperatureEntries.Add(new Entry(tempHS) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = (tempHS).ToString() });
                            accelXEntries.Add(new Entry((float)accelX) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = ((float)accelX).ToString() });
                            accelYEntries.Add(new Entry((float)accelY) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = ((float)accelY).ToString() });
                            accelZEntries.Add(new Entry((float)accelZ) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = ((float)accelZ).ToString() });
                            gyXEntries.Add(new Entry((float)gyX) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = ((float)gyX).ToString() });
                            gyYEntries.Add(new Entry((float)gyY) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = ((float)gyY).ToString() });
                            gyZEntries.Add(new Entry((float)gyZ) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = ((float)gyZ).ToString() });
                            // presEntries.Add(new Entry(presHS) { Color = SKColor.FromHsl(1, 100, 100, 255), ValueLabel = (presHS).ToString() });
                            packetHSCount = 0;
                        }
                        else
                        {
                            temperatureEntries.Add(new Entry(tempHS) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            accelXEntries.Add(new Entry((float)accelX) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            accelYEntries.Add(new Entry((float)accelY) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            accelZEntries.Add(new Entry((float)accelZ) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            gyXEntries.Add(new Entry((float)gyX) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            gyYEntries.Add(new Entry((float)gyY) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            gyZEntries.Add(new Entry((float)gyZ) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            // presEntries.Add(new Entry(presHS) { Color = SKColor.FromHsl(1, 100, 100, 255) });
                            packetHSCount++;
                        }

                        // Declares the temperature chart
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

                        // Declares the accel x chart
                        var accelXChart = new LineChart()
                        {
                            Entries = accelXEntries,
                            LabelTextSize = 100,
                            LineSize = 10,
                            LineMode = LineMode.Straight,
                            PointSize = 20,
                            BackgroundColor = SKColor.Parse("#393E46")
                        };
                        this.accelXChart.Chart = accelXChart;

                        var accelYChart = new LineChart()
                        {
                            Entries = accelYEntries,
                            LabelTextSize = 100,
                            LineSize = 10,
                            LineMode = LineMode.Straight,
                            PointSize = 20,
                            BackgroundColor = SKColor.Parse("#393E46")
                        };
                        this.accelYChart.Chart = accelYChart;

                        var accelZChart = new LineChart()
                        {
                            Entries = accelZEntries,
                            LabelTextSize = 100,
                            LineSize = 10,
                            LineMode = LineMode.Straight,
                            PointSize = 20,
                            BackgroundColor = SKColor.Parse("#393E46")
                        };
                        this.accelZChart.Chart = accelZChart;

                        var gyXChart = new LineChart()
                        {
                            Entries = gyXEntries,
                            LabelTextSize = 100,
                            LineSize = 10,
                            LineMode = LineMode.Straight,
                            PointSize = 20,
                            BackgroundColor = SKColor.Parse("#393E46")
                        };
                        this.gyXChart.Chart = gyXChart;

                        var gyYChart = new LineChart()
                        {
                            Entries = gyYEntries,
                            LabelTextSize = 100,
                            LineSize = 10,
                            LineMode = LineMode.Straight,
                            PointSize = 20,
                            BackgroundColor = SKColor.Parse("#393E46")
                        };
                        this.gyYChart.Chart = gyYChart;

                        var gyZChart = new LineChart()
                        {
                            Entries = gyZEntries,
                            LabelTextSize = 100,
                            LineSize = 10,
                            LineMode = LineMode.Straight,
                            PointSize = 20,
                            BackgroundColor = SKColor.Parse("#393E46")
                        };
                        this.gyZChart.Chart = gyZChart;

                        //var presChart = new LineChart()
                        //{
                        //    Entries = presEntries,
                        //    LabelTextSize = 100,
                        //    LineSize = 10,
                        //    LineMode = LineMode.Straight,
                        //    PointSize = 20,
                        //    BackgroundColor = SKColor.Parse("#393E46")
                        //};
                        //this.presChart.Chart = presChart;
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