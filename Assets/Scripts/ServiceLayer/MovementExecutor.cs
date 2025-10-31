using System.Collections.Generic;
using UnityEngine;
using VContainer;

/// <summary>
/// MovementExecutor - Service xử lý việc cập nhật vị trí quân cờ
/// </summary>
public class MovementExecutor
{
    [Inject] readonly Board board;
    [Inject] readonly CarryingSystem carryingSystem;

    #region Movement Result

    public class MovementResult
    {
        public bool IsSuccess { get; private set; }
        public BasePiece Piece { get; private set; }
        public BoardCoord From { get; private set; }
        public BoardCoord To { get; private set; }
        public string ErrorMessage { get; private set; }

        private MovementResult() { }

        public static MovementResult Success(BasePiece piece, BoardCoord from, BoardCoord to)
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
                ErrorMessage = errorMessage
            };
        }

        public override string ToString()
        {
            if (IsSuccess)
                return $"Success: {Piece?.Type} {From.ToLabel()} → {To.ToLabel()}";
            return $"Failed: {ErrorMessage}";
        }
    }

    #endregion

    #region Simple Movement (Move Command)

    /// <summary>
    /// Di chuyển piece từ from sang to (ô trống)
    /// Tự động update carried pieces
    /// </summary>
    public MovementResult MovePiece(BasePiece piece, BoardCoord from, BoardCoord to)
    {
        if (piece == null)
            return MovementResult.Failed("Piece is null");

        if (!board.IsInBoard(from) || !board.IsInBoard(to))
            return MovementResult.Failed($"Invalid coordinates: {from} → {to}");

        // Kiểm tra destination phải trống
        if (board.Pieces.ContainsKey(to))
            return MovementResult.Failed($"Destination {to.ToLabel()} is occupied");

        // 1. Remove from old position
        board.Pieces.Remove(from);

        // 2. Place at new position
        board.Pieces[to] = piece;

        // 3. Update logical positions
        piece.Position = to;
        UpdateCarriedPiecesPositions(piece, to);

        // 4. Update visual positions
        UpdateVisualPosition(piece, to);
        UpdateCarriedVisualPositions(piece);

        // 5. Send event
        SendMoveEvent(piece, from, to);

        Debug.Log($"MovePiece: {piece.Team} {piece.Type} {from.ToLabel()} → {to.ToLabel()}");
        return MovementResult.Success(piece, from, to);
    }

    /// <summary>
    /// Revert piece movement (dùng cho Undo)
    /// </summary>
    public MovementResult RevertMovePiece(BasePiece piece, BoardCoord currentPos, BoardCoord previousPos)
    {
        return MovePiece(piece, currentPos, previousPos);
    }

    #endregion

    #region Capture Movement

    /// <summary>
    /// Thực hiện capture: remove defender, move attacker vào vị trí (nếu DoMoveToTarget = true)
    /// </summary>
    public MovementResult ExecuteCapture(BasePiece attacker, BasePiece defender, BoardCoord from, BoardCoord to)
    {
        if (attacker == null || defender == null)
            return MovementResult.Failed("Attacker or defender is null");

        if (!board.IsInBoard(from) || !board.IsInBoard(to))
            return MovementResult.Failed("Invalid coordinates");

        // 1. Remove defender from board
        RemoveFromBoard(defender);

        // 2. Move attacker (nếu DoMoveToTarget = true)
        if (attacker.DoMoveToTarget)
        {
            board.Pieces.Remove(from);
            board.Pieces[to] = attacker;

            attacker.Position = to;
            UpdateCarriedPiecesPositions(attacker, to);

            UpdateVisualPosition(attacker, to);
            UpdateCarriedVisualPositions(attacker);
        }
        else
        {
            // Attacker đứng yên (ví dụ: Pháo bắn từ xa)
            Debug.Log($"  Attacker stays at {from.ToLabel()} (DoMoveToTarget = false)");
        }

        // 3. Send events
        SendCaptureEvent(attacker, defender, from, to);

        Debug.Log($"ExecuteCapture: {attacker.Team} {attacker.Type} captures {defender.Team} {defender.Type}");
        return MovementResult.Success(attacker, from, to);
    }

    /// <summary>
    /// Revert capture (dùng cho Undo)
    /// Restore defender, move attacker về vị trí cũ (nếu cần)
    /// </summary>
    public MovementResult RevertCapture(BasePiece attacker, BasePiece defender, BoardCoord attackerOriginalPos, BoardCoord defenderPos)
    {
        // 1. Restore defender
        PlaceOnBoard(defender, defenderPos);

        // 2. Move attacker back (nếu đã di chuyển)
        if (attacker.DoMoveToTarget)
        {
            board.Pieces.Remove(defenderPos);
            board.Pieces[attackerOriginalPos] = attacker;

            attacker.Position = attackerOriginalPos;
            UpdateCarriedPiecesPositions(attacker, attackerOriginalPos);

            UpdateVisualPosition(attacker, attackerOriginalPos);
            UpdateCarriedVisualPositions(attacker);
        }

        Debug.Log($"RevertCapture: Restored {defender.Type} at {defenderPos.ToLabel()}");
        return MovementResult.Success(attacker, defenderPos, attackerOriginalPos);
    }

    #endregion

    #region Boarding Movement

    /// <summary>
    /// Thực hiện boarding: carrier di chuyển tới passenger, passenger trở thành carried
    /// Xử lý cả 2 trường hợp:
    /// - A mang B: A di chuyển tới B, B becomes carried
    /// - B mang A: A di chuyển tới B, A becomes carried
    /// </summary>
    public MovementResult ExecuteBoarding(BasePiece mover, BasePiece target, BoardCoord from, BoardCoord to)
    {
        if (mover == null || target == null)
            return MovementResult.Failed("Mover or target is null");

        // Xác định ai mang ai (đã được CarryingSystem.TryAddCarry xử lý)
        var carrier = carryingSystem.GetCarrier(mover);
        var passenger = carrier == target ? mover : target;
        var actualCarrier = carrier == target ? target : mover;

        Debug.Log($"ExecuteBoarding: {actualCarrier.Type} carries {passenger.Type}");

        // Case 1: Mover becomes passenger
        if (passenger == mover)
        {
            // Remove mover from board (becomes carried)
            board.Pieces.Remove(from);

            // Update positions
            mover.Position = to;
            UpdateCarriedPiecesPositions(mover, to);

            // Update visuals (passenger ẩn hoặc ở cùng vị trí carrier)
            UpdateVisualPosition(mover, to);
            UpdateCarriedVisualPositions(mover);

            Debug.Log($"  {mover.Type} becomes carried by {target.Type}");
        }
        // Case 2: Mover becomes carrier
        else
        {
            // Move carrier to target position
            board.Pieces.Remove(from);
            board.Pieces[to] = mover;

            // Update positions
            mover.Position = to;
            UpdateCarriedPiecesPositions(mover, to);

            // Target (passenger) is now carried
            target.Position = to;
            UpdateCarriedPiecesPositions(target, to);

            // Update visuals
            UpdateVisualPosition(mover, to);
            UpdateCarriedVisualPositions(mover);
            UpdateVisualPosition(target, to);
            UpdateCarriedVisualPositions(target);

            Debug.Log($"  {mover.Type} carries {target.Type}");
        }

        SendBoardingEvent(actualCarrier, passenger, from, to);
        return MovementResult.Success(mover, from, to);
    }

    /// <summary>
    /// Revert boarding (dùng cho Undo)
    /// </summary>
    public MovementResult RevertBoarding(BasePiece mover, BasePiece target, BoardCoord from, BoardCoord to, bool moverWasPassenger)
    {
        // Case 1: Mover was passenger
        if (moverWasPassenger)
        {
            // Place mover back on board
            PlaceOnBoard(mover, from);
        }
        // Case 2: Mover was carrier
        else
        {
            // Move mover back
            board.Pieces.Remove(to);
            board.Pieces[from] = mover;

            mover.Position = from;
            UpdateCarriedPiecesPositions(mover, from);

            // Place target back on board
            PlaceOnBoard(target, to);

            // Update visuals
            UpdateVisualPosition(mover, from);
            UpdateCarriedVisualPositions(mover);
            UpdateVisualPosition(target, to);
            UpdateCarriedVisualPositions(target);
        }

        Debug.Log($"RevertBoarding: {mover.Type} and {target.Type} separated");
        return MovementResult.Success(mover, to, from);
    }

    #endregion

    #region Detach Movement

    /// <summary>
    /// Thực hiện detach: tách passenger ra khỏi carrier, đặt tại vị trí mới
    /// </summary>
    public MovementResult ExecuteDetach(BasePiece passenger, BoardCoord carrierPos, BoardCoord passengerDestination)
    {
        if (passenger == null)
            return MovementResult.Failed("Passenger is null");

        if (!board.IsInBoard(passengerDestination))
            return MovementResult.Failed("Invalid destination");

        // Place passenger on board at new position
        PlaceOnBoard(passenger, passengerDestination);

        Debug.Log($"ExecuteDetach: {passenger.Type} detached to {passengerDestination.ToLabel()}");
        SendDetachEvent(passenger, carrierPos, passengerDestination);

        return MovementResult.Success(passenger, carrierPos, passengerDestination);
    }

    /// <summary>
    /// Revert detach (dùng cho Undo)
    /// </summary>
    public MovementResult RevertDetach(BasePiece passenger, BoardCoord passengerPos)
    {
        // Remove passenger from board (will be reattached by StateBackupService)
        RemoveFromBoard(passenger);

        Debug.Log($"RevertDetach: {passenger.Type} removed from board (will be reattached)");
        return MovementResult.Success(passenger, passengerPos, passenger.Position);
    }

    #endregion

    #region Basic Operations

    /// <summary>
    /// Remove piece khỏi board
    /// </summary>
    public void RemoveFromBoard(BasePiece piece)
    {
        if (piece == null) return;

        board.Pieces.Remove(piece.Position);
        Debug.Log($"RemoveFromBoard: {piece.Type} at {piece.Position.ToLabel()}");
    }

    /// <summary>
    /// Place piece lên board tại vị trí
    /// </summary>
    public bool PlaceOnBoard(BasePiece piece, BoardCoord position)
    {
        if (piece == null || !board.IsInBoard(position))
            return false;

        board.Pieces[position] = piece;
        piece.Position = position;
        UpdateCarriedPiecesPositions(piece, position);

        UpdateVisualPosition(piece, position);
        UpdateCarriedVisualPositions(piece);

        Debug.Log($"PlaceOnBoard: {piece.Type} at {position.ToLabel()}");
        return true;
    }

    /// <summary>
    /// Update position only (không touch board dictionary)
    /// Dùng khi piece đang được mang
    /// </summary>
    public void UpdatePositionOnly(BasePiece piece, BoardCoord newPosition)
    {
        if (piece == null) return;

        piece.Position = newPosition;
        UpdateCarriedPiecesPositions(piece, newPosition);

        UpdateVisualPosition(piece, newPosition);
        UpdateCarriedVisualPositions(piece);
    }

    /// <summary>
    /// Teleport piece (instant, no animation)
    /// </summary>
    public void TeleportPiece(BasePiece piece, BoardCoord to)
    {
        if (piece == null) return;

        var from = piece.Position;

        board.Pieces.Remove(from);
        board.Pieces[to] = piece;

        piece.Position = to;
        UpdateCarriedPiecesPositions(piece, to);

        piece.transform.position = board.BoardCoordToWorld(to);
        UpdateCarriedVisualPositions(piece);

        Debug.Log($"TeleportPiece: {piece.Type} {from.ToLabel()} → {to.ToLabel()}");
    }

    #endregion

    #region Position Updates

    private void UpdateCarriedPiecesPositions(BasePiece carrier, BoardCoord position)
    {
        var carried = carryingSystem.GetAllCarriedPieces(carrier);
        foreach (var p in carried)
        {
            p.Position = position;
        }
    }

    private void UpdateVisualPosition(BasePiece piece, BoardCoord position)
    {
        if (piece.transform == null) return;
        piece.transform.position = board.BoardCoordToWorld(position);
    }

    private void UpdateCarriedVisualPositions(BasePiece carrier)
    {
        var carried = carryingSystem.GetAllCarriedPieces(carrier);
        foreach (var p in carried)
        {
            if (p.transform != null)
            {
                // Carried pieces ở cùng vị trí với carrier
                // Hoặc có thể thêm offset nếu muốn hiển thị stack
                p.transform.position = carrier.transform.position;
            }
        }
    }

    #endregion

    #region Events

    private void SendMoveEvent(BasePiece piece, BoardCoord from, BoardCoord to)
    {
        // TODO: Implement với EventBus hoặc UnityEvent
        // eventBus.Publish(new PieceMovedEvent(piece, from, to));
    }

    private void SendCaptureEvent(BasePiece attacker, BasePiece defender, BoardCoord from, BoardCoord to)
    {
        // TODO: Implement với EventBus
        // eventBus.Publish(new PieceCapturedEvent(attacker, defender, from, to));
    }

    private void SendBoardingEvent(BasePiece carrier, BasePiece passenger, BoardCoord from, BoardCoord to)
    {
        // TODO: Implement với EventBus
        // eventBus.Publish(new PieceBoardedEvent(carrier, passenger, from, to));
    }

    private void SendDetachEvent(BasePiece passenger, BoardCoord from, BoardCoord to)
    {
        // TODO: Implement với EventBus
        // eventBus.Publish(new PieceDetachedEvent(passenger, from, to));
    }

    #endregion
}