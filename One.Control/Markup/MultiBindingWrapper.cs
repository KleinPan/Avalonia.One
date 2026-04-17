using Avalonia.Data;
using Avalonia.Data.Converters;

namespace One.Control.Markup;

/// <summary>
/// 主要目的就是限制 Converter 只允许赋值一次
/// </summary>
public abstract class MultiBindingWrapper 
{
    private readonly MultiBinding _binding = new();

    /// <summary>
    /// 限制 Converter 只允许赋值一次
    /// </summary>
    protected IMultiValueConverter? Converter
    {
        get => _binding.Converter;
        set
        {
            if (_binding.Converter != null)
            {
                throw new InvalidOperationException($"The {GetType().Name}.Converter property is readonly.");
            }

            _binding.Converter = value;
        }
    }

    protected BindingMode Mode
    {
        get => _binding.Mode;
        set => _binding.Mode = value;
    }

    protected int BindingCount => _binding.Bindings.Count;

    protected void AddBinding(BindingBase binding) => _binding.Bindings.Add(binding);

    public MultiBinding ToMultiBinding() => _binding;
}
