using Microsoft.EntityFrameworkCore;
using SoftataWebAPI.Controllers;
using System.Xml.Linq;

namespace SoftataWebAPI.Data.Db
{
    public class SqliteDb
    {
        /// <summary>
        /// Clear the Database
        /// </summary>
        /// <param name="context"></param>
        public static void ClearDb(SoftataContext context)
        {

            List<string> Tables = new List<string> { "Devices", "GenericCommands", "dTypes" };

            foreach (var TableName in Tables)
            { 
                context.Database.ExecuteSqlRaw($"DELETE FROM {TableName}");
                context.SaveChanges();
            }

            foreach (var TableName in Tables)
            {
                context.Database.ExecuteSqlRaw($"DELETE FROM sqlite_sequence WHERE name = '{TableName}'");
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Create the database and populate it with the dictionaries in Info
        /// Info data needs to be read from Pico W - Softata sketch first </summary>
        /// <param name="context">SoftataContext</param>
        public static void CreateDb(SoftataContext context)
        {
            context.Database.EnsureCreated();
            ClearDb(context);

            List<DType> DTypes = new List<DType>();
            foreach (var kvp in Info.TargetDeviceTypes)
            {
                var typ = new DType { Name = kvp.Value, SoftataId = kvp.Key };
                DTypes.Add(typ);
            }

            foreach (var dtype in DTypes)
            {
                if (!context.dTypes.Any(d => d.Id == dtype.Id))
                {
                    context.dTypes.Add(dtype);
                }
            }
            context.SaveChanges();

            List<DType> deviceTypes;
            deviceTypes = context.dTypes.ToList();

            List<Device> _Devices = new List<Device>();
            foreach (var kvp in Info.TargetDevices)
            {
                DType d = deviceTypes[kvp.Key];
                Dictionary<int, string> dv = kvp.Value;
                foreach (var qa in dv)
                {
                    var dev = new Device { DtypeId = d.Id, DType = d, Name = qa.Value, SoftataId = qa.Key };
                    _Devices.Add(dev);
                }
            }

            context.Devices.AddRange(_Devices);
            context.SaveChanges();

            deviceTypes = context.dTypes.ToList();

            List<GenericCommand> _Commands = new List<GenericCommand>();
            foreach (var kvp in Info.GenericCmds)
            {
                Dictionary<string, int> dv = kvp.Value;
                foreach (var qa in dv)
                {
                    DType d = deviceTypes[kvp.Key];
                    var dev = new GenericCommand { DtypeId = d.Id, DType = d, Name = qa.Key, SoftataId = qa.Value };
                    _Commands.Add(dev);
                }
            }

            context.GenericCommands.AddRange(_Commands);
            context.SaveChanges();

            deviceTypes = context.dTypes.ToList();
            
            Console.WriteLine("Dictionary saved to database.");
        }

        /// <summary>
        /// Read the Database and populate the dictionaries in Info
        /// </summary>
        /// <param name="context">SoftataContext</param>
        public static void ReadDb(SoftataContext context)
        {
            Info.TargetDeviceTypes = new Dictionary<int, string>();
            Info.TargetDevices = new Dictionary<int, Dictionary<int, string>>();
            Info.GenericCmds = new Dictionary<int, Dictionary<string, int>>();
            using (context)
            {
                var deviceTypes = context.dTypes.ToList();
                var devices = context.Devices.ToList();
                var commands = context.GenericCommands.ToList();

                foreach (var devType in deviceTypes)
                {
                    Info.TargetDeviceTypes.Add(devType.SoftataId, devType.Name);
                    Info.TargetDevices.Add(devType.SoftataId, new Dictionary<int, string>());
                    Info.GenericCmds.Add(devType.SoftataId, new Dictionary<string, int>());
                    foreach (var device in devices)
                    {
                        if (device.DtypeId == devType.Id)
                        {
                            Info.TargetDevices[devType.SoftataId].Add(device.SoftataId, device.Name);
                        }
                    }
                    if (commands != null)
                    {
                        Console.WriteLine("Commands");
                        foreach (var cmd in commands)
                        {
                            if (cmd.DtypeId == devType.Id)
                            {
                                Info.GenericCmds[devType.SoftataId].Add(cmd.Name, cmd.SoftataId);
                            }
                        }
                    }
                }
            }
        }
    }
}
