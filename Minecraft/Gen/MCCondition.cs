
public class MCCondition(ConditionInfos infos)
{
    public ConditionInfos infos = infos;
    public static List<string> allowedConditions = [
        "<",
        "<=",
        "=",
        ">=",
        ">"
    ];
    public List<string>? Compile(string command)
    {
        if (infos.conditionSymbol == null)
        {
            new Error().Report(3);
            return []; // To make the compiler happy
        }
        if (infos.firstValue == null || infos.firstValue.type != "LocalVar" && infos.firstValue.type != "GlobalVar")
        {
            new Error().Report(8);
            return [];
        }
        if (infos.secondValue == null || infos.secondValue.type != "GlobalVar" && infos.secondValue.type != "Int" && infos.secondValue.type != "LocalVar")
        {
            new Error().Report(8);
            return [];
        }
        bool negation = infos.conditionSymbol.StartsWith("!");
        string condition = infos.conditionSymbol;
        if (negation)
        {
            condition = condition[1..];
        }
        if (!allowedConditions.Contains(condition))
        {
            new Error().Report(8);
        }
        if (infos.firstValue.type == "GlobalVar")
        {
            if (int.TryParse(infos.firstValue.preProcessedValue, out int numberA))
            {
                if (int.TryParse(infos.secondValue.preProcessedValue, out int numberB))
                {
                    bool success = false;
                    switch (condition)
                    {
                        case "<":
                            success = (numberA < numberB) ^ negation;
                            break;
                        case "<=":
                            success = (numberA <= numberB) ^ negation;
                            break;
                        case "=":
                            success = (numberA == numberB) ^ negation;
                            break;
                        case ">=":
                            success = (numberA >= numberB) ^ negation;
                            break;
                        case ">":
                            success = (numberA > numberB) ^ negation;
                            break;
                    }
                    if (success)
                    {
                        return [command];
                    }
                }
            }
        }
        else
        {
            string? firstKeyword = infos.firstValue.type == "LocalVar" ? Globals.variablesList[infos.firstValue.localVarName ?? ""].Item1 : infos.firstValue.preProcessedValue;
            string? secondKeyword = infos.secondValue.type == "LocalVar" ? Globals.variablesList[infos.secondValue.localVarName ?? ""].Item1 : infos.secondValue.preProcessedValue;
            string preCommand = "";
            string afterCommand = "";
            if (infos.secondValue.type == "GlobalVar" || infos.secondValue.type == "Int") // Scoreboard only supports variable to variable comparisons (it accepts "matches" but only on equality)
            {
                preCommand = $"scoreboard players set {Globals.varPlayer} __sys_temp_cond_var {infos.secondValue.preProcessedValue}";
                secondKeyword = "__sys_temp_cond_var";
                afterCommand = $"scoreboard players reset {Globals.varPlayer} __sys_temp_cond_var";
            }
            if (negation)
            {
                return [preCommand, $"execute unless score {Globals.varPlayer} {firstKeyword} {condition} {Globals.varPlayer} {secondKeyword} run {command}", afterCommand];
            }
            else
            {
                return [preCommand, $"execute if score {Globals.varPlayer} {firstKeyword} {condition} {Globals.varPlayer} {secondKeyword} run {command}", afterCommand];
            }
        }
        return null;
    }
}