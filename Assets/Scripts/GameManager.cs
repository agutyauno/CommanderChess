using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
public class GameManager : MonoBehaviour
{
    [SerializeField] List<Piece> pieces = new();
    [Inject] Board board;
    void Awake()
    {
        board.Init();
        SetUpPieces();
        Debug.Log("ring of fire zones:");
        foreach (var coord in pieces[0].RingOfFireZones)
        {
            Debug.Log(coord.ToLabel());
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
