using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Net.Http;
using static SoftataInfo2DB.Program;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using static Softata.SoftataLib.Analog;
using System.Reflection.Metadata;

namespace SoftataInfo2DB
{
    public static class Info
    {
        public static Softata.SoftataLib SoftataLib { get; set; } = new Softata.SoftataLib();

        public static Dictionary<int, string> TargetDeviceTypes { get; set; }

        public static Dictionary<int, Dictionary<int, string>> GenericCmds { get; set; }

        public static Dictionary<int, Dictionary<int, string>> TargetDevices { get; set; }
    }
    internal class Program
    {
        private static void GetDeviceTypes()
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

        private static void GetDeviceTypesGenericCommands()
        {
            Info.GenericCmds = new Dictionary<int, Dictionary<int, string>>();
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
                                .ToDictionary(x => x.index, x => x.value);
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

        private static void GetDeviceTypesDevices()
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


        private static string stringAddDevice(int cmdTarget, int deviceIndex)
        {
            var device = Info.TargetDevices[cmdTarget][deviceIndex];
            var dictionary = Info.GenericCmds[cmdTarget];
            var values = dictionary
                .Where(kvp => kvp.Value.ToLower().Contains("setupdefault"))
                .Select(kvp => kvp.Key);
            byte subCmd = (byte)values.FirstOrDefault();
            string response = Info.SoftataLib.SendTargetCommand((byte)cmdTarget, 1, subCmd, (byte)deviceIndex);
            return response;
            //subCmd = GetGenericCmdIndex("setupdefault", GenericCommands);
            //result = softatalib.SendTargetCommand(cmdTarget, 1, subCmd, (byte)TargetDevice.Index);

        }
        public class DictionaryEntry
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public class DType
        {
            public DType()
            {

            }

            ILazyLoader _lazyLoader;

            private DType(ILazyLoader lazyLoader)
            {
                _lazyLoader = lazyLoader;
            }

            public int Id { get; set; }
            public int SoftataId { get; set; }
            public string Name { get; set; }

            private ICollection<Device> _Devices;
            public ICollection<Device> Devices
            {
                get => _lazyLoader.Load(this, ref _Devices) ?? _Devices;
                set => _Devices = value;
            }

            public ICollection<GenericCommand> _Commands;
            public ICollection<GenericCommand> Commands
            {
                get => _lazyLoader.Load(this, ref _Commands) ?? _Commands;
                set => _Commands = value;
            }
        }

        public class GenericCommand
        {
            public GenericCommand()
            {

            }
            public int Id { get; set; }
            public int SoftataId { get; set; }
            public int DtypeId { get; set; }
            public string Name { get; set; }

            private DType _DType;
            private ILazyLoader LazyLoader { get; set; }
            public DType DType
            {
                get => LazyLoader.Load(this, ref _DType);
                set => _DType = value;
            }


        }

        public class Device
        {
            public Device()
            {

            }
            public int Id { get; set; }
            public int SoftataId { get; set; }
            public int DtypeId { get; set; }
            public string Name { get; set; }

            private DType _DType;
            private ILazyLoader LazyLoader { get; set; }
            public DType DType
            {
                get => LazyLoader.Load(this, ref _DType);
                set => _DType = value;
            }
        }



        public class DictionaryContext : DbContext
        {
            public DbSet<DictionaryEntry> DictionaryEntries { get; set; }

            public DbSet<DType> dTypes { get; set; }
            public DbSet<Device> Devices { get; set; }
            public DbSet<GenericCommand> GenericCommands { get; set; }
            /*
             * public DbSet<Dictionary<int, string>> TargetDeviceTypes { get; set; }
            public DbSet<Dictionary<int,Dictionary<int,string>>> GenericCmds { get; set; }
            public DbSet<Dictionary<int, Dictionary<int, string>>> TargetDevices { get; set; }
            */
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlite("Data Source=dictionary.db");
            }
        }

        private static  bool _Connect(string ipAddress, int _port)
        {
            //SoftataLib SoftataLib = GetSoftataLib();
            bool result = Info.SoftataLib.Connect(ipAddress, _port);
            //SessionExtensions.Set(HttpContext.Session, "SoftataLib", SoftataLib);
            if (result)
            {
                return true;
            }
            else
                return false;
        }

