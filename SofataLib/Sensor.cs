using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Softata
{
    public enum GroveSensorCmds{getpins, getProperties, setupdefault, setup, readall, read }
    public partial class SoftataLib
    {
        /*
        public enum Commands
        {
            //Digital IO
            pinMode = 0xD0,
            digitalWrite = 0xD1,
            digitalRead = 0xD2,
            digitalToggle = 0xD3,

            Undefined = 0xFF
        }
        */

        public static class Sensor
        {

            public static void GetPins(byte sensorType)
            {

                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = SendMessage(Commands.groveSensor, 0, (byte)GroveSensorCmds.getpins, "OK:", sensorType);
            }

            public static string[] GetProperties(byte sensorType)
            {

                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = SendMessage(Commands.groveSensor, 0, (byte)GroveSensorCmds.getProperties, "OK:", sensorType);
                if (!string.IsNullOrEmpty(result))
                    return result.Split(',');
                else
                    return new string[0];
            }

            public static int SetupDefault(byte sensorType)
            {
                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = SendMessage(Commands.groveSensor, 0, (byte)GroveSensorCmds.setupdefault, "OK:", sensorType);
                if (int.TryParse(result, out int linkedListNo))
                    return linkedListNo;
                else
                    return -1;
            }

            public static double[]?  ReadAll(byte linkedListNo)
            {
                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = SendMessage(Commands.groveSensor, 0, (byte)GroveSensorCmds.readall , "OK:",linkedListNo);
                try { 
                    string[] values = result.Split(',');
                    double[]? results = (from v in values
                             select double.Parse(v)).ToArray();
                    if(results != null)
                        return results;
                    else
                        return null;
                }
                catch
                {
                    return null;
                }
            }

            public static double? Read(byte linkedListNo, byte property)
            {
                //if (pinNumber <= 0 || pinNumber >= PinMax)
                //    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string result = SendMessage(Commands.groveSensor, property, (byte)GroveSensorCmds.read, "OK:", linkedListNo);
                try
                {
                    if (double.TryParse(result, out double value))
                        return value;
                    else
                        return null;
                }
                catch
                {
                    return null;
                }
            }

        }


    }
}
