using Microsoft.AspNetCore.Components;
using System;
using System.Globalization;
using Softata.Enums;
using Softata;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ComponentModel;
using ConsoleTextFormat;
using Softata.ActionCommands;


namespace BlazorSoftata2
{
    //public class Selection
    //{
    //    public int Index { get; set; }

    //    public int Order { get; set; }

    //    public string Item { get; set; }

    //    public Selection? Next { get; set; } = null;


    //    public Selection()
    //    {
    //        Index = 0;
    //        Item = "";
    //        Order = 0;
    //    }

    //    public Selection(int index)
    //    {
    //        Index = index;
    //        Item = "";
    //        Order = index;
    //    }

    //    public Selection(int index, string item)
    //    {
    //        Index = index;
    //        Item = item;
    //        Order = index;
    //    }

    //    public Selection(int index, string item, int order)
    //    {
    //        Index = index;
    //        Item = item;
    //        Order = order;
    //    }

    //    public Selection(int index, List<string> items)
    //    {
    //        Index = index;
    //        Item = items[index];
    //        Order = index;
    //    }

    //    public Selection(int index, List<string> items, int order)
    //    {
    //        Index = index;
    //        Item = items[index];
    //        Order = order;
    //    }

    //    public void Dispose()
    //    {
    //        if(Next != null) Next.Dispose();
    //    }
    //}
    public class SelectedDeviceLoopVars : IDisposable
    {
        public Tuple<byte, byte, byte> rgb { get; set; } = new Tuple<byte, byte, byte>(0x40, 0, 0);
        public byte imisc { get; set; } = 0xff;
        public byte misc_led { get; set; } = 0;
        public byte misc_bglevel { get; set; } = 0;
        public byte misc_brightness { get; set; } = 1;
        public byte misc_num { get; set; } = 0;
        public Tuple<int, int>? actuatorRange { get; set; } = null;

        public int Value { get; set; } = 0;

        public bool foundRange { get; set; } = false;
        // bool isRelay { get; set; } = false;
        //public bool isQuadRelay { get; set; } = false;
        public byte relay_bit_no { get; set; } = 0;
        public void Dispose()
        {

        }

    }

    public class CSVList
    {
        private string csv = "";
        private List<string> list = new List<string>();
        public event Action? OnChange = null;

        public CSVList( ) { }

        public string CSV
        {
       
            get => csv;
            set
            {
                int count = value.Count(c => c == ':');
                csv = value;
                if (count > 0)
                {
                    string[] temp = value.Split(':');
                    if (temp.Length > 1)
                    {
                        csv = temp[1];
                    }
                }
                list = csv.Split(',').ToList();
                NotifyStateChanged();
            }
        }

        public List<string> List { get => list; }

        private void NotifyStateChanged()
        {
            try
            {
                OnChange?.Invoke();
            }
            catch (Exception ex) { }
        }
    }

    //[TypeConverter(typeof(AppStateConverter))]
    public class AppState
    {
        public event Action? OnChange;

        private SoftataLib _softataLib;

        private bool _loaded = false;

        // See Softata.Enums: ActuatorCapabilities and DeviceCapabilities
        // This is an or-ing of capabilities. Can have multiple capabilities
        public int Capabilities { get; set; } //Used by Actuators and DeviceInputs.

        public CommandsPortal CommandsPortal { get; set; }

        public bool Loaded
        {
            get => _loaded;
            set
            {
                _loaded = value;
                NotifyStateChanged();          
            }
        }

     


        public int Port { get => port; set { port = value; NotifyStateChanged(); } }

        public string IpaddressStr { get => ipaddressStr; set { ipaddressStr = value; NotifyStateChanged(); } }

        public SoftataLib softataLib
        {
            get => _softataLib;
            set
            {
                _softataLib = value;
                NotifyStateChanged();
            }
        }


