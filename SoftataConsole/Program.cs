using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.Data.Common;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace FirmataBasic
{
    internal class Program
    {
        const int port = 4242;//27016;
        const string ipaddressStr = "192.168.0.13";

        private static Socket client;

        public enum Commands 
        {
            pinMode=0xD0,
            digitalWrite= 0xD1,
            digitalRead= 0xD2,
            digitalToggle=0xD3,
            analogWrite=0xA0,

            Undefined = 0xFF
        }

        public enum CommandType :byte
        {
            Digital = 0,
            Analog = 1,
            PWM = 2,
            Servo = 3,
            I2C=4,
            SPI=5,
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

        public enum  PinState
        {
            LOW = 0x00,
            HIGH = 0x01
        }
        private enum MessageHeader
        {
            AnalogState = 0xE0, // 224
            DigitalState = 0x90, // 144
            SystemExtension = 0xF0,
            ProtocolVersion = 0xF9
        }

        private enum StringReadMode
        {
            ReadLine,
            ReadToTerminator,
            ReadBlock
        }

        private class StringRequest
        {
            private readonly StringReadMode _mode;
            private readonly int _blockLength;
            private readonly char _terminator;

            public static StringRequest CreateReadLineRequest()
            {
                return new StringRequest(StringReadMode.ReadLine, '\\', 0);
            }

            public static StringRequest CreateReadRequest(int blockLength)
            {
                return new StringRequest(StringReadMode.ReadBlock, '\\', blockLength);
            }

            public static StringRequest CreateReadRequest(char terminator)
            {
                return new StringRequest(StringReadMode.ReadToTerminator, terminator, 0);
            }

            private StringRequest(StringReadMode mode, char terminator, int blockLength)
            {
                _mode = mode;
                _blockLength = blockLength;
                _terminator = terminator;
            }

            public char Terminator => _terminator;
            public int BlockLength => _blockLength;
            public StringReadMode Mode => _mode;
        }

        private const byte Pin_Mode = 0xF4;
        private const byte AnalogMessage = 0xE0;
        private const byte DigitalMessage = 0x90;
        private const byte SysExStart = 0xF0;
        private const byte SysExEnd = 0xF7;

        private const int Buffersize = 2048;
        private const int MaxQueuelength = 100;

        //private readonly ISerialConnection client;
        //private readonly bool _gotOpenConnection;
        //private readonly LinkedList<FirmataMessage> _receivedMessageList = new LinkedList<FirmataMessage>();
        //private readonly Queue<string> _receivedStringQueue = new Queue<string>();
        //private ConcurrentQueue<FirmataMessage> _awaitedMessagesQueue = new ConcurrentQueue<FirmataMessage>();
        //private ConcurrentQueue<StringRequest> _awaitedStringsQueue = new ConcurrentQueue<StringRequest>();
        //private StringRequest _currentStringRequest;

        //private int _messageTimeout = -1;
        //private ProcessMessageHandler _processMessage;
        private int _messageBufferIndex, _stringBufferIndex;
        private readonly byte[] _messageBuffer = new byte[Buffersize];
        private readonly char[] _stringBuffer = new char[Buffersize];

        static void SendMessagCmd(string cmd)
        {
            string sendmsg = cmd;
            Console.WriteLine("Sending: " + sendmsg);
            sendmsg += "\n";
            byte[] msg = Encoding.ASCII.GetBytes(sendmsg);
            int sent = client.Send(msg); ;

            Console.WriteLine($"Sent {sent-1} bytes");

            while (client.Available == 0) ;
            byte[] data = new byte[100];
            int recvd = client.Receive(data);

            string res = Encoding.ASCII.GetString(data);
            Console.WriteLine($"Received {res} [{recvd}] bytes\n");

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
            }

        }
        static string SendMessage(Commands MsgType, byte pin=0xff, byte state=0xff, byte other=0xff)
        {
            string result = "";
            // Construct command and parameters as list of bytes
            List<byte> sendmsg = new List<byte> {(byte)MsgType};
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
            Console.WriteLine("Sending: " +sendmsgStr);

            // Add \n to end of message and send.
            // The command terminator at other end
            sendmsg.Add((byte)'\n');
            sendBytes = sendmsg.ToArray<byte>();
            int sent = client.Send(sendBytes);;
            
            Console.WriteLine($"Sent {sent-1} bytes");

            //Wait for response
            while (client.Available == 0);
            byte[] data = new byte[100];
            int recvd = client.Receive(data);

            result = Encoding.ASCII.GetString(data);
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
            IPAddress ipAdress = IPAddress.Parse(ipaddressStr); 
            
            IPEndPoint ipEndpoint = new IPEndPoint(ipAdress, port);

            Console.WriteLine("Hello, World!");
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client.Connect(ipEndpoint);
                while(!client.Connected)
                {
                    Thread.Sleep(500);
                }

                Console.WriteLine("Socket created to {0}", client.RemoteEndPoint?.ToString());
                byte[] sendmsg = new byte[] { Pin_Mode, (byte)32, (byte)PinMode.DigitalOutput};

                SendMessagCmd("Begin");
                Thread.Sleep(500);

                Digital.SetPinMode(13, PinMode.DigitalInput);
                Digital.SetPinMode(12, PinMode.DigitalOutput);
                Digital.SetPinState(12, PinState.HIGH);

                //Digital.SetPinMode(50, PinMode.DigitalInput);

                SendMessage(Commands.analogWrite, (byte)26, (byte)PinMode.DigitalInput);
                Thread.Sleep(500);

                int n;
                for (int i = 0; i < 12; i++)
                {;
                    Digital.GetPinState(13);
                    Digital.TogglePinState(12);
                    //SendMessage(0x6D, (byte)13); ;
                    //SendMessage(0x6E, (byte)13, (byte)PinState.HIGH);
                    //Thread.Sleep(1000);
                    //SendMessage(0x6E, (byte)13, (byte)PinState.LOW);
                    Thread.Sleep(1000);
                }

                SendMessagCmd("End");
                Thread.Sleep(500);
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                if(client.Connected)
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            }
        }



        public void SetDigitalPort(int portNumber, int pins)
        {
            if (portNumber < 0 || portNumber > 15)
                throw new ArgumentOutOfRangeException(nameof(portNumber), "Messages.ArgumentEx_PortRange0_15");

            if (pins < 0 || pins > 0xFF)
                throw new ArgumentOutOfRangeException(nameof(pins), "Messages.ArgumentEx_ValueRange0_255");

            client.Send(new[] { (byte)(DigitalMessage | portNumber), (byte)(pins & 0x7F), (byte)((pins >> 7) & 0x03), (byte)0 }); //, 0, 3);
        }

        /// <inheritdoc cref="IFirmataProtocol.SetDigitalReportMode"/>
        public void SetDigitalReportMode(int portNumber, bool enable)
        {
            if (portNumber < 0 || portNumber > 15)
                throw new ArgumentOutOfRangeException(nameof(portNumber), "Messages.ArgumentEx_PortRange0_15");

            client.Send(new[] { (byte)(0xD0 | portNumber), (byte)(enable ? 1 : 0), (byte)0 }); //, 0, 2);
        }

        public static class Digital
        {
            public static void SetPinMode(int pinNumber, PinMode mode)
            {
                if (pinNumber < 0 || pinNumber > 127)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                SendMessage(Commands.pinMode, (byte)pinNumber, (byte)mode);
            }

            public static void SetPinState(int pinNumber, PinState state)
            {
                if (pinNumber < 0 || pinNumber > 127)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                SendMessage(Commands.digitalWrite, (byte)pinNumber, (byte)state);
            }

            public static void TogglePinState(int pinNumber)
            {
                if (pinNumber < 0 || pinNumber > 127)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                SendMessage(Commands.digitalToggle, (byte)pinNumber);
            }

            public static bool GetPinState(int pinNumber)
            {
                if (pinNumber < 0 || pinNumber > 127)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string state = SendMessage(Commands.digitalRead, (byte)pinNumber);

                if (state.ToUpper() == "ON")
                    return true;
                else
                    return false;
            }


        }


        public static class Analog
        {
            
        }
    }
}
