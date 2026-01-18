// Typeclass for tile containers
namespace TileGame.Types;

public class Container(int capacity, List<string>? filter = null)
{
    public int Capacity { get; init; } = capacity;
    public List<string>? Filter { get; init; } = filter;
    public Dictionary<Resource, int> Contents { get; init; } = [];

    public bool CanAddResource(Resource resource, int amount)
    {
        if (Filter != null && !Filter.Contains(resource.ID))
        {
            return false;
        }

        int currentAmount = Contents.ContainsKey(resource) ? Contents[resource] : 0;
        int totalAmount = currentAmount + amount;

        return totalAmount <= Capacity;
    }

    public bool CanRemoveResource(Resource resource, int amount)
    {
        if (!Contents.ContainsKey(resource) || Contents[resource] < amount)
        {
            return false;
        }

        return true;
    }

    public void AddResource(Resource resource, int amount)
    {
        if (!CanAddResource(resource, amount))
        {
            throw new InvalidOperationException("Cannot add resource to container: capacity exceeded or resource not allowed.");
        }

        if (!Contents.ContainsKey(resource))
        {
            Contents[resource] = 0;
        }

        Contents[resource] += amount;
    }

    public bool RemoveResource(Resource resource, int amount)
    {
        if (!CanRemoveResource(resource, amount))
        {
            throw new InvalidOperationException("Cannot remove resource from container: insufficient resources.");
        }

        Contents[resource] -= amount;

        if (Contents[resource] == 0)
        {
            Contents.Remove(resource);
        }

        return true;
    }

    internal void AddItem(object item)
    {
        throw new NotImplementedException();
    }
}