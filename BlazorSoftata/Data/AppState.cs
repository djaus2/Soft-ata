using System;
using System.Globalization;
using static Softata.SoftataLib;


namespace BlazorSoftata
{
    public class AppState
    {
        public event Action OnChange;

        public List<int> SerialSettings = new List<int>(2);
        private byte button = 0xff;
        private byte lED = 0xff;
        private byte relay = 0xff;
        private byte potentiometer = 0xff;
        private byte lightsensor = 0xff;
        private byte soundsensor = 0xff;
        private byte servo = 0xff;
        private bool readytoRun = false;
        private bool running = false;
        private byte i2C;
        private CommandType testType = CommandType.MaxType;
        private byte tx1;
        private byte tx2;
        private bool? usingGroveShield = null;


        private byte serialTx;
        private byte serialRx;

        public byte Button { get => button; set { button = value; NotifyStateChanged(); } }
        public byte LED { get => lED; set { lED = value; NotifyStateChanged(); } }
        public byte Relay { get => relay; set { relay = value; NotifyStateChanged(); } }

        public byte SoundSensor { get => soundsensor; set { soundsensor = value; NotifyStateChanged(); } }

        public byte LightSensor { get => lightsensor; set { lightsensor = value; NotifyStateChanged(); } }

        public byte Potentiometer { get => potentiometer; set { potentiometer = value; NotifyStateChanged(); } }
        public byte Servo { get => servo; set { servo = value; NotifyStateChanged(); } }
        public CommandType TestType { get => testType; set { testType = value; NotifyStateChanged(); } }
        public bool ReadytoRun { get => readytoRun; set { readytoRun = value; NotifyStateChanged(); } }
        public bool Running { get => running; set { running = value; NotifyStateChanged(); } }
        public bool DefaultSettings { get => defaultSettings; set { NotifyStateChanged(); } }
        public bool GetPins { get => servoGetPins; set { servoGetPins = value; NotifyStateChanged(); } }

        // List of tests as string names. Downloaded form Softata on Arduino
        public List<string> Actuators { get => actuators; set { actuators = value; NotifyStateChanged(); } }

        public List<string> Displays { get => displays; set { displays = value; NotifyStateChanged(); } }

        public int DisplayPeriod { get => neoperiod; set { neoperiod = value; NotifyStateChanged(); } }
        public int DisplayClearPeriod { get => neoclearperiod; set { neoclearperiod = value; NotifyStateChanged(); } }

        public List<string> Sensors { get => sensors; set { sensors = value; NotifyStateChanged(); } }


        // Test Categories
        public DeviceCategory TestCategory { get => testCategory; set { testCategory = value; NotifyStateChanged(); } }

        // Lists of tests as a CommandType
        public List<CommandType> LDigitals { get => lDigitals; set { lDigitals = value; NotifyStateChanged(); } }
        public List<CommandType> LAnalogs { get => lAnalogs; set { lAnalogs = value; NotifyStateChanged(); } }

        public List<CommandType> LActuators { get => lActuators; set { lActuators = value; NotifyStateChanged(); } }

        public List<CommandType> LSensors { get => lAnalogs; set { lAnalogs = value; NotifyStateChanged(); } }

        public List<CommandType> LDisplays { get => lActuators; set { lActuators = value; NotifyStateChanged(); } }

        public byte IDigital
        {
            get => idigital;
            set
            {
                if (value < LDigitals.Count())
                    GetPins = true;
                else
                    GetPins = false;
                idigital = value;
                NotifyStateChanged();
            }
        }

        public byte IAnalog
        {
            get => ianalog;
            set
            {
                if (value < LAnalogs.Count())
                    GetPins = true;
                else
                    GetPins = false;
                ianalog = value;
                NotifyStateChanged();
            }
        }
        public byte IActuator { get => iactuator;
            set {
                if (value < LActuators.Count())
                    GetPins = true;
                else
                    GetPins = false;
                iactuator = value;
                NotifyStateChanged();
            }
        }

