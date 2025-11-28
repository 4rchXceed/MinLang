using System.Numerics;

public class Program
{
    public static string[] parsedArgs = [];
    public static void Main(string[] args)
    {
        parsedArgs = args;
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: program <filePath> <outputPath.mccmd> <x>,<y>,<z>");
            return;
        }
        string filePath = args[0];
        Vector3 startPos = new Vector3(0, 0, 0);
        if (args.Length >= 3)
        {
            string[] coords = args[2].Split(',');
            if (coords.Length == 3 &&
                float.TryParse(coords[0], out float x) &&
                float.TryParse(coords[1], out float y) &&
                float.TryParse(coords[2], out float z))
            {
                startPos = new Vector3(x, y, z);
            }
            else
            {
                Console.WriteLine("Invalid starting position format. Using default (0,0,0).");
            }
        }
        Globals.startingPosition = startPos;
        CompilerMain compiler = new CompilerMain(filePath);
    }
}