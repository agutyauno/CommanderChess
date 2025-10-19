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
        target.OnUndoCapture();
        
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
