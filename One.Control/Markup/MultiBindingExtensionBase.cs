using Avalonia.Data;
using Avalonia.Data.Converters;

namespace One.Control.Markup;

public abstract class MultiBindingExtensionBase
{
    private readonly MultiBinding _binding = new();

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
