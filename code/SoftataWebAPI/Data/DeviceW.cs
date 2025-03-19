using SoftataWebAPI.Controllers;
using System.Collections.Generic;
using System.Linq;
using static SoftataWebAPI.Controllers.SoftataGenCmdsStr;

namespace SoftataWebAPI.Data
{
    /// <summary>
    /// <c>DeviceType</c> enum
    /// </summary>
    public enum DeviceType { sensor, display, actuator, serial, other, inputdevice }

    public enum CommonGenericCmds  {getCmds, getDevices, getPins, setupDefault, setupGeneral}
    
    
    /// <summary>
    /// Device object
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Device Type enum
        /// </summary>
        public DeviceType DeviceType { get; set; }
        /// <summary>
        /// Device Id
        /// </summary>
        public int DeviceId { get; set; }
    }

    public class Device2Lookup
    {
        internal bool Kontains(string a, string b)
        {
            return a.ToLower().Contains(b.ToLower()) || b.ToLower().Contains(a.ToLower());
        }
        public string Name { get; set; }

        /// <summary>
        /// <c>Device determined by Name</c> searching through TargetDevices.
        /// <br/>
        /// <c>Name</c> can be contained in an inner dictionary value  
        /// ... or an inner dictionary value can be contained in the <c>Name</c>.
        /// <br/>
        /// Searches through all devices of all device types.
        /// <br/>
        /// Get back the first device found, returning its <c>index</c> within its <c>DeviceType Device sublist</c>
        /// ... and the index of the <c>DeviceType</c>.
        /// <br/>
        /// Use <c>FirstorDefault</c> if multiple devices can be found.
        /// <br/>
        /// If not found <c>otherdevice</c> return.
        /// <br/>
        /// Nb: TargetDevices is <br/>
        /// <code>
        ///  Dictionary&lt;int,Dictionary&lt;int,string&gt;&gt;
        /// </code>
        /// </summary>
        internal Device Device
        {
            get
            {
                var result = Info.TargetDevices
                .SelectMany(outerDeviceTypekvp => outerDeviceTypekvp.Value
                    .Where(innerDevice => Kontains(innerDevice.Value,Name))
                    .Select(innerDevice => new { DeviceTypeIndex = outerDeviceTypekvp.Key, DeviceIndex = innerDevice.Key }))
                .FirstOrDefault();

                if (result == null)
                {
                    //Default device. other can be used to indicte not found
                    var dev = new Device
                    {
                        DeviceId = 0,
                        DeviceType = DeviceType.other
                    };
                    return dev;
                }
                else
                {
                    var dev = new Device
                    {
                        DeviceId = result.DeviceIndex,
                        DeviceType = (DeviceType)result.DeviceTypeIndex
                    };
                    return dev;
                }
            }
        }
    }

    /// <summary>
    /// Device Type enum and Instace Device Type Linked List Index
    /// </summary>
    public class DeviceInstance
    {
        /// <summary>
        /// Device Type enum
        /// cref="DeviceType"
        /// </summary>
        public DeviceType DeviceType { get; set; }
        public int ListLinkId { get; set; }
    }

    /// <summary>
    /// Generic Command object
    /// </summary>
    public class GenericCmd
    {
        /// <summary>
        /// Device Type enum
        /// cref="DeviceType"
        /// </summary>
        public DeviceType DeviceTypeId { get; set; }
        public int CmdId { get; set; }
    }



    /// <summary>
    /// <c>Generic Command object</c>
    /// Given DeviceType and a GenericCmd as a string
    /// Generate the CommandId that can be sent to <c>Arduino.Softata</c>
    /// </summary>
    public class GenericCmdLookup
    {
        internal bool Kontains(string a, string b)
        {
            return a.Contains(b) || b.Contains(a);
        }


        /// <summary>
        /// Device Type enum
        /// cref="DeviceType"
        /// </summary>
        public DeviceType DeviceType { get; set; }

        /// <summary>
        /// Generic Cmd as string
        /// </summary>
        public required string GenericCmdStr { get; set; }

        /// <summary>
        /// The generated CommandId that can be sent to <c>Arduino.Softata</c>
        /// </summary>
        internal byte CmdId
        {
            get
            {
                var result = Info.GenericCmds[(int)DeviceType].FirstOrDefault(entry => Kontains(entry.Key, GenericCmdStr));
                if (!result.Equals(default(KeyValuePair<string, int>)))
                {
                    return (byte)result.Value;
                }
                return 0; // Default value if no match is found
            }
        }
    }

    /// <summary>
    /// <c>Common Generic Command object</c>
    /// Given DeviceType and a Command Generic Command that all device types support
    /// Such as 
    /// <code>
    /// getCmds, getDevices, getPins, setupDefault, setupGeneral
    /// </code>
    /// Generate the CommandId that can be sent for that <c>DeviceType</c> to <c>Arduino.Softata</c>
    /// </summary>
    public class CommonGenericCmd
    {
        /// <summary>
        /// Device Type enum
        /// cref="DeviceType"
        /// </summary>
        public DeviceType DeviceTypeId { get; set; }
        /// <summary>
        /// Generic Command enum
        /// cref="CommonGenericCmds"
        /// </summary>
        public CommonGenericCmds CommonGenericCmdId { get; set; }
        /// <summary>
        /// Generated Generic Command Id that can be sent to <c>Arduino.Softata</c>
        /// </summary>
        internal byte CmdId
        {
            get
            {
                return (byte)((int)DeviceTypeId + (int)CommonGenericCmdId);
            }
        }
    }

    /// <summary>
    /// Display Generic Command object
    /// </summary>
    public class DisplayMiscCmd
    {
        /// <summary>
        /// Device Type enum
        /// cref="DeviceType"
        /// Must be <c>DeviceType.display</c>
        /// </summary>
        public DeviceType DeviceTypeId { get => DeviceType.display; }
        public int CmdId { get => 14; }
        public int MiscCmdId
        {
            get; set;
        }
    }
}