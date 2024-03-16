using Microsoft.AspNetCore.Mvc;
using Softata;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// The Sensor Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SoftataSensorontroller : ControllerBase
    {
        /// <summary>
        /// Get a list of implemented sensors
        /// </summary>
        /// <returns>List of sensors</returns>
        // GET: api/<SoftataSensorontroller>
        [HttpGet]
        public IEnumerable<string> Get()
        { 
            string[] Sensors = SoftataLib.Sensor.GetSensors();
            return Sensors;
        }

        /// <summary>
        /// Get a list of Pins for a sensor
        /// </summary>
        /// <param name="isensor">The enum ord of the sensor in the list of sensors</param>
        /// <returns>Statement of default and optional connections to sensor</returns>
        // GET api/<Softata>/5
        [Route("GetPins")]
        [HttpGet]  //Get pins for display
        public string GetPins(int isensor)
        {
            string value = SoftataLib.Sensor.GetPins((byte)isensor);
            return value;
        }
        /// <summary>
        /// Get list of properties for a specific sensor
        /// </summary>
        /// <param name="isensor">The enum ord of the sensor in the list of sensors</param>
        /// <returns>List of properties</returns>
        // GET api/<Softata>/5
        [Route("GetProperties")]
        [HttpGet]  //Get properties for sensor
        public IEnumerable<string> GetProperties(int isensor)
        {
            string[] properties = SoftataLib.Sensor.GetProperties((byte)isensor);
            return properties;
        }

        /// <summary>
        /// Setup a sensor with default connection settings
        /// </summary>
        /// <param name="isensor">The enum ord of the sensor in the list of sensors</param>
        /// <returns>OK with instance index or Fail</returns>
        // POST api/<Softata>
        [Route("SetupDefault")]
        [HttpPost] // Default setup for sensor
        public IActionResult SetupDefault(int isensor)
        {
            int sensorListIndex = SoftataLib.Sensor.SetupDefault((byte)isensor);
            if (sensorListIndex == -1)
            {
                return BadRequest("Sensor not found");
            }
            return Ok($"{sensorListIndex}");
        }

        /// <summary>
        /// Setup sensor with custom settings
        /// Not fully implemented at this level
        /// </summary>
        /// <param name="isensor">The enum ord of the sensor in the list of sensors</param>
        /// <param name="pin">GPIO Pin</param>
        /// <param name="settings"></param>
        /// <returns>OK with instance index or Fail</returns>
        [Route("Setup")]
        [HttpPost] // Setup for sensor
        public IActionResult Setup(int isensor, int pin, List<byte> settings = null)
        {
            int sensorListIndex = SoftataLib.Sensor.Setup((byte)isensor, (byte)pin, settings);
            if (sensorListIndex == -1)
            {
                return BadRequest("Sensor not found");
            }
            return Ok($"{sensorListIndex}");
        }

        //////////////////////////////////
        ///  Not fully implemented yet ///  
        //////////////////////////////////

        /*
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<SoftataSensorontroller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
