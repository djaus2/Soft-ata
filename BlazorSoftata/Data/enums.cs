namespace BlazorSoftata
{
    enum LCD1602MiscCmds : byte { home, autoscroll, noautoscroll, blink, noblink, LCD1602MiscCmds_MAX }
    enum NEOPIXELMiscCmds : byte { setpixelcolor, setpixelcolorAll, setpixelcolorOdds, setpixelcolorEvens, setBrightness, NEOPIXELMiscCmds_MAX }
    enum OLEDMiscCmds : byte { drawCircle, drawFrame, OLEDMiscCmds_MAX }
    public enum Sensors : byte { DHT11, BME280, URANGE, Undefined = 0xFF }
    public enum Displays : byte { OLED096, LCD1602, NEOPIXEL, Undefined = 0xFF }
    public enum Servos : byte { Servo }
   /* public enum CommandType : byte
    {
        Digital = 0,
        Analog = 1,
        PWM = 2,
        Servo = 3,
        Sensors = 4,
        Displays = 0x5,
        Serial = 6,
        PotLightSoundAnalog = 0x7,
        USonicRange = 0x8,
        PotRelay = 0x9,
        PotServo = 0xA,
        MaxType = 0xB,

        Undefined = 0xFF
    }*/

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
