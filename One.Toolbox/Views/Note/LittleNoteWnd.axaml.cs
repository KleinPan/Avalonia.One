using Avalonia.Controls;
using Avalonia.Input;

namespace One.Toolbox.Views.Note
{
    public partial class LittleNoteWnd : Window
    {
        public LittleNoteWnd()
        {
            InitializeComponent();
        }

        private void Border_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (e.Pointer.Type == PointerType.Mouse)
                this.BeginMoveDrag(e);
        }
    }
}