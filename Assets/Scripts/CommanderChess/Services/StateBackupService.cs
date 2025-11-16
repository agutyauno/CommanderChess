using System.Collections.Generic;
using UnityEngine;
using VContainer;
using CommanderChess.Domain;

namespace CommanderChess.Services
{
    public class StateBackupService
    {
        [Inject] readonly CarryingSystem carryingSystem;
        [Inject] readonly Board board;

        /// <summary>
        /// Simplified backup data for a single piece
        /// </summary>
        public struct PieceBackupData
        {
            public BoardCoord Position;
            public BasePiece Carrier;  // null if not carried
            public bool IsHero;
            public bool IsActive;  // Visual active state
        }

        /// <summary>
        /// Snapshot of game state at a point in time
        /// </summary>
        public class Snapshot
        {
            public Dictionary<BasePiece, PieceBackupData> PieceData { get; } = new();
            // BoardState removed - calculated from PieceData.Position
        }

        /// <summary>
        /// Create snapshot of entire board state
        /// </summary>
        public Snapshot CreateFullBoardSnapshot()
        {
            var snapshot = new Snapshot();

            foreach (var kvp in board.Pieces)
            {
                var piece = kvp.Value;
                snapshot.PieceData[piece] = new PieceBackupData
                {
                    Position = piece.Position,
                    Carrier = carryingSystem.GetCarrier(piece),
                    IsHero = piece.IsHero,
                    IsActive = piece.gameObject != null && piece.gameObject.activeSelf
                };
            }

            Debug.Log($"[Snapshot] Created: {snapshot.PieceData.Count} pieces");
            return snapshot;
        }

        /// <summary>
        /// Restore game state from snapshot
        /// </summary>
        public void RestoreSnapshot(Snapshot snapshot)
        {
            if (snapshot == null)
            {
                Debug.LogError("[Restore] Snapshot is null!");
                return;
            }

            // Phase 1: Clear board and detach all carrying
            board.Pieces.Clear();
            foreach (var piece in snapshot.PieceData.Keys)
            {
                carryingSystem.Detach(piece);
            }

            // Phase 2: Restore all pieces (position, visuals, board dictionary)
            foreach (var kvp in snapshot.PieceData)
            {
                var piece = kvp.Key;
                var data = kvp.Value;

                // Restore logic state
                piece.Position = data.Position;
                piece.IsHero = data.IsHero;

                // Restore visual state
                if (piece.gameObject != null)
                {
                    piece.gameObject.SetActive(data.IsActive);
                    piece.gameObject.transform.position = board.BoardCoordToWorld(data.Position);
                }

                // Add to board dictionary
                board.Pieces[data.Position] = piece;
            }

            // Phase 3: Restore carrying relationships (CarryingSystem handles order)
            foreach (var kvp in snapshot.PieceData)
            {
                var piece = kvp.Key;
                var data = kvp.Value;

                if (data.Carrier != null)
                {
                    if (!carryingSystem.TryAddCarry(data.Carrier, piece))
                    {
                        Debug.LogWarning($"[Restore] Failed: {data.Carrier.Type} -> {piece.Type}");
                    }
                }
            }

            // Recalculate all caches
            foreach (var piece in board.Pieces.Values)
            {
                piece.RecalculateCache();
            }

            // Minimal validation
            if (board.Pieces.Count != snapshot.PieceData.Count)
            {
                Debug.LogError($"[Restore] Count mismatch: {board.Pieces.Count} != {snapshot.PieceData.Count}");
            }
            else
            {
                Debug.Log($"[Restore] Success: {board.Pieces.Count} pieces restored");
            }
        }

        /// <summary>
        /// Validate snapshot restored correctly (minimal checks)
        /// </summary>
        public bool ValidateSnapshotRestored(Snapshot snapshot)
        {
            if (snapshot == null) return false;

            bool valid = true;

            // Check piece count
            if (board.Pieces.Count != snapshot.PieceData.Count)
            {
                Debug.LogError($"[Validate] Count mismatch: board={board.Pieces.Count}, snapshot={snapshot.PieceData.Count}");
                valid = false;
            }

            // Check critical piece data
            foreach (var kvp in snapshot.PieceData)
            {
                var piece = kvp.Key;
                var data = kvp.Value;

                if (piece.Position != data.Position)
                {
                    Debug.LogError($"[Validate] {piece.Type} position mismatch: {piece.Position} != {data.Position}");
                    valid = false;
                }

                if (!board.Pieces.ContainsKey(data.Position))
                {
                    Debug.LogError($"[Validate] Board missing {piece.Type} at {data.Position}");
                    valid = false;
                }
            }

            return valid;
        }
    }
}
