using Avalonia.Controls;

using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace One.Toolbox.Services.Navigation;

public sealed record ToolPageDescriptor(
    Type ViewModelType,
    Type ViewType,
    int Order,
    string I18nKey,
    string IconKey,
    Dock Dock);

public sealed class ToolPageRegistry
{
    // 缓存反射扫描结果：RegisterAll 与本类构造函数各扫描一次全程序集，重复的 asm.GetTypes() 开销无必要
    private static IReadOnlyList<ToolPageDescriptor>? _cachedDescriptors;

    public IReadOnlyList<ToolPageDescriptor> Descriptors { get; }

    public ToolPageRegistry()
    {
        Descriptors = GetDescriptors();
    }

    public static void RegisterAll(IServiceCollection services)
    {
        foreach (var d in GetDescriptors())
        {
            services.AddSingleton(d.ViewModelType);
        }
        services.AddSingleton<ToolPageRegistry>();
    }

    private static IReadOnlyList<ToolPageDescriptor> GetDescriptors()
    {
        if (_cachedDescriptors is null)
        {
            var asm = typeof(ToolPageRegistry).Assembly;
            _cachedDescriptors = asm.GetTypes()
                .Select(t => (Type: t, Attr: t.GetCustomAttribute<ToolPageAttribute>()))
                .Where(x => x.Attr is not null)
                .Select(x => new ToolPageDescriptor(
                    ViewModelType: x.Type,
                    ViewType: x.Attr!.ViewType,
                    Order: x.Attr.Order,
                    I18nKey: x.Attr.I18nKey,
                    IconKey: x.Attr.IconKey,
                    Dock: x.Attr.Dock))
                .OrderBy(d => d.Order)
                .ToList();
        }

        return _cachedDescriptors;
    }
}
