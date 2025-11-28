using System.Numerics;

public class Globals
{
    public static string varPlayer = "@a[limit=1,sort=nearest]";
    public static Vector3 startingPosition = new Vector3(0, 0, 0);
    public static List<string> setupCommands = []; // Example: variable creation (scoreboard), ...
    public static Dictionary<string, (string, MCVar)> variablesList = []; // List of variable names (it's here only to warn the user if a variable is created twice or doesn't exist) (key: logical name, value: minecraft name)
    public static Dictionary<string, MCFunction> functions = [];
    public static Dictionary<string, CustomDefine> customDefines = [];
}