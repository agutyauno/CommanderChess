using UnityEngine;
using VContainer;
using VContainer.Unity;
using CommanderChess.Domain;
using CommanderChess.Services;
using CommanderChess.CommandSystem;
using CommanderChess.Presentation;
using CommanderChess.GameState;

/// <summary>
/// GameLifeTimeScope - VContainer DI configuration
/// </summary>
public class GameLifeTimeScope : LifetimeScope
{
    [SerializeField] Board board;
    [SerializeField] GameStateManager gameStateManager;
    [SerializeField] InputHandler inputHandler;
    [SerializeField] BoardHighlighter boardHighlighter;
    [SerializeField] GameManager gameManager;
    [SerializeField] PieceSpawner pieceSpawner;

    protected override void Configure(IContainerBuilder builder)
    {
        // Core Systems
        builder.RegisterComponent(board).AsSelf();

        // Services (Singleton)
        builder.Register<CarryingSystem>(Lifetime.Singleton).AsSelf();
        builder.Register<StateBackupService>(Lifetime.Singleton).AsSelf();
        builder.Register<PathChecker>(Lifetime.Singleton).AsSelf();
        builder.Register<MovementExecutor>(Lifetime.Singleton).AsSelf();
        builder.Register<ZoneProvider>(Lifetime.Singleton).AsSelf();
        builder.Register<CommandManager>(Lifetime.Singleton).AsSelf();
        builder.Register<TurnManager>(Lifetime.Singleton).AsSelf();
        builder.Register<ActionValidator>(Lifetime.Singleton).AsSelf();
        builder.RegisterComponent(pieceSpawner).AsSelf();

        // Managers
        builder.RegisterComponent(gameStateManager).AsSelf();
        builder.RegisterComponent(inputHandler).AsSelf();
        builder.RegisterComponent(boardHighlighter).AsSelf();
        builder.RegisterComponent(gameManager).AsSelf();
    }
}