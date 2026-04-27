namespace One.Update.Abstractions.DTOs;

public class InstallerArgs
{
    public string AppName { get; set; } = string.Empty;

    public string InstallPath { get; set; } = string.Empty;

    public string CurrentVersion { get; set; } = string.Empty;

    public string TargetVersion { get; set; } = string.Empty;

    public string Format { get; set; } = "zip";

    public List<UpdatePackageInfo> Packages { get; set; } = new();

    public List<string> PackageFiles { get; set; } = new();

    public string TempPath { get; set; } = string.Empty;

    public string BackupPath { get; set; } = string.Empty;

    public string ReportUrl { get; set; } = string.Empty;

    public List<int> RecordIds { get; set; } = new();

    public string AppKey { get; set; } = string.Empty;

    public string InstallerExeName { get; set; } = string.Empty;

    public List<string> BlackFiles { get; set; } = new();

    public List<string> BlackFormats { get; set; } = new();

    public List<string> SkipDirectories { get; set; } = new();
}
