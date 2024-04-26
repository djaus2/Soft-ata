using Microsoft.AspNetCore.Mvc;
using Softata;
using static Softata.SoftataLib;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// The SoftataController Display Controller
    /// </summary>
    [Route("/Display")]
    [ApiController]
    public class SoftataDisplayController : ControllerBase
    {
        /// <summary>
        /// Get a list of implemented displays
        /// </summary>
        /// <returns>List of displays</returns>
        // GET: api/<SoftataController>
        [Route("Get")]
        [HttpGet] //Get displays
        public IEnumerable<string> Get()
        {
            string[] Displays = SoftataLib.Display.GetDisplays();
            return Displays;
        }

        /// <summary>
        /// Get a list of pins for a specific display
        /// </summary>
        /// <param name="idisplay">The enum ord of the display in the list of displays</param>
        /// <returns>Statement of default and optional connections to the display.</returns>
        // GET api/<SoftataController>/5
        [Route("GetPins")]
        [HttpGet]  //Get pins for display
        public string GetPins(int idisplay=0)
        {
            string value = SoftataLib.Display.GetPins((byte)idisplay);
            return value;
        }

        /// <summary>
        /// Setup a display with default connection settings
        /// </summary>
        /// <param name="idisplay">The enum ord of the display in the list of displays</param>
        /// <returns>OK with instance index or Fail</returns>
        // POST api/<SoftataController>
        [Route("SetupDefault")]
        [HttpPost] // Default setup for display
        public IActionResult SetupDefault(int idisplay=0)
        {
            int displayListIndex = SoftataLib.Display.SetupDefault((byte)idisplay);
            if (displayListIndex == -1)
            {
                return BadRequest("Display not found");
            }
            return Ok($"{displayListIndex}");
        }


        /// <summary>
        /// Setup display with custom settings
        /// Not fully implemented at this level
        /// </summary>
        /// <param name="idisplay">The enum ord of the display in the list of displays</param>
        /// <param name="pin">GPIO Pin</param>
        /// <param name="settings"></param>
        /// <returns>OK with instance index or Fail</returns>
        [Route("Setup")]
        [HttpPost] // Setup for display
        public IActionResult Setup(int idisplay, int pin, List<byte> settings = null)
        {
            int displayListIndex = SoftataLib.Display.Setup((byte)idisplay,(byte) pin, settings);
            if (displayListIndex == -1)
            {
                return BadRequest("Display not found");
            }
            return Ok($"{displayListIndex}");
        }

        /// <summary>
        /// Clear any display
        /// </summary>
        /// <param name="displayLinkedListIndex">Display instance index</param>
        /// <returns>Ok or fail</returns>
        // POST api/<SoftataController>
        [Route("Clear")]
        [HttpPost] // Default setup for display
        public IActionResult Clear(int displayLinkedListIndex=0)
        {
            bool result = SoftataLib.Display.Clear((byte)displayLinkedListIndex);
            if (!result)
            {
                return BadRequest("Display Clear fail.");
            }
            return Ok($"Cleared display");
        }

        /// <summary>
        /// Return the cursor to the home position (LCD display)
        /// </summary>
        /// <param name="displayLinkedListIndex">Display instance index</param>
        /// <returns></returns>
        // POST api/<SoftataController>
        [Route("Home")]
        [HttpPost] // Default setup for display
        public IActionResult Home(int displayLinkedListIndex=0)
        {
            bool result = SoftataLib.Display.Home((byte)displayLinkedListIndex);
            if (!result)
            {
                return BadRequest("Display:Home fail.");
            }
            return Ok($"Display:Home");
        }

        /// <summary>
        /// Write an int to any display as a string
        /// </summary>
        /// <param name="displayLinkedListIndex">Display Instance index</param>
        /// <param name="value">Value as a string</param>
        /// <returns>OK or Fail</returns>
        // POST api/<SoftataController>
        [Route("WriteIntString")]
        [HttpPost] // Default setup for display
        public IActionResult WriteIntString(int displayLinkedListIndex, int value)
        {
            bool result = SoftataLib.Display.WriteString((byte)displayLinkedListIndex, $"{value}");
            if (!result)
            {
                return BadRequest("Display:WriteIntString fail.");
            }
            return Ok($"Display:WriteIntString");
        }

        /// <summary>
        /// Write a string to an LCD display using current cursor.
        /// </summary>
        /// <param name="displayLinkedListIndex">Display Instance index</param>
        /// <param name="value">Value to write</param>
        /// <returns>OK or Fail</returns>
        // POST api/<SoftataController>
        [Route("WriteString")]
        [HttpPost] 
        public IActionResult WriteString(int displayLinkedListIndex, string value)
        {
            bool result = SoftataLib.Display.WriteString((byte)displayLinkedListIndex, value);
            if (!result)
            {
                return BadRequest("Display:WriteString fail.");
            }
            return Ok($"Display:WriteString");
        }

        /// <summary>
        /// Write a string to an LCD display
        /// ... at a specific location
        /// </summary>
        /// <param name="displayLinkedListIndex">Display Instance index</param>
        /// <param name="value">Value as a string</param>
        /// <param name="x">Position in row</param>
        /// <param name="y">Row</param>
        /// <returns>OK or Fail</returns>
        // POST api/<SoftataController>
        [Route("WriteStringXY")]
        [HttpPost] // Default setup for display
        public IActionResult WriteStringXY(int displayLinkedListIndex,int x, int y, string value)
        {
            bool result = SoftataLib.Display.WriteString((byte)displayLinkedListIndex, (byte) x, (byte) y,value);
            if (!result)
            {
                return BadRequest("Display:WriteStringXY fail.");
            }
            return Ok($"Display:WriteStringXY");
        }

        /// <summary>
        /// Set the cursor position on an LCD display.
        /// </summary>
        /// <param name="displayLinkedListIndex">Display Instance index</param>
        /// <param name="x">Position in row</param>
        /// <param name="y">Row</param>
        /// <returns></returns>
        // POST api/<SoftataController>
        [Route("SetCursor")]
        [HttpPost] // Default setup for display
        public IActionResult SetCursor(int displayLinkedListIndex, int x, int y)
        {
            bool result = SoftataLib.Display.SetCursor((byte)displayLinkedListIndex, (byte)x, (byte)y);
            if (!result)
            {
                return BadRequest("Display:SetCursor fail.");
            }
            return Ok($"Display:SetCursor");
        }

        /// <summary>
        /// Run a display specific miscellaneous command
        /// </summary>
        /// <param name="displayLinkedListIndex">Display instance index</param>
        /// <param name="miscCmndIndex">Misc Command</param>
        /// <returns>OK or fail</returns>
        // POST api/<SoftataController>
        [Route("Misc")]
        [HttpPost] // Default setup for display
        public IActionResult Misc(int displayLinkedListIndex, int miscCmndIndex)
        {
            bool result = SoftataLib.Display.Backlight((byte)displayLinkedListIndex, (byte)miscCmndIndex);
            if (!result)
            {
                return BadRequest("Display:Misc fail.");
            }
            return Ok($"Display:Misc");
        }
    }
}
