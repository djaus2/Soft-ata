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
        /// Set Digital pin mode
        /// </summary>>
        /// <param name="pin">GPIO Pin</param>
        /// <param name="mode">0=Input,1=Output</param>
        /// <returns>OK</returns>
        [Route("SetPinMode")]
        [HttpPost]
        public IActionResult SetDigitalPinMode(int pin, int mode)
        {
            PinMode _mode = (PinMode)mode;
            if (_mode == PinMode.DigitalInput || _mode == PinMode.DigitalOutput)
            {
                SoftataLib.Digital.SetPinMode(pin, _mode);
            }
            else
                return BadRequest("Invalid Pin:{pin} Mode:{Mode}");
            return Ok($"Pin:{pin} Mode:{(PinMode)mode}");
        }



        /// <summary>
        /// Get state of a Digital Input Pin
        /// </summary>
        /// <param name="pin">GPIO Pin</param>
        /// <returns>Pin state</returns>
        [Route("GetPinState")]
        [HttpGet]
        public int GetDigitalPinState(int pin)
        {
            bool value = SoftataLib.Digital.GetPinState(pin);
            return value ? 1 : 0;
        }


        /// <summary>
        /// Set Digital Output Pin state
        /// </summary>>
        /// <param name="pin">GPIO Pin</param>
        /// <param name="pinstate">0=Low,1=High</param>
        /// <returns>OK</returns>
        [Route("SetPinState")]
        [HttpPost]
        public IActionResult SetDigitalPinState(int pin, PinState pinstate)
        {
            SoftataLib.Digital.SetPinState(pin, pinstate);
            return Ok($"Pin:{pin} State:{pinstate}");
        }

        ///////////////////ADC////////////////////////////////////////

        /// <summary>
        /// Init ADC
        /// </summary>
        /// <param name="PicoMode">0=Grove Shield,1=Otherwise</param>>
        /// <returns>OK</returns>
        [Route("InitADCPins")]
        [HttpPost]
        public IActionResult InitADCPins(int PicoMode = 0)
        {
            SoftataLib.Analog.InitAnalogDevicePins((RPiPicoMode)PicoMode);
            return Ok($"PicoMode:{(RPiPicoMode)PicoMode}");
        }

        /// <summary>
        /// Set pin as ADC
        /// </summary>
        /// <param name="pin">ADC Pin GPIO 26,27,28</param>
        /// <returns>OK</returns>
        [Route("SetADCPin")]
        [HttpPost]
        public IActionResult SetADCPin(int pin)
        {
            if ((new List<int> { 26, 27, 28 }).Contains(pin))
            {
                SoftataLib.Analog.SetAnalogPin(AnalogDevice.Undefined, pin);
                return Ok($"Pin:{pin} ADC mode");
            }
            else
                return BadRequest($"Invalid ADC Pin:{pin}");
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
        /// Set PWM pin 
        /// </summary>>
        /// <param name="pin">GPIO Pin</param>
        /// <returns>OK</returns>
        [Route("SetPWMPin")]
        [HttpPost]
        public IActionResult SetPWMPin(int pin)
        {
            SoftataLib.Digital.SetPinMode(pin, PinMode.DigitalOutput);
            return Ok($"Pin:{pin} PWM Mode");
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
            return Ok($"Pin:{pin} Value:{value}");
        }

    }
}
