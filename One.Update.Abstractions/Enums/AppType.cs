namespace One.Update.Abstractions.Enums;

public enum AppType
{
    ClientApp = 1,

    [System.Obsolete("升级程序已合并到主程序包中，不再独立更新")]
    UpgradeApp = 2
}
