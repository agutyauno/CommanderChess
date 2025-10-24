
using System.Collections.Generic;
using VContainer;

public class StateBackupService
{
   [Inject] readonly CarryingSystem carryingSystem;
    [Inject] readonly Board board;

    public struct PieceBackupData
    {
        public BoardCoord Position;
        public Piece Carrier;
        public List<Piece> Carrying;
        public bool IsHero;
        public bool ExistsOnBoard; // Track if piece was on board
    }

    public class Snapshot
    {
        public Dictionary<Piece, PieceBackupData> PieceData { get; } = new();
        public Dictionary<BoardCoord, Piece> BoardState { get; } = new();
    }

    /// <summary>
    /// Backup state của một piece và tất cả pieces liên quan (carried pieces)
    /// </summary>
    public Snapshot CreateSnapshot(params Piece[] pieces)
    {
        if (pieces == null || pieces.Length == 0) return null;

        var snapshot = new Snapshot();
        var piecesToBackup = new HashSet<Piece>();

        // Collect all pieces cần backup (bao gồm cả carried pieces)
        foreach (var piece in pieces)
        {
            if (piece == null) continue;
            piecesToBackup.Add(piece);
            piecesToBackup.UnionWith(carryingSystem.GetAllCarriedPieces(piece));
        }

        // Backup từng piece
        foreach (var piece in piecesToBackup)
        {
            snapshot.PieceData[piece] = new PieceBackupData
            {
                Position = piece.Position,
                Carrier = carryingSystem.GetCarrier(piece),
                Carrying = new List<Piece>(carryingSystem.GetDirectCarrying(piece)),
                IsHero = piece.IsHero,
                ExistsOnBoard = board.Pieces.ContainsValue(piece)
            };
        }

        // Backup board state cho các vị trí liên quan
        foreach (var piece in piecesToBackup)
        {
            var pos = piece.Position;
            if (board.Pieces.TryGetValue(pos, out var occupant))
            {
                snapshot.BoardState[pos] = occupant;
            }
        }

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
                Carrying = new List<Piece>(carryingSystem.GetDirectCarrying(piece)),
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

        // Phase 1: Detach tất cả carrying relationships
        foreach (var piece in snapshot.PieceData.Keys)
        {
            carryingSystem.Detach(piece);
        }

        // Phase 2: Restore positions và board state
        foreach (var kvp in snapshot.PieceData)
        {
            var piece = kvp.Key;
            var data = kvp.Value;

            piece.Position = data.Position;
            piece.IsHero = data.IsHero;
        }

        // Clear và restore board state
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
            board.Pieces[kvp.Key] = kvp.Value;
        }

        // Phase 3: Restore carrying relationships
        // Phải restore theo thứ tự: root carriers trước, carried pieces sau
        var restored = new HashSet<Piece>();
        var queue = new Queue<Piece>(snapshot.PieceData.Keys);

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