        public static string Start(string ipAddress = "192.168.0.5", int _port = 4242)
        {
            //SoftataLib SoftataLib = GetSoftataLib();
            bool result = _Connect(ipAddress, _port);
            if (result)
            {

                string beginValue = Info.SoftataLib.SendMessageCmd("Begin");
                if (beginValue == "Ready")
                {
                    string OKresult = $"Connected to {ipAddress}:{_port} and Ready";
                    string value = Info.SoftataLib.SendMessageCmd("Version");
                    OKresult += $"\nSoftata Version:{value}";
                    ///////////////////////////////////
                    string cmdsOffset = Info.SoftataLib.SendMessageCmd("Soffset");
                    if (int.TryParse(cmdsOffset, out int _offset))
                    {
                        Info.SoftataLib.Offset = _offset; //Should be 0xf0
                        Console.WriteLine($"CommandsOffset: {_offset}");
                    }

                    GetDeviceTypes();

                    GetDeviceTypesGenericCommands();

                    GetDeviceTypesDevices();


                    ///////////////////////////////
                    return "Ok";
                }
                else
                {
                    Info.SoftataLib.Disconnect();
                    return "BadRequest";
                }
            }
            else
            {
                return "BadRequest";
            }
        }





        public static void MainS(string[] args)
        {
            using (var context = new DictionaryContext())
            {
                context.Database.EnsureDeleted();
                //context.Database.Migrate();
                context.Database.EnsureCreated();
            }
                var dictionary = new Dictionary<string, string>
        {
            { "hello", "world" },
            { "foo", "bar" }
        };
            Start();
            List<DType> DTypes = new List<DType>();
            foreach (var kvp in Info.TargetDeviceTypes)
            {
                var typ = new DType { Name = kvp.Value, SoftataId= kvp.Key };
                DTypes.Add(typ);
            }
            List<Device> _Devices = new List<Device>();
            foreach (var kvp in Info.TargetDevices)
            {
                Dictionary<int, string> dv = kvp.Value;
                foreach(var qa in dv)
                {
                    DType d = DTypes[kvp.Key];
                    var dev = new Device { DtypeId = kvp.Key+1, DType=d, Name = qa.Value, SoftataId = qa.Key };
                    _Devices.Add(dev);
                }
            }

            List<GenericCommand> _Commands = new List<GenericCommand>();
            foreach (var kvp in Info.GenericCmds)
            {
                Dictionary<int, string> dv = kvp.Value;
                foreach (var qa in dv)
                {
                    DType d = DTypes[kvp.Key];
                    var dev = new GenericCommand { DtypeId = kvp.Key + 1, DType = d, Name = qa.Value, SoftataId = qa.Key };
                    _Commands.Add(dev);
                }
            }

            using (var context = new DictionaryContext())
            {
                //context.Database.EnsureDeleted();
                //context.Database.Migrate();
                context.Database.EnsureCreated();
                //var entries = dictionary.Select(kv => new DictionaryEntry { Key = kv.Key, Value = kv.Value }).ToList();
                //context.DictionaryEntries.AddRange(entries);
                //context.SaveChanges();
                context.dTypes.AddRange(DTypes);
                context.SaveChanges();
                context.Devices.AddRange(_Devices);
                context.SaveChanges();
                context.GenericCommands.AddRange(_Commands);
                context.SaveChanges();
                /*
using (var context = new DictionaryContext())
{
    context.Database.EnsureCreated();
    var entries = dictionary.Select(kv => new DictionaryEntry { Key = kv.Key, Value = kv.Value }).ToList();
    context.DictionaryEntries.AddRange(entries);
    context.SaveChanges();
    
    context.dTypes.AddRange(DTypes);
    context.SaveChanges();
    
    context.Devices.AddRange(_Devices);
    context.SaveChanges();
}
                context.TargetDevices.Add(Info.TargetDevices);
                context.SaveChanges();
                */
            }

            Console.WriteLine("Dictionary saved to database.");
        }

        public static void Main(string[] args)
        {
            using (var context = new DictionaryContext())
            {
                var deviceTypes = context.dTypes.ToList();

                foreach (var devType in deviceTypes)
                {
                    Console.WriteLine($"{devType.Name}: {devType.Id} { devType.SoftataId}");
                    if (devType.Devices != null)
                    {
                        Console.WriteLine("Devices");
                        foreach (var device in devType.Devices)
                        {
                            Console.WriteLine($"{device.Name}: {device.Id} {device.SoftataId} {device.DtypeId} {device.DType.Name}");
                        }
                    }
                    if (devType.Commands != null)
                    {
                        Console.WriteLine("Commands");
                        foreach (var cmd in devType.Commands)
                        {
                            Console.WriteLine($"{cmd.Name}: {cmd.Id} {cmd.SoftataId} {cmd.DtypeId} {cmd.DType.Name}");
                        }
                    }
                }
                var dev = context.Devices.ToList();
                foreach(var d in dev)
                {
                    Console.WriteLine($"{d.Id} {d.Name} {d.DType.Name} {d.SoftataId} {d.DtypeId}");
                }
            }
        }
    }
}
