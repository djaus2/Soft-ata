using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Softata.SoftataLib;

namespace Softata
{
    public partial class SoftataLib
    {
        /*
        public enum Commands
        {
            ...

            //Serial
            serialSetup = 0xE0, // Setup Serial1/2
            serialGetChar = 0xE1, // Get a char
            serialGetString = 0xE2, // Get a string
            serialGetStringUntil = 0xE3, // Get a string until char
            serialWriteChar = 0xE4, // Write a char
            serialGetFloat = 0xE5, // Get Flost
            serialGetInt = 0xE6, // Get Int

            ...
        }
        */

        enum BAUDRATE
        {
            bd50, bd75, bd110, bd134, bd150, bd200, bd300, bd600, bd1200, bd1800, bd2400, bd4800, bd9600, bd19200, bd38400, bd57600, bd115200
        };

        static int[] Baudrates = { 50, 75, 110, 134, 150, 200, 300, 600, 1200, 1800, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };

        public static class Serial
        {
            // Note for these commands, the serialportNo is 1 or 2
            // other parameter specifies this 
            // Pin is nulldata except for Setup

            public static void serialSetup(byte pin, int baud, byte serialportNo)
            {
                string expect = "OK";
                if (pin <= 0 || pin >= PinMax)
                    throw new ArgumentOutOfRangeException(nameof(pin), "Messages.ArgumentEx_PinRange0_127");
                if (serialportNo < 1 || serialportNo > 2)
                    throw new ArgumentOutOfRangeException(nameof(serialportNo), "Messages.ArgumentEx_SerialPortRange1or2");

                // Sending baudrate as index in list of baudrates, only a byte needed
                byte baudIndex = (byte)Baudrates.Select((s, i) => new { i, s })
                    .Where(t => t.s == baud)
                    .Select(t => t.i)
                    .ToList().First();

                string state = SendMessage(Commands.serialSetup, pin, baudIndex, expect, serialportNo);
                if (state != expect)
                    throw new Exception($"serialGetChar({serialportNo}):InvalidResult");
                return;
            }

            //Nb: Next two commands both call serialGetChar on Pico
            public static char serialGetChar(byte serialportNo)
            {
                string expect = "SER";
                if (serialportNo <1 || serialportNo >2)
                    throw new ArgumentOutOfRangeException(nameof(serialportNo), "Messages.ArgumentEx_SerialPortRange1or2");
                string state = SendMessage(Commands.serialGetChar, nullData, nullData, expect, serialportNo);
                if (!(state.Length > expect.Length))
                {
                    Console.WriteLine($"Got {state} expected {expect}");
                    throw new Exception($"serialGetChar({serialportNo}):InvalidResult");
                }
                else if (byte.TryParse(state.Substring(expect.Length), out byte b))
                    return (char)b;
                else
                    throw new Exception($"serialGetChar({serialportNo}):InvalidResult");
            }

            public static byte serialGetByte(byte serialportNo)
            {
                string expect = "SER";
                if (serialportNo < 1 || serialportNo > 2)
                    throw new ArgumentOutOfRangeException(nameof(serialportNo), "Messages.ArgumentEx_SerialPortRange1or2");
                string state = SendMessage(Commands.serialGetChar, nullData, nullData, expect, serialportNo);
 
                if (!(state.Length > expect.Length))
                {
                    Console.WriteLine($"Got {state} expected {expect}");
                    throw new Exception($"serialGetByte({serialportNo}):InvalidResult");
                }
                else if (byte.TryParse(state.Substring(expect.Length), out byte b))
                    return b;
                else
                    throw new Exception($"serialGetByte({serialportNo}):InvalidResult");
            }



            //Nb: Next two commands both call serialWriteChar on Pico 
            public static void serialWriteChar(byte serialportNo, char value)
            {
                string expect = "OK";
                if (serialportNo < 1 || serialportNo > 2)
                    throw new ArgumentOutOfRangeException(nameof(serialportNo), "Messages.ArgumentEx_SerialPortRange1or2");
                string state = SendMessage(Commands.serialWriteChar, nullData, (byte)value, expect, serialportNo);
                if (state != expect)
                    throw new Exception($"serialWriteChar({serialportNo}):InvalidOutcome");
                return;;
            }

            public static void serialWriteByte(byte serialportNo, byte value)
            {
                string expect = "OK";
                if (serialportNo < 1 || serialportNo > 2)
                    throw new ArgumentOutOfRangeException(nameof(serialportNo), "Messages.ArgumentEx_SerialPortRange1or2");
                string state = SendMessage(Commands.serialWriteChar, nullData, value, expect, serialportNo);
                if (state != expect)
                    throw new Exception($"serialWriteByte({serialportNo}):InvalidOutcome");
                return; ;
            }


        }
    }
}
