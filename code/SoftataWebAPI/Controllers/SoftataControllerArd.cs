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
using SoftataWebAPI.Data.Db;
using SoftataWebAPI.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{

    /// <summary>
    /// The Alt Base Controller
    /// Loads Info data from Softata Sketch
    /// </summary>
    [Route("/Ard")]
    [ApiController]
    public class SoftataArdController(SoftataContext context, ISoftataGenCmds sharedService) : ControllerBase
    {
        const int port = 4242;
        const string ipaddressStr = "192.168.0.5";


        private Softata.SoftataLib GetSoftataLib()
        {
            return Info.SoftataLib;
            /*
            SoftataLib? SoftataLib =
                SessionExtensions.Get<SoftataLib>(HttpContext.Session, "SoftataLib");
            if (SoftataLib == null)
            {
                SoftataLib = new SoftataLib();
                SessionExtensions.Set(HttpContext.Session, "SoftataLib", SoftataLib);
            }
            return SoftataLib;*/
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
        /// Get app data, device types,devices,commands from Pico W Service
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
                    
                    sharedService.SoftataGetDatafrmPico();

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
        public IActionResult StartSessionArd()
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

        /// <summary>
        /// Validate an IPAddress
        /// </summary>
        /// <param name="ipString">The IpAddress string</param>
        /// <returns></returns>
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
            Softata.SoftataLib SoftataLib = GetSoftataLib();
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
