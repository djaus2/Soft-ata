using ConsoleTextFormat;
using Softata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Softata.SoftataLib;

using B = ConsoleTextFormat.Fmt.Bold;
using F = ConsoleTextFormat.Fmt;
using ConsoleTextFormat;

namespace SoftataBasic
{
    internal partial class Program
    {
        public delegate double ReadADC();
        public static bool YesNoQuit(string prompt, bool quit = false)
        {
            char ch = 'Y';
            if (!quit)
            {
                Console.Write($"{prompt}? [Y/N(Default)]:");
            }
            else
            {
                Console.WriteLine($"{Fmt.fgblu}{prompt}{Fmt.clr}");
                ch = 'Q';
            }

            string? s = Console.ReadLine();
            if (!string.IsNullOrEmpty(s))
            {
                if ((s.ToUpper())[0] == ch)
                    return true;
            }
            return false;
        }

        public static Dictionary<Analog.AnalogDevice, Tuple<double, double>>? MaxMins = null;

       public static void ADCGetMaxMin(SoftataLib.Analog.AnalogDevice device,byte pin, ReadADC? readADC)
        {

            double max = 0.0;
            double min = 0.0;

            if (MaxMins == null)
            {
                MaxMins = new Dictionary<Analog.AnalogDevice, Tuple<double, double>>();
            }
            if (!MaxMins.Keys.Contains(device))
            {
                MaxMins.Add(device, new Tuple<double, double>(PotMax, PotMin));
            }
            if (!softatalibAnalog!.AnalogDevicePins.Keys.Contains(device))
            {
                softatalibAnalog.SetAnalogPin(device, pin, true, Softata.SoftataLib.Analog.ADCResolutionBits.Bits10);
            }

            if (!YesNoQuit($"Calibrate {device}", false))
                return;

            Tuple<double, double> maxMin = MaxMins[device];
            max = maxMin.Item1;
            min = maxMin.Item2;


            Console.WriteLine($"{Tab5}{B.bgblu}Set device to max.{Fmt.clr}");
            Console.WriteLine($"{Tab5}{B.bgGre}Press any key to continue.{Fmt.clr}");
            Console.ReadLine();
            if (readADC == null)
                min = softatalibAnalog.AnalogRead(pin);
            else
                min = readADC();
            Console.WriteLine($"{Tab5}{B.bgblu}Set device to min (0).{Fmt.clr}");
            Console.WriteLine($"{Tab5}{B.bgGre}Press any key to continue.{Fmt.clr}");
            Console.ReadLine();
            if (readADC == null)
                max = softatalibAnalog.AnalogRead(pin);
            else
                max = readADC();
            if (min > max)
            {
                double temp = max;
                max = min;
                min = temp;
            }
            Console.WriteLine($"{Tab5}{Fmt.bgGre}Press any key to continue.{Fmt.clr}");
            Console.ReadLine();
            MaxMins[device] = new Tuple<double, double>(max, min);

            return;
        }
        public static void PotGetMaxMin()
        {
            ADCGetMaxMin(Analog.AnalogDevice.Potentiometer, POTENTIOMETER, softatalibAnalog!.AnalogReadPotentiometer);
        }

        public static void LightGetMaxMin()
        {
            ADCGetMaxMin(Analog.AnalogDevice.LightSensor, LIGHTSENSOR, softatalibAnalog!.AnalogReadLightSensor);
        }

        public static void SoundGetMaxMin()
        {
            ADCGetMaxMin(Analog.AnalogDevice.SoundSensor, SOUNDSENSOR, softatalibAnalog!.AnalogReadSoundSensor);
        }

        public static void OtherGetMaxMin(  byte pin, Softata.SoftataLib.Analog.ADCResolutionBits bits)
        {
            ADCGetMaxMin(Analog.AnalogDevice.Other, pin, null);
        }

        public static double Scale(double val, Tuple<double, double> maxMin, double scaleTo)
        {
            double max = maxMin.Item1;
            double min = maxMin.Item2;
            
            double calc = 0;
            if (val < min)
                calc = 0;
            else if (val > max)
                calc = scaleTo;
            else
                calc = scaleTo * ((val - min) / (max - min));
            return calc;
        }

        public static double Scale(double val, Analog.AnalogDevice device, double scakeTo)
        {
            if (MaxMins == null)
                return Scale(val, new Tuple<double,double>(100, 0), 100);
            return Scale(val, MaxMins[device], scakeTo); 
        }

        internal static SoftataLib.Analog? softatalibAnalog = null;

        public static void AnalogInit()
        {
            if (softatalibAnalog == null)
            {
                softatalibAnalog = new SoftataLib.Analog(softatalib);
            }
            softatalibAnalog = new SoftataLib.Analog(softatalib);
            softatalibAnalog.InitAnalogDevicePins(SoftataLib.RPiPicoMode.groveShield);
            MaxMins = new Dictionary<Analog.AnalogDevice, Tuple<double, double>>();
        }

        public static void Calibrate()
        {
            if (!(softatalibAnalog == null))
            {
                // Run calibration at least once
                if (hasRunCalibrationOnce)
                {
                    if (!(YesNoQuit("Calibrate Analog Devices", false)))
                        return;
                }
            }
            else
            {
                AnalogInit();
            }

            // Each of these does a setup before getting option to opt out
            PotGetMaxMin();
            LightGetMaxMin();
            SoundGetMaxMin();
        }
    }
}
