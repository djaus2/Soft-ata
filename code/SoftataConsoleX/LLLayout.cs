using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleTextFormat;
using Softata.ActionCommands;

namespace SoftataConsole2
{
    public class LLayout : ILLayout
    {
        public void Info(string msg, string msg2 = "")
        {
            Layout.Info(msg, msg2);
        }

        public void Prompt(string msg, string msg2 = "")
        {
            Layout.Prompt(msg, msg2);
        }


        public int Prompt4IntInRange(int info1, int info2)
        {
            return Layout.Prompt4IntInRange(info1, info2);
        }

        public bool Prompt4Bool()
        {
            return Layout.Prompt4Bool();
        }

        public int Prompt4Num(int info1, int info2, bool info3)
        {
            return Layout.Prompt4Num(info1, info2, info3);
        }

        public Selection PromptWithCSVList(int index, string miscCmds, bool v1, bool v2)
        {
            return Layout.PromptWithCSVList(index, miscCmds, v1, v2);
        }

    }
}
