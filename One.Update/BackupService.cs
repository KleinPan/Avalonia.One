using One.Update.Abstractions.Interfaces;

namespace One.Update;

public static class BackupService
{
    public static void Backup(string sourcePath, string backupPath, IReadOnlyCollection<string>? skipDirectories = null, IBlackListFilter? blackListFilter = null)
    {
        ArgumentNullException.ThrowIfNull(sourcePath);
        ArgumentNullException.ThrowIfNull(backupPath);

        if (!Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException($"Source directory not found: {sourcePath}");

        if (backupPath.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Backup path '{backupPath}' cannot be inside source path '{sourcePath}'. This would cause recursive copy.");

        if (Directory.Exists(backupPath))
            DeleteDirectory(backupPath);

        Directory.CreateDirectory(backupPath);
        CopyDirectory(sourcePath, backupPath, skipDirectories ?? Array.Empty<string>(), blackListFilter);
    }

    /// <summary>
    /// 从备份目录回滚到安装目录。
    /// 更新失败时调用此方法恢复到更新前的状态。
    /// </summary>
    /// <param name="backupPath">备份目录路径</param>
    /// <param name="targetPath">安装目录路径</param>
    /// <exception cref="DirectoryNotFoundException">备份目录不存在时抛出</exception>
    public static void Restore(string backupPath, string targetPath)
    {
        ArgumentNullException.ThrowIfNull(backupPath);
        ArgumentNullException.ThrowIfNull(targetPath);

        if (!Directory.Exists(backupPath))
            throw new DirectoryNotFoundException($"Backup directory not found: {backupPath}");

        if (!Directory.Exists(targetPath))
            Directory.CreateDirectory(targetPath);

        CopyDirectory(backupPath, targetPath, Array.Empty<string>(), null);
    }

    /// <summary>
    /// 递归复制目录。
    /// </summary>
    /// <param name="sourceDir">源目录</param>
    /// <param name="targetDir">目标目录</param>
    /// <param name="skipDirectories">跳过的目录名列表</param>
    private static void CopyDirectory(string sourceDir, string targetDir, IReadOnlyCollection<string> skipDirectories, IBlackListFilter? blackListFilter)
    {
        foreach (var dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.TopDirectoryOnly))
        {
            var dirName = Path.GetFileName(dirPath);
            if (skipDirectories.Any(skip => dirName.Contains(skip, StringComparison.OrdinalIgnoreCase)))
                continue;

            if (blackListFilter != null && blackListFilter.IsSkippedDirectory(dirPath))
                continue;

            var newTargetDir = Path.Combine(targetDir, dirName);
            Directory.CreateDirectory(newTargetDir);
            CopyDirectory(dirPath, newTargetDir, skipDirectories, blackListFilter);
        }

        foreach (var filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.TopDirectoryOnly))
        {
            if (blackListFilter != null && blackListFilter.IsBlacklisted(filePath))
                continue;

            var newFilePath = Path.Combine(targetDir, Path.GetFileName(filePath));
            try
            {
                File.Copy(filePath, newFilePath, true);
            }
            catch
            {
                var oldFilePath = newFilePath + ".old";
                if (File.Exists(oldFilePath))
                {
                    File.SetAttributes(oldFilePath, FileAttributes.Normal);
                    File.Delete(oldFilePath);
                }
                File.Move(newFilePath, oldFilePath);
                File.Copy(filePath, newFilePath, true);
            }
        }
    }

    /// <summary>
    /// 递归删除目录及其所有内容。
    /// 删除前将文件属性设为 Normal，确保只读文件也能被删除。
    /// </summary>
    /// <param name="targetDir">要删除的目录路径</param>
    public static void DeleteDirectory(string targetDir)
    {
        if (!Directory.Exists(targetDir))
            return;

        foreach (var file in Directory.GetFiles(targetDir))
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (var dir in Directory.GetDirectories(targetDir))
            DeleteDirectory(dir);

        Directory.Delete(targetDir, false);
    }
}
