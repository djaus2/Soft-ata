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
using static Softata.SoftataLib;
using System.Runtime.ConstrainedExecution;

namespace FirmataBasic
{
    internal class Program
    {
        // Set the same as Arduino:
        const int port = 4242;
        const string ipaddressStr = "192.168.0.11";

        // Configure hardware pin connections thus:
        static byte LED = 12;
        static byte BUTTON = 13;
        static byte POTENTIOMETER = 26;//A0
        static byte LIGHTSENSOR = 27;  //A1
        static byte SOUNDSENSOR = 28;  //A2 
        static byte RELAY = 16;

        // Choose test DEFAULT type (There is now a menu to select from)
        //static SoftataLib.CommandType Testtype = CommandType.Sensors;
        //static SoftataLib.CommandType Testtype = CommandType.PotLightSoundAnalog;
        //static SoftataLib.CommandType Testtype = CommandType.LCD1602Display;
        //static SoftataLib.CommandType Testtype = CommandType.NeopixelDisplay;
        //static SoftataLib.CommandType Testtype = CommandType.Digital;
        //static SoftataLib.CommandType Testtype = CommandType.Serial;
        static SoftataLib.CommandType Testtype = CommandType.PotRelay;
        //Set Serial1 or Serial2 for send and receive.
        //Nb: If both true or both false then loopback on same serial port.
        static bool Send1 = true;
        static bool Recv1 = true;
        // Next two are the same test
        //static SoftataLib.CommandType Testtype = CommandType.Analog;
        //static SoftataLib.CommandType Testtype = CommandType.PWM;

        static void Main(string[] args)
        {

            Console.WriteLine("Hello from Soft-ata!");
            Console.WriteLine();
            Console.WriteLine("TESTS");
            try
            {
                int num = 0;
                for (int i = 0; i < (int)CommandType.MaxType; i++)
                {
                    num++;

                    CommandType cmd = (CommandType)i;
                    Console.WriteLine($"{i + 1}.\t\t{cmd}");
                }
                Console.WriteLine();
                Console.Write($"Please make a selection (Default is {(int)Testtype+1}):");
                bool foundTestType = false; 
                do
                {
                    string? s = Console.ReadLine();
                    if(string.IsNullOrEmpty(s))
                        break;
                    if (byte.TryParse(s, out byte icmd))
                    {
                        if (icmd > 0 && icmd <= num)
                        {
                            Testtype = (CommandType)(num - 1);
                            foundTestType = true;
                        }
                    }
                } while (!foundTestType);

                Console.WriteLine($"Testtype: {Testtype}");

                SoftataLib.Init(ipaddressStr, port);

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
                    // LED-Button test
                    case CommandType.Digital:
                        SoftataLib.Digital.SetPinMode(BUTTON, SoftataLib.PinMode.DigitalInput);
                        SoftataLib.Digital.SetPinMode(LED, SoftataLib.PinMode.DigitalOutput);
                        SoftataLib.Digital.SetPinState(LED, SoftataLib.PinState.HIGH);

                        // Next is errant as no pin 50 on Pico
                        //Digital.SetAnalogPin(50, PinMode.DigitalInput);

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
                    case CommandType.Analog:
                    case CommandType.PWM:
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
                    case CommandType.Serial:
                        byte[] txPins = new byte[] { 0, 0, 4 }; //Nb: Recv are Tx+1
                        SoftataLib.Serial.serialSetup(txPins[1], 9600, 1);
                        SoftataLib.Serial.serialSetup(txPins[2], 9600, 2);

                        byte comTx = 1;
                        if (!Send1)
                            comTx = 2;
                        byte comRx = 1;
                        if (!Recv1)
                            comRx = 2;

                        if (true) // ASCII test
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
                        else // Byte test
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
                        break;
                    case CommandType.Sensors:
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
                                        isensor = (byte)(isen-1);
                                        found = true;
                                    }
                                }
                            }  while (!found);
                            string sensor = Sensors[isensor];

                            SoftataLib.Sensor.GetPins(isensor);
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
                                string pins = SoftataLib.Sensor.GetPins(isensor);
                                if (string.IsNullOrEmpty(pins))
                                    Console.WriteLine($"{sensor} getPins() failed");
                                else
                                {
                                    Console.WriteLine($"{sensor} getPins OK");
                                    Console.WriteLine($"{sensor} Pins = {pins}");
                                }
                                Console.WriteLine("Press any key to continue.");
                                Console.ReadLine();
                                Console.WriteLine();

