public class MCCommand(string command)
{
    public string command = command;
    public List<string> Compile()
    {
        return [command];
    }
}