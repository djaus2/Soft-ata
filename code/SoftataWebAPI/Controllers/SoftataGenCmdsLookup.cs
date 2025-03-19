using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using SoftataWebAPI.Data;
using SoftataWebAPI.Data.Db;

namespace SoftataWebAPI.Controllers
{
    /// <summary>
    /// Softata Generic Commands where command is specified as a string
    /// Can be the complete namr or unique part of the command name
    /// </summary>
    [Route("/Softata/Lu")]
    [ApiController]
    public class SoftataGenCmdsLookup : SoftataControllerCls
    {
        public SoftataGenCmdsLookup(SoftataDbContext softataContext, ISoftataGenCmds sharedService)
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
        /// Get Generic Command Id from the Device Type and Command string
        /// </summary>
        /// <param name="command2Lookup" cref="CommonGenericCmd">Device type and Common Generic Command</param>
        /// <returns>Ok(Command result)</returns>
        [Route("LookupCommonGenericCommand")]
        [HttpPost]
        public IActionResult LookupCommonGenericCommand(CommonGenericCmd command2Lookup)
        {
            return Ok(command2Lookup.CmdId);
        }

        /// <summary>
        /// Get Generic Command Id from the DeviceType and Command string
        /// </summary>
        /// <param name="genericCmdLookup" cref="GenericCmdLookup">Device type and Common Generic Command</param>
        /// <returns>Ok(Command result)</returns>
        [Route("LookupGenericCommand")]
        [HttpPost]
        public IActionResult LookupGenericCommand(GenericCmdLookup genericCmdLookup)
        {
            var cmd = genericCmdLookup.CmdId;
            return Ok(cmd);
        }


        /// <summary>
        /// Get Generic Command Id from the DeviceType and Command string
        /// </summary>
        /// <param name="deviceType" cref="DeviceType">Device Type</param>
        /// <param name="cmdStr">Generic Command as a string</param>
        /// <returns>Ok(Command result)</returns>
        [Route("GetpGenericCommand")]
        [HttpPost]
        public IActionResult LookupGenericCommand(DeviceType deviceType, string cmdStr)
        {
            byte subCmd = LookUpGenericCmd(deviceType, cmdStr);
            return Ok(subCmd);
        }



    }
}
