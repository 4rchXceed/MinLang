using System.Data.Common;
using System.Numerics;
using System.Runtime.CompilerServices;

public class CommandBlockManager
{
    public static CommandBlockManager? instance;
    private List<CommandBlock> commandBlocks = new List<CommandBlock>();
    public CommandBlockManager()
    {
        instance ??= this;
    }
    public Vector3 AddFunction(List<string> commands, string debugName = "None", bool isLoop = false) // Returns the pos of the block made to start the commands
    {
        if (commands.Count == 0) return new Vector3(0, 0, 0);
        if (commandBlocks.Count > 0)
        {
            Vector3 lastPos = commandBlocks[^1].position + new Vector3(1, 0, 0); // +1 for a two gap between functions
            Vector3 firstPos = commandBlocks[^1].position + new Vector3(1, 0, 0);
            commands.Insert(0, "setblock " + firstPos.X + " " + firstPos.Y + " " + firstPos.Z + " minecraft:air"); // Replace the "init" block (redstone block) with air to allow the function to be re-called in the future
            bool first = true;
            foreach (string command in commands)
            {
                CommandBlock newBlock = new CommandBlock
                {
                    command = command,
                    position = lastPos + new Vector3(1, 0, 0),
                    debugName = debugName,
                    type = first ? (isLoop ? "repeating" : "impulse") : "chain",
                };
                first = false;
                commandBlocks.Add(newBlock);
                lastPos = newBlock.position;
            }
            return firstPos; // The first block is at lastPos + (1,0,0), so we return lastPos to place the redstone block there
        }
        else
        {
            commands.Insert(0, "setblock " + Globals.startingPosition.X + " " + Globals.startingPosition.Y + " " + Globals.startingPosition.Z + " minecraft:air"); // Replace the "init" block (redstone block) with air to allow the function to be re-called in the future
            CommandBlock firstBlock = new CommandBlock
            {
                command = commands[0],
                position = Globals.startingPosition + new Vector3(1, 0, 0),
                debugName = debugName,
                type = isLoop ? "repeating" : "impulse",
            };
            commandBlocks.Add(firstBlock);
            for (int i = 1; i < commands.Count; i++)
            {
                CommandBlock newBlock = new CommandBlock
                {
                    command = commands[i],
                    position = commandBlocks[i - 1].position + new Vector3(1, 0, 0),
                    debugName = debugName,
                    type = "chain",
                };
                commandBlocks.Add(newBlock);
            }
            return Globals.startingPosition; // The first block is at starting position + (1,0,0), so we return starting position to place the redstone block there
        }
    }

    public Vector3 GetNextFreePosition()
    {
        if (commandBlocks.Count == 0) return Globals.startingPosition;
        return commandBlocks[^1].position + new Vector3(1, 0, 0);
    }

    public string ConcatenateCommands(string startingCommand = "")
    {
        string[] allCommands = ["summon falling_block ~ ~1 ~ {BlockState:{Name:\"minecraft:redstone_block\"},Time:1,Passengers:[{id:\"falling_block\",BlockState:{Name:\"activator_rail\"},Time:1,Passengers:["];
        int currentIndex = 0;
        foreach (CommandBlock cb in commandBlocks)
        {
            string command = cb.command;
            if (command.Contains("START_COMMAND={"))
            {
                string functionName = command.Split("START_COMMAND={")[1].Split("}")[0].Trim(); // Extract the function name
                MCFunction? func = Globals.functions.ContainsKey(functionName) ? Globals.functions[functionName] : null;
                if (func != null)
                {
                    command = command.Replace("START_COMMAND={" + functionName + "}", func.startCommand);
                }
                else
                {
                    new Error().Report(5);
                    continue;
                }
            }
            string type = cb.type;
            bool alwaysActive = type == "chain" || type == "repeating"; // Chain command blocks are always active, repeating doesn't have a stop condition yet
            if (type == "impulse" || type == "repeating")
            {
                if (type != "repeating")
                {
                    type = "";
                }
                else
                {
                    type = "repeating_";
                }
                if (Program.parsedArgs.Contains("-O+dbg"))
                {
                    string debugCommand = $"setblock {cb.position.X} {cb.position.Y + 1} {cb.position.Z} oak_sign{{front_text:{{messages:[\"{cb.debugName}\",\"\",\"\",\"\"]}}}} replace"; // Debug sign above the command block
                    allCommands[currentIndex] += $"{{id:\"command_block_minecart\",Command:\"{debugCommand.Replace("\"", "\\\"")}\"}},";
                }
            }
            else
            {
                type = type + "_";
            }
            string setblockCommand = $"setblock {cb.position.X} {cb.position.Y} {cb.position.Z} minecraft:{type}command_block[facing={cb.facing}]{{auto:{(alwaysActive ? "1b" : "0b")},Command:\"{command.Replace("\"", "\\\"")}\"}}";

            allCommands[currentIndex] += $"{{id:\"command_block_minecart\",Command:\"{setblockCommand.Replace("\"", "\\\"")}\"}},";
            if (allCommands[currentIndex].Length > 32000) // Minecraft command length limit WITH some buffer
            {
                allCommands[currentIndex] = allCommands[currentIndex].TrimEnd(',') + "]}]}";
                currentIndex++;
                Array.Resize(ref allCommands, allCommands.Length + 1);
                allCommands[currentIndex] = "summon falling_block ~ ~1 ~ {BlockState:{Name:\"minecraft:redstone_block\"},Time:1,Passengers:[{id:\"falling_block\",BlockState:{Name:\"activator_rail\"},Time:1,Passengers:[";
            }
        }
        if (startingCommand != "")
        {
            allCommands[currentIndex] += $"{{id:\"command_block_minecart\",Command:\"{startingCommand.Replace("\"", "\\\"")}\"}},"; // Include the starting command
        }

        allCommands[currentIndex] += $"{{id:\"command_block_minecart\",Command:\"kill @e[type=command_block_minecart]\"}}"; // Kill the command block minecarts after they executed their command (else they run their command again and again)
        allCommands[currentIndex] += "]}]}";

        return string.Join("\n", allCommands);
    }

    public void PrintCommands()
    {
        foreach (CommandBlock cb in commandBlocks)
        {
            Console.WriteLine(cb.position + ": " + cb.type);
            Console.WriteLine($"->  {cb.command}");
        }
    }
}