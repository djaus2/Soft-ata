namespace SoftataInfo2DB
{
    public static class Info
    {
        public static Softata.SoftataLib SoftataLib { get; set; } = new Softata.SoftataLib();

        public static Dictionary<int, string> TargetDeviceTypes { get; set; }

        public static Dictionary<int, Dictionary<int, string>> GenericCmds { get; set; }

        public static Dictionary<int, Dictionary<int, string>> TargetDevices { get; set; }
    }
}
