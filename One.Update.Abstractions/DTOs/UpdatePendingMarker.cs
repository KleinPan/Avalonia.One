namespace One.Update.Abstractions.DTOs;

/// <summary>
/// 更新待决标记文件 DTO。
/// 主程序在启动升级程序前写入此标记文件到临时目录，
/// 用于崩溃回滚检测：如果更新后主程序无法正常启动，升级程序将检测到此标记并自动回滚。
/// 
/// 工作流程：
///   1. 主程序写入 update_pending.json
///   2. 升级程序执行文件替换
///   3. 升级程序启动主程序
///   4. 主程序启动成功 → 删除标记文件 + 删除备份目录
///   5. 主程序启动失败 → 升级程序检测到标记文件仍存在 → 自动回滚
/// </summary>
public class UpdatePendingMarker
{
    /// <summary>
    /// 备份目录路径，回滚时从此目录恢复文件。
    /// </summary>
    public string BackupPath { get; set; } = string.Empty;

    /// <summary>
    /// 目标版本号，用于日志记录。
    /// </summary>
    public string TargetVersion { get; set; } = string.Empty;

    /// <summary>
    /// 安装目录路径，回滚时将备份文件恢复到此目录。
    /// </summary>
    public string InstallPath { get; set; } = string.Empty;

    /// <summary>
    /// 标记写入时间（UTC），用于判断标记是否过期。
    /// 如果标记超过一定时间（如 30 分钟）仍存在，说明更新后主程序可能无法启动。
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 主程序可执行文件名，用于回滚后重新启动主程序。
    /// </summary>
    public string AppName { get; set; } = string.Empty;
}
