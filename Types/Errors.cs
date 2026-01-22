namespace TileGame.Errors;

public class ContentPackError(string message) : Exception(message)
{
}