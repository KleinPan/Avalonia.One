using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

using System.Windows.Input;

namespace One.Control.Helpers;

public class DragDropHelper : AvaloniaObject
{
    static DragDropHelper()
    {
        DropFilesCommandProperty.Changed.AddClassHandler<Interactive>(HandleCommandChanged);
    }

    /// <summary>标识 <seealso cref="DropFilesCommandProperty"/> avalonia附加属性。</summary>
    /// <value>提供一个派生自 <see cref="ICommand"/> 的对象或绑定。</value>
    public static readonly AttachedProperty<ICommand?> DropFilesCommandProperty = AvaloniaProperty.RegisterAttached<DragDropHelper, Interactive, ICommand?>(
        "DropFilesCommand",
        default(ICommand?),
        false,
        BindingMode.OneTime
    );

    /// <summary><see cref="DropFilesCommandProperty"/> 的变化事件处理程序。</summary>
    private static void HandleCommandChanged(Interactive interactElem, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is ICommand commandValue)
        {
            // 添加非空值
            interactElem.AddHandler(DragDrop.DragOverEvent, DragOverHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
            interactElem.AddHandler(DragDrop.DropEvent, DropHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        }
        else
        {
            // 删除之前的值
            interactElem.RemoveHandler(DragDrop.DragOverEvent, DragOverHandler);
            interactElem.RemoveHandler(DragDrop.DropEvent, DropHandler);
        }

        static void DragOverHandler(object s, DragEventArgs e)
        {
            if (s is not Interactive interactElem)
            {
                return;
            }

            var commandValue = interactElem.GetValue(DropFilesCommandProperty);
            var files = e.Data.GetFiles();

            if (commandValue != null && files != null && files.Any())
            {
                e.DragEffects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        static void DropHandler(object s, DragEventArgs e)
        {
            if (s is not Interactive interactElem)
            {
                return;
            }

            ICommand? commandValue = interactElem.GetValue(DropFilesCommandProperty);

            var fileUris = e.Data.GetFiles()?.Select(x => x.Path).ToList();

            if (fileUris == null || fileUris.Count == 0)
            {
                return;
            }

            if (commandValue?.CanExecute(fileUris) == true)
            {
                commandValue.Execute(fileUris);
                e.Handled = true;
            }
        }
    }

    /// <summary>附加属性 <see cref="DropFilesCommandProperty"/> 的访问器。</summary>
    public static void SetDropFilesCommand(AvaloniaObject element, ICommand? commandValue)
    {
        element.SetValue(DropFilesCommandProperty, commandValue);
    }

    /// <summary>附加属性 <see cref="DropFilesCommandProperty"/> 的访问器。</summary>
    public static ICommand? GetDropFilesCommand(AvaloniaObject element)
    {
        return element.GetValue(DropFilesCommandProperty);
    }
}
