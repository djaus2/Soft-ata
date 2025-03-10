using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Net.Http;
using static SoftataInfo2DB.Program;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using static Softata.SoftataLib.Analog;
using System.Reflection.Metadata;

namespace SoftataInfo2DB
{
    internal partial class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 0)
            { 
                CreateDb(args);
            }
            else
                ReadDb(args);
        }




        public static void CreateDb(string[] args)
        {
            bool create = true;
            if (args.Length != 0)
            {
                //if(args[0] == "create")
                    create = true;
            }
            using (var context = new SoftataContext())
            {
                if(create)
                    context.Database.EnsureDeleted();
                //context.Database.Migrate();
                context.Database.EnsureCreated();
            }

            Start();
            List<DType> DTypes = new List<DType>();
            foreach (var kvp in Info.TargetDeviceTypes)
            {
                var typ = new DType { Name = kvp.Value, SoftataId= kvp.Key };
                DTypes.Add(typ);
            }
            List<DType> deviceTypes;
            using (var context = new SoftataContext())
            {
                context.Database.EnsureCreated();
                foreach (var dtype in DTypes)
                {
                    if (!context.dTypes.Any(d => d.Id == dtype.Id))
                    {
                        context.dTypes.Add(dtype);
                    }
                }
                context.SaveChanges();
                deviceTypes = context.dTypes.ToList();
            


                List<Device> _Devices = new List<Device>();
                foreach (var kvp in Info.TargetDevices)
                {
                    DType d = deviceTypes[kvp.Key];
                    Dictionary<int, string> dv = kvp.Value;
                    foreach(var qa in dv)
                    {
                        var dev = new Device { DtypeId = d.Id, DType=d, Name = qa.Value, SoftataId = qa.Key };
                        _Devices.Add(dev);
                    }
                }

                
                context.Devices.AddRange(_Devices);
                context.SaveChanges();

                deviceTypes = context.dTypes.ToList();

                List<GenericCommand> _Commands = new List<GenericCommand>();
                foreach (var kvp in Info.GenericCmds)
                {
                    Dictionary<int, string> dv = kvp.Value;
                    foreach (var qa in dv)
                    {
                        DType d = deviceTypes[kvp.Key];
                        var dev = new GenericCommand { DtypeId = d.Id, DType = d, Name = qa.Value, SoftataId = qa.Key };
                        _Commands.Add(dev);
                    }
                }
         

                context.GenericCommands.AddRange(_Commands);
                context.SaveChanges();

                deviceTypes = context.dTypes.ToList();
            }
            Console.WriteLine("Dictionary saved to database.");
        }

        public static void ReadDb(string[] args)
        {
            Info.TargetDeviceTypes = new Dictionary<int, string>();
            Info.TargetDevices = new Dictionary<int, Dictionary<int, string>>();
            Info.GenericCmds = new Dictionary<int, Dictionary<int, string>>();
            using (var context = new SoftataContext())
            {
                var deviceTypes = context.dTypes.ToList();
                var devices = context.Devices.ToList();
                var commands = context.GenericCommands.ToList();

                foreach (var devType in deviceTypes)
                {
                    Info.TargetDevices.Add(devType.SoftataId, new Dictionary<int, string>());
                    foreach (var device in devices)
                    {
                        if (device.DtypeId == devType.Id)
                        {
                            Info.TargetDevices[devType.SoftataId].Add(device.SoftataId, device.Name);
                        }
                    }

                    Info.GenericCmds.Add(devType.SoftataId, new Dictionary<int, string>());
                    if (commands != null)
                    {
                        Console.WriteLine("Commands");
                        foreach (var cmd in commands)
                        {
                            if (cmd.DtypeId == devType.Id)
                            {
                                Info.GenericCmds[devType.SoftataId].Add(cmd.SoftataId, cmd.Name);
                            }
                        }
                    }
                }
            }
        }
    }
}
