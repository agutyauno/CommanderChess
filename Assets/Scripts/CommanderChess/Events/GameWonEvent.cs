using CommanderChess.Domain;
using CommanderChess.Services;

namespace CommanderChess.Events
{
    public enum WinCondition
    {
        CommanderKilled,    // Highest priority
        AllNavyLost,
        AllAirforceLost,
        AllGroundUnitsLost, // All infantry + tank + artillery
        Surrender
    }

    public readonly struct GameWonEvent : IGameEvent
    {
        public readonly Team Winner;
        public readonly Team Loser;
        public readonly WinCondition Condition;

        public GameWonEvent(Team winner, Team loser, WinCondition condition)
        {
            Winner = winner;
            Loser = loser;
            Condition = condition;
        }
    }
}
