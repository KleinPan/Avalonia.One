using System;

namespace One.Base.Attributes;

/// <summary> 导出的测试项名称对应的设置 </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CustomerSettingAttribute : Attribute
{
    public string TargetTestItemName { get; set; }

    public CustomerSettingAttribute(string color)
    {
        TargetTestItemName = color;
    }
}