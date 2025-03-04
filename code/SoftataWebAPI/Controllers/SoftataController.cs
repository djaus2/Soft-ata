using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Softata;
using Softata.Enums;
using System;
using System.Net;
using System.Net.NetworkInformation;
//using static Softata.SoftataLib;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;
using static Softata.SoftataLib;
using static Softata.SoftataLib.Analog;
using System.Linq.Expressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{

    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            System.Diagnostics.Debug.WriteLine("======SETTING===========");
            foreach (var k in session.Keys)
            {
 
                System.Diagnostics.Debug.WriteLine(key);
            }
            session.SetString(key, JsonSerializer.Serialize(value));
            System.Diagnostics.Debug.WriteLine("========SET=============");
            foreach (var k in session.Keys)
            {
                System.Diagnostics.Debug.WriteLine(key);
            }
            System.Diagnostics.Debug.WriteLine("========END SET=============");
        }

        public static T? Get<T>(this ISession session, string key)
        {
            System.Diagnostics.Debug.WriteLine("========GET=========");
            foreach (var k in session.Keys)
            {
                System.Diagnostics.Debug.WriteLine(key);
            }
            System.Diagnostics.Debug.WriteLine("========GOT=========");
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
        
    }

    public class Selection
    {
        public int Index { get; set; }

        public int Order { get; set; }

        public string Item { get; set; }

        public Selection()
        {
            Index = 0;
            Item = "";
            Order = 0;
        }

        public Selection(int index)
        {
            Index = index;
            Item = "";
            Order = index;
        }

        public Selection(int index, string item)
        {
            Index = index;
            Item = item;
            Order = index;
        }

        public Selection(int index, string item, int order)
        {
            Index = index;
            Item = item;
            Order = order;
        }

        public Selection(int index, List<string> items)
        {
            Index = index;
            Item = items[index];
            Order = index;
        }

        public Selection(int index, List<string> items, int order)
        {
            Index = index;
            Item = items[index];
            Order = order;
        }
    }



    public static class Info
    {
        public static Softata.SoftataLib SoftataLib { get; set; } = new Softata.SoftataLib();

        public static Dictionary<int, string> TargetDeviceTypes { get; set; }

        public static Dictionary<int, Dictionary<string, int>> GenericCmds { get; set; }

        public static Dictionary<int,Dictionary<int,string>> TargetDevices { get; set; }
    }

    /// <summary>
    /// The Base Controller
    /// </summary>
    [Route("/")]
    [ApiController]
    public class SoftataController : ControllerBase
    {
        const int port = 4242;
        const string ipaddressStr = "192.168.0.5";

        private SoftataLib GetSoftataLib()
        {
            return Info.SoftataLib;
            SoftataLib? SoftataLib =
                SessionExtensions.Get<SoftataLib>(HttpContext.Session, "SoftataLib");
            if (SoftataLib == null)
            {
                SoftataLib = new SoftataLib();
                SessionExtensions.Set(HttpContext.Session, "SoftataLib", SoftataLib);
            }
            return SoftataLib;
        }

        private void GetDeviceTypes()
        {
            Info.TargetDeviceTypes = new Dictionary<int, string>();
            string deviceTypesCSV = Info.SoftataLib.SendMessageCmd("Devices");
            string[] parts = deviceTypesCSV.Split(":");
            List<string> devicesTypes = parts[1].Split(",").ToList();
            var deviceTypelist =
                    devicesTypes.Select((value, index) => new { value, index })
                    .ToDictionary(x => x.index, x => x.value);
            foreach (var item in deviceTypelist)
            {
                Info.TargetDeviceTypes.Add(item.Key, item.Value);
            }
        }

        private void GetDeviceTypesGenericCommands()
        {
            Info.GenericCmds = new Dictionary<int, Dictionary<string, int>>();
            foreach (var dt in Info.TargetDeviceTypes)
            {
                try
                {
                    byte subcmd = 0;//getGenericCommands
                    string cmdsCSV = Info.SoftataLib.SendTargetCommand((byte)(dt.Key), 0, subcmd);
                    if (cmdsCSV.Contains(":"))
                    {
                        string[] parts = cmdsCSV.Split(":");
                        List<string> cmdsList = parts[1].Split(",").ToList();
                        var cmdlist =
                                cmdsList.Select((value, index) => new { value, index })
                                .ToDictionary(x => x.value, x => x.index);
                        Info.GenericCmds.Add(dt.Key, cmdlist);
                        Thread.Sleep(500);
                    }
                    else
                        continue;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        private void GetDeviceTypesDevices()
        {
            Info.TargetDevices = new Dictionary<int, Dictionary<int, string>>();
            Thread.Sleep(200);
            foreach (var dt in Info.TargetDeviceTypes)
            {
                try
                {
                    byte subCmd = 1;//getDevices
                    string DevicesCSV = Info.SoftataLib.SendTargetCommand((byte)(dt.Key), 0, subCmd);
                    if (DevicesCSV.Contains(":"))
                    {
                        string[] parts2 = DevicesCSV.Split(":");
                        List<string> devices = parts2[1].Split(",").ToList();
                        var devicelist =
                                devices.Select((value, index) => new { value, index })
                                .ToDictionary(x => x.index, x => x.value);
                        Info.TargetDevices.Add(dt.Key, devicelist);
                    }
                    else
                        continue;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        [Route("AddDevice")]
        [HttpPost]
        private IActionResult AddDevice(int cmdTarget, int deviceIndex)
        {
            var device = Info.TargetDevices[cmdTarget][deviceIndex];
            var dictionary = Info.GenericCmds[cmdTarget];
            var values = dictionary
                .Where(kvp => kvp.Key.ToLower().Contains("setupdefault"))
                .Select(kvp => kvp.Value);
            byte subCmd = (byte)values.FirstOrDefault();
            string response = Info.SoftataLib.SendTargetCommand((byte)cmdTarget, 1, subCmd, (byte)deviceIndex);
            return Ok(response);
            //subCmd = GetGenericCmdIndex("setupdefault", GenericCommands);
            //result = softatalib.SendTargetCommand(cmdTarget, 1, subCmd, (byte)TargetDevice.Index);

        }



        /// <summary>
        /// Connect to the Pico W Server and send the Begin, Version and Devices commands
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="_port"></param>
        /// <returns>IActionResult(Ok or BadRequest)</returns>
        // POST api/<SoftataController>
        [Route("Start")]
        [HttpPost]
        public IActionResult Start(string ipAddress = "0.tcp.ngrok.io", int _port = port)
        {
            //SoftataLib SoftataLib = GetSoftataLib();
            bool result = _Connect(ipaddressStr, _port);
            if (result)
            {

                string beginValue = Info.SoftataLib.SendMessageCmd("Begin");
                if (beginValue == "Ready")
                {
                    string OKresult = $"Connected to {ipAddress}:{_port} and Ready";
                    string value = Info.SoftataLib.SendMessageCmd("Version");
                    OKresult += $"\nSoftata Version:{value}";
                    ///////////////////////////////////
                    string cmdsOffset = Info.SoftataLib.SendMessageCmd("Soffset");
                    if (int.TryParse(cmdsOffset, out int _offset))
                    {
                        Info.SoftataLib.Offset = _offset; //Should be 0xf0
                        Console.WriteLine($"CommandsOffset: {_offset}");
                    }

                    GetDeviceTypes();

                    GetDeviceTypesGenericCommands();

                    GetDeviceTypesDevices();


                    ///////////////////////////////
                    OKresult += $"\n{value}";
                    var connection = new Tuple<string, int>(ipAddress, _port);
                    HttpContext.Session.Set<Tuple<string, int>>("ConnectionDetails", connection);
                    return Ok(OKresult);
                }
                else
                {
                    Info.SoftataLib.Disconnect();
                    return BadRequest($"Connected to {ipAddress}:{_port} but Begin not ready. Disconnecting");
                }
            }
            else
            {
                return BadRequest($"Failed to connect to {ipAddress}:{_port}");
            }
        }

        // POST api/<SoftataController>
        /// <summary>
        /// Connect to the Pico W Server and send the Begin, Version and Devices commands
        /// Use Ngrok tunnel
        /// </summary>
        /// <param name="ngrokIndex"></param>
        /// <param name="_port"></param>
        /// <returns>IActionResult(Ok or BadRequest)</returns>
        [Route("NgrokStart")]
        [HttpPost]
        public IActionResult NgrokStart(int ngrokIndex = 0, int _port = port)
        {
            string ipAddress = $"{ngrokIndex}.tcp.ngrok.io";
            return Start(ipAddress, _port);
        }


        /// <summary>
        /// Connect to the Pico W Server using Session stored details and send the Begin, Version and Devices commands
        /// First start with Start() or NgrokStart() to cache the connection details in teh current Session
        /// </summary>
        /// <returns>IActionResult(Ok or BadRequest)</returns>
        // POST api/<SoftataController>
        [Route("StartSession")]
        [HttpPost]
        public IActionResult StartSession()
        {
            string ipAddress = "192.168.0.5";
            int port = 4242;
            if (!HttpContext.Session.Keys.Contains("ConnectionDetails"))
            {
                return BadRequest("No Connection Details"); ;
            }
            else
            {
                var connectDetails = HttpContext.Session.Get<Tuple<string, int>>("ConnectionDetails");
                if (connectDetails != null)
                {
                    string _ipAddress = connectDetails.Item1;
                    if (!string.IsNullOrEmpty(_ipAddress))
                    {
                        ipAddress = _ipAddress;
                        int? _port = connectDetails.Item2;
                        if (_port != null)
                        {
                            port = (int)_port;
                            return Start(ipAddress, port);
                        }
                    }
                }
            }
            return BadRequest("No or invalid Connection Details");
        }

        private bool _Connect(string ipAddress, int _port)
        {
            //SoftataLib SoftataLib = GetSoftataLib();
            bool result = Info.SoftataLib.Connect(ipAddress, _port);
            //SessionExtensions.Set(HttpContext.Session, "SoftataLib", SoftataLib);
            if (result)
            {
                return true;
            }
            else
                return false;
        }


        /// <summary>
        /// Connect to the Pico W Server (only). No Begin command is sent.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="_port"></param>
        /// <returns></returns>
        // POST api/<SoftataController>
        [Route("Connect")]
        [HttpPost]
        public IActionResult Connect(string ipAddress = "192.168.0.5", int _port = port)
        {
            //SoftataLib SoftataLib = GetSoftataLib();
            if (_Connect(ipAddress, _port))
            {
                return Ok($"Connected to {ipAddress}:{_port}");
            }
            else
            {
                return BadRequest("Server not available or invalid IPAdress");
            }
        }



        /// <summary>
        /// Connect to the Pico W Server (only). No Begin command is sent.
        /// Use Ngrok tunnel
        /// </summary>
        /// <param name="ngrokIndex"></param>
        /// <param name="_port"></param>
        /// <returns></returns>
        // POST api/<SoftataController>
        [Route("NgrokConnect")]
        [HttpPost]
        public IActionResult NgrokConnect(int ngrokIndex = 0, int _port = port)
        {
            string ipAddress = $"{ngrokIndex}.tcp.ngrok.io";
            return Connect(ipAddress, _port);
        }





        //////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Convert int to string
        /// </summary>
        /// <returns>value as string</returns>
        // GET: api/<SoftataController>
        [Route("GetStr")]
        [HttpGet]
        public string GetStr(int value)
        {
            return $"{value}";
        }



        List<string> Commands = new List<string> { "Begin", "End", "Devices", "Reset", "Version", "Null" };

        /// <summary>
        /// Send a simple command to the server
        /// </summary>
        /// <param name="cmd">Begin, End, Devices, Reset, Version, Null</param>
        /// <returns>Result or Acknowledgement</returns>
        [HttpGet("{cmd}")]
        public string Get(string cmd)
        {
            //SoftataLib SoftataLib = GetSoftataLib();
            // This is a "fix". The browser is looking for a favicon.ico file
            // There isn't one
            if (!Commands.Contains(cmd))
                return "";
            string value = Info.SoftataLib.SendMessageCmd(cmd);
            return value;
        }

        private static bool ValidateIPv4(string ipString)
        {
            // 15 is the max length of an IP address (xxx.xxx.xxx.xxx)
            if (!string.IsNullOrWhiteSpace(ipString) && ipString.Length <= 15)
            {      
                if (ipString.Count(c => c == '.') == 3)
                {
                    if (IPAddress.TryParse(ipString, out IPAddress? address))
                    {
                        if (address.ToString() == ipString)
                        {
                            Ping p = new Ping();
                            PingReply r = p.Send(ipString, 2000);
                            if (r != null)
                            {
                                if (r.Status == IPStatus.Success)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }



        /// <summary>
        /// (Optional) Set Shield Mode to Grove (Default: Restrict GPIO/PWM to pins 16-21 only etc)
        /// ... Or General (GPIO/PWM 0...26 etc)
        /// </summary>
        /// <param name="mode">groveShield,general</param>
        /// <returns>OK</returns>
        // POST api/<SoftataController>
        [Route("SetPicoShieldMode")]
        [HttpPost]
        public IActionResult SetPicoShieldMode(RPiPicoMode mode = RPiPicoMode.groveShield)
        {
            SoftataLib SoftataLib = GetSoftataLib();
            bool result = Info.SoftataLib.SetPicoShieldMode(mode);
            return Ok($"Set Pico Mode {mode}");
        }


        /// <summary>
        /// Send a "raw" message to the Pico W Server
        /// Other controllers use this method to send commands to the server
        /// </summary>
        /// <param name="msgOrDeviceType"></param>
        /// <param name="pin"></param>
        /// <param name="state"></param>
        /// <param name="expect"></param>
        /// <param name="other"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        // POST api/<SoftataController>
        [Route("SendMessage")]
        [HttpPost]
        public IActionResult SendMessage(int msgOrDeviceType, byte pin = 0xff,int state = 0xff  , string expect="OK:", int other = 0xff, byte[]? Data = null)
        {
            //SoftataLib SoftataLib = GetSoftataLib();
            string result = Info.SoftataLib.SendMessage((Commands)msgOrDeviceType, (byte)pin, (byte)state, expect, (byte)other ,Data);
            if(result != "Reset")
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}
