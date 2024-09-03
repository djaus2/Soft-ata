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



namespace SoftataBasic
{
    internal class Program
    {

        // Set the same as Arduino:
        static int port = 4242;
        static string ipaddressStr = "192.168.0.12";

        // Configure hardware pin connections thus:
        static byte LED = 12;
        static byte BUTTON = 13;
        static byte POTENTIOMETER = 26;//A0
        static byte LIGHTSENSOR = 27;  //A1
        static byte SOUNDSENSOR = 28;  //A2 
        static byte RELAY = 16;

        static byte MAX_NUM_NEOPIXEL_PIXELS = 8;

        // Choose test DEFAULT type (There is now a menu to select from)
        //static SoftataLib.ConsoleTestType Testtype = ConsoleTestType.Sensors;
        //static SoftataLib.ConsoleTestType Testtype = ConsoleTestType.Analog_Potentiometer_Light_and_Sound;
        //static SoftataLib.ConsoleTestType Testtype = ConsoleTestType.LCD1602Display;
        //static SoftataLib.ConsoleTestType Testtype = ConsoleTestType.NeopixelDisplay;
        //static SoftataLib.ConsoleTestType Testtype = ConsoleTestType.Digital_Button_and_LED;
        //static SoftataLib.ConsoleTestType Testtype = ConsoleTestType.Serial;
        static ConsoleTestType Testtype = ConsoleTestType.Digital_Button_and_LED;
        //Set Serial1 or Serial2 for send and receive.
        //Nb: If both true or both false then loopback on same serial port.
        //static bool Send1 = true;
        //static bool Recv1 = true;
        // Next two are the same test
        //static SoftataLib.ConsoleTestType Testtype = ConsoleTestType.Analog_Potentiometer_and__LED;
        //static SoftataLib.ConsoleTestType Testtype = ConsoleTestType.PWM;