        public AppState()
        {
            softataLib = new SoftataLib();
            DeviceTypes.OnChange += NotifyStateChanged;
            Devices.OnChange += NotifyStateChanged;
            GenericCommands.OnChange += NotifyStateChanged;
            Pinouts.OnChange += NotifyStateChanged;

            targetCommand = new Selection();
            targetDevicePin = new Selection();
            targetDevicePinout = new Selection();
            targetDevice = new Selection();
            targetDeviceType = new Selection();
            // For actuators
            Num_Bits = new Selection(4, "");


            selectedDeviceLoopVars = new SelectedDeviceLoopVars();

            port = 4242;
            ipaddressStr = "192.168.0.12";
            connected = false;

          
   
        }

        private SelectedDeviceLoopVars _selectedDeviceLoopVars;
        public SelectedDeviceLoopVars selectedDeviceLoopVars
        {
            get => _selectedDeviceLoopVars;
            set
            {
                _selectedDeviceLoopVars = value;
                NotifyStateChanged();
            }
        }

        public bool Connected { get => connected; set { connected = value; NotifyStateChanged(); } }
        public bool Quit { get => quit; set  { quit = value; NotifyStateChanged(); } }
        public bool Back { get => back; set  { back = value; NotifyStateChanged(); } }

        public CSVList DeviceTypes = new CSVList();
        public CSVList Devices = new CSVList();
        public CSVList GenericCommands = new CSVList();
        public CSVList Pinouts = new CSVList();

        public List<string>? DisplayMiscCmds { get; set; } = null;



        public Dictionary<int, string> UseGenericCommands = new Dictionary<int, string>();

        public List<string> Stringlist = new List<string>();

        //public string CommandsCSV { get => commandsCSV; set  { commandsCSV = value; NotifyStateChanged(); } }

        public byte linkedListNo { get; set; } = 0xff;

        public int targetdevicetypeindex { get; set; }
        public Selection TargetDeviceType { get => targetDeviceType;  set { targetDeviceType = value;  NotifyStateChanged(); } }
        public Selection TargetDevice { get => targetDevice;  set { targetDevice = value;   NotifyStateChanged(); } }

        public Selection TargetDevicePinout { get => targetDevicePinout; set { targetDevicePinout = value;  NotifyStateChanged(); } }
        public Selection TargetDevicePin { get => targetDevicePin; set { targetDevicePin = value; NotifyStateChanged(); } }

        public Selection TargetCommand { get => targetCommand;  set { targetCommand = value; NotifyStateChanged(); } }

        /////////////////////////////////////////////////////////
        public List<string>? sensorProperties = null;

        public Selection Num_Bits = new Selection(4, "");

        public int StartBit { get; set; } = 0;
        public int EndBit { get { return Num_Bits.Index - 1; } }

        private int _Max_Num_Bits = 4;
        public int Max_Num_Bits { get => max_Num_Bits; set { max_Num_Bits = value; Num_Bits.Index = value; Num_Bits.Item = $"Num Bits = {value}"; MaxVal =  (1 << Num_Bits.Index) -1; } }

        public int MaxVal { get => maxVal; set => maxVal = value; }
        /////////////////////////////////////////////////////////

        private bool connected = false;
        private bool quit = false;
        private bool back = false;


        private Selection targetDeviceType = new Selection();
        private Selection targetDevice = new Selection();
        private Selection targetDevicePinout = new Selection();
        private Selection targetDevicePin = new Selection();
        private Selection targetCommand = new Selection();
        private int port = 4242;
        private string ipaddressStr = "192.168.0.12";
        private int max_Num_Bits = 4;
        private int maxVal = 0;

        /////////////////////////////////////////////////////////

        private void NotifyStateChanged()
        {
            try
            {
                OnChange?.Invoke();
            } catch (Exception ex) { }
        }


    }

    /*
    public class AppStateConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string stringValue)
            {
                // Implement your conversion logic here
                return new AppState(); // Example
            }
            return base.ConvertFrom(context, culture, value);
        }
    }*/
}
