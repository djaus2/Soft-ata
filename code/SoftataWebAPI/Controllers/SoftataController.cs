using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Softata;
using Softata.Enums;
using System;
using System.Net;
using System.Net.NetworkInformation;
using static Softata.SoftataLib;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{

    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }

    /// <summary>
    /// The Base Controller
    /// </summary>
    [Route("/")]
    [ApiController]
    public class SoftataController : ControllerBase
    {
        const int port = 4242;
        const string ipaddressStr = "192.168.0.12";

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
            // This is a "fix". The browser is looking for a favicon.ico file
            // There isn't one
            if (!Commands.Contains(cmd))
                return "";
            string value = SoftataLib.SendMessageCmd(cmd);
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

        private bool _Connect(string ipAddress, int _port)
        {
            //if (ValidateIPv4(ipAddress))
            //{
                bool result = SoftataLib.Connect(ipAddress, _port);
                if (result)
                {
                    return true;
                }
            //}
            //else
            //{
            //    bool result = SoftataLib.ConnectAlt(ipAddress, _port);
            //    if (result)
            //    {
            //        return true;
            //    }
            //}
            else
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
            bool result = SoftataLib.SetPicoShieldMode(mode);
            return Ok($"Set Pico Mode {mode}");
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
            bool result = _Connect(ipAddress, _port);
            if (result)
            {
                string beginValue = SoftataLib.SendMessageCmd("Begin");
                if (beginValue == "Ready")
                {
                    string OKresult = $"Connected to {ipAddress}:{_port} and Ready";
                    string value = SoftataLib.SendMessageCmd("Version");
                    OKresult += $"\nSoftata Version:{value}";
                    value = SoftataLib.SendMessageCmd("Devices");
                    OKresult += $"\n{value}";
                    var connection = new Tuple<string, int> ( ipAddress, _port );
                    HttpContext.Session.Set< Tuple<string, int>>("ConnectionDetails", connection);
                    return Ok(OKresult);
                }
                else
                {
                    SoftataLib.Disconnect();
                    return BadRequest($"Connected to {ipAddress}:{_port} but Begin not ready. Disconnecting");
                }
            }
            else
            {
                return BadRequest($"Failed to connect to {ipAddress}:{_port}");
            }
        }


        /// <summary>
        /// Connect to the Pico W Server and send the Begin, Version and Devices commands
        /// </summary>
        /// <returns>IActionResult(Ok or BadRequest)</returns>
        // POST api/<SoftataController>
        [Route("StartSession")]
        [HttpPost]
        public IActionResult StartSession()
        {
            string ipAddress = "192.168.0.12";
            int port = 4242;
            if (!HttpContext.Session.Keys.Contains("ConnectionDetails"))
            {
                return BadRequest("No Connection Details");;
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

        /// <summary>
        /// Connect to the Pico W Server (only). No Begin command is sent.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="_port"></param>
        /// <returns></returns>
        // POST api/<SoftataController>
        [Route("Connect")]
        [HttpPost]
        public IActionResult Connect(string ipAddress = "192.168.0.12", int _port = port)
        {
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
        public IActionResult NgrokStart(int ngrokIndex = 0 , int _port = port)
        {
            string ipAddress = $"{ngrokIndex}.tcp.ngrok.io";
            return Start(ipAddress, _port);
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
            string result = SoftataLib.SendMessage((Commands)msgOrDeviceType, (byte)pin, (byte)state, expect, (byte)other ,Data);
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
