using System;

public class BoardingCommand : BaseCommand
{
    public override string Description => $"";

    public override bool WasSuccessful => throw new NotImplementedException();

    public override bool CanExecute()
    {
        throw new NotImplementedException();
    }

    public override bool Execute()
    {
        throw new NotImplementedException();
    }

    public override bool Undo()
    {
        throw new NotImplementedException();
    }
}
