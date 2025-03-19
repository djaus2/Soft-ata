using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftataLib
{
    public class CommandResponse
    {
        public CommandResponse()
        {
            CommandResultInt = 0;
            CommandResultDouble = 0.0;
            CommandResultString = "";

        }
        public int CommandResultInt { get; set; }

        public double CommandResultDouble { get; set; }

        public string CommandResultString { get; set; }
    }
}
