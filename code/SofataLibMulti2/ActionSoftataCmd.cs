using ConsoleTextFormat;
using Softata.Enums;
using Softata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.Security.Cryptography;



namespace   Softata.ActionCommands
{
    public class SelectedDeviceLoopVars2 : IDisposable
    {
        public Tuple<byte, byte, byte> rgb { get; set; } = new Tuple<byte, byte, byte>(0x40, 0, 0);
        public byte imisc { get; set; } = 0xff;
        public byte misc_led { get; set; } = 0;
        public byte misc_bglevel { get; set; } = 0;
        public byte misc_brightness { get; set; } = 1;
        public byte misc_num { get; set; } = 0;
        public Tuple<int, int>? deviceDataRange { get; set; } = null;

        public bool foundRange { get; set; } = false;
        // bool isRelay { get; set; } = false;
        //public bool isQuadRelay { get; set; } = false;
        public byte relay_bit_no { get; set; } = 0;
        public void Dispose()
        {

        }

    }


    public class CommandsPortal
    {
        /////////////////////////////////////////////////////////////////////////////////////////////
        
        // Call this once for the app
        public static void Setup(
            ILLayout llayout,
            Func<string, string[],byte> getGenericCmdIndex, 
            SoftataLib _softatalib)
        {
            LLayout = llayout;
            GetGenericCmdIndex = getGenericCmdIndex;
            softatalib = _softatalib;
        }

        public static ILLayout LLayout { get; set; }

        public static Func<string, string[], byte> GetGenericCmdIndex { get; set; }
        public static SoftataLib softatalib { get; set; }

        public SelectedDeviceLoopVars2 selectedDeviceLoopVars { get; set; }


        /////////////////////////////////////////////////////////////////////////////////////////////

        // Construct with new device
        public CommandsPortal(string[] genericCommands, Dictionary<int, string> _useGenericCommands, List<string> _stringList,
            Selection targetDeviceType, Selection targetDevice, byte _linkedListNo, int _capabilities)
        {
            GenericCommands = genericCommands;
            useGenericCommands = _useGenericCommands;
            stringlist = _stringList;

            TargetDeviceType = targetDeviceType;
            TargetDevice = targetDevice;
            linkedListNo = _linkedListNo;
            capabilities = _capabilities;

            selectedDeviceLoopVars = new SelectedDeviceLoopVars2();
            sensorProperties = null;
        }
        public string[] GenericCommands { get; set; }
        public Dictionary<int, string> useGenericCommands { get; set; }
        public Selection TargetDeviceType { get; set; }
        public Selection TargetDevice { get; set; }
        public List<string> stringlist { get; set; }
        public byte linkedListNo { get; set; }
        public int capabilities { get; set; } //Used by both Actuators and DeviceInputs
        public List<string>? sensorProperties { get; set; } 
        public int Num_Bits { get; set; }

