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
    public static readonly AttachedProperty<ICommand> DropFilesCommandProperty = AvaloniaProperty.RegisterAttached<DragDropHelper, Interactive, ICommand>(
        "DropFilesCommand",
        default(ICommand),
        false,
        BindingMode.OneTime
    );

    /// <summary><see cref="DropFilesCommandProperty"/> 的变化事件处理程序。</summary>
    private static void HandleCommandChanged(Interactive interactElem, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is ICommand commandValue)
        {
            // 添加非空值
            interactElem.AddHandler(DragDrop.DropEvent, Handler);
        }
        else
        {
            // 删除之前的值
            interactElem.RemoveHandler(DragDrop.DropEvent, Handler);
        }
        // 本地处理函数
        static void Handler(object s, DragEventArgs e)
        {
            if (s is Interactive interactElem)
            {
                // 这是如何从GUI元素中获取参数的方法。

                ICommand commandValue = interactElem.GetValue(DropFilesCommandProperty);

                var b = e.Data.GetFiles();

                var c = b.Select(x => x.Path).ToList();

                if (commandValue?.CanExecute(c) == true)
                {
                    commandValue.Execute(c);
                }
            }
        }
    }

    /// <summary>附加属性 <see cref="DropFilesCommandProperty"/> 的访问器。</summary>
    public static void SetDropFilesCommand(AvaloniaObject element, ICommand commandValue)
    {
        element.SetValue(DropFilesCommandProperty, commandValue);
    }

    /// <summary>附加属性 <see cref="DropFilesCommandProperty"/> 的访问器。</summary>
    public static ICommand GetDropFilesCommand(AvaloniaObject element)
    {
        return element.GetValue(DropFilesCommandProperty);
    }
}