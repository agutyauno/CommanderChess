using System;

namespace CommanderChess.Core
{
    /// <summary>
    /// Represents a move in the game
    /// </summary>
    [Serializable]
    public class Move
    {
        public BoardPosition From { get; private set; }
        public BoardPosition To { get; private set; }
        public MoveType Type { get; private set; }
        public PieceType PromotionPiece { get; private set; }
        public CommanderAbility AbilityUsed { get; private set; }
        
        // For undo functionality
        public PieceType CapturedPiece { get; set; }
        public PlayerSide? CapturedPieceSide { get; set; }
        public BoardPosition? EnPassantCapturePosition { get; set; }
        public bool WasFirstMove { get; set; }
        
        public Move(BoardPosition from, BoardPosition to, MoveType type = MoveType.Normal)
        {
            From = from;
            To = to;
            Type = type;
            PromotionPiece = PieceType.None;
            AbilityUsed = CommanderAbility.None;
            CapturedPiece = PieceType.None;
        }

        public static Move CreatePromotion(BoardPosition from, BoardPosition to, PieceType promoteTo)
        {
            return new Move(from, to, MoveType.Promotion)
            {
                PromotionPiece = promoteTo
            };
        }

        public static Move CreateCapture(BoardPosition from, BoardPosition to)
        {
            return new Move(from, to, MoveType.Capture);
        }

        public static Move CreateEnPassant(BoardPosition from, BoardPosition to, BoardPosition capturePos)
        {
            return new Move(from, to, MoveType.EnPassant)
            {
                EnPassantCapturePosition = capturePos
            };
        }

        public static Move CreateCastling(BoardPosition from, BoardPosition to)
        {
            return new Move(from, to, MoveType.Castling);
        }

        public static Move CreateCommanderAbility(BoardPosition from, BoardPosition to, CommanderAbility ability)
        {
            return new Move(from, to, MoveType.CommanderAbility)
            {
                AbilityUsed = ability
            };
        }

        public string ToNotation()
        {
            string notation = From.ToNotation() + To.ToNotation();
            
            if (Type == MoveType.Promotion)
            {
                notation += GetPromotionChar(PromotionPiece);
            }
            
            return notation;
        }

        private char GetPromotionChar(PieceType type)
        {
            return type switch
            {
                PieceType.Queen => 'q',
                PieceType.Rook => 'r',
                PieceType.Bishop => 'b',
                PieceType.Knight => 'n',
                _ => 'q'
            };
        }

        public override string ToString()
        {
            return $"Move: {From} -> {To} ({Type})";
        }

        public override bool Equals(object obj)
        {
            if (obj is not Move other)
                return false;

            return From == other.From && To == other.To && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(From, To, Type);
        }
    }
}
