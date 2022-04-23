using System.Collections.Generic;

public class CommandManager
{
    public List<ICommand> commandList = new List<ICommand>();
    public CommandPanel commandPanel;
    int index;
    public CommandManager(CommandPanel commandPanel)
    {
        this.commandPanel = commandPanel;
    }

    public void ClearHistory()
    {
        index = 0;
        commandList.Clear();
        commandPanel.ClearCommands();
    }

    public void AddCommand(ICommand command, bool execute = true)
    {
        if (index < commandList.Count)
            commandList.RemoveRange(index, commandList.Count - index);

        commandList.Add(command);
        if (execute) command.Execute();
        
        commandPanel.AddCommand(command.Name, index);
        index++;
    }

    public void UndoCommand()
    {
        if (commandList.Count == 0) return;

        if (index > 0)
        {
            commandList[index - 1].Undo();
            index--;
        }

        commandPanel.UndoCommand(index);
    }

    public void RedoCommand()
    {
        if (commandList.Count == 0) return;

        if (index < commandList.Count)
        {
            index++;
            commandList[index - 1].Execute();
        }

        commandPanel.RedoCommand(index);

        //VoxelManager.instance.CreateMesh();
    }
}