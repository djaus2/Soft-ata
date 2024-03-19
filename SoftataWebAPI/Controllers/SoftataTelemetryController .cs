using Microsoft.AspNetCore.Mvc;
using Softata;
using System.Diagnostics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// The Softata Telemetry Controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SoftataTelemtryController : ControllerBase
    {
        /// <summary>
        /// One read of all properties of sensor as a json string
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <returns>Values as json string</returns>
        [Route("SingleReadTelemetry")]
        [HttpGet] // Nb: Same as ReadTelemetry() in SoftataSensorController
        public string SingleReadTelemetry(int sensorListIndex)
        {
            string json = SoftataLib.Sensor.GetTelemetry((byte)sensorListIndex);
            return json;
        }

        /// <summary>
        /// Start sending Sensor Telemetry over Bluetooth
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <param name="period">Period in seconds</param>
        /// <returns>Ok or Fail</returns>
        [Route("StartSendingTelemetryBT")]
        [HttpPost]
        public IActionResult StartSendingTelemetryBT(int sensorListIndex, int period)
        {
            string result = SoftataLib.Sensor.StartSendingTelemetryBT((byte)sensorListIndex, (byte)period);
            return Ok($"{sensorListIndex}");
        }

        /// <summary>
        /// Start sending Sensor Telemetry to an Azure IoT Hub
        /// </summary>
        /// <param name="sensorListIndex">Sensor instance index</param>
        /// <param name="period">Period in seconds</param>
        /// <returns>Ok or Fail</returns>
        [Route("StartSendingTelemetryToIoTHub")]
        [HttpPost] // Default setup for sensor
        public IActionResult StartSendingTelemetryToIoTHub(int sensorListIndex, int period)
        {
            string result = SoftataLib.Sensor.StartSendingTelemetryToIoTHub((byte)sensorListIndex, (byte)period);
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
            string result = SoftataLib.Sensor.PauseSendTelemetry((byte)sensorListIndex);
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
            string result = SoftataLib.Sensor.ContinueSendTelemetry((byte)sensorListIndex);
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
            string result = SoftataLib.Sensor.StopSendingTelemetry((byte)sensorListIndex);
            return Ok($"{sensorListIndex}");
        }

    }
}
