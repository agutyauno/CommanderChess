using System.Collections.Generic;
using UnityEngine;
using VContainer;
public class GameManager : MonoBehaviour
{
    [SerializeField] List<Piece> pieces = new();
    [Inject] Board board;
    void Awake()
    {
        board.Init();
        SetUpPieces();
        pieces[2].TryAddCarryingPiece(pieces[0]); // tank mang commander
        pieces[1].TryAddCarryingPiece(pieces[2]); // airforce mang tank

        foreach (var piece in pieces[1].CarryingPieces)
        {
            Debug.Log($"{pieces[1].Type} is carrying {piece.Type} at {piece.Position.ToLabel()}");
        }

        foreach (var piece in pieces[2].CarryingPieces)
        {   
            if (piece == null) continue;
            Debug.Log($"{pieces[2].Type} is carrying {piece.Type} at {piece.Position.ToLabel()}");
        }
        Debug.Log(pieces[0].Carrier.Type);
    }


    //xép cờ lên bàn
    void SetUpPieces()
    {
        foreach (var piece in pieces)
        {
            piece.Init();
            var pos = piece.Position;
            if (!board.PlacePiece(pos, piece))
            {
                Debug.LogError($"Failed placing piece {piece.Type} at {pos.ToLabel()} (occupied or invalid)");
            }
        }
    }
}
