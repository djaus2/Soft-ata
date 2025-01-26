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
        public Tuple<int, int>? actuatorRange { get; set; } = null;

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


                                    byte num_bits = 4;
                                    num_bits = (byte)Layout.Prompt4IntInRange(1, 8, num_bits);
                                    Layout.Info($"Using NonDefault Setup: {TargetPin.Item} Num Bits:{num_bits}");

                                    subCmd = GetGenericCmdIndex("setupgeneral", GenericCommands);
                                    byte[] data = new byte[] { num_bits };
                                    result = softatalib.SendTargetCommand(cmdTarget, (byte)TargetPin.Index, subCmd, (byte)TargetDevice.Index,0xff,data);

                                }
                                cmdTarget = (byte)TargetDeviceType.Index;
                                
                                /*
                                // Select from default and more general setup.  Default is Default!
                                Dictionary<int,string> setups = GetListGenericCmds("setup", GenericCommands);
                                subCmd = GetGenericCmdIndex("setupdefault", GenericCommands);
                                Selection setupCommand = Layout.PromptWithDictionaryList(subCmd, setups, true, true);
                                byte setupCmd = (byte)setupCommand.Index;
                                string result = "";
                                if(subCmd == setupCmd)
                                {
                                    result = softatalib.SendTargetCommand(cmdTarget, 1, setupCmd, (byte)TargetDevice.Index);
                                }
                                else
                                {
                                    byte pinnn = 16;
                                    byte num_bits = 4;
                                    L.Info("Enter Pin Number: ");
                                    pinnn = (byte)L.Prompt4Num(pinnn, 16, false);
                                    L.Info("Enter Number of Bits: ");
                                    num_bits = (byte)L.Prompt4Num(num_bits,4, false);
                                    byte[] data = new byte[num_bits];
                                    result = softatalib.SendTargetCommand(cmdTarget, pinnn, setupCmd, (byte)TargetDevice.Index,0xff,data);
                                }

                                //subCmd = GetGenericCmdIndex("setupdefault", GenericCommands);
                                */

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
                                int actuatorcapabilities = (int)ActuatorCapabilities.a_none;

                                if (TargetDeviceType.Item.ToLower() == "actuator")
                                {
                                    subCmd = GetGenericCmdIndex("GetActuatorCapabiliti", GenericCommands);
                                    string response = softatalib.SendTargetCommand((byte)TargetDeviceType.Index, (byte)pinn, (byte)subCmd, (byte)0xff, linkedListNo);
                               
                                    if(int.TryParse(response, out int capabilities))
                                    {
                                        actuatorcapabilities = capabilities;
                                    }
                                }


                                using (SelectedDeviceLoopVars selectedDeviceLoopVars = new SelectedDeviceLoopVars())
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
                                            string command = TargetCommand.Item.ToLower();
                                            byte[]? data = new byte[0];
                                            bool isMiscCMd = false;
                                            ///////////////////////////////////
                                            Layout.Info($"Selected target device command : ", $" {TargetCommand.Item}");
                                            if (TargetDeviceType.Item.ToLower() == "actuator")
                                            {
                                                if (!selectedDeviceLoopVars.foundRange) // Only get range once
                                                {
                                                    subCmd = GetGenericCmdIndex("getNumBits", GenericCommands);           
                                                    result = softatalib.SendTargetCommand((byte)TargetDeviceType.Index, (byte)1, (byte)subCmd, (byte)0xff, linkedListNo,null);
                                                    if (int.TryParse(result, out int numBits))
                                                    {
                                                        if (numBits > 1)
                                                        {
                                                            //selectedDeviceLoopVars.isQuadRelay = true;
                                                            selectedDeviceLoopVars.foundRange = true;
                                                            selectedDeviceLoopVars.actuatorRange = new Tuple<int, int>(0, (2<<(numBits-1))-1);
                                                            }
                                                        else if (numBits == 1)
                                                        {
                                                            //selectedDeviceLoopVars.isRelay = true;
                                                            selectedDeviceLoopVars.foundRange = true;
                                                        }
                                                        else if(numBits==-1)
                                                        {
                                                            //selectedDeviceLoopVars.isRelay = false;
                                                        }
                                                    }

                                                    if (!selectedDeviceLoopVars.foundRange)
                                                    {
                                                        // Format XXX:min...max<space>qwwqeqsdsfcfq
                                                        subCmd = GetGenericCmdIndex("getValueRange", GenericCommands);
                                                        result = softatalib.SendTargetCommand(cmdTarget, 1, subCmd, (byte)TargetDevice.Index);
                                                        if (result.ToLower().Contains("..."))
                                                        {
                                                            if (result.ToLower().Contains(":"))
                                                            {
                                                                int indx = result.IndexOf(':');
                                                                result = result.Substring(indx + 1);
                                                            }
                                                            if (result.Contains(' '))
                                                            {
                                                                int indx = result.IndexOf(' ');
                                                                result = result.Substring(0, indx);
                                                                if (result.Contains("..."))
                                                                {
                                                                    string[] range = result.Split("...");
                                                                    if (range.Length == 2)

                                                                        if (int.TryParse(range[0], out int val1))
                                                                        {
                                                                            if (int.TryParse(range[1], out int val2))
                                                                            {
                                                                                selectedDeviceLoopVars.actuatorRange = new Tuple<int, int>(val1, val2);
                                                                                selectedDeviceLoopVars.foundRange = true;
                                                                            }
                                                                        }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                if (
                                                         (command.ToLower().Contains("getvaluerange".ToLower())) ||
                                                         (command.ToLower().Contains("getnumbits".ToLower())) ||
                                                         (command.ToLower().Contains("getinstancevaluerange".ToLower())) ||                                                      
                                                         (command.ToLower().Contains("GetActuatorCapabiliti".ToLower()))
                                                    )
                                                {
                                                    // No further info required
                                                }
                                                else if (actuatorcapabilities ==  (int)(ActuatorCapabilities.a_bit & ActuatorCapabilities.a_writebyte))
                                                {
                                                    // Actuators that can write value to manipulate bits
                                                        if (command.ToLower().Contains("Write".ToLower()))
                                                        {
                                                            if (selectedDeviceLoopVars.actuatorRange != null)
                                                            {
                                                                //Layout.Info("Found range: ", $"{selectedDeviceLoopVars.actuatorRange.Item1}...{selectedDeviceLoopVars.actuatorRange.Item2}");

                                                                int num = Layout.Prompt4IntInRange(selectedDeviceLoopVars.actuatorRange.Item1, selectedDeviceLoopVars.actuatorRange.Item2);
                                                                data = new byte[] { (byte)num };
                                                            }
                                                            else
                                                            {
                                                                Layout.Info("Actuator write needs a valid range");
                                                                continue;
                                                            }
                                                        }
                                                        else if (command.ToLower().Contains("SetBitState".ToLower()))
                                                        {
                                                            bool istate = Layout.Prompt4Bool();
                                                            selectedDeviceLoopVars.relay_bit_no = (byte)Layout.Prompt4Num(selectedDeviceLoopVars.relay_bit_no + 1, 4, false);
                                                            data = new byte[] { selectedDeviceLoopVars.relay_bit_no, istate ? (byte)1 : (byte)0 };
                                                        }
                                                        else
                                                        {
                                                            selectedDeviceLoopVars.relay_bit_no = (byte)Layout.Prompt4Num(selectedDeviceLoopVars.relay_bit_no + 1, 4, false);
                                                            data = new byte[] { selectedDeviceLoopVars.relay_bit_no };
                                                        }
                                                }
                                                else if (actuatorcapabilities == (int)(ActuatorCapabilities.a_writebyte))
                                                {
                                                    // Actuators that can be byte written to only (No bit manipluations)
                                                    if (command.ToLower().Contains("bit".ToLower()))
                                                    {
                                                        // Not implemented
                                                        L.Info("Bit commands not implemented for this device.");
                                                        byte dummy = 0;
                                                        data = new byte[] {dummy};
                                                    }
                                                    else 
                                                    {
                                                        if (selectedDeviceLoopVars.actuatorRange != null)
                                                        {
                                                            Layout.Info("Found range: ", $"{selectedDeviceLoopVars.actuatorRange.Item1}...{selectedDeviceLoopVars.actuatorRange.Item2}");

                                                            int num = Layout.Prompt4IntInRange(selectedDeviceLoopVars.actuatorRange.Item1, selectedDeviceLoopVars.actuatorRange.Item2);
                                                            data = new byte[] { (byte)num };
                                                        }
                                                        else
                                                        {
                                                            Layout.Info("Actuator write needs a valid range");
                                                            continue;
                                                        }
                                                    }
                                                }
                                                else if(actuatorcapabilities == (int)(ActuatorCapabilities.a_bit))
                                                {
                                                    // Actuators that can only be bit manipluated
                                                    if (command.ToLower().Contains("bit".ToLower()))
                                                    {
                                                        selectedDeviceLoopVars.relay_bit_no = (byte)Layout.Prompt4Num(selectedDeviceLoopVars.relay_bit_no + 1, 4, false);
                                                        data = new byte[] { selectedDeviceLoopVars.relay_bit_no };
                                                    }
                                                    else
                                                    {
                                                        // Not implemented
                                                        L.Info("Write commands not implemented for this device.");
                                                        data = new byte[] { (byte)0, (byte)1 };
                                                    }

                                                }
                                                else if (actuatorcapabilities == (int)ActuatorCapabilities.a_none)
                                                {
                                                    // Nothing to add. Single bit
                                                    // Non single bit commands will return Not Implemented.
                                                    // Needs a dummy pin/bit number.
                                                    byte dummy = 0;
                                                    data = new byte[] { dummy };
                                                }
                                                else
                                                {
                                                    // ?? 
                                                }
                                            }
                                            else if (TargetDeviceType.Item.ToLower() == "sensor")
                                            {
                                                if (sensorProperties == null)
                                                {
                                                    subCmd = GetGenericCmdIndex("properties", GenericCommands);
                                                    result = softatalib.SendTargetCommand(cmdTarget, 1, subCmd, (byte)TargetDevice.Index);
                                                    sensorProperties = new List<string>(result.Split(","));
                                                }
                                                if ((command.ToLower().Contains("readall")) || (command.ToLower().Contains("gettelemetry")))
                                                {
                                                    Layout.Info("Getting: ", result);
                                                }
                                                else if (command.ToLower().Contains("read"))
                                                {
                                                    var seln = Layout.PromptWithCSVList(0, result, true, true);
                                                    pinn = seln.Index;
                                                    if (pinn < 0)
                                                    {
                                                        if (pinn == -1)
                                                            quit = true;
                                                        else if (pinn == -2)
                                                            back = true;
                                                        continue;
                                                    }
                                                    Layout.Info("Getting: ", sensorProperties[pinn]);
                                                }
                                            }
                                            else if (TargetDeviceType.Item.ToLower() == "display")
                                            {

                                                int line = 0;
                                                int pos = 0;
                                                string? message = "";

                                                if (command.ToLower().Contains("cursor"))
                                                {
                                                    //case GroveDisplayCmds.setCursor:
                                                    L.Info("Enter line 1 or 2");
                                                    line = L.Prompt4Num(line, 2, false);
                                                    L.Info("Enter line position  1 to 40");
                                                    pos = L.Prompt4Num(pos, 40, false);
                                                    data = new byte[] { 0x2, (byte)pos, (byte)line };
                                                }
                                                if (command.ToLower().Contains("writestring"))
                                                {
                                                    message = Console.ReadLine();
                                                    if (!string.IsNullOrEmpty(message))
                                                    {
                                                        byte[] bytes = Encoding.ASCII.GetBytes(message);
                                                        data = data.Concat(bytes).ToArray<byte>();
                                                    }
                                                }
                                                if (command.ToLower().Contains("misc"))
                                                {
                                                    // Misc commands are quite specific to the device (display) so hard to make generic
                                                    isMiscCMd = true;
                                                    SoftataLib.Display softatalibDisplay = new SoftataLib.Display(softatalib);

                                                    SoftataLib.Display.Neopixel? softataLibDisplayNeopixel = null;
                                                    SoftataLib.Display.BARGRAPHDisplay? softataLibDisplayBargraphDisplay = null;
                                                    //SoftataLib.Display.BARGRAPHDisplay softataLibDisplayGBargraphDisplay;
                                                    SoftataLib.Display.LCD1602Display? softataLibDisplayLCD1602Display = null;
                                                    //SoftataLib.Display.Oled096? softataLibDisplayOled096 = null;
                                                    subCmd = GetGenericCmdIndex("misc", GenericCommands);
                                                    string MiscCmds = softatalib.SendTargetCommand(cmdTarget, 1, subCmd, (byte)TargetDevice.Index);
                                                    TargetMiscCmd = Layout.PromptWithCSVList(TargetMiscCmd.Index, MiscCmds, true, true);

                                                    int res = TargetMiscCmd.Index;
                                                    if (res < 0)
                                                        break;
                                                    selectedDeviceLoopVars.imisc = (byte)(1 + res);
                                                    L.Info($"{TargetMiscCmd.Item} chosen");
                                                    {
                                                        bool misc_done = false;
                                                        Type? type = null;
                                                        Object? obj = null;
                                                        object[]? paramz = null;
                                                        byte parameter = 0xff;

                                                        switch ((DisplayDevice)TargetDevice.Index)
                                                        {
                                                            case DisplayDevice.LCD1602:
                                                                if (softataLibDisplayLCD1602Display == null)
                                                                    softataLibDisplayLCD1602Display = new SoftataLib.Display.LCD1602Display(softatalib);
                                                                obj = softataLibDisplayLCD1602Display;
                                                                break;
                                                            case DisplayDevice.GBARGRAPH:
                                                                if (softataLibDisplayBargraphDisplay == null)
                                                                    softataLibDisplayBargraphDisplay = new Display.BARGRAPHDisplay(softatalib);
                                                                obj = softataLibDisplayBargraphDisplay;
                                                                if (TargetMiscCmd.Item.ToLower().Contains("_led"))
                                                                {
                                                                    selectedDeviceLoopVars.misc_led = (byte)Layout.Prompt4Num(selectedDeviceLoopVars.misc_led + 1, 10, false);
                                                                    parameter = selectedDeviceLoopVars.misc_led;
                                                                }
                                                                else if (TargetMiscCmd.Item.ToLower().Contains("setlevel"))
                                                                {
                                                                    selectedDeviceLoopVars.misc_bglevel = (byte)Layout.Prompt4Num(selectedDeviceLoopVars.misc_bglevel + 1, 10, false);
                                                                    parameter = (byte)(selectedDeviceLoopVars.misc_bglevel + 1);
                                                                }
                                                                break;
                                                            case DisplayDevice.NEOPIXEL:
                                                                if (softataLibDisplayNeopixel == null)
                                                                {
                                                                    softataLibDisplayNeopixel = new SoftataLib.Display.Neopixel(softatalib);
                                                                }
                                                                obj = softataLibDisplayNeopixel;
                                                                if (TargetMiscCmd.Item.ToLower().Contains("setpixelcolor"))
                                                                {
                                                                    TargetMiscCmd.Item = Regex.Replace(TargetMiscCmd.Item, "setpixelcolor", "", RegexOptions.IgnoreCase);
                                                                    if (TargetMiscCmd.Item.ToLower() == "one")
                                                                    {
                                                                        selectedDeviceLoopVars.rgb = ConColors.SelectRGB();
                                                                        Console.WriteLine($"({selectedDeviceLoopVars.rgb.Item1},{selectedDeviceLoopVars.rgb.Item2},{selectedDeviceLoopVars.rgb.Item3})");

                                                                        selectedDeviceLoopVars.misc_led = (byte)Layout.Prompt4Num(selectedDeviceLoopVars.misc_led + 1, 8, false);
                                                                        paramz = new object[] { linkedListNo, selectedDeviceLoopVars.rgb.Item1, selectedDeviceLoopVars.rgb.Item2, selectedDeviceLoopVars.rgb.Item3, selectedDeviceLoopVars.misc_led };
                                                                    }
                                                                    else
                                                                    {
                                                                        paramz = new object[] { linkedListNo, selectedDeviceLoopVars.rgb.Item1, selectedDeviceLoopVars.rgb.Item2, selectedDeviceLoopVars.rgb.Item3 };
                                                                    }
                                                                }
                                                                else if (TargetMiscCmd.Item.ToLower().Contains("setbrightness"))
                                                                {
                                                                    selectedDeviceLoopVars.misc_brightness = (byte)Layout.Prompt4Num(selectedDeviceLoopVars.misc_brightness + 1, 9, false);

                                                                    Tuple<byte, byte, byte> rgbTemp = new Tuple<byte, byte, byte>(0, 0, 0);

                                                                    if (selectedDeviceLoopVars.misc_brightness != 0)
                                                                    {
                                                                        byte levl = (byte)(255 >> (selectedDeviceLoopVars.misc_brightness - 1));
                                                                        rgbTemp = new Tuple<byte, byte, byte>((byte)(selectedDeviceLoopVars.rgb.Item1 / levl), (byte)(selectedDeviceLoopVars.rgb.Item2 / levl), (byte)(selectedDeviceLoopVars.rgb.Item3 / levl));
                                                                    }
                                                                    softataLibDisplayNeopixel.Misc_SetAll(linkedListNo, rgbTemp.Item1, rgbTemp.Item2, rgbTemp.Item3);
                                                                    misc_done = true;
                                                                }
                                                                else if ((TargetMiscCmd.Item.ToLower().Contains("setnum"))
                                                                                                    ||
                                                                           (TargetMiscCmd.Item.ToLower().Contains("setpxl")))
                                                                {
                                                                    Layout.Prompt("Select num 1...9 which maps to 0 ..8", "");
                                                                    Console.WriteLine();
                                                                    selectedDeviceLoopVars.misc_num = (byte)Layout.Prompt4Num(selectedDeviceLoopVars.misc_num + 1, 9, false);
                                                                    paramz = new object[] { linkedListNo, selectedDeviceLoopVars.rgb.Item1, selectedDeviceLoopVars.rgb.Item2, selectedDeviceLoopVars.rgb.Item3, selectedDeviceLoopVars.misc_num };
                                                                }

                                                                else
                                                                {
                                                                    paramz = new object[] { linkedListNo, selectedDeviceLoopVars.rgb.Item1, selectedDeviceLoopVars.rgb.Item2, selectedDeviceLoopVars.rgb.Item3 };
                                                                }
                                                                break;
                                                        }

                                                        if ((obj != null) && (!misc_done))
                                                        {
                                                            type = obj.GetType();
                                                            if (type != null)
                                                            {
                                                                if (paramz == null)
                                                                {
                                                                    if (parameter != 0xff)
                                                                        paramz = new object[] { linkedListNo, parameter };
                                                                    else
                                                                        paramz = new object[] { linkedListNo };
                                                                }
                                                                MethodInfo[]? MiscMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                                                                if (MiscMethods != null)
                                                                {
                                                                    var miscMethod = MiscMethods.Where(m => m.Name.Contains(TargetMiscCmd.Item, StringComparison.OrdinalIgnoreCase));
                                                                    if (miscMethod != null)
                                                                    {
                                                                        if (miscMethod.Count() > 0)
                                                                        {
                                                                            miscMethod.First().Invoke(obj, paramz);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            if (!isMiscCMd)
                                            {
                                                if (data.Length == 0)
                                                {
                                                    data = null;
                                                }
                                                // Normally targetDeviceIndex is 0xff, means ignore it and use linkedListIndex, ie use instantiated index
                                                // For specific devicetype.device commands use its devicetype.device index.
                                                byte targetDeviceIndex = 0xff;
                                                if (stringlist.Any(s => s.Contains(TargetCommand.Item)))
                                                {
                                                    targetDeviceIndex = (byte)TargetDevice.Index;
                                                }
                                                string response = softatalib.SendTargetCommand((byte)TargetDeviceType.Index, (byte)pinn, (byte)TargetCommand.Index, targetDeviceIndex, linkedListNo, data);

                                                L.Info(response);

                                                if (TargetDeviceType.Item.ToLower() == "actuator")
                                                {
                                                }
                                                else if (TargetDeviceType.Item.ToLower() == "display")
                                                {
                                                }
                                                else if (TargetDeviceType.Item.ToLower() == "sensor")
                                                {
                                                }
                                            }
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


                               