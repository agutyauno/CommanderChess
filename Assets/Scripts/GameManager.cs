using System.Collections.Generic;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    [SerializeField] Board board;
    [SerializeField] List<Piece> pieces = new();
    void Awake()
    {
        SetUpPieces();
        board.Init();
        Debug.Log("avalable moves");
        foreach (var p in pieces[0].PossibleMoves)
        {
            Debug.Log(p.ToLabel());
        }

        Debug.Log("avalable attacks");
        foreach (var p in pieces[0].PossibleAttacks)
        {
            Debug.Log(p.ToLabel());
        }
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
