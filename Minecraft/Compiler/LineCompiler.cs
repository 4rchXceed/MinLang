using System.ComponentModel;
using System.Text.Json;

public class LineCompiler
{
    public static List<string> Compile(List<object> lines)
    {
        List<string> commands = [];
        foreach (object line in lines)
        {
            if (line is MCCommand cmd)
            {
                commands.Add(cmd.command);
            }
            else
            if (line is JsonElement exprInfo)
            {
                string? lineType = exprInfo.GetProperty("expressionType").GetString();
                if (lineType != null)
                {
                    switch (lineType)
                    {
                        case "FunctionInvocationExpression":
                            FunctionInvocationExpressionInfos? funcInvocInfo = JsonSerializer.Deserialize<FunctionInvocationExpressionInfos>(exprInfo.GetRawText());
                            if (funcInvocInfo != null)
                            {
                                MCFunctionCall funcCall = new MCFunctionCall(funcInvocInfo);
                                commands = commands.Concat(funcCall.Compile()).ToList();
                            }
                            break;
                        case "VariableModificationExpression":
                            VariableModificationExpressionInfos? varModInfo = JsonSerializer.Deserialize<VariableModificationExpressionInfos>(exprInfo.GetRawText());
                            if (varModInfo != null)
                            {
                                if (Globals.variablesList.ContainsKey(varModInfo.variableName ?? ""))
                                {
                                    MCVar varMod = Globals.variablesList[varModInfo.variableName ?? ""].Item2;
                                    switch (varModInfo.functionName)
                                    {
                                        case "Add":
                                            commands.Add(varMod.Add(Math.Abs(varModInfo.argumentCount)));
                                            break;
                                        case "Sub":
                                            commands.Add(varMod.Sub(Math.Abs(varModInfo.argumentCount)));
                                            break;
                                        case "Set":
                                            commands.Add(varMod.Set(varModInfo.argumentCount));
                                            break;
                                        default:
                                            new Error().Report(13);
                                            break;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Variable not found: " + varModInfo.variableName);
                                    new Error().Report(12);
                                }
                            }
                            break;
                        case "IfExpression":
                            IfExpressionInfos? ifInfo = JsonSerializer.Deserialize<IfExpressionInfos>(exprInfo.GetRawText());
                            if (ifInfo != null)
                            {
                                MCIf ifExpr = new MCIf(ifInfo);
                                commands = commands.Concat(ifExpr.Compile()).ToList();
                            }
                            break;
                        case "ForExpression":
                            ForExpressionInfos? forInfo = JsonSerializer.Deserialize<ForExpressionInfos>(exprInfo.GetRawText());
                            if (forInfo != null)
                            {
                                MCFor forExpr = new MCFor(forInfo);
                                commands = commands.Concat(forExpr.Compile()).ToList();
                            }
                            break;
                        case "FunctionReturn":
                            new Error().Report(14);
                            break;
                        case "LocalVariableExpression":
                            LocalVariableExpressionInfos? localVarInfo = JsonSerializer.Deserialize<LocalVariableExpressionInfos>(exprInfo.GetRawText());
                            if (localVarInfo != null)
                            {
                                if (Globals.variablesList.ContainsKey(localVarInfo.name ?? ""))
                                {
                                    if (localVarInfo.type != "Int")
                                    {
                                        new Error().Report(16);
                                        break;
                                    }
                                    MCVar localVar = Globals.variablesList[localVarInfo.name ?? ""].Item2;
                                    commands.Add(localVar.Set(localVarInfo.initialValue != null && int.TryParse(localVarInfo.initialValue.preProcessedValue, out int val) ? val : 0));
                                }
                                else
                                {
                                    new Error().Report(15);
                                }
                            }
                            break;
                        default:
                            Console.WriteLine("Unknown line type: " + lineType);
                            new Error().Report(10);
                            break;
                    }
                }
                else
                {
                    new Error().Report(10);
                }
            }
            else
            {
                new Error().Report(10);
            }
        }
        return commands;
    }
}