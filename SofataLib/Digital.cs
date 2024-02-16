using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Softata
{
    public partial class SoftataLib
    {
        /*
        public enum Commands
        {
            //DigitalButtonLED IO
            pinMode = 0xD0,
            digitalWrite = 0xD1,
            digitalRead = 0xD2,
            digitalToggle = 0xD3,

            Undefined = 0xFF
        }
        */

        public static class Digital
        {

            public static void SetPinMode(int pinNumber, PinMode mode)
            {
           
                if (pinNumber <= 0 || pinNumber >= PinMax)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                SendMessage(Commands.pinMode, (byte)pinNumber, (byte)mode);
            }

            public static void SetPinState(int pinNumber, PinState state)
            {
                if (pinNumber <= 0 || pinNumber >= PinMax)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                SendMessage(Commands.digitalWrite, (byte)pinNumber, (byte)state);
            }

            public static void TogglePinState(int pinNumber)
            {
                if (pinNumber <= 0 || pinNumber >= PinMax)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                SendMessage(Commands.digitalToggle, (byte)pinNumber);
            }

            public static bool GetPinState(int pinNumber)
            {
                if (pinNumber <= 0 || pinNumber >= PinMax)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string state = SendMessage(Commands.digitalRead, (byte)pinNumber, nullData, "ON,OFF");

                if (state.ToUpper() == "ON")
                    return true;
                else
                    return false;
            }
        }


    }
}
