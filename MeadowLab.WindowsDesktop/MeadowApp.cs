using Meadow;
using Meadow.Foundation.Displays;
using System;
using System.Threading.Tasks;
using Softata;
using System.Threading;

namespace MeadowDesktopHello
{
    public class MeadowApp : App<Desktop>
    {
        private const int Port = 4242;
        private const string ipaddressStr = "192.168.0.9";
        public override Task Initialize()
        {
            Resolver.Log.Info($"Meadow.Windows Softata Sample");
            Resolver.Log.Info($"----------------------------");

            Resolver.Log.Info($"Initializing {this.GetType().Name}");
            Resolver.Log.Info($" Platform OS is a {Device.PlatformOS.GetType().Name}");
            Resolver.Log.Info($" Platform: {Device.Information.Platform}");
            Resolver.Log.Info($" OS: {Device.Information.OSVersion}");
            Resolver.Log.Info($" Model: {Device.Information.Model}");
            Resolver.Log.Info($" Processor: {Device.Information.ProcessorType}");



            Resolver.Log.Info($"{Device.NetworkAdapters.Count} network adapters detected");
;

            foreach (var adapter in Device.NetworkAdapters)
            {
                Resolver.Log.Info($"  {adapter.Name}  {adapter.IpAddress}");
                if(adapter.Name == "Wi-Fi")
                {
                    Resolver.Log.Info($"  {adapter.Name}  {adapter.IpAddress}");
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

                    string endMsg = SoftataLib.SendMessageCmd("End");
                    Resolver.Log.Info($"{endMsg}");
                    Thread.Sleep(500);
                    break;
                }
            }

            return base.Initialize();
        }

        public override Task Run()
        {


            return Task.CompletedTask;
        }

    }
}

