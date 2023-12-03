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
        const int port = 4242;
        const string ipaddressStr = "192.168.0.13";
 
        static void Main(string[] args)
        {
            SoftataLib.Init(ipaddressStr, port);

            Console.WriteLine("Hello from Soft-ata!");
            try
            {

                SoftataLib.SendMessageCmd("Begin");
                
                Thread.Sleep(500);

                //SoftataLib.Digital.SetPinMode(13, SoftataLib.PinMode.DigitalInput);
                SoftataLib.Digital.SetPinMode(12, SoftataLib.PinMode.DigitalOutput);
                //SoftataLib.Digital.SetPinState(12, SoftataLib.PinState.HIGH);

                // Next is errant as no pin 50 on Pico
                //Digital.SetPinMode(50, PinMode.DigitalInput);

                // Next is errant as no such command
                //SoftataLib.SendMessage(SoftataLib.Commands.Undefined, (byte)26, (byte)PinMode.DigitalInput);


                // Next is now errant
                //SoftataLib.SendMessage(SoftataLib.Commands.analogWrite, (byte)26, (byte)PinMode.DigitalInput);
                Thread.Sleep(500);

                int n;
                int ADPin = 26;
                for (int i = 0; i < 10; i++)
                {
                    //SoftataLib.Digital.GetPinState(13);
                    //SoftataLib.Digital.TogglePinState(12);
                    //SoftataLib.Digital.TogglePinState(12);
                    int val  =SoftataLib.Analog.AnalogRead(ADPin);
                    if (val != int.MaxValue)
                    {
                        Console.WriteLine($"AnalogRead({ADPin}) = {val}");
                        SoftataLib.PWM.SetPWM(12, (byte) (val/4));
                    }
                    else
                        Console.WriteLine($"AnalogRead({ADPin}) failed");
                    Console.WriteLine();
                    Thread.Sleep(2000);
                }

                SoftataLib.SendMessageCmd("End");
                Thread.Sleep(500);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
