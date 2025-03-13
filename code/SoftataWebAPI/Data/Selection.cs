namespace SoftataWebAPI.Data
{
    public class Selection
    {
        public int Index { get; set; }

        public int Order { get; set; }

        public string Item { get; set; }

        public Selection()
        {
            Index = 0;
            Item = "";
            Order = 0;
        }

        public Selection(int index)
        {
            Index = index;
            Item = "";
            Order = index;
        }

        public Selection(int index, string item)
        {
            Index = index;
            Item = item;
            Order = index;
        }

        public Selection(int index, string item, int order)
        {
            Index = index;
            Item = item;
            Order = order;
        }

        public Selection(int index, List<string> items)
        {
            Index = index;
            Item = items[index];
            Order = index;
        }

        public Selection(int index, List<string> items, int order)
        {
            Index = index;
            Item = items[index];
            Order = order;
        }
    }
}
