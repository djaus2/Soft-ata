using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Softata.Enums;

namespace Softata
{

    public enum DISPLAY_COMMANDS { getCmdsCMD,  getDisplaysCMD, getpinsCMD, tbdCMD, setupDefaultCMD, setupCMD, clearCMD, backlightCMD, setCursorCMD, writestrngCMD, cursor_writestringCMD, home, dummyCMD, miscCMD, dispose };

    //#define DISPLAY_COMMANDS C(D_getCmdsCMD)C(D_getDisplaysCMD)C(D_dispose)C(D_getpinsCMD)C(D_selectDisplayCMD)C(D_setupDefaultCMD)C(D_setupCMD)C(D_miscCMD)C(d_clearCMD)C(d_backlightCMD)C(d_setCursorCMD)C(d_writestrngCMD)C(d_cursor_writestringCMD)C(d_home)C(d_dummyCMD)


    public enum GroveDisplayCmds { getListofGenericCMDs, getDisplays, getpins, setupDefault, setup, dispose ,
        getListofMiscCmds, clear, backlight, setCursor, writestrngCMD , cursor_writestringCMD , homeCMD , dummyCMD , misc  }; //=255 }
    public partial class SoftataLib
    {


        public class Display
        {
            internal SoftataLib softatalib;

            public Display(SoftataLib parent)
            {
                this.softatalib = parent;
            }

            //Links to info about the Grove Sensors 
            public Dictionary<DisplayDevice, string> Links = new Dictionary<DisplayDevice, string>
            {
                {DisplayDevice.OLED096,"https://wiki.seeedstudio.com/Grove-OLED_Display_0.96inch/" }
                ,{DisplayDevice.LCD1602,"https://wiki.seeedstudio.com/Grove-LCD_RGB_Backlight/" }
                ,{DisplayDevice.NEOPIXEL,"https://www.adafruit.com/category/184" } //Nb 8 Pixel version used
                ,{DisplayDevice.BARGRAPH,"" }
                ,{DisplayDevice.GBARGRAPH,"https://wiki.seeedstudio.com/Grove-LED_Bar/" }
                /* ,Add here */
            };


            public string GetCmds()
            {
                Tuple<int, string, bool>[] DisplayableStrings = new Tuple<int, string, bool>[0];
                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.getListofGenericCMDs, "Display Cmds:", 0);
                return result;
                /*result = result.Replace("Display Cmds", "");
                return result.Split(',;');
                /*if (!string.IsNullOrEmpty(result))
                {
                    String[] _cmds = result.Split(',');
                    for (int i = 0; i < _cmds.Length; i++)
                    {
                        DisplayableStrings.ad= new Tuple<int,string, bool>[_cmds.Length];
                        bool viewable = _cmds[i].Substring(0, 2) == "D_";
                        DisplayableStrings[i].Item1 = i;
                        DisplayableStrings[i].Item2 = _cmds[i];
                        DisplayableStrings[i].Item13= viewable;
                    }
                }
                return DisplayableStrings;*/
            }

            public string[] GetDisplays()
            {

                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.getDisplays, "OK:", 0);
                result = result.Replace("Displays:", "");
                if (!string.IsNullOrEmpty(result))
                    return result.Split(',');
                else
                    return new string[0];
            }


