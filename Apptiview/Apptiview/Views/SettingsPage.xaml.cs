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
//using Newtonsoft.Json;

namespace Apptiview.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public ObservableCollection<string> DeviceList { get; set; }
        public ObservableCollection<string> StateBT { get; set; }
        public List<Guid> ids;

        public ObservableCollection<string> testList { get; set; }
        public class Account
        {
            //[JsonProperty (PropertyName = "id")]
            [Newtonsoft.Json.JsonProperty("id")]
            public string id
            {
                get { return id; }
                set { id = value; }
            }

            //[JsonProperty(PropertyName = "Username")]
            [Newtonsoft.Json.JsonProperty("Username")]
            public string Username
            {
                get { return Username; }
                set { Username = value; }
            }

            //[JsonProperty(PropertyName = "Password")]
            [Newtonsoft.Json.JsonProperty("Password")]
            public string Password
            {
                get { return Password; }
                set { Password = value; }
            }
        }

        public class AzureDataService
        {
            IMobileServiceClient client { get; set; }
            IMobileServiceSyncTable<Account> accountTable;

            public async Task Initialize()
            {
                // Our client
                client = new MobileServiceClient("https://apptiviewdatabaseapp.azurewebsites.net");

                // Our local storage path
                string path = "localstore.db";
                //var path = Path.Combine(MobileServiceClient.DefaultDatabasePath, "syncstore.db");
                var store = new MobileServiceSQLiteStore(path);
                //store.DefineTable<Account>();
                await client.SyncContext.InitializeAsync(store, new MobileServiceSyncHandler());

                // Our sync table
                accountTable = client.GetSyncTable<Account>();
            }

            //public async Task<List<Account>> getAccount()
            public async Task<List<Account>> getAccount()
            {
                await SyncAccount();
                return await accountTable.ToListAsync();
                //return accountTable.TableName;
            }

            public async Task AddAccount(string user, string pass)
            {
                var newAccount = new Account
                {
                    Username = "TEST",
                    Password = "test"
                };
                await accountTable.InsertAsync(newAccount);
                //Synchronize coffee
                await SyncAccount();
            }

            public async Task SyncAccount()
            {
                // Pull and then sync
                await accountTable.PullAsync("allAccounts", accountTable.CreateQuery());
                await client.SyncContext.PushAsync();
            }
        }
        public AzureDataService database;
        public ObservableCollection<Account> accountList;

        IAdapter adapter;

        public SettingsPage(IAdapter adapter)
        {
            InitializeComponent();

            // Initialize database
            database = new AzureDataService();
            InitializeDatabase(database);
            accountList = new ObservableCollection<Account>();

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
            if (splitString[1] == " BHB")
            {
                Guids._guids.HBConnected = true;
                Guids._guids.HeadbandGuid = ids[position-1];
            }
            else if (splitString[1] == " BHS")
            {
                Guids._guids.HSConnected = true;
                Guids._guids.HeadsetGuid = ids[position-1];
            }
            
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
                if (a.Device.Name == "BHB" || a.Device.Name == "BHS")
                {                    
                    DeviceList.Add(count + ". " + a.Device.Name);
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

        async void InitializeDatabase(AzureDataService database)
        {
            await database.Initialize();
            return;
        }

        void SQLQuery(object sender, SelectedItemChangedEventArgs e)
        {
            //GetItemsAsync();
            //Account item = new Account { User = "ched", Pass = "chedder", Id = "1" };
            //MobileService.GetTable<Account>().InsertAsync(item);
        }

        async void grabTable(object sender, SelectedItemChangedEventArgs e)
        {
            //grabBtn.IsEnabled = false;
            //List<Account> accounts = await database.getAccount();
            //grabBtn.IsEnabled = true;
        }

        //public Task<List<string>> GetItemsAsync()
        //{
            
        //    return database.Table<string>().ToListAsync();
        //}
    }
}