        static void Main(string[] args)
        {

            Console.WriteLine("Hello from Soft-ata!");
            Console.WriteLine();
            Console.WriteLine("For details see https://davidjones.sportronics.com.au/cats/softata/");
            Console.WriteLine();
;
            //SettingsManager.ClearAllSettings();
            SettingsManager.ReadAllSettings();

            string _ipaddressStr = SettingsManager.ReadSetting("IpaddressStr");
            if (!string.IsNullOrEmpty(_ipaddressStr))
            {             
                if (IPAddress.TryParse(_ipaddressStr, out IPAddress? address))
                {
                    ipaddressStr = _ipaddressStr;
                }
                else
                    Console.WriteLine("Invalid App SettingsIP Address");
            }
            else
            {
                SettingsManager.AddUpdateAppSettings("IpaddressStr", ipaddressStr);
            }
            string _port = SettingsManager.ReadSetting("Port");
            if (!string.IsNullOrEmpty(_ipaddressStr))
            {
                if(int.TryParse(_port, out int _portNo))
                {
                    port = _portNo;
                }
                else
                    Console.WriteLine("Invalid AppSettings  Port");
            }
            else
            {
                SettingsManager.AddUpdateAppSettings("Port", port.ToString());
            }

            string IpAddress = ipaddressStr;
            int Port = port;

            Console.WriteLine("TESTS");
            bool quit = false;
            bool connected = false;
            while (!quit)
            {
                try
                {
                    if (!connected)
                    {
                        Console.WriteLine($"Default Softata Server is at {ipaddressStr}:{port}");
                        Console.WriteLine("Enter new values or press [Enter] to continue:");
                        Console.Write("Plz Enter IPAdress: ");
                        string? ip = Console.ReadLine();
                        if (!string.IsNullOrEmpty(ip))
                        {
                            if (IPAddress.TryParse(ip, out IPAddress? address))
                            {
                                IpAddress = ip;
                                SettingsManager.AddUpdateAppSettings("IpaddressStr", IpAddress);
                            }
                            else
                                Console.WriteLine("Invalid IP Address");                                                           
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
                                Console.WriteLine("Invalid Port");
                        }
                        Console.WriteLine($"Selected Softata Server is at {IpAddress}:{Port}");
                    }
                    int num = 0;
                    for (int i = 0; i < (int)ConsoleTestType.MaxType; i++)
                    {
                        num++;

                        ConsoleTestType cmd = (ConsoleTestType)i;
                        string msg = $"{i + 1}.\t\t{cmd}";
                        msg = msg.Replace("_", " ");
                        Console.WriteLine(msg);
                    }
                    Console.WriteLine("[Q]\t\tQuit");
                    Console.WriteLine();
                    Console.Write($"Please make a selection (Default is {(int)Testtype + 1}):");
                    bool foundTestType = false;

                    do
                    {
                        string? s = Console.ReadLine();
                        if (string.IsNullOrEmpty(s))
                            break;
                        if (byte.TryParse(s, out byte icmd))
                        {
                            if (icmd > 0 && icmd <= num)
                            {
                                Testtype = (ConsoleTestType)(icmd - 1);
                                foundTestType = true;
                            }
                        }
                        else if (s.ToUpper() == "Q")
                        {
                            foundTestType = true;
                            quit = true;
                        }
                    } while (!foundTestType);

                    if (quit)
                        return;

                    Console.WriteLine($"Testtype: {Testtype}");

                    if (!connected)
                    {
                        bool res = SoftataLib.Connect(IpAddress, Port);
                        if (!res)
                        {
                            connected = false;
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
                            connected = true;
                            Console.WriteLine($"Connected to {IpAddress}:{Port}");
                            Console.WriteLine();
                            Console.WriteLine("Press [Enter] to continue or [Q] to quit");
                            string? key = Console.ReadLine();
                            if (!string.IsNullOrEmpty(key))
                            {
                                if (key.ToUpper() == "Q")
                                    quit = true;
                            }
                        }
                        if (quit)
                            return;
                    }

                    SoftataLib.SendMessageCmd("Begin");
                    Thread.Sleep(500);
                    string Version = SoftataLib.SendMessageCmd("Version");
                    Console.WriteLine($"Softata Version: {Version}");
                    Thread.Sleep(500);
                    string devicesCSV = SoftataLib.SendMessageCmd("Devices");
                    Console.WriteLine($"{devicesCSV}");
                    Thread.Sleep(500);

                    switch (Testtype)
                    {
                        case ConsoleTestType.Test_OTA_Or_WDT:
                            SoftataLib.Digital.SetPinMode(BUTTON, SoftataLib.PinMode.DigitalInput);
                            SoftataLib.Digital.SetPinMode(LED, SoftataLib.PinMode.DigitalOutput);
                            SoftataLib.Digital.SetPinState(LED, SoftataLib.PinState.HIGH);

                            Console.WriteLine("WDT Test: Enable WDT in softata.h, deploy and boot then press [Return]");
                            Console.WriteLine("OTA Test: Enable OTA (optonally disable WDT) in softata.h, deploy and boot then press [Return]");
                            Console.ReadLine();
                            Console.WriteLine("4 LED toggles then WDT.Update/OTA.handle turned off, with busy wait on device,for 100 secs");

                            for (int i = 0; i < 4; i++)
                            {
                                SoftataLib.Digital.TogglePinState(LED);
                                Console.WriteLine($"{i} secs");
                                Thread.Sleep(1000);
                            }
                            Console.WriteLine($"Turning off WDT/OTA Updates with busy wait on device. Press [Enter]");
                            Console.ReadLine();
                            SoftataLib.Digital.TurnOffWDTUpdates();
                            for (int i = 0; i < 100; i++)
                            {
                                Console.WriteLine($"{i} secs");
                                SoftataLib.Digital.TogglePinState(LED);
                                Thread.Sleep(1000);
                            }
                            break;
                        // LED-Button test
                        case ConsoleTestType.Digital_Button_and_LED:
                            SoftataLib.Digital.SetPinMode(BUTTON, SoftataLib.PinMode.DigitalInput);
                            SoftataLib.Digital.SetPinMode(LED, SoftataLib.PinMode.DigitalOutput);
                            SoftataLib.Digital.SetPinState(LED, SoftataLib.PinState.HIGH);

                            // Next is errant as no pin 50 on Pico
                            //Digital_Button_and_LED.SetAnalogPin(50, PinMode.DigitalInput);

                            // Next is errant as no such command
                            //SoftataLib.SendMessage(SoftataLib.Commands.Undefined, (byte)26, (byte)PinMode.DigitalInput);

                            for (int i = 0; i < 0x10; i++)
                            {
                                while (SoftataLib.Digital.GetPinState(BUTTON))
                                    Thread.Sleep(100);
                                SoftataLib.Digital.TogglePinState(LED);
                            }
                            break;

                        // Potentiometer-LED Test
                        case ConsoleTestType.Analog_Potentiometer_and__LED:
                        case ConsoleTestType.PWM:
                            // Note no pin setup needed for analog
                            SoftataLib.Digital.SetPinMode(LED, SoftataLib.PinMode.DigitalOutput);
                            for (int i = 0; i < 50; i++)
                            {
                                int val = SoftataLib.Analog.AnalogRead(POTENTIOMETER);
                                if (val != int.MaxValue)
                                {
                                    Console.WriteLine($"AnalogRead({POTENTIOMETER}) = {val}");
                                    byte pwmVal = (byte)(val >> 2);
                                    if (val > 1023)
                                        pwmVal = 255;
                                    SoftataLib.PWM.SetPWM(LED, (byte)(pwmVal));
                                }
                                else
                                    Console.WriteLine($"AnalogRead({POTENTIOMETER}) failed");
                                Console.WriteLine();
                                Thread.Sleep(500);
                            }
                            break;
                        case ConsoleTestType.Loopback:
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
                                else if (!SoftataLib.Baudrates.Contains(baudRate))
                                    Console.WriteLine("Invalid");
                            } while (!SoftataLib.Baudrates.Contains(baudRate));


                            SoftataLib.Serial.serialSetup(txPins[1], baudRate, 1);
                            SoftataLib.Serial.serialSetup(txPins[2], baudRate, 2);


                            if (iserialMode == 1) // ASCII test
                            {
                                for (char sendCh = ' '; sendCh <= '~'; sendCh++)
                                {
                                    char recvCh;
                                    SoftataLib.Serial.serialWriteChar(comTx, sendCh);
                                    Thread.Sleep(100);
                                    recvCh = SoftataLib.Serial.serialGetChar(comRx);
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
                                    SoftataLib.Serial.serialWriteByte(comTx, sendByte);
                                    recvByte = SoftataLib.Serial.serialGetByte(comRx);
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
                                    string msg = SoftataLib.Serial.readLine(comRx, false);
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
                            bool debug = false;
                            string[] Sensors = SoftataLib.Sensor.GetSensors();
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

                                string pins = SoftataLib.Sensor.GetPins(isensor);
                                if (string.IsNullOrEmpty(pins))
                                    Console.WriteLine($"{sensor} getPins() failed");
                                else
                                {
                                    Console.WriteLine($"{sensor} getPins OK");
                                    Console.WriteLine($"{sensor} Pins = {pins}");
                                }
                                Console.WriteLine("Press any key to setup sensor");
                                Console.ReadLine();
                                byte sensorLinkedListIndex = (byte)SoftataLib.Sensor.SetupDefault(isensor);
                                if (sensorLinkedListIndex < 0)
                                    Console.WriteLine($"Instantiated sensor {sensor} not found");
                                else
                                {
                                    Console.WriteLine($"Instantiated {sensor} found at {sensorLinkedListIndex}");
                                    string[] properties = SoftataLib.Sensor.GetProperties(isensor);
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
                                            string json = SoftataLib.Sensor.GetTelemetry((byte)sensorLinkedListIndex, debug);
                                            Console.WriteLine($"\t\t Telemetry: {json}");
                                            Console.WriteLine("Press [Esc] to stop");
                                            Thread.Sleep((int)period);
                                        }
                                        else if (sensorMode == 3)
                                        {
                                            string indxStr = SoftataLib.Sensor.StartSendingTelemetryBT((byte)sensorLinkedListIndex);
                                            if (int.TryParse(indxStr, out int val))
                                                Console.WriteLine($"Streaming to BT started. List No:{val}");
                                            else
                                                Console.WriteLine($"Streaming to BT failed to start.");
                                            showMenu = true;
                                        }
                                        else if (sensorMode == 4)
                                        {
                                            string indxStr = SoftataLib.Sensor.StartSendingTelemetryToIoTHub((byte)sensorLinkedListIndex);
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
                                            string json = SoftataLib.Sensor.PauseSendTelemetry((byte)sensorLinkedListIndex);
                                            if (!string.IsNullOrEmpty(json))
                                                Console.WriteLine($"json {json}");
                                        }
                                        else if (sensorMode == 6)
                                        {
                                            string json = SoftataLib.Sensor.ContinueSendTelemetry((byte)sensorLinkedListIndex);
                                            if (!string.IsNullOrEmpty(json))
                                                Console.WriteLine($"json {json}");
                                        }

                                        else if (sensorMode == 1)
                                        {
                                            double[]? values = SoftataLib.Sensor.ReadAll((byte)sensorLinkedListIndex, debug);
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
                                                double? value = SoftataLib.Sensor.Read((byte)sensorLinkedListIndex, p, debug);
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
                            byte idisplay = 0;
                            string display = "";
                            string[] Miscs = new string[0];
                            string[] Displays = SoftataLib.Display.GetDisplays();
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

                                string pins = SoftataLib.Display.GetPins(idisplay);
                                if (string.IsNullOrEmpty(pins))
                                    Console.WriteLine($"{display} getPins() failed");
                                else
                                {
                                    Console.WriteLine($"{display} getPins OK");
                                    Console.WriteLine($"{display} Pins = {pins}");
                                }

                                Miscs = SoftataLib.Display.GetMiscCmds(idisplay);
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

                            byte numPixels = SoftataLib.Display.Neopixel.MaxNumPixels;

                            // Only do non-default setup for Neopixel
                            if (displayDevice == DisplayDevice.NEOPIXEL)
                            {
                                Console.WriteLine($"Select number of Pixels:");
                                for (byte i = 1; i <= SoftataLib.Display.Neopixel.MaxNumPixels; i++)
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
                                displayLinkedListIndex = (byte)SoftataLib.Display.Setup(idisplay, 16, numPixels);
                            }
                            else if (displayDevice == DisplayDevice.BARGRAPH)
                            {

                                // Use default settings
                                //displayLinkedListIndex = (byte)SoftataLib.Display.SetupDefault(idisplay); 
                                //Or use custom settings: {data,latch,clock} GPIO Pins
                                List<byte> settings = new List<byte> { 20, 21 }; // Send the  data pin as 16
                                displayLinkedListIndex = (byte)SoftataLib.Display.Setup(idisplay, 16, settings);
                            }
                            else
                                displayLinkedListIndex = (byte)SoftataLib.Display.SetupDefault(idisplay);

                            Console.WriteLine($"displayLinkedListIndex: {displayLinkedListIndex}");
                            if (displayLinkedListIndex < 0)
                                Console.WriteLine($"Instantiated sensor {display} not found");
                            else
                            {
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
                                        Console.WriteLine("Default: 0.");
                                        Console.Write("Selection:");
                                        bool found = false;

                                        do
                                        {
                                            string? s = Console.ReadLine();
                                            if (byte.TryParse(s, out byte imis))
                                            {
                                                if ((imis >= 0) && (imis <= Miscs.Length)) // && (idis != 1))
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
                                        if (imisc < Miscs.Length)
                                        {
                                            misc = Miscs[imisc];
                                            Console.WriteLine($"Selected {misc} Misc command");
                                            Console.WriteLine("Note: Not passing any params to Misc command at this stage.");
                                            bool result = SoftataLib.Display.Misc(displayLinkedListIndex, (byte)imisc);
                                            if (result)
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
                                            Console.WriteLine($"Instantiated {display} linked at {displayLinkedListIndex}");
                                            SoftataLib.Display.Neopixel.Clear(displayLinkedListIndex); ;
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Misc_SetAll(displayLinkedListIndex, 255, 0, 0);
                                            Thread.Sleep(100);
                                            SoftataLib.Display.Neopixel.Misc_SetAll(displayLinkedListIndex, 0xFF, 0xA5, 0);   //Orange
                                            Thread.Sleep(100);
                                            SoftataLib.Display.Neopixel.Misc_SetAll(displayLinkedListIndex, 255, 255, 0);     //Yellow
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Misc_SetAll(displayLinkedListIndex, 0, 255, 0);
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Misc_SetAll(displayLinkedListIndex, 0, 0, 255);
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Misc_SetAll(displayLinkedListIndex, 0xA0, 0x20, 0xf0);//Purple
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Misc_SetAll(displayLinkedListIndex, 255, 255, 255);   //White
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Misc_SetOdd(displayLinkedListIndex, 255, 0, 0);
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(100);
                                            SoftataLib.Display.Neopixel.Misc_SetEvens(displayLinkedListIndex, 0, 255, 0);
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Misc_SetOdd(displayLinkedListIndex, 0, 0, 255);
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(100);
                                            SoftataLib.Display.Neopixel.Misc_SetEvens(displayLinkedListIndex, 255, 255, 0);
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Misc_SetOdd(displayLinkedListIndex, 0, 255, 255);
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(100);
                                            SoftataLib.Display.Neopixel.Misc_SetEvens(displayLinkedListIndex, 255, 255, 255);
                                            Thread.Sleep(500);
                                            SoftataLib.Display.Neopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(500);
                                            SoftataLib.Display.Neopixel.Misc_SetEvens(displayLinkedListIndex, 255, 255, 255);
                                            Thread.Sleep(500);
                                            SoftataLib.Display.Neopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(500);
                                            SoftataLib.Display.Neopixel.Misc_SetEvens(displayLinkedListIndex, 255, 255, 255);
                                            Thread.Sleep(2000);
                                            SoftataLib.Display.Neopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(500);
                                            for (byte n = 0; n <= SoftataLib.Display.Neopixel.MaxNumPixels; n++)
                                            {
                                                SoftataLib.Display.Neopixel.Misc_SetN(displayLinkedListIndex, 255, 255, 255, n);
                                                Thread.Sleep(1000);
                                            }
                                            SoftataLib.Display.Neopixel.Clear(displayLinkedListIndex);
                                            Thread.Sleep(500);
                                            Console.WriteLine("OK");
                                        }
                                        break;
                                    case DisplayDevice.LCD1602:
                                        {
                                            Console.WriteLine($"Instantiated {display} linked at {displayLinkedListIndex}");
                                            SoftataLib.Display.Clear(displayLinkedListIndex);
                                            SoftataLib.Display.SetCursor(displayLinkedListIndex, 0, 0);
                                            SoftataLib.Display.WriteString(displayLinkedListIndex, "First Line");
                                            SoftataLib.Display.SetCursor(displayLinkedListIndex, 0, 1);
                                            SoftataLib.Display.WriteString(displayLinkedListIndex, "Wait 5sec");
                                            Thread.Sleep(5000);
                                            SoftataLib.Display.Clear(displayLinkedListIndex);
                                            SoftataLib.Display.WriteString(displayLinkedListIndex, 4, 0, "(4,0):Cursor");
                                            SoftataLib.Display.WriteString(displayLinkedListIndex, 2, 1, "(2,1):Write");
                                            Thread.Sleep(5000);
                                            {
                                                SoftataLib.Display.WriteString(displayLinkedListIndex, 2, 1, "(2,1):Write");
                                                Thread.Sleep(1000);
                                                if (true)
                                                {
                                                    SoftataLib.Display.Clear(displayLinkedListIndex);
                                                    Thread.Sleep(1000);
                                                    SoftataLib.Display.SetCursor(displayLinkedListIndex, 0, 0);

                                                    SoftataLib.Display.WriteString(displayLinkedListIndex, "Time:");
                                                    int numTimes = 10;
                                                    for (int i = 0; i < numTimes; i++)
                                                    {
                                                        DateTime now = DateTime.Now;
                                                        string format = "HH:mm:ss";
                                                        string time = now.ToString(format);
                                                        string msg = $"{i + 1}/{numTimes} {time}";
                                                        SoftataLib.Display.WriteString(displayLinkedListIndex, 0, 1, msg);
                                                        Thread.Sleep(1000);
                                                    }
                                                }
                                            }
                                        }
                                        SoftataLib.Display.Clear(displayLinkedListIndex);
                                        Thread.Sleep(1000);
                                        break;
                                    case DisplayDevice.OLED096:
                                        SoftataLib.Display.Clear(displayLinkedListIndex);
                                        Thread.Sleep(500);
                                        //Dummy test to see if simple Misc test works (with no date).
                                        SoftataLib.Display.Oled096.misctest(displayLinkedListIndex);
                                        Thread.Sleep(1000);
                                        SoftataLib.Display.Oled096.drawFrame(displayLinkedListIndex);
                                        Thread.Sleep(4000);
                                        SoftataLib.Display.Oled096.drawCircle(displayLinkedListIndex);
                                        Thread.Sleep(4000);
                                        SoftataLib.Display.Oled096.drawCircle(displayLinkedListIndex, 60, 32, 10);
                                        Thread.Sleep(4000);
                                        SoftataLib.Display.Clear(displayLinkedListIndex);
                                        Thread.Sleep(500);
                                        SoftataLib.Display.WriteString(displayLinkedListIndex, 0, 0, "Hello Joe!");
                                        Thread.Sleep(2500);
                                        SoftataLib.Display.WriteString(displayLinkedListIndex, 1, 1, "Hi there!");
                                        Thread.Sleep(2500);
                                        SoftataLib.Display.Clear(displayLinkedListIndex);
                                        Thread.Sleep(500);
                                        int numbr = 10;
                                        for (int i = 0; i < numbr; i++)
                                        {
                                            DateTime now = DateTime.Now;
                                            string format = "HH:mm:ss";
                                            string time = now.ToString(format);
                                            string msg = $"{i + 1}/{numbr} {time}";
                                            SoftataLib.Display.WriteString(displayLinkedListIndex, 0, 1, msg);
                                            Thread.Sleep(2000);
                                        }
                                        SoftataLib.Display.Clear(displayLinkedListIndex);
                                        SoftataLib.Display.Home(displayLinkedListIndex);
                                        SoftataLib.Display.WriteString(displayLinkedListIndex, 0, 1, "Done");
                                        Thread.Sleep(500);

                                        break;
                                    case DisplayDevice.BARGRAPH:
                                        Console.WriteLine($"Instantiated {display} linked at {displayLinkedListIndex}");
                                        SoftataLib.Display.Clear(displayLinkedListIndex);
                                        Thread.Sleep(1000);
                                        SoftataLib.Display.WriteString(displayLinkedListIndex, "1");
                                        Thread.Sleep(1000);
                                        SoftataLib.Display.WriteString(displayLinkedListIndex, "2");
                                        Thread.Sleep(1000);
                                        SoftataLib.Display.WriteString(displayLinkedListIndex, "4");
                                        Thread.Sleep(1000);
                                        SoftataLib.Display.WriteString(displayLinkedListIndex, "8");
                                        Thread.Sleep(1000);
                                        SoftataLib.Display.WriteString(displayLinkedListIndex, "16");
                                        Thread.Sleep(1000);
                                        SoftataLib.Display.WriteString(displayLinkedListIndex, "32");
                                        Thread.Sleep(1000);
                                        SoftataLib.Display.WriteString(displayLinkedListIndex, "64");
                                        Thread.Sleep(1000);
                                        SoftataLib.Display.WriteString(displayLinkedListIndex, "128");
                                        Thread.Sleep(1000);
                                        SoftataLib.Display.Clear(displayLinkedListIndex);
                                        Thread.Sleep(1000);
                                        break;
                                }
                            }
                            break;
                        case ConsoleTestType.Analog_Potentiometer_Light_and_Sound:
                            SoftataLib.Analog.InitAnalogDevicePins(SoftataLib.Analog.RPiPicoMode.groveShield);
                            SoftataLib.Analog.SetAnalogPin(SoftataLib.Analog.AnalogDevice.Potentiometer, POTENTIOMETER, 1023);
                            SoftataLib.Analog.SetAnalogPin(SoftataLib.Analog.AnalogDevice.LightSensor, LIGHTSENSOR);
                            SoftataLib.Analog.SetAnalogPin(SoftataLib.Analog.AnalogDevice.SoundSensor, SOUNDSENSOR);
                            for (int i = 0; i < 100; i++)
                            {
                                double value;
                                value = SoftataLib.Analog.AnalogReadLightSensor();
                                if (value != double.MaxValue)
                                    Console.WriteLine($"\tLight: {value:0.##}");
                                else
                                    Console.WriteLine($"\tLight Sensor: failed");
                                Thread.Sleep(2000);

                                value = SoftataLib.Analog.AnalogReadPotentiometer();
                                if (value != double.MaxValue)
                                    Console.WriteLine($"\tPotentiometer: {value:0.##}");
                                else
                                    Console.WriteLine($"\tPotentiometer: failed");
                                Thread.Sleep(2000);

                                value = SoftataLib.Analog.AnalogReadSoundSensor();
                                if (value != double.MaxValue)
                                    Console.WriteLine($"\tSound: {value:0.##}");
                                else
                                    Console.WriteLine($"\tSound Sensor: failed");
                                Thread.Sleep(2000);
                            }
                            break;
                        case ConsoleTestType.Potentiometer_and_Actuator:
                            string[] Actuators = SoftataLib.Actuator.GetActuators();
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
                                switch (actuatorDevice)
                                {
                                    case ActuatorDevice.SIPO_74HC595:
                                        {
                                            Console.WriteLine();
                                            Console.WriteLine("Potentiometer-Relay Test");
                                            Console.WriteLine("Potentiometer controls relay. On if >50%");
                                            Console.WriteLine("Potentiometer connected to A0, Relay to D16/18 or 20");

                                            Console.WriteLine("1. D16: Digital IO (Default)");
                                            Console.WriteLine("2. Actuator-Relay Class( Default Settings (Pin 16)");
                                            Console.WriteLine("3. Actuator-Relay Class( Setting: (Pin 18");
                                            Console.WriteLine("4. Actuator-Relay Class( Setting: (Pin 20)");
                                            Console.WriteLine("5. Quit");

                                            Console.Write("Selection:");
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
                                                    SoftataLib.Digital.SetPinMode(RELAY, SoftataLib.PinMode.DigitalOutput);
                                                    break;
                                                case 2:
                                                    actuatorIndex = (byte)SoftataLib.Actuator.SetupDefault(SoftataLib.Actuator.ActuatorDevice.Relay);
                                                    break;
                                                case 3:
                                                    relayPin = 18;
                                                    actuatorIndex = (byte)SoftataLib.Actuator.Setup(SoftataLib.Actuator.ActuatorDevice.Relay, relayPin);
                                                    break;
                                                case 4:
                                                    relayPin = 20;
                                                    actuatorIndex = (byte)SoftataLib.Actuator.Setup(SoftataLib.Actuator.ActuatorDevice.Relay, relayPin);
                                                    break;
                                            }



                                            SoftataLib.Analog.InitAnalogDevicePins(SoftataLib.Analog.RPiPicoMode.groveShield);
                                            SoftataLib.Analog.SetAnalogPin(SoftataLib.Analog.AnalogDevice.Potentiometer, POTENTIOMETER, 1023);
                                            Console.WriteLine("Press any key to continue.");
                                            Console.ReadLine();

                                            bool state = false;
                                            Console.WriteLine("Relay OFF");
                                            for (int i = 0; i < 20; i++)
                                            {
                                                double val = SoftataLib.Analog.AnalogReadPotentiometer();
                                                if (val != double.MaxValue)
                                                {
                                                    Console.WriteLine($"AnalogRead({POTENTIOMETER}) = {val:0.##}");
                                                    if (val > 50)
                                                    {
                                                        if (!state)
                                                        {
                                                            Console.WriteLine("\t\t\tRelay ON");
                                                            state = !state;

                                                            switch (potRelaySelection)
                                                            {
                                                                case 1:
                                                                    SoftataLib.Digital.SetPinState(RELAY, SoftataLib.PinState.HIGH);
                                                                    break;
                                                                case 2:
                                                                case 3:
                                                                case 4:
                                                                    SoftataLib.Actuator.SetBit(actuatorIndex, 0);
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (state)
                                                        {
                                                            Console.WriteLine("\t\t\tRelay OFF");
                                                            state = !state;
                                                            switch (potRelaySelection)
                                                            {
                                                                case 1:
                                                                    SoftataLib.Digital.SetPinState(RELAY, SoftataLib.PinState.LOW);
                                                                    break;
                                                                case 2:
                                                                case 3:
                                                                case 4:
                                                                    SoftataLib.Actuator.ClearBit(actuatorIndex, 0);
                                                                    break;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                    Console.WriteLine($"\t\tAnalogRead({POTENTIOMETER}) failed");
                                                Console.WriteLine();
                                                Thread.Sleep(2000);
                                            }
                                        }
                                        break;
                                    case ActuatorDevice.Relay:
                                        {
                                            Console.WriteLine();
                                            Console.WriteLine("Potentiometer 74HC59 SIPO Test"); ;
                                            Console.WriteLine("Potentiometer controls which 8 bits are set");
                                            Console.WriteLine("0% Pot = none, 50% Pot = 4 bits, 100% = all 8 bits etc.");
                                            Console.WriteLine("Potentiometer connected to A0");
                                            Console.WriteLine("Default '74HC595 setup:");
                                            Console.WriteLine("595 Datapin = 16"); //Not 18 as in blog post
                                            Console.WriteLine("505 LatchPin = 20");
                                            Console.WriteLine("595 ClockPin = 21");


                                            Console.WriteLine("1. Default setup");
                                            Console.WriteLine("5. Quit");

                                            Console.Write("Selection:");
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
                                                    actuatorParaIndex = (byte)SoftataLib.Actuator.SetupDefault(SoftataLib.Actuator.ActuatorDevice.Shift95ParaOut);
                                                    break; ;
                                            }



                                            SoftataLib.Analog.InitAnalogDevicePins(SoftataLib.Analog.RPiPicoMode.groveShield);
                                            SoftataLib.Analog.SetAnalogPin(SoftataLib.Analog.AnalogDevice.Potentiometer, POTENTIOMETER, 1023);
                                            Console.WriteLine("Press any key to continue.");
                                            Console.ReadLine();

                                            Console.WriteLine("Setting 0 to 8 bits depending upon Pot.");
                                            bool paraState = false;
                                            byte prevBits = 0;
                                            SoftataLib.Actuator.ActuatorWrite(actuatorParaIndex, prevBits);
                                            Console.WriteLine($"\t\t\tClear all 8 bits");
                                            for (int i = 0; i < 30; i++)
                                            {
                                                double val = SoftataLib.Analog.AnalogReadPotentiometer();
                                                double max = 91.3;
                                                double min = 0.29;
                                                byte bits = (byte)(256 * (val - min) / (max - min));
                                                Console.WriteLine($"AnalogRead({POTENTIOMETER}) = {val:0.##}");
                                                Console.Write("\t\t\tBits: ");
                                                string hex = Convert.ToString(bits, 2);
                                                string paddedString = hex.PadLeft(8, '0');
                                                Console.WriteLine(paddedString);
                                                Thread.Sleep(1000);
                                            }

                                            prevBits = 0;
                                            SoftataLib.Actuator.ActuatorWrite(actuatorParaIndex, prevBits);
                                            Console.WriteLine($"\t\t\tClear all 8 bits");
                                            for (int i = 0; i < 30; i++)
                                            {
                                                double val = SoftataLib.Analog.AnalogReadPotentiometer();
                                                double max = 91.3;
                                                double min = 0.29;
                                                byte bits = (byte)(8 * (val - min) / (max - min));
                                                Console.WriteLine($"AnalogRead({POTENTIOMETER}) = {val:0.##}");
                                                Console.Write("\t\t\tBits: ");
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
                                                Console.WriteLine(pat);
                                                Thread.Sleep(1000);
                                            }
                                        }
                                        break;
                                    case ActuatorDevice.Servo:
                                        Console.WriteLine("Connect Servo to D16");
                                        Console.WriteLine("Press any key to continue.");
                                        Console.ReadLine();
                                        byte id = (byte)SoftataLib.Actuator.SetupDefault(SoftataLib.Actuator.ActuatorDevice.Servo);
                                        for (int i = 0; i < 2; i++)
                                        {
                                            SoftataLib.Actuator.ActuatorWrite(id, 90);
                                            Console.WriteLine($"\t\t\tAngle: 90");
                                            Thread.Sleep(2000);
                                            SoftataLib.Actuator.ActuatorWrite(id, 180);
                                            Console.WriteLine($"\t\t\tAngle: 180");
                                            Thread.Sleep(2000);
                                            SoftataLib.Actuator.ActuatorWrite(id, 90);
                                            Console.WriteLine($"\t\t\tAngle: 90");
                                            Thread.Sleep(2000);
                                            SoftataLib.Actuator.ActuatorWrite(id, 0);
                                            Console.WriteLine($"\t\t\tAngle: 0");
                                            Thread.Sleep(2000);
                                        }
                                        Console.WriteLine("Connect Servo to D16");
                                        Console.WriteLine("Connect Potentiometer to A0.");
                                        Console.WriteLine("Press any key to continue.");
                                        Console.ReadLine();
                                        SoftataLib.Analog.SetAnalogPin(SoftataLib.Analog.AnalogDevice.Potentiometer, POTENTIOMETER, 1023);
                                        Console.WriteLine("Turn potetiometer full in one direction.");
                                        Console.WriteLine("Press any key to continue.");
                                        Console.ReadLine();
                                        double max1 = SoftataLib.Analog.AnalogReadPotentiometer();
                                        Console.WriteLine("Turn potetiometer full in other direction.");
                                        Console.WriteLine("Press any key to continue.");
                                        Console.ReadLine();
                                        double max2 = SoftataLib.Analog.AnalogReadPotentiometer();
                                        if (max1 > max2)
                                        {
                                            double temp = max2;
                                            max2 = max1;
                                            max1 = temp;
                                        }
                                        Console.WriteLine("Turn potetiometer periodically.");
                                        Console.WriteLine("Press any key to start.");
                                        Console.ReadLine();
                                        Console.WriteLine("Runs for 20 steps.");
                                        for (int i = 0; i < 20; i++)
                                        {
                                            double val = SoftataLib.Analog.AnalogReadPotentiometer();
                                            byte angle = (byte)(180 * (val - max1) / (max2 - max1));
                                            Console.WriteLine($"\t\t\t\tAngle: {angle}");

                                            SoftataLib.Actuator.ActuatorWrite(id, angle);
                                            Thread.Sleep(500);
                                        }


                                        break;
                                    default:
                                        Console.WriteLine("Actuator not yet implemented for this test.");
                                        break;
                                }
                                Console.WriteLine("Finished test");
                                Console.WriteLine("Press any key to continue.");
                                Console.ReadLine();


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
                SoftataLib.SendMessageCmd("End");
                Thread.Sleep(500);
            }

            return;
        }
    }
}
