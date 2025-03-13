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
using SoftataWebAPI.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{



    /// <summary>
    /// The Base Controller
    /// </summary>
    [Route("/Sensor")]
    [ApiController]
    public class SoftataDevicesController : ControllerBase
    {
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
            string response = Info.SoftataLib.SendTargetCommand((byte)cmdTarget, 1, subCmd, (byte)deviceIndex);
            return Ok(response);
            //subCmd = GetGenericCmdIndex("setupdefault", GenericCommands);
            //result = softatalib.SendTargetCommand(cmdTarget, 1, subCmd, (byte)TargetDevice.Index);

        }
    }
}
