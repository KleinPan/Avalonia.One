﻿using Avalonia.Data;
using Avalonia.Data.Converters;

using One.Control.Converters.I18n;

namespace One.Control.Markup.I18n;

public class I18nBinding : MultiBindingExtensionBase
{
    public I18nBinding(object key)
    {
        Mode = BindingMode.OneWay;
        Converter = new I18nConverter(this);
        KeyConverter = new I18nKeyConverter();
        ValueConverter = new I18nValueConverter();
        Args = new ArgCollection(this);

        var cultureBinding = new Binding { Source = I18nManager.Instance, Path = nameof(I18nManager.Culture) };
        Bindings.Add(cultureBinding);

        Key = key;
        if (Key is not BindingBase keyBinding)
        {
            keyBinding = new Binding { Source = key };
        }

        Bindings.Add(keyBinding);
    }

    public I18nBinding(object key, params object[]? args) : this(key)
    {
        if (args is not { Length: > 0 })
        {
            return;
        }

        foreach (object arg in args)
        {
            Args.Add(arg);
        }
    }

    public object Key { get; }

    public ArgCollection Args { get; }

    public IValueConverter KeyConverter { get; set; }

    public IValueConverter ValueConverter { get; set; }
}