using Microsoft.AspNetCore.Mvc;
using SoftataWebAPI.Data;
using SoftataWebAPI.Data.Db;
using System.Text.Json;

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// An API controller for SoftataLib that
    /// allows you to get and set the state.
    /// This easily could be using a database.
    /// </summary>
    [Route("/Session")]
    [ApiController]
    public class SoftataSession : SoftataControllerCls
    {
        public SoftataSession(SoftataDbContext softataContext, ISoftataGenCmds sharedService)
        : base(softataContext, sharedService)
        {
        }

        /// <summary>
        /// Interact with the Session (Set/Get)
        /// </summary>
        /// <returns>Set of Session values</returns>
        // GET: api/<SoftataController>
        [Route("PeekStr")]
        [HttpGet]
        public string PeekStr(string key)
        {
            string? name = HttpContext.Session.GetString(key);
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }
            else
            {

                return (string)name;
            }

        }

        /// <summary>
        /// Get session string value and remove from session.
        /// </summary>
        /// <returns></returns>
        // GET: api/<SoftataController>
        [Route("PopStr")]
        [HttpGet]
        public string PopStr(string key)
        {
            string? name = HttpContext.Session.GetString(key);
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }
            else
            {
                HttpContext.Session.Remove(key);
                return (string)name;
            }

        }

        /// <summary>
        /// Add string value to Session
        /// </summary>
        /// <param name="key"></param>
        /// <param name="aval"></param>
        /// <returns>Ok</returns>
        [Route("SetStr")]
        [HttpPost]
        public IActionResult SetStr(string key, string aval)
        {
            HttpContext.Session.SetString(key, aval);
            return Ok();
        }

        //////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get session value as int
        /// </summary>
        /// <returns></returns>
        // GET: api/<SoftataController>
        [Route("PeekNum")]
        [HttpGet]
        public int PeekNum(string key)
        {
            int? name = HttpContext.Session.GetInt32(key);
            if (name == null)
            {
                return 0xffff;
            }
            else
            {

                return (int)name;
            }

        }

        /// <summary>
        /// Get session value as int and remove from session.
        /// </summary>
        /// <returns></returns>
        // GET: api/<SoftataController>
        [Route("PopNum")]
        [HttpGet]
        public int PopNum(string key)
        {
            int? name = HttpContext.Session.GetInt32(key);
            if (name == null)
            {
                return 0xffff;
            }
            else
            {
                HttpContext.Session.Remove(key);
                return (int)name;
            }

        }


        /// <summary>
        /// Add value to session with key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="aval"></param>
        /// <returns>Ok</returns>
        [Route("SetNum")]
        [HttpPost]
        public IActionResult SetNum(string key, int aval)
        {
            HttpContext.Session.SetInt32(key, aval);
            return Ok();
        }

        ////////////////////////////////////////////////////////////////////
        ///
        /// <summary>
        /// Get session value as int
        /// </summary>
        /// <returns></returns>
        // GET: api/<SoftataController>
        [Route("PeekObj")]
        [HttpGet]
        public object PeekObj(string key)
        {
            object? name = HttpContext.Session.Get<object>(key);
            if (name == null)
            {
                return (object)"";
            }
            else
            {

                return (object)name;
            }

        }

        /// <summary>
        /// Get session value as object and remove from session.
        /// </summary>
        /// <returns></returns>
        // GET: api/<SoftataController>
        [Route("PopObject")]
        [HttpGet]
        public object? PopObject(string key)
        {
            object? val = HttpContext.Session.Get<object>(key);
            if (val == null)
                return -1;
            var jsonElement = HttpContext.Session.Get<JsonElement>(key);
            if (jsonElement.ValueKind == JsonValueKind.Undefined)
            {
                return -1;
            }
            else if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                var item = JsonSerializer.Deserialize<List<object>>(jsonElement.GetRawText())!;
                HttpContext.Session.Remove(key);
                return item;
            }
            else if (jsonElement.ValueKind == JsonValueKind.String)
            {
                var item = JsonSerializer.Deserialize<String>(jsonElement.GetRawText())!;
                HttpContext.Session.Remove(key);
                return item;
            }
            else if (jsonElement.ValueKind == JsonValueKind.Number)
            {
                var item = JsonSerializer.Deserialize<double>(jsonElement.GetRawText())!;
                HttpContext.Session.Remove(key);
                return item;
            }
            else
            {
                var item = JsonSerializer.Deserialize<object>(jsonElement.GetRawText())!;
                HttpContext.Session.Remove(key);
                return item; 
            }

        }

        /// <summary>
        /// Add value to session with key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="aval"></param>
        /// <returns>Ok</returns>
        [Route("SetObj")]
        [HttpPost]
        public IActionResult SetObj(string key, object aval)
        {
            HttpContext.Session.Set<object>(key, aval);
            return Ok();
        }

        ////////////////////////////////////////////////////////////////////
    }
}
