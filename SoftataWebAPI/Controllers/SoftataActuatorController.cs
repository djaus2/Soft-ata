using Microsoft.AspNetCore.Mvc;
using Softata;
using System.Diagnostics;
using static Softata.SoftataLib.Actuator;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// The Actuator Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SoftataActuatorController : ControllerBase
    {
        /// <summary>
        /// Get a list of implemented actuators
        /// </summary>
        /// <returns>List of sensors</returns>
        // GET: api/<SoftataSensorontroller>
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
        // GET api/<Softata>/5
        [Route("GetPins")]
        [HttpGet]
        public string GetPins(int iactuator)
        {
            string value = SoftataLib.Actuator.GetPins((ActuatorDevice)iactuator);
            return value;
        }

        /// <summary>
        /// Get range of valid values for actuator
        /// </summary>
        /// <param name="iactuator">The enum ord of the actuator in the list of sensors</param>
        /// <returns>Valid range of values to write as a string</returns>
        // GET api/<Softata>/5
        [Route("GetValueRange")]
        [HttpGet]
        public string GetValueRange(int iactuator)
        {
            string value = SoftataLib.Actuator.GetValueRange((ActuatorDevice)iactuator);
            return value;
        }


        /// <summary>
        /// Setup a actuator with default connection settings
        /// </summary>
        /// <param name="iactuator">The enum ord of the actuator in the list of actuators</param>
        /// <returns>OK with instance index or Fail</returns>
        // POST api/<Softata>
        [Route("SetupDefault")]
        [HttpPost] // Default setup for actuator
        public IActionResult SetupDefault(int iactuator)
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
        public IActionResult Setup(int iactuator, int pin, List<byte> settings = null)
        {
            int actuatorListIndex = SoftataLib.Sensor.Setup((byte)iactuator, (byte)pin, settings);
            if (actuatorListIndex == -1)
            {
                return BadRequest("Actuator not found");
            }
            return Ok($"{actuatorListIndex}");
        }

        /// <summary>
        /// Write a byte to Actuator
        /// </summary>
        /// <param name="displayLinkedListIndex">Display Instance index</param>
        /// <param name="value">Value to set</param>
        /// <returns>OK or Fail</returns>
        // POST api/<Softata>
        [Route("WriteByte")]
        [HttpPost]
        public IActionResult WriteByte(int displayLinkedListIndex, int value)
        {
            bool result = SoftataLib.Actuator.ActuatorWrite((byte)displayLinkedListIndex, (byte)value);
            if (!result)
            {
                return BadRequest("Actuator:WriteByte fail.");
            }
            return Ok($"Actuator:WriteByte");
        }

        /*
        /// <summary>
        /// Read all properties of sensor
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <returns>Values as a list</returns>
        [Route("ReadAll")]
        [HttpGet]  //Get properties for sensor
        public IEnumerable<double> ReadAll(int sensorListIndex)
        {
            double[]? values = SoftataLib.Sensor.ReadAll((byte)sensorListIndex);
            if (values != null)
                return (double[])values;
            else
                return new double[0];
        }

        /// <summary>
        /// Read all pne property of sensor
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <returns>The value</returns>
        [Route("Read")]
        [HttpGet]  //Get properties for sensor
        public double Read(int sensorListIndex, int property)
        {
            double? value = SoftataLib.Sensor.Read((byte)sensorListIndex, (byte)property);
            if (value != null)
                return (double)value;
            else
                return (double)137137;
        }

        /// <summary>
        /// Read all properties of sensor as a json string
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <returns>Values as json string</returns>
        [Route("ReadTelemetry")]
        [HttpGet]
        public string ReadTelemetry(int sensorListIndex)
        {
            string json = SoftataLib.Sensor.GetTelemetry((byte)sensorListIndex);
            return json;
        }
        */
        /////////////////////////////////////////////
        /// 2Do 
        /// - Stream sensor data over Bluetooth
        /// - Stream sensor data to Azure IoT Hub
        /// - Implement Actuator Controller
        ///////////////////////////////////////////// 
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
