using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Softata.Enums;

namespace Softata
{
    public partial class SoftataLib
    {
        

        public  class Digital
        {
            private SoftataLib softatalib;

            public Digital(SoftataLib parent)
            {
                this.softatalib = parent;
            }
            private  bool ValidateDigitalPin(int pinNumber)
            {
                if ((pinNumber < 0) || (pinNumber >= PinMax))
                {
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_DigitalPinRange0to28");
                    //return false;
                }

                if (softatalib._RPiPicoMode == RPiPicoMode.groveShield)
                {
                    if (!softatalib.GroveGPIOPinList.Contains($"p{pinNumber}"))
                    {
                        throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_DigitalPinRange16to21");
                    }
                }
                return true;
            }


            public  bool SetPinMode(int pinNumber, PinMode mode)
            {
           
                if(!ValidateDigitalPin(pinNumber))
                    return false;
                string resp = softatalib.SendMessage(Commands.pinMode, (byte)pinNumber, (byte)mode);
                if (resp.Contains("OK"))
                    return true;
                else
                    return false;             
            }

            public  bool SetPinState(int pinNumber, PinState state)
            {
                if (!ValidateDigitalPin(pinNumber))
                    return false;
                string resp = softatalib.SendMessage(Commands.digitalWrite, (byte)pinNumber, (byte)state);
                if (resp.Contains("OK"))
                    return true;
                else
                    return false;
            }

            public  bool TogglePinState(int pinNumber)
            {
                if (!ValidateDigitalPin(pinNumber))
                    return false;
                string resp = softatalib.SendMessage(Commands.digitalToggle, (byte)pinNumber);
                if (resp.Contains("OK"))
                    return true;
                else
                    return false;
            }

            public  bool GetPinState(int pinNumber)
            {
                if (!ValidateDigitalPin(pinNumber))
                    return false;

                string state = softatalib.SendMessage(Commands.digitalRead, (byte)pinNumber, nullData, "ON,OFF");

                if (state.ToUpper() == "ON")
                    return true;
                else
                    return false;
            }

            public  void TurnOffWDTUpdates()
            {
                Console.WriteLine("Cause WatchDog reboot and/or OTA update failure, press[Return]");
                softatalib.SendMessageCmd("WDTTimeOut");
            }
        }


    }
}
