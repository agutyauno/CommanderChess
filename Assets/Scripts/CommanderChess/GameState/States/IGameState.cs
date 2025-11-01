using CommanderChess.Domain;

namespace CommanderChess.GameState
{
    /// <summary>
    /// IGameState - Interface cho State Pattern
    /// Mỗi state implement interface này để xử lý input và logic riêng
    /// </summary>
    public interface IGameState
    {
        GameStateData Data { get; }
        GameStateManager Manager { get; }
        /// <summary>
        /// Được gọi khi enter vào state này
        /// </summary>
        void Enter();

        /// <summary>
        /// Được gọi khi exit khỏi state này
        /// </summary>
        void Exit();

        /// <summary>
        /// Xử lý khi người chơi click vào board (ô trống)
        /// </summary>
        void HandleBoardClick(BoardCoord coord);

        /// <summary>
        /// Xử lý khi người chơi click vào một piece
        /// </summary>
        void HandlePieceClick(BasePiece piece);

        /// <summary>
        /// Xử lý khi người chơi cancel action (right click hoặc ESC)
        /// </summary>
        void HandleCancel();

        /// <summary>
        /// Update mỗi frame (nếu cần)
        /// </summary>
        void Update();
    }
}