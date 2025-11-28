using System.Numerics;

public class MCFunction
{
    public FunctionInfos infos;
    public List<MCVar> args = [];
    public string startCommand = "";
    public Vector3 startBlockPos;
    private bool CheckArguments()
    {
        if (infos.functionArgs == null) return false;
        foreach (FunctionArgInfos arg in infos.functionArgs)
        {
            if (arg.type != "Int")
            {
                new Error().Report(4);
                return false;
            }
        }
        foreach (FunctionArgInfos arg in infos.functionArgs)
        {
            if (arg.name != null)
            {
                MCVar? var = Globals.variablesList.ContainsKey(arg.name) ? Globals.variablesList[arg.name].Item2 : null;
                if (var != null)
                {
                    args.Add(var);
                }
            }
        }

        return true;
    }

    public MCFunction(FunctionInfos infos)
    {
        this.infos = infos;
        Globals.functions.Add(infos.name ?? "", this);
    }

    public void Compile()
    {
        if (infos.functionArgs == null) return;
        if (!CheckArguments()) return;
        List<string> commands = LineCompiler.Compile(infos.lines ?? []);
        if (CommandBlockManager.instance != null && infos.name != null)
        {
            bool isLoop = infos.flags != null && infos.flags.Contains("loop");
            startBlockPos = CommandBlockManager.instance.AddFunction(commands, infos.name, isLoop);
            startCommand = $"setblock {startBlockPos.X} {startBlockPos.Y} {startBlockPos.Z} minecraft:redstone_block";
        }
    }
}