using System;

public interface ICommand
{
    string Description { get; }
    DateTime Timestamp { get; }
    bool WasSuccessful { get; }
    bool Execute();
    bool Undo();
    bool CanExecute();
}
