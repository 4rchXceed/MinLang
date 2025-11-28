using System.Numerics;

public class CommandBlock
{
    public string command = "";
    public Vector3 position = Vector3.Zero;
    public string debugName = "DefaultCommandBlock";
    public string facing = "east"; // north, south, east, west, up, down (default: north = X+)
    public string type = "impulse"; // impulse, chain, repeating (default: impulse)
}