﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using SoftataWebAPI.Data;

namespace SoftataWebAPI.Controllers 
{

    /// <summary>
    /// Softata Generic Commands where command is specified by index within the device type list of generic commands.
    /// Device type is specified DeviceType enum or DeviceInstance encapsulates this
    /// </summary>
    [Route("/SoftataGenCmds")]
    [ApiController]
    public class SoftataGenCmds(ISoftataGenCmds sharedService) : ControllerBase
    {
        private readonly ISoftataGenCmds _sharedService = sharedService;


        /// <summary>
        /// Action a Generic Command on a Device Type, 
        /// ... without specifying the actual device
        /// ... nor having an instance of the device type.
        /// </summary>
        /// <param name="deviceType">Index of Device Type</param>
        /// <param name="cmd">Generic Command index</param>
        /// <returns></returns>
        [Route("ActionDeviceTypeGenericCmdNoParams")]
        [HttpPost]
        public IActionResult ActionDeviceTypedGenericCmd(DeviceType deviceType, int cmd)
        {
            var dictionary = Info.GenericCmds[(byte)deviceType];
            byte subCmd =  (byte)cmd;
            string response = Info.SoftataLib.SendTargetCommand((byte)deviceType, 1, subCmd);
            return Ok(response);
        }


        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With no parameters
        /// </summary>
        /// <param name="deviceInstance">Instatiated device linked list index</param>
        /// <param name="subCmd">Index to  the command</param>
        /// <returns>Ok(Command result)</returns>
        [Route("ActionDeviceCmdNoParams")]
        [HttpPost]
        public IActionResult ActionDeviceCmdNoParams(DeviceInstance deviceInstance, int subCmd)
        {
            string result = _sharedService.ActionDeviceCmdwithByteArrayParams((int)deviceInstance.DeviceType, deviceInstance.ListLinkId, subCmd);
            return Ok(result);
        }


        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With one byte parameter
        /// </summary>
        /// <param name="deviceInstance">Instatiated device linked list index</param>
        /// <param name="subCmd">Index to  the command</param>
        /// <param name="param">Byte parameter</param>
        /// <returns>Ok(Command result)</returns>
        [Route("ActionDeviceCmdwithByteParam")]
        [HttpPost]
        public IActionResult ActionDeviceCmdwithByteParam(DeviceInstance deviceInstance, int subCmd, byte param)
        {
            string result = _sharedService.ActionDeviceCmdwithByteArrayParams((int)deviceInstance.DeviceType, deviceInstance.ListLinkId, subCmd);
            return Ok(result);
        }

        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With string parameter plus
        /// Optional byte array of parameters
        /// </summary>
        /// <param name="deviceInstance">Instatiated device linked list index</param>m>
        /// <param name="subCmd">Index to  the command</param>
        /// <param name="txt">String parameter</param>
        /// <param name="csv">CSV list of byte parameters</param>
        /// <returns>Ok(Command result)</returns>
        [Route("ActionDeviceCmdwithTextandCSVParams")]
        [HttpPost]
        public IActionResult ActionDeviceCmdwithTextandCSVParams(DeviceInstance deviceInstance, int subCmd, string txt, string csv)
        {
            byte[] txtbytes = Encoding.ASCII.GetBytes(txt);
            byte[] bytes = string.IsNullOrWhiteSpace(csv) ? new byte[0] : csv.Split(',').Select(byte.Parse).ToArray();
            if (bytes.Length != 0)
            {
                bytes = (byte[])bytes.Concat(txtbytes);
            }
            else
                bytes = txtbytes;
            string result = _sharedService.ActionDeviceCmdwithByteArrayParams((int)deviceInstance.DeviceType, deviceInstance.ListLinkId, subCmd, bytes);
            return Ok(result);
        }
    

        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With CSV list of byte parameters
        /// </summary>
        /// <param name="deviceInstance">Instatiated device linked list index</param>
        /// <param name="subCmd">Index to  the command</param>
        /// <param name="csv">CSV list of byte parameters</param>
        /// <returns>Ok(Command result)</returns>
        [Route("ActionDeviceCmdwithCSVListofParams")]
        [HttpPost]
        public IActionResult ActionDeviceCmdwithCSVListofParams(DeviceInstance deviceInstance, int subCmd, string csv)
        {
            byte[] bytes = string.IsNullOrWhiteSpace(csv) ? new byte[0] : csv.Split(',').Select(byte.Parse).ToArray();
            string result = _sharedService.ActionDeviceCmdwithByteArrayParams((int)deviceInstance.DeviceType, deviceInstance.ListLinkId, subCmd, bytes);
            return Ok(result);
        }

        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With one byte parameter
        /// </summary>
        /// <param name="deviceInstance">Instatiated device linked list index</param>
        /// <param name="subCmd">Index to  the command</param>
        /// <param name="paramz">Byte array of parameters</param>
        /// <returns>Ok(Command result)</returns>
        [Route("ActionDeviceCmdwithByteArrayParams")]
        [HttpPost]
        public IActionResult ActionDeviceCmdwithByteArrayParams([FromBody]DeviceInstance deviceInstance, int subCmd, [FromQuery]byte[]? paramz=null)
        {
            string result = _sharedService.ActionDeviceCmdwithByteArrayParams((int)deviceInstance.DeviceType, deviceInstance.ListLinkId, subCmd, paramz);
            return Ok(result);
        }
    }
}
