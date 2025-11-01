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

        public struct PieceBackupData
        {
            public BoardCoord Position;
            public BasePiece Carrier;
            public List<BasePiece> Carrying;
            public bool IsHero;
            public bool ExistsOnBoard;
            public bool IsActive; // Track if piece is visually active
        }

        public class Snapshot
        {
            public Dictionary<BasePiece, PieceBackupData> PieceData { get; } = new();
            public Dictionary<BoardCoord, BasePiece> BoardState { get; } = new();
        }

        /// <summary>
        /// Backup state của một piece và tất cả pieces liên quan (carried pieces)
        /// </summary>
        public Snapshot CreateSnapshot(params BasePiece[] pieces)
        {
            // Log chi tiết hơn
            if (pieces == null)
            {
                Debug.LogWarning("CreateSnapshot: pieces array is null");
                return null;
            }

            if (pieces.Length == 0)
            {
                Debug.LogWarning("CreateSnapshot: pieces array is empty");
                return null;
            }

            var snapshot = new Snapshot();
            var piecesToBackup = new HashSet<BasePiece>();

            // Collect all pieces cần backup
            foreach (var piece in pieces)
            {
                if (piece == null)
                {
                    Debug.LogWarning("CreateSnapshot: null piece in array, skipping");
                    continue;
                }

                piecesToBackup.Add(piece);
                piecesToBackup.UnionWith(carryingSystem.GetAllCarriedPieces(piece));
            }

            // Kiểm tra sau khi collect
            if (piecesToBackup.Count == 0)
            {
                Debug.LogWarning("CreateSnapshot: no valid pieces to backup");
                return null;
            }

            Debug.Log($"Creating snapshot for {piecesToBackup.Count} pieces");

            // Backup từng piece
            foreach (var piece in piecesToBackup)
            {
                snapshot.PieceData[piece] = new PieceBackupData
                {
                    Position = piece.Position,
                    Carrier = carryingSystem.GetCarrier(piece),
                    Carrying = new List<BasePiece>(carryingSystem.GetDirectCarrying(piece)),
                    IsHero = piece.IsHero,
                    ExistsOnBoard = board.Pieces.ContainsValue(piece),
                    IsActive = piece.gameObject.activeSelf // Store active state
                };
            }

            // Backup board state
            foreach (var piece in piecesToBackup)
            {
                var pos = piece.Position;
                if (board.Pieces.TryGetValue(pos, out var occupant))
                {
                    snapshot.BoardState[pos] = occupant;
                }
            }

            Debug.Log($"Snapshot created: {snapshot.PieceData.Count} pieces, {snapshot.BoardState.Count} board positions");
            return snapshot;
        }

        /// <summary>
        /// Backup toàn bộ board state (dùng cho các tình huống phức tạp)
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
                    Carrying = new List<BasePiece>(carryingSystem.GetDirectCarrying(piece)),
                    IsHero = piece.IsHero,
                    ExistsOnBoard = true
                };
            }

            foreach (var kvp in board.Pieces)
            {
                snapshot.BoardState[kvp.Key] = kvp.Value;
            }

            return snapshot;
        }

        /// <summary>
        /// Restore state từ snapshot
        /// </summary>
        public void RestoreSnapshot(Snapshot snapshot)
        {
            if (snapshot == null) return;

            // Phase 1: Detach all carrying relationships
            foreach (var piece in snapshot.PieceData.Keys)
            {
                carryingSystem.Detach(piece);
            }

            // Phase 2: Restore positions, board state and visual state
            foreach (var kvp in snapshot.PieceData)
            {
                var piece = kvp.Key;
                var data = kvp.Value;

                piece.Position = data.Position;
                piece.IsHero = data.IsHero;

                // Restore visual state
                if (piece.gameObject != null)
                {
                    piece.gameObject.SetActive(data.IsActive);
                }
            }

            // Clear and restore board state
            var positionsToUpdate = new HashSet<BoardCoord>();
            foreach (var piece in snapshot.PieceData.Keys)
            {
                positionsToUpdate.Add(piece.Position);
            }

            foreach (var pos in positionsToUpdate)
            {
                board.Pieces.Remove(pos);
            }

            foreach (var kvp in snapshot.BoardState)
            {
                if (snapshot.PieceData[kvp.Value].ExistsOnBoard)
                {
                    board.Pieces[kvp.Key] = kvp.Value;
                }
            }

            // Phase 3: Restore carrying relationships
            // Phải restore theo thứ tự: root carriers trước, carried pieces sau
            var restored = new HashSet<BasePiece>();
            var queue = new Queue<BasePiece>(snapshot.PieceData.Keys);

            while (queue.Count > 0)
            {
                var piece = queue.Dequeue();
                var data = snapshot.PieceData[piece];

                // Nếu piece này cần có carrier nhưng carrier chưa được restore
                if (data.Carrier != null && !restored.Contains(data.Carrier))
                {
                    queue.Enqueue(piece); // Đợi carrier restore trước
                    continue;
                }

                // Restore carrier relationship
                if (data.Carrier != null)
                {
                    carryingSystem.TryAddCarry(data.Carrier, piece);
                }

                restored.Add(piece);
            }

            // Phase 4: Recalculate cache cho tất cả pieces
            foreach (var piece in snapshot.PieceData.Keys)
            {
                piece.RecalculateCache();
            }
        }

        /// <summary>
        /// So sánh hai snapshots để debug
        /// </summary>
        public bool AreSnapshotsEqual(Snapshot a, Snapshot b)
        {
            if (a.PieceData.Count != b.PieceData.Count) return false;

            foreach (var kvp in a.PieceData)
            {
                var piece = kvp.Key;
                if (!b.PieceData.ContainsKey(piece)) return false;

                var dataA = kvp.Value;
                var dataB = b.PieceData[piece];

                if (dataA.Position != dataB.Position) return false;
                if (dataA.Carrier != dataB.Carrier) return false;
                if (dataA.IsHero != dataB.IsHero) return false;
            }

            return true;
        }
    }
}
