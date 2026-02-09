using System.Text.RegularExpressions;

namespace One.SimpleLog.Helpers;

internal static class LogHelper
{
    private static readonly Dictionary<string, long> SizeUnits;
    private static readonly Dictionary<string, long> TimeUnits;

    static LogHelper()
    {
        SizeUnits = new Dictionary<string, long>
        {
            { "B", 1 },
            { "KB", 1024 },
            { "MB", 1024 * 1024 },
            { "GB", 1024 * 1024 * 1024 },
        };

        TimeUnits = new Dictionary<string, long>
        {
            { "S", 1 },
            { "M", 60 },
            { "H", 60 * 60 },
            { "D", 60 * 60 * 24 },
        };
    }

    internal static long SizeUnitConvert(string input)
    {
        return UnitConvert(input, SizeUnits);
    }

    internal static long TimeUnitConvert(string input)
    {
        return UnitConvert(input, TimeUnits);
    }

    internal static long UnitConvert(string input, Dictionary<string, long> dict)
    {
        Match match = Regex.Match(input.ToUpper(), @"(\d+)\s*([A-Z]+)");
        if (!match.Success)
            return 0;

        _ = long.TryParse(match.Groups[1].Value, out long value);
        string unit = match.Groups[2].Value;
        if (dict.ContainsKey(unit))
            return value * dict[unit];

        return value;
    }

    #region PropertyReplace

    /// <summary>///</summary>
    /// <param name="inputString"></param>
    /// <param name="propertyName">代替换的属性名</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    internal static string ReplaceTargetProperty(Dictionary<string, string> dic, string inputString, string propertyName)
    {
        // 正则表达式来匹配中括号内的内容
        string pattern = @"\$\{([^:]+):([^}]+)\}";

        // 创建正则表达式对象
        Regex regex = new Regex(pattern);

        /*
        // 查找匹配项
        MatchCollection matches = regex.Matches(tempProperty);

        // 输出所有匹配项的结果
        foreach (Match match in matches)
        {
            if (match.Success)
            {
                // 提取冒号前后的内容
                string beforeColon = match.Groups[1].Value;
                string afterColon = match.Groups[2].Value;

                // 输出结果
                Console.WriteLine($"冒号前: {beforeColon}, 冒号后: {afterColon}");

                if (propertyDic.ContainsKey(beforeColon))
                {
                }
            }
        }
        */
        // 替换匹配项
        string result = regex.Replace(
            inputString,
            match =>
            {
                // 提取冒号前后的内容
                string property = match.Groups[1].Value;
                string slotHeader = match.Groups[2].Value;

                //不是属性，直接返回
                if (property != propertyName)
                {
                    return match.Value;
                }
                else
                {
                    // 根据字典获取替换值
                    if (dic.TryGetValue(slotHeader, out string replacementValue))
                    {
                        return replacementValue;
                    }
                    else
                    {
                        // 如果找不到替换值，保留原始字符串
                        return match.Value;
                        //throw new Exception($"Not find a property named => {slotHeader}");
                    }
                }
            }
        );

        return result;
    }

    internal static string ReplacePredefinedProperty(Dictionary<string, string> dic, string inputString, Func<string, string> action)
    {
        // 正则表达式来匹配中括号内的内容
        string pattern = @"\$\{([^}]+)\}"; //@"\$\{([^:]+):([^}]+)\}"

        // 创建正则表达式对象
        Regex regex = new Regex(pattern);

        // 替换匹配项
        string result = regex.Replace(
            inputString,
            match =>
            {
                // 提取冒号前后的内容
                string property = match.Groups[1].Value;

                // 根据字典获取替换值
                if (dic.TryGetValue(property, out string replacementValue))
                {
                    var temp = action.Invoke(replacementValue);
                    return temp;
                }
                else
                {
                    // 如果找不到替换值，保留原始字符串
                    return match.Value;
                }
            }
        );

        return result;
    }

    #endregion PropertyReplace
}