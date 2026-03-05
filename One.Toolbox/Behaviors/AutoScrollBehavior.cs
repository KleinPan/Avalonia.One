using Avalonia.Xaml.Interactivity;

using AvaloniaEdit;

namespace One.Toolbox.Behaviors;

public class AutoScrollBehavior : Behavior<TextEditor>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.TextChanged += OnTextChanged;
        }
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (AssociatedObject == null)
            return;

        AssociatedObject.CaretOffset =
            AssociatedObject.Document.TextLength;

        //AssociatedObject.ScrollToEnd();

        // 关键：滚动到 caret
        AssociatedObject.TextArea.Caret.BringCaretToView();
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.TextChanged -= OnTextChanged;
        }

        base.OnDetaching();
    }
}