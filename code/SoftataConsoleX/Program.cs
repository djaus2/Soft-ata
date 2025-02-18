using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Data.Common;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Softata;
using Softata.Enums;
using System.Collections.Generic;
using System.Collections;
using static Softata.SoftataLib;
//using SoftataConsole;

using B = ConsoleTextFormat.Fmt.Bold;
using F = ConsoleTextFormat.Fmt;
using L = ConsoleTextFormat.Layout;
using ConsoleTextFormat;
using System.Runtime.Intrinsics.X86;
using static System.Runtime.CompilerServices.RuntimeHelpers;
using System.Diagnostics.Eventing.Reader;
using System.Transactions;
using System.Windows.Input;
using System.Data.SqlTypes;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using Softata.ActionCommands;
using SoftataConsole2;



namespace SoftataBasic
{
    public class SelectedDeviceLoopVars: IDisposable
    {
        public Tuple<byte, byte, byte> rgb { get; set; } = new Tuple<byte, byte, byte>(0x40, 0, 0);
        public byte imisc { get; set; } = 0xff;
        public byte misc_led { get; set; } = 0;
        public byte misc_bglevel { get; set; } = 0;
        public byte misc_brightness { get; set; } = 1;
        public byte misc_num { get; set; } = 0;
        public Tuple<int, int>? deviceRange { get; set; } = null;

        public bool foundRange { get; set; } = false;
        // bool isRelay { get; set; } = false;
        //public bool isQuadRelay { get; set; } = false;
        public byte relay_bit_no { get; set; } = 0;
        public void Dispose()
        {
            
        }
   
    }
    internal partial class Program
    {
        static int port = 4242;
        static string ipaddressStr = "192.168.0.12";

        const string Tab5 = "\t\t\t\t\t";


        private static Softata.SoftataLib _softatalib;

        static Softata.SoftataLib softatalib { get { return _softatalib; } set { _softatalib = value; } }

        internal static void ShowHeading(string test = "")
        {
            test = string.IsNullOrEmpty(test) ? "" : $": {test}";
            Layout.RainbowHeading($"SOFTATA TESTS{test}");
            Console.WriteLine("--------------------------");
            Console.WriteLine("For details see https://davidjones.sportronics.com.au/cats/softata/");
        }

