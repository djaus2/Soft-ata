using System.ComponentModel;

namespace Softata.Enums
{
    public static class Factors
    {
        // Found that max LDR value from ADC equates to 61/100
        public const double LightSensorMax = 61.0; // /100
    }
    //Nb: https://stackoverflow.com/questions/13734746/combine-multiple-enums-into-master-enum-list
    //  ... First Answer used
    public enum AllDisplayMiscCommands : byte 
    {
        /* LCD1602MiscCmds */
            home, autoscroll, noautoscroll, blink, noblink,/* Add Here, */ LCD1602MiscCmds_MAX,
        /* NEOPIXELMiscCmds */
            setpixelcolor, setpixelcolorAll, setpixelcolorOdds, setpixelcolorEvens, setBrightness, /* Add Here, */NEOPIXELMiscCmds_MAX ,
        /* OLEDMiscCmds */
        drawCircle, drawFrame, /* Add Here, */ OLEDMiscCmds_MAX ,
        /*BARGRAPHMiscCmds */
        flow, flow2, setLed, clrLed, toggleLed, setLevel, exercise, /* Add Here, */ BARGRAPHMiscCmds_MAX 
    }

   

    enum LCD1602MiscCmds : byte { home = AllDisplayMiscCommands.home, autoscroll = AllDisplayMiscCommands.autoscroll, noautoscroll = AllDisplayMiscCommands.noautoscroll, blink = AllDisplayMiscCommands.blink, noblink = AllDisplayMiscCommands.noblink,/* Add Here, */ LCD1602MiscCmds_MAX = AllDisplayMiscCommands.LCD1602MiscCmds_MAX }
    enum NEOPIXELMiscCmds : byte { setpixelcolor = AllDisplayMiscCommands.setpixelcolor, setpixelcolorAll  = AllDisplayMiscCommands.setpixelcolorAll, setpixelcolorOdds  = AllDisplayMiscCommands.setpixelcolorOdds, setpixelcolorEvens = AllDisplayMiscCommands.setpixelcolorEvens, setBrightness = AllDisplayMiscCommands.setBrightness, /* Add Here, */ NEOPIXELMiscCmds_MAX = AllDisplayMiscCommands.NEOPIXELMiscCmds_MAX }
    public enum OLEDMiscCmds : byte { drawCircle = AllDisplayMiscCommands.drawCircle, drawFrame = AllDisplayMiscCommands.drawFrame, /* Add Here, */ OLEDMiscCmds_MAX = AllDisplayMiscCommands.OLEDMiscCmds_MAX }
    public enum BARGRAPHMiscCmds: byte { flow = AllDisplayMiscCommands.flow, flow2 = AllDisplayMiscCommands.flow2, setLed = AllDisplayMiscCommands.setLed, clrLed = AllDisplayMiscCommands.clrLed, toggleLed = AllDisplayMiscCommands.toggleLed, setLevel = AllDisplayMiscCommands.setLevel, exercise = AllDisplayMiscCommands.exercise, /* Add Here, */ BARGRAPHMiscCmds_MAX = AllDisplayMiscCommands.BARGRAPHMiscCmds_MAX }

    public static class DisplayMiscEnumFirstCmd
    {
        public static AllDisplayMiscCommands LCD1602MiscCmds = AllDisplayMiscCommands.home;
        public static AllDisplayMiscCommands NEOPIXELMiscCmds = AllDisplayMiscCommands.setpixelcolor;
        public static AllDisplayMiscCommands OLEDMiscCmds = AllDisplayMiscCommands.drawCircle;
        public static AllDisplayMiscCommands BARGRAPHMiscCmds = AllDisplayMiscCommands.flow;
    }

    public enum SensorDevice : byte { DHT11, BME280, UltrasonicRANGER,/* Add Here, */  Undefined = 0xFF }
    public enum DisplayDevice : byte { OLED096, LCD1602, NEOPIXEL, BARGRAPH, GBARGRAPH, /* Add Here, */ Undefined = 0xFF }
    public enum ActuatorDevice : byte { Servo, SIPO_74HC595, Relay,/* Add Here, */ Undefined = 0xFF }
    public enum SerialDevice : byte { Loopback, GPS, Undefined = 0xFF }

 


