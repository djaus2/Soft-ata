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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// The Base Controller
    /// </summary>
    [Route("/")]
    [ApiController]
    public class SoftataGPIOADCController : ControllerBase
    {
        /// <summary>
        /// Set pin mode
        /// </summary>>
        /// <param name="pin">GPIO Pin</param>
        /// <param name="mode">0=Input,1=Output,2=ADC,3=PWM</param>
        /// <returns>OK</returns>
        [Route("SetPinMode")]
        [HttpPost]
        public IActionResult SetPinMode(int pin, int mode)
        {
            PinMode _mode = (PinMode)mode;
            if (_mode != PinMode.AnalogInput)
            {
                SoftataLib.Analog.SetAnalogPin(AnalogDevice.Undefined, pin);
            }
            else if (_mode == PinMode.DigitalInput || _mode == PinMode.DigitalOutput)
            {
                SoftataLib.Digital.SetPinMode(pin, _mode);
            }
            if (_mode != PinMode.PwmOutput)
            {
                SoftataLib.Digital.SetPinMode(pin, PinMode.DigitalOutput);
            }
            else
                return BadRequest("Invalid Pin Mode");
            return Ok($"{pin}");
        }

        /// <summary>
        /// Init ADC
        /// </summary>
        /// <param name="PicoMode">0=Grove Shield,1=Otherwise</param>>
        /// <returns>OK</returns>
        [Route("InitAnalogDevicePins")]
        [HttpPost]
        public IActionResult InitADCPins(int PicoMode = 0)
        {
            SoftataLib.Analog.InitAnalogDevicePins((RPiPicoMode)PicoMode);
            return Ok($"{RPiPicoMode.groveShield}");
        }


        /// <summary>
        /// Get state of a Digital Input Pin
        /// </summary>
        /// <param name="pin">GPIO Pin</param>
        /// <returns>Pin state</returns>
        [Route("GetPinState")]
        [HttpGet]
        public bool GetPinState(int pin)
        {
            bool value = SoftataLib.Digital.GetPinState(pin);
            return value;
        }


        /// <summary>
        /// Set Digital Output Pin state
        /// </summary>>
        /// <param name="pin">GPIO Pin</param>
        /// <param name="pinstate">0=Low,1=High</param>
        /// <returns>OK</returns>
        [Route("SetPinState")]
        [HttpPost] 
        public IActionResult SetPinState(int pin, PinState pinstate)
        {
            SoftataLib.Digital.SetPinState(pin, pinstate);
            return Ok($"{pin}");
        }

        /// <summary>
        /// Read an ADC Pin value
        /// </summary>
        /// <param name="pin">GPIO Pin</param>
        /// <returns>Pin state</returns>
        [Route("ReadADC")]
        [HttpGet]
        public int ReadADC(int pin)
        {
            int value = SoftataLib.Analog.AnalogRead(pin);
            return value;
        }

        /// <summary>
        /// Set PWM Pin value
        /// </summary>
        /// <param name="pin">GPIO Pin</param>>
        /// <param name="value">0..255</param>
        /// <returns>OK</returns>
        [Route("SetPWM")]
        [HttpPost]
        public IActionResult SetPWM(int pin, byte value)
        {
            SoftataLib.PWM.SetPWM(pin, value);
            return Ok($"{pin}");
        }

    }
}
