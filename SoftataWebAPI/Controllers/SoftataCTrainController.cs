using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Softata;
using Softata.Enums;
using System;
using System.Net;
using System.Net.NetworkInformation;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// The Base Controller
    /// </summary>
    [Route("/")]
    [ApiController]
    public class SoftataTrainController : ControllerBase
    {
        /// <summary>
        /// Stop the train
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [Route("TrainStop")]
        [HttpPost]
        public IActionResult TrainStop(int index)
        {

            if (true)
            {;
                return Ok(index);
            }
            else
            {
                return BadRequest("RightSwitch");
            }
        }

        /// <summary>
        /// Continue the train at previous speed
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [Route("TrainContinue")]
        [HttpPost]
        public IActionResult TrainContinue(int index)
        {

            if (true)
            {
                ;
                return Ok(index);
            }
            else
            {
                return BadRequest("RightSwitch");
            }
        }

        /// <summary>
        /// Set train speed moving forwards
        /// </summary>
        /// <param name="index"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        [Route("TrainForward")]
        [HttpPost]
        public IActionResult TrainForward(int index, byte speed)
        {

            if (true)
            {
                return Ok(index);
            }
            else
            {
                return BadRequest("RightSwitch");
            }
        }


        /// <summary>
        /// Set train speed moving backwards
        /// </summary>
        /// <param name="index"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        [Route("TrainReverse")]
        [HttpPost]
        public IActionResult TrainReverse(int index, byte speed)
        {

            if (true)
            {
                return Ok(index);
            }
            else
            {
                return BadRequest("RightSwitch");
            }
        }

        public enum TrackPoint
        {
            straightY,
            straightLeft,
            straightRight,
            curvedLeftIn,
            curvedLeftOut,
            curvedRightIn,
            curvedRightOut
        }

        public enum TrackSwitch
        {
            left,
            right,
            straighAhead
        }


        /// <summary>
        /// Track point settings
        /// </summary>
        /// <param name="index"></param>
        /// <param name="trackPoint"></param>
        /// <param name="trackSwoitch"></param>
        /// <returns></returns>
        // POST api/<SoftataController>
        [Route("SetSWitch")]
        [HttpPost]
        public IActionResult SetSWitch(int index, TrackPoint trackPoint, TrackSwitch trackSwoitch)
        {
            string result = ""; // SoftataLib.SendMessage((Commands)msgOrDeviceType, (byte)pin, (byte)state, expect, (byte)other ,Data);
            if(result != "Reset")
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [Route("SetLeftSwitch")]
        [HttpPost]
        public IActionResult SetLeftSwitch(int index, TrackSwitch state)
        {
            List<TrackSwitch> SwitchStates = new List<TrackSwitch> { TrackSwitch.straighAhead, TrackSwitch.left };

            if (SwitchStates.Contains(state))
            {
                TrackPoint point = TrackPoint.straightY;
                return SetSWitch(index, point, state);
            }
            else
            {
                return BadRequest("RightSwitch");
            }
        }

        [Route("SetRightSwitch")]
        [HttpPost]
        public IActionResult SetRightSwitch(int index, TrackSwitch state)
        {
            List<TrackSwitch> SwitchStates = new List<TrackSwitch> { TrackSwitch.straighAhead, TrackSwitch.right };

            if (SwitchStates.Contains(state))
            {
                TrackPoint point = TrackPoint.straightY;
                return SetSWitch(index, point, state);
            }
            else
            {
                return BadRequest("RightSwitch");
            }
        }


        [Route("SetYSwitch")]
        [HttpPost]
        public IActionResult SetYSwitch(int index, TrackSwitch state)
        {
            List<TrackSwitch> SwitchStates = new List<TrackSwitch> { TrackSwitch.left, TrackSwitch.right };

            if (SwitchStates.Contains(state))
            {
                TrackPoint point = TrackPoint.straightY;
                return SetSWitch(index, point, state);
            }
            else
            {
                return BadRequest("YSwitch");
            }
        }
    }
}
