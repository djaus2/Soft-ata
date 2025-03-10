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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{

    /// <summary>
    /// The Base Controller
    /// </summary>
    [Route("/Database")]
    [ApiController]
    public class SoftataControllerDatabase(SoftataContext context) : ControllerBase
    {
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

        [Route("SoftataGetData")]
        [HttpGet]
        public IActionResult SoftataGetData()
        {
            GetDeviceTypes();
            GetDeviceTypesGenericCommands();
            GetDeviceTypesDevices();
            return Ok("GotSoftataData");
        }

        [Route("SoftataSaveData")]
        [HttpGet]
        public IActionResult SoftataCreateDb(int pin)
        {
            if (pin == 1370  )
            {
                var ctx = context;
                SoftataWebAPI.Data.Db.Database.CreateDb(ctx);
                return Ok("GotSoftataData");
            }
            return BadRequest("Failed");
        }


        [Route("SoftataReadData")]
        [HttpGet]
        public IActionResult ReadSoftataData()
        {
            var ctx = context;
            SoftataWebAPI.Data.Db.Database.ReadDb(ctx);
            return Ok("ReadSoftataData");
        }
    }
}
