using One.Update.Abstractions.DTOs;

namespace One.Update.Abstractions.Interfaces;

/// <summary>
/// 更新安装器接口。
/// 由升级程序（如 Carina.Upgrade）实现，只负责文件替换、清理和启动主程序。
/// 下载、解压、备份由主程序在启动升级程序前完成。
/// </summary>
public interface IUpdateInstaller
{
    /// <summary>
    /// 执行更新安装。
    /// 将解压目录的文件覆盖到安装目录，然后清理临时文件。
    /// </summary>
    /// <param name="args">安装参数，包含安装路径、解压目录、备份路径等</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>安装是否成功</returns>
    Task<bool> InstallAsync(InstallerArgs args, CancellationToken cancellationToken = default);

    /// <summary>
    /// 执行回滚操作。
    /// 从备份目录恢复文件到安装目录。
    /// </summary>
    /// <param name="args">安装参数，包含备份路径和安装路径</param>
    /// <returns>回滚是否成功</returns>
    Task<bool> RollbackAsync(InstallerArgs args);

    /// <summary>
    /// 启动主程序。
    /// 安装完成后调用此方法启动主程序。
    /// </summary>
    /// <param name="args">安装参数，包含主程序名称和安装路径</param>
    void StartApp(InstallerArgs args);
}
