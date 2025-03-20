using Microsoft.AspNetCore.Mvc;
using Softata;
using SoftataWebAPI.Data.Db;
using SoftataWebAPI.Data;
using System.Diagnostics;
using System.Net.Sockets;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// The Softata Telemetry Controller
    /// </summary>
    [Route("/Telemetry")]
    [ApiController]
    public class SoftataTelemetryController : SoftataControllerCls
    {
        public SoftataTelemetryController(SoftataDbContext softataContext, ISoftataGenCmds sharedService)
    : base(softataContext, sharedService)
        {
        }

        protected Softata.SoftataLib.Sensor sensor
        {
            get
            {
                return sharedService.GetSensor(HttpContext,0);
            }
        }

        /// <summary>
        /// One read of all properties of sensor as a json string
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <param name="client"></param>
        /// <returns>Values as json string</returns>
        [Route("SingleReadTelemetry")]
        [HttpGet] // Nb: Same as ReadTelemetry() in SoftataSensorController
        public string SingleReadTelemetry(int sensorListIndex)
        {
            if (Client == null)
            {
                return "BadRequest:  Client socket cannot be null. Must be connected";
            }
            string json = sensor.GetTelemetry((byte)sensorListIndex, Client);
            return json;
        }

        /// <summary>
        /// BT: Start sending Sensor Telemetry over Bluetooth
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <param name="period">Period in seconds</param>
        /// <param name="client"></param>
        /// <returns>Ok or Fail</returns>
        [Route("StartSendingTelemetryBT")]
        [HttpPost]
        public IActionResult StartSendingTelemetryBT(int sensorListIndex, int period)
        {
            if (Client == null)
            {
                return BadRequest("Client socket cannot be null. Must be connected");
            }
            string result = sensor.StartSendingTelemetryBT((byte)sensorListIndex, Client, (byte)period);
            return Ok($"{sensorListIndex}");
        }

        /// <summary>
        /// IoTHub: Start sending Sensor Telemetry to an Azure IoT Hub
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <param name="period">Period in seconds</param>
        /// <param name="client"></param>
        /// <returns>Ok or Fail</returns>
        [Route("StartSendingTelemetryToIoTHub")]
        [HttpPost] // Default setup for sensor
        public IActionResult StartSendingTelemetryToIoTHub(int sensorListIndex, int period)
        {
            if (Client == null)
            {
                return BadRequest("Client socket cannot be null. Must be connected");
            }
            string result = sensor.StartSendingTelemetryToIoTHub((byte)sensorListIndex, Client, (byte)period);
            return Ok($"{sensorListIndex}");
        }

        /// <summary>
        /// Pause sending Sensor All Telemetry
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <returns>Ok or Fail</returns>
        [Route("PauseSendingTelemetry")]
        [HttpPost]
        public IActionResult PauseSendingTelemetry(int sensorListIndex)
        {
            if (Client == null)
            {
                return BadRequest("Client socket cannot be null. Must be connected");
            }
            string result = this.sensor.PauseSendTelemetry((byte)sensorListIndex, Client);
            return Ok($"{sensorListIndex}");
        }

        /// <summary>
        /// Continue sending All Sensor Telemetry
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <returns>Ok or Fail</returns>
        [Route("ContinueSendingTelemetry")]
        [HttpPost]
        public IActionResult ContinueSendingTelemetry(int sensorListIndex)
        {
            if (Client == null)
            {
                return BadRequest("Client socket cannot be null. Must be connected");
            }
            string result = this.sensor.ContinueSendTelemetry((byte)sensorListIndex, Client);
            return Ok($"{sensorListIndex}");
        }

        /// <summary>
        /// Stop sending All Sensor Telemetry
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <returns>Ok or Fail</returns>
        [Route("StopSendingTelemetry")]
        [HttpPost]
        public IActionResult StopSendingTelemetryBT(int sensorListIndex)
        {
            if (Client == null)
            {
                return BadRequest("Client socket cannot be null. Must be connected");
            }
            string result = sensor.StopSendingTelemetry((byte)sensorListIndex, Client);
            return Ok($"{sensorListIndex}");
        }

    }
}
