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
    /// The Databse CRuD Controller
    /// Nb: No update option
    /// </summary>
    [Route("/Database")]
    [ApiController]
    public class SoftataDatabaseController(SoftataContext context, ISoftataGenCmds sharedService) : ControllerBase
    {
        [Route("SoftataClearDb")]
        [HttpGet]
        public IActionResult SoftataClearDb(int pin)
        {
            if (sharedService.SoftataClearDb(context, pin))
                return Ok("SoftataClearDb");
            else
                return BadRequest("Failed-ClearSoftataDb");
        }

        /// <summary>
        /// Clears the database tables
        /// Reads data from Pico: SoftataGetDatafrmPico()
        /// Saves data to database
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        [Route("SoftataCreateDb")]
        [HttpGet]
        public IActionResult SoftataCreateDb(int pin)
        {
            if(sharedService.SoftataCreateDb(context, pin))
                return Ok("GotSoftataData");
            else
                return BadRequest("Failed-GotSoftataData");
        }


        [Route("SoftataReadDb")]
        [HttpGet]
        public IActionResult SoftataReadDb()
        {
            if(sharedService.ReadSoftataDataDb(context))
                return Ok("SoftataReadDb");
            else
                return BadRequest("Failed-SoftataReadDb");
        }
    }
}
