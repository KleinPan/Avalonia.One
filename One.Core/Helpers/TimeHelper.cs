using System;
using System.Runtime.InteropServices;

namespace One.Base.Helpers;

[StructLayout(LayoutKind.Sequential)]
public struct SYSTEMTIME
{
    public short Year;
    public short Month;
    public short DayOfWeek;
    public short Day;
    public short Hour;
    public short Minute;
    public short Second;
    public short Millisecond;
}

public class TimeHelper
{
    #region Windows

    [DllImportAttribute("Kernel32.dll")]
    private static extern void GetLocalTime(SYSTEMTIME st);

    [DllImportAttribute("Kernel32.dll")]
    private static extern void SetLocalTime(SYSTEMTIME st);

    public static void SetSystemTime(DateTime dateTime)
    {
        SYSTEMTIME MySystemTime = new SYSTEMTIME();

        //GetLocalTime(MySystemTime);

        MySystemTime.Year = (short)dateTime.Year;

        MySystemTime.Month = (short)dateTime.Month;

        MySystemTime.Day = (short)dateTime.Day;
        MySystemTime.DayOfWeek = (short)dateTime.DayOfWeek;

        MySystemTime.Hour = (short)(dateTime.Hour);

        MySystemTime.Minute = (short)dateTime.Minute;

        MySystemTime.Second = (short)dateTime.Second;
        MySystemTime.Millisecond = (short)dateTime.Millisecond;

        try
        {
            SetLocalTime(MySystemTime);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    #endregion Windows

    #region Unix

    private static DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static string GetUnixTimestamp()
    {
        // 获取当前时间的 UTC 时间
        DateTime currentTime = DateTime.UtcNow;

        // 计算当前时间与 Unix 时间起点（1970年1月1日）之间的时间间隔（秒数）
        TimeSpan timeSpan = currentTime - unixEpoch;

        // 将时间间隔转换为秒数并取整
        long unixTimestamp = (long)timeSpan.TotalSeconds;

        // 输出 Unix 时间戳
        return unixTimestamp.ToString();
    }

    /// <summary>转换为对应的unix时间戳</summary>
    /// <param name="timestamp"></param>
    /// <param name="fromBase"></param>
    /// <returns></returns>
    public static DateTime GetUnixTime(string timestamp, int fromBase = 16)
    {
        // 输入正确的 Unix 时间戳
        var unixTimestamp = Convert.ToInt64(timestamp, fromBase);

        // 将 Unix 时间戳转换为 DateTime 对象

        DateTime dateTime = unixEpoch.AddSeconds(unixTimestamp);

        // 打印出转换后的日期时间
        return dateTime.ToLocalTime();
    }

    #endregion Unix
}