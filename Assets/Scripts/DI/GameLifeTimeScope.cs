using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifeTimeScope : LifetimeScope
{
    [SerializeField] Board board;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(board).AsSelf();
        builder.Register<PieceController>(Lifetime.Scoped).AsSelf();
        builder.RegisterComponentInHierarchy<GameManager>();
        builder.RegisterComponentInHierarchy<Piece>();
    }
}
