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
    public IReadOnlyList<ToolPageDescriptor> Descriptors { get; }

    public ToolPageRegistry()
    {
        Descriptors = DiscoverDescriptors().OrderBy(d => d.Order).ToList();
    }

    public static void RegisterAll(IServiceCollection services)
    {
        foreach (var d in DiscoverDescriptors())
        {
            services.AddSingleton(d.ViewModelType);
        }
        services.AddSingleton<ToolPageRegistry>();
    }

    private static IEnumerable<ToolPageDescriptor> DiscoverDescriptors()
    {
        var asm = typeof(ToolPageRegistry).Assembly;
        foreach (var t in asm.GetTypes())
        {
            var attr = t.GetCustomAttribute<ToolPageAttribute>();
            if (attr is null) continue;
            yield return new ToolPageDescriptor(
                ViewModelType: t,
                ViewType: attr.ViewType,
                Order: attr.Order,
                I18nKey: attr.I18nKey,
                IconKey: attr.IconKey,
                Dock: attr.Dock);
        }
    }
}
