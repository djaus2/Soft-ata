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
using Softata.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Numerics;
using System.Reflection;
using ConsoleTextFormat;
using static Softata.SoftataLib;
using Softata.Enums;


namespace Softata
{
    public partial class SoftataLib
    {
        public SoftataLib()
        {

        }

        //Ref: https://datasheets.raspberrypi.com/pico/Pico-R3-A4-Pinout.pdf
        public const int PinMax = 28; //

        public const byte nullData = 0xfe;

        public int Offset { get; set; } = 0; //Should be 0xf0;

        public  int port { get; set; } = 4242;
        public  string ipAddresStr { get; set; } = "192.168.0.12";

        public List<string> GetDisplayMiscCmds(List<string> GenericCommands, Socket client,  Selection TargetDevice)
        {
            byte TargetDeviceTypeIndex = (byte)Softata.Enums.G_DEVICETYPES.Ddisplay;
            List<string> MiscCmds = new List<string>();
            byte subCmd = GetGenericCmdIndexfromList("miscGetList", GenericCommands);
            string _miscCmds = SendTargetCommand(TargetDeviceTypeIndex, client,1, subCmd, (byte)TargetDevice.Index);
            if (!string.IsNullOrEmpty(_miscCmds))
            {
                MiscCmds = _miscCmds.Split(":")[^1].Split(",").ToList();
            }
            return MiscCmds;
        }

        public static byte GetGenericCmdIndex(string cmd, string[] GenericCmds)
        {
            byte subCmd = 0;
            for (int i = 0; i < GenericCmds.Length; i++)
            {
                if (GenericCmds[i].ToLower().Contains(cmd.ToLower()))
                {
                    subCmd = (byte)i;
                    break;
                }
            }
            return subCmd;
        }

        public List<string> GetSensorPropertiess(List<string> GenericCommands, Socket client,Selection TargetDevice)
        {
            byte TargetDeviceTypeIndex = (byte)Softata.Enums.G_DEVICETYPES.Dsensor;
            List<string> SensorProps = new List<string>();
            byte subCmd = GetGenericCmdIndexfromList("getProperties", GenericCommands);
            string _sensorproperties = SendTargetCommand(TargetDeviceTypeIndex, client, 1, subCmd, (byte)TargetDevice.Index);
            if (!string.IsNullOrEmpty(_sensorproperties))
            {
                SensorProps = _sensorproperties.Split(":")[^1].Split(",").ToList();
            }
            return SensorProps;
        }

        private  Socket? _client;

        private  Socket? client
        {
            get
            {
                if (_client == null)
                {
                };
                return _client;
            }
            set => _client = value;
        }
        public  Socket? Client { get => client; set => client = value; }// { get => Client1; set => Client1 = value; }
        /*public  Socket? Client1 { get => client; set => client = value; }*/

        public  bool Connected
        {
            get
            {
                if (Client == null)
                    return false;
                return Client.Connected;
            }
        }

        private  bool Inited = false;

        public enum RPiPicoMode { groveShield, generalMode, Undefined = 255 };
        internal  RPiPicoMode _RPiPicoMode = RPiPicoMode.groveShield; //Default

        


        public  bool SetPicoShieldMode(RPiPicoMode mode)
        {
            _RPiPicoMode = mode;
            return true;
        }

        public  void Disconnect()
        {

            try
            {
                if (Client != null)
                {
                    if (Client.Connected)
                    {
                        Client.Close();
                    }
                    Client.Dispose();
                }
            }
            catch (Exception ex) { }
        }

        public  void Reconnect()
        {
            try
            {
                if (Client != null)
                {
                    if (Client.Connected)
                    {
                        Client.Close();
                    }
                    Client.Dispose();
                }
            }
            catch (Exception ex) { }
            Connect(ipAddresStr, port);
        }

