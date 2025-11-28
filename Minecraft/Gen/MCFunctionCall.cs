public class MCFunctionCall(FunctionInvocationExpressionInfos infos)
{
    public FunctionInvocationExpressionInfos infos = infos;
    public List<string> Compile()
    {
        List<string> commands = [];
        if (!Globals.functions.ContainsKey(infos.functionName))
        {
            CustomDefine? def = Globals.customDefines.ContainsKey(infos.functionName) ? Globals.customDefines[infos.functionName] : null;
            if (def != null)
            {
                List<string> cbCommands = [];
                foreach (string commandText in def.commands)
                {
                    string command = commandText;
                    for (int i = 0; i < def.args.Count; i++)
                    {
                        FunctionArgInfos arg = def.args[i];
                        if (i >= infos.argumentsValues.Count)
                        {
                            Console.WriteLine("Not enough arguments for function: " + infos.functionName);
                            new Error().Report(18);
                            continue;
                        }
                        if (infos.argumentsValues[i].type == "LocalVar")
                        {
                            new Error().Report(19);
                            continue;
                        }
                        if (infos.argumentsValues[i].isFunctionRawCode)
                        {
                            FunctionInvocationExpressionInfos? nestedFunc = infos.argumentsValues[i].functionInvocation;
                            if (nestedFunc != null)
                            {
                                MCFunctionCall nestedCall = new MCFunctionCall(nestedFunc);
                                List<string> nestedCommands = nestedCall.Compile();
                                cbCommands = cbCommands.Concat(nestedCommands.Take(nestedCommands.Count - 1)).ToList();
                                command = command.Replace($"?{arg.name}?", nestedCommands.Last());
                            }
                            else
                            {
                                new Error().Report(10);
                                continue;
                            }
                        }
                        else
                        {
                            if (infos.argumentsValues[i].type != arg.type && arg.type == "Int") // Only Int arguments are supported, the others means that there's a custom define, and every arg of custom define is interpreted as String, so no need
                            {
                                new Error().Report(20);
                            }
                            if (infos.argumentsValues[i].functionInvocation != null)
                            {
                                var funcName = infos.argumentsValues[i].functionInvocation?.functionName ?? "";
                                MCFunction? functionToCall = Globals.functions.TryGetValue(funcName, out var func) ? func : null;
                                if (functionToCall == null)
                                {
                                    new Error().Report(5);
                                    continue;
                                }
                                else
                                {
                                    if (functionToCall.startCommand == "")
                                    {
                                        command = command.Replace($"?{arg.name}?", "START_COMMAND={" + functionToCall.infos.name + "}"); // Syntax to then be replaced by the actual command later
                                    }
                                    else
                                    {
                                        command = command.Replace($"?{arg.name}?", functionToCall.startCommand);
                                    }
                                }
                            }
                            else
                            {
                                command = command.Replace($"?{arg.name}?", infos.argumentsValues[i].preProcessedValue ?? ""); // Replace type placeholders
                            }
                        }
                    }
                    cbCommands.Add(command);
                }
                return cbCommands;
            }
            else
            {
                Console.WriteLine("Function not found: " + infos.functionName);
                new Error().Report(5);
            }
        }
        MCFunction callingFunction = Globals.functions[infos.functionName]; // ?? is to make the compiler happy
        if (callingFunction.startCommand == null)
        {
            new Error().Report(3);
            return [];
        }
        if (infos.argumentsValues.Count < callingFunction.args.Count)
        {
            new Error().Report(18);
        }
        foreach (MCVar arg in callingFunction.args)
        {
            int argIndex = callingFunction.args.IndexOf(arg);
            ValueInfos argValue = infos.argumentsValues[argIndex];
            if (argValue.type == "LocalVar")
            {
                if (!Globals.variablesList.ContainsKey(argValue.localVarName ?? ""))
                {
                    new Error().Report(12);
                    continue;
                }
                commands.Add(arg.SetOtherVar(Globals.variablesList[argValue.localVarName ?? ""].Item2));
                continue;
            }
            if (argValue.type != "Int")
            {
                new Error().Report(4);
                continue;
            }
            commands.Add(arg.Set(int.TryParse(argValue.preProcessedValue, out int val) ? val : 0));
        }
        if (callingFunction.startCommand == "")
        {
            commands.Add("START_COMMAND={" + callingFunction.infos.name + "}"); // Syntax to then be replaced by the actual command later
        }
        else
        {
            commands.Add(callingFunction.startCommand);
        }
        return commands;
    }
}