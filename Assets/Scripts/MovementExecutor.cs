using System.Collections.Generic;
using UnityEngine;
using VContainer;

/// <summary>
/// MovementExecutor - Service xử lý việc cập nhật vị trí quân cờ trên bàn cờ
/// 
/// TRÁCH NHIỆM:
/// - Cập nhật position của piece và các carried pieces
/// - Cập nhật board dictionary
/// - Gửi events thông báo di chuyển
/// - Xử lý visual position (transform.position)
/// 
/// KHÔNG TRÁCH NHIỆM:
/// - Validation (do Command xử lý)
/// - Backup/Restore (do StateBackupService xử lý)
/// - Game logic (hero, ring of fire, etc.)
/// </summary>
public class MovementExecutor
{
    [Inject] readonly Board board;
    [Inject] readonly CarryingSystem carryingSystem;
    // TODO: Inject EventBus khi implement
    // [Inject] readonly EventBus eventBus;

    #region Public API

    /// <summary>
    /// Di chuyển piece từ vị trí from sang to
    /// Tự động update cả carried pieces
    /// </summary>
    public MovementResult MovePiece(Piece piece, BoardCoord from, BoardCoord to)
    {
        if (piece == null)
        {
            Debug.LogError("MovementExecutor: piece is null");
            return MovementResult.Failed("Piece is null");
        }

        if (!board.IsInBoard(from) || !board.IsInBoard(to))
        {
            Debug.LogError($"MovementExecutor: Invalid coordinates - from: {from}, to: {to}");
            return MovementResult.Failed("Invalid coordinates");
        }

        // 1. Update board dictionary
        UpdateBoardDictionary(piece, from, to);

        // 2. Update piece positions (logic)
        UpdatePiecePosition(piece, to);

        // 3. Update carried pieces positions
        UpdateCarriedPiecesPositions(piece, to);

        // 4. Update visual positions (transform)
        UpdateVisualPosition(piece, to);
        UpdateCarriedVisualPositions(piece);

        // 5. Send events
        SendMoveEvent(piece, from, to);

        return MovementResult.Success(piece, from, to);
    }

    /// <summary>
    /// Di chuyển piece về vị trí cũ (thường dùng cho undo)
    /// </summary>
    public MovementResult RevertMovePiece(Piece piece, BoardCoord currentPos, BoardCoord previousPos)
    {
        return MovePiece(piece, currentPos, previousPos);
    }

    /// <summary>
    /// Chỉ update position logic, không touch board dictionary
    /// Dùng khi piece được mang theo carrier
    /// </summary>
    public void UpdatePositionOnly(Piece piece, BoardCoord newPosition)
    {
        if (piece == null) return;

        UpdatePiecePosition(piece, newPosition);
        UpdateCarriedPiecesPositions(piece, newPosition);
        UpdateVisualPosition(piece, newPosition);
        UpdateCarriedVisualPositions(piece);
    }

    /// <summary>
    /// Remove piece khỏi board (dùng khi bị capture hoặc boarding)
    /// </summary>
    public void RemoveFromBoard(Piece piece)
    {
        if (piece == null) return;

        board.Pieces.Remove(piece.Position);
        Debug.Log($"MovementExecutor: Removed {piece.Type} from board at {piece.Position.ToLabel()}");
    }

    /// <summary>
    /// Place piece lên board tại vị trí mới
    /// </summary>
    public bool PlaceOnBoard(Piece piece, BoardCoord position)
    {
        if (piece == null || !board.IsInBoard(position))
        {
            return false;
        }

        board.Pieces[position] = piece;
        UpdatePiecePosition(piece, position);
        UpdateVisualPosition(piece, position);
        
        Debug.Log($"MovementExecutor: Placed {piece.Type} on board at {position.ToLabel()}");
        return true;
    }

    /// <summary>
    /// Teleport piece (không gửi animation event, instant move)
    /// </summary>
    public void TeleportPiece(Piece piece, BoardCoord to)
    {
        if (piece == null) return;

        var from = piece.Position;
        
        // Update board
        board.Pieces.Remove(from);
        board.Pieces[to] = piece;

        // Update positions
        UpdatePiecePosition(piece, to);
        UpdateCarriedPiecesPositions(piece, to);

        // Update visual (instant, no animation)
        piece.transform.position = board.BoardCoordToWorld(to);
        UpdateCarriedVisualPositions(piece);

        Debug.Log($"MovementExecutor: Teleported {piece.Type} from {from.ToLabel()} to {to.ToLabel()}");
    }

    #endregion

    #region Board Dictionary Management

    private void UpdateBoardDictionary(Piece piece, BoardCoord from, BoardCoord to)
    {
        // Remove from old position
        if (board.Pieces.TryGetValue(from, out var pieceAtFrom) && pieceAtFrom == piece)
        {
            board.Pieces.Remove(from);
        }

        // Add to new position
        board.Pieces[to] = piece;
    }

    #endregion

    #region Position Updates (Logic)

    private void UpdatePiecePosition(Piece piece, BoardCoord newPosition)
    {
        piece.Position = newPosition;
    }

    private void UpdateCarriedPiecesPositions(Piece carrier, BoardCoord position)
    {
        var carriedPieces = carryingSystem.GetAllCarriedPieces(carrier);
        
        foreach (var carried in carriedPieces)
        {
            carried.Position = position;
        }
    }

    #endregion

    #region Visual Position Updates (Transform)

    private void UpdateVisualPosition(Piece piece, BoardCoord position)
    {
        if (piece.transform == null) return;

        Vector3 worldPos = board.BoardCoordToWorld(position);
        piece.transform.position = worldPos;
    }

    private void UpdateCarriedVisualPositions(Piece carrier)
    {
        var carriedPieces = carryingSystem.GetAllCarriedPieces(carrier);
        
        foreach (var carried in carriedPieces)
        {
            if (carried.transform != null)
            {
                // Carried pieces có thể có offset visual
                // Hoặc ẩn đi (tùy design)
                carried.transform.position = carrier.transform.position;
            }
        }
    }

    #endregion

    #region Events

    private void SendMoveEvent(Piece piece, BoardCoord from, BoardCoord to)
    {
        // TODO: Implement event system
        // eventBus.Publish(new PieceMovedEvent(piece, from, to));
        
        Debug.Log($"MovementExecutor: {piece.Team} {piece.Type} moved from {from.ToLabel()} to {to.ToLabel()}");
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Result object cho movement operations
    /// </summary>
    public class MovementResult
    {
        public bool IsSuccess { get; private set; }
        public Piece Piece { get; private set; }
        public BoardCoord From { get; private set; }
        public BoardCoord To { get; private set; }
        public string ErrorMessage { get; private set; }

        private MovementResult() { }

        public static MovementResult Success(Piece piece, BoardCoord from, BoardCoord to)
        {
            return new MovementResult
            {
                IsSuccess = true,
                Piece = piece,
                From = from,
                To = to,
                ErrorMessage = null
            };
        }

        public static MovementResult Failed(string errorMessage)
        {
            return new MovementResult
            {
                IsSuccess = false,
                Piece = null,
                From = default,
                To = default,
                ErrorMessage = errorMessage
            };
        }

        public override string ToString()
        {
            if (IsSuccess)
            {
                return $"Movement Success: {Piece.Type} {From.ToLabel()} → {To.ToLabel()}";
            }
            return $"Movement Failed: {ErrorMessage}";
        }
    }

    #endregion
}