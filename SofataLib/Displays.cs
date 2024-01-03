﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Softata
{
    public enum GroveDisplayCmds{getpins, tbd, setupDefault, setup, clear,backlight,setCursor,misc, getDisplays=255 }
    public partial class SoftataLib
    {


        public static class Display
        {

            public static string[] GetDisplays()
            {

                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.getDisplays, "OK:", 0);
                if (!string.IsNullOrEmpty(result))
                    return result.Split(',');
                else
                    return new string[0];
            }

            public static string GetPins(byte displayType)
            {

                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.getpins, "OK:", displayType);
                return result;
            }


            public static int SetupDefault(byte displayType)
            {
                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.setupDefault, "OK:", displayType);
                if (int.TryParse(result, out int linkedListNo))
                    return linkedListNo;
                else
                    return -1;
            }


            //2DO:
            /*
            public static int Setup(byte displayType, byte pin)
            {
                string result = SendMessage(Commands.groveDisplay, pin, (byte)GroveDisplayCmds.setup, "OK:", displayType);
                if (int.TryParse(result, out int linkedListNo))
                    return linkedListNo;
                else
                    return -1;
            }*/

            public static bool Clear(byte linkedListNo)
            {
                string result = SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.backlight, "OK:", linkedListNo);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public static bool Backlight(byte linkedListNo, byte property)
            {
                string result = SendMessage(Commands.groveDisplay, property, (byte)GroveDisplayCmds.backlight, "OK:", linkedListNo);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public static bool SetCursor(byte linkedListNo, int x, int y)
            {
                byte[] data = new byte[] { 0x2, (byte)x, (byte)y };
                string result = SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.setCursor, "OK:", linkedListNo, data);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            enum LCD1602MiscCmds { home, autoscroll, noautoscroll, blink, noblink, LCD1602MiscCmds_MAX };
            enum OLEDMiscCmds { drawCircle, drawFrame, OLEDMiscCmds_MAX };

            public static class Neopixel
            {
                enum NEOPIXELMiscCmds { setpixelcolor, setpixelcolorAll, setpixelcolorOdds, setpixelcolorEvens, setBrightness, NEOPIXELMiscCmds_MAX };

                public static bool Clear(byte displayLinkedListIndex)
                {
                    byte[] data = new byte[] { 0x4, (byte)NEOPIXELMiscCmds.setpixelcolorAll, 0, 0, 0 };
                    string result = SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }
                public static bool Misc_SetAll(byte displayLinkedListIndex, byte red, byte green, byte blue)
                {
                    byte[] data = new byte[] { 0x4, (byte)NEOPIXELMiscCmds.setpixelcolorAll, red, green, blue };
                    string result = SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

                public static bool Misc_Set(byte displayLinkedListIndex, byte pixel, byte red, byte green, byte blue)
                {
                    byte[] data = new byte[] { 0x5, (byte)NEOPIXELMiscCmds.setpixelcolor, red, green, blue, pixel };
                    string result = SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

                public static bool Misc_SetOdd(byte displayLinkedListIndex, byte red, byte green, byte blue)
                {
                    byte[] data = new byte[] { 0x4, (byte)NEOPIXELMiscCmds.setpixelcolorOdds, red, green, blue };
                    string result = SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }

                public static bool Misc_SetEvens(byte displayLinkedListIndex, byte red, byte green, byte blue)
                {
                    byte[] data = new byte[] { 0x4, (byte)NEOPIXELMiscCmds.setpixelcolorEvens, red, green, blue };
                    string result = SendMessage(Commands.groveDisplay, 0, (byte)GroveDisplayCmds.misc, "OK:", displayLinkedListIndex, data);
                    return true;
                }
            }
        }


    }
}