        //////////////////////////////////////////////////////////////////////////////////////////
        public string RunGenericMethod(Selection TargetCommand)
        {
            using (this.selectedDeviceLoopVars  )
            {
                string command = TargetCommand.Item.ToLower();
                Selection TargetMiscCmd = new Selection(0);
                byte[]? data = new byte[0];
                bool isMiscCMd = false;
                byte pinn = 0;
                string result = "";
                bool quit = false;
                bool back = false;
                ///////////////////////////////////
                LLayout.Info($"Selected target device command : ", $" {TargetCommand.Item}");
                if (TargetDeviceType.Item.ToLower() == "deviceinput")
                {
                    if (!selectedDeviceLoopVars.foundRange) // Only get range once
                    {
                        byte subCmd = GetGenericCmdIndex("getNumBits", GenericCommands);
                        result = softatalib.SendTargetCommand((byte)TargetDeviceType.Index, (byte)1, (byte)subCmd, (byte)0xff, linkedListNo, null);
                        if (int.TryParse(result, out int numBits))
                        {
                            Num_Bits = numBits;
                            if (numBits > 1)
                            {
                                //selectedDeviceLoopVars.isQuadRelay = true;
                                selectedDeviceLoopVars.foundRange = true;
                                selectedDeviceLoopVars.deviceDataRange = new Tuple<int, int>(0, (2 << (numBits - 1)) - 1);
                            }
                            else if (numBits == 1)
                            {
                                //selectedDeviceLoopVars.isRelay = true;
                                selectedDeviceLoopVars.foundRange = true;
                            }
                            else if (numBits == -1)
                            {
                                //selectedDeviceLoopVars.isRelay = false;
                            }
                        }

                        if (!selectedDeviceLoopVars.foundRange)
                        {
                            // Format XXX:min...max<space>qwwqeqsdsfcfq
                            subCmd = GetGenericCmdIndex("getinstanceValueRange", GenericCommands);//Was getvaluerange=05
                            // result = softatalib.SendTargetCommand((byte)TargetCommand.Index, 1, subCmd, (byte)TargetDevice.Index);
                            result = softatalib.SendTargetCommand((byte)TargetDeviceType.Index, 1, subCmd, (byte)TargetDevice.Index);
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
                                }
                                if (result.Contains("..."))
                                {
                                    string[] range = result.Split("...");
                                    if (range.Length == 2)

                                        if (int.TryParse(range[0], out int val1))
                                        {
                                            if (int.TryParse(range[1], out int val2))
                                            {
                                                selectedDeviceLoopVars.deviceDataRange = new Tuple<int, int>(val1, val2);
                                                selectedDeviceLoopVars.foundRange = true;
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
                                (command.ToLower().Contains("GetInputCapab".ToLower()))
                        )
                    {
                        // No further info required
                    }
                    else if (                        
                        (capabilities == (int)(DeviceInputCapabilities.i_bit | DeviceInputCapabilities.i_readbyte)) ||
                         (capabilities == (int)(DeviceInputCapabilities.i_bit | DeviceInputCapabilities.i_readbyte | DeviceInputCapabilities.i_readword))
                         )


                    {
                        // DeviceInputs that can read a value or read bits
                        Console.WriteLine("Mega");

                        if (command.ToLower().Contains("Bit".ToLower()))
                        {
                            Console.WriteLine("Poll");
                            selectedDeviceLoopVars.relay_bit_no = (byte)LLayout.Prompt4Num(selectedDeviceLoopVars.relay_bit_no + 1, Num_Bits, false);
                            byte[] bytes = new byte[] { selectedDeviceLoopVars.relay_bit_no };
                            data = data.Concat(bytes).ToArray<byte>();
                        }
                        else if (command.ToLower().Contains("Read".ToLower()))
                        {
                            Console.WriteLine("Read");
                        }
                        else
                        {
                            Console.WriteLine("Other");
                            selectedDeviceLoopVars.relay_bit_no = (byte)LLayout.Prompt4Num(selectedDeviceLoopVars.relay_bit_no + 1, Num_Bits, false);
                            byte[] bytes = new byte[] { selectedDeviceLoopVars.relay_bit_no };
                            data = data.Concat(bytes).ToArray<byte>();
                        }
                    }
                    else if (capabilities == (int)(DeviceInputCapabilities.i_readbyte))
                    {
                        // Actuators that can be byte written to only (No bit manipluations)
                        if (command.ToLower().Contains("bit".ToLower()))
                        {
                            // Not implemented
                            LLayout.Info("Bit commands not implemented for this device.");
                            byte dummy = 0;
                            byte[] bytes = new byte[] { dummy };
                            data = data.Concat(bytes).ToArray<byte>();
                        }
                        else
                        {

                        }
                    }
                    else if (capabilities == (int)(DeviceInputCapabilities.i_bit))
                    {
                        // DeviceInputs that can only be bit read 
                        if (command.ToLower().Contains("bit".ToLower()))
                        {
                            selectedDeviceLoopVars.relay_bit_no = (byte)LLayout.Prompt4Num(selectedDeviceLoopVars.relay_bit_no + 1, Num_Bits, false);
                            byte[] bytes = new byte[] { selectedDeviceLoopVars.relay_bit_no };
                            data = data.Concat(bytes).ToArray<byte>();
                        }
                        else
                        {
                            // Not implemented
                            LLayout.Info("Write commands not implemented for this device.");
                            byte[] bytes = new byte[] { (byte)0, (byte)1 };
                            data = data.Concat(bytes).ToArray<byte>();
                        }

                    }
                    else if (capabilities == (int)DeviceInputCapabilities.i_none)
                    {
                        // Nothing to add. Single bit
                        // Non single bit commands will return Not Implemented.
                        // Needs a dummy pin/bit number.
                        byte dummy = 0;
                        byte[] bytes = new byte[] { dummy };
                        data = data.Concat(bytes).ToArray<byte>();
                    }
                    else
                    {
                        // ?? 
                    }
                }
                else   if (TargetDeviceType.Item.ToLower() == "actuator")
                {
                    if (!selectedDeviceLoopVars.foundRange) // Only get range once
                    {
                        byte subCmd = GetGenericCmdIndex("getNumBits", GenericCommands);
                        result = softatalib.SendTargetCommand((byte)TargetDeviceType.Index, (byte)1, (byte)subCmd, (byte)0xff, linkedListNo, null);
                        if (int.TryParse(result, out int numBits))
                        {
                            Num_Bits = numBits;
                            if (numBits > 1)
                            {
                                //selectedDeviceLoopVars.isQuadRelay = true;
                                selectedDeviceLoopVars.foundRange = true;
                                selectedDeviceLoopVars.deviceDataRange = new Tuple<int, int>(0, (2 << (numBits - 1)) - 1);
                            }
                            else if (numBits == 1)
                            {
                                //selectedDeviceLoopVars.isRelay = true;
                                selectedDeviceLoopVars.foundRange = true;
                            }
                            else if (numBits == -1)
                            {
                                //selectedDeviceLoopVars.isRelay = false;
                            }
                        }

                        if (!selectedDeviceLoopVars.foundRange)
                        {
                            // Format XXX:min...max<space>qwwqeqsdsfcfq
                            subCmd = GetGenericCmdIndex("getinstanceValueRange", GenericCommands);//Was getvaluerange=05
                            // result = softatalib.SendTargetCommand((byte)TargetCommand.Index, 1, subCmd, (byte)TargetDevice.Index);
                            result = softatalib.SendTargetCommand((byte)TargetDeviceType.Index, 1, subCmd, (byte)TargetDevice.Index);
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
                                }
                                if (result.Contains("..."))
                                {
                                    string[] range = result.Split("...");
                                    if (range.Length == 2)

                                        if (int.TryParse(range[0], out int val1))
                                        {
                                            if (int.TryParse(range[1], out int val2))
                                            {
                                                selectedDeviceLoopVars.deviceDataRange = new Tuple<int, int>(val1, val2);
                                                selectedDeviceLoopVars.foundRange = true;
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
                    else if (capabilities ==(int)(ActuatorCapabilities.a_bit | ActuatorCapabilities.a_writebyte))
                    {
                        // Actuators that can write value to manipulate bits
                        if (command.ToLower().Contains("Write".ToLower()))
                        {
                            if (selectedDeviceLoopVars.deviceDataRange != null)
                            {
                                //Layout.Info("Found range: ", $"{selectedDeviceLoopVars.actuatorRange.Item1}...{selectedDeviceLoopVars.actuatorRange.Item2}");

                                int num = LLayout.Prompt4IntInRange(selectedDeviceLoopVars.deviceDataRange.Item1, selectedDeviceLoopVars.deviceDataRange.Item2);
                                byte[] bytes  = new byte[] { (byte)num };
                                data = data.Concat(bytes).ToArray<byte>();
                            }
                            else
                            {
                                LLayout.Info("Actuator write needs a valid range");
                                return "Actuator write needs a valid range";
                            }
                        }
                        else if (command.ToLower().Contains("SetBitState".ToLower()))
                        {
                            ////int x = selectedDeviceLoopVars.n
                            bool istate = LLayout.Prompt4Bool();
                            selectedDeviceLoopVars.relay_bit_no = (byte)LLayout.Prompt4Num(selectedDeviceLoopVars.relay_bit_no+1 , Num_Bits, false);
                            byte[] bytes = new byte[] { selectedDeviceLoopVars.relay_bit_no, istate ? (byte)1 : (byte)0 };
                            data = data.Concat(bytes).ToArray<byte>();
                        }
                        else
                        {
                            selectedDeviceLoopVars.relay_bit_no = (byte)LLayout.Prompt4Num(selectedDeviceLoopVars.relay_bit_no+1 , Num_Bits, false);
                            byte[] bytes = new byte[] { selectedDeviceLoopVars.relay_bit_no };
                            data = data.Concat(bytes).ToArray<byte>();
                        }
                    }
                    else if (capabilities == (int)(ActuatorCapabilities.a_writebyte))
                    {
                        // Actuators that can be byte written to only (No bit manipluations)
                        if (command.ToLower().Contains("bit".ToLower()))
                        {
                            // Not implemented
                            LLayout.Info("Bit commands not implemented for this device.");
                            byte dummy = 0;
                            byte[] bytes = new byte[] { dummy };
                            data = data.Concat(bytes).ToArray<byte>();
                        }
                        else
                        {
                            if (selectedDeviceLoopVars.deviceDataRange != null)
                            {
                                LLayout.Info("Found range: ", $"{selectedDeviceLoopVars.deviceDataRange.Item1}...{selectedDeviceLoopVars.deviceDataRange.Item2}");

                                int num = LLayout.Prompt4IntInRange(selectedDeviceLoopVars.deviceDataRange.Item1, selectedDeviceLoopVars.deviceDataRange.Item2);
                                byte[] bytes = new byte[] { (byte)num };
                                data = data.Concat(bytes).ToArray<byte>();
                            }
                            else
                            {
                                LLayout.Info("Actuator write needs a valid range");
                                return "Actuator write needs a valid range";
                            }
                        }
                    }
                    else if (capabilities == (int)(ActuatorCapabilities.a_bit))
                    {
                        // Actuators that can only be bit manipluated
                        if (command.ToLower().Contains("bit".ToLower()))
                        {
                            selectedDeviceLoopVars.relay_bit_no = (byte)LLayout.Prompt4Num(selectedDeviceLoopVars.relay_bit_no+1 , Num_Bits, false);
                            byte[] bytes = new byte[] { selectedDeviceLoopVars.relay_bit_no };
                            data = data.Concat(bytes).ToArray<byte>();
                        }
                        else
                        {
                            // Not implemented
                            LLayout.Info("Write commands not implemented for this device.");
                            byte[] bytes = new byte[] { (byte)0, (byte)1 };
                            data = data.Concat(bytes).ToArray<byte>();
                        }

                    }
                    else if (capabilities == (int)ActuatorCapabilities.a_none)
                    {
                        if (command.ToLower().Contains("SetBitState".ToLower()))
                        {
                            ////int x = selectedDeviceLoopVars.n
                            bool istate = LLayout.Prompt4Bool();
                            selectedDeviceLoopVars.relay_bit_no = (byte)LLayout.Prompt4Num(selectedDeviceLoopVars.relay_bit_no + 1, Num_Bits, false);
                            byte[] bytes = new byte[] { selectedDeviceLoopVars.relay_bit_no, istate ? (byte)1 : (byte)0 };
                            data = data.Concat(bytes).ToArray<byte>();
                        }
                        else
                        {
                            // Nothing to add. Single bit
                            // Non single bit commands will return Not Implemented.
                            // Needs a dummy pin/bit number.
                            byte dummy = 0;
                            byte[] bytes = new byte[] { dummy };
                            data = data.Concat(bytes).ToArray<byte>();
                        }
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
                            byte subCmd = GetGenericCmdIndex("properties", GenericCommands);
                            result = softatalib.SendTargetCommand((byte)TargetDeviceType.Index, 1, subCmd, (byte)TargetDevice.Index);
                            sensorProperties = new List<string>(result.Split(","));
                        }
                        if ((command.ToLower().Contains("readall")) || (command.ToLower().Contains("gettelemetry")))
                        {
                            Layout.Info("Getting: ", string.Join(",", sensorProperties));
                        }
                        else if (command.ToLower().Contains("read"))
                        {
                            var seln = Layout.PromptWithCSVList(0, string.Join(",", sensorProperties), true, true);
                            pinn = (byte)seln.Index;
                            if (pinn < 0)
                            {
                                if (pinn == -1)
                                    quit = true;
                                else if (pinn == -2)
                                    back = true;
                                return "";
                            }
                            Layout.Info("Getting: ", sensorProperties[pinn]);
                        }
                    
                }
                else if (TargetDeviceType.Item.ToLower() == "display")
                {

                    int line = 0;
                    int pos = 0;
                    string? message = "";

                    if ((command.ToLower().Contains("cursor")) && (command.ToLower().Contains("writestring")))
                    {
                        LLayout.Info("Enter a CSV string:Line 0 or 1,line position  0 to 39");
                        string maxes = "1,39";
                        List<int> vals = LLayout.Prompt4NumswithMaxesandText(2, maxes, out message, true);
                        line = vals[0]; //1 or 2
                        pos = vals[1]; //1 ... 40
                        byte[] bytes = new byte[] { (byte)pos, (byte)line };
                        data = data.Concat(bytes).ToArray<byte>();
                        if (!string.IsNullOrEmpty(message))
                        {
                            byte[] bytes2 = Encoding.ASCII.GetBytes(message);
                            data = data.Concat(bytes2).ToArray<byte>();
                        }
                    }
                    else if (command.ToLower().Contains("cursor"))
                    {
                        //case GroveDisplayCmds.setC +1ursor:
                        LLayout.Info("Enter a CSV string:Line 0 or 1,line position  0 to 39");
                        string maxes = "1,39";
                        List<int> vals = LLayout.Prompt4NumswithMaxesandText(2,maxes, out message, false);
                        line = vals[0] ; //1 or 2
                        pos = vals[1]  ; //1 ... 40
                        byte[] bytes = new byte[] { (byte)pos, (byte)line };
                        data = data.Concat(bytes).ToArray<byte>();
                    }
                    else if (command.ToLower().Contains("writestring"))
                    {
                        message = LLayout.Prompt4String("Enter text to display");
                        if (!string.IsNullOrEmpty(message))
                        {
                            byte[] bytes = Encoding.ASCII.GetBytes(message);
                            data = data.Concat(bytes).ToArray<byte>();
                        }
                    }
                    else if (command.ToLower().Contains("misc"))
                    {
                        // Misc commands are quite specific to the device (display) so hard to make generic
                        isMiscCMd = true;
                        SoftataLib.Display softatalibDisplay = new SoftataLib.Display(softatalib);

                        SoftataLib.Display.Neopixel? softataLibDisplayNeopixel = null;
                        SoftataLib.Display.BARGRAPHDisplay? softataLibDisplayBargraphDisplay = null;
                        //SoftataLib.Display.BARGRAPHDisplay softataLibDisplayGBargraphDisplay;
                        SoftataLib.Display.LCD1602Display? softataLibDisplayLCD1602Display = null;
                        //SoftataLib.Display.Oled096? softataLibDisplayOled096 = null;
                        byte subCmd = GetGenericCmdIndex("miscgetlist", GenericCommands);
                        string MiscCmds = softatalib.SendTargetCommand((byte)TargetDeviceType.Index, 1, subCmd, (byte)TargetDevice.Index);
                        TargetMiscCmd = LLayout.PromptWithCSVList(TargetMiscCmd.Index, MiscCmds, true, true);

                       
                        int res = TargetMiscCmd.Index;
                        if (res < 0)
                            return "";
                        selectedDeviceLoopVars.imisc = (byte)(1 + res);
                        LLayout.Info($"{TargetMiscCmd.Item} chosen");
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
                                        softataLibDisplayBargraphDisplay = new SoftataLib.Display.BARGRAPHDisplay(softatalib);
                                    obj = softataLibDisplayBargraphDisplay;
                                    if (TargetMiscCmd.Item.ToLower().Contains("_led"))
                                    {
                                        selectedDeviceLoopVars.misc_led = (byte)LLayout.Prompt4Num(selectedDeviceLoopVars.misc_led + 1, 10, false);
                                        parameter = selectedDeviceLoopVars.misc_led;
                                    }
                                    else if (TargetMiscCmd.Item.ToLower().Contains("setlevel"))
                                    {
                                        selectedDeviceLoopVars.misc_bglevel = (byte)LLayout.Prompt4Num(selectedDeviceLoopVars.misc_bglevel + 1, 10, false);
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

                                            selectedDeviceLoopVars.misc_led = (byte)LLayout.Prompt4Num(selectedDeviceLoopVars.misc_led + 1, 8, false);
                                            paramz = new object[] { linkedListNo, selectedDeviceLoopVars.rgb.Item1, selectedDeviceLoopVars.rgb.Item2, selectedDeviceLoopVars.rgb.Item3, selectedDeviceLoopVars.misc_led };
                                        }
                                        else
                                        {
                                            paramz = new object[] { linkedListNo, selectedDeviceLoopVars.rgb.Item1, selectedDeviceLoopVars.rgb.Item2, selectedDeviceLoopVars.rgb.Item3 };
                                        }
                                    }
                                    else if (TargetMiscCmd.Item.ToLower().Contains("setbrightness"))
                                    {
                                        selectedDeviceLoopVars.misc_brightness = (byte)LLayout.Prompt4Num(selectedDeviceLoopVars.misc_brightness + 1, 9, false);

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
                                        LLayout.Prompt("Select num 1...9 which maps to 0 ..8", "");
                                        Console.WriteLine();
                                        selectedDeviceLoopVars.misc_num = (byte)LLayout.Prompt4Num(selectedDeviceLoopVars.misc_num + 1, 9, false);
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

                    LLayout.Info(response);

                    if (TargetDeviceType.Item.ToLower() == "actuator")
                    {
                    }
                    else if (TargetDeviceType.Item.ToLower() == "display")
                    {
                    }
                    else if (TargetDeviceType.Item.ToLower() == "sensor")
                    {
                    }
                    else if (TargetDeviceType.Item.ToLower() == "display")
                    {
                    }
  
                    return response;
                }
            }
            return "";
        }
    }
    
}

