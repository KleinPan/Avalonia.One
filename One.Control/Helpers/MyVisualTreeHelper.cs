using Avalonia.LogicalTree;

namespace One.Control.Helpers;

public class MyVisualTreeHelper
{
    public static T FindControlByName<T>(Avalonia.Controls.Control parent, string name) where T : Avalonia.Controls.Control
    {
        if (parent is T t && t.Name == name)
        {
            return t;
        }

        foreach (var child in parent.GetLogicalChildren())
        {
            if (child is Avalonia.Controls.Control control)
            {
                var foundControl = FindControlByName<T>(control, name);
                if (foundControl != null)
                {
                    return foundControl;
                }
            }
        }

        return null;
    }
}