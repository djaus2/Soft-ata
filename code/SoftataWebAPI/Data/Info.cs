using System.Collections.Specialized;
using System;
using System.Collections.Generic;

namespace SoftataWebAPI.Data
{
    /// <summary>
    /// Info class<br/>
    /// A centralized repository for key data<br/>
    /// Namely the TargetDeviceTypes, TargetDevices and GenericCmds <br/>
    /// Read from the database: cref="SoftataWebAPI.Data.Db.Sqlite.ReadDb()"<br/>
    /// ... or from Pico W - Softata sketch cref="SoftataWebAPI.Data.SharedService.SoftataGetDatafrmPico()"
    /// </summary>
    public static class Info
    {
        public static Softata.SoftataLib SoftataLib { get; set; } = new Softata.SoftataLib();

        //public static Softata.SoftataLib.CommandsPortal commandsPortal {get;set;}

        public static Dictionary<int, string> TargetDeviceTypes { get; set; }

        public static Dictionary<int, Dictionary<string, int>> GenericCmds { get; set; }
        public static Dictionary<int, Dictionary<string, int>> UseGenericCmds { get; set; }
        public static Dictionary<int, Dictionary<int, string>> TargetDevices { get; set; }


        public static Queue<KeyValuePair<string, int>> NameValueQ { get; set; } = new Queue<KeyValuePair<string, int>>();
        //= new Queue<NameValueCollection>();

       public static void ClearQ()
        {
            NameValueQ.Clear();
        }
    }
}
