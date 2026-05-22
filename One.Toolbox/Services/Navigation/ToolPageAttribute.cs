using Avalonia.Controls;

namespace One.Toolbox.Services.Navigation;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ToolPageAttribute : Attribute
{
    public int Order { get; }
    public string I18nKey { get; }
    public string IconKey { get; }
    public Type ViewType { get; }
    public Dock Dock { get; set; } = Dock.Top;

    public ToolPageAttribute(int order, string i18nKey, string iconKey, Type viewType)
    {
        Order = order;
        I18nKey = i18nKey;
        IconKey = iconKey;
        ViewType = viewType;
    }
}
