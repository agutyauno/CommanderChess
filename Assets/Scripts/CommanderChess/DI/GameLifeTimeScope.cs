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
        builder.Register<EventBus>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        // Services (Singleton)
        builder.Register<CarryingSystem>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        builder.Register<StateBackupService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        builder.Register<PathChecker>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        builder.Register<MovementExecutor>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        builder.Register<ZoneProvider>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        builder.Register<CommandManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        builder.Register<TurnManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        builder.Register<ActionValidator>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        builder.RegisterComponent(pieceSpawner).AsSelf();

        // Managers
        builder.RegisterComponent(gameStateManager).AsSelf();
        builder.RegisterComponent(inputHandler).AsSelf();
        builder.RegisterComponent(boardHighlighter).AsSelf();
        builder.RegisterComponent(gameManager).AsSelf();
    }
}