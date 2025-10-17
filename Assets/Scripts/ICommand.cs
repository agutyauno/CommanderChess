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

public class CaptureCommand : BaseCommand
{
    readonly BoardCoord attackerPos;
    readonly BoardCoord targetPos;
    readonly Board board;
    private bool wasSuccessful = false;
    Piece attacker;
    Piece target;

    public CaptureCommand(BoardCoord attackerPos, BoardCoord targetPos, Board board)
    {
        this.attackerPos = attackerPos;
        this.targetPos = targetPos;
        this.board = board;
    }

    public override string Description => $"{attacker.Team} {attacker.Type} at {attackerPos} captures {target.Team} {target.Type} at {targetPos}";

    public override bool WasSuccessful => wasSuccessful;

    public override bool Execute()
    {
        if (!CanExecute()) return false;

        // Xóa target và các quân nó đang mang khỏi bàn cờ
        target.OnCaptured();
        board.Pieces.Remove(targetPos);

        // Di chuyển attacker nếu cần
        if (attacker.DoMoveToTarget)
        {
            attacker.Position = targetPos;
            // ToDo: gửi event cập nhật vị trí quân cờ
        }
        attacker.RecalculateCache();
        wasSuccessful = true;
        return true;
    }

    public override bool Undo()
    {
        if (!WasSuccessful) return false;
        // Đặt lại target vào vị trí ban đầu
        board.PlacePiece(target, targetPos);

        // Đặt lại attacker về vị trí ban đầu nếu nó đã di chuyển
        if (attacker.DoMoveToTarget)
        {
            attacker.Position = attackerPos;
            // ToDo: gửi event cập nhật vị trí quân cờ
        }
        attacker.RecalculateCache();
        return true;
    }

    public override bool CanExecute()
    {
        if (!board.Pieces.TryGetValue(attackerPos, out attacker)) return false;
        if (!board.Pieces.TryGetValue(targetPos, out target)) return false;

        // Kiểm tra tính hợp lệ của việc tấn công
        if (!attacker.PossibleAttacks.Contains(targetPos)) return false;

        return true;
    }
}

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