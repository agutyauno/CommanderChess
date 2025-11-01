namespace CommanderChess.GameState
{
    /// <summary>
    /// GameState enum - Định nghĩa các trạng thái game
    /// </summary>
    public enum GameState
    {
        Idle,                    // Chờ người chơi chọn quân
        PieceSelected,           // Đã chọn quân, hiển thị moves/attacks
        ExecutingAction,         // Đang thực thi command (chờ animation)
        SelectingDetachTarget,   // Đang chọn vị trí để detach quân
        WaitingForOpponent,      // Chờ đối thủ đánh (online mode)
        GameOver                 // Trò chơi kết thúc
    }
}