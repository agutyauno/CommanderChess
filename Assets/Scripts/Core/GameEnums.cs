using System;

namespace CommanderChess.Core
{
    /// <summary>
    /// Player side (White/Black)
    /// </summary>
    public enum PlayerSide
    {
        White = 0,
        Black = 1
    }

    /// <summary>
    /// Types of chess pieces
    /// </summary>
    public enum PieceType
    {
        None = 0,
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King,
        Commander // Special piece unique to Commander Chess
    }

    /// <summary>
    /// Game states
    /// </summary>
    public enum GameState
    {
        NotStarted,
        Playing,
        Check,
        Checkmate,
        Stalemate,
        Draw,
        WhiteWins,
        BlackWins
    }

    /// <summary>
    /// Types of moves
    /// </summary>
    public enum MoveType
    {
        Normal,
        Capture,
        EnPassant,
        Castling,
        Promotion,
        CommanderAbility
    }

    /// <summary>
    /// Commander special abilities
    /// </summary>
    public enum CommanderAbility
    {
        None,
        Rally,          // Boost nearby allied pieces
        Charge,         // Move extra squares
        Shield,         // Protect adjacent pieces
        Tactics,        // Allow friendly piece swap
        Inspire         // Give extra move to adjacent piece
    }
}
