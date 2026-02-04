// Shouldn't get really used at all unless you're looking for a more loose and misorganized method
namespace Crucible.Types;

public class BaseRegistry<T>
{
    private readonly List<T> _items = new();

    public void Register(T item)
    {
        _items.Add(item);
    }

    public IEnumerable<T> GetAll()
    {
        return _items;
    }

    public bool Contains(T item)
    {
        return _items.Contains(item);
    }
}