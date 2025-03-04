﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Softata.Enums;

namespace Softata
{
    public partial class SoftataLib
    {
        public  class Analog  
        {
            private SoftataLib softatalib;

            public Analog(SoftataLib parent)
            {
                this.softatalib = parent;
                this.InitAnalogDevicePins();
            }
            const int Max_Analog = 1023;//??
            private  bool ValidateAnalogAssignedPin(int pinNumber)
            {
                if (!softatalib.GroveAnalogPinList.Contains($"A{pinNumber-26}"))
                {
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_AnalogPinRange26to28");
                }
                foreach (var item in AnalogDevicePins)
                {
                    if (item.Value.PinNumber == pinNumber)
                    {
                        return true;
                    }
                }
                throw new ArgumentOutOfRangeException(nameof(pinNumber), "Analog pin not assigned");
            }

            private  bool ValidateAnalogPin(int pinNumber, bool update)
            {
                if (!softatalib.GroveAnalogPinList.Contains($"A{pinNumber-26}"))
                {
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_DigitalPinRange26to28");
                }
                if(update)
                { return true; }

                foreach (var item in AnalogDevicePins)
                {
                    if (item.Value.PinNumber == pinNumber)
                    {
                        throw new ArgumentOutOfRangeException(nameof(pinNumber), "Analog pin already assigned");
                    }
                }
                return true;
            }

            public enum ADCResolutionBits { Bits10, Bits12 };
            public enum ADCResolutionBitsX { Bits10 = 10, Bits12 = 12 };
            public enum AnalogDevice { LightSensor = 0, SoundSensor = 1, Potentiometer = 2, Other = 3,  Undefined = 255 };

            public enum OtherAnalogDevice { ADOther1=1,ADOther2=2, ADOther3=3, ADOther4=4, MaxType = 5, };    
            public class ADevice
            {
                public int PinNumber { get; set; }
                public AnalogDevice DeviceType { get; set; }
                public ADCResolutionBits ResolutionBits { get; set; } = ADCResolutionBits.Bits10; //1023
                public byte BitsShiftRight { get; set; } = 0;
            }



            public  Dictionary<AnalogDevice, ADevice> AnalogDevicePins = new Dictionary<AnalogDevice, ADevice>();


            public  void InitAnalogDevicePins(RPiPicoMode rPiPicoMode)
            {
                softatalib._RPiPicoMode = rPiPicoMode;
                AnalogDevicePins = new Dictionary<AnalogDevice, ADevice>();;
            }

            public  void InitAnalogDevicePins()
            {
                AnalogDevicePins = new Dictionary<AnalogDevice, ADevice>();
            }

            public  bool SetAnalogPin(AnalogDevice device, byte pinNumber, bool update = false, ADCResolutionBits resolutionsBits =  ADCResolutionBits.Bits10, byte bitsShiftRight=0)
            {
                if(!ValidateAnalogPin(pinNumber, update))
                    return false;

                if(resolutionsBits == ADCResolutionBits.Bits10)
                {
                    string state = softatalib.SendMessage(Commands.analogSetResolution, pinNumber, 10, "AD:");
                }
                else 
                {
                    string state = softatalib.SendMessage(Commands.analogSetResolution, pinNumber, 12, "AD:");
                }
                ADevice aDevice = new ADevice { PinNumber = pinNumber, DeviceType = device, ResolutionBits = resolutionsBits, BitsShiftRight = bitsShiftRight };
                if (AnalogDevicePins.ContainsKey(device))
                {
                    if((!update) && (device != AnalogDevice.Other))
                        return false;
                    AnalogDevicePins[device] = aDevice;
                }
                else
                    AnalogDevicePins.Add(device, aDevice);
                return true;
            }
            
            public  int AnalogRead(byte pinNumber)
            {
                if (!ValidateAnalogAssignedPin(pinNumber))
                    return int.MaxValue;

                string state = softatalib.SendMessage(Commands.analogRead, (byte)pinNumber, nullData, "AD:");

                int result = int.MaxValue;
                if (state != "Reset")
                {
                    if (int.TryParse(state, out int val))
                    {
                        foreach (var item in AnalogDevicePins)
                        {
                            if (item.Value.PinNumber == pinNumber)
                            {
                                byte bitsShiftRight = item.Value.BitsShiftRight;
                                result = val >> bitsShiftRight;
                                break;
                            }
                        }
                        
                    }
                }
                return result;
            }

