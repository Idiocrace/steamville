namespace Crucible.Types;

public class Registry<T>
{
    // ID: Parent Mod: Reference type
    private readonly Dictionary<string, Dictionary<string, T>> _items = new();
    
    public void Register(string modId, string itemId, T item)
    {
        if (!_items.ContainsKey(modId))
        {
            _items[modId] = new Dictionary<string, T>();
        }

        _items[modId][itemId] = item;
    }

    public T? Get(string modId, string itemId)
    {
        if (_items.ContainsKey(modId) && _items[modId].ContainsKey(itemId))
        {
            return _items[modId][itemId];
        }

        return default;
    }

    public bool Contains(string modId, string itemId)
    {
        return _items.ContainsKey(modId) && _items[modId].ContainsKey(itemId);
    }

    public IEnumerable<T> GetAll()
    {
        foreach (var modItems in _items.Values)
        {
            foreach (var item in modItems.Values)
            {
                yield return item;
            }
        }
    }
}
