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
    public class SoftataGroveADCController : ControllerBase
    {

        /// <summary>
        /// Set Digital pin mode
        /// </summary>>
        /// <param name="pin">GroveGPIOPin</param>
        /// <param name="mode">Input,Output</param>
        /// <returns>OK</returns>
        [Route("Grove/Digital/SetPinMode")]
        [HttpPost]
        public IActionResult GroveDigitalSetPinMode(GroveGPIOPin pin, DigitalPinMode mode)
        {
            byte pinn =  (byte)((byte)GroveGPIOPinX.p16 + (byte)pin);
            SoftataLib.Digital.SetPinMode(pinn, (PinMode)mode);
                return Ok($"Pin:{pinn} Mode:{(PinMode)mode}");
        }



        /// <summary>
        /// Get state of a Digital Input Pin
        /// </summary>
        /// <param name="pin">GroveGPIOPin</param>
        /// <returns>Pin state</returns>
        [Route("Grove/Digital/GetPinState")]
        [HttpGet]
        public int GroveDigitalGetPinState(GroveGPIOPin pin)
        {
            byte pinn = (byte)((byte)GroveGPIOPinX.p16 + (byte)pin);
            bool value = true; // SoftataLib.Digital.GetPinState(pinn);
            return value ? 1 : 0;
        }

  

        /// <summary>
        /// Set Digital Output Pin state
        /// </summary>>
        /// <param name="pin">GroveGPIOPin</param>
        /// <param name="pinstate">Low,High</param>
        /// <returns>OK</returns>
        [Route("Grove/Digital/SetlPinState")]
        [HttpPost]
        public IActionResult GroveDigitalSetPinState(GroveGPIOPin pin, PinState pinstate)
        {
            byte pinn = (byte)((byte)GroveGPIOPinX.p16 + (byte)pin);
            SoftataLib.Digital.SetPinState(pinn, pinstate);
            return Ok($"Pin:{pinn} State:{pinstate}");
        }

        ///////////////////ADC////////////////////////////////////////


        /// <summary>
        /// Set pin as ADC with Maximum Value
        /// </summary>
        /// <param name="pin">GroveAnalogPin</param>
        /// <param name="device"></param>
        /// <param name="adcResolutionBits">10,12</param>
        /// <param name="bitsShiftRight">ie / by 2 </param>
        /// <returns>OK</returns>
        [Route("Grove/Analog/SetPinModeADC")]
        [HttpPost]
        public IActionResult SetPinModeADC(GroveAnalogPin pin, AnalogDevice device, ADCResolutionBits adcResolutionBits=ADCResolutionBits.Bits10, byte bitsShiftRight=0)
        {
            byte pinn = (byte)((byte)GroveAnalogPinX.A0 + (byte)pin);
            bool result = SoftataLib.Analog.SetAnalogPin(device, pinn, adcResolutionBits,bitsShiftRight);
            if(result)
                return Ok($"Pin:{pinn} ADC mode ADCResolutionBits: {adcResolutionBits} BitShiftRight: {bitsShiftRight}");
            else
                return BadRequest($"Pin:{pinn} ADC mode ADCResolutionBits: {adcResolutionBits} BitShiftRight: {bitsShiftRight}");
        }


        /// <summary>
        /// Read an ADC Pin value
        /// </summary>
        /// <param name="pin">GroveAnalogPin</param>
        /// <returns>Pin state</returns>
        [Route("Grove/Analog/ReadPinADC")]
        [HttpGet]
        public int GroveAnalogReadPinADC(GroveAnalogPin pin)
        {
            byte pinn = (byte)((byte)GroveAnalogPinX.A0 + (byte)pin);
            int value = SoftataLib.Analog.AnalogRead(pinn);
            return value;
        }


        /// <summary>
        /// Set PWM pin mode = DigitalOutput
        /// </summary>>
        /// <param name="pin">GroveGPIOPin</param>
        /// <param name="pwmResolutionBits">4-16 Default 8 Bits</param>
        /// <returns>OK</returns>
        [Route("Grove/PWM/SetPinMode")]
        [HttpPost]
        public IActionResult GrovePWMSetPinMode(GrovePWMPin pin, byte pwmResolutionBits = 8)
        {
            byte pinn = (byte)((byte)GrovePWMPinX.p16 + (byte)pin);
            SoftataLib.PWM.SetPinModePWM(pinn, pwmResolutionBits);
            return Ok($"Pin:{pinn} Mode:{PinMode.PwmOutput} Bits(4to16): {pwmResolutionBits}");
        }

        /// <summary>
        /// Set PWM Pin value
        /// </summary>
        /// <param name="pin">GrovePWMPin</param>>
        /// <param name="value">0..255</param>
        /// <returns>OK</returns>
        [Route("Grove/PWM/SetPinPWM")]
        [HttpPost]
        public IActionResult GroveAnalogSetPinPWM(GrovePWMPin pin, int value)
        {
            byte pinn = (byte)((byte)GrovePWMPinX.p16 + (byte)pin);
            SoftataLib.PWM.SetPWM(pinn, value);
            return Ok($"Pin:{pinn} Value:{value}");
        }

    }
}
