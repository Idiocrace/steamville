/*
For modders: YOU NEED THE FOLLOWING FOR YOUR MOD TO WORK:

A SteamvilleMod.dll file that can contain any code that your heart desires
- Doesn't have to be self contained; it can reference any external dependency
- THIS IS WHY MODDING IS DONE AT YOUR OWN RISK: We can't guarantee compatibility, stability, or security of any external code
- Needs to have a class that contains a public static void Init() method that will be called on mod load
- You can use this to register new entities, tiles, items, etc. using the Mod, or call other mod stuff
- Needs to be in the SteamvilleMod namespace to be detected
- Also needs to be an extension of the SVMod class from the SVModLoader library
*/

namespace Steamville.Mods;

public static class ModLoader
{
    public static List<SVMod> LoadedMods = new();
    
}