        public  Socket? Connect(String server, Int32 port)
        {
            try
            {
                // Create a new TcpClient.
                TcpClient client1 = new TcpClient(server, port);

                //// Translate the passed message into ASCII and store it as a byte array.
                //Byte[] data = System.Text.Encoding.ASCII.GetBytes("Hello World!");

                // Get a client stream for reading and writing.
                NetworkStream stream1 = client1.GetStream();

                client = client1.Client;


                //// Send the message to the connected TcpServer.
                //stream.Write(data, 0, data.Length);

                //Console.WriteLine("Sent: Hello World!");

                //// Receive the TcpServer.response.
                //// Buffer to store the response bytes.
                //data = new Byte[256];

                //// String to store the response ASCII representation.
                //String responseData = String.Empty;

                //// Read the first batch of the TcpServer response bytes.
                //Int32 bytes = stream.Read(data, 0, data.Length);
                //responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                //Console.WriteLine("Received: {0}", responseData);

                //// Close everything.
                //stream.Close();
                //client.Close();
                return client;
            }
            catch (ArgumentNullException e)
            {
                return null;
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
                return null;
            }
        }
        public  bool ConnectOld(string _ipAddresStr, int _port)
        {
            Console.WriteLine("Connecting to Softata Server from .NET");
            ipAddresStr = _ipAddresStr;
            port = _port;
            //IPHostEntry iphostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAdress = IPAddress.Parse(ipAddresStr);

            IPEndPoint ipEndpoint = new IPEndPoint(ipAdress, port);

            Console.WriteLine("Connecting to Softata Server.");
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Client.Connect(ipEndpoint);
                while (!Client.Connected)
                {
                    Thread.Sleep(500);
                }

                Console.WriteLine("Socket created to {0}", Client.RemoteEndPoint?.ToString());
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (Client.Connected)
                {
                    Client.Shutdown(SocketShutdown.Both);
                    Client.Close();
                }
                Client = null;
            }
            if (Client == null)
                return false;
            else
                return true; ;
        }



        // Add commands here that use param = 0xff
        // For others 0xff is not sent
        private  List<Commands> WritingCmds = new List<Commands> { Commands.pwmWrite, Commands.serialWriteChar };




        // For simplicity add items to lists below using their ordinal:
        public  Dictionary<DeviceCategory, List<byte>> DeviceCategoryMembers = new Dictionary<DeviceCategory, List<byte>>()
        {
            { DeviceCategory.digital,new List<byte>(){0}},
            { DeviceCategory.analog,new List<byte>(){1,7,9,0xA }},
            { DeviceCategory.sensor,new List<byte>(){4} },
            { DeviceCategory.actuator,new List<byte>(){9,0xA}},
            { DeviceCategory.display,new List<byte>(){5}},
            { DeviceCategory.serial,new List<byte>(){6,0xB}}
        };

