using ConsoleTextFormat;

namespace SoftataLibActionCommands
{
    public interface ILayout
    {
        void Info(string msg, string msg2 = "");
        void Prompt(string v1, string v2);
        bool Prompt4Bool();
        int Prompt4IntInRange(int info1, int info2);
        int Prompt4IntInRange(int info1, string info2);
        int Prompt4Num(int info1, int info2, bool info3);
        Selection PromptWithCSVList(int index, string miscCmds, bool v1, bool v2);
    }
}