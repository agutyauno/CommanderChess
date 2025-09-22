using System.Collections.Generic;
using UnityEngine;
public class BoardManager : MonoBehaviour
{
    [SerializeField] Board board;
    [SerializeField] List<Piece> pieces = new();

    void Awake()
    {
        SetUpPieces();
    }

    //xép cờ lên bàn
    void SetUpPieces()
    {
        foreach (var piece in pieces)
        {
            var pos = piece.InitialPosition;
            if (board.IsInBoard(pos))
            {
                piece.transform.position = board.Grid.CellToWorld(board.BoardCoordToCell(pos));
            }
            else
            {
                Debug.LogError($"Piece {piece.Type} initial position {pos} is out of board range.");
            }
        }
    }
}
