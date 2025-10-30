using VContainer;

/// <summary>
/// ActionValidator - Validate các hành động trước khi tạo command
/// </summary>
public class ActionValidator
{
    [Inject] readonly Board board;
    [Inject] readonly TurnManager turnManager;
    [Inject] readonly CarryingSystem carryingSystem;

    #region Validation Result Struct
    public struct ValidationResult
    {
        public bool IsValid;
        public string Reason;

        public static ValidationResult Valid() => new ValidationResult { IsValid = true, Reason = null };
        public static ValidationResult Invalid(string reason) => new ValidationResult { IsValid = false, Reason = reason };
    }
    #endregion

    #region Move Validation

    /// <summary>
    /// Validate hành động di chuyển
    /// </summary>
    public ValidationResult ValidateMove(BasePiece piece, BoardCoord to)
    {
        // Kiểm tra piece không null
        if (piece == null)
            return ValidationResult.Invalid("Piece is null");

        // Kiểm tra có phải lượt của piece này không
        if (!turnManager.IsCurrentPlayerPiece(piece))
            return ValidationResult.Invalid($"Not {piece.Team}'s turn");

        // Kiểm tra tọa độ đích có trong bàn cờ không
        if (!board.IsInBoard(to))
            return ValidationResult.Invalid("Target position is out of board");

        // Kiểm tra tọa độ đích có quân cờ khác không (move chỉ đến ô trống)
        if (board.Pieces.ContainsKey(to))
            return ValidationResult.Invalid("Target position is occupied");

        // Kiểm tra có trong danh sách nước đi hợp lệ không
        if (!piece.PossibleMoves.Contains(to))
            return ValidationResult.Invalid("Target position is not in valid moves");

        return ValidationResult.Valid();
    }

    #endregion

    #region Capture Validation

    /// <summary>
    /// Validate hành động tấn công
    /// </summary>
    public ValidationResult ValidateCapture(BasePiece attacker, BoardCoord targetPos)
    {
        // Kiểm tra attacker không null
        if (attacker == null)
            return ValidationResult.Invalid("Attacker is null");

        // Kiểm tra có phải lượt của attacker không
        if (!turnManager.IsCurrentPlayerPiece(attacker))
            return ValidationResult.Invalid($"Not {attacker.Team}'s turn");

        // Kiểm tra tọa độ đích có trong bàn cờ không
        if (!board.IsInBoard(targetPos))
            return ValidationResult.Invalid("Target position is out of board");

        // Kiểm tra có quân địch ở vị trí đích không
        if (!board.TryGetPiece(targetPos, out var target))
            return ValidationResult.Invalid("No piece at target position");

        if (target.Team == attacker.Team)
            return ValidationResult.Invalid("Cannot attack ally piece");

        // Kiểm tra có trong danh sách tấn công hợp lệ không
        if (!attacker.PossibleAttacks.Contains(targetPos))
            return ValidationResult.Invalid("Target position is not in valid attacks");

        return ValidationResult.Valid();
    }

    #endregion

    #region Boarding Validation

    /// <summary>
    /// Validate hành động boarding (mang theo đồng minh)
    /// </summary>
    public ValidationResult ValidateBoarding(BasePiece carrier, BoardCoord targetPos)
    {
        // Kiểm tra carrier không null
        if (carrier == null)
            return ValidationResult.Invalid("Carrier is null");

        // Kiểm tra có phải lượt của carrier không
        if (!turnManager.IsCurrentPlayerPiece(carrier))
            return ValidationResult.Invalid($"Not {carrier.Team}'s turn");

        // Kiểm tra tọa độ đích có trong bàn cờ không
        if (!board.IsInBoard(targetPos))
            return ValidationResult.Invalid("Target position is out of board");

        // Kiểm tra có quân đồng minh ở vị trí đích không
        if (!board.TryGetPiece(targetPos, out var passenger))
            return ValidationResult.Invalid("No piece at target position");

        if (passenger.Team != carrier.Team)
            return ValidationResult.Invalid("Cannot board enemy piece");

        // Kiểm tra có trong danh sách nước đi hợp lệ không
        if (!carrier.PossibleMoves.Contains(targetPos))
            return ValidationResult.Invalid("Target position is not in valid moves");

        // Kiểm tra carrier có thể mang passenger không
        if (!carryingSystem.CanCarryDirectly(carrier, passenger) && 
            !carryingSystem.CanCarryDirectly(passenger, carrier))
        {
            return ValidationResult.Invalid($"{carrier.Type} cannot carry {passenger.Type} (or vice versa)");
        }

        // Kiểm tra group size constraint
        int carrierGroupSize = carryingSystem.CountGroupSize(carrier);
        int passengerGroupSize = carryingSystem.CountGroupSize(passenger);
        if (carrierGroupSize + passengerGroupSize > 3)
        {
            return ValidationResult.Invalid($"Group size would exceed limit: {carrierGroupSize} + {passengerGroupSize} > 3");
        }

        return ValidationResult.Valid();
    }

    #endregion

    #region Detach Validation

    /// <summary>
    /// Validate hành động detach (tách quân ra khỏi carrier)
    /// </summary>
    public ValidationResult ValidateDetach(BasePiece carrier, BasePiece passenger, BoardCoord to)
    {
        // Kiểm tra carrier và passenger không null
        if (carrier == null)
            return ValidationResult.Invalid("Carrier is null");

        if (passenger == null)
            return ValidationResult.Invalid("Passenger is null");

        // Kiểm tra có phải lượt của carrier không
        if (!turnManager.IsCurrentPlayerPiece(carrier))
            return ValidationResult.Invalid($"Not {carrier.Team}'s turn");

        // Kiểm tra passenger có đang được carrier mang không
        var carriedPieces = carryingSystem.GetDirectCarrying(carrier);
        if (!carriedPieces.Contains(passenger))
            return ValidationResult.Invalid($"{carrier.Type} is not carrying {passenger.Type}");

        // Kiểm tra tọa độ đích có trong bàn cờ không
        if (!board.IsInBoard(to))
            return ValidationResult.Invalid("Target position is out of board");

        // Kiểm tra tọa độ đích có trống không
        if (board.Pieces.ContainsKey(to))
            return ValidationResult.Invalid("Target position is occupied");

        return ValidationResult.Valid();
    }

    #endregion
}