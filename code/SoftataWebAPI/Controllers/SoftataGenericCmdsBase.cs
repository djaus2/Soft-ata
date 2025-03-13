using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SoftataWebAPI.Data;
using SoftataWebAPI.Data.Db;
using System.Text;

namespace SoftataWebAPI.Controllers 
{

    /// <summary>
    /// Softata Generic Commands where command is specified by index wrt device type
    /// </summary>
    [Route("/Softata/Indx")]
    [ApiController]
    public class SoftataGenericCmdsBase(ISoftataGenCmds sharedService) : ControllerBase
    {
        /// <summary>
        /// Service for SoftataGenericCmds that shares ActionDeviceCmdwithByteArrayParams service to other controllers
        /// </summary>
        private readonly ISoftataGenCmds _sharedService = sharedService;


        /// <summary>
        /// Action a Generic Command on a Device Type, 
        /// ... without specifying the actual device
        /// ... nor having an instance of the device type.
        /// </summary>
        /// <param name="ideviceType">Index of Device Type</param>
        /// <param name="cmd">Generic Command index</param>
        /// <returns></returns>
        [Route("DeviceTypeGenCmd")]
        [HttpPost]
        public IActionResult ActionDeviceTypeIndexedGenericCmd(int ideviceType, int cmd)
        {
            var dictionary = Info.GenericCmds[ideviceType];
            byte subCmd =  (byte)cmd;
            string response = Info.SoftataLib.SendTargetCommand((byte)ideviceType, 1, subCmd);
            return Ok(response);
        }


        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With no parameters
        /// </summary>
        /// <param name="ideviceType">Device type</param>
        /// <param name="linkedListNo">Index to device instance in linked list</param>
        /// <param name="subCmd">Index to  the command</param>
        /// <returns>Ok(Command result)</returns>
        [Route("DeviceGenCmdNoParams")]
        [HttpPost]
        public IActionResult ActionDeviceCmdNoParams(int ideviceType, int linkedListNo, int subCmd)
        {
            string result = _sharedService.ActionDeviceCmdwithByteArrayParams(ideviceType, linkedListNo, subCmd);
            return Ok(result);
        }


        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With one byte parameter
        /// </summary>
        /// <param name="ideviceType">Device type</param>
        /// <param name="linkedListNo">Index to device instance in linked list</param>
        /// <param name="subCmd">Index to  the command</param>
        /// <param name="param">Byte parameter</param>
        /// <returns>Ok(Command result)</returns>
        [Route("DevGenCmdByteParam")]
        [HttpPost]
        public IActionResult ActionDeviceCmdwithByteParam(int ideviceType, int linkedListNo, int subCmd, byte param)
        {
            string result = _sharedService.ActionDeviceCmdwithByteArrayParams(ideviceType, linkedListNo, subCmd, new byte[] {param});
            return Ok(result);
        }

        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With string parameter plus
        /// Optional byte array of parameters
        /// </summary>
        /// <param name="ideviceType">Device type</param>
        /// <param name="linkedListNo">Index to device instance in linked list</param>
        /// <param name="subCmd">Index to  the command</param>
        /// <param name="txt">String parameter</param>
        /// <param name="csv">CSV list of byte parameters</param>
        /// <returns>Ok(Command result)</returns>
        [Route("DevCmdTextCSVParams")]
        [HttpPost]
        public IActionResult ActionDeviceCmdwithTextandCSVParams(int ideviceType, int linkedListNo, int subCmd, string txt, string csv)
        {
            byte[] txtbytes = Encoding.ASCII.GetBytes(txt);
            byte[] bytes = string.IsNullOrWhiteSpace(csv) ? new byte[0] : csv.Split(',').Select(byte.Parse).ToArray();
            if (bytes.Length != 0)
            {
                bytes = bytes.Concat(txtbytes).ToArray();
            }
            else
                bytes = txtbytes;
            string result = _sharedService.ActionDeviceCmdwithByteArrayParams(ideviceType, linkedListNo, subCmd, bytes);
            return Ok(result);
        }

        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With CSV list of byte parameters
        /// </summary>
        /// <param name="ideviceType">Device type</param>
        /// <param name="linkedListNo">Index to device instance in linked list</param>
        /// <param name="subCmd">Index to  the command</param>
        /// <param name="csv">CSV list of byte parameters</param>
        /// <returns>Ok(Command result)</returns>
        [Route("DevCmdCSVParams")]
        [HttpPost]
        public IActionResult ActionDeviceCmdwithCSVListofParams(int ideviceType, int linkedListNo, int subCmd, string csv)
        {
            byte[] bytes = string.IsNullOrWhiteSpace(csv) ? new byte[0] : csv.Split(',').Select(byte.Parse).ToArray();
            string result = _sharedService.ActionDeviceCmdwithByteArrayParams(ideviceType, linkedListNo, subCmd, bytes);
            return Ok(result);
        }

        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With byte array of parameters
        /// </summary>
        /// <param name="ideviceType">Device type</param>
        /// <param name="linkedListNo">Index to device instance in linked list</param>
        /// <param name="subCmd">Index to  the command</param>
        /// <param name="paramz">Byte array of parameters</param>
        /// <returns>Ok(Command result)</returns>
        [Route("DevCmdByteArrayParams")]
        [HttpPost]
        public IActionResult ActionDeviceCmdwithByteArrayParams(int ideviceType, int linkedListNo, int subCmd, [FromQuery]byte[]? paramz=null)
        {
            string response = _sharedService.ActionDeviceCmdwithByteArrayParams( ideviceType, linkedListNo, subCmd,  paramz);
            return Ok(response);
        }
    }
}
