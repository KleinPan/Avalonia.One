namespace One.Update.Abstractions.Interfaces;

/// <summary>
/// 黑名单过滤器接口。
/// 在更新过程中过滤不需要覆盖的文件和目录。
/// 
/// 过滤规则：
///   - BlackFiles：精确匹配文件名，如 "client.id" → 跳过所有名为 client.id 的文件
///   - BlackFormats：匹配文件扩展名，如 ".log" → 跳过所有 .log 文件
///   - SkipDirectories：匹配目录名，如 "logs" → 跳过整个 logs 目录
/// 
/// 用途：
///   - 保留用户配置文件（如 client.id、user.config）
///   - 保留运行时生成的文件（如日志、缓存）
///   - 保留特定数据目录（如 userdata、backup）
/// </summary>
public interface IBlackListFilter
{
    /// <summary>
    /// 判断文件是否在黑名单中（应跳过）。
    /// </summary>
    /// <param name="filePath">文件完整路径或相对路径</param>
    /// <returns>true 表示应跳过此文件，false 表示不跳过</returns>
    bool IsBlacklisted(string filePath);

    /// <summary>
    /// 判断目录是否在跳过列表中。
    /// </summary>
    /// <param name="directoryPath">目录完整路径</param>
    /// <returns>true 表示应跳过此目录，false 表示不跳过</returns>
    bool IsSkippedDirectory(string directoryPath);

    /// <summary>
    /// 添加黑名单文件名列表。
    /// </summary>
    /// <param name="fileNames">要跳过的文件名列表</param>
    void AddBlackFiles(IEnumerable<string> fileNames);

    /// <summary>
    /// 添加黑名单文件扩展名列表。
    /// </summary>
    /// <param name="formats">要跳过的文件扩展名列表，如 ".log"、".tmp"</param>
    void AddBlackFormats(IEnumerable<string> formats);

    /// <summary>
    /// 添加跳过的目录名列表。
    /// </summary>
    /// <param name="directoryNames">要跳过的目录名列表</param>
    void AddSkipDirectories(IEnumerable<string> directoryNames);

    /// <summary>
    /// 当前黑名单文件名集合（只读）。
    /// 用于构建 InstallerArgs 时导出黑名单配置。
    /// </summary>
    IReadOnlyCollection<string> BlackFileNames { get; }

    /// <summary>
    /// 当前黑名单扩展名集合（只读）。
    /// 用于构建 InstallerArgs 时导出黑名单配置。
    /// </summary>
    IReadOnlyCollection<string> BlackFormats { get; }

    /// <summary>
    /// 当前跳过目录名集合（只读）。
    /// 用于构建 InstallerArgs 时导出黑名单配置和备份时跳过目录。
    /// </summary>
    IReadOnlyCollection<string> SkipDirectoryNames { get; }
}
