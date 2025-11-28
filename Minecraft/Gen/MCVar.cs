public class MCVar(LocalVariableExpressionInfos? varInfo)
{
    public string name = varInfo?.name ?? string.Empty;
    public int defaultValue = int.TryParse(varInfo?.initialValue?.preProcessedValue, out int val) ? val : 0;
    public LocalVariableExpressionInfos? infos = varInfo;
    public bool generated = false;
    public string mcName = "";
    public void Compile(string prefix = "mlvar_")
    {
        if (infos?.type != "Int")
        {
            new Error().Report(4);
        }
        Globals.setupCommands.Add($"scoreboard objectives remove {prefix}{name}");
        Globals.setupCommands.Add($"scoreboard objectives add {prefix}{name} dummy");
        Globals.setupCommands.Add($"scoreboard players set {Globals.varPlayer} {prefix}{name} {defaultValue}");
        if (!Globals.variablesList.ContainsKey(name))
        {
            Globals.variablesList.Add(name, (prefix + name, this));
            mcName = prefix + name;
            generated = true;
        }
        else
        {
            Console.WriteLine($"Warning: Variable '{name}' is already defined.");
            new Error().Report(11);
        }
    }

    public string Add(int nbr)
    {
        if (!generated) new Error().Report(7);
        return $"scoreboard players add {Globals.varPlayer} {mcName} {nbr}";
    }

    public string Sub(int nbr)
    {
        if (!generated) new Error().Report(7);
        return $"scoreboard players remove {Globals.varPlayer} {mcName} {nbr}";
    }

    public string Set(int nbr)
    {
        if (!generated) new Error().Report(7);
        return $"scoreboard players set {Globals.varPlayer} {mcName} {nbr}";
    }

    public string SetOtherVar(MCVar otherVar)
    {
        if (!generated) new Error().Report(7);
        if (!otherVar.generated) new Error().Report(7);
        return $"scoreboard players operation {Globals.varPlayer} {mcName} = {Globals.varPlayer} {otherVar.mcName}";
    }
}