        public DisplayDevice DisplayDevice
        {
            get {
                return (DisplayDevice)IDisplay;
            }
        }

        public byte IDisplay
        {
            get => idisplay;
            set
            {
                TestCategory = DeviceCategory.display;
                //if (Displays != null)
                //{
                    if (value < Displays.Count())
                        GetPins = true;
                    else
                        GetPins = false;
               // }
                idisplay = value;
                NotifyStateChanged();
            }
        }

        public byte ISensor
        {
            get => isensor;
            set
            {
                if (value < Sensors.Count())
                    GetPins = true;
                else
                    GetPins = false;
                isensor = value;
                NotifyStateChanged();
            }
        }
        public string AppVersion { get => appVersion; set { appVersion = value; NotifyStateChanged(); } }
        public string Connected { get => connected; set { connected = value; NotifyStateChanged(); } }
        public string DevicesCSV { get => devicesCSV; set { devicesCSV = value; NotifyStateChanged(); }
}
public byte I2C { get => i2C; set { if ((value < 0) || (value > 1)) return; i2C = value; NotifyStateChanged(); } }

        public byte Tx1 { get => tx1; set { tx1 = value; NotifyStateChanged(); } }
        public byte Rx1 => (byte)(tx1 + 1);

        //Seriall 1 or Serial 2
        public byte SerialTx { get => serialTx; set { serialTx = value; NotifyStateChanged(); } }
        public byte SerialRx { get => serialRx; set { serialRx = value; NotifyStateChanged(); } }

        public byte Tx2 { get => tx2; set { tx2 = value; NotifyStateChanged(); } }

        public byte Rx2 => (byte)(tx2 + 1);


        List<byte> GrovePicoShield_GPIO = new List<byte> { 16, 18, 20, 17, 19, 21 };
        static List<byte> PicoShield_GPIO_Full; // = Enumerable.Range(0, 26).Cast<byte>().ToList<byte>();
        private string strUsingGroveShield;
        private string appVersion = "";
        private string connected = "";
        private string devicesCSV = "";
        private List<string> actuators;
        private List<string> displays;
        private List<string> sensors;
        private byte idigital;
        private byte ianalog;
        private byte iactuator;
        private byte idisplay = 0xff;
        private byte isensor = 0xff;
        private int neoperiod = 2000;
        private int neoclearperiod = 100;
        private bool servoGetPins;
        private bool defaultSettings = false;
        private DeviceCategory testCategory;
        private List<CommandType> lDigitals;
        private List<CommandType> lAnalogs;
        private List<CommandType> lDisplays;
        private List<CommandType> lSensors;
        private List<CommandType> lActuators;

        public AppState()
        {
            PicoShield_GPIO_Full = new List<byte>();
            for( byte b=0;b<22;b++)
            {
                //Inbuilt LED already used
                if(b!=32)
                    PicoShield_GPIO_Full.Add(b);
            }
        }


        public string StrUsingGroveShield
        {
            get => strUsingGroveShield;
            set
            {
                strUsingGroveShield = value;
                if (bool.TryParse(value, out var resultBool))
                {
                    UsingGroveShield = resultBool;
;                   NotifyStateChanged();
                }

            }
        }

        public bool? UsingGroveShield { get => usingGroveShield; 
            set { 
                usingGroveShield = value; 
                NotifyStateChanged(); 
            }
}
        public List<byte> GPIOPins
        {
            get
            {
                if (UsingGroveShield == true)
                    return GrovePicoShield_GPIO;
                else if (UsingGroveShield == false)
                    return PicoShield_GPIO_Full;
                else return new List<byte> { };
            }
        }

        

        private void NotifyStateChanged()
        {
            try
            {
                OnChange?.Invoke();
            } catch (Exception ex) { }
        }


    }
}
