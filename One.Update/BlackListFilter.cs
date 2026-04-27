using One.Update.Abstractions.Interfaces;

namespace One.Update;

/// <summary>
/// 黑名单过滤器实现。
/// 在更新过程中根据文件名、扩展名和目录名过滤文件，防止覆盖用户配置和运行时数据。
/// 
/// 匹配规则：
///   - BlackFiles：不区分大小写匹配文件名（含扩展名），如 "client.id" 匹配所有名为 client.id 的文件
///   - BlackFormats：不区分大小写匹配扩展名，如 ".log" 匹配所有 .log 文件
///   - SkipDirectories：不区分大小写匹配目录名，如 "logs" 匹配路径中包含 logs 的目录
/// </summary>
public class BlackListFilter : IBlackListFilter
{
    private readonly HashSet<string> _blackFiles = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _blackFormats = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _skipDirectories = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IReadOnlyCollection<string> BlackFileNames => _blackFiles;

    /// <inheritdoc />
    public IReadOnlyCollection<string> BlackFormats => _blackFormats;

    /// <inheritdoc />
    public IReadOnlyCollection<string> SkipDirectoryNames => _skipDirectories;

    /// <inheritdoc />
    public bool IsBlacklisted(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        var fileName = Path.GetFileName(filePath);
        var extension = Path.GetExtension(filePath);

        if (_blackFiles.Contains(fileName))
            return true;

        if (!string.IsNullOrEmpty(extension) && _blackFormats.Contains(extension))
            return true;

        return false;
    }

    /// <inheritdoc />
    public bool IsSkippedDirectory(string directoryPath)
    {
        ArgumentNullException.ThrowIfNull(directoryPath);

        var dirName = Path.GetFileName(directoryPath);
        if (!string.IsNullOrEmpty(dirName) && _skipDirectories.Contains(dirName))
            return true;

        foreach (var skipDir in _skipDirectories)
        {
            if (directoryPath.Contains(skipDir, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void AddBlackFiles(IEnumerable<string> fileNames)
    {
        ArgumentNullException.ThrowIfNull(fileNames);
        foreach (var name in fileNames)
        {
            if (!string.IsNullOrWhiteSpace(name))
                _blackFiles.Add(name);
        }
    }

    /// <inheritdoc />
    public void AddBlackFormats(IEnumerable<string> formats)
    {
        ArgumentNullException.ThrowIfNull(formats);
        foreach (var format in formats)
        {
            var normalized = format.StartsWith('.') ? format : $".{format}";
            _blackFormats.Add(normalized);
        }
    }

    /// <inheritdoc />
    public void AddSkipDirectories(IEnumerable<string> directoryNames)
    {
        ArgumentNullException.ThrowIfNull(directoryNames);
        foreach (var name in directoryNames)
        {
            if (!string.IsNullOrWhiteSpace(name))
                _skipDirectories.Add(name);
        }
    }
}
