using Android;
using Android.Content.PM;
using Android.OS;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
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

            var testString = "temp";

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

        void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }
            DisplayAlert("Item Selected", e.SelectedItem.ToString(), "Ok");
        }

        async void ScanBluetooth(Object sender, System.EventArgs e)
        {
            var adapterScan = CrossBluetoothLE.Current.Adapter;

            adapterScan.DeviceDiscovered += (s, a) =>
            {
                if (a.Device.Name == null)
                {
                    DeviceList.Add("Device name N/A");
                } else
                {
                    DeviceList.Add(a.Device.Name);
                }
            };
            await adapterScan.StartScanningForDevicesAsync();

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