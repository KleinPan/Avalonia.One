using System.Diagnostics;

namespace One.Update.Abstractions.DTOs;

public class UpdateConfig
{
    public string ServerUrl { get; set; } = string.Empty;

    public string? ReportUrl { get; set; }

    public string CurrentVersion { get; set; } = string.Empty;

    public int AppType { get; set; } = (int)Enums.AppType.ClientApp;

    public string AppKey { get; set; } = string.Empty;

    public string InstallPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

    public string InstallerExeName { get; set; } = string.Empty;

    public string AppName { get; set; } = string.Empty;

    public List<string> BlackFiles { get; set; } = new();

    public List<string> BlackFormats { get; set; } = new();

    public List<string> SkipDirectories { get; set; } = new();

    public long? MaxDownloadSpeed { get; set; }

    public static string DetectCurrentVersion()
    {
        return FileVersionInfo.GetVersionInfo(Environment.ProcessPath ?? AppContext.BaseDirectory).FileVersion ?? "0.0.0";
    }

    public static string DeriveAppName(string installerExeName)
    {
        return installerExeName
            .Replace(".Upgrade", ".Avalonia", StringComparison.OrdinalIgnoreCase)
            .Replace(".exe", ".dll", StringComparison.OrdinalIgnoreCase);
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ServerUrl) || !Uri.IsWellFormedUriString(ServerUrl, UriKind.Absolute))
            throw new ArgumentException("Invalid ServerUrl");

        if (string.IsNullOrWhiteSpace(CurrentVersion))
            CurrentVersion = DetectCurrentVersion();

        if (string.IsNullOrWhiteSpace(AppKey))
            throw new ArgumentException("AppKey cannot be empty");

        if (string.IsNullOrWhiteSpace(InstallPath))
            InstallPath = AppDomain.CurrentDomain.BaseDirectory;

        if (string.IsNullOrWhiteSpace(InstallerExeName))
            throw new ArgumentException("InstallerExeName cannot be empty");

        if (string.IsNullOrWhiteSpace(AppName))
            AppName = DeriveAppName(InstallerExeName);
    }
}
