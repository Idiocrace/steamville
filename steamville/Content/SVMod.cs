using Crucible.Types;

namespace SteamVille.Content;

public abstract class SVMod
{
    public abstract string ModID { get; }
    public abstract string ModName { get; }
    public abstract List<string> ModAuthors { get; }
    public abstract string ModVersion { get; }
    public abstract string ModDescription { get; }

    public abstract void Initialize(Registry<Tileable> tileRegistry, Registry<Resource> resourceRegistry);
}