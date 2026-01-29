using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace One.Base.ExtensionMethods;

/// <summary>字符串扩展方法</summary>
public static class StringExtensions
{
    #region 正则表达式缓存

    private static readonly Regex NumericRegex = new(@"^[+-]?\d+\.?\d*$", RegexOptions.Compiled);
    private static readonly Regex NumericOnlyRegex = new(@"^[0-9]+$", RegexOptions.Compiled);
    private static readonly Regex NumericOrLettersRegex = new(@"^[a-zA-Z0-9]+$", RegexOptions.Compiled);
    private static readonly Regex FileSizeRegex = new(@"^([\d.]+)((?:TB|GB|MB|KB|Bytes))$", RegexOptions.Compiled);
    private static readonly string[] PathUnsafeChars = { "/", "\\", ":", "*", "?", "\"", "<", ">", "|" };

    #endregion

    #region 泛型类型转换

    /// <summary>尝试将字符串转换为指定类型，失败返回默认值</summary>
    public static T To<T>(this string? t, T defaultValue = default)
    {
        if (string.IsNullOrEmpty(t)) return defaultValue;

        try
        {
            var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if (type.IsEnum)
                return (T)Enum.Parse(type, t, true);

            var method = typeof(T).GetMethod("TryParse", new[] { typeof(string), typeof(T).MakeByRefType() });
            if (method != null)
            {
                object[] args = { t, defaultValue! };
                if ((bool)method.Invoke(null, args)!)
                    return (T)args[1];
                return defaultValue;
            }

            // 处理数值类型（避免装箱拆箱）
            return ConvertTo<T>(t, defaultValue);
        }
        catch
        {
            return defaultValue;
        }
    }

    private static T ConvertTo<T>(string t, T defaultValue)
    {
        var type = typeof(T);
        if (type == typeof(int)) return (T)(object)t.ToInt();
        if (type == typeof(long)) return (T)(object)t.ToLong();
        if (type == typeof(double)) return (T)(object)t.ToDouble();
        if (type == typeof(decimal)) return (T)(object)t.ToDecimal();
        if (type == typeof(byte)) return (T)(object)t.ToByte();
        if (type == typeof(short)) return (T)(object)t.ToInt16();
        return defaultValue;
    }

    private static T TryParseEnum<T>(this string t, T defaultValue)
    {
        if (Enum.TryParse(typeof(T), t, true, out var result))
            return (T)result!;
        return defaultValue;
    }

    #endregion

    #region 数值类型转换（保留原有 API 兼容性）

    /// <summary>转Int,失败返回0</summary>
    public static int ToInt(this string? t) => int.TryParse(t, out var n) ? n : 0;

    /// <summary>转Int,失败返回默认值</summary>
    public static int ToInt(this string? t, int defaultValue) => int.TryParse(t, out var n) ? n : defaultValue;

    /// <summary>转Long,失败返回0</summary>
    public static long ToLong(this string? t) => long.TryParse(t, out var n) ? n : 0;

    /// <summary>转Long,失败返回默认值</summary>
    public static long ToLong(this string? t, long defaultValue) => long.TryParse(t, out var n) ? n : defaultValue;

    /// <summary>转Double,失败返回0</summary>
    public static double ToDouble(this string? t) => double.TryParse(t, out var n) ? n : 0;

    /// <summary>转Double,失败返回默认值</summary>
    public static double ToDouble(this string? t, double defaultValue) => double.TryParse(t, out var n) ? n : defaultValue;

    /// <summary>转Decimal,失败返回0</summary>
    public static decimal ToDecimal(this string? t) => decimal.TryParse(t, out var n) ? n : 0;

    /// <summary>转Decimal,失败返回默认值</summary>
    public static decimal ToDecimal(this string? t, decimal defaultValue) => decimal.TryParse(t, out var n) ? n : defaultValue;

    /// <summary>转byte,失败返回0</summary>
    public static byte ToByte(this string? t) => byte.TryParse(t, out var n) ? n : (byte)0;

    /// <summary>转byte,失败返回默认值</summary>
    public static byte ToByte(this string? t, byte defaultValue) => byte.TryParse(t, out var n) ? n : defaultValue;

    /// <summary>转Int16,失败返回0</summary>
    public static short ToInt16(this string? t) => short.TryParse(t, out var n) ? n : (short)0;

    /// <summary>转Int16,失败返回默认值</summary>
    public static short ToInt16(this string? t, short defaultValue) => short.TryParse(t, out var n) ? n : defaultValue;

    #endregion

    #region 类型判断

    /// <summary>是否是数值类型（可带正负号和小数点）</summary>
    public static bool IsNumeric(this string? str) => str is not null && NumericRegex.IsMatch(str);