            public  string GetPins(byte displayType)
            {

                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.getpins, "OK:", displayType);
                return result;
            }

            // Default display setup (no data)
            public  int SetupDefault(byte displayType)
            {
                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.setupDefault, "OK:", displayType);
                if (int.TryParse(result, out int linkedListNo))
                    return linkedListNo;
                else
                    return -1;
            }

            public  int Dispose(byte displayType, byte linkedListNo)
            {
                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.dispose, "OK:");
                if (int.TryParse(result, out int res))
                    return res;
                else
                    return -1;
            }

            // Single data display setup
            public  int Setup(byte displayType, byte pin, byte datum)
            {
                List<byte> data = new List<byte> { datum };
                return Setup(displayType, pin, data);
            }

            // Multi data display setup
            public  int Setup(byte displayType, byte pin, List<byte> ldata)
            {
                var data2 = ldata.Prepend((byte)ldata.Count());
                byte[] data = data2.ToArray<byte>();
                string result = softatalib.SendMessage(Commands.groveDisplay, pin, (byte)GroveDisplayCmds.setup, "OK:", displayType, data);
                if (int.TryParse(result, out int linkedListNo))
                    return linkedListNo;
                else
                    return -1;
            }

            public  bool Clear(byte linkedListNo)
            {
                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.clear, "OK:", linkedListNo);
                if (!string.IsNullOrEmpty(result))
                    Console.WriteLine(result);
                if (true) //TBD
                    return true;
                else
                    return false;
            }
            public  bool Home(byte linkedListNo)
            {
                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.homeCMD, "OK:", linkedListNo);
                if (!string.IsNullOrEmpty(result))
                    Console.WriteLine(result);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public  bool Backlight(byte linkedListNo, byte property)
            {
                string result = softatalib.SendMessage(Commands.groveDisplay, property, (byte)GroveDisplayCmds.backlight, "OK:", linkedListNo);
                if (!string.IsNullOrEmpty(result))
                    Console.WriteLine(result);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public  bool SetCursor(byte linkedListNo, int x, int y)
            {
                byte[] data = new byte[] { 0x2, (byte)x, (byte)y };
                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.setCursor, "OK:", linkedListNo, data);
                if (!string.IsNullOrEmpty(result))
                    Console.WriteLine(result);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public bool BSetCursor(int cmd, byte linkedListNo, int x, int y)
            {
                byte[] data = new byte[] { 0x2, (byte)x, (byte)y };
                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)cmd, "OK:", linkedListNo, data);
                if (!string.IsNullOrEmpty(result))
                    Console.WriteLine(result);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public  bool WriteString(byte linkedListNo, byte[] dataIn)
            {
                byte[] bytes2 = dataIn.Append((byte)0).ToArray<byte>(); //Need to append 0
                byte[] data = bytes2.Prepend((byte)bytes2.Length).ToArray<byte>(); //Prepend string length +1
                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.writestrngCMD, "OK:", linkedListNo, data);
                if (!string.IsNullOrEmpty(result))
                    Console.WriteLine(result);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public  bool WriteString(byte linkedListNo, string msg)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(msg);
                byte[] bytes2 = bytes.Append((byte)0).ToArray<byte>(); //Need to append 0
                byte[] data = bytes2.Prepend((byte)bytes2.Length).ToArray<byte>(); //Prepend string length +1
                char[] cg = data.Select(b => (char)b).ToArray();
                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.writestrngCMD, "OK:", linkedListNo, data);
                if (!string.IsNullOrEmpty(result))
                    Console.WriteLine(result);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public  bool WriteString(byte linkedListNo, int x, int y, string msg)
            {
                byte[] cursor = new byte[] { (byte)x, (byte)y };
                byte[] bytes = Encoding.ASCII.GetBytes(msg);
                byte[] bytes1 = cursor.Concat(bytes).ToArray<byte>();
                byte[] bytes2 = bytes1.Append((byte)0).ToArray<byte>(); //Need to append 0
                byte[] data = bytes2.Prepend((byte)bytes2.Length).ToArray<byte>(); //Prepend string length +1
                char[] cg = data.Select(b => (char)b).ToArray();

                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.cursor_writestringCMD, "OK:", linkedListNo, data);
                if (!string.IsNullOrEmpty(result))
                    Console.WriteLine(result);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public  bool Misc(byte linkedListNo, byte cmd, byte[]? _data = null)
            {
                byte[] data = new byte[] { 0x4, cmd, 0, 0, 0 };
                if(_data != null)
                {
                    if (_data.Length > 0)
                    {
                        for (int i = 0; (i < _data.Length)&& (i<3); i++)
                            data[i + 2] = _data[i];
                    }
                }
                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", linkedListNo, data);
                if (!string.IsNullOrEmpty(result))
                    Console.WriteLine(result);
                return true;
            }

            public  bool MiscCmd(byte linkedListNo, DisplayDevice displayType, AllDisplayMiscCommands cmd, byte[]? _data = null)
            {
                byte command = 0;
                AllDisplayMiscCommands start = 0;
                AllDisplayMiscCommands max = 0;
                switch (displayType)
                {
                    case DisplayDevice.OLED096:
                        start =  DisplayMiscEnumFirstCmd.OLEDMiscCmds;
                        max = AllDisplayMiscCommands.OLEDMiscCmds_MAX;
                        break;
                    case DisplayDevice.LCD1602:
                        start =  DisplayMiscEnumFirstCmd.LCD1602MiscCmds;
                        max = AllDisplayMiscCommands.LCD1602MiscCmds_MAX;
                        break;
                    case DisplayDevice.NEOPIXEL:
                        start = DisplayMiscEnumFirstCmd.NEOPIXELMiscCmds;
                        max = AllDisplayMiscCommands.NEOPIXELMiscCmds_MAX;
                        break;
                    case DisplayDevice.BARGRAPH:
                        start = DisplayMiscEnumFirstCmd.BARGRAPHMiscCmds;
                        max = AllDisplayMiscCommands.BARGRAPHMiscCmds_MAX;
                        break;
                    case DisplayDevice.GBARGRAPH:
                        start = DisplayMiscEnumFirstCmd.BARGRAPHMiscCmds;
                        max = AllDisplayMiscCommands.BARGRAPHMiscCmds_MAX;
                        break;
                    default:
                        return false;
                }
                if ((cmd >= start) && (cmd < max))
                {
                    command = cmd - start;
                }
                else
                    return false;


                byte[] data = new byte[] { 0x4, command, 0, 0, 0 };
                if (_data != null)
                {
                    if (_data.Length > 0)
                    {
                        for (int i = 0; (i < _data.Length) && (i < 3); i++)
                            data[i + 2] = _data[i];
                    }
                }
                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", linkedListNo, data);
                if (!string.IsNullOrEmpty(result))
                    Console.WriteLine(result);
                return true;
            }


            public  string[] GetMiscCmds(byte displayType)
            {

                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.getListofMiscCmds, "OK:", displayType);
                result = result.Replace("Misc:", "");
                if (!string.IsNullOrEmpty(result))
                    return result.Split(',');
                else
                    return new string[0];
            }
            public  class Oled096
            {
                internal SoftataLib softatalib;

                public Oled096(SoftataLib parent)
                {
                    this.softatalib = parent;
                }
                enum OLEDMiscCmds { drawCircle, drawFrame, test, OLEDMiscCmds_MAX };

                public  bool misctest(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x4, (byte)OLEDMiscCmds.test, 0, 0, 0 };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

               
                public  bool drawCircle(byte displayLinkedListIndex, byte x = 60, byte y = 32, byte radius = 20)
                {
                    byte[] data = new byte[] { 0x4, (byte)OLEDMiscCmds.drawCircle, x, y, radius };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

               
                public  bool drawFrame(byte displayLinkedListIndex, byte x = 30, byte y = 5, byte w = 60, byte h = 55)
                {
                    byte[] data = new byte[] { 0x5, (byte)OLEDMiscCmds.drawFrame, x, y, w, h };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }
            }
            public  class Neopixel
            {
                internal SoftataLib softatalib;

                public Neopixel(SoftataLib parent)
                {
                    this.softatalib = parent;
                }
                // Misc commands
                enum NEOPIXELMiscCmds { setpixelcolor, setpixelcolAll, setpixelcolorOdds, setpixelcolorEvens, setBrightness, setNum,setPxl, NEOPIXELMiscCmds_MAX };

                public  byte MaxNumPixels = 8;
                public  bool Clear(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x4, (byte)NEOPIXELMiscCmds.setpixelcolAll, 0, 0, 0 };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public bool Misc_SetPixelColorOne(byte displayLinkedListIndex, byte red, byte green, byte blue, byte pixel)
                {
                    byte[] data = new byte[] { 0x5, (byte)NEOPIXELMiscCmds.setpixelcolor, red, green, blue, pixel };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public  bool Misc_SetAll(byte displayLinkedListIndex, byte red, byte green, byte blue)
                {
                    byte[] data = new byte[] { 0x4, (byte)NEOPIXELMiscCmds.setpixelcolAll, red, green, blue };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public  bool Misc_SetNum(byte displayLinkedListIndex, byte red, byte green, byte blue, byte n)
                {
                    byte[] data = new byte[] { 0x5, (byte)NEOPIXELMiscCmds.setNum, red, green, blue, n };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public bool Misc_SetPxl(byte displayLinkedListIndex, byte red, byte green, byte blue, byte n)
                {
                    byte[] data = new byte[] { 0x5, (byte)NEOPIXELMiscCmds.setPxl, red, green, blue, n };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public  bool Misc_SetOdds(byte displayLinkedListIndex, byte red, byte green, byte blue)
                {
                    byte[] data = new byte[] { 0x4, (byte)NEOPIXELMiscCmds.setpixelcolorOdds, red, green, blue };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }
 
                public  bool Misc_SetEvens(byte displayLinkedListIndex, byte red, byte green, byte blue)
                {
                    byte[] data = new byte[] { 0x4, (byte)NEOPIXELMiscCmds.setpixelcolorEvens, red, green, blue };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }
                public bool Misc_SetBrightness(byte displayLinkedListIndex, byte level)
                {
                    byte[] data = new byte[] { 0x2, (byte)NEOPIXELMiscCmds.setBrightness, level };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }
            }

            public  class LCD1602Display
            {
                internal SoftataLib softatalib;

                public LCD1602Display(SoftataLib parent)
                {
                    this.softatalib = parent;
                }
                // Misc commands
                enum LCD1602MiscCmds { autoscroll, noautoscroll, blink, noblink, LCD1602MiscCmds_MAX };


                public  bool AutoscrollOn(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)LCD1602MiscCmds.autoscroll };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public  bool NoAutoscroll(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)LCD1602MiscCmds.noautoscroll };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public  bool BlinkOn(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)LCD1602MiscCmds.blink };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public  bool NoBlink(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)LCD1602MiscCmds.noblink };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

            }

            public  class BARGRAPHDisplay
            {
                // Misc commands
                internal SoftataLib softatalib;



                
                /////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Display Class generic methods
                /////////////////////////////////////////////////////////////////////////////////////////////////////////
                
                public bool clear(byte displayLinkedListIndex)
                {
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.clear, "OK:", displayLinkedListIndex);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }


                /////////////////////////////////////////////////////////////////////////////////////////////////////////
                /// Misc commands
                /////////////////////////////////////////////////////////////////////////////////////////////////////////
              
                public BARGRAPHDisplay(SoftataLib parent)
                {
                    this.softatalib = parent;
                }

                public  bool FlowOdd(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)((byte)BARGRAPHMiscCmds.flow - (byte)DisplayMiscEnumFirstCmd.BARGRAPHMiscCmds) };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public bool FlowEven(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)((byte)BARGRAPHMiscCmds.flow2 - (byte)DisplayMiscEnumFirstCmd.BARGRAPHMiscCmds) };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public bool allOn(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)((byte)BARGRAPHMiscCmds.allOn - (byte)DisplayMiscEnumFirstCmd.BARGRAPHMiscCmds) };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                /// <summary>
                /// Note: For the following led is called as 0...9. mapping to Arduino Softata numbers the pins 1 ...10
                /// </summary>
                /// <param name="displayLinkedListIndex"></param>
                /// <param name="led"></param>
                /// <returns></returns>
                public bool set_LED(byte displayLinkedListIndex, byte led)
                {
                    byte[] data = new byte[] { 0x2, (byte)((byte)BARGRAPHMiscCmds.setLed - (byte)DisplayMiscEnumFirstCmd.BARGRAPHMiscCmds) ,(byte)(led +1) };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public bool clr_LED(byte displayLinkedListIndex, byte led)
                {
                    byte[] data = new byte[] { 0x2, (byte)((byte)BARGRAPHMiscCmds.clrLed - (byte)DisplayMiscEnumFirstCmd.BARGRAPHMiscCmds) , (byte)(led + 1) };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public bool toggle_LED(byte displayLinkedListIndex, byte led)
                {
                    byte[] data = new byte[] { 0x2, (byte)((byte)BARGRAPHMiscCmds.toggleLed - (byte)DisplayMiscEnumFirstCmd.BARGRAPHMiscCmds) , (byte)(led + 1) };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

                public bool setLevel(byte displayLinkedListIndex, byte level)
                {
                    byte[] data = new byte[] { 0x2, (byte)((byte)BARGRAPHMiscCmds.setLevel - (byte)DisplayMiscEnumFirstCmd.BARGRAPHMiscCmds) , level };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    if (!string.IsNullOrEmpty(result))
                        Console.WriteLine(result);
                    return true;
                }

            }
        }


    }
}
