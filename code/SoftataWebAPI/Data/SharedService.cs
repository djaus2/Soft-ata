using Softata;
using SoftataWebAPI.Controllers;
using SoftataWebAPI.Data.Db;
using System.Net.Sockets;

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
        string ActionDeviceCmdwithByteArrayParams(int ideviceType, HttpContext HttpContext, Socket? client, int linkedListNo, int subCmd, byte[]? paramz = null);

        string ActionDeviceCmdwithByteParam(int ideviceType, HttpContext HttpContext, Socket? client, int linkedListNo, int subCmd, byte param);


        //////////////////////////////////////////////

        void GetDeviceTypesfrmPico(HttpContext HttpContext, Socket? Client);
        void GetDeviceTypesGenericCommandsfrmPico(HttpContext HttpContext, Socket? Client);
        void GetDeviceTypesDevicesfrmPico(HttpContext HttpContext, Socket? Client);
        /////////////////////////////
        void SoftataGetDatafrmPico(HttpContext HttpContext,Socket? Client);

        //////////////////////////////////////////////

        bool SoftataClearDb(SoftataDbContext context, int pin);
        bool SoftataCreateDb(SoftataDbContext context, int pin, HttpContext HttpContext);
        bool ReadSoftataDataDb(SoftataDbContext context);

        Softata.SoftataLib GetSoftata(HttpContext HttpContext);
        Softata.SoftataLib.Sensor GetSensor(HttpContext HttpContext, int index);
        //bool UpdateSoftata(HttpContext HttpContext, Softata.SoftataLib softatlib);

        bool SetClient(HttpContext HttpContext, Socket client);

        void RemoveClient(HttpContext HttpContext);

        Socket? GetClient(HttpContext HttpContext);

    }



    /// <summary>
    /// Service for SoftataGenericCmds
    /// All the generic commands are sent to the device using the ActionDeviceCmdwithByteArrayParams method
    /// </summary>
    public class SharedService : ISoftataGenCmds
    {

        public Softata.SoftataLib GetSoftata(HttpContext HttpContext)
        {

            Softata.SoftataLib? softatlib = HttpContext.Session.Get<Softata.SoftataLib>("SOFT");
            if (softatlib == null)
            {
                softatlib = new Softata.SoftataLib();
                HttpContext.Session.Set<object>("SOFT", softatlib);
            }
            return softatlib;
        }

        /// <summary>
        /// Get sensor via Session
        /// </summary>
        /// <param name="HttpContext"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Softata.SoftataLib.Sensor GetSensor(HttpContext HttpContext,int index)
        {
            string sensorKey = "SENSOR_" + index.ToString();
            Softata.SoftataLib.Sensor? softatlib = HttpContext.Session.Get<Softata.SoftataLib.Sensor>(sensorKey);
            if (softatlib == null)
            {
                softatlib = new Softata.SoftataLib.Sensor(GetSoftata(HttpContext));
                HttpContext.Session.Set<object>(sensorKey, softatlib);
            }
            return softatlib;
        }

       


        public bool SetClient(HttpContext HttpContext, Socket client)
        {
            if (client == null)
                return false;
            HttpContext.Session.Set<Socket>("CLIENT", client);
            return true;
        }

        public Socket? GetClient(HttpContext HttpContext)
        {
            return HttpContext.Session.Get<Socket>("CLIENT");
        }

        public void RemoveClient(HttpContext HttpContext)
        {
            if(HttpContext.Session.Get<Socket>("CLIENT") != null)
                HttpContext.Session.Remove("CLIENT");
        }

        public Softata.SoftataLib softatalib { get; set; }
        /// <summary>
        /// All the generic commands are sent to the device using this method
        /// </summary>
        /// <param name="ideviceType">Index of teh device type</param>
        /// <param name="linkedListNo">Linked list index of instatiated device, or 0xff if command is with the device class</param>
        /// <param name="subCmd">Index of the generic command with the device type subset</param>
        /// <param name="paramz">Nullable array of byte parameters</param>
        /// <returns>Reposnse from RPi Pico w</returns>
        public string ActionDeviceCmdwithByteArrayParams(int ideviceType, HttpContext HttpContext, Socket? client, int linkedListNo, int subCmd, byte[]? paramz = null)
        {
            softatalib = GetSoftata(HttpContext);
            string response = softatalib.SendTargetCommand((byte)ideviceType, client, 1, (byte)subCmd, (byte)0xff, (byte)linkedListNo, paramz);
            return response;
        }

        public string ActionDeviceCmdwithByteParam(int ideviceType, HttpContext HttpContext, Socket? client, int linkedListNo, int subCmd, byte param)
        {
            softatalib = GetSoftata(HttpContext);
            string response = softatalib.SendTargetCommand((byte)ideviceType, client, param, (byte)subCmd, (byte)0xff, (byte)linkedListNo, null);
            return response;
        }

        public void GetDeviceTypesfrmPico(HttpContext HttpContext, Socket? Client)
        {
            softatalib = GetSoftata(HttpContext);
            Info.TargetDeviceTypes = new Dictionary<int, string>();
            string deviceTypesCSV = softatalib.SendMessageCmd("Devices", Client);
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

        public void GetDeviceTypesGenericCommandsfrmPico(HttpContext HttpContext, Socket? Client)
        {
            softatalib = GetSoftata(HttpContext);
            Info.GenericCmds = new Dictionary<int, Dictionary<string, int>>();
            foreach (var dt in Info.TargetDeviceTypes)
            {
                try
                {
                    byte subcmd = 0;//getGenericCommands
                    string cmdsCSV = softatalib.SendTargetCommand((byte)(dt.Key), Client,0, subcmd);
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

        public void GetDeviceTypesDevicesfrmPico(HttpContext HttpContext, Socket? Client)
        {
            softatalib = GetSoftata(HttpContext);
            Info.TargetDevices = new Dictionary<int, Dictionary<int, string>>();
            Thread.Sleep(200);
            foreach (var dt in Info.TargetDeviceTypes)
            {
                try
                {
                    byte subCmd = 1;//getDevices
                    string DevicesCSV = softatalib.SendTargetCommand((byte)(dt.Key), Client, 0, subCmd);
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

        public void SoftataGetDatafrmPico(HttpContext HttpContext, Socket? Client)
        {
            GetDeviceTypesfrmPico(HttpContext, Client);
            GetDeviceTypesDevicesfrmPico(HttpContext, Client);
            GetDeviceTypesGenericCommandsfrmPico( HttpContext, Client);
        }

        public bool SoftataClearDb(SoftataDbContext context, int pin)
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

        /// <summary>
        /// Create the database and populate it with the dictionaries in Info
        /// Requires SoftataGetDatafrmPico(HttpContext,Client0 to have been called first
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pin" ></param>
        /// <param name="HttpContext"></param>
        /// <returns></returns>
        public bool SoftataCreateDb(SoftataDbContext context, int pin, HttpContext HttpContext)
        {
            try
            {
                ///SoftataGetDatafrmPico(HttpContext,Client); //For simplicity assume has been done
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

        public bool ReadSoftataDataDb(SoftataDbContext context)
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
