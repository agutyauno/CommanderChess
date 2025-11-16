using CommanderChess.Domain;
using CommanderChess.Services;

namespace CommanderChess.Events
{
    public readonly struct HeroBecameEvent : IGameEvent
    {
        public readonly BasePiece Piece;
        public readonly HeroCondition Condition;

        public HeroBecameEvent(BasePiece piece, HeroCondition condition)
        {
            Piece = piece;
            Condition = condition;
        }
    }

    public enum HeroCondition
    {
        CanAttackCommander, // Can attack commander without passing through danger zones
        LastPiece           // Last piece besides Commander and HQ
    }
}
