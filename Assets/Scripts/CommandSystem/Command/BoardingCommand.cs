using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardingCommand : BaseCommand
{
    private readonly Piece pieceA;
    private readonly Piece pieceB;
    private readonly CarryingSystem carryingSystem;

    private bool wasSuccessful;

    // Backup để undo
    private CarryingBackup backup;

    private class CarryingBackup
    {
        public Piece Carrier;
        public Piece Passenger;
        public Dictionary<Piece, PieceBackupData> pieceData = new();
    }

    private struct PieceBackupData
    {
        public Piece Carrier;
        public List<Piece> Carrying;
        public BoardCoord Position;
    }

    public BoardingCommand(
        Piece pieceA,
        Piece pieceB,
        CarryingSystem carryingSystem)
    {
        this.pieceA = pieceA;
        this.pieceB = pieceB;
        this.carryingSystem = carryingSystem;
    }

    public override string Description =>
        $"Board {pieceA.Type} with {pieceB.Type}";

    public override bool WasSuccessful => wasSuccessful;

    public override bool CanExecute()
    {
        if (pieceA == null || pieceB == null)
        {
            Debug.LogWarning("One of the pieces is null");
            return false;
        }

        if (pieceA == pieceB)
        {
            Debug.LogWarning("Cannot board piece with itself");
            return false;
        }

        if (pieceA.Team != pieceB.Team)
        {
            Debug.LogWarning("Cannot board pieces from different teams");
            return false;
        }

        // Kiểm tra ít nhất một trong hai có thể mang cái kia
        bool aCanCarryB = pieceA.AllowedCarryTypes.Contains(pieceB.Type);
        bool bCanCarryA = pieceB.AllowedCarryTypes.Contains(pieceA.Type);

        if (!aCanCarryB && !bCanCarryA)
        {
            Debug.LogWarning($"Neither {pieceA.Type} nor {pieceB.Type} can carry the other");
            return false;
        }

        return true;
    }

    public override bool Execute()
    {
        if (!CanExecute())
        {
            wasSuccessful = false;
            return false;
        }

        try
        {
            // Backup state trước khi thực hiện
            BackupState();

            // Sử dụng TryAddCarry - tự động xác định ai mang ai
            wasSuccessful = carryingSystem.TryAddCarry(pieceA, pieceB);

            if (wasSuccessful)
            {
                // Xác định carrier và passenger sau khi execute
                Piece carrier, passenger;
                if (carryingSystem.GetCarrier(pieceB) == pieceA)
                {
                    carrier = pieceA;
                    passenger = pieceB;
                }
                else
                {
                    carrier = pieceB;
                    passenger = pieceA;
                }

                backup.Carrier = carrier;
                backup.Passenger = passenger;

                // Update positions
                UpdatePositions(carrier);

                Debug.Log($"✓ {Description}");
                Debug.Log($"  Result: {carrier.Type} carries {passenger.Type}");
            }
            else
            {
                Debug.LogWarning($"✗ {Description} failed");
            }

            return wasSuccessful;
        }
        catch (Exception e)
        {
            Debug.LogError($"Execute failed: {e.Message}");
            wasSuccessful = false;
            return false;
        }
    }

    public override bool Undo()
    {
        if (!wasSuccessful || backup == null)
        {
            Debug.LogWarning("Cannot undo: command was not successful");
            return false;
        }

        try
        {
            // Detach passenger
            bool detached = carryingSystem.Detach(backup.Passenger);

            if (!detached)
            {
                Debug.LogError("Failed to detach during undo");
                return false;
            }

            // Restore all piece states
            RestoreState();

            Debug.Log($"↶ Undo: {Description}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Undo failed: {e.Message}");
            return false;
        }
    }

    private void BackupState()
    {
        backup = new CarryingBackup();

        // Backup tất cả pieces liên quan
        var piecesToBackup = new HashSet<Piece> { pieceA, pieceB };

        // Thêm các quân đang được mang bởi A và B
        piecesToBackup.UnionWith(carryingSystem.GetAllCarriedPieces(pieceA));
        piecesToBackup.UnionWith(carryingSystem.GetAllCarriedPieces(pieceB));

        foreach (var piece in piecesToBackup)
        {
            backup.pieceData[piece] = new PieceBackupData
            {
                Carrier = carryingSystem.GetCarrier(piece),
                Carrying = carryingSystem.GetDirectCarrying(piece),
                Position = piece.Position
            };
        }
    }

    private void RestoreState()
    {
        foreach (var kvp in backup.pieceData)
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
            // TODO: Convert to world position
        }
    }

    private void UpdatePositions(Piece carrier)
    {
        // Update position của tất cả quân trong group
        var allCarried = carryingSystem.GetAllCarriedPieces(carrier);

        foreach (var piece in allCarried)
        {
            piece.Position = carrier.Position;
            // TODO: Update world position properly
            // piece.transform.position = board.BoardCoordToWorld(carrier.Position);
        }
    }
}
