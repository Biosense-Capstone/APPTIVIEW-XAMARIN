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

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using System.Collections;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using System.IO;

using SQLite;
//using Newtonsoft.Json;

namespace Apptiview.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        static bool scanning = false;

        public ObservableCollection<string> StateBT { get; set; }
        public List<Guid> ids;

        //public ObservableCollection<string> testList { get; set; }
        public class testSQL
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
            public int data { get; set; }
            public string stringData { get; set; }
        }

        public class bluetoothList
        {
            public string name { get; set; }
            public string Connected { get; set; } = "Not connected";
        }
        public ObservableCollection<bluetoothList> DeviceList { get; set; }
        public bluetoothList headbandDevice = new bluetoothList();
        public bluetoothList headsetDevice = new bluetoothList();

        IAdapter adapter;

        public SettingsPage(IAdapter adapter)
        {
            InitializeComponent();

            /*
            // Initialize local storage
            // Get an absolute path to the database file
            var databasePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "MyData.db");
            var db = new SQLiteConnection(databasePath);
            db.CreateTable<testSQL>();
            Console.WriteLine("Table created!");

            var stock = new testSQL()
            {
                data = 10,
                stringData = "test"
            };
            var stock1 = new testSQL()
            {
                data = 5,
                stringData = "Fuck you"
            };
            var stock2 = new testSQL()
            {
                data = 5,
                stringData = "Firetruck"
            };
            db.Insert(stock);
            db.Insert(stock1);
            db.Insert(stock2);
            var query = db.Table<testSQL>().Where(v => v.stringData.StartsWith("F"));
            foreach (var stockTemp in query)
                Console.WriteLine("Stock: " + stockTemp.stringData);
            Console.WriteLine("Auto stock data: {0}", stock.data);
            */

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
            this.DeviceList = new ObservableCollection<bluetoothList>();
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
            //string[] splitString = e.SelectedItem.ToString().Split('.');
            bluetoothList temp = (bluetoothList)e.SelectedItem;
            string[] splitString = temp.name.Split('.');
            int position = Int32.Parse(splitString[0]);
            if (splitString[1] == " BHB")
            {
                if (Guids._guids.HBConnected == false)
                {
                    Guids._guids.HBConnected = true;
                    headbandDevice.Connected = "Connected";
                    Guids._guids.HeadbandGuid = ids[position - 1];
                    DisplayAlert("Connected to device", splitString[1], "Ok");
                } else
                {
                    Guids._guids.HBConnected = false;
                    headbandDevice.Connected = "Not connected";
                    DisplayAlert("Disconnected from device", splitString[1], "Ok");
                }
                devices.ItemsSource = null;
                devices.ItemsSource = DeviceList;
                devices.SelectedItem = null;
            }
            else if (splitString[1] == " BHS")
            {
                if (Guids._guids.HSConnected == false)
                {
                    Guids._guids.HSConnected = true;
                    headsetDevice.Connected = "Connected";
                    Guids._guids.HeadsetGuid = ids[position - 1];
                    DisplayAlert("Connected to device", splitString[1], "Ok");
                } else
                {
                    Guids._guids.HSConnected = false;
                    headsetDevice.Connected = "Not connected";
                    DisplayAlert("Disconnected from device", splitString[1], "Ok");
                }
                devices.ItemsSource = null;
                devices.ItemsSource = DeviceList;
                devices.SelectedItem = null;
            }
        }

        async void ScanBluetooth(Object sender, System.EventArgs e)
        {

            // Once scanner is on, it will not turn off. Needs to stay on to receive advertisements
            if (scanning == false)
            {
                scanning = true;
            } else
            {
                return;
            }

            var adapterScan = CrossBluetoothLE.Current.Adapter;
            //DeviceList.Clear();
            ids.Clear();
            int count = 1;

            adapterScan.DeviceDiscovered += (s, a) =>
            {
                if (a.Device.Name != null && (a.Device.Name == "BHB" || a.Device.Name.Substring(0,3) == "BHS"))
                {
                    if (a.Device.Name == "BHB")
                    {
                        headbandDevice.name = count + ". " + a.Device.Name;
                        DeviceList.Add(headbandDevice);
                        //DeviceList.Add(count + ". " + a.Device.Name);
                    } else
                    {
                        headsetDevice.name = count + ". " + a.Device.Name.Substring(0, 3);
                        DeviceList.Add(headsetDevice);
                        //DeviceList.Add(count + ". " + a.Device.Name.Substring(0, 3));
                    }
                    ids.Add(a.Device.Id);
                    //Plugin.BLE.Abstractions.AdvertisementRecord[] advertisement = a.Device.AdvertisementRecords.ToArray();
                    count++;
                }
            };
            adapterScan.ScanTimeout = -1;
            adapterScan.ScanMode = ScanMode.LowLatency;
            try
            {
                await adapterScan.StartScanningForDevicesAsync();
            }
            catch(Plugin.BLE.Abstractions.Exceptions.DeviceDiscoverException)
            {

            }

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