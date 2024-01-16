using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Softata
{
    public enum GroveSensorCmds{getpins, getProperties, setupdefault, setup,  readall, read, getTelemetry, getSensors =255,
    }
    // getpins, getProperties are specific sensor class static commands so send sensor type as other rather than linkedListNo
    // getSensors is a static sensor class command.
    public partial class SoftataLib
    {

        public static class Sensor
        {

            public static string[] GetSensors()
            {
                string result = SendMessage(Commands.groveSensor, 0, (byte)GroveSensorCmds.getSensors, "OK:", 0);
                if (!string.IsNullOrEmpty(result))
                    return result.Split(',');
                else
                    return new string[0];
            }

            public static string GetPins(byte sensorType)
            {
                string result = SendMessage(Commands.groveSensor, 0, (byte)GroveSensorCmds.getpins, "OK:", sensorType);
                return result;
            }

            public static string[] GetProperties(byte sensorType)
            {
                string result = SendMessage(Commands.groveSensor, 0, (byte)GroveSensorCmds.getProperties, "OK:", sensorType);
                if (!string.IsNullOrEmpty(result))
                    return result.Split(',');
                else
                    return new string[0];
            }

            public static int SetupDefault(byte sensorType)
            {
                string result = SendMessage(Commands.groveSensor, 0, (byte)GroveSensorCmds.setupdefault, "OK:", sensorType);
                if (int.TryParse(result, out int linkedListNo))
                    return linkedListNo;
                else
                    return -1;
            }

            public static double[]?  ReadAll(byte linkedListNo)
            {
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

            public static string GetTelemetry(byte linkedListNo)
            {
                string result = SendMessage(Commands.groveSensor, 0, (byte)GroveSensorCmds.getTelemetry, "OK:", linkedListNo);
                return result;
            }

            public static double? Read(byte linkedListNo, byte property)
            {
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
