using System;
using System.Collections.Generic;

public class Commander : Piece
{
    public override PieceType Type => PieceType.Commander;
    public override BoardCoord InitialPosition => new BoardCoord(7, 1); // Example initial position
    public override List<BoardCoord> GetPossibleAttacks()
    {
        var moves = new List<BoardCoord>();
        if (board == null) return moves;

        var dirs = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        foreach (var (dx, dy) in dirs)
        {
            int step = 1;
            while (true)
            {
                var next = new BoardCoord(Position.x + dx * step, Position.y + dy * step);
                if (!board.IsInBoard(next)) break;
                moves.Add(next);
                step++;
            }
        }
        return moves;
    }

    public override List<BoardCoord> GetPossibleMoves()
    {
         var attacks = new List<BoardCoord>();
        if (board == null) return attacks;

        var neighbors = new (int dx, int dy)[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        foreach (var (dx, dy) in neighbors)
        {
            var pos = new BoardCoord(Position.x + dx, Position.y + dy);
            if (board.IsInBoard(pos)) attacks.Add(pos);
        }
        return attacks;
    }
}
