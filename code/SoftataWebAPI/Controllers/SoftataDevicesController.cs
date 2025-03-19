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
//using static Softata.SoftataLib.Analog;
using System.Linq.Expressions;
using SoftataWebAPI.Data;
using System.Net.Sockets;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{



    /// <summary>
    /// The Base Controller
    /// </summary>
    [Route("/Sensor")]
    [ApiController]
    public class SoftataDevicesController(ISoftataGenCmds sharedService) : ControllerBase
    {
        const int port = 4242;

        #region
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private Softata.SoftataLib softatalib
        {
            get
            {
                return sharedService.GetSoftata(HttpContext);
            }
        }
        private Socket? _Connect(string ipAddress, int _port)
        {

            //softatalib softatalib = Getsoftatalib();
            Socket? client = softatalib.Connect(ipAddress, _port);
            if (client != null)
            {
                if (client.Connected)
                {
                    var connection = new Tuple<string, int>(ipAddress, _port);
                    HttpContext.Session.Set<Tuple<string, int>>("ConnectionDetails", connection);
                    return client;
                }
            }
            return null;
        }

        private Socket? _client = null;
        private Socket? Client
        {
            get
            {
                if (_client != null)
                {
                    return _client;
                }
                else
                {
                    Tuple<string, int>? connectionDetails = HttpContext.Session.Get<Tuple<string, int>>("ConnectionDetails");
                    if (connectionDetails == null)
                        return null;


                    _client = _Connect(connectionDetails.Item1, connectionDetails.Item2);
                    return _client;
                }

            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion


        [Route("/AddDevice")]
        [HttpPost]
        private IActionResult AddDevice(int cmdTarget, int deviceIndex)
        {
            var device = Info.TargetDevices[cmdTarget][deviceIndex];
            var dictionary = Info.GenericCmds[cmdTarget];
            var values = dictionary
                .Where(kvp => kvp.Key.ToLower().Contains("setupdefault"))
                .Select(kvp => kvp.Value);
            byte subCmd = (byte)values.FirstOrDefault();
            string response = softatalib.SendTargetCommand((byte)cmdTarget, Client,1, subCmd, (byte)deviceIndex);
            return Ok(response);
            //subCmd = GetGenericCmdIndex("setupdefault", GenericCommands);
            //result = softatalib.SendTargetCommand(cmdTarget, 1, subCmd, (byte)TargetDevice.Index);

        }
    }
}
