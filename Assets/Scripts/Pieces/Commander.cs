using System;
using System.Collections.Generic;
using UnityEngine;

public class Commander : Piece
{
    public override PieceType Type => PieceType.Commander;
    protected override Piece CheckVaildCarryPiece(Piece piece)
    {
        return null;
    }
}
