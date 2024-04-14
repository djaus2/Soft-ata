using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;
using Meadow.Peripherals.Leds;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Softata;
using System.Threading;


namespace MeadowApp
{

    public class MeadowApp : App<F7CoreComputeV2>
    {    
        private const string WIFI_NAME = "APQLZM";
        private const string WIFI_PASSWORD = "silly1371";
        private const int Port = 4242;
        private const string ipaddressStr = "192.168.0.9";

        static IWiFiNetworkAdapter wifi;

        public override async Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
            await ConnectToWiFi(WIFI_NAME, WIFI_PASSWORD);


            return; //base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            return Task.CompletedTask;
        }

        public async Task ConnectToWiFi(string Ssid, string Pwd) //), int port)
        {
            
            Resolver.Log.Info("Run...");

            wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();

            // connected event test.
            wifi.NetworkConnected += WiFiAdapter_NetworkConnected;


            try
            {
                // connect to the wifi network.
                Resolver.Log.Info($"Connecting to WiFi Network {Ssid}");

                await wifi.Connect(Ssid, Pwd, TimeSpan.FromSeconds(45));
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to Connect: {ex.Message}");
            }
            if (wifi.IsConnected)
            {

            }
        }

        void WiFiAdapter_NetworkConnected(INetworkAdapter networkAdapter, NetworkConnectionEventArgs e)
        {
            SoftataLib.Connect(ipaddressStr, Port);
            Resolver.Log.Info("Connection request completed");

            SoftataLib.SendMessageCmd("Begin");
            Thread.Sleep(500);

            string Version = SoftataLib.SendMessageCmd("Version");
            Resolver.Log.Info($"Softata Version: {Version}");
            Thread.Sleep(500);
            string devicesCSV = SoftataLib.SendMessageCmd("Devices");
            Resolver.Log.Info($"{devicesCSV}");
            Thread.Sleep(500);
        }


    }
}