                                while (true) { 
                                    double[]? values = SoftataLib.Sensor.ReadAll((byte)sensorLinkedListIndex);
                                    if (values == null)
                                        Console.WriteLine($"{sensor} readAll() failed");
                                    else
                                    {
                                        Console.WriteLine($"{sensor} readAll OK");
                                        for (int p = 0; p < properties.Length; p++)
                                            Console.WriteLine($"\t\t{sensor} {properties[p]} = {values[p]}");
                                    }
                                    Console.WriteLine();
                                    for (byte p = 0; p < properties.Length; p++)
                                    {
                                        double? value = SoftataLib.Sensor.Read((byte)sensorLinkedListIndex, p);
                                        if (value == null)
                                            Console.WriteLine($"{sensor} read() failed");
                                        else
                                            Console.WriteLine($"\t\t\t{sensor} {properties[p]} = {value}");
                                        Console.WriteLine();
                                    }
                                    Thread.Sleep(6000);
                                }
                            }
                        }
                        break;
                    case CommandType.NeopixelDisplay:
                        string[] Displays = SoftataLib.Display.GetDisplays();
                        if (Displays.Length == 0)
                            Console.WriteLine($"No displays found");
                        else
                        {
                            Console.WriteLine($"Displays found:");
                            for (byte i = 0; i < Displays.Length; i++)
                            {
                                string display = Displays[i];
                                Console.WriteLine($"Display = {display}");
                            }
                            for (byte i = 2; i < 3; i++) // Neopixel only
                            {
                                string display = Displays[i];
                                Console.WriteLine($"Display = {display}");
                                SoftataLib.Display.GetPins(i);
                                byte displayLinkedListIndex = (byte)SoftataLib.Display.SetupDefault(i);
                                Console.WriteLine($"displayLinkedListIndex: {displayLinkedListIndex}");
                                if (displayLinkedListIndex < 0)
                                    Console.WriteLine($"Instantiated sensor {display} not found");
                                else
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
                                    Console.WriteLine("OK");
                                }
                            }
                        }
                        break;
                    case CommandType.LCD1602Display:
                        string[] Displays2 = SoftataLib.Display.GetDisplays();
                        if (Displays2.Length == 0)
                            Console.WriteLine($"No displays found");
                        else
                        {
                            Console.WriteLine($"Displays found:");
                            for (byte i = 0; i < Displays2.Length; i++)
                            {
                                string display = Displays2[i];
                                Console.WriteLine($"Display = {display}");
                            }
                            for (byte i = 1; i < 2; i++) // LCD1602 only
                            {
                                string display = Displays2[i];
                                Console.WriteLine($"Display = {display}");
                                SoftataLib.Display.GetPins(i);
                                byte displayLinkedListIndex = (byte)SoftataLib.Display.SetupDefault(i);
                                Console.WriteLine($"displayLinkedListIndex: {displayLinkedListIndex}");
                                if (displayLinkedListIndex < 0)
                                    Console.WriteLine($"Instantiated sensor {display} not found");
                                else
                                {
                                    Console.WriteLine($"Instantiated {display} linked at {displayLinkedListIndex}");
                                    SoftataLib.Display.Clear(displayLinkedListIndex);
                                    SoftataLib.Display.SetCursor(displayLinkedListIndex,0, 0);
                                    SoftataLib.Display.WriteString(displayLinkedListIndex,"First Line");
                                    SoftataLib.Display.SetCursor(displayLinkedListIndex, 0, 1);
                                    SoftataLib.Display.WriteString(displayLinkedListIndex,"Wait 5sec");
                                    Thread.Sleep(5000);
                                    SoftataLib.Display.Clear(displayLinkedListIndex);
                                    SoftataLib.Display.WriteString(displayLinkedListIndex,4,0, "(4,0):Cursor");
                                    SoftataLib.Display.WriteString(displayLinkedListIndex, 2, 1, "(2,1):Write");
                                }
                            }
                        }
                        break;
                    case CommandType.PotLightSoundAnalog:
                        SoftataLib.Analog.InitAnalogDevicePins(Analog.RPiPicoMode.groveShield);
                        SoftataLib.Analog.SetAnalogPin(Analog.AnalogDevice.Potentiometer, POTENTIOMETER, 1023);
                        SoftataLib.Analog.SetAnalogPin(Analog.AnalogDevice.LightSensor, LIGHTSENSOR);
                        SoftataLib.Analog.SetAnalogPin(Analog.AnalogDevice.SoundSensor, SOUNDSENSOR);
                        for (int i=0;i<100;i++)
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
                    case CommandType.PotRelay:
                        Console.WriteLine();
                        Console.WriteLine("Potentiometer-Relay Test");
                        Console.WriteLine("Potentiometer controls relay. On if >50%");
                        Console.WriteLine("Potentiometer connected to A0, Relay to D16");
                        SoftataLib.Analog.InitAnalogDevicePins(Analog.RPiPicoMode.groveShield);
                        SoftataLib.Analog.SetAnalogPin(Analog.AnalogDevice.Potentiometer, POTENTIOMETER, 1023);
                        Console.WriteLine("Press any key to continue.");
                        Console.ReadLine();
                        SoftataLib.Digital.SetPinMode(RELAY, SoftataLib.PinMode.DigitalOutput);
                        bool state = false;
                        Console.WriteLine("Relay OFF");
                        for (int i = 0; i < 20; i++)
                        {
                            double val = SoftataLib.Analog.AnalogReadPotentiometer();
                            if (val != double.MaxValue)
                            {
                                Console.WriteLine($"AnalogRead({POTENTIOMETER}) = {val:0.##}");
                                if(val > 50)
                                {
                                    if (!state)
                                    {
                                        Console.WriteLine("\t\t\tRelay ON");
                                        state = true;
                                        SoftataLib.Digital.SetPinState(RELAY, SoftataLib.PinState.HIGH);
                                    }
                                }
                                else
                                {
                                    if (state)
                                    {
                                        Console.WriteLine("\t\t\tRelay OFF");
                                        state = false;
                                        SoftataLib.Digital.SetPinState(RELAY, SoftataLib.PinState.LOW);
                                    }
                                }
                            }
                            else
                                Console.WriteLine($"\t\tAnalogRead({POTENTIOMETER}) failed");
                            Console.WriteLine();
                            Thread.Sleep(2000);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            SoftataLib.SendMessageCmd("End");
            Thread.Sleep(500);

            return;
        }
    }
}
