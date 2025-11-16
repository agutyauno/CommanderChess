using System.Collections.Generic;
using CommanderChess.Domain;
using CommanderChess.Events;
using VContainer;

namespace CommanderChess.Services
{
    /// <summary>
    /// Tracks unit losses per team for win condition checking
    /// </summary>
    public class GameStatsTracker
    {
        private readonly EventBus eventBus;
        private readonly Dictionary<Team, UnitLosses> teamLosses;

        [Inject]
        public GameStatsTracker(EventBus eventBus)
        {
            this.eventBus = eventBus;
            teamLosses = new Dictionary<Team, UnitLosses>
            {
                { Team.Red, new UnitLosses() },
                { Team.Blue, new UnitLosses() }
            };

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            eventBus.Subscribe<PieceCapturedEvent>(OnPieceCaptured);
            eventBus.Subscribe<PieceDestroyedEvent>(OnPieceDestroyed);
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<PieceCapturedEvent>(OnPieceCaptured);
            eventBus.Unsubscribe<PieceDestroyedEvent>(OnPieceDestroyed);
        }

        private void OnPieceCaptured(PieceCapturedEvent e)
        {
            RecordLoss(e.Defender);
        }

        private void OnPieceDestroyed(PieceDestroyedEvent e)
        {
            RecordLoss(e.DestroyedPiece);
        }

        private void RecordLoss(BasePiece piece)
        {
            var losses = teamLosses[piece.Team];

            switch (piece.Type)
            {
                case BasePiece.PieceType.Navy:
                    losses.NavyLost++;
                    break;
                case BasePiece.PieceType.AirForce:
                    losses.AirforceLost++;
                    break;
                case BasePiece.PieceType.Infantry:
                    losses.InfantryLost++;
                    break;
                case BasePiece.PieceType.Tank:
                    losses.TankLost++;
                    break;
                case BasePiece.PieceType.Artillery:
                    losses.ArtilleryLost++;
                    break;
                case BasePiece.PieceType.Commander:
                    losses.CommanderLost = true;
                    break;
            }
        }

        public UnitLosses GetLosses(Team team) => teamLosses[team];

        public void Reset()
        {
            foreach (var losses in teamLosses.Values)
            {
                losses.Reset();
            }
        }
    }

    public class UnitLosses
    {
        public int NavyLost { get; set; }
        public int AirforceLost { get; set; }
        public int InfantryLost { get; set; }
        public int TankLost { get; set; }
        public int ArtilleryLost { get; set; }
        public bool CommanderLost { get; set; }

        public void Reset()
        {
            NavyLost = 0;
            AirforceLost = 0;
            InfantryLost = 0;
            TankLost = 0;
            ArtilleryLost = 0;
            CommanderLost = false;
        }
    }
}
