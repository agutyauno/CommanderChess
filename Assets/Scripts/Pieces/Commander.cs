using System;
using System.Collections.Generic;
using UnityEngine;

public class Commander : Piece
{
    public override PieceType Type => PieceType.Commander;
    public override BoardCoord InitialPosition => new BoardCoord(1, 7); // Example initial position
    public override List<Vector2Int> GetPossibleAttacks()
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetPossibleMoves()
    {
        throw new NotImplementedException();
    }
}
