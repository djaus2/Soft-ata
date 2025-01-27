using ConsoleTextFormat;

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

    }
}