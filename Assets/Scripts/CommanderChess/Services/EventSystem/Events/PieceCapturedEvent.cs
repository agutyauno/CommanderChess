using CommanderChess.Domain;

namespace CommanderChess.Services
{
    public readonly struct PieceCapturedEvent : IGameEvent
    {
        public readonly BasePiece Attacker;
        public readonly BasePiece Defender;
        public readonly BoardCoord From;
        public readonly BoardCoord To;
        
        public PieceCapturedEvent(BasePiece attacker, BasePiece defender, BoardCoord from, BoardCoord to)
        {
            Attacker = attacker;
            Defender = defender;
            From = from;
            To = to;
        }
    }

}