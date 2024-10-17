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



namespace SoftataBasic
{
    internal partial class Program
    {
        const double PotMin = 0;
        const double PotMax = 100;
        const double LightMin = 0;
        const double LightMax = 61;
        const double SoundMin = 0;
        const double SoundMax = 100;
        const int numLoops = 20;
        // Set the same as Arduino:
        static int port = 4242;
        static string ipaddressStr = "192.168.0.12";

        static bool hasRunCalibrationOnce = false;

        // Configure hardware pin connections thus:
        static byte LED = 16;
        static byte BUTTON = 18;
        static byte POTENTIOMETER = 26;//A0
        static byte LIGHTSENSOR = 27;  //A1
        static byte SOUNDSENSOR = 28;  //A2 
        static byte RELAY = 16;

        static byte MAX_NUM_NEOPIXEL_PIXELS = 8;
        const string Tab5 = "\t\t\t\t\t";

        // Choose test DEFAULT type (There is now a menu to select from)
        //static softatalib.ConsoleTestType Testtype = ConsoleTestType.Sensors;
        //static softatalib.ConsoleTestType Testtype = ConsoleTestType.Analog_Potentiometer_Light_and_Sound;
        //static softatalib.ConsoleTestType Testtype = ConsoleTestType.LCD1602Display;
        //static softatalib.ConsoleTestType Testtype = ConsoleTestType.NeopixelDisplay;
        //static softatalib.ConsoleTestType Testtype = ConsoleTestType.Digital_Button_and_LED;
        //static softatalib.ConsoleTestType Testtype = ConsoleTestType.Serial;
        static ConsoleTestType Testtype = ConsoleTestType.Digital_Button_and_LED;
        //Set Serial1 or Serial2 for send and receive.
        //Nb: If both true or both false then loopback on same serial port.
        //static bool Send1 = true;
        //static bool Recv1 = true;
        // Next two are the same test
        //static softatalib.ConsoleTestType Testtype = ConsoleTestType.Analog_Potentiometer_and_LED;
        //static softatalib.ConsoleTestType Testtype = ConsoleTestType.PWM;

        private static Softata.SoftataLib _softatalib;

        static Softata.SoftataLib softatalib { get {return  _softatalib; } set {_softatalib = value; } }

        internal static void ShowHeading(string test="")
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
            hasRunCalibrationOnce = false;
            softatalib  = new SoftataLib();
            AnalogInit();
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
            /*string? s_ipaddress = Environment.GetEnvironmentVariable("SOFTATA_IPADDRESS");
            if(!string.IsNullOrEmpty(s_ipaddress))
            {
                if (s_ipaddress.Count(c => c == '.') == 3)
                {
                    if (IPAddress.TryParse(s_ipaddress, out IPAddress? address))
                    {
                        IpAddress = s_ipaddress;
                    };
                }
            }
            string? s_portstr = Environment.GetEnvironmentVariable("SOFTATA_PORT");
            if (!string.IsNullOrEmpty(s_portstr))
            {
                if (int.TryParse(s_portstr, out int port))
                {
                    Port = port;
                }
            }*/


            bool quit = false;


            
            while (!quit)
            {
                ShowHeading();
                try
                {
                    if (!connected)
                    {
                        Console.WriteLine($"{B.fgblu}Default Softata Server is at{B.fgYel} {ipaddressStr}:{port}{Fmt.clr}");
                        Layout.Info("Enter new values"," or press [Enter] to continue:");
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
                        Console.WriteLine(" ... and is waiting (4s slow flash) before proceeding:");                      Console.WriteLine();
                    }
                    
                    quit = false;
                    Testtype = Layout.SelectEnum<ConsoleTestType>((int)Testtype + 1, ref quit, true);

                    if (quit)
                        return;

                    ShowHeading();
                    Layout.Info($"Selected Test: ",$" {Testtype}");

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

                            //Environment.SetEnvironmentVariable("SOFTATA_IPADDRESS", IpAddress);
                            //Environment.SetEnvironmentVariable("SOFTATA_PORT", Port.ToString());
                            Console.WriteLine($"Connected to {IpAddress}:{Port}");
                            Console.WriteLine();
                            quit = YesNoQuit("Press [Enter] to continue or [Q] to quit", true);
                        }
                        if (quit)
                            return;
                    



                        softatalib.SendMessageCmd("Begin");
                        Thread.Sleep(500);
                        string Version = softatalib.SendMessageCmd("Version");
                        Console.WriteLine($"Softata Version: {Version}");
                        Thread.Sleep(500);
                        string devicesCSV = softatalib.SendMessageCmd("Devices");
                        Console.WriteLine($"{devicesCSV}");
                        Thread.Sleep(500);
                    }

                    // Explicitly declare subclasses
                    // Instantiate where needed.
                    SoftataLib.Digital softatalibDigital;
                    //SoftataLib.Analog softatalibAnalog;
                    SoftataLib.PWM softatalibPWM;
                    SoftataLib.Serial softatalibSerial1;
                    SoftataLib.Serial softatalibSerial2;
                    SoftataLib.Sensor softatalibSensor;
                    SoftataLib.Actuator softatalibActuator;
                    SoftataLib.Display softatalibDisplay;
                    switch (Testtype)
                    {
                        case ConsoleTestType.Analog_Potentiometer_and_LED:
                        case ConsoleTestType.PWM:
                        case ConsoleTestType.Analog_Potentiometer_Light_and_Sound:
                        case ConsoleTestType.Potentiometer_and_Actuator:
                        case ConsoleTestType.Analog_Device_Raw:
                            Calibrate();
                            break;
                    }

