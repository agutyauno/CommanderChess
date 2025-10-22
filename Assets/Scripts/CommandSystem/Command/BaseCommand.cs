using System;

public abstract class BaseCommand : ICommand
{
    //implement interface
    public abstract string Description { get; }
    public DateTime Timestamp { get; private set; }
    public abstract bool WasSuccessful { get; }

    BoardCoord from;
    BoardCoord to;
    Board board;
    CarryingSystem carryingSystem;


    protected BaseCommand()
    {
        Timestamp = DateTime.Now;
    }

    public abstract bool Execute();
    public abstract bool Undo();
    public abstract bool CanExecute();
}
