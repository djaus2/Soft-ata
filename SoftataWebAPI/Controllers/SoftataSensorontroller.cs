using Microsoft.AspNetCore.Mvc;
using Softata;
using System.Diagnostics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// The Softata Sensor Controller
    /// </summary>
    [Route("/Sensor")]
    [ApiController]
    public class SoftataSensorController : ControllerBase
    {
        /// <summary>
        /// Get a list of implemented sensors
        /// </summary>
        /// <returns>List of sensors</returns>
        // GET: api/<SoftataSensorontroller>
        [Route("Get")]
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
        // GET api/<SoftataController>/5
        [Route("GetPins")]
        [HttpGet]  //Get pins for display
        public string GetPins(int isensor=0)
        {
            string value = SoftataLib.Sensor.GetPins((byte)isensor);
            return value;
        }


        /// <summary>
        /// Get list of properties for a specific sensor
        /// </summary>
        /// <param name="isensor">The enum ord of the sensor in the list of sensors</param>
        /// <returns>List of properties</returns>
        // GET api/<SoftataController>/5
        [Route("GetProperties")]
        [HttpGet]  //Get properties for sensor
        public IEnumerable<string> GetProperties(int isensor=0)
        {
            string[] properties = SoftataLib.Sensor.GetProperties((byte)isensor);
            return properties;
        }


        /// <summary>
        /// Setup a sensor with default connection settings
        /// </summary>
        /// <param name="isensor">The enum ord of the sensor in the list of sensors</param>
        /// <returns>OK with instance index or Fail</returns>
        // POST api/<SoftataController>
        [Route("SetupDefault")]
        [HttpPost] // Default setup for sensor
        public IActionResult SetupDefault(int isensor=0)
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

        /// <summary>
        /// Read all properties of sensor
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <returns>Values as a list</returns>
        [Route("ReadAll")]
        [HttpGet]  //Get properties for sensor
        public IEnumerable<double> ReadAll(int sensorListIndex=0)
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
        /// <param name="property">Index to the property</param>
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
        public string ReadTelemetry(int sensorListIndex=0)
        {
            string json = SoftataLib.Sensor.GetTelemetry((byte)sensorListIndex);
            return json;
        }
    }
}
