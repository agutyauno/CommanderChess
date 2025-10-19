using System;

public abstract class BaseCommand : ICommand
{
    public abstract string Description { get; }
    public DateTime Timestamp { get; private set; }
    public abstract bool WasSuccessful { get; }

    protected BaseCommand()
    {
        Timestamp = DateTime.Now;
    }

    public abstract bool Execute();
    public abstract bool Undo();
    public abstract bool CanExecute();
}
