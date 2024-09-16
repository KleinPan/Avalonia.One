﻿using Avalonia;
using Avalonia.Xaml.Interactivity;

using AvaloniaEdit;

namespace One.Toolbox.Behaviors
{
    public class DocumentTextBindingBehavior : Behavior<TextEditor>
    {
        private TextEditor _textEditor = null;

        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<DocumentTextBindingBehavior, string>(nameof(Text));

        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is TextEditor textEditor)
            {
                _textEditor = textEditor;
                _textEditor.TextChanged += TextChanged;
                this.GetObservable(TextProperty).Subscribe(TextPropertyChanged);
                //_textEditor.TextArea.Options.ShowEndOfLine = false;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (_textEditor != null)
            {
                _textEditor.TextChanged -= TextChanged;
            }
        }

        private void TextChanged(object sender, EventArgs eventArgs)
        {
            if (_textEditor != null && _textEditor.Document != null)
            {
                Text = _textEditor.Document.Text;
            }
        }

        private void TextPropertyChanged(string text)
        {
            if (_textEditor != null && _textEditor.Document != null && text != null /*&& !string.IsNullOrEmpty(text)*/)
            {
                int caretOffset = _textEditor.CaretOffset;
                _textEditor.Document.Text = text;

                if (caretOffset > _textEditor.Document.TextLength)
                {
                    _textEditor.CaretOffset = _textEditor.Document.TextLength;
                }
                else
                {
                    _textEditor.CaretOffset = caretOffset;
                }
                //_textEditor.CaretOffset = caretOffset;
            }
        }
    }
}