                    Layout.RainbowHeading($"Softata Test: { Testtype}");
                    switch (Testtype)
                    {
                        case ConsoleTestType.Test_OTA_Or_WDT:
                            
                            softatalibDigital = new SoftataLib.Digital(softatalib);

                            softatalibDigital.SetPinMode(BUTTON, SoftataLib.PinMode.DigitalInput);
                            softatalibDigital.SetPinMode(LED, SoftataLib.PinMode.DigitalOutput);
                            softatalibDigital.SetPinState(LED, SoftataLib.PinState.High);

                            Console.WriteLine("WDT Test: Enable WDT in softata.h, deploy and boot then press [Return]");
                            Console.WriteLine("OTA Test: Enable OTA (optonally disable WDT) in softata.h, deploy and boot then press [Return]");
                            Console.ReadLine();
                            Console.WriteLine("4 LED toggles then WDT.Update/OTA.handle turned off, with busy wait on device,for 100 secs");

                            for (int i = 0; i < 4; i++)
                            {
                                softatalibDigital.TogglePinState(LED);
                                Console.WriteLine($"{i} secs");
                                Thread.Sleep(1000);
                            }
                            Console.WriteLine($"Turning off WDT/OTA Updates with busy wait on device. Press [Enter]");
                            Console.ReadLine();
                            softatalibDigital.TurnOffWDTUpdates();
                            for (int i = 0; i < 100; i++)
                            {
                                Console.WriteLine($"{i} secs");
                                softatalibDigital.TogglePinState(LED);
                                Thread.Sleep(1000);
                            }
                            break;
                        // LED-Button test
                        case ConsoleTestType.Digital_Button_and_LED:
                            softatalibDigital = new SoftataLib.Digital(softatalib);

                            softatalibDigital.SetPinMode(BUTTON, SoftataLib.PinMode.DigitalInput);
                            softatalibDigital.SetPinMode(LED, SoftataLib.PinMode.DigitalOutput);
                            softatalibDigital.SetPinState(LED, SoftataLib.PinState.High);

                            Layout.Info("Button connected to pin", $" {BUTTON}");
                            Layout.Info($"LED connected to pin", $" {LED}");
                            Layout.Info("LED will toggle when button NOT pressed.");
                            Layout.Press2Continue();


                            int digMax = 0x10;
                            for (int i = 0; i < digMax; i++)
                            {
                                Console.WriteLine($"{i + 1}/{digMax}");
                                while (softatalibDigital.GetPinState(BUTTON))
                                    Thread.Sleep(100);
                                softatalibDigital.TogglePinState(LED);
                            }
                            break;

                        // Potentiometer-LED Test
                        case ConsoleTestType.Analog_Potentiometer_and_LED:
                        case ConsoleTestType.PWM:
                            // Note no pin setup needed for analog
                            softatalibDigital = new SoftataLib.Digital(softatalib);
                            //softatalibAnalog = new SoftataLib.Analog(softatalib);
                            softatalibPWM = new SoftataLib.PWM(softatalib);

                            byte numPWMBits = 10;
                            Layout.Info($"Potentiometer connected to pin", $" {POTENTIOMETER}");
                            Layout.Info($"Light Sensor connected to pin", $" {LIGHTSENSOR}");
                            Layout.Info($"{numPWMBits}", $" Bit PWM being used. 10 ADC bits");
                            Layout.Info("LED brightness depends upon potentiometer.");
                            Layout.Press2Continue();

                            softatalibDigital.SetPinMode(LED, SoftataLib.PinMode.DigitalOutput);
                            softatalibPWM.SetPinModePWM(LED, numPWMBits);
                            
                            for (int i = 0; i < numLoops; i++)
                            {
                                int val = softatalibAnalog!.AnalogRead(POTENTIOMETER);
                                if (val != int.MaxValue)
                                {
                                    Console.WriteLine($"{i + 1}/{numLoops}. AnalogRead({POTENTIOMETER}) = {val}");
                                    int pwmVal = val;
                                    if (val > 1023)
                                        pwmVal = 1023;
                                    softatalibPWM.SetPWM(LED, pwmVal);
                                }
                                else
                                    Console.WriteLine($"AnalogRead({POTENTIOMETER}) failed");
                                Console.WriteLine();
                                Thread.Sleep(500);
                            }
                            break;
                        case ConsoleTestType.Loopback:
                            softatalibSerial1 = new SoftataLib.Serial(softatalib);
                            softatalibSerial2 = new SoftataLib.Serial(softatalib);

                            byte[] txPins = new byte[] { 0, 0, 4 }; //Nb: Recv are Tx+1


                            Console.WriteLine("Serial Test");
                            Console.WriteLine("1. Serial 1");
                            Console.WriteLine("2. Serial 2");
                            Console.WriteLine("3. (Tx)Serial 1 -> (Rx)Serial 2");
                            Console.WriteLine("4. (Tx)Serial 2 -> (Rx)Serial 1");
                            Console.WriteLine("5. Quit");

                            Console.Write("Selection:");
                            bool serialFound = false;
                            byte iserialTxRx = 1;
                            do
                            {
                                string? s = Console.ReadLine();
                                if (byte.TryParse(s, out byte numTxRx))
                                {
                                    if ((numTxRx > 0) && (numTxRx <= 5))
                                    {
                                        serialFound = true;
                                        iserialTxRx = numTxRx;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(s))
                                {
                                    string key = s;
                                    if (key.ToUpper() == "Q")
                                    {
                                        iserialTxRx = 5;
                                        serialFound = true;
                                    }
                                }
                            } while (!serialFound);

                            if (iserialTxRx == 5)
                                break;

                            byte comTx = 1;
                            byte comRx = 1;

                            switch (iserialTxRx)
                            {
                                case 1:
                                    comTx = 1;
                                    comRx = 1;
                                    break;
                                case 2:
                                    comTx = 2;
                                    comRx = 2;
                                    break;
                                case 3:
                                    comTx = 1;
                                    comRx = 2;
                                    break;
                                case 4:
                                    comTx = 2;
                                    comRx = 1;
                                    break;
                                case 5:
                                    break;
                            }

                            Console.WriteLine("");
                            Console.WriteLine("Serial mode Selection:");
                            Console.WriteLine("1. ASCII");
                            Console.WriteLine("2. Byte");
                            ;
                            if (iserialTxRx < 3)
                            {
                                Console.WriteLine("3. GPS");
                            }
                            Console.WriteLine("4. Quit");

                            Console.Write("Selection:");
                            serialFound = false;
                            int iserialMode = 1;
                            do
                            {
                                string? s = Console.ReadLine();
                                if (byte.TryParse(s, out byte numMode))
                                {
                                    if ((numMode > 0) && (numMode <= 4) && (!((numMode == 3) && (iserialTxRx > 2))))
                                    {
                                        serialFound = true;
                                        iserialMode = numMode;

                                    }
                                }
                                else if (!string.IsNullOrEmpty(s))
                                {
                                    string key = s;
                                    if (key.ToUpper() == "Q")
                                    {
                                        iserialMode = 4;
                                        serialFound = true;
                                    }
                                }
                            } while (!serialFound);
                            if (iserialMode == 4)
                                break;


                            Console.WriteLine();
                            Console.WriteLine("BAUD Rate (Default 9600).");
                            int baudRate = 9600;
                            do
                            {
                                serialFound = false;
                                baudRate = 9600;
                                while (!serialFound)
                                {
                                    Console.Write("Enter BAUD:");
                                    string? baudStr = Console.ReadLine();
                                    if (int.TryParse(baudStr, out int baud))
                                    {
                                        serialFound = true;
                                        baudRate = baud;
                                    }
                                    else if (string.IsNullOrEmpty(baudStr))
                                    {
                                        serialFound = true;
                                    }
                                }
                                if (!serialFound)
                                    Console.WriteLine("Invalid");
                                else if (!softatalib.Baudrates.Contains(baudRate))
                                    Console.WriteLine("Invalid");
                            } while (!softatalib.Baudrates.Contains(baudRate));


                            softatalibSerial1.serialSetup(txPins[1], baudRate, 1);
                            softatalibSerial2.serialSetup(txPins[2], baudRate, 2);


                            if (iserialMode == 1) // ASCII test
                            {
                                for (char sendCh = ' '; sendCh <= '~'; sendCh++)
                                {
                                    char recvCh;
                                    softatalibSerial1.serialWriteChar(comTx, sendCh);
                                    Thread.Sleep(100);
                                    recvCh = softatalibSerial1.serialGetChar(comRx);
                                    if (recvCh == sendCh)
                                        Console.WriteLine($"Serial{comTx} Sent {sendCh} Got {recvCh} on Serial{comRx},OK");
                                    else
                                        Console.WriteLine($"Serial{comTx} Sent {sendCh} Got {recvCh} on Serial{comRx},NOK!");
                                    Thread.Sleep(200);
                                }
                            }
                            else if (iserialMode == 2)  // Byte test
                            {
                                for (byte sendByte = 0x00; sendByte <= 0xff; sendByte++)
                                {
                                    byte recvByte;
                                    softatalibSerial1.serialWriteByte(comTx, sendByte);
                                    recvByte = softatalibSerial1.serialGetByte(comRx);
                                    if (recvByte == sendByte)
                                        Console.WriteLine($"Serial{comTx} Sent {sendByte} Got {recvByte} on Serial{comRx},OK");
                                    else
                                        Console.WriteLine($"Serial{comTx} Sent {sendByte} Got {recvByte} on Serial{comRx},NOK!");

                                    Thread.Sleep(200);
                                    if (sendByte == 0xff)
                                        break;
                                }
                            }
                            else if (iserialMode == 3)  // GPS
                            {
                                Console.WriteLine();
                                Console.WriteLine("Reading GPS");
                                Console.WriteLine("Press [Esc] to stop");
                                Thread.Sleep(0500);
                                while (true)
                                {
                                    string msg = softatalibSerial1.readLine(comRx, false);
                                    Console.WriteLine($"\t{msg}");

                                    if (Console.KeyAvailable)
                                    {
                                        var cki = Console.ReadKey();
                                        if (cki.Key == ConsoleKey.Escape)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        case ConsoleTestType.Sensors:
                            softatalibSensor = new SoftataLib.Sensor(softatalib);
                            bool debug = false;
                            string[] Sensors = softatalibSensor.GetSensors();
                            if (Sensors.Length == 0)
                                Console.WriteLine($"No sensors found");
                            else
                            {
                                Console.WriteLine($"Sensors found:");
                                for (byte i = 0; i < Sensors.Length; i++)
                                {
                                    string _sensor = Sensors[i];
                                    Console.WriteLine($"{i + 1}.\t\t{_sensor}");
                                }
                                Console.Write("Selection:");
                                bool found = false;
                                byte isensor = 0;
                                do
                                {
                                    string? s = Console.ReadLine();
                                    if (byte.TryParse(s, out byte isen))
                                    {
                                        if (isen > 0 && isen <= Sensors.Length)
                                        {
                                            isensor = (byte)(isen - 1);
                                            found = true;
                                        }
                                    }
                                } while (!found);
                                string sensor = Sensors[isensor];

                                string pins = softatalibSensor.GetPins(isensor);
                                if (string.IsNullOrEmpty(pins))
                                    Console.WriteLine($"{sensor} getPins() failed");
                                else
                                {
                                    Console.WriteLine($"{sensor} getPins OK");
                                    Console.WriteLine($"{sensor} Pins = {pins}");
                                }
                                Console.WriteLine("Press any key to setup sensor");
                                Console.ReadLine();
                                byte sensorLinkedListIndex = (byte)softatalibSensor.SetupDefault(isensor);
                                if (sensorLinkedListIndex < 0)
                                    Console.WriteLine($"Instantiated sensor {sensor} not found");
                                else
                                {
                                    Console.WriteLine($"Instantiated {sensor} found at {sensorLinkedListIndex}");
                                    string[] properties = softatalibSensor.GetProperties(isensor);
                                    if (properties.Length == 0)
                                        Console.WriteLine($"{sensor} getProperties() failed");
                                    else
                                    {
                                        Console.WriteLine($"{sensor} getProperties OK");
                                        foreach (string property in properties)
                                            Console.WriteLine($"{sensor} property = {property}");
                                    }
                                    Console.WriteLine();

                                    Console.WriteLine("Press any key to continue.");
                                    Console.ReadLine();
                                    Console.WriteLine();

                                    //////////////////////////////////
                                    int sensorMode = 1;
                                    /////////////////////////////////
                                    Console.WriteLine("(START) SELECT SENSOR MODE");
                                    Console.WriteLine("1. Read Sensor Values.");
                                    Console.WriteLine("2. Get Telemetry");
                                    Console.WriteLine("3. Start Stream Telemetry to Bluetooth");
                                    Console.WriteLine("4. Start Stream Telemetry to Azure IoT Hub");
                                    Console.WriteLine("Default 1.");
                                    Console.Write("Selection:");
                                    do
                                    {
                                        string? s = Console.ReadLine();
                                        if (byte.TryParse(s, out byte mode))
                                        {
                                            if (mode > 0 && mode <= 4)
                                            {
                                                sensorMode = (byte)mode;
                                                found = true;
                                            }
                                        }
                                    } while (!found);
                                    bool showMenu = false;
                                    bool keepRunning = true;


                                    uint period = 2500;
                                    switch (sensorMode)
                                    {
                                        case 1:
                                        case 2:
                                            found = false;
                                            do
                                            {
                                                Console.Write($"Please enter the period btw sensor reads (Default {period}mS): ");
                                                string? p = Console.ReadLine();
                                                if (uint.TryParse(p, out uint _period))
                                                {
                                                    period = _period;
                                                    found = true;
                                                }
                                                else if (string.IsNullOrEmpty(p))
                                                {
                                                    found = true;
                                                }
                                            } while (!found);
                                            Console.WriteLine();
                                            break;
                                    }
                                    int sensorQuitModeNo = 7;
                                    while (keepRunning)
                                    {
                                        if (showMenu)
                                        {
                                            sensorMode = 1;
                                            Console.WriteLine(" SELECT SENSOR MODE");
                                            Console.WriteLine("1. Read Sensor Values.");
                                            Console.WriteLine("2. Get Telemetry");
                                            Console.WriteLine("5. Pause BT or IoT Telemetry Stream");
                                            Console.WriteLine("6. Continue BT IoT Telemetry Stream");
                                            Console.WriteLine("7. Quit");
                                            Console.WriteLine("Default 1.");

                                            Console.Write("Selection:");
                                            do
                                            {
                                                string? s = Console.ReadLine();
                                                if (byte.TryParse(s, out byte mode))
                                                {
                                                    if ((mode > 0) && (mode <= sensorQuitModeNo) && (mode != 3) && (mode != 4))
                                                    {
                                                        sensorMode = (byte)mode;
                                                        found = true;
                                                        if (sensorMode == 7)
                                                        {
                                                            keepRunning = false;
                                                        }
                                                    }
                                                }
                                                else if (!string.IsNullOrEmpty(s))
                                                {
                                                    string key = s;
                                                    if (key.ToUpper() == "Q")
                                                    {
                                                        sensorMode = sensorQuitModeNo;
                                                        keepRunning = false;
                                                    }
                                                }
                                            } while (!found);
                                            showMenu = true;
                                            switch (sensorMode)
                                            {
                                                case 1:
                                                case 2:
                                                    showMenu = false;
                                                    break;
                                            }

                                        }
                                        if (sensorMode == sensorQuitModeNo)
                                        {
                                            Console.WriteLine("Quitting app. Please wait.");
                                            break;
                                        }
                                        else if (sensorMode == 2)
                                        {
                                            string json = softatalibSensor.GetTelemetry((byte)sensorLinkedListIndex, debug);
                                            Console.WriteLine($"\t\t Telemetry: {json}");
                                            Console.WriteLine("Press [Esc] to stop");
                                            Thread.Sleep((int)period);
                                        }
                                        else if (sensorMode == 3)
                                        {
                                            string indxStr = softatalibSensor.StartSendingTelemetryBT((byte)sensorLinkedListIndex);
                                            if (int.TryParse(indxStr, out int val))
                                                Console.WriteLine($"Streaming to BT started. List No:{val}");
                                            else
                                                Console.WriteLine($"Streaming to BT failed to start.");
                                            showMenu = true;
                                        }
                                        else if (sensorMode == 4)
                                        {
                                            string indxStr = softatalibSensor.StartSendingTelemetryToIoTHub((byte)sensorLinkedListIndex);
                                            if (int.TryParse(indxStr, out int val))
                                            {
                                                Console.WriteLine($"Streaming to Azure IoT Hub started. List No:{val}");
                                                Console.WriteLine("Nb: Observe Telemetry in Device Explorer:");
                                                Console.WriteLine("https://github.com/Azure/azure-iot-explorer/");
                                                Console.WriteLine("--------------------------------------------");
                                                Console.WriteLine();
                                            }
                                            else
                                                Console.WriteLine($"Streaming to Azure IoT Hub failed to start.");
                                            showMenu = true;
                                        }
                                        else if (sensorMode == 5)
                                        {
                                            string json = softatalibSensor.PauseSendTelemetry((byte)sensorLinkedListIndex);
                                            if (!string.IsNullOrEmpty(json))
                                                Console.WriteLine($"json {json}");
                                        }
                                        else if (sensorMode == 6)
                                        {
                                            string json = softatalibSensor.ContinueSendTelemetry((byte)sensorLinkedListIndex);
                                            if (!string.IsNullOrEmpty(json))
                                                Console.WriteLine($"json {json}");
                                        }

                                        else if (sensorMode == 1)
                                        {
                                            double[]? values = softatalibSensor.ReadAll((byte)sensorLinkedListIndex, debug);
                                            if (values == null)
                                                Console.WriteLine($"{sensor} readAll() failed");
                                            else
                                            {
                                                if (debug)
                                                    Console.WriteLine($"{sensor} readAll() OK");
                                                else
                                                    Console.WriteLine("ReadAll():");
                                                for (int p = 0; p < properties.Length; p++)
                                                    Console.WriteLine($"\t\t{sensor} {properties[p]} = {values[p]}");
                                            }
                                            Console.WriteLine("Individual Read():");
                                            for (byte p = 0; p < properties.Length; p++)
                                            {
                                                double? value = softatalibSensor.Read((byte)sensorLinkedListIndex, p, debug);
                                                if (value == null)
                                                    Console.WriteLine($"{sensor} read() failed");
                                                else
                                                    Console.WriteLine($"\t\t{sensor} {properties[p]} = {value}");
                                                Console.WriteLine();
                                            }
                                            Console.WriteLine("Press [Esc] to stop");
                                            Thread.Sleep((int)period);
                                        }
                                        if (Console.KeyAvailable)
                                        {
                                            var cki = Console.ReadKey();
                                            if (cki.Key == ConsoleKey.Escape)
                                            {
                                                showMenu = true;
                                            }
                                        }

                                    }
                                }
                            }
                            break;

                        case ConsoleTestType.Displays:
                            softatalibDisplay = new SoftataLib.Display(softatalib);

                            SoftataLib.Display.Neopixel softataLibDisplayNeopixel;
                            SoftataLib.Display.BARGRAPHDisplay softataLibDisplayBargraphDisplay;
                            //SoftataLib.Display.BARGRAPHDisplay softataLibDisplayGBargraphDisplay;
                            SoftataLib.Display.LCD1602Display softataLibDisplayLCD1602Display;
                            SoftataLib.Display.Oled096 softataLibDisplayOled096;

                            byte idisplay = 0;
                            string display = "";
                            string[] Miscs = new string[0];
                            string[] Displays = softatalibDisplay.GetDisplays();
                            if (Displays.Length == 0)
                                Console.WriteLine($"No displays found");
                            else
                            {
                                Console.WriteLine($"Displays found:");
                                for (byte i = 0; i < Displays.Length; i++)
                                {
                                    display = Displays[i];
                                    Console.WriteLine($"{i + 1}.\t\t{display}");
                                }
                                Console.WriteLine("Default: 1.");
                                //Console.WriteLine("Nb: Option 1. not yet available.");

                                Console.Write("Selection:");
                                bool found = false;

                                do
                                {
                                    string? s = Console.ReadLine();
                                    if (byte.TryParse(s, out byte idis))
                                    {
                                        if ((idis > 0) && (idis <= Displays.Length)) // && (idis != 1))
                                        {
                                            idisplay = (byte)(idis - 1);
                                            found = true;
                                        }
                                        else if (string.IsNullOrEmpty(s))
                                        {
                                            found = true;
                                        }
                                    }
                                } while (!found);
                                display = Displays[idisplay];

                                string pins = softatalibDisplay.GetPins(idisplay);
                                if (string.IsNullOrEmpty(pins))
                                    Console.WriteLine($"{display} getPins() failed");
                                else
                                {
                                    Console.WriteLine($"{display} getPins OK");
                                    Console.WriteLine($"{display} Pins = {pins}");
                                }

                                Miscs = softatalibDisplay.GetMiscCmds(idisplay);
                                if (Miscs == null)
                                    Console.WriteLine($"{display} getMiscCmds() failed");
                                else
                                {
                                    Console.WriteLine($"{display} getMiscCmds OK");
                                    Console.WriteLine($"{display} Misc Cmds:");
                                    for (byte i = 0; i < Miscs.Length; i++)
                                    {
                                        string misc = Miscs[i];
                                        Console.WriteLine($"{misc}");
                                    }
                                }
                            }
                            byte displayLinkedListIndex;

                            Console.WriteLine("Press any key to continue.");
                            Console.ReadLine();

                            //////////////////////////////////////////
                            // NOTE: enum order of DisplayDevice must match that returned by GroveDisplayCmds.getDisplays
                            DisplayDevice displayDevice = (DisplayDevice)idisplay;
                            //////////////////////////////////////////


                            byte numPixels = 0; //// softatalibDisplayNeopixel.MaxNumPixels;

                            // Only do non-default setup for Neopixel
                            if (displayDevice == DisplayDevice.NEOPIXEL)
                            {
                                softataLibDisplayNeopixel = new SoftataLib.Display.Neopixel(softatalib);
                                Console.WriteLine($"Select number of Pixels:");
                                for (byte i = 1; i <= softataLibDisplayNeopixel.MaxNumPixels; i++)
                                {
                                    Console.WriteLine($"{i} Pixels");
                                }
                                Console.WriteLine($"Default: {numPixels}.");
                                bool found = false;
                                do
                                {
                                    string? s = Console.ReadLine();
                                    if (byte.TryParse(s, out byte idis))
                                    {
                                        if ((idis > 0) && (idis <= MAX_NUM_NEOPIXEL_PIXELS))
                                        {
                                            numPixels = idis;
                                            found = true;
                                        }
                                        else if (string.IsNullOrEmpty(s))
                                        {
                                            found = true;
                                        }
                                    }
                                } while (!found);
                                displayLinkedListIndex = (byte)softatalibDisplay.Setup(idisplay, 16, numPixels);
                            }
                            else if ((displayDevice == DisplayDevice.BARGRAPH)) //|| (displayDevice == DisplayDevice.GBARGRAPH))
                            {
                                softataLibDisplayBargraphDisplay = new SoftataLib.Display.BARGRAPHDisplay(softatalib);
                                // Use default settings
                                //displayLinkedListIndex = (byte)softatalibDisplay.SetupDefault(idisplay); 
                                //Or use custom settings: {data,latch,clock} GPIO Pins
                                List<byte> settings = new List<byte> { 20, 21 }; // Send the  data pin as 16
                                displayLinkedListIndex = (byte)softatalibDisplay.Setup(idisplay, 16, settings);
                            }
                            else if (displayDevice == DisplayDevice.OLED096)
                            {
                                softataLibDisplayOled096 = new SoftataLib.Display.Oled096(softatalib);
                                displayLinkedListIndex = (byte)softatalibDisplay.SetupDefault(idisplay);
                            }
                            else if (displayDevice == DisplayDevice.LCD1602)
                            {
                                softataLibDisplayLCD1602Display = new SoftataLib.Display.LCD1602Display(softatalib);
                                displayLinkedListIndex = (byte)softatalibDisplay.SetupDefault(idisplay);
                            }
                            else
                                displayLinkedListIndex = (byte)softatalibDisplay.SetupDefault(idisplay);

                            int bargraphPin = 0;
                            Console.WriteLine($"displayLinkedListIndex: {displayLinkedListIndex}");
                            if (displayLinkedListIndex < 0)
                                Console.WriteLine($"Instantiated sensor {display} not found");
                            else
                            {
                                bool bargraphResult = true;
                                if (Miscs.Length > 0)
                                {
                                    int imisc = 0xff;
                                    do
                                    {
                                        string misc = "";
                                        Console.WriteLine($"Select a Display Misc Cmd to run:");
                                        Console.WriteLine($"{0}.\t\tSkip/Done");
                                        for (byte i = 0; i < Miscs.Length; i++)
                                        {
                                            misc = Miscs[i];
                                            Console.WriteLine($"{i + 1}.\t\t{misc}");
                                        }
                                        Console.WriteLine($"{Miscs.Length + 1}.\t\tClear Display");
                                        Console.WriteLine($"{Miscs.Length + 2}.\t\tWrite Value");
                                        Console.WriteLine("Default: 0.");
                                        Console.Write("Selection:");
                                        bool found = false;
                                        var ExtendedMiscsTemp = Miscs.Append("Clear Display");
                                        string[] ExtendedMiscs = ExtendedMiscsTemp.Append("Write Value").ToArray();

                                        do
                                        {
                                            string? s = Console.ReadLine();
                                            if (byte.TryParse(s, out byte imis))
                                            {
                                                if ((imis >= 0) && (imis <= ExtendedMiscs.Length)) // && (idis != 1))
                                                {
                                                    if (imis == 0)
                                                        imisc = 0xff;
                                                    else
                                                        imisc = (byte)(imis - 1);
                                                    found = true;
                                                }
                                                else if (string.IsNullOrEmpty(s))
                                                {
                                                    found = true;
                                                }
                                            }
                                        } while (!found);
                                        if (imisc < ExtendedMiscs.Length)
                                        {
                                            misc = ExtendedMiscs[imisc];
                                            Console.WriteLine($"Selected {misc} Misc command");
                                            //Console.WriteLine("Note: Not passing any params to Misc command at this stage.");
                                            byte[]? data = null;
                                            if (displayDevice == DisplayDevice.GBARGRAPH)
                                            {
                                                if (
                                                    ((BARGRAPHMiscCmds)imisc >= BARGRAPHMiscCmds.setLed)
                                                    &&
                                                    ((BARGRAPHMiscCmds)imisc <= BARGRAPHMiscCmds.toggleLed)
                                                  )

                                                {
                                                    Console.WriteLine("Enter Bargraph Pin.");
                                                    Console.Write($"Default: {bargraphPin}");
                                                    string? indxStr = Console.ReadLine();
                                                    if (!string.IsNullOrEmpty(indxStr))
                                                    {
                                                        if (byte.TryParse(indxStr, out byte indx))
                                                        {
                                                            bargraphPin = indx;
                                                        }
                                                    }
                                                    //setLed,clrLed,toggleLed
                                                    data = new byte[] { (byte)bargraphPin };
                                                }
                                                else if ((BARGRAPHMiscCmds)imisc == BARGRAPHMiscCmds.setLevel)
                                                {
                                                    Console.WriteLine("Enter Bargraph Level 0-10");
                                                    Console.Write($"Default: 0");
                                                    string? levelStr = Console.ReadLine();
                                                    byte bargraphLevel = 0;
                                                    if (!string.IsNullOrEmpty(levelStr))
                                                    {
                                                        if (byte.TryParse(levelStr, out byte indx))
                                                        {
                                                            if ((indx >= 0) && (indx <= 10))
                                                                bargraphLevel = indx;
                                                        }
                                                    }
                                                    data = new byte[] { bargraphLevel };
                                                }
                                                else if (imisc == (ExtendedMiscs.Length - 2))
                                                {
                                                    //Clear Display
                                                    data = new byte[] { 0 };
                                                    imisc = 5;
                                                }
                                                else if (imisc == (ExtendedMiscs.Length - 1))
                                                {
                                                    //Set Value
                                                    Console.WriteLine("Enter valuel 0-2^10-1");
                                                    Console.Write($"Default: 0");
                                                    string? levelStr = Console.ReadLine();
                                                    int bargraphValue = 0;
                                                    if (!string.IsNullOrEmpty(levelStr))
                                                    {
                                                        if (int.TryParse(levelStr, out int indx))
                                                        {
                                                            if ((indx >= 0) && (indx < 1024))
                                                                bargraphValue = indx;
                                                        }
                                                    }
                                                    bargraphResult = softatalibDisplay.WriteString(displayLinkedListIndex, bargraphValue.ToString());
                                                }
                                            }

                                            if (imisc != (ExtendedMiscs.Length - 1))
                                            {
                                                bargraphResult = softatalibDisplay.Misc(displayLinkedListIndex, (byte)imisc, data);
                                            }
                                            if (bargraphResult)
                                                Console.WriteLine($"Misc command {misc} ran OK");
                                            else
                                                Console.WriteLine($"Misc command {misc} failed");
                                        }
                                        else
                                            Console.WriteLine($"Skipping/Exiting Misc command/s");
                                    } while (imisc != 0xff);
                                }

                                switch (displayDevice)
                                {
                                    case DisplayDevice.NEOPIXEL:
                                        {
                                            SoftataLib.Display.Neopixel softatalibDisplayNeopixel = new SoftataLib.Display.Neopixel(softatalib);
                                            Console.WriteLine($"Instantiated {display} linked at {displayLinkedListIndex}");
                                            softatalibDisplayNeopixel.Clear(displayLinkedListIndex); ;
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Misc_SetAll(displayLinkedListIndex, 255, 0, 0);
                                            Thread.Sleep(100);
                                            softatalibDisplayNeopixel.Misc_SetAll(displayLinkedListIndex, 0xFF, 0xA5, 0);   //Orange
                                            Thread.Sleep(100);
                                            softatalibDisplayNeopixel.Misc_SetAll(displayLinkedListIndex, 255, 255, 0);     //Yellow
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Misc_SetAll(displayLinkedListIndex, 0, 255, 0);
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Misc_SetAll(displayLinkedListIndex, 0, 0, 255);
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Misc_SetAll(displayLinkedListIndex, 0xA0, 0x20, 0xf0);//Purple
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Misc_SetAll(displayLinkedListIndex, 255, 255, 255);   //White
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Misc_SetOdd(displayLinkedListIndex, 255, 0, 0);
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(100);
                                            softatalibDisplayNeopixel.Misc_SetEvens(displayLinkedListIndex, 0, 255, 0);
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Misc_SetOdd(displayLinkedListIndex, 0, 0, 255);
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(100);
                                            softatalibDisplayNeopixel.Misc_SetEvens(displayLinkedListIndex, 255, 255, 0);
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Misc_SetOdd(displayLinkedListIndex, 0, 255, 255);
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(100);
                                            softatalibDisplayNeopixel.Misc_SetEvens(displayLinkedListIndex, 255, 255, 255);
                                            Thread.Sleep(500);
                                            softatalibDisplayNeopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(500);
                                            softatalibDisplayNeopixel.Misc_SetEvens(displayLinkedListIndex, 255, 255, 255);
                                            Thread.Sleep(500);
                                            softatalibDisplayNeopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(500);
                                            softatalibDisplayNeopixel.Misc_SetEvens(displayLinkedListIndex, 255, 255, 255);
                                            Thread.Sleep(2000);
                                            softatalibDisplayNeopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(500);
                                            for (byte n = 0; n <= softatalibDisplayNeopixel.MaxNumPixels; n++)
                                            {
                                                softatalibDisplayNeopixel.Misc_SetN(displayLinkedListIndex, 255, 255, 255, n);
                                                Thread.Sleep(1000);
                                            }
                                            softatalibDisplayNeopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(500);
                                            Console.WriteLine("OK");
                                        }
                                        break;
                                    case DisplayDevice.LCD1602:
                                        {
                                            Console.WriteLine($"Instantiated {display} linked at {displayLinkedListIndex}");
                                            softatalibDisplay.Clear(displayLinkedListIndex);
                                            softatalibDisplay.SetCursor(displayLinkedListIndex, 0, 0);
                                            softatalibDisplay.WriteString(displayLinkedListIndex, "First Line");
                                            softatalibDisplay.SetCursor(displayLinkedListIndex, 0, 1);
                                            softatalibDisplay.WriteString(displayLinkedListIndex, "Wait 5sec");
                                            Thread.Sleep(5000);
                                            softatalibDisplay.Clear(displayLinkedListIndex);
                                            softatalibDisplay.WriteString(displayLinkedListIndex, 4, 0, "(4,0):Cursor");
                                            softatalibDisplay.WriteString(displayLinkedListIndex, 2, 1, "(2,1):Write");
                                            Thread.Sleep(5000);
                                            {
                                                softatalibDisplay.WriteString(displayLinkedListIndex, 2, 1, "(2,1):Write");
                                                Thread.Sleep(1000);
                                                if (true)
                                                {
                                                    softatalibDisplay.Clear(displayLinkedListIndex);
                                                    Thread.Sleep(1000);
                                                    softatalibDisplay.SetCursor(displayLinkedListIndex, 0, 0);

                                                    softatalibDisplay.WriteString(displayLinkedListIndex, "Time:");
                                                    int numTimes = 10;
                                                    for (int i = 0; i < numTimes; i++)
                                                    {
                                                        DateTime now = DateTime.Now;
                                                        string format = "HH:mm:ss";
                                                        string time = now.ToString(format);
                                                        string msg = $"{i + 1}/{numTimes} {time}";
                                                        softatalibDisplay.WriteString(displayLinkedListIndex, 0, 1, msg);
                                                        Thread.Sleep(1000);
                                                    }
                                                }
                                            }
                                        }
                                        softatalibDisplay.Clear(displayLinkedListIndex);
                                        Thread.Sleep(1000);
                                        break;
                                    case DisplayDevice.OLED096:
                                        SoftataLib.Display.Oled096 softatalibDisplayOled096 = new SoftataLib.Display.Oled096(softatalib);
                                        softatalibDisplay.Clear(displayLinkedListIndex);
                                        Thread.Sleep(500);
                                        //Dummy test to see if simple Misc test works (with no date).
                                        softatalibDisplayOled096.misctest(displayLinkedListIndex);
                                        Thread.Sleep(1000);
                                        softatalibDisplayOled096.drawFrame(displayLinkedListIndex);
                                        Thread.Sleep(4000);
                                        softatalibDisplayOled096.drawCircle(displayLinkedListIndex);
                                        Thread.Sleep(4000);
                                        softatalibDisplayOled096.drawCircle(displayLinkedListIndex, 60, 32, 10);
                                        Thread.Sleep(4000);
                                        softatalibDisplay.Clear(displayLinkedListIndex);
                                        Thread.Sleep(500);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, 0, 0, "Hello Joe!");
                                        Thread.Sleep(2500);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, 1, 1, "Hi there!");
                                        Thread.Sleep(2500);
                                        softatalibDisplay.Clear(displayLinkedListIndex);
                                        Thread.Sleep(500);
                                        int numbr = 10;
                                        for (int i = 0; i < numbr; i++)
                                        {
                                            DateTime now = DateTime.Now;
                                            string format = "HH:mm:ss";
                                            string time = now.ToString(format);
                                            string msg = $"{i + 1}/{numbr} {time}";
                                            softatalibDisplay.WriteString(displayLinkedListIndex, 0, 1, msg);
                                            Thread.Sleep(2000);
                                        }
                                        softatalibDisplay.Clear(displayLinkedListIndex);
                                        softatalibDisplay.Home(displayLinkedListIndex);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, 0, 1, "Done");
                                        Thread.Sleep(500);

                                        break;
                                    case DisplayDevice.BARGRAPH:
                                    case DisplayDevice.GBARGRAPH:
                                        Console.WriteLine($"Instantiated {display} linked at {displayLinkedListIndex}");
                                        softatalibDisplay.Clear(displayLinkedListIndex);
                                        Thread.Sleep(1000);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, "1");
                                        Thread.Sleep(1000);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, "2");
                                        Thread.Sleep(1000);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, "4");
                                        Thread.Sleep(1000);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, "8");
                                        Thread.Sleep(1000);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, "16");
                                        Thread.Sleep(1000);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, "32");
                                        Thread.Sleep(1000);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, "64");
                                        Thread.Sleep(1000);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, "128");
                                        Thread.Sleep(1000);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, "256");
                                        Thread.Sleep(1000);
                                        softatalibDisplay.WriteString(displayLinkedListIndex, "512");
                                        Thread.Sleep(1000);
                                        softatalibDisplay.Clear(displayLinkedListIndex);
                                        Thread.Sleep(1000);
                                        for (byte i = 0; i < 11; i++)
                                        {
                                            byte[] data = new byte[] { i };
                                            bool bargraphLeelResult = softatalibDisplay.Misc(displayLinkedListIndex, (byte)BARGRAPHMiscCmds.setLevel, data);
                                            Thread.Sleep(1000);
                                        }
                                        softatalibDisplay.Clear(displayLinkedListIndex);
                                        break;
                                }
                            }
                            break;
                        case ConsoleTestType.Analog_Potentiometer_Light_and_Sound:

                            Layout.Info($"Potentiometer connected to pin"," {POTENTIOMETER}");
                            Layout.Info($"Light Sensor connected to pin"," {LIGHTSENSOR}");
                            Layout.Info($"Sound Sensor connected to pin"," {SOUNDSENSOR}");
                            Layout.Press2Continue();

                            int maxLoop = 20;
                            for (int i = 0; i < maxLoop; i++)
                            {
                                double value;
                                value = softatalibAnalog!.AnalogReadLightSensor();
                                if (value != double.MaxValue)
                                    Console.WriteLine($"{Tab5} {i+1}/{maxLoop}. Light: {value:0.##}");
                                else
                                    Console.WriteLine($"{Tab5} {i+1}/{maxLoop}. Light Sensor: failed");
                                Thread.Sleep(100);

                                value = softatalibAnalog!.AnalogReadPotentiometer();
                                if (value != double.MaxValue)
                                {
                                    value = Scale(value, Analog.AnalogDevice.Potentiometer, 100.00);
                                    Console.WriteLine($"{Tab5} {i+1}/{maxLoop}. Potentiometer: {value:0.##}");
                                }
                                else
                                    Console.WriteLine($"{Tab5} {i+1}/{maxLoop}. Potentiometer: failed");
                                Thread.Sleep(100);

                                value = softatalibAnalog!.AnalogReadSoundSensor();
                                if (value != double.MaxValue)
                                    Console.WriteLine($"{Tab5} {i+1}/{maxLoop}. Sound: {value:0.##}");
                                else
                                    Console.WriteLine($"{Tab5} {i+1}/{maxLoop}. Sound: failed");
                                Thread.Sleep(800);
                            }
                            break;
                        case ConsoleTestType.Potentiometer_and_Actuator:
                            softatalibActuator = new Actuator(softatalib);
                            softatalibDigital = new SoftataLib.Digital(softatalib);

                            string[] Actuators = softatalibActuator.GetActuators();
                            if (Actuators.Length == 0)
                                Console.WriteLine($"No actuators found");
                            else
                            {
                                Console.WriteLine($"Actuators found:");
                                for (byte i = 0; i < Actuators.Length; i++)
                                {
                                    string actuatorName = Actuators[i];
                                    Console.WriteLine($"{i + 1}. {actuatorName}");
                                }

                                byte iactuator = 0;
                                Console.Write($"Select actuator (Default {iactuator + 1}):");
                                bool found = false;

                                do
                                {
                                    string? s = Console.ReadLine();
                                    if (string.IsNullOrEmpty(s))
                                        break;
                                    if (byte.TryParse(s, out byte isen))
                                    {
                                        if (isen > 0 && isen <= Actuators.Length)
                                        {
                                            iactuator = (byte)(isen - 1);
                                            found = true;
                                        }
                                    }
                                } while (!found);
                                string actuator = Actuators[iactuator];
                                Console.WriteLine();
                                Console.WriteLine($"Using Actuator: {actuator}");

                                ActuatorDevice actuatorDevice = (ActuatorDevice)iactuator;

                                Console.WriteLine("Testing Actuator");

                                int delayAR = 333;

                                //softatalibAnalog.InitAnalogDevicePins(SoftataLib.RPiPicoMode.groveShield);
                                //softatalibAnalog.SetAnalogPin(SoftataLib.Analog.AnalogDevice.Potentiometer, POTENTIOMETER);


                                switch (actuatorDevice)
                                {
                                    case ActuatorDevice.Relay:
                                        {
                                            Console.WriteLine();
                                            Console.WriteLine($"{Tab5}Potentiometer-Relay Test");
                                            Console.WriteLine($"{Tab5}Potentiometer controls relay. On if >50%");
                                            Console.WriteLine($"{Tab5}Potentiometer connected to A0, Relay to D16/18 or 20");

                                            Console.WriteLine($"{Tab5}1. D16: Digital IO (Default)");
                                            Console.WriteLine($"{Tab5}2. Actuator-Relay Class( Default Settings (Pin 16)");
                                            Console.WriteLine($"{Tab5}3. Actuator-Relay Class( Setting: (Pin 18");
                                            Console.WriteLine($"{Tab5}4. Actuator-Relay Class( Setting: (Pin 20)");
                                            Console.WriteLine($"{Tab5}5. Quit");

                                            Console.Write($"{Tab5}Selection:");
                                            bool potRelaySelectionFound = false;
                                            byte potRelaySelection = 1;
                                            do
                                            {
                                                string? s = Console.ReadLine();
                                                if (byte.TryParse(s, out byte prSelection))
                                                {
                                                    if ((prSelection > 0) && (prSelection <= 5))
                                                    {
                                                        potRelaySelectionFound = true;
                                                        potRelaySelection = prSelection;
                                                    }
                                                }
                                                else if (!string.IsNullOrEmpty(s))
                                                {
                                                    string key = s;
                                                    if (key.ToUpper() == "Q")
                                                    {
                                                        potRelaySelectionFound = true;
                                                        potRelaySelection = 5;
                                                    }
                                                }
                                                else
                                                {
                                                    potRelaySelectionFound = true;
                                                    potRelaySelection = 1; //Default
                                                }
                                            } while (!potRelaySelectionFound);

                                            if (potRelaySelection == 5)
                                                break;

                                            byte relayPin = 16;
                                            byte actuatorIndex = 0;

                                            switch (potRelaySelection)
                                            {
                                                case 1:
                                                    softatalibDigital.SetPinMode(RELAY, SoftataLib.PinMode.DigitalOutput);
                                                    break;
                                                case 2:
                                                    actuatorIndex = (byte)softatalibActuator.SetupDefault(SoftataLib.Actuator.ActuatorDevice.Relay);
                                                    break;
                                                case 3:
                                                    relayPin = 18;
                                                    actuatorIndex = (byte)softatalibActuator.Setup(SoftataLib.Actuator.ActuatorDevice.Relay, relayPin);
                                                    break;
                                                case 4:
                                                    relayPin = 20;
                                                    actuatorIndex = (byte)softatalibActuator.Setup(SoftataLib.Actuator.ActuatorDevice.Relay, relayPin);
                                                    break;
                                            }




                                            Console.WriteLine($"{Tab5}Press any key to continue.");
                                            Console.ReadLine();

                                            bool state = false;
                                            Console.WriteLine($"{Tab5}\tRelay OFF");
                                            Console.WriteLine();
                                            Console.WriteLine($"{Tab5}{numLoops} loops.");
                                            for (int i = 0; i < numLoops; i++)
                                            {
                                                double val = softatalibAnalog!.AnalogReadPotentiometer();
                                                if (val != double.MaxValue)
                                                {
                                                    Console.WriteLine($"{Tab5} {i+1}/{numLoops}. AnalogRead({POTENTIOMETER}) = {val:0.##}");
                                                    if (val > 50)
                                                    {
                                                        if (!state)
                                                        {
                                                            Console.WriteLine($"\n{Tab5}\tSetting Relay ON");
                                                            state = !state;

                                                            switch (potRelaySelection)
                                                            {
                                                                case 1:
                                                                    softatalibDigital.SetPinState(RELAY, SoftataLib.PinState.High);
                                                                    break;
                                                                case 2:
                                                                case 3:
                                                                case 4:
                                                                    softatalibActuator.SetBit(actuatorIndex, 0);
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (state)
                                                        {
                                                            Console.WriteLine($"\n{Tab5}\tSetting Relay OFF");
                                                            state = !state;
                                                            switch (potRelaySelection)
                                                            {
                                                                case 1:
                                                                    softatalibDigital.SetPinState(RELAY, SoftataLib.PinState.Low);
                                                                    break;
                                                                case 2:
                                                                case 3:
                                                                case 4:
                                                                    softatalibActuator.ClearBit(actuatorIndex, 0);
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                    Console.WriteLine($"{Tab5}{i + 1}/{numLoops}. AnalogRead({POTENTIOMETER}) failed");
                                                Thread.Sleep(delayAR);
                                            }
                                        }
                                        break;
                                    case ActuatorDevice.SIPO_74HC595:
                                        {
                                            Console.WriteLine();
                                            Console.WriteLine($"{Tab5}Potentiometer 74HC59 SIPO Test"); ;
                                            Console.WriteLine($"{Tab5}Potentiometer controls which 8 bits are set");
                                            Console.WriteLine($"{Tab5}0% Pot = none, 50% Pot = 4 bits, 100% = all 8 bits etc.");
                                            Console.WriteLine($"{Tab5}Potentiometer connected to A0");
                                            Console.WriteLine($"{Tab5}Default '74HC595 setup:");
                                            Console.WriteLine($"{Tab5}595 Datapin = 16"); //Not 18 as in blog post
                                            Console.WriteLine($"{Tab5}505 LatchPin = 20");
                                            Console.WriteLine($"{Tab5}595 ClockPin = 21");


                                            Console.WriteLine($"{Tab5}1. Default setup");
                                            Console.WriteLine($"{Tab5}5. Quit");

                                            Console.Write($"{Tab5}Selection:");
                                            bool potShift595ParaOutFound = false;
                                            byte potShift595ParaOutSelection = 1;
                                            do
                                            {
                                                string? s = Console.ReadLine();
                                                if (byte.TryParse(s, out byte prSelection))
                                                {
                                                    if ((prSelection > 0) && (prSelection <= 5))
                                                    {
                                                        potShift595ParaOutFound = true;
                                                        potShift595ParaOutSelection = prSelection;
                                                    }
                                                }
                                                else if (!string.IsNullOrEmpty(s))
                                                {
                                                    string key = s;
                                                    if (key.ToUpper() == "Q")
                                                    {
                                                        potShift595ParaOutFound = true;
                                                        potShift595ParaOutSelection = 5;
                                                    }
                                                }
                                                else
                                                {
                                                    potShift595ParaOutFound = true;
                                                    potShift595ParaOutSelection = 1; //Default
                                                }
                                            } while (!potShift595ParaOutFound);

                                            if (potShift595ParaOutSelection == 5)
                                                break;

                                            byte actuatorParaIndex = 0;

                                            switch (potShift595ParaOutSelection)
                                            {
                                                case 1:
                                                    actuatorParaIndex = (byte)softatalibActuator.SetupDefault(SoftataLib.Actuator.ActuatorDevice.Shift95ParaOut);
                                                    break; ;
                                            }


                                            Console.WriteLine($"{Tab5}Setting 0 to 8 bits depending upon Pot.");
                                            bool paraState = false;
                                            byte prevBits = 0;
                                            softatalibActuator.ActuatorWrite(actuatorParaIndex, prevBits);
                                            Console.WriteLine($"{Tab5}Clear all 8 bits");
                                            for (int i = 0; i < (numLoops / 2); i++)
                                            {
                                                double val = softatalibAnalog!.AnalogReadPotentiometer();
                                                val = Scale(val, Analog.AnalogDevice.Potentiometer, 255);
                                                byte bits = (byte)Math.Round(val);
                                                Console.WriteLine($"{Tab5}{i + 1}/{numLoops/2}. AnalogRead({POTENTIOMETER}) = {val:0.##}");
                                                Console.Write($"{Tab5}Bits: ");
                                                string hex = Convert.ToString(bits, 2);
                                                string paddedString = hex.PadLeft(8, '0');
                                                Console.WriteLine(paddedString);
                                                Console.WriteLine($"{Tab5}{i+1}/{numLoops / 2} =====");
                                                Thread.Sleep(delayAR);
                                            }

                                            prevBits = 0;
                                            softatalibActuator.ActuatorWrite(actuatorParaIndex, prevBits);
                                            Console.WriteLine($"{Tab5}Clear all 8 bits");
                                            for (int i = 0; i < (numLoops / 2); i++)
                                            {
                                                double val = softatalibAnalog!.AnalogReadPotentiometer();
                                                byte bits = (byte)Scale(val, Analog.AnalogDevice.Potentiometer, 8);
                                                Console.WriteLine($"{Tab5}{i+1}/{numLoops/2}. AnalogRead({POTENTIOMETER}) = {val:0.##}");
                                                Console.Write($"{Tab5}Bits: ");
                                                string pat = "";
                                                switch (bits)
                                                {
                                                    case 1:
                                                        pat = "00000001";
                                                        break;
                                                    case 2:
                                                        pat = "00000010";
                                                        break;
                                                    case 3:
                                                        pat = "00000100";
                                                        break;
                                                    case 4:
                                                        pat = "00001000";
                                                        break;
                                                    case 5:
                                                        pat = "00010000";
                                                        break;
                                                    case 6:
                                                        pat = "00100000";
                                                        break;
                                                    case 7:
                                                        pat = "01000000";
                                                        break;
                                                    case 8:
                                                        pat = "10000000";
                                                        break;
                                                }
                                                Console.WriteLine($"{Tab5}{pat}");
                                                Console.WriteLine($"{Tab5}{i}/{numLoops / 2} =====");
                                                Thread.Sleep(delayAR);
                                            }
                                        }
                                        break;
                                    case ActuatorDevice.Servo:
                                        Console.WriteLine($"{Tab5}Connect Servo to D16");
                                        Console.WriteLine($"{Tab5}Press any key to continue.");
                                        Console.ReadLine();
                                        byte id = (byte)softatalibActuator.SetupDefault(SoftataLib.Actuator.ActuatorDevice.Servo);
                                        for (int i = 0; i < 2; i++)
                                        {
                                            softatalibActuator.ActuatorWrite(id, 90);
                                            Console.WriteLine($"{Tab5}Angle: 90");
                                            Thread.Sleep(2000);
                                            softatalibActuator.ActuatorWrite(id, 180);
                                            Console.WriteLine($"{Tab5}Angle: 180");
                                            Thread.Sleep(2000);
                                            softatalibActuator.ActuatorWrite(id, 90);
                                            Console.WriteLine($"{Tab5}Angle: 90");
                                            Thread.Sleep(2000);
                                            softatalibActuator.ActuatorWrite(id, 0);
                                            Console.WriteLine($"{Tab5}Angle: 0");
                                            Thread.Sleep(2000);
                                        }
                                        Console.WriteLine($"{Tab5}Connect Servo to D16");
                                        Console.WriteLine($"{Tab5}Connect Potentiometer to A0.");
                                        Console.WriteLine($"{Tab5}Press any key to continue.");
                                        Console.ReadLine();

                                        //softatalibAnalog.
                                        Console.WriteLine($"{Tab5}Turn potetiometer periodically.");
                                        Console.WriteLine($"{Tab5}Press any key to start.");
                                        Console.ReadLine();
                                        Console.WriteLine($"{Tab5}Runs for {numLoops} steps.");
                                        for (int i = 0; i < numLoops; i++)
                                        {
                                            double val = softatalibAnalog!.AnalogReadPotentiometer();

                                            byte angle = (byte)Scale(val, Analog.AnalogDevice.Potentiometer, 180);
                                            Console.WriteLine($"{Tab5}{i+1}/{numLoops} Angle: {angle}");

                                            softatalibActuator.ActuatorWrite(id, angle);
                                            Thread.Sleep(delayAR);
                                        }
                                        break;
                                    default:
                                        Console.WriteLine($"{Tab5}Actuator not yet implemented for this test.");
                                        break;
                                }
                                Console.WriteLine($"{Tab5}Finished test");
                                Console.WriteLine($"{Tab5}Press any key to continue.");
                                Console.ReadLine();


                            }
                            break;
                        case ConsoleTestType.Analog_Device_Raw:
                            Console.WriteLine($"Analog Device Raw Test");
                            Console.WriteLine($"Analog Device connected to pin {POTENTIOMETER}, {LIGHTSENSOR} or {SOUNDSENSOR} ");
                            Console.WriteLine("Select from 0: A0 1: A1 2: A2 (Default A0)");

                            byte pin = 26;
                            bool foundPin = false;
                            Analog.ADCResolutionBits resolution = Analog.ADCResolutionBits.Bits10;

                            while (!foundPin)
                            {
                                string? sAnalog = Console.ReadLine();
                                if (!string.IsNullOrEmpty(sAnalog))
                                {
                                    if (int.TryParse(sAnalog, out int prSelection))
                                    {
                                        if ((prSelection >= 0) && (prSelection < 3))
                                        {
                                            foundPin = true;
                                            pin = (byte)(POTENTIOMETER + prSelection);

                                        }
                                    }
                                    else
                                    {
                                        foundPin = true;
                                        pin = POTENTIOMETER;
                                    }
                                }
                            }
                            Console.WriteLine("ADC Resolution");
                            Console.WriteLine("Enter from 10 or 12 (Default 10), (alt 0 | 2)");


                            foundPin = false;
                            
                            while (!foundPin)
                            {
                                string? sAnalog = Console.ReadLine();
                                if (!string.IsNullOrEmpty(sAnalog))
                                {
                                    if (int.TryParse(sAnalog, out int prSelection2))
                                    {
                                        if ((prSelection2 == 10) || (prSelection2 == 12) ||(prSelection2 == 0) || (prSelection2 == 2))
                                        {
                                            if (prSelection2 == 0)
                                                prSelection2 = 10;
                                            else if (prSelection2 == 2)
                                                prSelection2 = 12;

                                            if (prSelection2 == 12)
                                            {
                                                resolution = Analog.ADCResolutionBits.Bits12;
                                            }
                                            foundPin = true;
                                        }
                                    }
                                }
                                else
                                {
                                    foundPin = true;
                                }
                            }


                            OtherGetMaxMin( pin, resolution);
                            softatalibAnalog!.SetAnalogPin(Analog.AnalogDevice.Other,pin, true,resolution);

 
                            for (int i = 0; i < numLoops; i++)
                            {
                                double value;
                                value = softatalibAnalog!.AnalogRead(pin);
                                if (value != double.MaxValue)
                                    Console.WriteLine($"{Tab5} {i + 1}/{numLoops} Analog Device: {value:0.##}");
                                else
                                    Console.WriteLine($"{Tab5} {i + 1}/{numLoops} Analog Device: failed");
                                Thread.Sleep(100);
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            if (connected)
            {
                softatalib.SendMessageCmd("End");
                Thread.Sleep(500);
            }

            return;
        }
    }
}
