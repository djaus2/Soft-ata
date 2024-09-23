using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Softata.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Softata
{
    public partial class SoftataLib
    {
        public enum GroveActuatorCmds { a_getpinsCMD, a_getValueRangeCMD, a_setupDefaultCMD, a_setupCMD, a_writeDoubleValueCMD, a_writeByteValueCMD, a_writeWordValueCMD, a_SetBitStateCMD, a_SetBitCMD, a_ClearBitCMD, a_ToggleBitCMD, a_getActuatorsCMD = 255 };

        public static class Actuator
        {


            public enum ActuatorDevice { Servo = 0,Shift95ParaOut=1, Relay = 2, Undefined = 0xff };


            private static  List<int> ValidActuatorPins = new List<int> { 16,18,20 };
            private static string csvValidActuatorPins = "16,18,20";


            public static string[] GetActuators()
            {

                string result = SendMessage(Commands.groveActuator, 0, (byte)GroveActuatorCmds.a_getActuatorsCMD, "OK:ACTUATORS:", 0);
                if (!string.IsNullOrEmpty(result))
                    return result.Split(',');
                else
                    return new string[0];
            }

            public static string GetPins(ActuatorDevice deviceType, bool debug = true)
            {
                string result = SendMessage(Commands.groveActuator, 0, (byte)GroveActuatorCmds.a_getpinsCMD, "OK:", (byte)deviceType, null, debug = true);
                return result;
            }

            public static string GetValueRange(ActuatorDevice deviceType, bool debug = true)
            {
                string result = SendMessage(Commands.groveActuator, 0, (byte)GroveActuatorCmds.a_getValueRangeCMD, "OK:", (byte)deviceType, null, debug = true);
                return result;
            }

            public static int Setup(ActuatorDevice deviceType, byte pinNumber)
            {
                if (!ValidActuatorPins.Contains(pinNumber))
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), $"Valid actuator pins are {csvValidActuatorPins} with RPi Pico");

                string result = SendMessage(Commands.groveActuator, pinNumber, (byte)GroveActuatorCmds.a_setupCMD, "OK:", (byte)deviceType);
                if (int.TryParse(result, out int linkedListNo))
                    return linkedListNo;
                else
                    return -1;

            }

            public static int Setup(ActuatorDevice deviceType, byte pinNumber, byte min=0, byte max=0)
            {
                if (!ValidActuatorPins.Contains(pinNumber))
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), $"Valid actuator pins are {csvValidActuatorPins} with RPi Pico");
                string result = "";
                if((max==min) || (max<min))
                { 
                    result = SendMessage(Commands.groveActuator, pinNumber, (byte)GroveActuatorCmds.a_setupCMD, "OK:", (byte)deviceType);
                }
                else
                {
                    byte[] bytes = new byte[] { min,max };
                    byte[] data = bytes.Prepend((byte)bytes.Length).ToArray<byte>(); //Prepend string length +1

                    result = SendMessage(Commands.groveActuator, pinNumber, (byte)GroveActuatorCmds.a_setupCMD, "OK:", (byte)deviceType, data);
                }
                if (int.TryParse(result, out int linkedListNo))
                    return linkedListNo;
                else
                    return -1;

            }

            public static int SetupDefault(ActuatorDevice deviceType)
            {

                string result = SendMessage(Commands.groveActuator, 0, (byte)GroveActuatorCmds.a_setupDefaultCMD, "OK:", (byte)deviceType);
                if (int.TryParse(result, out int linkedListNo))
                    return linkedListNo;
                else
                    return -1;
            }



            public static bool  ActuatorWrite(byte linkedListNo, byte value)
            {
                byte[] bytes = new byte[] { value };
                byte[] data = bytes.Prepend((byte)bytes.Length).ToArray<byte>(); //Prepend string length +1
                string result = SendMessage(Commands.groveActuator, 0, (byte)GroveActuatorCmds.a_writeByteValueCMD, "OK:", linkedListNo, data);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public static bool ActuatorWriteWord(byte linkedListNo, UInt16 value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                byte[] data = bytes.Prepend((byte)bytes.Length).ToArray<byte>(); //Prepend string length +1
                string result = SendMessage(Commands.groveActuator, 0, (byte)GroveActuatorCmds.a_writeWordValueCMD, "OK:", linkedListNo, data);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public static bool ActuatorWriteDouble(byte linkedListNo, double value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                byte[] data = bytes.Prepend((byte)bytes.Length).ToArray<byte>(); //Prepend string length +1
                string result = SendMessage(Commands.groveActuator, 0, (byte)GroveActuatorCmds.a_writeByteValueCMD, "OK:", linkedListNo, data);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public static bool ToggleBit(byte actuatorListIndex, byte bit)
            {
                byte[] bytes = new byte[]{bit};
                byte[] data = bytes.Prepend((byte)bytes.Length).ToArray<byte>(); //Prepend string length +1
                string result = SendMessage(Commands.groveActuator, 0, (byte)GroveActuatorCmds.a_ToggleBitCMD, "OK:", actuatorListIndex, data);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public static bool ClearBit(byte actuatorListIndex, byte bit)
            {
                byte[] bytes = new byte[] { bit };
                byte[] data = bytes.Prepend((byte)bytes.Length).ToArray<byte>(); //Prepend string length +1
                string result = SendMessage(Commands.groveActuator, 0, (byte)GroveActuatorCmds.a_ClearBitCMD, "OK:", actuatorListIndex, data);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

            public static bool SetBit(byte actuatorListIndex, byte bit)
            {
                byte[] bytes = new byte[] { bit };
                byte[] data = bytes.Prepend((byte)bytes.Length).ToArray<byte>(); //Prepend string length +1
                string result = SendMessage(Commands.groveActuator, 0, (byte)GroveActuatorCmds.a_SetBitCMD, "OK:", actuatorListIndex, data);
                if (true) //TBD
                    return true;
                else
                    return false;
            }
        }
    }
}
