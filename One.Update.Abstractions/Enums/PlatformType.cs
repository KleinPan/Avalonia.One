namespace One.Update.Abstractions.Enums;

/// <summary>
/// 平台类型枚举，标识客户端运行的目标操作系统。
/// 服务端可根据平台返回对应的更新包。
/// </summary>
public enum PlatformType
{
    /// <summary>Windows 平台</summary>
    Windows = 1,

    /// <summary>Linux 平台</summary>
    Linux = 2
}
