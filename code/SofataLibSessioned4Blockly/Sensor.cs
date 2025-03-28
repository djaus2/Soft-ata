﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Softata.Enums;

namespace Softata
{

    public enum GroveSensorCmds{getpins, getProperties, setupdefault, setup,  readall, read, getTelemetry, sendTelemetryBT, sendTelemetryToIoTHub, pause_sendTelemetry, continue_sendTelemetry, stop_sendTelemetry, getSensors}
    // getpins, getProperties are specific sensor class  commands so send sensor type as other rather than linkedListNo
    // getSensors is a  sensor class command.
    public partial class SoftataLib
    {

        
        public  class Sensor
        {

            private SoftataLib softatalib;

            public Sensor(SoftataLib parent)
            {
                this.softatalib = parent;
            }

            //Links to info about the Grove Sensors 
            public  Dictionary<SensorDevice, string> Links = new Dictionary<SensorDevice, string>
            {
                {SensorDevice.DHT11,"https://wiki.seeedstudio.com/Grove-TemperatureAndHumidity_Sensor/" }
                ,{SensorDevice.BME280,"https://wiki.seeedstudio.com/Grove-Barometer_Sensor-BME280/" }
                ,{SensorDevice.UltrasonicRANGER,"https://wiki.seeedstudio.com/Grove-Ultrasonic_Ranger/" }
                /* ,Add here */
            };
            public  string[] GetSensors(Socket? client)
            {
                string result = softatalib.SendMessage(Commands.groveSensor, client, 0, (byte)GroveSensorCmds.getSensors, "OK:", 0);
                result = result.Replace("SENSORS:","");
                if (!string.IsNullOrEmpty(result))
                    return result.Split(',');
                else
                    return new string[0];
            } 

            public  string GetPins(byte sensorType,Socket client, bool debug = true)
            {
                string result = softatalib.SendMessage(Commands.groveSensor, client, 0, (byte)GroveSensorCmds.getpins, "OK:", sensorType, null, debug=true);
                return result;
            }

            public  string[] GetProperties(byte sensorType, Socket client, bool debug = true)
            {
                string result = softatalib.SendMessage(Commands.groveSensor, client, 0, (byte)GroveSensorCmds.getProperties, "OK:", sensorType, null, debug=true);
                if (!string.IsNullOrEmpty(result))
                    return result.Split(',');
                else
                    return new string[0];
            }



            public  int SetupDefault(byte sensorType, Socket client, bool debug = true)
            {
                string result = softatalib.SendMessage(Commands.groveSensor, client, 0, (byte)GroveSensorCmds.setupdefault, "OK:", sensorType, null, debug);
                if (int.TryParse(result, out int linkedListNo))
                    return linkedListNo;
                else
                    return -1;
            }

            public  int Setup(byte sensorType,Socket client, byte pin, byte datum)
            {
                List<byte> data = new List<byte> { datum };
                return Setup(sensorType, client, pin, data);
            }

            public  int Setup(byte sensorType, Socket client, byte pin, List<byte> ldata)
            {
                var data2 = ldata.Prepend((byte)ldata.Count());
                byte[] data = data2.ToArray<byte>();
                string result = softatalib.SendMessage(Commands.groveSensor,client, pin, (byte)GroveSensorCmds.setup, "OK:", sensorType, data);
                if (int.TryParse(result, out int linkedListNo))
                    return linkedListNo;
                else
                    return -1;
            }

            public  double[]?  ReadAll(byte linkedListNo, Socket client, bool debug=true )
            {
                string result = softatalib.SendMessage(Commands.groveSensor, client, 0, (byte)GroveSensorCmds.readall , "OK:",linkedListNo,null,debug);
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

            public  string GetTelemetry(byte linkedListNo, Socket client, bool debug=true)
            {
                string result = softatalib.SendMessage(Commands.groveSensor, client,0, (byte)GroveSensorCmds.getTelemetry, "OK:", linkedListNo, null,debug);
                return result;
            }

            public  double? Read(byte linkedListNo, Socket client, byte property, bool debug = true)
            {
                string result = softatalib.SendMessage(Commands.groveSensor, client, property, (byte)GroveSensorCmds.read, "OK:", linkedListNo, null,debug);
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

            public  string StartSendingTelemetryBT(byte linkedListNo, Socket client, byte period = 5, bool debug =true)
            {
                var data1 = new List<byte> { period };
                var data2 = data1.Prepend((byte)data1.Count());
                byte[] data = data2.ToArray<byte>();
                string result = softatalib.SendMessage(Commands.groveSensor, client,0, (byte)GroveSensorCmds.sendTelemetryBT, "OK:", linkedListNo, data, debug);
                if(int.TryParse(result, out int value))
                {
                    return result;
                }
                return "-1";
            }

            public  string StopSendingTelemetry(byte linkedListNo, Socket client, bool debug = true)
            {;
                string result = softatalib.SendMessage(Commands.groveSensor, client,0, (byte)GroveSensorCmds.stop_sendTelemetry, "OK:", linkedListNo, null, debug);
                if (int.TryParse(result, out int value))
                {
                    return result;
                }
                return "-1";
            }

            /// <summary>
            /// Start telemetry transmission
            /// </summary>
            /// <param name="linkedListNo">As returned from sensor instantiation [ ie from Setup()]</param>
            /// <param name="period">(Optional)Telemetry period in seconds [Default=5 sec]</param>
            /// <param name="debug">(Optional)Extram msgs diplayed</param>
            /// <returns>SensorListIndex as string (1..10) if all OK, "-1" otherwise</returns>
            public  string StartSendingTelemetryToIoTHub(byte linkedListNo, Socket client, byte period = 5, bool debug = true)
            {
                var data1 = new List<byte> { period };
                var data2 = data1.Prepend((byte)data1.Count());
                byte[] data = data2.ToArray<byte>();

                string result = softatalib.SendMessage(Commands.groveSensor, client, 0, (byte)GroveSensorCmds.sendTelemetryToIoTHub, "OK:", linkedListNo,data, debug);
                if (int.TryParse(result, out int value))
                {
                    return result;
                }
                return "-1";              
            }

            public  string PauseSendTelemetry(byte linkedListNo, Socket client, bool debug = true)
            {
                string result = softatalib.SendMessage(Commands.groveSensor, client,0, (byte)GroveSensorCmds.pause_sendTelemetry, "OK:", linkedListNo, null, debug = true);
                return result;
            }

            public  string ContinueSendTelemetry(byte linkedListNo, Socket client, bool debug = true)
            {
                string result = softatalib.SendMessage(Commands.groveSensor, client, 0, (byte)GroveSensorCmds.continue_sendTelemetry, "OK:", linkedListNo, null, debug= true);
                return result;
            }
        }


    }
}
