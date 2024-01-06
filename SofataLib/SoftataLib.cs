using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Data.Common;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net.NetworkInformation;

namespace Softata
{
    public partial class SoftataLib
    {
        //Ref: https://datasheets.raspberrypi.com/pico/Pico-R3-A4-Pinout.pdf
        public const int PinMax = 28;

        public const byte nullData = 0xfe;

        public static  int port { get; set; } = 4242;
        public static string ipAddresStr { get; set; } = "192.168.0.11";

        private static bool Inited = false;
        public  static void Init(string _ipAddresStr, int _port)
        {
            ipAddresStr = _ipAddresStr;
            port = _port;
            //IPHostEntry iphostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAdress = IPAddress.Parse(ipAddresStr);

            IPEndPoint ipEndpoint = new IPEndPoint(ipAdress, port);

            Console.WriteLine("Connecting to Softata Server.");
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client.Connect(ipEndpoint);
                while (!client.Connected)
                {
                    Thread.Sleep(500);
                }

                Console.WriteLine("Socket created to {0}", client.RemoteEndPoint?.ToString());
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (client.Connected)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
                client = null;
            }
        }

        private static Socket? client;

        static SoftataLib()
        {

        }

        // Add commands here that use param = 0xff
        // For others 0xff is not sent
        private static List<Commands> WritingCmds = new List<Commands> { Commands.pwmWrite, Commands.serialWriteChar};


        public enum Commands
        {
            //Digital IO
            pinMode = 0xD0,
            digitalWrite = 0xD1,
            digitalRead = 0xD2,
            digitalToggle = 0xD3,
        
            //Analog/PWM
            analogRead = 0xA2,
            pwmWrite = 0xB1,

            //Serial
            serialSetup = 0xE0, // Setup Serial1/2
            serialGetChar = 0xE1, // Get a char
            serialGetString = 0xE2, // Get a string
            serialGetStringUntil = 0xE3, // Get a string until char
            serialWriteChar = 0xE4, // Write a char
            serialGetFloat = 0xE5, // Get Flost
            serialGetInt = 0xE6, // Get Int

            groveSensor = 0xF0, 
            groveDisplay = 0xF1,
            groveActuator = 0xF2,

            Undefined = 0xFF
        }

        public enum CommandType : byte
        {
            Digital = 0,
            Analog = 1,
            PWM = 2,
            Servo = 3,
            Sensors = 4,
            SPI = 5,
            OneWire = 6,
            Serial = 7,
            NeopixelDisplay = 0x8,
            LCD1602Display = 0x9,
            PotLightSoundAnalog = 0xA,
            Undefined = 0xFF
        }
        public enum PinMode
        {
            Undefined = -1,
            DigitalInput = 0,
            DigitalOutput = 1,
            AnalogInput = 2,
            PwmOutput = 3,
            ServoControl = 4,
            I2C = 6,
            OneWire = 7,
            StepperControl = 8,
            Encoder = 9,
            Serial = 10,
            InputPullup = 11
        }

        public enum PinState
        {
            LOW = 0x00,
            HIGH = 0x01
        }
        public static string SendMessageCmd(string cmd)
        {
            if (client == null)
                throw new Exception("SendMessageCmd: Not connected");

            // Construct command and parameters as list of bytes
            List<byte> sendmsg = new List<byte> { 1, (byte)cmd[0] };

            byte[] sendBytes = sendmsg.ToArray<byte>();
            Console.WriteLine($"Sending {sendBytes.Length} data bytes");
            ;
            int sent = client.Send(sendBytes, 0, sendBytes.Length, SocketFlags.None);
            if (sent != sendBytes.Length)
                throw new Exception($"SendMessage: Sent {sent} bytes, expected {sendBytes.Length} bytes");

            Console.WriteLine($"Sent {sent} bytes");

            //Wait for response
            while (client.Available == 0) ;
            byte[] data = new byte[100];
            int recvd = client.Receive(data);

            string result = Encoding.ASCII.GetString(data).Substring(0, recvd).Trim();
            Console.WriteLine($"Received {result} [{recvd}] bytes\n");

            switch (cmd)
            {
                case "Begin":
                    break;
                case "Version":
                    return result;
                    //break
                case "Devices":
                    return result;
                    //break
                case "Null":
                    break;
                case "End":
                    Thread.Sleep(2000);
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    break;
                case "Reset":
                    break;
            }
            return "";
        }
        public static string SendMessage(Commands MsgType, byte pin = 0xff, byte state = 0xff, string expect = "OK", byte other=0xff, byte[]? Data=null )
        {
            if (client == null)
                throw new Exception("SendMessageCmd: Not connected");

            // Construct command and parameters as list of bytes
            List<byte> sendmsg = new List<byte> { 0,(byte)MsgType };
            if (pin != 0xff)
            {
                sendmsg.Add(pin);
                if ((state != 0xff) || (WritingCmds.Contains(MsgType)))
                {
                    sendmsg.Add(state);
                    if (other != 0xff)
                    {
                        sendmsg.Add(other);
                    }
                }
            }
            if (Data!= null)
            {
                foreach (int d in Data)
                {
                    sendmsg.Add((byte)d);
                }
            }
              
            sendmsg[0] = (byte)(sendmsg.Count-1);
            // Get bytes from list.
            byte[] sendBytes = sendmsg.ToArray<byte>();
            Console.WriteLine($"Sending {sendBytes.Length} data bytes");

            int sent = client.Send(sendBytes, 0, sendBytes.Length, SocketFlags.None);
            if (sent != sendBytes.Length)
                throw new Exception($"SendMessage: Sent {sent} bytes, expected {sendBytes.Length} bytes");

            Console.WriteLine($"Sent {sent} bytes");

            //Wait for response
            while (client.Available == 0) ;
            byte[] data = new byte[100];
            int recvd = client.Receive(data);

            string result = Encoding.UTF8.GetString(data).Substring(0, recvd);

            // Make the check work in both ways.
            if ((!expect.Contains(result)) && (!result.Contains(expect)))
            {
                Console.WriteLine($"Expected {expect} got {result}... rebooting");
                SendMessageCmd("Reset");
                recvd = client.Receive(data);
                result = Encoding.ASCII.GetString(data).Substring(0, recvd).Trim();
                Console.WriteLine($"Received {result} [{recvd}] bytes\n");
                return "Reset";
            }
            Console.WriteLine($"Received {result} [{recvd}] bytes\n");
            return result.Replace(expect,"");
        }
        static void Main(string[] args)
        {
            byte[] data = new byte[100];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)0;
            }

            IPHostEntry iphostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAdress = IPAddress.Parse(ipAddresStr);

            IPEndPoint ipEndpoint = new IPEndPoint(ipAdress, port);

            Console.WriteLine("Hello, World!");
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client.Connect(ipEndpoint);
                while (!client.Connected)
                {
                    Thread.Sleep(500);
                }

                Console.WriteLine("Socket created to {0}", client.RemoteEndPoint?.ToString());

                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                if (client.Connected)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            }
        }


        public static class Servo
        {

        }

        public static class I2C
        {

        }

        public static class SPI
        {

        }


        public static class Encoder
        {

        }   

        public static class Stepper
        {

        }

        public static class OneWire
        {

        }
    }
}
