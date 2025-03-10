using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Net.Http;
using static SoftataInfo2DB.Program;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using static Softata.SoftataLib.Analog;
using System.Reflection.Metadata;

namespace SoftataInfo2DB
{
    internal partial class Program
    {
        private static void GetDeviceTypes()
        {
            Info.TargetDeviceTypes = new Dictionary<int, string>();
            string deviceTypesCSV = Info.SoftataLib.SendMessageCmd("Devices");
            string[] parts = deviceTypesCSV.Split(":");
            List<string> devicesTypes = parts[1].Split(",").ToList();
            var deviceTypelist =
                    devicesTypes.Select((value, index) => new { value, index })
                    .ToDictionary(x => x.index, x => x.value);
            foreach (var item in deviceTypelist)
            {
                Info.TargetDeviceTypes.Add(item.Key, item.Value);
            }
        }

        private static void GetDeviceTypesGenericCommands()
        {
            Info.GenericCmds = new Dictionary<int, Dictionary<int, string>>();
            foreach (var dt in Info.TargetDeviceTypes)
            {
                try
                {
                    byte subcmd = 0;//getGenericCommands
                    string cmdsCSV = Info.SoftataLib.SendTargetCommand((byte)(dt.Key), 0, subcmd);
                    if (cmdsCSV.Contains(":"))
                    {
                        string[] parts = cmdsCSV.Split(":");
                        List<string> cmdsList = parts[1].Split(",").ToList();
                        var cmdlist =
                                cmdsList.Select((value, index) => new { value, index })
                                .ToDictionary(x => x.index, x => x.value);
                        Info.GenericCmds.Add(dt.Key, cmdlist);
                        Thread.Sleep(500);
                    }
                    else
                        continue;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        private static void GetDeviceTypesDevices()
        {
            Info.TargetDevices = new Dictionary<int, Dictionary<int, string>>();
            Thread.Sleep(200);
            foreach (var dt in Info.TargetDeviceTypes)
            {
                try
                {
                    byte subCmd = 1;//getDevices
                    string DevicesCSV = Info.SoftataLib.SendTargetCommand((byte)(dt.Key), 0, subCmd);
                    if (DevicesCSV.Contains(":"))
                    {
                        string[] parts2 = DevicesCSV.Split(":");
                        List<string> devices = parts2[1].Split(",").ToList();
                        var devicelist =
                                devices.Select((value, index) => new { value, index })
                                .ToDictionary(x => x.index, x => x.value);
                        Info.TargetDevices.Add(dt.Key, devicelist);
                    }
                    else
                        continue;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }


        private static string stringAddDevice(int cmdTarget, int deviceIndex)
        {
            var device = Info.TargetDevices[cmdTarget][deviceIndex];
            var dictionary = Info.GenericCmds[cmdTarget];
            var values = dictionary
                .Where(kvp => kvp.Value.ToLower().Contains("setupdefault"))
                .Select(kvp => kvp.Key);
            byte subCmd = (byte)values.FirstOrDefault();
            string response = Info.SoftataLib.SendTargetCommand((byte)cmdTarget, 1, subCmd, (byte)deviceIndex);
            return response;
            //subCmd = GetGenericCmdIndex("setupdefault", GenericCommands);
            //result = softatalib.SendTargetCommand(cmdTarget, 1, subCmd, (byte)TargetDevice.Index);

        }

        private static  bool _Connect(string ipAddress, int _port)
        {
            //SoftataLib SoftataLib = GetSoftataLib();
            bool result = Info.SoftataLib.Connect(ipAddress, _port);
            //SessionExtensions.Set(HttpContext.Session, "SoftataLib", SoftataLib);
            if (result)
            {
                return true;
            }
            else
                return false;
        }

        public static string Start(string ipAddress = "192.168.0.5", int _port = 4242)
        {
            //SoftataLib SoftataLib = GetSoftataLib();
            bool result = _Connect(ipAddress, _port);
            if (result)
            {

                string beginValue = Info.SoftataLib.SendMessageCmd("Begin");
                if (beginValue == "Ready")
                {
                    string OKresult = $"Connected to {ipAddress}:{_port} and Ready";
                    string value = Info.SoftataLib.SendMessageCmd("Version");
                    OKresult += $"\nSoftata Version:{value}";
                    ///////////////////////////////////
                    string cmdsOffset = Info.SoftataLib.SendMessageCmd("Soffset");
                    if (int.TryParse(cmdsOffset, out int _offset))
                    {
                        Info.SoftataLib.Offset = _offset; //Should be 0xf0
                        Console.WriteLine($"CommandsOffset: {_offset}");
                    }

                    GetDeviceTypes();

                    GetDeviceTypesGenericCommands();

                    GetDeviceTypesDevices();


                    ///////////////////////////////
                    return "Ok";
                }
                else
                {
                    Info.SoftataLib.Disconnect();
                    return "BadRequest";
                }
            }
            else
            {
                return "BadRequest";
            }
        }

    }
}
