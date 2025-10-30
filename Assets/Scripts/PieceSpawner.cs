using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PieceSpawner : MonoBehaviour
{
    [Inject] readonly Board board;
    [Inject] readonly MovementExecutor movementExecutor;
    [Inject] readonly CarryingSystem carryingSystem;
    [Inject] readonly IObjectResolver objectResolver;
    [SerializeField] List<PieceSetupData> redTeam;
    [SerializeField] List<PieceSetupData> blueTeam;

    public void Spawn()
    {
        if (!Validate())
        {
            Debug.LogError("PieceSpawner validation failed. Aborting spawn.");
            return;
        }

        foreach (var setup in redTeam)
        {
            GameObject obj = objectResolver.Instantiate(setup.Prefab);
            var piece = obj.GetComponent<BasePiece>();
            piece.PieceData = setup.PieceData;
            movementExecutor.PlaceOnBoard(piece, setup.Position);
            piece.Init();
            if (piece.PossibleAttacks.Count > 0)
            {
                Debug.Log($"Piece {piece.Type} at {piece.Position} can attack {piece.PossibleAttacks.Count} positions");
            }
            else
            {
                Debug.Log($"Piece {piece.Type} at {piece.Position} has no attack positions");
            }

            if (piece.PossibleMoves.Count > 0)
            {
                Debug.Log($"Piece {piece.Type} at {piece.Position} can move to {piece.PossibleMoves.Count} positions");
            }
            else
            {
                Debug.Log($"Piece {piece.Type} at {piece.Position} has no move positions");
            }
        }

        foreach (var setup in blueTeam)
        {
            GameObject obj = objectResolver.Instantiate(setup.Prefab);
            var piece = obj.GetComponent<BasePiece>();
            piece.PieceData = setup.PieceData;
            movementExecutor.PlaceOnBoard(piece, setup.Position);
            piece.Init();

        }
    }

     /// <summary>
    /// Clear tất cả quân cờ trên board (dùng cho reset game)
    /// </summary>
    public void ClearAllPieces()
    {
        Debug.Log("Clearing all pieces...");

        // Get all piece GameObjects
        BasePiece[] pieces = GetComponentsInChildren<BasePiece>();
        
        foreach (var piece in pieces)
        {
            // Unregister from carrying system
            carryingSystem.UnregisterPiece(piece);
            
            // Destroy GameObject
            Destroy(piece.gameObject);
        }

        // Clear board dictionary
        board.Pieces.Clear();

        Debug.Log($"Cleared {pieces.Length} pieces");
    }

    bool Validate()
    {
        foreach (var setupData in redTeam)
        {
            if (setupData.Prefab == null)
            {
                Debug.LogError($"Red team piece {setupData.Piece.Type} has no prefab assigned");
                return false;
            }

            if (setupData.Piece == null)
            {
                Debug.LogError($"Red team piece at position {setupData.Position} has no Piece component");
                return false;
            }

            if (!board.IsInBoard(setupData.Position))
            {
                Debug.LogError($"Red team piece {setupData.Piece.Type} has invalid position {setupData.Position}");
                return false;
            }

            if (board.Pieces.ContainsKey(setupData.Position))
            {
                Debug.LogError($"Red team piece {setupData.Piece.Type} position {setupData.Position} is already occupied");
                return false;
            }

            if (setupData.PieceData == null)
            {
                Debug.LogError($"Red team piece {setupData.Piece.Type} has no PieceData assigned");
                return false;
            }

            if (setupData.Piece.Team != Team.Red)
            {
                Debug.LogError($"Red team piece {setupData.Piece.Type} has incorrect team {setupData.Piece.Team}");
                return false;
            }
        }

        foreach (var setupData in blueTeam)
        {
            if (setupData.Prefab == null)
            {
                Debug.LogError($"Red team piece {setupData.Piece.Type} has no prefab assigned");
                return false;
            }

            if (setupData.Piece == null)
            {
                Debug.LogError($"Red team piece at position {setupData.Position} has no Piece component");
                return false;
            }

            if (!board.IsInBoard(setupData.Position))
            {
                Debug.LogError($"Red team piece {setupData.Piece.Type} has invalid position {setupData.Position}");
                return false;
            }

            if (board.Pieces.ContainsKey(setupData.Position))
            {
                Debug.LogError($"Red team piece {setupData.Piece.Type} position {setupData.Position} is already occupied");
                return false;
            }

            if (setupData.PieceData == null)
            {
                Debug.LogError($"Red team piece {setupData.Piece.Type} has no PieceData assigned");
                return false;
            }

            if (setupData.Piece.Team != Team.Blue)
            {
                Debug.LogError($"Red team piece {setupData.Piece.Type} has incorrect team {setupData.Piece.Team}");
                return false;
            }
        }
        

        return true;
    }
}

[System.Serializable]
public class PieceSetupData
{
    [SerializeField] string positionLabel;
    [SerializeField] PieceData pieceData;
    [SerializeField] GameObject prefab;

    public BoardCoord Position { get => GetPositionFromLabel();}
    public GameObject Prefab { get => prefab; }
    public BasePiece Piece { get => prefab.GetComponent<BasePiece>(); }
    public PieceData PieceData { get => pieceData; }

    BoardCoord GetPositionFromLabel()
    {
        BoardCoord coord;
        bool ok = BoardCoord.TryParseLabel(positionLabel, out coord);
        if (!ok)
        {
            Debug.LogError($"Failed parsing position label: {positionLabel}");
        }
        return coord;
    }
}
