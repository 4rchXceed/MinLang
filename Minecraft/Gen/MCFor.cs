using System.Numerics;

public class MCFor(ForExpressionInfos infos)
{
    public ForExpressionInfos infos = infos;
    private static int systemFunctionIndex = 0;
    public List<string> Compile()
    {
        if (infos.body == null)
        {
            new Error().Report(3);
            return [];
        }
        List<string> commands = [];
        if (!Globals.functions.ContainsKey(infos.body.functionName))
        {
            new Error().Report(5);
        }
        MCFunctionCall bodyFunctionCall = new MCFunctionCall(infos.body);
        // This is a "hard-coded" function, due to it special syntax
        List<string> forLoopFunCbs = [];
        MCVar forIndex = new MCVar(new LocalVariableExpressionInfos
        {
            name = infos.variableName,
            type = "Int",
            initialValue = new ValueInfos
            {
                type = "Int",
                preProcessedValue = "0"
            }
        });
        forIndex.Compile($"__sys_for_");
        string conditionStr;
        if (infos.step > 0)
        {
            conditionStr = "<";
            forLoopFunCbs.Add(forIndex.Add(infos.step));
        }
        else
        {
            conditionStr = ">";
            forLoopFunCbs.Remove(forIndex.Sub(Math.Abs(infos.step)));
        }
        ConditionInfos conditionObject = new ConditionInfos
        {
            conditionSymbol = conditionStr,
            firstValue = new ValueInfos
            {
                localVarName = infos.variableName,
                type = "LocalVar"
            },
            secondValue = new ValueInfos
            {
                preProcessedValue = infos.end.ToString(),
                type = "Int"
            }
        };
        string forTempFunctionName = $"SysForTempNbr{systemFunctionIndex}";
        MCFunction forTempFunction = new MCFunction(new FunctionInfos
        {
            name = forTempFunctionName,
            lines = [],
            functionArgs = []
        });
        systemFunctionIndex++;
        MCIf forLoopIf = new MCIf(new IfExpressionInfos
        {
            functionInvocation = infos.body,
            condition = conditionObject,
        });
        MCIf forLoopRecursionIf = new MCIf(new IfExpressionInfos
        {
            functionInvocation = new FunctionInvocationExpressionInfos
            {
                functionName = forTempFunctionName,
                argumentsValues = []
            },
            condition = conditionObject
        });
        forLoopFunCbs = forLoopFunCbs.Concat(forLoopIf.Compile()).ToList();
        forLoopFunCbs = forLoopFunCbs.Concat(forLoopRecursionIf.Compile()).ToList();
        List<object> forLoopFunCbsCommands = [];
        foreach (string cmd in forLoopFunCbs)
        {
            forLoopFunCbsCommands.Add(new MCCommand(cmd));
        }
        forTempFunction.infos.lines = forLoopFunCbsCommands;
        forTempFunction.Compile();
        MCFunctionCall forTempFunctionCall = new MCFunctionCall(new FunctionInvocationExpressionInfos
        {
            functionName = forTempFunctionName,
            argumentsValues = []
        });
        commands = commands.Concat(forTempFunctionCall.Compile()).ToList();
        return commands;
    }
}