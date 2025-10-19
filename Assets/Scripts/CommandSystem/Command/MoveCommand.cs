public class MoveCommand : BaseCommand
{
    readonly BoardCoord from;
    readonly BoardCoord to;
    readonly Board board;
    private bool wasSuccessful = false;
    Piece piece;

    public MoveCommand(BoardCoord from, BoardCoord to, Board board)
    {
        this.from = from;
        this.to = to;
        this.board = board;
    }

    public override string Description => $"Move {piece.Team} {piece.Type} from {from} to {to}";

    public override bool WasSuccessful => wasSuccessful;
    public override bool Execute()
    {
        if (!CanExecute()) return false;
        if (!ValidateMove(piece, to)) return false;
        piece.Position = to;
        // ToDo: gửi event cập nhật vị trí quân cờ 
        piece.RecalculateCache(); 
        wasSuccessful = true;
        return true;
    }

    public override bool Undo()
    {
        if (!WasSuccessful) return false;
        if (!ValidateMove(piece, from)) return false;
        piece.Position = from;
        // toDo: gửi event cập nhật vị trí quân cờ  
        piece.RecalculateCache();
        return true;
    }

    public override bool CanExecute()
    {
        piece = board.Pieces[from];
        if (piece == null) return false;
        return true;
    }

    private bool ValidateMove(Piece piece, BoardCoord to)
    {
        if (!board.IsInBoard(to)) return false;
        if (board.Pieces.ContainsKey(to)) return false;
        if (!piece.PossibleMoves.Contains(to)) return false;
        return true;
    }
}
