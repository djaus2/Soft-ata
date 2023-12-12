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

namespace FirmataBasic
{
    internal class Program
    {
        // Set the same as Arduino:
        const int port = 4242;
        const string ipaddressStr = "192.168.0.13";

        // Configure hardware pin connections thus:
        static byte LED = 12;
        static byte BUTTON = 13;
        static byte POTENTIOMETER = 26;

        // Choose test type
        //static SoftataLib.CommandType Testtype = CommandType.Digital;
        static SoftataLib.CommandType Testtype = CommandType.Serial;
        //Set Serial1 or Serial2 for send and receive.
        //Nb: If both true or both false then loopback on same serial port.
        static bool Send1 = true;
        static bool Recv1 = true;
        // Next two are the same test
        //static SoftataLib.CommandType Testtype = CommandType.Analog;
        //static SoftataLib.CommandType Testtype = CommandType.PWM;

        static void Main(string[] args)
        {
            SoftataLib.Init(ipaddressStr, port);

            Console.WriteLine("Hello from Soft-ata!");
            try
            {

                SoftataLib.SendMessageCmd("Begin");               
                Thread.Sleep(500);

                switch (Testtype)
                {
                    // LED-Button test
                    case CommandType.Digital:
                        SoftataLib.Digital.SetPinMode(BUTTON, SoftataLib.PinMode.DigitalInput);
                        SoftataLib.Digital.SetPinMode(LED, SoftataLib.PinMode.DigitalOutput);
                        SoftataLib.Digital.SetPinState(LED, SoftataLib.PinState.HIGH);

                        // Next is errant as no pin 50 on Pico
                        //Digital.SetPinMode(50, PinMode.DigitalInput);

                        // Next is errant as no such command
                        //SoftataLib.SendMessage(SoftataLib.Commands.Undefined, (byte)26, (byte)PinMode.DigitalInput);

                        for (int i = 0; i < 0x10; i++)
                        {
                            while( SoftataLib.Digital.GetPinState(BUTTON))
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
                        byte[] txPins = new byte[] { 0,0,4 }; //Nb: Recv are Tx+1
                        SoftataLib.Serial.serialSetup(txPins[1], 9600, 1);
                        SoftataLib.Serial.serialSetup(txPins[2], 9600, 2);

                        byte comTx = 1;
                        if(!Send1)
                            comTx = 2;
                        byte comRx = 1;
                        if(!Recv1)
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