    /// <summary>是否仅由数字组成</summary>
    public static bool IsNumericOnly(this string? str) => str is not null && NumericOnlyRegex.IsMatch(str);

    /// <summary>是否由字母和数字组成</summary>
    public static bool IsNumericOrLetters(this string? str) => str is not null && NumericOrLettersRegex.IsMatch(str);

    /// <summary>验证是否为空字符串（包含空白字符）</summary>
    public static bool IsNullOrEmptyStr(this string? str) => string.IsNullOrEmpty(str) || (str?.Trim().Length ?? 0) == 0;

    #endregion

    #region 编码转换

    /// <summary>转换为指定编码的字节数组</summary>
    public static byte[] ToBytes(this string? t, Encoding encoding) => encoding?.GetBytes(t ?? string.Empty) ?? Array.Empty<byte>();

    /// <summary>转换为UTF8字节数组</summary>
    public static byte[] ToUtf8Bytes(this string? t) => Encoding.UTF8.GetBytes(t ?? string.Empty);

    #endregion

    #region 数组转换

    /// <summary>转int[],逗号分隔</summary>
    public static int[] ToIntArray(this string? t) => t.ToIntArray(',');

    /// <summary>转int[],指定分隔符</summary>
    public static int[] ToIntArray(this string? t, params char[] separators)
    {
        if (string.IsNullOrEmpty(t)) return Array.Empty<int>();
        return t.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray();
    }

    /// <summary>过滤非int值,返回逗号分隔的字符串</summary>
    public static string ClearNoInt(this string? t, char separator = ',')
    {
        if (string.IsNullOrEmpty(t)) return string.Empty;
        var parts = t.Split(separator);
        return string.Join(separator.ToString(), parts.Where(p => int.TryParse(p, out _)));
    }

    /// <summary>是否可以转换成int[]</summary>
    public static bool IsIntArray(this string? t, char separator = ',')
    {
        if (string.IsNullOrEmpty(t)) return false;
        return t.Split(separator).All(p => int.TryParse(p, out _));
    }

    #endregion

    #region 日期时间转换

    /// <summary>转DateTime,失败返回当前时间</summary>
    public static DateTime ToDateTime(this string? t) => DateTime.TryParse(t, out var n) ? n : DateTime.Now;

    /// <summary>转DateTime,失败返回默认值</summary>
    public static DateTime ToDateTime(this string? t, DateTime defaultValue) => DateTime.TryParse(t, out var n) ? n : defaultValue;

    /// <summary>转DateTime并格式化</summary>
    public static string ToDateTime(this string? t, string format, string defaultValue = "")
    {
        if (!DateTime.TryParse(t, out var n)) return defaultValue;
        return n.ToString(format);
    }

    /// <summary>转短日期格式(yyyy-MM-dd)</summary>
    public static string ToShortDateTime(this string? t) => t.ToDateTime("yyyy-MM-dd", string.Empty);

    /// <summary>是否是有效的日期时间</summary>
    public static bool IsDateTime(this string? t) => DateTime.TryParse(t, out _);

    #endregion

    #region 字符串截取

    /// <summary>截取字符串（中文按两个字符计算）</summary>
    public static string Left(this string? t, int len, string suffix = "")
    {
        if (string.IsNullOrEmpty(t)) return string.Empty;
        if (len <= 0) return string.Empty;

        var byteLen = Encoding.Default.GetByteCount(t);
        if (byteLen <= len * 2) return t;

        var result = new StringBuilder();
        var currentLen = 0;

        foreach (var c in t)
        {
            currentLen += (c > 127) ? 2 : 1;
            if (currentLen > len * 2) break;
            result.Append(c);
        }

        return result + suffix;
    }

    /// <summary>简单截取字符串（不处理中文）</summary>
    public static string StrLeft(this string? t, int len)
    {
        if (string.IsNullOrEmpty(t)) return string.Empty;
        return t.Length > len ? t[..len] : t;
    }

    /// <summary>截取字符串（中文按两个字符计算）</summary>
    public static string CutStr(this string? str, int len, bool htmlEnable = false, string suffix = "...")
    {
        if (string.IsNullOrEmpty(str) || len <= 0) return string.Empty;

        var text = str;
        if (!htmlEnable)
        {
            // 内联 HTML 过滤逻辑（避免循环依赖）
            text = Regex.Replace(text!, "<[^>]+>", string.Empty, RegexOptions.IgnoreCase);
        }

        var byteLen = Encoding.Default.GetByteCount(text!);

        if (byteLen <= len * 2) return text;
        if (len <= 0) return suffix;

        var result = new StringBuilder();
        var currentLen = 0;

        foreach (var c in text)
        {
            currentLen += (c > 127) ? 2 : 1;
            if (currentLen > len * 2) break;
            result.Append(c);
        }

        return result + suffix;
    }

