using Microsoft.AspNetCore.Mvc;
using Softata;
using System.Diagnostics;
using static Softata.SoftataLib.Actuator;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// The Softata Actuator Controller
    /// </summary>
    [Route("/Actuator")]
    [ApiController]
    public class SoftataActuatorController : ControllerBase
    {
        /// <summary>
        /// Get a list of implemented actuators
        /// </summary>
        /// <returns>List of sensors</returns>
        // GET: api/<SoftataSensorontroller>
        [Route("Get")]
        [HttpGet]
        public IEnumerable<string> Get()
        {
            string[] actuators = SoftataLib.Actuator.GetActuators();
            return actuators;
        }

        /// <summary>
        /// Get a list of Pins for an actuator
        /// </summary>
        /// <param name="iactuator">The enum ord of the actuator in the list of sensors</param>
        /// <returns>Statement of default and optional connections to sensor</returns>
        // GET api/<SoftataController>/5
        [Route("GetPins")]
        [HttpGet]
        public string GetPins(int iactuator=0)
        {
            string value = SoftataLib.Actuator.GetPins((ActuatorDevice)iactuator);
            return value;
        }

        /// <summary>
        /// Get range of valid values for actuator
        /// </summary>
        /// <param name="iactuator">The enum ord of the actuator in the list of sensors</param>
        /// <returns>Valid range of values to write as a string</returns>
        // GET api/<SoftataController>/5
        [Route("GetValueRange")]
        [HttpGet]
        public string GetValueRange(int iactuator=0)
        {
            string value = SoftataLib.Actuator.GetValueRange((ActuatorDevice)iactuator);
            return value;
        }


        /// <summary>
        /// Setup a actuator with default connection settings
        /// </summary>
        /// <param name="iactuator">The enum ord of the actuator in the list of actuators</param>
        /// <returns>OK with instance index or Fail</returns>
        // POST api/<SoftataController>
        [Route("SetupDefault")]
        [HttpPost] // Default setup for actuator
        public IActionResult SetupDefault(int iactuator=0)
        {
            int actuatorListIndex = SoftataLib.Actuator.SetupDefault((SoftataLib.Actuator.ActuatorDevice)(byte)iactuator);
            if (actuatorListIndex == -1)
            {
                return BadRequest("Sensor not found");
            }
            return Ok($"{actuatorListIndex}");
        }

        /// </summary>
        /// <param name="iactuator">The enum ord of the actuator in the list of actuators</param>
        /// <returns>OK with instance index or Fail</returns>
        // POST api/<SoftataController>
        [Route("SetupDefaultfromList")]
        [HttpPost] // Default setup for actuator
        public IActionResult SetupDefaultfromList(ActuatorDevice iactuator )
        {
            int actuatorListIndex = SoftataLib.Actuator.SetupDefault((SoftataLib.Actuator.ActuatorDevice)(byte)iactuator);
            if (actuatorListIndex == -1)
            {
                return BadRequest("Sensor not found");
            }
            return Ok($"{actuatorListIndex}");
        }

        /// <summary>
        /// Setup actuator with custom settings
        /// Not fully implemented at this level
        /// </summary>
        /// <param name="iactuator">The enum ord of the actuator in the list of actuators</param>
        /// <param name="pin">GPIO Pin</param>
        /// <param name="settings"></param>
        /// <returns>OK with instance index or Fail</returns>
        [Route("Setup")]
        [HttpPost]
        public IActionResult Setup(int iactuator, byte pin, List<byte> settings = null)
        {
            int actuatorListIndex = SoftataLib.Actuator.Setup((ActuatorDevice)iactuator, (byte)pin);
            if (actuatorListIndex == -1)
            {
                return BadRequest("Actuator not found");
            }
            return Ok($"{actuatorListIndex}");
        }

        /// <summary>
        /// Write a byte to Actuator
        /// </summary>
        /// <param name="actuatorListIndex">Display Instance index</param>
        /// <param name="value">Value to set</param>
        /// <returns>OK or Fail</returns>
        // POST api/<SoftataController>
        [Route("WriteByte")]
        [HttpPost]
        public IActionResult WriteByte(int actuatorListIndex, int value)
        {
            bool result = SoftataLib.Actuator.ActuatorWrite((byte)actuatorListIndex, (byte)value);
            if (!result)
            {
                return BadRequest("Actuator:WriteByte fail.");
            }
            return Ok($"Actuator:WriteByte");
        }

        /// <summary>
        /// Write a word to Actuator
        /// </summary>
        /// <param name="actuatorListIndex">Display Instance index</param>
        /// <param name="value">Value to set</param>
        /// <returns>OK or Fail</returns>
        // POST api/<SoftataController>
        [Route("WriteWord")]
        [HttpPost]
        public IActionResult WriteWord(int actuatorListIndex, int value)
        {
            bool result = SoftataLib.Actuator.ActuatorWriteWord((byte)actuatorListIndex, (UInt16)value);
            if (!result)
            {
                return BadRequest("Actuator:WriteByte fail.");
            }
            return Ok($"Actuator:WriteByte");
        }

        [Route("ToggleBit")]
        [HttpPost]
        public IActionResult ToggleBit(int actuatorListIndex, byte bit)
        {
            bool result = SoftataLib.Actuator.ToggleBit((byte)actuatorListIndex, bit);
            if (!result)
            {
                return BadRequest("Actuator:ToggleBit fail.");
            }
            return Ok($"{actuatorListIndex}");
        }

        [Route("ClearBit")]
        [HttpPost]
        public IActionResult ClearBit(int actuatorListIndex, byte bit)
        {
            bool result = SoftataLib.Actuator.ClearBit((byte)actuatorListIndex, bit);
            if (!result)
            {
                return BadRequest("Actuator:ClearBit fail.");
            }
            return Ok($"{actuatorListIndex}");
        }

        [Route("SetBit")]
        [HttpPost]
        public IActionResult SetBit(int actuatorListIndex, byte bit)
        {
            bool result = SoftataLib.Actuator.SetBit((byte)actuatorListIndex, bit);
            if (!result)
            {
                return BadRequest("Actuator:SetBit fail.");
            }
            return Ok($"{actuatorListIndex}");
        }
    }
}
