using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using SoftataWebAPI.Data;
using System.Net.Sockets;
using SoftataWebAPI.Data.Db;

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// Softata Generic Commands where command is specified as a string
    /// Can be the complete namr or unique part of the command name
    /// </summary>
    [Route("/Softata/Str")]
    [ApiController]
    public class SoftataGenCmdsStr : SoftataControllerCls
    {

        const int port = 4242;

        public SoftataGenCmdsStr(SoftataDbContext softataContext, ISoftataGenCmds sharedService)
         : base(softataContext, sharedService)
        {
        }


        /// <summary>
        /// Look up the Generic Command Index
        /// </summary>
        /// <param name="deviceType">Device Type</param>
        /// <param name="cmd">The command as a string, or part thereof</param>
        /// <returns>Cmd index</returns>
        private byte LookUpGenericCmd(DeviceType deviceType, string cmd)
        {
            byte cmdTarget = (byte)deviceType;
            var dictionary = Info.GenericCmds[cmdTarget];
            var values = dictionary
                .Where(kvp => ((kvp.Key.ToLower().Contains(cmd.ToLower()))) ||
                (cmd.ToLower().Contains(kvp.Key.ToLower())))
                .Select(kvp => kvp.Value);
            byte subCmd = (byte)values.FirstOrDefault();
            return subCmd;
        }

        /// <summary>
        /// Setup device using default setup.
        /// </summary>
        /// <param name="device" cref="Device">The DeviceType-Device</param>
        /// <returns>Instance index of device type Linked List Id</returns>
        [Route("SetupDeviceDefault")]
        [HttpPost]
        public IActionResult SetupDeviceDefault(Data.Device device)
        {
            System.Range Range = new Range(0,7);

            DeviceType devType = device.DeviceType;
            var dev = (byte)device.DeviceId;
            var dictionary = Info.GenericCmds[(int)devType];
            string cmd = "setupdefault";
            byte subCmd = LookUpGenericCmd(devType, cmd);
            string response = softatalib.SendTargetCommand((byte)(Offset + ((byte)devType)), Client,1, subCmd, dev);
            return Ok(response);
        }

        /// <summary>
        /// Setup named device using default setup.
        /// Nb:
        /// TargetDevices is Dictionary(int,Dictionary(int,string))
        /// </summary>
        /// <param name="device2Lookup" cref="Device2Lookup">The Device Name</param>
        /// <returns>Instance index of device type Linked List Id</returns>
        [Route("SetupDevDefaultLookup")]
        [HttpPost]
        public IActionResult SetupDeviceDefaultLookup(Device2Lookup device2Lookup)
        {
            Data.Device device = device2Lookup.Device;
            DeviceType devType = device.DeviceType;
            var dev = (byte)device.DeviceId;
            var dictionary = Info.GenericCmds[(int)devType];
            string cmd = "setupdefault";
            byte subCmd = LookUpGenericCmd(devType, cmd);
            string response = softatalib.SendTargetCommand((byte)(Offset + ((byte)devType)), Client, 1, subCmd, dev);
            return Ok(response);
        }

        /// <summary>
        /// Action a Generic Command on a Device Type Class
        /// With no parameters
        /// </summary>
        /// <param name="command2Lookup">Device type and Common Generic Command</param>
        /// <returns>Ok(Command result)</returns>
        [Route("ActionCommonDevicesCmdNoParamsLookup")]
        [HttpPost]
        public IActionResult ActionCommonDevicesCmdNoParamsLookup(CommonGenericCmd command2Lookup)
        {
            byte subCmd = command2Lookup.CmdId;
            int devType = (byte)command2Lookup.DeviceTypeId;
            string response = softatalib.SendTargetCommand((byte)(Offset + ((byte)devType)), Client,1, subCmd);
            return Ok(response);
        }

        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With no parameters
        /// </summary>
        /// <param name="deviceInstance">Instatiated device linked list index</param>
        /// <param name="cmd">Command as string, or part thereof</param>
        /// <returns>Ok(Command result)</returns>
        [Route("ActionDeviceCmdNoParams")]
        [HttpPost]
        public IActionResult ActionDeviceCmdNoParams(DeviceInstance deviceInstance, string cmd)
        {
            byte subCmd = LookUpGenericCmd(deviceInstance.DeviceType, cmd);
            string result =  sharedService.ActionDeviceCmdwithByteArrayParams(Offset + (int)deviceInstance.DeviceType, HttpContext, Client, deviceInstance.ListLinkId, subCmd);
            return Ok(result);
        }

        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With one byte parameter
        /// </summary>
        /// <param name="deviceInstance">Instatiated device linked list index</param>
        /// <param name="cmd">Command as a string, or part thereof</param>
        /// <param name="param">Byte parameter</param>
        /// <returns>Ok(Command result)</returns>
        [Route("ActionDeviceCmdwithByteParam")]
        [HttpPost]
        public IActionResult ActionDeviceCmdwithByteParam(DeviceInstance deviceInstance, string cmd, byte param)
        {
            byte subCmd = LookUpGenericCmd(deviceInstance.DeviceType, cmd);
            string result = sharedService.ActionDeviceCmdwithByteParam(Offset + (int)deviceInstance.DeviceType, HttpContext, Client, deviceInstance.ListLinkId, subCmd, param);
            return Ok(result);
        }


        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With string parameter plus
        /// plus optional CSV byte list of parameters
        /// </summary>
        /// <param name="deviceInstance">Instatiated device linked list index</param>
        /// <param name="cmd">Command as a string, or part thereof</param>
        /// <param name="txt">String parameter</param>
        /// <param name="csv">CSV list of byte parameters</param>
        /// <returns>Ok(Command result)</returns>
        [Route("ActionDeviceCmdwithTextandCSVParams")]
        [HttpPost]
        public IActionResult ActionDeviceCmdwithTextandCSVParams(DeviceInstance deviceInstance, string cmd, string txt, string csv)
        {
            byte subCmd = LookUpGenericCmd(deviceInstance.DeviceType, cmd);
            byte[] txtbytes = Encoding.ASCII.GetBytes(txt);
            byte[] bytes = string.IsNullOrWhiteSpace(csv) ? new byte[0] : csv.Split(',').Select(byte.Parse).ToArray();
            if (bytes.Length != 0)
            {
                bytes = (byte[])bytes.Concat(txtbytes);
            }
            else
                bytes = txtbytes;
            string result = sharedService.ActionDeviceCmdwithByteArrayParams(Offset + (int)deviceInstance.DeviceType, HttpContext, Client, deviceInstance.ListLinkId, subCmd, bytes);
            return Ok(result);
        }


        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With CSV list of byte parameters
        /// </summary>
        /// <param name="deviceInstance">Instatiated device linked list index</param>
        /// <param name="cmd">Command as a string, or part thereof</param>
        /// <param name="csv">CSV list of byte parameters</param>
        /// <returns>Ok(Command result)</returns>
        [Route("ActionDeviceCmdwithCSVListParams")]
        [HttpPost]
        public IActionResult ActionDeviceCmdwithCSVListofParams(DeviceInstance deviceInstance, string cmd, string csv)
        {
            byte subCmd = LookUpGenericCmd(deviceInstance.DeviceType, cmd);
            byte[] bytes = string.IsNullOrWhiteSpace(csv) ? new byte[0] : csv.Split(',').Select(byte.Parse).ToArray();
            string result = sharedService.ActionDeviceCmdwithByteArrayParams(Offset + (int)deviceInstance.DeviceType, HttpContext, Client, deviceInstance.ListLinkId, subCmd, bytes);
            return Ok(result);
        }
        /*
         * This error happens because the action method ActionDeviceCmdwithByteArrayParamsx in the SoftataGenericStrCmds 
         * controller has more than one parameter that is inferred to be bound from the request body.
         * This might be happening because ASP.NET Core MVC only allows one parameter to be bound from the request body. 
         * In your action method, both DeviceInstance deviceInstance and byte[] paramz are inferred to be bound 
         * from the body, which is not allowed. To resolve this, you need to explicitly specify the binding source 
         * for one of the parameters using attributes like [FromQuery], [FromRoute], or [FromBody].
         */

        /// <summary>
        /// Action a Generic Command on a Device instance
        /// With byte array of parameters
        /// </summary>
        /// <param name="deviceInstance">Instatiated device linked list index</param>
        /// <param name="cmd">Command as a string, or part thereof</param>
        /// <param name="paramz">Byte array of parameters</param>
        /// <returns>Ok(Command result)</returns>
        [Route("ActionDeviceCmdwithByteArrayParams")]
        [HttpPost]
        public IActionResult ActionDeviceCmdwithByteArrayParams([FromBody] DeviceInstance deviceInstance, string cmd, [FromQuery] byte[] paramz)
        {
            byte subCmd = LookUpGenericCmd(deviceInstance.DeviceType, cmd);
            string result = sharedService.ActionDeviceCmdwithByteArrayParams(Offset + (int)deviceInstance.DeviceType, HttpContext, Client, deviceInstance.ListLinkId, subCmd, paramz);
            return Ok(result);
        }
        
        
    }
}
