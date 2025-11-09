using System.Collections.Generic;
using UnityEngine;
using VContainer;
using CommanderChess.Domain;

namespace CommanderChess.Services
{
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
        /// Tự động xử lý carried pieces - chúng di chuyển theo carrier
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

            // 3. Update logical positions (piece + all carried)
            piece.Position = to;
            UpdateCarriedPiecesPositions(piece, to);

            // 4. Update visual positions (piece + all carried)
            UpdateVisualPosition(piece, to);
            UpdateCarriedVisualPositions(piece);

            // 5. Send event
            SendMoveEvent(piece, from, to);
            return MovementResult.Success(piece, from, to);
        }

        /// <summary>
        /// Revert piece movement (dùng cho Undo)
        /// Tự động xử lý carried pieces
        /// </summary>
        public MovementResult RevertMovePiece(BasePiece piece, BoardCoord currentPos, BoardCoord previousPos)
        {
            return MovePiece(piece, currentPos, previousPos);
        }

        #endregion

        #region Capture Movement

        /// <summary>
        /// Thực hiện capture: remove defender (+ carried), move attacker (+ carried) vào vị trí
        /// </summary>
        public MovementResult ExecuteCapture(BasePiece attacker, BasePiece defender, BoardCoord from, BoardCoord to, bool shouldMoveToTarget)
        {
            if (attacker == null || defender == null)
        return MovementResult.Failed("Attacker or defender is null");

    if (!board.IsInBoard(from) || !board.IsInBoard(to))
        return MovementResult.Failed("Invalid coordinates");

    // 1. Remove defender + all carried pieces from board
    ShotDownPiece(defender);

    // 2. Move attacker + all carried pieces (CHỈ NÕU shouldMoveToTarget = true)
    if (shouldMoveToTarget)
    {
        board.Pieces.Remove(from);
        board.Pieces[to] = attacker;

        // Update positions for attacker + all carried
        attacker.Position = to;
        UpdateCarriedPiecesPositions(attacker, to);

        // Update visuals for attacker + all carried
        UpdateVisualPosition(attacker, to);
        UpdateCarriedVisualPositions(attacker);
        
        Debug.Log($"Attacker moved from {from.ToLabel()} to {to.ToLabel()}");
    }
    else
    {
        // Attacker stays at original position (ranged attack)
        Debug.Log($"Attacker stays at {from.ToLabel()} (ranged attack)");
    }

    // 3. Send events
    SendCaptureEvent(attacker, defender, from, to);

    return MovementResult.Success(attacker, from, to);
        }

        /// <summary>
        /// Revert capture (dùng cho Undo)
        /// Restore defender (+ carried), move attacker (+ carried) về vị trí cũ
        /// </summary>
        public MovementResult RevertCapture(BasePiece attacker, BasePiece defender, BoardCoord attackerOriginalPos, BoardCoord defenderPos, bool attackerHadMoved)
        {
           // 1. Restore defender + carried pieces được xử lý bởi StateBackupService
    PlaceOnBoard(defender, defenderPos);

    // Defender's carried pieces positions sẽ được restore bởi StateBackupService
    // Chỉ cần update visual
    UpdateVisualPosition(defender, defenderPos);
    UpdateCarriedVisualPositions(defender);

    // 2. Move attacker + carried pieces back (CHỈ NẾU attacker đã di chuyển)
    if (attackerHadMoved)
    {
        board.Pieces.Remove(defenderPos);
        board.Pieces[attackerOriginalPos] = attacker;

        // Update positions for attacker + carried
        attacker.Position = attackerOriginalPos;
        UpdateCarriedPiecesPositions(attacker, attackerOriginalPos);

        // Update visuals for attacker + carried
        UpdateVisualPosition(attacker, attackerOriginalPos);
        UpdateCarriedVisualPositions(attacker);
        
        Debug.Log($"Attacker moved back from {defenderPos.ToLabel()} to {attackerOriginalPos.ToLabel()}");
    }
    else
    {
        // Attacker didn't move, nothing to revert
        Debug.Log($"Attacker stayed at {attackerOriginalPos.ToLabel()}, no position change");
    }

    return MovementResult.Success(attacker, defenderPos, attackerOriginalPos);
        }

        #endregion

        #region Boarding Movement

        /// <summary>
        /// Thực hiện boarding: carrier di chuyển tới passenger, passenger trở thành carried
        /// Xử lý cả trường hợp cả 2 pieces đều đang mang quân khác
        /// 
        /// Lưu ý quan trọng:
        /// - Nếu mover đang mang quân → các quân đó vẫn theo mover sau khi boarding
        /// - Nếu target đang mang quân → các quân đó vẫn ở với target
        /// - CarryingSystem.TryAddCarry() sẽ tự động redistribute nếu cần
        /// </summary>
        public MovementResult ExecuteBoarding(BasePiece mover, BasePiece target, BoardCoord from, BoardCoord to, bool moverBecomesPassenger)
        {
            try
            {
                if (mover == null || target == null)
                    return MovementResult.Failed("Mover or target is null");

                // Case 1: Mover becomes passenger
                if (moverBecomesPassenger)
                {
                    // Remove mover from board (becomes carried)
                    board.Pieces.Remove(from);

                    // Update logical position
                    mover.Position = to;
                    UpdateCarriedPiecesPositions(mover, to);

                    // Update visual with offset for passenger
                    Vector3 carrierPos = board.BoardCoordToWorld(to);
                    UpdateCarriedVisualPositions(target);

                    SendBoardingEvent(target, mover, from, to);
                }
                // Case 2: Mover becomes carrier
                else
                {
                    // Move carrier to target position
                    board.Pieces.Remove(from);
                    board.Pieces[to] = mover;

                    // Update carrier position & visuals (centered)
                    mover.Position = to;
                    UpdateCarriedPiecesPositions(mover, to);
                    UpdateVisualPosition(mover, to);

                    // Update passenger position & visuals (with offset)
                    target.Position = to;
                    UpdateCarriedPiecesPositions(target, to);

                    Vector3 carrierPos = board.BoardCoordToWorld(to);
                    UpdateCarriedVisualPositions(mover);

                    SendBoardingEvent(mover, target, from, to);
                }

                return MovementResult.Success(mover, from, to);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ExecuteBoarding failed: {e.Message}\n{e.StackTrace}");
                return MovementResult.Failed($"Exception: {e.Message}");
            }
        }

        /// <summary>
        /// Revert boarding (dùng cho Undo)
        /// Xử lý cả trường hợp có carried pieces
        /// </summary>
        public MovementResult RevertBoarding(BasePiece mover, BasePiece target, BoardCoord from, BoardCoord to, bool moverWasPassenger)
        {
            // Case 1: Mover was passenger
            if (moverWasPassenger)
            {
                // Place mover back on board (+ its carried pieces)
                PlaceOnBoard(mover, from);

                // Update positions for mover + carried
                UpdateCarriedPiecesPositions(mover, from);
                UpdateVisualPosition(mover, from);
                UpdateCarriedVisualPositions(mover);
            }
            // Case 2: Mover was carrier
            else
            {
                // Move mover back to original position (+ its original carried)
                board.Pieces.Remove(to);
                board.Pieces[from] = mover;

                mover.Position = from;
                UpdateCarriedPiecesPositions(mover, from);

                // Place target back on board (+ its carried)
                PlaceOnBoard(target, to);
                UpdateCarriedPiecesPositions(target, to);

                // Update visuals
                UpdateVisualPosition(mover, from);
                UpdateCarriedVisualPositions(mover);
                UpdateVisualPosition(target, to);
                UpdateCarriedVisualPositions(target);
            }

            return MovementResult.Success(mover, to, from);
        }

        #endregion

        #region Detach Movement

        /// <summary>
        /// Thực hiện detach: tách passenger (+ carried của passenger) ra khỏi carrier
        /// 
        /// Lưu ý: Nếu passenger đang mang quân khác, các quân đó vẫn theo passenger
        /// </summary>
        public MovementResult ExecuteDetach(BasePiece passenger, BoardCoord carrierPos, BoardCoord passengerDestination)
        {
            if (passenger == null)
                return MovementResult.Failed("Passenger is null");

            if (!board.IsInBoard(passengerDestination))
                return MovementResult.Failed("Invalid destination");

            // Place passenger on board at new position
            PlaceOnBoard(passenger, passengerDestination);

            // Update positions for passenger + all its carried pieces
            UpdateCarriedPiecesPositions(passenger, passengerDestination);

            // Update visuals for passenger + carried
            UpdateVisualPosition(passenger, passengerDestination);
            UpdateCarriedVisualPositions(passenger);

            SendDetachEvent(passenger, carrierPos, passengerDestination);
            return MovementResult.Success(passenger, carrierPos, passengerDestination);
        }

        /// <summary>
        /// Revert detach (dùng cho Undo)
        /// Xử lý cả trường hợp passenger có carried pieces
        /// </summary>
        public MovementResult RevertDetach(BasePiece passenger, BoardCoord passengerPos)
        {
            // Remove passenger from board (will be reattached by StateBackupService)
            RemoveFromBoard(passenger);

            return MovementResult.Success(passenger, passengerPos, passenger.Position);
        }

        #endregion

        #region Basic Operations

        /// <summary>
        /// Remove piece (+ all carried) khỏi board
        /// </summary>
        public void RemoveFromBoard(BasePiece piece)
        {
            if (piece == null) return;
            board.Pieces.Remove(piece.Position);
        }

        /// <summary>
        /// Place piece (+ visual update cho carried) lên board
        /// </summary>
        public bool PlaceOnBoard(BasePiece piece, BoardCoord position)
        {
            if (piece == null || !board.IsInBoard(position))
                return false;

            if (board.Pieces.ContainsKey(position))
                return false;
            board.Pieces[position] = piece;
            piece.Position = position;
            UpdateCarriedPiecesPositions(piece, position);

            UpdateVisualPosition(piece, position);
            UpdateCarriedVisualPositions(piece);
            return true;
        }

        /// <summary>
        /// Update position only (không touch board dictionary)
        /// Dùng khi piece đang được mang hoặc special cases
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
        /// Teleport piece (+ carried) - instant, no animation
        /// </summary>
        public void TeleportPiece(BasePiece piece, BoardCoord to)
        {
            if (piece == null) return;

            var from = piece.Position;

            board.Pieces.Remove(from);
            board.Pieces[to] = piece;

            piece.Position = to;
            UpdateCarriedPiecesPositions(piece, to);

            // Instant visual update (no animation)
            piece.transform.position = board.BoardCoordToWorld(to);
            UpdateCarriedVisualPositions(piece);
        }

        /// <summary>
        /// Thực hiện shot down: remove piece (+ carried) khỏi board và disable visuals
        /// </summary>
        public void ShotDownPiece(BasePiece piece)
        {
            if (piece == null) return;
            var carriedPieces = carryingSystem.GetAllCarriedPieces(piece);

            // Remove from board
            RemoveFromBoard(piece);

            // Disable visuals for main piece and carried pieces
            if (piece.gameObject != null)
            {
                piece.gameObject.SetActive(false);
            }

            foreach (var carried in carriedPieces)
            {
                if (carried != null)
                {
                    carried.gameObject.SetActive(false);
                }
            }

            Debug.Log($"Shot down: {piece.Type} at {piece.Position.ToLabel()} (+ {carriedPieces.Count} carried pieces)");
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
            Vector3 offset = new();
            var carried = carryingSystem.GetAllCarriedPieces(carrier);
            foreach (var p in carried)
            {
                if (p.transform != null)
                {
                    offset += new Vector3(-0.25f, -0.25f, -0.1f);
                    p.transform.position = carrier.transform.position + offset;
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
}