            public  double  AnalogReadLightSensor()
            {
                AnalogDevice device = AnalogDevice.LightSensor;

                if (!AnalogDevicePins.ContainsKey(device))
                    throw new ArgumentOutOfRangeException(nameof(device), "Analog: Light Sensor not assigned");
                int pinNumber = AnalogDevicePins[device].PinNumber;
                int maxADCValue = 1023;
                if (AnalogDevicePins[device].ResolutionBits == ADCResolutionBits.Bits12)
                    maxADCValue = 4095;

                string state = softatalib.SendMessage(Commands.analogRead, (byte)pinNumber, nullData, "AD:");

                double result = int.MaxValue;
                if (state != "Reset")
                {
                    if (int.TryParse(state, out int val))
                    {
                        result = (100.0 * ((double)val)) / ((double)maxADCValue);
                        return result;
                    }
                    else
                        return double.MaxValue;
                }
                else
                    return double.MaxValue;
            }

            public  double AnalogReadSoundSensor()
            {
                AnalogDevice device = AnalogDevice.SoundSensor;


                if (!AnalogDevicePins.ContainsKey(device))
                    throw new ArgumentOutOfRangeException(nameof(device), "Analog: Sound Sensor  not assigned.");
                int pinNumber = AnalogDevicePins[device].PinNumber;
                int maxADCValue = 1023;
                if (AnalogDevicePins[device].ResolutionBits == ADCResolutionBits.Bits12)
                    maxADCValue = 4095;

                string state = softatalib.SendMessage(Commands.analogRead, (byte)pinNumber, nullData, "AD:");

                double result = int.MaxValue;
                if (state != "Reset")
                {
                    if (int.TryParse(state, out int val))
                    {
                        result = (100.0 * ((double)val)) / ((double)maxADCValue);
                        return result;
                    }
                    else
                        return double.MaxValue;
                }
                else
                    return double.MaxValue;
            }

            // Note only one of each analog device type allowed
            public  double AnalogReadPotentiometer()
            {
                AnalogDevice device = AnalogDevice.Potentiometer;


                if (!AnalogDevicePins.ContainsKey(device))
                    throw new ArgumentOutOfRangeException(nameof(device), "Analog: Potentiometer not assigned.");
                int pinNumber = AnalogDevicePins[device].PinNumber;
                int maxADCValue = 1023;
                if(AnalogDevicePins[device].ResolutionBits==ADCResolutionBits.Bits12)
                    maxADCValue = 4095;


                string state = softatalib.SendMessage(Commands.analogRead, (byte)pinNumber, nullData, "AD:");

                double result = int.MaxValue;
                if (state != "Reset")
                {
                    if (int.TryParse(state, out int val))
                    {
                        result = (100.0 * ((double)val)) / ((double)maxADCValue);
                        return result;
                    }
                    else
                        return double.MaxValue;
                }
                else
                    return double.MaxValue;
            }
        }



        public  class PWM
        {
            private SoftataLib softatalib;

            public PWM(SoftataLib parent)
            {
                this.softatalib = parent;
            }
            private  bool ValidatePWMPin(int pinNumber)
            {
                if ((pinNumber < 0) || (pinNumber >= PinMax))
                {
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PWMPinRange0to28");
                    //return false;
                }

                if (softatalib._RPiPicoMode == RPiPicoMode.groveShield)
                {
                    if (!softatalib.GroveGPIOPinList.Contains($"p{pinNumber}"))
                    {
                        throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PWMPinRange16to21");
                    }
                }
                return true;
            }


            public  bool SetPinModePWM(byte pinNumber, byte pwmResolutionBits = 8)
            {
                if (pwmResolutionBits == 0)
                    pwmResolutionBits = 8;
                if (pwmResolutionBits < 4 || pwmResolutionBits > 16)
                    throw new ArgumentOutOfRangeException(nameof(pwmResolutionBits), "Messages.ArgumentEx_PWMRange4to16Bits");

                // Set pin as Digital Output
                if (!ValidatePWMPin(pinNumber))
                    return false;

                string resp = softatalib.SendMessage(Commands.pinMode, (byte)pinNumber, (byte)PinMode.DigitalOutput);
                if (resp.Contains(":"))
                {
                    resp = softatalib.SendMessage(Commands.analogWriteResolution, (byte)pinNumber, pwmResolutionBits,"PW");
                    if (resp.Contains(":"))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            public  bool  SetPWM(int pinNumber, int value)
            {
                // Need to check value against number of bits
                if (!ValidatePWMPin(pinNumber))
                    return false;

                UInt16 Value = (UInt16)value;

                byte[] bytes = BitConverter.GetBytes(Value);
                byte[] data = bytes.Prepend((byte)bytes.Length).ToArray<byte>(); //Prepend string length +1


                string result = softatalib.SendMessage(Commands.pwmWrite, (byte)pinNumber, nullData,"PW",nullData,data);
                if(result.Contains(":" ))
                    return true;
                else
                    return false;
            }

        }
    }
}
