using ConsoleTextFormat;
using static ConsoleTextFormat.Fmt;

namespace Softata.ActionCommands
{
    public interface ILLayout
    {
        void Info(string msg, string msg2 = "");
        void Prompt(string msg, string msg2 = "");
        bool Prompt4Bool();
        int Prompt4IntInRange(int info1, int info2);
        int Prompt4Num(int info1, int info2, bool info3);
        Selection PromptWithCSVList(int defaultInt, string csvList, bool quit = true, bool back = true);
        string ReadLine();
        // ... andText means optuionally add text on end of CSV list
        List<int> Prompt4NumswithMaxesandText(int numValues, string csvListMaxes, out string textOnEndStr, bool textOnEnd = false, Col promptcol = Col.blue, Col infocol = Col.yellow);
        List<int> Prompt4NumsandText(int numValues, out string textOnEndStr, bool textOnEnd = false, Col promptcol = Col.blue, Col infocol = Col.yellow);
        string Prompt4String(string prompt, Col promptcol = Col.blue, Col infocol = Col.yellow);


    }
}