        //////////////////////////////////////////
        // NOTE enum order of DisplayDevice must match that returned by GroveDisplayCmds.getDisplays()
        //public enum DisplayDevice { OLEDSO096, LCD1602Display, NeopixelDisplay }
        //////////////////////////////////////////

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
            SteperControl = 8,
            Encoder = 9,
            Serial = 10,
            InputPullup = 11
        }

        public enum GroveGPIOPinX { p16 = 16, p17 = 17, p18 = 18, p19 = 19, p20 = 20, p21 = 21 }

        public enum GroveGPIOPin { p16, p17, p18, p19, p20, p21 }
        public  List<string> GroveGPIOPinList = Enum.GetNames(typeof(GroveGPIOPin)).ToList();
        public enum GroveAnalogPin { A0, A1, A2 } //,AVsys=29, ATemp=0xff }
        public enum GroveAnalogPinX { A0 = 26, A1 = 27, A2 = 28 } //,AVsys=29, ATemp=0xff }

        public  List<string> GroveAnalogPinList = Enum.GetNames(typeof(GroveAnalogPin)).ToList();

        public enum GrovePWMPin { p16, p17, p18, p19, p20, p21 }
        public enum GrovePWMPinX { p16 = 16, p17 = 17, p18 = 18, p19 = 19, p20 = 20, p21 = 21 }

        public  List<string> GrovePWMPinList = Enum.GetNames(typeof(GrovePWMPin)).ToList();


        public enum DigitalPinMode { Input = PinMode.DigitalInput, Output = PinMode.DigitalOutput }
        public enum AnalogMode { ADC = PinMode.AnalogInput, PWM = PinMode.PwmOutput }

        public enum PinState
        {
            Low = 0x00,
            High = 0x01
        }
        public  string SendMessageCmd(string cmd, Socket? _client = null)
        {
            //Passed from Session with Bolckly
            if(_client != null)
                Client = _client;

            if (Client == null)

                throw new Exception("SendMessageCmd: Client is null");

            else if (!Client.Connected)
                throw new Exception("SendMessageCmd: Not connected");

            string result = "";


            // Construct command and parameters as list of bytes
            List<byte> sendmsg = new List<byte> { 1, (byte)cmd[0] };

            byte[] sendBytes = sendmsg.ToArray<byte>();
            Console.WriteLine($"Sending {sendBytes.Length} data bytes");
            int sent = Client.Send(sendBytes, 0, sendBytes.Length, SocketFlags.None);
            if (sent != sendBytes.Length)
                throw new Exception($"SendMessage: Sent {sent} bytes, expected {sendBytes.Length} bytes");
            Console.WriteLine($"Sent {sent} bytes");

            //Wait for response
            while (Client.Available == 0) ;
            byte[] data = new byte[100];
            int recvd = Client.Receive(data);

            result = Encoding.ASCII.GetString(data).Substring(0, recvd).Trim();
            Console.WriteLine($"Received {result} [{recvd}] bytes\n");

            switch (cmd)
            {
                case "Ack":
                    Client.Disconnect(true);
                    Client.Close();
                     return result;
                case "Begin":
                    return result;
                //break;
                case "Version":
                    return result;
                //break
                case "Devices":
                    return result;
                case "Soffset":
                    return result;
                //break
                case "Null":
                    break;
                case "End":
                    Thread.Sleep(2000);
                    Client.Shutdown(SocketShutdown.Both);
                    Client.Close();
                    Client.Dispose();
                    Client = null;
                    break;
                case "WDTTimeOut":
                    Thread.Sleep(20000);
                    Client.Shutdown(SocketShutdown.Both);
                    Client.Close();
                    Client.Dispose();
                    Client = null;
                    break;
                case "Reset":
                    break;
            }
            return "";
        }

        
        // State
        private object Aval2 { get; set; } = "";
        private Guid key { get; set; } = Guid.NewGuid();

        public  Dictionary<Guid, SoftataLib> SoftataLibs = new Dictionary<Guid, SoftataLib>();
        public  string NewSoftataLib()
        {
            SoftataLib av = new SoftataLib();
            av.key = Guid.NewGuid();
            SoftataLibs.Add(av.key, av);
            return av.key.ToString();
        }

        public  SoftataLib? GetSoftataLib(string key)
        {
            if (Guid.TryParse(key, out Guid guid))
            {
                if(SoftataLibs.ContainsKey(guid))
                    return SoftataLibs[guid];
            }
            return null;
        }

        public void SetAvalue(object aval)
        {
            SoftataLibs[key].Aval2 = aval;
            return;
        }

        public object GetAvalue()
        {
            return SoftataLibs[key].Aval2;
        }

        public  void ClearAll()
        {
            SoftataLibs = new Dictionary<Guid, SoftataLib>();
        }

        public  bool Delete(string key)
        {
            if (Guid.TryParse(key, out Guid guid))
            {
                if (SoftataLibs.ContainsKey(guid))
                {
                    SoftataLibs.Remove(guid);
                    return true;
                }
            }
            return false;
        }
        

        public string SendMessage(Commands MsgType, Socket? client = null, byte pin = 0xff, byte state = 0xff, string expect = "OK", byte other = 0xff, byte[]? Data = null, bool debug = true)
        {
            if (client != null)
                Client = _client;
            if (Client == null)
                throw new Exception("SendMessageCmd: Client is null");
            else if (!Client.Connected)
                throw new Exception("SendMessageCmd: Not connected");


            // Construct command and parameters as list of bytes
            List<byte> sendmsg = new List<byte> { 0, (byte)MsgType };
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
            if (Data != null)
            {
                foreach (int d in Data)
                {
                    sendmsg.Add((byte)d);
                }
            }

            sendmsg[0] = (byte)(sendmsg.Count - 1);
            // Get bytes from list.
            byte[] sendBytes = sendmsg.ToArray<byte>();
            /* If msg is Display setup nondefault:
             * =======================================
             * msg should now be:
             * number of bytes -1 eg 6 (if total num bytes is7)
             * 241, 0xf1 = Displays                 ... maps to cmd in Arduino
             * pin  (eg 16)                         ... maps to pin in Arduino
             * subcommand = 3  for nondefault setup ... maps toparam in Arduino
             * enum index of display (eg 2 for Neopixel) ... maps to other in Arduino
             *     Nb: Once instatiated, other is its index in the display list as returned from setup() 
             * Length of otherData                  ... maps to otherData in Arduino
             * other data                           ...
             */
            if (debug)
                Console.WriteLine($"Sending {sendBytes.Length} data bytes");

            int sent = Client.Send(sendBytes, 0, sendBytes.Length, SocketFlags.None);
            if (sent != sendBytes.Length)
                throw new Exception($"SendMessage: Sent {sent} bytes, expected {sendBytes.Length} bytes");

            if (debug)
                Console.WriteLine($"Sent {sent} bytes");

            //Wait for response
            while (Client.Available == 0) ;
            byte[] data = new byte[256];
            int recvd = Client.Receive(data);

            string result = Encoding.UTF8.GetString(data).Substring(0, recvd);

            // Make the check work in both ways.
            if ((!expect.Contains(result)) && (!result.Contains(expect)))
            {
                Console.WriteLine($"Expected {expect} got {result}... rebooting");
                SendMessageCmd("Reset");
                recvd = Client.Receive(data);
                result = Encoding.ASCII.GetString(data).Substring(0, recvd).Trim();
                Console.WriteLine($"Received {result} [{recvd}] bytes\n");
                return "Reset";
            }
            if (debug)
                Console.WriteLine($"Received {result} [{recvd}] bytes\n");
            return result.Replace(expect, "");
        }


        /// <summary>
        /// Same as SendMessage(...........) but target type is byte rather than enum
        /// </summary>
        /// <param name="cmdTargetType"></param>
        /// <param name="pin"></param>
        /// <param name="state"></param>
        /// <param name="expect"></param>
        /// <param name="other"></param>
        /// <param name="Data"></param>
        /// <param name="debug"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string SendMessageB(byte cmdTargetType,Socket? client, byte pin = 0xff, byte state = 0xff, string expect = "OK:", byte other=0xff, byte[]? Data=null, bool debug=true )
        {
            if (client != null)
                Client = client;
            if (Client == null)
                throw new Exception("SendMessageCmd: Client is null");
            else if (!Client.Connected)
                throw new Exception("SendMessageCmd: Not connected");

            cmdTargetType = (byte)(cmdTargetType + Offset);
            // Clear any previous received data
            while (Client.Available != 0)
            {
                byte[] dummayData = new byte[256];
                int dummyRecvd = Client.Receive(dummayData);
            }

            // Construct command and parameters as list of bytes
            List<byte> sendmsg = new List<byte> { 0,(byte)cmdTargetType };
            if (pin != 0xff)
            {
                sendmsg.Add(pin);
                if ((state != 0xff))// || (WritingCmds.Contains(MsgType)))
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
            /* If msg is Display setup nondefault:
             * =======================================
             * msg should now be:
             * number of bytes -1 eg 6 (if total num bytes is7)
             * 241, 0xf1 = Displays                 ... maps to cmd in Arduino
             * pin  (eg 16)                         ... maps to pin in Arduino
             * subcommand = 3  for nondefault setup ... maps toparam in Arduino
             * enum index of display (eg 2 for Neopixel) ... maps to other in Arduino
             *     Nb: Once instatiated, other is its index in the display list as returned from setup() 
             * Length of otherData                  ... maps to otherData in Arduino
             * other data                           ...
             */
            if(debug)
                Console.WriteLine($"Sending {sendBytes.Length} data bytes");

            int sent = Client.Send(sendBytes, 0, sendBytes.Length, SocketFlags.None);
            if (sent != sendBytes.Length)
                throw new Exception($"SendMessage: Sent {sent} bytes, expected {sendBytes.Length} bytes");

            if(debug)
                Console.WriteLine($"Sent {sent} bytes");

            //Wait for response
            while (Client.Available == 0) ;
            byte[] data = new byte[256];
            int recvd = Client.Receive(data);

            string result = Encoding.UTF8.GetString(data).Substring(0, recvd);
            System.Diagnostics.Debug.WriteLine($"3. Result: {result}");

            // Make the check work in both ways.
            if ((!expect.Contains(result)) && (!result.Contains(expect)))
            {
                Console.WriteLine($"Expected {expect} got {result}... rebooting");
                SendMessageCmd("Reset");
                recvd = Client.Receive(data);
                result = Encoding.ASCII.GetString(data).Substring(0, recvd).Trim();
                Console.WriteLine($"Received {result} [{recvd}] bytes\n");
                return "Reset";
            }
            if(debug)
                 Console.WriteLine($"Received {result} [{recvd}] bytes\n");
            return result.Replace(expect,"");
        }

        //softatalib.SendTargetCommand((byte)TargetDeviceType.Index,0,(byte)TargetDevice.Index, (byte)TargetCommand.Index , linkedListNo, data);
        public string SendTargetCommand(byte cmdDeviceType, Socket Client,byte pin, byte cmd, byte cmdTargetType = 0xff, byte linkedListIndex=0, byte[]? dataIn = null)
        {
            System.Diagnostics.Debug.WriteLine($"2. SendTargetCommand(): cmdDeviceType:{cmdDeviceType}, pin:{pin}, cmd:{cmd}, cmdTargetType:{cmdTargetType}, linkedListIndex:{linkedListIndex}");
            //other can be linkedListIndex or cmdTargetType
            // If actual instance use linkedListIndex
            // For others that don't need instance use the display(etc) type.
            byte other = linkedListIndex;
            if (cmdTargetType != 0xff)
            {
                other = cmdTargetType;
            }
            byte[]? data = null;
            if (dataIn != null)
            {
                byte[] bytes2 = dataIn.Append((byte)0).ToArray<byte>(); //Need to append 0
                data = bytes2.Prepend((byte)bytes2.Length).ToArray<byte>(); //Prepend string length +1

            }
            // public string SendMessageB(byte cmdTargetType, byte pin = 0xff, byte state = 0xff, string expect = "OK:", byte other=0xff, byte[]? Data=null, bool debug=true )

            string response = SendMessageB     ( cmdDeviceType,Client, pin,(byte)cmd, "OK:", other, data);
            System.Diagnostics.Debug.WriteLine($"2. Response: {response}");
            return response;
        }

        /// <summary>
        /// Get dictionary list of similar commands so can be used in a menu
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="GenericCmds"></param>
        /// <returns></returns>
        private static Dictionary<int, string> GetListGenericCmds(string cmd, string[] GenericCmds)
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

        public static byte GetGenericCmdIndexfromList(string cmd, List<string> GenericCmds)
        {
            byte subCmd = 0;
            for (int i = 0; i < GenericCmds.Count(); i++)
            {
                if (GenericCmds[i].ToLower().Contains(cmd.ToLower()))
                {
                    subCmd = (byte)i;
                    break;
                }
            }
            return subCmd;
        }

    }
}
