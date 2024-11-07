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
    public enum GroveDisplayCmds{getpins=0, getMiscCmds=1, setupDefault=2, setup=3, clear=4,backlight=5,setCursor=6, writestrngCMD=7,cursor_writestringCMD=8,homeCMD=9, misc=10,dispose=11, getDisplays=255 }
    public partial class SoftataLib
    {


        public  class Display
        {
            internal SoftataLib softatalib;

            public Display(SoftataLib parent)
            {
                this.softatalib = parent;
            }

            //Links to info about the Grove Sensors 
            public  Dictionary<DisplayDevice, string> Links = new Dictionary<DisplayDevice, string>
            {
                {DisplayDevice.OLED096,"https://wiki.seeedstudio.com/Grove-OLED_Display_0.96inch/" }
                ,{DisplayDevice.LCD1602,"https://wiki.seeedstudio.com/Grove-LCD_RGB_Backlight/" }
                ,{DisplayDevice.NEOPIXEL,"https://www.adafruit.com/category/184" } //Nb 8 Pixel version used
                ,{DisplayDevice.BARGRAPH,"" }
                ,{DisplayDevice.GBARGRAPH,"https://wiki.seeedstudio.com/Grove-LED_Bar/" }
                /* ,Add here */
            };
            public  string[] GetDisplays()
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
                if (true) //TBD
                    return true;
                else
                    return false;
            }
            public  bool Home(byte linkedListNo)
            {
                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.homeCMD, "OK:", linkedListNo);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public  bool Backlight(byte linkedListNo, byte property)
            {
                string result = softatalib.SendMessage(Commands.groveDisplay, property, (byte)GroveDisplayCmds.backlight, "OK:", linkedListNo);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public  bool SetCursor(byte linkedListNo, int x, int y)
            {
                byte[] data = new byte[] { 0x2, (byte)x, (byte)y };
                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.setCursor, "OK:", linkedListNo, data);
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
                return true;
            }


            public  string[] GetMiscCmds(byte displayType)
            {

                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.getMiscCmds, "OK:", displayType);
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
                    return true;
                }

               
                public  bool drawCircle(byte displayLinkedListIndex, byte x = 60, byte y = 32, byte radius = 20)
                {
                    byte[] data = new byte[] { 0x4, (byte)OLEDMiscCmds.drawCircle, x, y, radius };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

               
                public  bool drawFrame(byte displayLinkedListIndex, byte x = 30, byte y = 5, byte w = 60, byte h = 55)
                {
                    byte[] data = new byte[] { 0x5, (byte)OLEDMiscCmds.drawFrame, x, y, w, h };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
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
                enum NEOPIXELMiscCmds { setpixelcolor, setpixelcolorAll, setpixelcolorOdds, setpixelcolorEvens, setBrightness, setN, NEOPIXELMiscCmds_MAX };

                public  byte MaxNumPixels = 8;
                public  bool Clear(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x4, (byte)NEOPIXELMiscCmds.setpixelcolorAll, 0, 0, 0 };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }
                public  bool Misc_SetAll(byte displayLinkedListIndex, byte red, byte green, byte blue)
                {
                    byte[] data = new byte[] { 0x4, (byte)NEOPIXELMiscCmds.setpixelcolorAll, red, green, blue };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

                public  bool Misc_SetN(byte displayLinkedListIndex, byte red, byte green, byte blue, byte n)
                {
                    byte[] data = new byte[] { 0x5, (byte)NEOPIXELMiscCmds.setN, red, green, blue, n };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

                public  bool Misc_Set(byte displayLinkedListIndex, byte pixel, byte red, byte green, byte blue)
                {
                    byte[] data = new byte[] { 0x5, (byte)NEOPIXELMiscCmds.setpixelcolor, pixel,red, green, blue };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

                public  bool Misc_SetOdd(byte displayLinkedListIndex, byte red, byte green, byte blue)
                {
                    byte[] data = new byte[] { 0x4, (byte)NEOPIXELMiscCmds.setpixelcolorOdds, red, green, blue };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }
                public bool Misc_SetBrightness(byte displayLinkedListIndex, byte level)
                {
                    byte[] data = new byte[] { 0x2, (byte)NEOPIXELMiscCmds.setBrightness, level };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

                public  bool Misc_SetEvens(byte displayLinkedListIndex, byte red, byte green, byte blue)
                {
                    byte[] data = new byte[] { 0x4, (byte)NEOPIXELMiscCmds.setpixelcolorEvens, red, green, blue };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
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
                enum LCD1602MiscCmds { home, autoscroll, noautoscroll, blink, noblink, LCD1602MiscCmds_MAX };

                public  bool Home(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)LCD1602MiscCmds.home };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

                public  bool Autoscroll(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)LCD1602MiscCmds.autoscroll };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

                public  bool NoAutoscroll(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)LCD1602MiscCmds.noautoscroll };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

                public  bool Blink(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)LCD1602MiscCmds.blink };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

                public  bool NoBlink(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)LCD1602MiscCmds.noblink };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

            }

            public  class BARGRAPHDisplay
            {
                // Misc commands
                internal SoftataLib softatalib;

                public BARGRAPHDisplay(SoftataLib parent)
                {
                    this.softatalib = parent;
                }

                public  bool Flow(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x1, (byte)BARGRAPHMiscCmds.flow };
                    string result = softatalib.SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

            }
        }


    }
}
