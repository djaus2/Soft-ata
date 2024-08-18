using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Softata;
using Softata.Enums;
using System;
using System.Net;
using System.Net.NetworkInformation;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// The Base Controller
    /// </summary>
    [Route("/")]
    [ApiController]
    public class SoftataMiscController : ControllerBase
    {

        /// <summary>
        /// Convert int to string
        /// </summary>
        /// <returns>value as string</returns>
        // GET: api/<SoftataController>
        [Route("GetMenu")]
        [HttpGet]
        public string GetMenu(string value)
        {
            var array = value.Split(',');
            string retMsg = "Please select from:\n";
            for (int i = 0; i < array.Length; i++)
            {
                retMsg += $"{i+1}. {array[i]}";
                if(i < array.Length - 1)
                {
                    retMsg += "\n";
                }
            }
            return retMsg;
        }



       
    }
}
