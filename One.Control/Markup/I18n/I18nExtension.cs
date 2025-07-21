using Avalonia.Markup.Xaml;

namespace One.Control.Markup.I18n;

public class I18nExtension : MarkupExtension
{
    public I18nExtension(object key)
    {
        Key = key;
    }

    /// <summary>可以这样写Running {0} operating system {markup:I18n {x:Static l:Language.RunningCountInfo},Win10}</summary>
    /// <param name="key"></param>
    /// <param name="args"></param>
    public I18nExtension(object key, params object[] args)
    {
        Key = key;
        Args = args;
    }

    public object Key { get; }
    public object[]? Args { get; }

    public override object ProvideValue(IServiceProvider serviceProvider) => new I18nBinding(Key, Args);
}