using UnityEngine;

namespace CommanderChess.Core
{
    /// <summary>
    /// Manages default board layouts and terrain configurations for Commander Chess
    /// </summary>
    public static class BoardLayout
    {
        /// <summary>
        /// Applies the standard Commander Chess terrain layout to the board
        /// </summary>
        public static void ApplyStandardLayout(Board board)
        {
            if (board == null)
            {
                Debug.LogError("Board is null");
                return;
            }

            // Set all to land first
            for (int va = 0; va < Board.BOARD_WIDTH; va++)
            {
                for (int ha = 0; ha < Board.BOARD_HEIGHT; ha++)
                {
                    board.SetTerrain(new BoardCoord(va, ha), Terrain.Land);
                }
            }

            // TODO: Based on game design, set specific terrain types
            // Example structure (needs to be adjusted based on actual game design):
            
            // Sea areas (bottom-left corner as mentioned in design doc)
            SetSeaAreas(board);
            
            // River running through the middle (separating two sides)
            SetRiverAreas(board);
            
            // Shallow crossing points on the river
            SetShallowCrossings(board);
            
            // Seaside areas
            SetSeasideAreas(board);
        }

        /// <summary>
        /// Sets sea terrain areas
        /// </summary>
        private static void SetSeaAreas(Board board)
        {
            // todo: implement sea area setup
        }

        /// <summary>
        /// Sets river terrain areas (deep water)
        /// </summary>
        private static void SetRiverAreas(Board board)
        {
            // todo: implement river area setup
        }

        /// <summary>
        /// Sets shallow crossing points on the river
        /// </summary>
        private static void SetShallowCrossings(Board board)
        {
            // todo: implement shallow crossing setup
        }

        /// <summary>
        /// Sets seaside terrain areas
        /// </summary>
        private static void SetSeasideAreas(Board board)
        {
           //todo: implement seaside area setup
        }

        /// <summary>
        /// Creates a custom layout from a 2D terrain array
        /// </summary>
        public static void ApplyCustomLayout(Board board, Terrain[,] layout)
        {
            if (board == null || layout == null)
            {
                Debug.LogError("Board or layout is null");
                return;
            }

            int width = Mathf.Min(layout.GetLength(0), Board.BOARD_WIDTH);
            int height = Mathf.Min(layout.GetLength(1), Board.BOARD_HEIGHT);

            for (int va = 0; va < width; va++)
            {
                for (int ha = 0; ha < height; ha++)
                {
                    board.SetTerrain(new BoardCoord(va, ha), layout[va, ha]);
                }
            }
        }

        /// <summary>
        /// Gets the terrain layout as a 2D array
        /// </summary>
        public static Terrain[,] GetLayoutArray(Board board)
        {
            Terrain[,] layout = new Terrain[Board.BOARD_WIDTH, Board.BOARD_HEIGHT];

            for (int va = 0; va < Board.BOARD_WIDTH; va++)
            {
                for (int ha = 0; ha < Board.BOARD_HEIGHT; ha++)
                {
                    layout[va, ha] = board.GetTerrain(new BoardCoord(va, ha));
                }
            }

            return layout;
        }
    }
}
