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
        

        public static class Digital
        {
            private static bool ValidateDigitalPin(int pinNumber)
            {
                if ((pinNumber < 0) || (pinNumber >= PinMax))
                {
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_DigitalPinRange0to28");
                    //return false;
                }

                if (_RPiPicoMode == RPiPicoMode.groveShield)
                {
                    if (!GroveGPIOPinList.Contains($"p{pinNumber}"))
                    {
                        throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_DigitalPinRange16to21");
                    }
                }
                return true;
            }


            public static bool SetPinMode(int pinNumber, PinMode mode)
            {
           
                if(!ValidateDigitalPin(pinNumber))
                    return false;
                string resp = SendMessage(Commands.pinMode, (byte)pinNumber, (byte)mode);
                if (resp.Contains("OK"))
                    return true;
                else
                    return false;             
            }

            public static bool SetPinState(int pinNumber, PinState state)
            {
                if (!ValidateDigitalPin(pinNumber))
                    return false;
                string resp = SendMessage(Commands.digitalWrite, (byte)pinNumber, (byte)state);
                if (resp.Contains("OK"))
                    return true;
                else
                    return false;
            }

            public static bool TogglePinState(int pinNumber)
            {
                if (!ValidateDigitalPin(pinNumber))
                    return false;
                string resp = SendMessage(Commands.digitalToggle, (byte)pinNumber);
                if (resp.Contains("OK"))
                    return true;
                else
                    return false;
            }

            public static bool GetPinState(int pinNumber)
            {
                if (!ValidateDigitalPin(pinNumber))
                    return false;

                string state = SendMessage(Commands.digitalRead, (byte)pinNumber, nullData, "ON,OFF");

                if (state.ToUpper() == "ON")
                    return true;
                else
                    return false;
            }

            public static void TurnOffWDTUpdates()
            {
                Console.WriteLine("Cause WatchDog reboot and/or OTA update failure, press[Return]");
                SendMessageCmd("WDTTimeOut");
            }
        }


    }
}