        public static bool connected
        {
            get
            {
                if (softatalib == null)
                    return false;
                return softatalib.Connected;
            }
        }
        static void Main(string[] args)
        {
            
            softatalib = new SoftataLib();
            Softata.ActionCommands.CommandsPortal.Setup(
                new LLayout(),
                GetGenericCmdIndex,
                softatalib
            );


            Console.Clear();
            ShowHeading();
            Console.WriteLine("For details see https://davidjones.sportronics.com.au/cats/softata/");
            Console.WriteLine();


            //SettingsManager.ClearAllSettings();
            SettingsManager.ReadAllSettings();

            string? _ipaddressStr = SettingsManager.ReadSetting("IpaddressStr");
            if (!string.IsNullOrEmpty(_ipaddressStr))
            {
                if (_ipaddressStr.Count(c => c == '.') == 3)
                {
                    if (IPAddress.TryParse(_ipaddressStr, out IPAddress? address))
                    {
                        ipaddressStr = _ipaddressStr;
                    }
                    else
                        Console.WriteLine("\t\t App SettingsIP Address");
                }
                else
                    Console.WriteLine("\t\tInvalid App Settings IP Address");
            }
            else
            {
                SettingsManager.AddUpdateAppSettings("IpaddressStr", ipaddressStr);
            }
            string _port = SettingsManager.ReadSetting("Port");
            if (!string.IsNullOrEmpty(_port))
            {
                if (int.TryParse(_port, out int _portNo))
                {
                    port = _portNo;
                }
                else
                    Console.WriteLine("\t\tInvalid AppSettings  Port");
            }
            else
            {
                SettingsManager.AddUpdateAppSettings("Port", port.ToString());
            }

            string IpAddress = ipaddressStr;
            int Port = port;


            bool quit = false;
            bool back = false;

            string DeviceTypesCSV = "";
            string DevicesCSV = "";
            string CommandsCSV = "";
            string PinoutsCSV = "";

            Selection TargetDeviceType = new Selection(); 
            Selection TargetDevice = new Selection();
            Selection TargetCommand = new Selection();


            while (!quit)
            {
                Selection TargetPin = new Selection();
                Selection TargetMiscCmd = new Selection();
                back = false;
                CommandsCSV = "";
                PinoutsCSV = "";
                ShowHeading();

                try
                {

                    if (!connected)
                    {
                        Console.WriteLine($"{B.fgblu}Default Softata Server is at{B.fgYel} {ipaddressStr}:{port}{Fmt.clr}");
                        Layout.Info("Enter new values", " or press [Enter] to continue:");
                        Console.Write("Plz Enter IPAdress: ");
                        string? ip = Console.ReadLine();
                        if (!string.IsNullOrEmpty(ip))
                        {
                            if (ip.Count(c => c == '.') == 3)
                            {
                                if (IPAddress.TryParse(ip, out IPAddress? address))
                                {
                                    IpAddress = ip;
                                    SettingsManager.AddUpdateAppSettings("IpaddressStr", IpAddress);
                                }
                                else
                                    Console.WriteLine("\t\tInvalid IP Address");
                            }
                            else
                                Console.WriteLine("\t\tInvalid IP Address");
                        }

                        Console.Write("Plz Enter Port: ");
                        string? prt = Console.ReadLine();
                        if (!string.IsNullOrEmpty(prt))
                        {
                            if (int.TryParse(prt, out int portNo))
                            {
                                Port = portNo;
                                SettingsManager.AddUpdateAppSettings("Port", Port.ToString());
                            }
                            else
                                Console.WriteLine("\t\tInvalid Port");
                        }
                        ShowHeading();
                        Console.WriteLine($"{B.fgblu}The selected Softata Server is at{B.fgYel} {ipaddressStr}:{port}{Fmt.clr}");
                        Console.WriteLine("Make sure the Pico has has booted ...");
                        Console.WriteLine(" ... and is waiting (4s slow flash) before proceeding:"); Console.WriteLine();
                    }

                    quit = false;
                    Layout.AddHideMenuItems("MaxType");
                    Layout.AddHideMenuItems("Undefined");



                    if (quit)
                        return;

                    ShowHeading();


                    if (!connected)
                    {
                        bool res = softatalib.Connect(IpAddress, Port);
                        if (!res)
                        {
                            Console.WriteLine($"Failed to connect to {IpAddress}:{Port}");
                            Console.WriteLine("Press [Enter] to try again or [Q] to quit");
                            string? key = Console.ReadLine();
                            if (!string.IsNullOrEmpty(key))
                            {
                                if (key.ToUpper() == "Q")
                                    quit = true;
                            }
                            else continue;
                        }
                        else
                        {

                            Console.WriteLine($"Connected to {IpAddress}:{Port}");
                            Console.WriteLine();
                            //quit = YesNoQuit("Press [Enter] to continue or [Q] to quit", true);
                        }
                    }

                    if (!quit)
                    {
                        List<string> stringlist = new List<string>();// { "setup", "pins", "properties", "valuerange" };

                        if (DeviceTypesCSV == "")

                        {

                            softatalib.SendMessageCmd("Begin");
                            Thread.Sleep(100);
                            string Version = softatalib.SendMessageCmd("Version");
                            Console.WriteLine($"Softata Version: {Version}");
                            Thread.Sleep(100);
                            string cmdsOffset = softatalib.SendMessageCmd("Soffset");
                            if (int.TryParse(cmdsOffset, out int _offset))
                            {
                                softatalib.Offset = _offset; //Should be 0xf0
                                Console.WriteLine($"CommandsOffset: {_offset}");
                            }
                            Thread.Sleep(100);
                            DeviceTypesCSV = softatalib.SendMessageCmd("Devices");
                            Console.WriteLine($"{DeviceTypesCSV}");
                        }

                        TargetDeviceType = Layout.PromptWithCSVList(TargetDeviceType.Index , DeviceTypesCSV, true, false);
                        if (TargetDeviceType.Index < 0)
                        {
                            quit = true;
                            continue;
                        }

                        else
                        {
                            byte cmdTarget = (byte)TargetDeviceType.Index;

                            Layout.Info($"Selected target device type: ", $" {TargetDeviceType.Item}");

                            byte subCmd = 0; //getCmds Always first
                            CommandsCSV = softatalib.SendTargetCommand(cmdTarget, 1, subCmd);
                            string[] temp = CommandsCSV.Split(":");
                            string[] GenericCommands = temp.Last().Split(",");
                            List<string>? sensorProperties = null;
                            //Get commands to be diaplyed in menu
                            Dictionary<int,string> useGenericCommands = new Dictionary<int,string>();
                            for (int i = 0; i < GenericCommands.Length; i++)
                            {
                                if (char.IsUpper(GenericCommands[i][0]))
                                {
                    
    
                                }
                                else
                                {
                                    // Will have leading A_ D_ or S_
                                    // OR
                                    // leading S__ D__ or S__
                                    if (GenericCommands[i].Substring(1, 2) == "__")
                                    {
                                        // These commands are called from the device class type
                                        // Using the specific device type ordinal
                                        // Without using an instance
                                        string cmd = GenericCommands[i].Substring(3);
                                        stringlist.Add(cmd);
                                        useGenericCommands.Add(i, cmd);
                                        // Will have leading A_ D_ or S_
                                        // General Actuator/Display/Sensor class info
                                    }
                                    else
                                    {
                                        //Will have leading a_ d_ or s_ 
                                        string cmd = GenericCommands[i].Substring(2);
                                        useGenericCommands.Add(i, cmd);
                                    }
                                }
                            }
                            //define DISPLAY_COMMANDS C(D_getDevicesCMD)C(D_getCmdsCMD)C(D_getpinsCMD)C(D_setupDefaultCMD)C(D_setupCMD)C(D_dispose)C(d_miscGetListCMD)C(d_clearCMD)C(d_backlightCMD)C(d_setCursorCMD)C(d_writestringCMD)C(d_cursor_writestringCMD)C(d_home)C(d_dummyCMD)C(D_miscCMD)
                            
                            subCmd = GetGenericCmdIndex("getDevices", GenericCommands);
                            DevicesCSV = softatalib.SendTargetCommand(cmdTarget, 0, subCmd);
                            int pinn = 0;
                            TargetDevice = Layout.PromptWithCSVList(TargetDevice.Index, DevicesCSV, true, true);
                            if (TargetDevice.Index < 0)
                            {
                                if (TargetDevice.Index == -1)
                                    quit = true;
                                continue;
                            }
                            else
                            {
                                Layout.Info($"Selected target device : ", $" {TargetDevice.Item}");
                                Layout.Info("Default Setup:");

                                subCmd = GetGenericCmdIndex("getpins", GenericCommands);
                                PinoutsCSV = softatalib.SendTargetCommand(cmdTarget, 1, subCmd, (byte)TargetDevice.Index);
                                TargetPin = Layout.PromptWithCSVList(TargetPin.Index, PinoutsCSV, true, true);
                                if (TargetPin.Index < 0)
                                {
                                    if (TargetPin.Index == -1)
                                        quit = true;
                                    continue;
                                }
                                string result = "";
                                if (TargetPin.Index == 0)
                                {
                                    if (TargetPin.Item.Contains("="))
                                    {
                                        string[] info = TargetPin.Item.Split("=");
                                        TargetPin.Item = info[0].Trim();
                                    }
                                    Layout.Info($"Using Default Setup: Pin:{TargetPin.Item}");
                                    subCmd = GetGenericCmdIndex("setupdefault", GenericCommands);
                                    result = softatalib.SendTargetCommand(cmdTarget, 1, subCmd, (byte)TargetDevice.Index);

                                }
                                else
                                {
                                    pinn = 16;
                                    pinn = Layout.Prompt4IntInRange(16, 21,pinn);
                                    TargetPin.Index = pinn;
                                    TargetPin.Item = $"Pin = {pinn}";

                                    byte num_bits = 0;
                                    byte[]? data = null;
                                    if (TargetDeviceType.Item.ToLower() == "actuator")
                                    {

                                        if (TargetDevice.Item.ToLower().Contains("quad"))
                                        {
                                            num_bits = 4;
                                            int max_bits = 22 - pinn;
                                            Layout.Info($"Max bits: {max_bits}", "Bits can only use pins 16 to 21");
                                            num_bits = (byte)Layout.Prompt4IntInRange(1, max_bits, num_bits);
                                            Layout.Info($"Using NonDefault Setup: {TargetPin.Item} Num Bits:{num_bits}");
                                            data = new byte[] { num_bits };
                                        }
                                        else if (TargetDevice.Item.ToLower().Contains("sipo"))
                                        {
                                            num_bits = 4;
                                            int max_bits = 8;
                                            Layout.Info($"Max bits: {max_bits}", "");
                                            num_bits = (byte)Layout.Prompt4IntInRange(1, max_bits, num_bits);
                                            Layout.Info($"Using NonDefault Setup: {TargetPin.Item} Num Bits:{num_bits}");
                                            data = new byte[] { num_bits };
                                        }
                                        else
                                        {
                                            Layout.Info($"Using NonDefault Setup: {TargetPin.Item}");
                                        }
                                        Layout.Info($"Note: When selecting a bit (num): ", $"bits are numbered from 1.");
                                    }

                                    subCmd = GetGenericCmdIndex("setupgeneral", GenericCommands);                                   
                                    result = softatalib.SendTargetCommand(cmdTarget, (byte)TargetPin.Index, subCmd, (byte)TargetDevice.Index,0xff,data);

                                }


                                if ((TargetDeviceType.Item.ToLower() == "actuator")
                                    || (TargetDeviceType.Item.ToLower() == "deviceinput"))
                                {
                                    Layout.Info($"Note: When selecting a bit (num): ", $"bits are numbered from 1.");
                                }

                                cmdTarget = (byte)TargetDeviceType.Index;
                                

                                byte linkedListNo = 0xff;
                                if (byte.TryParse(result, out byte lln))
                                {
                                    if (lln != 0xff)
                                        linkedListNo = lln;
                                    else
                                        quit = true;

                                }
                                else
                                    quit = true;

                                // Default is bit functions only.
                                int capabilities = (int)DeviceInputCapabilities.i_none;


                                if (TargetDeviceType.Item.ToLower() == "actuator")
                                {
                                    subCmd = GetGenericCmdIndex("GetActuatorCapab", GenericCommands);
                                    string response = softatalib.SendTargetCommand((byte)TargetDeviceType.Index, (byte)pinn, (byte)subCmd, (byte)0xff, linkedListNo);
                               
                                    if(int.TryParse(response, out int _capabilities))
                                    {
                                        capabilities = _capabilities;
                                    }
                                }
                                else if (TargetDeviceType.Item.ToLower() == "deviceinput")
                                {
                                    subCmd = GetGenericCmdIndex("GetInputCapab", GenericCommands);
                                    string response = softatalib.SendTargetCommand((byte)TargetDeviceType.Index, (byte)pinn, (byte)subCmd, (byte)0xff, linkedListNo);

                                    if (int.TryParse(response, out int _capabilities))
                                    {
                                        capabilities = _capabilities;
                                    }
                                }


                                Softata.ActionCommands.CommandsPortal commandsPortal = new Softata.ActionCommands.CommandsPortal
                                (
                                    GenericCommands,
                                    useGenericCommands,
                                    stringlist,
                                    TargetDeviceType,
                                    TargetDevice,
                                    linkedListNo,
                                    capabilities
                                );

                                using (Softata.ActionCommands.SelectedDeviceLoopVars2 selectedDeviceLoopVars = commandsPortal.selectedDeviceLoopVars)
                                {

                                    while ((!quit) && (!back))
                                    {
                                        pinn = 0;
                                        TargetCommand = Layout.PromptWithDictionaryList(TargetCommand.Order, useGenericCommands, true, true);
                                        if (TargetCommand.Index < 0)
                                        {
                                            if (TargetCommand.Index == -1)
                                                quit = true;
                                            else if (TargetCommand.Index == -2)
                                                back = true;
                                            continue;
                                        }
                                        else 
                                        {
                                            commandsPortal.RunGenericMethod(TargetCommand);
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    quit = true;
                }
            }

            if (connected)
            {
                softatalib.SendMessageCmd("End");
                Thread.Sleep(500);
            }
            
            return;
        }

        private static byte GetGenericCmdIndex(string cmd, string[] GenericCmds)
        {
            byte subCmd = 0;
            for (int i=0;i< GenericCmds.Length;i++)
            {
                if (GenericCmds[i].ToLower().Contains(cmd.ToLower()))
                {
                    subCmd = (byte)i;
                    break;
                }
            }
            return subCmd;
        }

        /// <summary>
        /// Get dictionaly list of similar commands so can be used in a menu
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="GenericCmds"></param>
        /// <returns></returns>
        private static Dictionary<int,string>GetListGenericCmds(string cmd, string[] GenericCmds)
        {
            Dictionary<int, string> cmds = new Dictionary<int, string>();
            for (int i = 0; i < GenericCmds.Length; i++)
            {
                if (GenericCmds[i].ToLower().Contains(cmd.ToLower()))
                {
                    cmds.Add(i, GenericCmds[i]);
                }
            }
            return cmds;
        }

        private static byte GetuseGenericCmdIndex(string cmd, Dictionary<int, string> useGenericCmds)
        {
            byte subCmd = 0;
            foreach (var genCmd in useGenericCmds)
            {
                if (genCmd.Value.ToLower().Contains(cmd.ToLower()))
                {
                    subCmd = (byte)genCmd.Key;
                    break;
                }
            }
            return subCmd;
        }

  
    }
} 


                               