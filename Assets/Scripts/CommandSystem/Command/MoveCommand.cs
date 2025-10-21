using UnityEngine;
using System.Collections.Generic;
using System;
using VContainer;
public class MoveCommand : BaseCommand
{
    readonly BoardCoord from;
    readonly BoardCoord to;
    private bool wasSuccessful = false;

    readonly Board board;
    readonly CarryingSystem carryingSystem;

    readonly Dictionary<Piece, PieceBackupData> backupData = new();
    Piece movedPiece;

    public MoveCommand(BoardCoord from, BoardCoord to, Board board, CarryingSystem carryingSystem)
    {
        this.from = from;
        this.to = to;
        this.board = board;
        this.carryingSystem = carryingSystem;
    }
    

    private struct PieceBackupData
    {
        public Piece Carrier;
        public List<Piece> Carrying;
        public BoardCoord Position;
        public bool IsHero;
    }

    public override string Description => $"Move {movedPiece.Team} {movedPiece.Type} from {from} to {to}";
    public override bool WasSuccessful => wasSuccessful;
    public override bool Execute()
    {
        if (!CanExecute())
        {
            wasSuccessful = false;
            return false;
        }

        try
        {
            BackupState();
            board.Pieces.Remove(from);
            board.Pieces[to] = movedPiece;
            movedPiece.Position = to;
            // ToDo: gửi event cập nhật vị trí quân cờ

            UpdateCarriedPositions();
            movedPiece.RecalculateCache();
            wasSuccessful = true;
            Debug.Log(Description);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Execution failed: {e.Message}");
            wasSuccessful = false;
            return false;
        }
    }

    public override bool Undo()
    {
        if (!wasSuccessful)
        {
            Debug.LogWarning("Cannot undo: command was not successful");
            return false;
        }

        try
        {
            // Di chuyển piece về vị trí cũ
            board.Pieces.Remove(to);
            board.Pieces[from] = movedPiece;
            movedPiece.Position = from;
            // todo: gửi event cập nhật vị trí quân cờ
            // Restore carried pieces positions
            RestoreState();

            movedPiece.RecalculateCache();

            Debug.Log($"↶ Undo: {Description}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Undo failed: {e.Message}");
            return false;
        }
    }

    public override bool CanExecute()
    {
        // Kiểm tra piece tồn tại
        if (!board.Pieces.TryGetValue(from, out movedPiece))
        {
            Debug.LogWarning($"No piece at {from}");
            return false;
        }

        // Kiểm tra vị trí đích hợp lệ
        if (!board.IsInBoard(to))
        {
            Debug.LogWarning($"Position {to} is out of board");
            return false;
        }

        // Kiểm tra vị trí đích trống (hoặc có thể boarding)
        if (board.TryGetPiece(to, out var occupant))
        {
            // Nếu là địch thì không thể move (phải dùng CaptureCommand)
            if (occupant.Team != movedPiece.Team)
            {
                Debug.LogWarning("Cannot move to enemy position. Use CaptureCommand instead.");
                return false;
            }

            // Nếu là đồng minh thì phải có thể boarding
            if (!occupant.AllowedCarryTypes.Contains(movedPiece.Type))
            {
                Debug.LogWarning($"Cannot board: {occupant.Type} cannot carry {movedPiece.Type}");
                return false;
            }
        }

        // Kiểm tra move có trong possible moves không
        if (!movedPiece.PossibleMoves.Contains(to))
        {
            Debug.LogWarning($"Invalid move: {to} not in possible moves");
            return false;
        }

        return true;
    }

    private void BackupState()
    {
        backupData.Clear();

        var pieceToBackup = new HashSet<Piece>{movedPiece};
        pieceToBackup.UnionWith(carryingSystem.GetAllCarriedPieces(movedPiece));
       
        foreach (var piece in pieceToBackup)
        {
            backupData[piece] = new PieceBackupData
            {
                Carrier = carryingSystem.GetCarrier(piece),
                Carrying = carryingSystem.GetDirectCarrying(piece),
                Position = piece.Position,
                IsHero = piece.IsHero
            };
        }
    }

    private void RestoreState()
    {
        foreach (var kvp in backupData)
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
    
    private void UpdateCarriedPositions()
    {
        var carried = carryingSystem.GetAllCarriedPieces(movedPiece);
        foreach (var piece in carried)
        {
            piece.Position = movedPiece.Position;
            // ToDo: gửi event cập nhật vị trí quân cờ
        }
    }

}
