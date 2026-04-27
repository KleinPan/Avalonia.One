namespace One.Update.Abstractions.Enums;

/// <summary>
/// 更新状态枚举，用于上报更新执行结果。
/// 客户端在更新完成后将状态报告给服务端，便于运维追踪更新情况。
/// </summary>
public enum UpdateStatus
{
    /// <summary>更新成功</summary>
    Success = 1,

    /// <summary>更新失败</summary>
    Failed = 2,

    /// <summary>更新已回滚（更新失败后自动恢复到更新前状态）</summary>
    RolledBack = 3
}
