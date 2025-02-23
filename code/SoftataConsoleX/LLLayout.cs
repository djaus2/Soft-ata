using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleTextFormat;
using Softata.ActionCommands;
using static ConsoleTextFormat.Fmt;

namespace SoftataConsole2
{
    public class LLayout : ILLayout
    {
        public string ReadLine()
        {
            return Console.ReadLine();
        }
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

        public List<int> Prompt4NumsandText(int numValues, out string textOnEndStr, bool textOnEnd , Fmt.Col promptcol, Fmt.Col infocol)
        {
            string prompt = $"Enter CSV list of values";
            textOnEndStr = "";

            List<int> intList = new List<int>();
            int expectedNumVals = numValues;
            string msg = $"You need to enter {numValues} values separated by commas.";
            if (textOnEnd)
            {
                msg += " With text on end.";
                expectedNumVals++;
            }
            while (intList.Count != numValues)
            {
                string csv = Prompt4String(prompt, promptcol, infocol);
                List<int>  temp = new List<int>();
                string[] parts = csv.Split(",");
                if (parts.Length != expectedNumVals)
                {
                    Info(msg);
                    continue;
                }
                string[]parts2 = parts;
                if (textOnEnd)
                {
                    textOnEndStr = parts.Last();
                    parts2 = parts.SkipLast(1).ToArray();
                }
                intList = new List<int>();
                foreach (string item in parts2)
                {
                    if (int.TryParse(item, out int number))
                    {
                        intList.Add(number);
                    }
                    else
                    {
                        break;
                    }
                }
                if (intList.Count != numValues)
                {
                    Info(msg);
                    continue;
                }
            }
            return intList;
        }

        public string Prompt4String(string prompt, Fmt.Col promptcol, Fmt.Col infocol)
        {
            Console.Write($"{prompt}: ");
            string? res = Console.ReadLine();
            if (res == null)
                return "";
            return res;
        }

        public List<int> Prompt4NumswithMaxesandText(int numValues, string csvListMaxes, out string textOnEndStr, bool textOnEnd = false , Col promptcol = Col.blue, Col infocol = Col.yellow)
        {
            string[] parts = csvListMaxes.Split(",");
            string msg = $"This requires {{numValues}} values separated by commas";
            int expectedNumVals = numValues;
            textOnEndStr = "";


            if (parts.Length != numValues)
            {
                Info(msg);
                return new List<int> { -1 };
            }
            List<int> intListMazes = new List<int>();
            foreach (string item in parts)
            {
                if (int.TryParse(item, out int number))
                {
                    intListMazes.Add(number);
                }
                else
                {
                    Info(msg);
                    break;
                }
            }
            if (intListMazes.Count != numValues)
            {
                Info(msg);
                return new List<int> { -1 };
            }
            string ranges = "0 to " + csvListMaxes.Replace(",", ", 0 to ");
            msg = $"You need to enter {numValues} values separated by commas within ranges: {ranges}";
            if(textOnEnd)
            {
                msg += " With text on end";
            }
            Info(msg);
            List<int> values = new List<int>();
            while (values.Count() != numValues)
            {
                values = Prompt4NumsandText(numValues, out textOnEndStr, textOnEnd , promptcol, infocol);
                if (values.Count != numValues)
                {
                    Info(msg);
                    continue;
                }
                for (int i = 0; i < numValues; i++)
                {
                    if (values[i] < 0 || values[i] > intListMazes[i])
                    {
                        values = new List<int>();
                        Info(msg);
                        break;
                    }
                }

            }
            return values;
        }


    }
}
