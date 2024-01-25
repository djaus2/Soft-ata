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
        public enum GroveActuatorCmds { a_getpinsCMD, a_tbdCMD, a_setupDefaultCMD, a_setupCMD, a_writeDoubleValueCMD, a_writeByteValueCMD, a_getActuatorsCMD = 255 };

        public static class Actuator
        {


            public enum ActuatorDevice { Servo = 0, Undefined = 255 };


            public enum RPiPicoMode { groveShield,defaultMode, Undefined = 255 };
            private static RPiPicoMode _RPiPicoMode = RPiPicoMode.groveShield;

            private static  List<int> ValidActuatorPins = new List<int> { 16,18,20 };
            private static string csvValidActuatorPins = "16,18,20";
            public static void InitActuatorDevices(RPiPicoMode rPiPicoMode)
            {
                _RPiPicoMode = rPiPicoMode;
                if(rPiPicoMode != RPiPicoMode.groveShield)
                {
                    ValidActuatorPins = Enumerable.Range(0, 26).ToList<int>();
                }
                string csv = String.Join(",", ValidActuatorPins.Select(x => x.ToString()).ToArray());
            }

            public static string[] GetActuators()
            {

                string result = SendMessage(Commands.groveActuator, 0, (byte)GroveActuatorCmds.a_getActuatorsCMD, "OK:ACTUATORS:", 0);
                if (!string.IsNullOrEmpty(result))
                    return result.Split(',');
                else
                    return new string[0];
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

            public static bool ActuatorWrite(byte linkedListNo, double value)
            {
                byte[] bytes = BitConverter.GetBytes(value);
                byte[] data = bytes.Prepend((byte)bytes.Length).ToArray<byte>(); //Prepend string length +1
                string result = SendMessage(Commands.groveActuator, 0, (byte)GroveActuatorCmds.a_writeByteValueCMD, "OK:", linkedListNo, data);
                if (true) //TBD
                    return true;
                else
                    return false;
            }

        }
    }
}
