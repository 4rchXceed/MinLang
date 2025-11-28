public class UCObject
{
    public List<GlobalReplaceVarInfos> globalVars { get; set; } = [];
    public List<CustomDefineInfos> customDefines { get; set; } = [];
    public List<FunctionInfos> functions { get; set; } = [];
    public List<LocalVariableExpressionInfos> localVariables { get; set; } = [];
}

public class GlobalReplaceVarInfos
{
    public string? type { get; set; } = null;
    public string? name { get; set; } = null;
    public ValueInfos? value { get; set; } = null;
}

public class CustomDefineInfos
{
    public List<FunctionArgInfos> functionArguments { get; set; } = [];
    public string? functionName { get; set; } = null;
    public string? functionExpression { get; set; } = null;
}

public class FunctionArgInfos
{
    public string? name { get; set; } = null;
    public string? type { get; set; } = null;
}

public class FunctionInfos
{
    public bool isAsCode { get; set; } = false;
    public string? name { get; set; } = null;
    public List<FunctionArgInfos>? functionArgs { get; set; } = null;
    public List<object>? lines { get; set; } = null;
    public List<string>? flags { get; set; } = null;
}

public partial class ExpressionTemplateInfos
{
    public string? expressionType { get; set; } = null;
}

public class ForExpressionInfos : ExpressionTemplateInfos
{
    public int start { get; set; } = 0;
    public int end { get; set; } = 0;
    public int step { get; set; } = 1;
    public string variableName { get; set; } = "";
    public FunctionInvocationExpressionInfos? body { get; set; } = null;
}


public class FunctionInvocationExpressionInfos : ExpressionTemplateInfos
{
    public string functionName { get; set; } = "Undefined function name";
    public List<ValueInfos> argumentsValues { get; set; } = [];
}
public class ValueInfos
{
    public string type { get; set; } = "Void";
    public string? preProcessedValue { get; set; } = null;
    public string? localVarName { get; set; } = null;
    public bool isFunctionRawCode { get; set; } = false;
    public FunctionInvocationExpressionInfos? functionInvocation { get; set; } = null;
}

public class VariableModificationExpressionInfos : ExpressionTemplateInfos
{
    public string? variableName { get; set; } = null;
    public string? functionName { get; set; } = null;
    public int argumentCount { get; set; } = 0;
}

public class FunctionReturnInfos : ExpressionTemplateInfos
{
    public ValueInfos? returnValue { get; set; } = null;
}

public class IfExpressionInfos : ExpressionTemplateInfos
{
    public ConditionInfos? condition { get; set; } = null;
    public FunctionInvocationExpressionInfos? functionInvocation { get; set; } = null;
}

public class ConditionInfos
{
    public ValueInfos? firstValue { get; set; } = null;
    public string? conditionSymbol { get; set; } = null;
    public ValueInfos? secondValue { get; set; } = null;
}

public class LocalVariableExpressionInfos : ExpressionTemplateInfos
{
    public ValueInfos? initialValue { get; set; } = null;
    public string? type { get; set; } = null;
    public string? name { get; set; } = null;
}