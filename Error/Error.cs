public class Error
{
    private Dictionary<int, string> errorMessages = new Dictionary<int, string>
    {
        { 1, "File not found." },
        { 2, "Syntax error." },
        { 3, "Semantic error." },
        { 4, "All variables/Function arg must be of type Int (Minecraft scoreboard's only variable is dummy = int)."},
        { 5, "Function not found." },
        { 6, "Function already exists." },
        { 7, "Variable not initialized [NOT SUPPOSED TO HAPPEN]" },
        { 8, "If only supports: [GlobalVar/LocalVar] <|<=|=|>=|> [GlobalVar/Int]"},
        { 9, "Only Int variables are allowed." },
        { 10, "Generic UCObject error" },
        { 11, "Variable already exists." },
        { 12, "Variable doesn't exist." },
        { 13, "Only 'Add', 'Sub' and 'Set' operations are allowed on variables." },
        { 14, "NotImplementedError" },
        { 15, "I really don't know how you managed to get this error." },
        { 16, "You can't assign a variable to another variable." },
        { 17, "Main function not found (it's the entry point of the program)." },
        { 18, "Not enough arguments provided to function." },
        { 19, "You can't pass a variable as an argument to a custom define." },
        { 20, "Function argument type mismatch." },

    };
    public void Report(int code)
    {
        Console.WriteLine(Environment.StackTrace);
        Console.ForegroundColor = ConsoleColor.Red;
        if (errorMessages.TryGetValue(code, out string? message))
        {
            Console.WriteLine("Error: " + message + $" (Code {code})");
        }
        else
        {
            Console.WriteLine("Error: Unknown error code.");
        }
        Console.ForegroundColor = ConsoleColor.White;
        Environment.Exit(code);
    }
}