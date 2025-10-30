using System;

public class BoardingCommand : BaseCommand
{
    BasePiece pieceToBoard;

    // trạng thái để undo
    bool wasShotDown = false;
    bool becamePassenger = false;

    public BoardingCommand(
        BoardCoord from,
        BoardCoord to,
        Board board,
        CarryingSystem carryingSystem,
        StateBackupService backupService,
        PathChecker pathChecker,
        MovementExecutor movementExecutor) :
    base(from, to, board, carryingSystem, backupService, pathChecker, movementExecutor)
    {
        SelectedPiece = board.Pieces[from];
        pieceToBoard = board.Pieces[to];
        wasShotDown = false;
        becamePassenger = false;
    }

    public override string Description => $"Boarding: {SelectedPiece.Type} -> {pieceToBoard.Type} at {To.ToLabel()}";

    public override bool CanExecute()
    {
        if (SelectedPiece == null) return false;
        if (pieceToBoard == null) return false;

        if (!SelectedPiece.PossibleMoves.Contains(To)) return false;
        return true;
    }

    protected override bool DoExecute()
    {
        var result = pathChecker.CheckPath(SelectedPiece, From, To).Result;
        switch (result)
        {
            case PathResult.GoThrough:
            case PathResult.Inside:
                // quân bị bắn hạ => remove khỏi board
                movementExecutor.RemoveFromBoard(SelectedPiece);
                wasShotDown = true;
                break;

            case PathResult.None:
                // cố gắng boarding (TryAddCarry tự chọn ai là carrier)
                if (!carryingSystem.TryAddCarry(SelectedPiece, pieceToBoard)) return false;

                // Nếu SelectedPiece trở thành passenger thì không nên override occupant trên board.
                // Thay vào đó remove SelectedPiece khỏi board và cập nhật vị trí logic/visual.
                if (carryingSystem.GetCarrier(SelectedPiece) == pieceToBoard)
                {
                    // SelectedPiece now carried by pieceToBoard
                    movementExecutor.RemoveFromBoard(SelectedPiece);
                    movementExecutor.UpdatePositionOnly(SelectedPiece, To);
                    becamePassenger = true;
                }
                else
                {
                    // SelectedPiece is carrier (carries pieceToBoard), di chuyển bình thường
                    var movementResult = movementExecutor.MovePiece(SelectedPiece, From, To);
                    if (!movementResult.IsSuccess)
                        return false;
                }
                break;
        }
        return true;
    }

    protected override bool DoUndo()
    {
        try
        {
            // Nếu bị bắn hạ thì restore quân về vị trí ban đầu (trước khi restore snapshot)
            if (wasShotDown)
            {
                if (!movementExecutor.PlaceOnBoard(SelectedPiece, From))
                {
                    UnityEngine.Debug.LogError("BoardingCommand: Failed to restore shot piece to board");
                    return false;
                }
                return true;
            }

            // Nếu trở thành passenger khi execute => đặt lại trên board về vị trí ban đầu
            if (becamePassenger)
            {
                if (!movementExecutor.PlaceOnBoard(SelectedPiece, From))
                {
                    UnityEngine.Debug.LogError("BoardingCommand: Failed to place passenger back on board");
                    return false;
                }
                return true;
            }

            // Nếu di chuyển như carrier thì revert move (currentPos = To, previousPos = From)
            var res = movementExecutor.RevertMovePiece(SelectedPiece, To, From);
            if (!res.IsSuccess)
            {
                UnityEngine.Debug.LogError($"BoardingCommand: Failed to revert move: {res}");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"BoardingCommand DoUndo failed: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }
}
