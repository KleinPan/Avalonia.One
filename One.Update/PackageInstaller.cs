using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;
using One.Update.Abstractions.DTOs;
using One.Update.Abstractions.Enums;
using One.Update.Abstractions.Interfaces;

namespace One.Update;

public class PackageInstaller : IUpdateInstaller
{
    private readonly BlackListFilter _blackListFilter;
    private readonly HttpClient _reportHttpClient;
    private HashSet<string> _protectedFiles = new(StringComparer.OrdinalIgnoreCase);

    public PackageInstaller(HttpClient reportHttpClient)
    {
        _blackListFilter = new BlackListFilter();
        _reportHttpClient = reportHttpClient;
    }

    public async Task<bool> InstallAsync(InstallerArgs args, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);

        InitializeBlackList(args);
        InitializeProtectedFiles(args);

        var extractPath = Path.Combine(args.TempPath, "extracted");

        try
        {
            ApplyUpdate(extractPath, args.InstallPath);

            CleanupTempFiles(args.TempPath, extractPath);

            try
            {
                if (Directory.Exists(args.BackupPath))
                    BackupService.DeleteDirectory(args.BackupPath);
            }
            catch
            {
            }

            await ReportUpdateResultAsync(args, UpdateStatus.Success);

            return true;
        }
        catch (Exception ex)
        {
            var rolledBack = false;
            try
            {
                if (Directory.Exists(args.BackupPath))
                {
                    BackupService.Restore(args.BackupPath, args.InstallPath);
                    rolledBack = true;
                }
            }
            catch
            {
            }

            await ReportUpdateResultAsync(
                args,
                rolledBack ? UpdateStatus.RolledBack : UpdateStatus.Failed,
                ex.Message);

            return false;
        }
    }

    public Task<bool> RollbackAsync(InstallerArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (!Directory.Exists(args.BackupPath))
            return Task.FromResult(false);

        try
        {
            InitializeProtectedFiles(args);
            BackupService.Restore(args.BackupPath, args.InstallPath);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public void StartApp(InstallerArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var appPath = Path.Combine(args.InstallPath, args.AppName);
        if (File.Exists(appPath))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = appPath,
                UseShellExecute = true
            });
        }

        Environment.Exit(0);
    }

    private void InitializeBlackList(InstallerArgs args)
    {
        if (args.BlackFiles?.Count > 0)
            _blackListFilter.AddBlackFiles(args.BlackFiles);

        if (args.BlackFormats?.Count > 0)
            _blackListFilter.AddBlackFormats(args.BlackFormats);

        if (args.SkipDirectories?.Count > 0)
            _blackListFilter.AddSkipDirectories(args.SkipDirectories);
    }

    private void InitializeProtectedFiles(InstallerArgs args)
    {
        _protectedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(args.InstallerExeName))
        {
            _protectedFiles.Add(args.InstallerExeName);

            var installerBaseName = Path.GetFileNameWithoutExtension(args.InstallerExeName);
            _protectedFiles.Add($"{installerBaseName}.dll");
        }

        _protectedFiles.Add("One.Update.dll");
        _protectedFiles.Add("One.Update.Abstractions.dll");
    }

    private void ApplyUpdate(string sourceDir, string targetDir)
    {
        if (!Directory.Exists(targetDir))
            Directory.CreateDirectory(targetDir);

        var newFiles = GetAllFiles(sourceDir, sourceDir);
        var existingFiles = GetAllFiles(targetDir, targetDir);

        foreach (var relativePath in newFiles.Keys)
        {
            if (_blackListFilter.IsBlacklisted(relativePath))
                continue;

            if (IsProtectedFile(relativePath))
                continue;

            var sourceFilePath = newFiles[relativePath];
            var targetFilePath = Path.Combine(targetDir, relativePath);

            var targetFileDir = Path.GetDirectoryName(targetFilePath);
            if (!string.IsNullOrEmpty(targetFileDir) && !Directory.Exists(targetFileDir))
                Directory.CreateDirectory(targetFileDir);

            if (File.Exists(targetFilePath))
            {
                File.SetAttributes(targetFilePath, FileAttributes.Normal);
                try
                {
                    File.Delete(targetFilePath);
                }
                catch (Exception)
                {
                    var oldFilePath = targetFilePath + ".old";
                    if (File.Exists(oldFilePath))
                    {
                        File.SetAttributes(oldFilePath, FileAttributes.Normal);
                        File.Delete(oldFilePath);
                    }
                    File.Move(targetFilePath, oldFilePath);
                }
            }

            File.Copy(sourceFilePath, targetFilePath, true);
        }

        var newRelativePaths = new HashSet<string>(newFiles.Keys, StringComparer.OrdinalIgnoreCase);
        foreach (var relativePath in existingFiles.Keys)
        {
            if (_blackListFilter.IsBlacklisted(relativePath))
                continue;

            if (IsProtectedFile(relativePath))
                continue;

            if (!newRelativePaths.Contains(relativePath))
            {
                var fileToDelete = Path.Combine(targetDir, relativePath);
                if (File.Exists(fileToDelete))
                {
                    File.SetAttributes(fileToDelete, FileAttributes.Normal);
                    try
                    {
                        File.Delete(fileToDelete);
                    }
                    catch (Exception)
                    {
                        var oldFilePath = fileToDelete + ".old";
                        if (File.Exists(oldFilePath))
                        {
                            File.SetAttributes(oldFilePath, FileAttributes.Normal);
                            File.Delete(oldFilePath);
                        }
                        File.Move(fileToDelete, oldFilePath);
                    }
                }
            }
        }
    }

    private bool IsProtectedFile(string relativePath)
    {
        var fileName = Path.GetFileName(relativePath);
        return _protectedFiles.Contains(fileName);
    }

    private Dictionary<string, string> GetAllFiles(string directory, string rootDirectory)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                var relativePath = Path.GetRelativePath(rootDirectory, file);
                result[relativePath] = file;
            }

            foreach (var dir in Directory.GetDirectories(directory))
            {
                if (_blackListFilter.IsSkippedDirectory(dir))
                    continue;

                foreach (var kvp in GetAllFiles(dir, rootDirectory))
                    result[kvp.Key] = kvp.Value;
            }
        }
        catch
        {
        }

        return result;
    }

    private static void CleanupTempFiles(string tempPath, string extractPath)
    {
        try
        {
            if (Directory.Exists(extractPath))
                BackupService.DeleteDirectory(extractPath);

            if (Directory.Exists(tempPath))
                BackupService.DeleteDirectory(tempPath);
        }
        catch
        {
        }
    }

    public static void CleanupOldFiles(string directory)
    {
        try
        {
            foreach (var oldFile in Directory.GetFiles(directory, "*.old", SearchOption.AllDirectories))
            {
                try
                {
                    File.SetAttributes(oldFile, FileAttributes.Normal);
                    File.Delete(oldFile);
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
    }

    private async Task ReportUpdateResultAsync(InstallerArgs args, UpdateStatus status, string? errorMessage = null)
    {
        if (string.IsNullOrEmpty(args.ReportUrl))
            return;

        foreach (var recordId in args.RecordIds)
        {
            try
            {
                var request = new UpdateReportRequest
                {
                    RecordId = recordId,
                    Status = (int)status,
                    AppType = (int)AppType.ClientApp,
                    ErrorMessage = errorMessage
                };

                await _reportHttpClient.PostAsJsonAsync(args.ReportUrl, request);
            }
            catch
            {
            }
        }
    }

    public static InstallerArgs ParseArgs(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        const string prefix = "--update-args";

        for (var i = 0; i < args.Length - 1; i++)
        {
            if (!string.Equals(args[i], prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            var base64Json = args[i + 1].Trim('"');
            var jsonBytes = Convert.FromBase64String(base64Json);
            var json = System.Text.Encoding.UTF8.GetString(jsonBytes);

            return JsonSerializer.Deserialize<InstallerArgs>(json)
                ?? throw new InvalidOperationException("Failed to deserialize InstallerArgs.");
        }

        throw new InvalidOperationException($"Missing '{prefix}' argument.");
    }
}