    /*public enum DisplayDevice : byte {
        [Description("OLED096")]
        OLED096,
        [Description("LCD1602")]
        LCD1602,
        [Description("NEOPIXEL")]
        NEOPIXEL,
        [Description("BARGRAPH")]
        BARGRAPH,
        [Description("GBARGRAPH") ]

        GBARGRAPH,
        // Add Here,
        [Description("Undefined")]
        Undefined = 0xFF 
    }*/

    public static class DeviceTypesLists
    {


        public static List<string> DisplayNames = Enum.GetNames(typeof(DisplayDevice)).ToList();

        public static List<string> SensorNames = Enum.GetNames(typeof(SensorDevice)).ToList();

        public static List<string> ActuatorNames = Enum.GetNames(typeof(ActuatorDevice)).ToList();

    }






    public enum Commands
    {
        //Digital_Button_and_LED IO
        pinMode = 0xD0,
        digitalWrite = 0xD1,
        digitalRead = 0xD2,
        digitalToggle = 0xD3,

        //Analog_Potentiometer_and_LED/PWM
        analogRead = 0xA2,
        analogSetResolution = 0xA3,

        analogWriteResolution = 0xB0,
        pwmWrite = 0xB1,

        //Serial
        serialSetup = 0xE0, // Setup Serial1/2
        serialGetChar = 0xE1, // Get a char
        serialGetString = 0xE2, // Get a string
        serialGetStringUntil = 0xE3, // Get a string until char
        serialWriteChar = 0xE4, // Write a char
        serialGetFloat = 0xE5, // Get Flost
        serialGetInt = 0xE6, // Get Int
        serialReadLine = 0xE7,

        groveSensor = 0xF0,
        groveDisplay = 0xF1,
        groveActuator = 0xF2,
        Undefined = 0xFF,
        
    }

    public enum ConsoleTestType : byte
    {
        Digital_Button_and_LED = 0,
        Analog_Potentiometer_and_LED = 1,
        PWM = 2,
        Servo = 3,
        Sensors = 4,
        Displays = 0x5,
        Loopback = 6,
        Analog_Potentiometer_Light_and_Sound = 0x7,
        USonicRange = 0x8,
        Potentiometer_and_Actuator = 0x9,
        GPS_Serial = 0xA,
        Test_OTA_Or_WDT = 0xB,
        Analog_Device_Raw = 0xC,
        MaxType = 0xD,       
        Undefined = 0xFF,
    }

    public enum BlazorTestType : byte
    {
        Digital_Button_and_LED = 0,
        Analog_Potentiometer_and__LED = 1,
        PWM = 2,
        Servo = 3,
        Sensors = 4,
        Displays = 0x5,
        Loopback = 6,
        Analog_Potentiometer_Light_and_Sound = 0x7,
        USonicRange = 0x8,
        Potentiometer_and_Actuator = 0x9,
        GPS_Serial = 0xA,
        Test_OTA_Or_WDT = 0xB,
        Potentiometer_Relay =0xC,
        Potentiometer_Actuator =0xD,
        Potentiometer_Servo = 0xE,
        MaxType = 0xF,
        Undefined = 0xFF,
    }

    public enum DeviceCategory : byte
    {
        digital = 0,
        analog = 0x1,
        sensor = 0x2,
        actuator = 0x3,
        display = 0x4,
        serial = 0x5,
        MaxType = 0x6,

        //communication,
        Undefined = 0xFF
    }

    enum BAUDRATE : byte
    {
        bd50, bd75, bd110, bd134, bd150, bd200, bd300, bd600, bd1200, bd1800, bd2400, bd4800, bd9600, bd19200, bd38400, bd57600, bd115200
    };

    enum BAURATERanges : byte
    {
        up_to_300,
        from_600_to_4800,
        from_9600
    }

}
