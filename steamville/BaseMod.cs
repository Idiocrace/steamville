using SteamVille.Content;
using Crucible.Types;

namespace BaseMod;

public class BaseMod : SVMod
{
    public override string ModID => "basemod";
    public override string ModName => "Base Mod";
    public override List<string> ModAuthors => new List<string> { "deltathedumb" };
    public override string ModVersion => "1.0.0";
    public override string ModDescription => "The foundational mod that provides essential functionalities for SteamVille.";
    
    public override void Initialize(Registry<Tileable> tileRegistry, Registry<Resource> resourceRegistry)
    {
        // Register resources
        resourceRegistry.Register(ModID, BMResources.Coal.ID, BMResources.Coal);
        resourceRegistry.Register(ModID, BMResources.Water.ID, BMResources.Water);
    }
}

public static class BMResources
{
    public static Resource Coal => new Resource
    {
        ID = "coal",
        Name = "Coal",
        Sprite = null!,
        Color = "#393c41",
    };

    public static Resource Water => new Resource
    {
        ID = "water",
        Name = "Water",
        Sprite = null!,
        Color = "#4034e9",
    };

    public static Resource Electricity => new Resource
    {
        ID = "electricity",
        Name = "Electricity",
        Sprite = null!,
        Color = "#ffff00",
    };

    public static Resource Steam => new Resource
    {
        ID = "steam",
        Name = "Steam",
        Sprite = null!,
        Color = "#c8cbce",
    };

    public static Resource HighSteam => new Resource
    {
        ID = "high_steam",
        Name = "High Pressure Steam",
        Sprite = null!,
        Color = "#797c80",
    };
}