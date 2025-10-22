
using System.Collections.Generic;
using VContainer;

public class StateBackupService
{
    [Inject] readonly CarryingSystem carryingSystem;
    public struct PieceBackupData
    {
        public BoardCoord Position;
        public Piece Carrier;
        public List<Piece> Carrying;
    }

    public class Snapshot
    {
        public Dictionary<Piece, PieceBackupData> backupData = new();
    }

    public Snapshot BackupState(Piece piece)
    {
        if (piece == null) return null;
        var snapshot = new Snapshot();

        var pieceToBackup = new HashSet<Piece> { piece };
        pieceToBackup.UnionWith(carryingSystem.GetAllCarriedPieces(piece));

        foreach (var p in pieceToBackup)
        {
            snapshot.backupData[p] = new PieceBackupData
            {
                Position = p.Position,
                Carrier = carryingSystem.GetCarrier(p),
                Carrying = carryingSystem.GetDirectCarrying(p)
            };
        }

        return snapshot;
    }

    public void RestoreState(Snapshot snapshot)
    {
        foreach (var kvp in snapshot.backupData)
        {
            var piece = kvp.Key;
            var data = kvp.Value;

            // Restore carrier relationship
            if (data.Carrier != null)
            {
                // Note: Simplified restore, may need more complex logic
                var carrierNode = carryingSystem.GetDirectCarrying(data.Carrier);
                if (!carrierNode.Contains(piece))
                {
                    // Re-attach
                    carryingSystem.TryAddCarry(data.Carrier, piece);
                }
            }

            // Restore position
            piece.Position = data.Position;
            // ToDo: gửi event cập nhật vị trí quân cờ
        }
    }
    
    
}
