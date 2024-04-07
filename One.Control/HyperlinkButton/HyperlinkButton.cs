using Avalonia;
using Avalonia.Controls;

namespace One.Control;

public class HyperlinkButton : Button
{
    public static readonly DirectProperty<HyperlinkButton, string> UrlProperty
       = AvaloniaProperty.RegisterDirect<HyperlinkButton, string>(nameof(Url), o => o.Url, (o, v) => o.Url = v);

    private string _url;

    public static readonly DirectProperty<HyperlinkButton, string> AliasProperty
        = AvaloniaProperty.RegisterDirect<HyperlinkButton, string>(nameof(Alias), o => o.Alias, (o, v) => o.Alias = v);

    private string _alias;

    public string Url
    {
        get => _url;
        set
        {
            SetAndRaise(UrlProperty, ref _url, value);
            var textBlock = new TextBlock
            {
                Text = _url
            };
            if (string.IsNullOrEmpty(_alias))
            {
                Content = textBlock;
            }
            if (!string.IsNullOrEmpty(_url))
            {
                Classes.Add("hyperlink");
            }
        }
    }

    public string Alias
    {
        get => string.IsNullOrEmpty(_alias) ? _url : _alias;
        set
        {
            SetAndRaise(UrlProperty, ref _alias, value);
            var textBlock = new TextBlock
            {
                Text = string.IsNullOrEmpty(_alias) ? _url : _alias
            };

            Content = textBlock;
        }
    }
}