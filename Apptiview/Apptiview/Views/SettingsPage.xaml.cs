using Android;
using Android.Content.PM;
using Android.OS;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Apptiview.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public ObservableCollection<string> DeviceList { get; set; }
        public ObservableCollection<string> StateBT { get; set; }
        public List<Guid> ids;
        //public Guid headbandGuid;

        IAdapter adapter;

        public SettingsPage(IAdapter adapter)
        {
            InitializeComponent();

            // Set up adapter and ble
            var ble = CrossBluetoothLE.Current;
            this.adapter = adapter;

            // Grab state of bluetooth
            this.StateBT = new ObservableCollection<string>();
            if (ble.State == BluetoothState.On)
            {
                StateBT.Add("Bluetooth is: On");
            } else
            {
                StateBT.Add("Bluetooth is: Off");
            }

            // Create Device list
            this.DeviceList = new ObservableCollection<string>();
            // Set up ids
            this.ids = new List<Guid>();

            // Updates bluetooth state field
            ble.StateChanged += (s, e) =>
            {
                StateBT.Clear();
                if (ble.State == BluetoothState.On)
                {
                    StateBT.Add("Bluetooth is: On");
                }
                else
                {
                    StateBT.Add("Bluetooth is: Off");
                }
            };

            BindingContext = this;
        }

        // On selection of bluetooth device
        // Grabs uuid and sets it to global variable
        void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }
            string[] splitString = e.SelectedItem.ToString().Split('.');
            int position = Int32.Parse(splitString[0]);
            Guids._guids.HeadbandGuid = ids[position];
            DisplayAlert("Connected to device", e.SelectedItem.ToString(), "Ok");
        }

        async void ScanBluetooth(Object sender, System.EventArgs e)
        {
            var adapterScan = CrossBluetoothLE.Current.Adapter;
            DeviceList.Clear();
            ids.Clear();
            int count = 1;

            adapterScan.DeviceDiscovered += (s, a) =>
            {
                if (a.Device.Name == "BioSenseHeadband" || a.Device.Name == "BioSenseHeadset")
                {                    
                    DeviceList.Add(count + ". " + a.Device.Name);
                    ids.Add(a.Device.Id);
                    Plugin.BLE.Abstractions.AdvertisementRecord[] advertisement = a.Device.AdvertisementRecords.ToArray();
                    count++;
                }
            };
            await adapterScan.StartScanningForDevicesAsync();

            /*
             * Scanner can find 0 devices but not "Time out"
             * Caused because it will find devices but not print them out
            */
            adapterScan.ScanTimeoutElapsed += (sender1, e1) =>
            {
                adapterScan.StopScanningForDevicesAsync();
                Device.BeginInvokeOnMainThread(() =>
                {
                    IsBusy = false;
                    DisplayAlert("Timeout", "Bluetooth scan timeout elapsed, no devices were found", "OK");
                });
            };

            BindingContext = this;
        }
    }
}