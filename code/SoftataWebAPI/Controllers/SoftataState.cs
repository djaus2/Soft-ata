using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Softata;
using Softata.Enums;
using System;
using System.Net;
using System.Net.NetworkInformation;
using static Softata.SoftataLib;
using static Softata.SoftataLib.Analog;



namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// An API controller for SoftataLib that
    /// allows you to get and set the state.
    /// This easily could be using a database.
    /// </summary>
    [Route("/")]
    [ApiController]
    public class SoftataState : ControllerBase
    {

        /// <summary>
        /// Get state of SoftataLib.Aval
        /// </summary>
        /// <param name="index">As returned from</param
        /// <returns> SoftataLib.Aval as string</returns>
        [Route("State/GetAvalue")]
        [HttpGet]
        public object GetAValue(string key)
        {
            var lib = SoftataLib.GetSoftataLib(key);
            if (lib != null)
                return lib.GetAvalue();
            else
                return ($"SoftataLib not found [{key}]");
        }

        /// <summary>
        /// Set state of SoftataLib.Aval
        /// </summary>
        /// <param name="key"></param>
        /// <param name="aval"></param>
        /// <returns>Ok</returns>
        [Route("State/SetAvalue")]
        [HttpPost]
        public IActionResult SetAvalue(string key, object aval)
        {
            var lib = SoftataLib.GetSoftataLib(key);
            if (lib != null)
            {
                lib.SetAvalue(aval);
                return Ok($"Avalue:{aval}");
            }
            else
                return BadRequest($"SoftataLib not found [{key}]");
        }

        /// <summary>
        /// Update state of SoftataLib.Aval
        /// Create new if not exists
        /// </summary>
        /// <param name="key"></param>
        /// <param name="aval"></param>
        /// <returns>Ok</returns>
        [Route("State/UpdateAvalue")]
        [HttpPost]
        public IActionResult UpdateAvalue(string key, object aval)
        {
            var lib = SoftataLib.GetSoftataLib(key);
            if (lib != null)
            {
                lib.SetAvalue(aval);
                return Ok($"Avalue:{aval}");
            }
            else
                return BadRequest($"SoftataLib not found [{key}]");
        }

        /// <summary>
        /// Create new SoftataLib instance
        /// </summary>
        /// <returns>Index of instance</returns>
        [Route("NewLib")]
        [HttpGet]
        public string NewLib()
        {
            string guid = SoftataLib.NewSoftataLib();
            return guid;
        }

        /// <summary>
        /// Deelete existing SoftataLib instance
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Index of instance</returns>
        [Route("DeleteState")]
        [HttpDelete]
        public IActionResult DeleteState(string key)
        {
            var lib = SoftataLib.GetSoftataLib(key);
            if (lib != null)
            {
                if( SoftataLib.Delete(key))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Deletion failed.");
                }
            }
            return BadRequest("Invalid key/key not found.");
        }

        /// <summary>
        /// Create new SoftataLib instance
        /// </summary>
        /// <returns>Index of instance</returns>
        [Route("ClearAll")]
        [HttpDelete]
        public void ClearAll()
        {
             SoftataLib.ClearAll();
        }
        //////////////////////////////////////////////////////////
        //Using Session for state example
        /////////////////////////////////////////////////////////
 
        public const string SessionKeyName = "_Name";
        public const string SessionKeyAge = "_Age";

        /// <summary>
        /// Interact with the Session (Set/Get)
        /// </summary>
        /// <returns>Set of Session values</returns>
        // GET: api/<SoftataController>
        [Route("GetSetSession")]
        [HttpGet]
        public IEnumerable<string> GetSetSession()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyName)))
            {
                HttpContext.Session.SetString(SessionKeyName, "The Doctor");
                HttpContext.Session.SetInt32(SessionKeyAge, 73);
            }
            var name = HttpContext.Session.GetString(SessionKeyName);
            var age = HttpContext.Session.GetInt32(SessionKeyAge).ToString();
            return from x in new[] { name, age } select x;
        }





    }
}
