namespace One.Update.Abstractions.DTOs;

/// <summary>
/// 更新结果上报请求 DTO。
/// 客户端在更新完成后（无论成功或失败），将结果上报给服务端。
/// 服务端可据此统计更新成功率、识别问题版本等。
/// 
/// 协议约定：
///   - 请求路径：POST /api/update/report
///   - Content-Type：application/json
/// </summary>
public class UpdateReportRequest
{
    /// <summary>
    /// 更新记录 ID，与 <see cref="UpdatePackageInfo.RecordId"/> 对应。
    /// 服务端通过此 ID 关联更新检查记录和更新结果。
    /// </summary>
    public int RecordId { get; set; }

    /// <summary>
    /// 更新执行状态，参见 <see cref="Enums.UpdateStatus"/>。
    ///   - Success(1)：更新成功
    ///   - Failed(2)：更新失败
    ///   - RolledBack(3)：更新失败后已回滚
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 更新类型，参见 <see cref="Enums.AppType"/>。
    ///   - ClientApp(1)：主程序更新
    ///   - UpgradeApp(2)：升级程序更新
    /// </summary>
    public int? AppType { get; set; }

    /// <summary>
    /// 错误信息（仅在更新失败时填写）。
    /// 包含异常类型和消息，便于服务端排查问题。
    /// </summary>
    public string? ErrorMessage { get; set; }
}
