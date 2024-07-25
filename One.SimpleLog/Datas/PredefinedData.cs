using System.Collections.Generic;

namespace One.SimpleLog.Datas
{
    internal class PredefinedData
    {
        public static Dictionary<string, string> PredefinedTimeFormate { get; set; } = new Dictionary<string, string>()
        {
            {"shortdate","yyyy-MM-dd" },
        };
    }
}
