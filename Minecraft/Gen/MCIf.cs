public class MCIf(IfExpressionInfos infos)
{
    public IfExpressionInfos infos = infos;
    public List<string> Compile()
    {
        if (infos.functionInvocation == null || infos.functionInvocation.functionName == null || infos.condition == null)
        {
            return []; // to make the compiler happy
        }
        List<string> commands = [];
        MCFunctionCall startCall = new MCFunctionCall(infos.functionInvocation);
        List<string> startCommands = startCall.Compile();
        foreach (string cmd in startCommands)
        {
            MCCondition condition = new MCCondition(infos.condition);
            List<string>? conditionCommands = condition.Compile(cmd);
            if (conditionCommands != null)
            {
                commands = commands.Concat(conditionCommands).ToList();
            }
        }
        return commands;
    }
}