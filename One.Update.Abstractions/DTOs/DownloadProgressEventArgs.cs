namespace One.Update.Abstractions.DTOs;

public class DownloadProgressEventArgs : EventArgs
{
    public double Progress { get; }
    public string? Version { get; }
    public long BytesPerSecond { get; }
    public TimeSpan? RemainingTime { get; }

    public DownloadProgressEventArgs(double progress, string? version = null, long bytesPerSecond = 0, TimeSpan? remainingTime = null)
    {
        Progress = progress;
        Version = version;
        BytesPerSecond = bytesPerSecond;
        RemainingTime = remainingTime;
    }

    public string FormatSpeed()
    {
        if (BytesPerSecond <= 0) return string.Empty;
        if (BytesPerSecond < 1024) return $"{BytesPerSecond} B/s";
        if (BytesPerSecond < 1024 * 1024) return $"{BytesPerSecond / 1024.0:F1} KB/s";
        return $"{BytesPerSecond / 1024.0 / 1024.0:F1} MB/s";
    }

    public string FormatRemainingTime()
    {
        if (RemainingTime == null) return string.Empty;
        var ts = RemainingTime.Value;
        if (ts.TotalSeconds < 1) return string.Empty;
        if (ts.TotalMinutes < 1) return $"{(int)ts.TotalSeconds}s";
        if (ts.TotalHours < 1) return $"{(int)ts.TotalMinutes}m {(int)ts.Seconds}s";
        return $"{(int)ts.TotalHours}h {(int)ts.Minutes}m";
    }
}
