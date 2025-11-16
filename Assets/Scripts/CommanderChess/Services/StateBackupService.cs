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

            Debug.Log($"[CreateFullBoardSnapshot] Creating snapshot of {board.Pieces.Count} pieces");

            foreach (var kvp in board.Pieces)
            {
                var piece = kvp.Value;
                snapshot.PieceData[piece] = new PieceBackupData
                {
                    Position = piece.Position,
                    Carrier = carryingSystem.GetCarrier(piece),
                    Carrying = new List<BasePiece>(carryingSystem.GetDirectCarrying(piece)),
                    IsHero = piece.IsHero,
                    ExistsOnBoard = true,
                    IsActive = piece.gameObject != null && piece.gameObject.activeSelf // FIX: Must set IsActive!
                };

                Debug.Log($"  Backup: {piece.Type} at {piece.Position}, Active: {piece.gameObject?.activeSelf}");
            }

            foreach (var kvp in board.Pieces)
            {
                snapshot.BoardState[kvp.Key] = kvp.Value;
            }

            Debug.Log($"[CreateFullBoardSnapshot] Snapshot created: {snapshot.PieceData.Count} pieces, {snapshot.BoardState.Count} board positions");
            return snapshot;
        }

        /// <summary>
        /// Restore state từ snapshot
        /// CRITICAL: Đảm bảo restore chính xác positions, visuals, và board dictionary
        /// </summary>
        public void RestoreSnapshot(Snapshot snapshot)
        {
            if (snapshot == null)
            {
                Debug.LogError("RestoreSnapshot: snapshot is null!");
                return;
            }

            Debug.Log($"[RestoreSnapshot] Starting restore of {snapshot.PieceData.Count} pieces, {snapshot.BoardState.Count} board positions");

            // Phase 1: Detach all carrying relationships
            foreach (var piece in snapshot.PieceData.Keys)
            {
                carryingSystem.Detach(piece);
            }

            // Phase 2: Restore piece states (position, hero status, visual state)
            foreach (var kvp in snapshot.PieceData)
            {
                var piece = kvp.Key;
                var data = kvp.Value;

                // Restore logic position
                piece.Position = data.Position;
                piece.IsHero = data.IsHero;

                // Restore visual state
                if (piece.gameObject != null)
                {
                    piece.gameObject.SetActive(data.IsActive);
                    
                    // Update visual position to match logic position
                    piece.gameObject.transform.position = board.BoardCoordToWorld(data.Position);
                }

                Debug.Log($"  Restored piece {piece.Type} to {data.Position}, Active: {data.IsActive}");
            }

            // Phase 3: Restore board dictionary - CRITICAL SECTION
            Debug.Log($"[RestoreSnapshot] Phase 3: Restoring board dictionary");
            Debug.Log($"  Current board has {board.Pieces.Count} pieces before clear");
            
            // CRITICAL FIX: Clear ENTIRE board dictionary first
            // This ensures old positions are completely removed
            board.Pieces.Clear();
            Debug.Log($"  Board cleared completely");

            // Restore board state from snapshot
            int restoredCount = 0;
            foreach (var kvp in snapshot.BoardState)
            {
                var pos = kvp.Key;
                var piece = kvp.Value;
                
                // Check if piece data exists
                if (!snapshot.PieceData.ContainsKey(piece))
                {
                    Debug.LogWarning($"  Board position {pos} has piece {piece.Type} but no PieceData in snapshot!");
                    continue;
                }
                
                var data = snapshot.PieceData[piece];

                // Add to board if piece should exist on board
                // Note: We restore ALL pieces that were on board in snapshot, regardless of active state
                // Active state is handled by gameObject.SetActive in Phase 2
                if (data.ExistsOnBoard)
                {
                    board.Pieces[pos] = piece;
                    restoredCount++;
                    Debug.Log($"  Board[{pos}] = {piece.Type} (Active: {data.IsActive})");
                }
                else
                {
                    Debug.Log($"  Skipping {piece.Type} at {pos} - was not on board");
                }
            }

            Debug.Log($"[RestoreSnapshot] Board restored: {restoredCount} pieces added, total: {board.Pieces.Count}");
            
            // Validate board dictionary after restore
            if (board.Pieces.Count == 0 && snapshot.BoardState.Count > 0)
            {
                Debug.LogError($"[RestoreSnapshot] CRITICAL: Board is empty after restore but snapshot had {snapshot.BoardState.Count} positions!");
                Debug.LogError($"[RestoreSnapshot] This indicates a bug in restore logic!");
                
                // Debug: Show what we tried to restore
                foreach (var kvp in snapshot.BoardState)
                {
                    var piece = kvp.Value;
                    var data = snapshot.PieceData[piece];
                    Debug.LogError($"  Failed to restore: {piece.Type} at {kvp.Key}, ExistsOnBoard={data.ExistsOnBoard}, IsActive={data.IsActive}");
                }
            }
            
            // Verify each piece in board has correct position
            foreach (var kvp in board.Pieces)
            {
                var pos = kvp.Key;
                var piece = kvp.Value;
                
                if (piece.Position != pos)
                {
                    Debug.LogError($"[RestoreSnapshot] MISMATCH: Board[{pos}] has {piece.Type} but piece.Position={piece.Position}");
                }
            }

            // Phase 4: Restore carrying relationships
            // Phải restore theo thứ tự: root carriers trước, carried pieces sau
            var restored = new HashSet<BasePiece>();
            var queue = new Queue<BasePiece>(snapshot.PieceData.Keys);
            int maxIterations = snapshot.PieceData.Count * 2; // Prevent infinite loop
            int iterations = 0;

            while (queue.Count > 0 && iterations < maxIterations)
            {
                iterations++;
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
                    bool success = carryingSystem.TryAddCarry(data.Carrier, piece);
                    if (!success)
                    {
                        Debug.LogWarning($"Failed to restore carrying: {data.Carrier.Type} carrying {piece.Type}");
                    }
                    else
                    {
                        Debug.Log($"  Restored carrying: {data.Carrier.Type} -> {piece.Type}");
                    }
                }

                restored.Add(piece);
            }

            if (iterations >= maxIterations)
            {
                Debug.LogError($"RestoreSnapshot: Infinite loop detected in carrying relationship restoration!");
            }

            // Phase 5: Recalculate cache cho tất cả pieces
            foreach (var piece in snapshot.PieceData.Keys)
            {
                piece.RecalculateCache();
            }

            Debug.Log($"[RestoreSnapshot] Restore complete - {restored.Count} pieces restored");
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

        /// <summary>
        /// Validate snapshot đã được restore đúng chưa
        /// </summary>
        public bool ValidateSnapshotRestored(Snapshot snapshot)
        {
            if (snapshot == null)
            {
                Debug.LogError("ValidateSnapshotRestored: snapshot is null");
                return false;
            }

            bool allValid = true;

            // Check piece positions
            foreach (var kvp in snapshot.PieceData)
            {
                var piece = kvp.Key;
                var data = kvp.Value;

                if (piece.Position != data.Position)
                {
                    Debug.LogError($"Position mismatch: {piece.Type} is at {piece.Position}, should be {data.Position}");
                    allValid = false;
                }

                if (piece.IsHero != data.IsHero)
                {
                    Debug.LogError($"IsHero mismatch: {piece.Type} IsHero={piece.IsHero}, should be {data.IsHero}");
                    allValid = false;
                }

                if (piece.gameObject != null && piece.gameObject.activeSelf != data.IsActive)
                {
                    Debug.LogError($"Active state mismatch: {piece.Type} Active={piece.gameObject.activeSelf}, should be {data.IsActive}");
                    allValid = false;
                }
            }

            // Check board dictionary
            Debug.Log($"[ValidateSnapshotRestored] Checking board dictionary: snapshot has {snapshot.BoardState.Count} positions, board has {board.Pieces.Count}");
            
            foreach (var kvp in snapshot.BoardState)
            {
                var pos = kvp.Key;
                var expectedPiece = kvp.Value;
                var data = snapshot.PieceData[expectedPiece];

                // Should exist on board if ExistsOnBoard is true (regardless of IsActive)
                if (data.ExistsOnBoard)
                {
                    if (!board.Pieces.TryGetValue(pos, out var actualPiece))
                    {
                        Debug.LogError($"Board dictionary missing: Position {pos} should have {expectedPiece.Type} (Active: {data.IsActive})");
                        allValid = false;
                    }
                    else if (actualPiece != expectedPiece)
                    {
                        Debug.LogError($"Board dictionary mismatch: Position {pos} has {actualPiece.Type}, should be {expectedPiece.Type}");
                        allValid = false;
                    }
                }
            }
            
            // Check reverse: all pieces in board should be in snapshot
            foreach (var kvp in board.Pieces)
            {
                var pos = kvp.Key;
                var piece = kvp.Value;
                
                if (!snapshot.BoardState.ContainsKey(pos))
                {
                    Debug.LogError($"Board has unexpected piece: {piece.Type} at {pos} (not in snapshot)");
                    allValid = false;
                }
                else if (snapshot.BoardState[pos] != piece)
                {
                    Debug.LogError($"Board position mismatch: {pos} has {piece.Type}, snapshot has {snapshot.BoardState[pos].Type}");
                    allValid = false;
                }
            }

            // Check carrying relationships
            foreach (var kvp in snapshot.PieceData)
            {
                var piece = kvp.Key;
                var data = kvp.Value;

                var actualCarrier = carryingSystem.GetCarrier(piece);
                if (actualCarrier != data.Carrier)
                {
                    Debug.LogError($"Carrier mismatch: {piece.Type} carrier is {actualCarrier?.Type}, should be {data.Carrier?.Type}");
                    allValid = false;
                }
            }

            if (allValid)
            {
                Debug.Log("[ValidateSnapshotRestored] ✅ All validations passed!");
            }

            return allValid;
        }
    }
}
