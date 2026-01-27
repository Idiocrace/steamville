using DiscordRPC;

namespace TileGame;

public class RichPresenceManager
{
    private const string clientID = "1465736272625795082";
    private DiscordRpcClient? client;
    private bool isEnabled = false;
    private Dictionary<string, object> context = [];

    public void Enable()
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

    public void Disable()
    {
        if (client != null)
        {
            try { client.Deinitialize(); client.Dispose(); } catch { }
            client = null;
        }
        isEnabled = false;
    }

    public void UpdatePresence(Dictionary<string, object> context)
    {
        if (!isEnabled || client == null) return;

        var presence = new RichPresence
        {
            Details = context.ContainsKey("details") ? context["details"].ToString() : "Playing TileGame",
            State = context.ContainsKey("state") ? context["state"].ToString() : "In Menu"
        };

        client.SetPresence(presence);
    }
}