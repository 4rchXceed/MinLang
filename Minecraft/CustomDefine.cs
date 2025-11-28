public class CustomDefine
{
    public string name = "";
    public List<string> commands = [];
    public List<FunctionArgInfos> args = [];
    public CustomDefine(string name, string command, List<FunctionArgInfos> args)
    {
        this.name = name;
        this.commands = command.Split(";").ToList();
        this.args = args;
        Globals.customDefines.Add(name, this);
    }
}