    /// <summary>获取字符串长度（中文按两个字符计算）</summary>
    public static int GetLength(this string? str)
    {
        if (string.IsNullOrEmpty(str)) return 0;
        return Encoding.Default.GetByteCount(str);
    }

    /// <summary>获取字符串真实长度（中文按2计算）</summary>
    public static int LengthReal(this string? s) => Encoding.Default.GetByteCount(s ?? string.Empty);

    #endregion

    #region 进制转换

    /// <summary>16进制转二进制字符串</summary>
    public static string HexToBinary(this string? t)
    {
        if (string.IsNullOrEmpty(t)) return string.Empty;
        return string.Concat(t.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));
    }

    /// <summary>二进制字符串转十进制</summary>
    public static long BinaryToDecimal(this string? t)
    {
        if (string.IsNullOrEmpty(t)) return 0;
        var result = 0L;
        var pow = 1L;

        for (var i = t.Length - 1; i >= 0; i--)
        {
            if (t[i] == '1') result += pow;
            pow <<= 1;
        }

        return result;
    }

    /// <summary>二进制字符串转字节数组</summary>
    public static byte[] BinaryToBytes(this string? t)
    {
        if (string.IsNullOrEmpty(t)) return Array.Empty<byte>();

        var bits = t.PadRight(((t.Length + 7) / 8) * 8, '0');
        var bytes = new List<byte>();

        for (var i = 0; i < bits.Length; i += 8)
        {
            var byteStr = bits.Substring(i, 8);
            bytes.Add(Convert.ToByte(byteStr, 2));
        }

        return bytes.ToArray();
    }

    /// <summary>二进制字符串转索引数组（从右到左）</summary>
    public static int[] BinaryToIndexArray(this string? t)
    {
        if (string.IsNullOrEmpty(t)) return Array.Empty<int>();

        var indices = new List<int>();
        var pos = 1;

        for (var i = t.Length - 1; i >= 0; i--)
        {
            if (t[i] == '1') indices.Add(pos);
            pos++;
        }

        return indices.ToArray();
    }

    #endregion

    #region 文件路径

    /// <summary>清除文件名不安全的字符</summary>
    public static string ClearPathUnsafe(this string? t)
    {
        if (string.IsNullOrEmpty(t)) return string.Empty;
        var result = t;
        foreach (var c in PathUnsafeChars)
            result = result.Replace(c, "");
        return result;
    }

    /// <summary>将文件大小字符串还原为字节数（如 "10.1MB" -> 10590668）</summary>
    public static long ParseFileSize(this string? formatedSize)
    {
        if (string.IsNullOrEmpty(formatedSize))
            throw new ArgumentNullException(nameof(formatedSize));

        // 纯数字直接返回
        if (long.TryParse(formatedSize.Replace(",", ""), out var size))
            return size;

        var match = FileSizeRegex.Match(formatedSize);
        if (!match.Success)
            throw new ArgumentException($"无效的文件大小格式: {formatedSize}");

        var value = double.Parse(match.Groups[1].Value);
        var unit = match.Groups[2].Value.ToUpper();

        var multiplier = unit switch
        {
            "TB" => 1099511627776,
            "GB" => 1073741824,
            "MB" => 1048576,
            "KB" => 1024,
            "BYTES" => 1,
            _ => 1
        };

        return (long)(value * multiplier);
    }

    #endregion

    #region 其他工具

    /// <summary>去除小数末尾的0</summary>
    public static decimal ClearDecimal0(this string? t)
    {
        if (!decimal.TryParse(t, out var d)) return 0;
        var asDouble = double.Parse(d.ToString("G"));
        return decimal.Parse(asDouble.ToString());
    }

    /// <summary>获取JavaScript安全字符串</summary>
    public static string ToJsSafeString(this string? str)
    {
        if (string.IsNullOrEmpty(str)) return string.Empty;
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    /// <summary>URL编码</summary>
    public static string UrlEncode(this string? content, Encoding? encoding = null)
    {
        if (string.IsNullOrEmpty(content)) return string.Empty;
        encoding ??= Encoding.UTF8;
        return Uri.EscapeDataString(content);
    }

    /// <summary>转换为其他编码</summary>
    public static string ToEncoding(this string? content, Encoding? encoding = null)
    {
        if (string.IsNullOrEmpty(content) || encoding == null) return content ?? string.Empty;
        return encoding.GetString(encoding.GetBytes(content));
    }

    #endregion
}