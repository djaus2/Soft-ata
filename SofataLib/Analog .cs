using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Softata.Enums;

namespace Softata
{
    public partial class SoftataLib
    {
        public static class Analog
        {

            const int Max_Analog = 1023;//??
            public enum AnalogDevice { LightSensor = 0, SoundSensor = 1, Potentiometer = 2, Undefined = 255 };

            public class ADevice
            {
                public int PinNumber { get; set; }
                public AnalogDevice DeviceType { get; set; }
                public int MaxValue { get; set; }
            }
            public enum RPiPicoMode { groveShield,defaultMode, Undefined = 255 };
            private static RPiPicoMode _RPiPicoMode = RPiPicoMode.groveShield;

            public static Dictionary<AnalogDevice, ADevice> AnalogDevicePins = new Dictionary<AnalogDevice, ADevice>();

            private static  List<int> ValidAnalogPins = new List<int> { 26,27, 28 };
            private static string csvValidAnalogPins = "26,27, 28";
            public static void InitAnalogDevicePins(RPiPicoMode rPiPicoMode)
            {
                _RPiPicoMode = rPiPicoMode;
                AnalogDevicePins = new Dictionary<AnalogDevice, ADevice>();
                string csv = String.Join(",", ValidAnalogPins.Select(x => x.ToString()).ToArray());
            }

            public static void SetAnalogPin(AnalogDevice device, int pinNumber, int maxValue = 1023)
            {
                if (!ValidAnalogPins.Contains(pinNumber))
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), $"Valid analog pins are {csvValidAnalogPins} with RPi Pico");

                if (AnalogDevicePins.ContainsKey(device))
                    throw new ArgumentOutOfRangeException(nameof(device), "That device already assigned. Only one allowed");


                foreach (var item in AnalogDevicePins)
                {
                    if (item.Value.PinNumber == pinNumber)
                    {
                        throw new ArgumentOutOfRangeException(nameof(pinNumber), "AnalogPotLED pin already assigned");
                        //break;
                    }
                }

                ADevice aDevice = new ADevice { PinNumber = pinNumber, DeviceType = device, MaxValue = maxValue };
                AnalogDevicePins.Add(device, aDevice);
            }
            
            public static int AnalogRead(int pinNumber)
            {
                if (!ValidAnalogPins.Contains(pinNumber))
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), $"Valid analog pins are {csvValidAnalogPins} with RPi Pico");

                string state = SendMessage(Commands.analogRead, (byte)pinNumber, nullData, "AD:");

                int result = int.MaxValue;
                if (state != "Reset")
                {
                    if (int.TryParse(state, out int val))
                    {
                        result = val;
                        return result;
                    }
                    else
                        return int.MaxValue;
                }
                else
                    return int.MaxValue;
            }

            public static double  AnalogReadLightSensor()
            {
                AnalogDevice device = AnalogDevice.LightSensor;

                if (!AnalogDevicePins.ContainsKey(device))
                    throw new ArgumentOutOfRangeException(nameof(device), "That device already assigned. Only one allowed");
                int pinNumber = AnalogDevicePins[device].PinNumber;
                int maxValue = AnalogDevicePins[device].MaxValue;

                string state = SendMessage(Commands.analogRead, (byte)pinNumber, nullData, "AD:");

                double result = int.MaxValue;
                if (state != "Reset")
                {
                    if (int.TryParse(state, out int val))
                    {
                        result = (100.0 * val) / maxValue;
                        return result;
                    }
                    else
                        return double.MaxValue;
                }
                else
                    return double.MaxValue;
            }

            public static double AnalogReadSoundSensor()
            {
                AnalogDevice device = AnalogDevice.SoundSensor;

                if (!AnalogDevicePins.ContainsKey(device))
                    throw new ArgumentOutOfRangeException(nameof(device), "That device already assigned. Only one allowed");
                int pinNumber = AnalogDevicePins[device].PinNumber;
                int maxValue = AnalogDevicePins[device].MaxValue;

                string state = SendMessage(Commands.analogRead, (byte)pinNumber, nullData, "AD:");

                double result = int.MaxValue;
                if (state != "Reset")
                {
                    if (int.TryParse(state, out int val))
                    {
                        result = (100.0 * val) / maxValue;
                        return result;
                    }
                    else
                        return double.MaxValue;
                }
                else
                    return double.MaxValue;
            }

            public static double AnalogReadPotentiometer()
            {
                AnalogDevice device = AnalogDevice.Potentiometer;

                if (!AnalogDevicePins.ContainsKey(device))
                    throw new ArgumentOutOfRangeException(nameof(device), "That device already assigned. Only one allowed");
                int pinNumber = AnalogDevicePins[device].PinNumber;
                int maxValue = AnalogDevicePins[device].MaxValue;

                string state = SendMessage(Commands.analogRead, (byte)pinNumber, nullData, "AD:");

                double result = int.MaxValue;
                if (state != "Reset")
                {
                    if (int.TryParse(state, out int val))
                    {
                        result = (100.0 * val) / maxValue;
                        return result;
                    }
                    else
                        return double.MaxValue;
                }
                else
                    return double.MaxValue;
            }
        }

        public static class PWM
        {
            public static void SetPWM(int pinNumber, byte value)
            {
                if (pinNumber <= 0 || pinNumber >= PinMax)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                SendMessage(Commands.pwmWrite, (byte)pinNumber, value);
            }

        }
    }
}
