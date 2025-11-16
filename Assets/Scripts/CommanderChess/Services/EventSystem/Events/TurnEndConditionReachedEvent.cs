using System;
using CommanderChess.Domain;

namespace CommanderChess.Services
{
    /// <summary>
    /// Fired when a command has executed that could end the turn (Move, Capture, Boarding)
    /// HUD should show Confirm/Cancel buttons
    /// </summary>
    public readonly struct TurnEndConditionReachedEvent : IGameEvent
    {
        public readonly Team Team;
        public readonly int TurnNumber;
        public readonly string CommandDescription;
        public readonly DateTime Timestamp;

        public TurnEndConditionReachedEvent(Team team, int turnNumber, string commandDescription, DateTime timestamp)
        {
            Team = team;
            TurnNumber = turnNumber;
            CommandDescription = commandDescription;
            Timestamp = timestamp;
        }
    }
}
