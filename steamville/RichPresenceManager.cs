using DiscordRPC;

namespace TileEngine;

public static class RichPresenceManager
{
    private const string clientID = "1465736272625795082";
    private static DiscordRpcClient? client;
    private static bool isEnabled = false;
    private static Dictionary<string, object> context = [];

    public static void Enable()
    {
        if (string.IsNullOrWhiteSpace(clientID))
        {
            Console.WriteLine("clientID not set inside RichPresenceManager. This issue can only be resolved in a build patch. Please alert us of this issue immediately.");
            return;
        }

        client = new DiscordRpcClient(clientID);
        client.Initialize();
        isEnabled = true;
    }

    public static void Disable()
    {
        if (client != null)
        {
            try { client.Deinitialize(); client.Dispose(); } catch { }
            client = null;
        }
        isEnabled = false;
    }

    public static void UpdatePresence(Dictionary<string, object> newContext)
    {
        if (!isEnabled || client == null) return;

        context = newContext;

        var presence = new RichPresence
        {
            Details = context.ContainsKey("details") ? context["details"].ToString() : "Playing TileGame",
            State = context.ContainsKey("state") ? context["state"].ToString() : "In Menu"
        };

        client.SetPresence(presence);
    }
}