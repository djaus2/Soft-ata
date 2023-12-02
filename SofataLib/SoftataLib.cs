using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Data.Common;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Softata
{
    public class SoftataLib
    {
        //Ref: https://datasheets.raspberrypi.com/pico/Pico-R3-A4-Pinout.pdf
        private const int PinMax = 28;

        const byte nullData = 0xff;

        public static  int port { get; set; } = 4242;
        public static string ipAddresStr { get; set; } = "192.168.0.13";

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
                SendMessageCmd("Begin");
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


        public enum Commands
        {
            pinMode = 0xD0,
            digitalWrite = 0xD1,
            digitalRead = 0xD2,
            digitalToggle = 0xD3,
            analogWrite = 0xA0, //Not iumplemented

            Undefined = 0xFF
        }

        public enum CommandType : byte
        {
            Digital = 0,
            Analog = 1,
            PWM = 2,
            Servo = 3,
            I2C = 4,
            SPI = 5,
            OneWire = 6,
            Undefined = 0xFF,
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
        public static void SendMessageCmd(string cmd)
        {
            string sendmsg = cmd;
            Console.WriteLine("Sending: " + sendmsg);
            sendmsg += "\n";
            byte[] msg = Encoding.ASCII.GetBytes(sendmsg);
            int sent = client.Send(msg); ;

            Console.WriteLine($"Sent {sent - 1} bytes");

            while (client.Available == 0) ;
            byte[] data = new byte[100];
            int recvd = client.Receive(data);

            string result = Encoding.ASCII.GetString(data).Substring(0, recvd).Trim();
            Console.WriteLine($"Received {result} [{recvd}] bytes\n");

            switch (cmd)
            {
                case "Begin":
                    break;
                case "Null":
                    break;
                case "End":
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    break;
                case "Reset":
                    break;
            }
        }
        public static string SendMessage(Commands MsgType, byte pin = 0xff, byte state = 0xff, string expect = "OK", byte other = 0xff)
        {
            string result = "";
            // Construct command and parameters as list of bytes
            List<byte> sendmsg = new List<byte> { (byte)MsgType };
            if (pin != 0xff)
            {
                sendmsg.Add(pin);
                if (state != 0xff)
                {
                    sendmsg.Add(state);
                    if (other != 0xff)
                    {
                        sendmsg.Add(other);
                    }
                }
            }

            // Get bytes as string for display here.
            byte[] sendBytes = sendmsg.ToArray<byte>();
            string sendmsgStr = BitConverter.ToString(sendBytes).Replace(",", string.Empty);
            Console.WriteLine("Sending: " + sendmsgStr);

            // Add \n to end of message and send.
            // The command terminator at other end
            sendmsg.Add((byte)'\n');
            sendBytes = sendmsg.ToArray<byte>();
            int sent = client.Send(sendBytes); ;

            Console.WriteLine($"Sent {sent - 1} bytes");

            //Wait for response
            while (client.Available == 0) ;
            byte[] data = new byte[100];
            int recvd = client.Receive(data);

            result = Encoding.ASCII.GetString(data).Substring(0, recvd).Trim();

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
            return result;
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

        public static class Digital
        {

            public static void SetPinMode(int pinNumber, PinMode mode)
            {
                if (pinNumber <= 0 || pinNumber >= PinMax)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                SendMessage(Commands.pinMode, (byte)pinNumber, (byte)mode);
            }

            public static void SetPinState(int pinNumber, PinState state)
            {
                if (pinNumber <= 0 || pinNumber >= PinMax)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                SendMessage(Commands.digitalWrite, (byte)pinNumber, (byte)state);
            }

            public static void TogglePinState(int pinNumber)
                {
                    if (pinNumber <= 0 || pinNumber >= PinMax)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                    SendMessage(Commands.digitalToggle, (byte)pinNumber);
                }

            public static bool GetPinState(int pinNumber)
            {
                if (pinNumber <= 0 || pinNumber >= PinMax)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string state = SendMessage(Commands.digitalRead, (byte)pinNumber, nullData, "ON,OFF");

                if (state.ToUpper() == "ON")
                    return true;
                else
                    return false;
            }
        }

        public static class Analog
        {

        }

        public static class PWM
        {

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

        public static class Serial
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
