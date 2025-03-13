using SoftataWebAPI.Controllers;
using SoftataWebAPI.Data.Db;

namespace SoftataWebAPI.Data
{
    /// <summary>
    /// Interface to SoftataGenericCmds
    /// All the generic commands are sent to the device using the ActionDeviceCmdwithByteArrayParams method
    /// </summary>
    public interface ISoftataGenCmds
    {
        /// <summary>
        /// All the generic commands are sent to the device using this method
        /// </summary>
        /// <param name="ideviceType">Index of teh device type</param>
        /// <param name="linkedListNo">Linked list index of instatiated device, or 0xff if command is with the device class</param>
        /// <param name="subCmd">Index of the generic command with the device type subset</param>
        /// <param name="paramz">Nullable array of byte parameters</param>
        /// <returns>Reposnse from RPi Pico w</returns>
        string ActionDeviceCmdwithByteArrayParams(int ideviceType, int linkedListNo, int subCmd, byte[]? paramz = null);

        //////////////////////////////////////////////

        void GetDeviceTypesfrmPico();
        void GetDeviceTypesGenericCommandsfrmPico();
        void GetDeviceTypesDevicesfrmPico();
        /////////////////////////////
        void SoftataGetDatafrmPico();

        //////////////////////////////////////////////

        bool SoftataClearDb(SoftataContext context, int pin);
        bool SoftataCreateDb(SoftataContext context, int pin);
        bool ReadSoftataDataDb(SoftataContext context);

    }



    /// <summary>
    /// Service for SoftataGenericCmds
    /// All the generic commands are sent to the device using the ActionDeviceCmdwithByteArrayParams method
    /// </summary>
    public class SharedService : ISoftataGenCmds
    {
        /// <summary>
        /// All the generic commands are sent to the device using this method
        /// </summary>
        /// <param name="ideviceType">Index of teh device type</param>
        /// <param name="linkedListNo">Linked list index of instatiated device, or 0xff if command is with the device class</param>
        /// <param name="subCmd">Index of the generic command with the device type subset</param>
        /// <param name="paramz">Nullable array of byte parameters</param>
        /// <returns>Reposnse from RPi Pico w</returns>
        public string ActionDeviceCmdwithByteArrayParams(int ideviceType, int linkedListNo, int subCmd, byte[]? paramz = null)
        {
            string response = Info.SoftataLib.SendTargetCommand((byte)ideviceType, 1, (byte)subCmd, (byte)0xff, (byte)linkedListNo, paramz);
            return response;
        }

        public void GetDeviceTypesfrmPico()
        {
            Info.TargetDeviceTypes = new Dictionary<int, string>();
            string deviceTypesCSV = Info.SoftataLib.SendMessageCmd("Devices");
            string[] parts = deviceTypesCSV.Split(":");
            List<string> devicesTypes = parts[1].Split(",").ToList();
            var deviceTypelist =
                    devicesTypes.Select((value, index) => new { value, index })
                    .ToDictionary(x => x.index, x => x.value);
            foreach (var item in deviceTypelist)
            {
                Info.TargetDeviceTypes.Add(item.Key, item.Value);
            }
        }

        public void GetDeviceTypesGenericCommandsfrmPico()
        {
            Info.GenericCmds = new Dictionary<int, Dictionary<string, int>>();
            foreach (var dt in Info.TargetDeviceTypes)
            {
                try
                {
                    byte subcmd = 0;//getGenericCommands
                    string cmdsCSV = Info.SoftataLib.SendTargetCommand((byte)(dt.Key), 0, subcmd);
                    if (cmdsCSV.Contains(":"))
                    {
                        string[] parts = cmdsCSV.Split(":");
                        List<string> cmdsList = parts[1].Split(",").ToList();
                        var cmdlist =
                                cmdsList.Select((value, index) => new { value, index })
                                .ToDictionary(x => x.value, x => x.index);
                        Info.GenericCmds.Add(dt.Key, cmdlist);
                        Thread.Sleep(500);
                    }
                    else
                        continue;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        public void GetDeviceTypesDevicesfrmPico()
        {
            Info.TargetDevices = new Dictionary<int, Dictionary<int, string>>();
            Thread.Sleep(200);
            foreach (var dt in Info.TargetDeviceTypes)
            {
                try
                {
                    byte subCmd = 1;//getDevices
                    string DevicesCSV = Info.SoftataLib.SendTargetCommand((byte)(dt.Key), 0, subCmd);
                    if (DevicesCSV.Contains(":"))
                    {
                        string[] parts2 = DevicesCSV.Split(":");
                        List<string> devices = parts2[1].Split(",").ToList();
                        var devicelist =
                                devices.Select((value, index) => new { value, index })
                                .ToDictionary(x => x.index, x => x.value);
                        Info.TargetDevices.Add(dt.Key, devicelist);
                    }
                    else
                        continue;
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        public void SoftataGetDatafrmPico()
        {
            GetDeviceTypesfrmPico();
            GetDeviceTypesDevicesfrmPico();
            GetDeviceTypesGenericCommandsfrmPico();
        }

        public bool SoftataClearDb(SoftataContext context, int pin)
        {
            try
            {
                if (pin == 1370)
                {
                    var ctx = context;
                    SoftataWebAPI.Data.Db.SqliteDb.ClearDb(ctx);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SoftataCreateDb(SoftataContext context, int pin)
        {
            try
            {
                SoftataGetDatafrmPico();
                if (pin == 1370)
                {
                    var ctx = context;
                    SoftataWebAPI.Data.Db.SqliteDb.CreateDb(ctx);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool ReadSoftataDataDb(SoftataContext context)
        {
            try
            {
                var ctx = context;
                SoftataWebAPI.Data.Db.SqliteDb.ReadDb(ctx);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
