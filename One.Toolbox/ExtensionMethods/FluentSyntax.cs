namespace One.Toolbox.ExtensionMethods;

public static class FluentSyntax
{
    public static ICollection<T> RemoveOne<T>(this ICollection<T> collection, Func<T, bool> search)
    {
        if (collection.FirstOrDefault(search) is { } x)
        {
            collection.Remove(x);
        }

        return collection;
    }
}