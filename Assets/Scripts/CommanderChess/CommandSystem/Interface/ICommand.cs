using System;

namespace CommanderChess.CommandSystem
{
    public interface ICommand
    {
        string Description { get; }
        DateTime Timestamp { get; }
        bool WasSuccessful { get; }
        bool Execute();
        bool CanExecute();
    }
}
