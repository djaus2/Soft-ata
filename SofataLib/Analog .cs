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
        /*
        public enum Commands
        {      
            //Analog/PWM
            analogRead = 0xA2,
            pwmWrite = 0xB1,

            Undefined = 0xFF
        }
        */
        public static class Analog
        {
            public static int AnalogRead(int pinNumber)
            {
                if (pinNumber <= 0 || pinNumber >= PinMax)
                    throw new ArgumentOutOfRangeException(nameof(pinNumber), "Messages.ArgumentEx_PinRange0_127");

                string state = SendMessage(Commands.analogRead, (byte)pinNumber, nullData, "AD:");

                int result = int.MaxValue;
                if (state.ToUpper().StartsWith("AD:"))
                {
                    state = state.Substring(3);
                    if (int.TryParse(state, out int val))
                    {
                        result = val;
                    }
                }
                return result;
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
