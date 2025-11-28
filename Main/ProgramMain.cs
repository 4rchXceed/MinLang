using System.Numerics;
using System.Text.Json;

public class CompilerMain
{
    public CommandBlockManager cbManager;
    private List<string> languageSetupCommands = new List<string>
    {
        "scoreboard objectives add __sys_temp_cond_var dummy",
    };
    public CompilerMain(string filePath)
    {
        cbManager = new CommandBlockManager();
        if (!File.Exists(filePath))
        {
            new Error().Report(1);
        }
        UCObject? code = JsonSerializer.Deserialize<UCObject>(File.ReadAllText(filePath));
        if (code == null)
        {
            new Error().Report(2);
        }
        else
        {
            foreach (CustomDefineInfos customDefine in code.customDefines)
            {
                if (customDefine.functionName != null && customDefine.functionExpression != null)
                {
                    CustomDefine def = new CustomDefine(customDefine.functionName, customDefine.functionExpression, customDefine.functionArguments);
                }
            }
            foreach (LocalVariableExpressionInfos localVarInfo in code.localVariables)
            {
                MCVar localVar = new MCVar(localVarInfo);
                localVar.Compile();
                Globals.variablesList[localVarInfo.name ?? ""] = (localVar.mcName ?? "", localVar);
            }
            List<MCFunction> functions = [];
            foreach (FunctionInfos fun in code.functions)
            {
                MCFunction mcFun = new MCFunction(fun);
                functions.Add(mcFun);
            }
            foreach (MCFunction mcFun in functions) // Compile after all functions are created to allow recursion
            {
                mcFun.Compile();
            }
            if (!Globals.functions.ContainsKey("Main"))
            {
                new Error().Report(17);
            }

            Vector3 mainFunctionPos = Globals.functions["Main"].startBlockPos;
            Globals.setupCommands.Add($"setblock {mainFunctionPos.X} {mainFunctionPos.Y} {mainFunctionPos.Z} minecraft:redstone_block");
            foreach (string cmd in languageSetupCommands)
            {
                Globals.setupCommands.Add(cmd);
            }
            Vector3 setupFunctionPos = cbManager.AddFunction(Globals.setupCommands, "setup");
            string startCommand = $"setblock {setupFunctionPos.X} {setupFunctionPos.Y} {setupFunctionPos.Z} minecraft:redstone_block";
            // cbManager.PrintCommands();
            Console.WriteLine("Start Command: " + startCommand);
            string final = cbManager.ConcatenateCommands(startCommand);
            string outputPath = "output.mccmd";
            if (Program.parsedArgs.Length >= 2)
            {
                outputPath = Program.parsedArgs[1];
            }
            File.WriteAllText(outputPath, final);
        }
    }
}