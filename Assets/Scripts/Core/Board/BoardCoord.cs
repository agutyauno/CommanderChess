using System;

namespace CommanderChess.Core
{
    /// <summary>
    /// Represents a coordinate on the board using VA (Vertical Axis) and HA (Horizontal Axis)
    /// VA: 0-11 (12 columns)
    /// HA: 0-10 (11 rows)
    /// Origin (0,0) at bottom-left corner
    /// </summary>
    [Serializable]
    public struct BoardCoord : IEquatable<BoardCoord>
    {
        public readonly int X;
        public readonly int Y;

        public BoardCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Calculates Manhattan distance to another coordinate
        /// </summary>
        public int ManhattanDistance(BoardCoord other)
        {
            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }

        /// <summary>
        /// Calculates Chebyshev distance (max of horizontal and vertical distance)
        /// </summary>
        public int ChebyshevDistance(BoardCoord other)
        {
            return Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));
        }

        /// <summary>
        /// Calculates Euclidean distance to another coordinate
        /// </summary>
        public float EuclideanDistance(BoardCoord other)
        {
            int dVA = X - other.X;
            int dHA = Y - other.Y;
            return (float)Math.Sqrt(dVA * dVA + dHA * dHA);
        }

        /// <summary>
        /// Checks if this coordinate is on the same horizontal axis
        /// </summary>
        public bool IsSameHA(BoardCoord other)
        {
            return Y == other.Y;
        }

        /// <summary>
        /// Checks if this coordinate is on the same vertical axis
        /// </summary>
        public bool IsSameVA(BoardCoord other)
        {
            return X == other.X;
        }

        /// <summary>
        /// Checks if this coordinate is on the same diagonal as another
        /// </summary>
        public bool IsSameDiagonal(BoardCoord other)
        {
            return Math.Abs(X - other.X) == Math.Abs(Y - other.Y);
        }

        /// <summary>
        /// Gets the direction vector to another coordinate (normalized to -1, 0, or 1)
        /// </summary>
        public (int dX, int dY) GetDirectionTo(BoardCoord other)
        {
            int dX = other.X - X;
            int dY = other.Y - Y;
            
            return (
                dX == 0 ? 0 : dX / Math.Abs(dX),
                dY == 0 ? 0 : dY / Math.Abs(dY)
            );
        }

        #region Operators
        public static bool operator ==(BoardCoord a, BoardCoord b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(BoardCoord a, BoardCoord b)
        {
            return !(a == b);
        }

        public static BoardCoord operator +(BoardCoord a, BoardCoord b)
        {
            return new BoardCoord(a.X + b.X, a.Y + b.Y);
        }

        public static BoardCoord operator -(BoardCoord a, BoardCoord b)
        {
            return new BoardCoord(a.X - b.X, a.Y - b.Y);
        }

        public static BoardCoord operator *(BoardCoord coord, int scalar)
        {
            return new BoardCoord(coord.X * scalar, coord.Y * scalar);
        }
        #endregion

        #region IEquatable
        public bool Equals(BoardCoord other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is BoardCoord coord && Equals(coord);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
        #endregion

        #region String Representation
        public override string ToString()
        {
            return $"({X},{Y})";
        }

        /// <summary>
        /// Returns string in the format "VA,HA" (e.g., "3,5")
        /// </summary>
        public string ToGameNotation()
        {
            return $"{X},{Y}";
        }
        #endregion

        #region Static Helpers
        /// <summary>
        /// Creates a BoardCoord from game notation string (e.g., "3,5")
        /// </summary>
        public static BoardCoord FromString(string notation)
        {
            string[] parts = notation.Split(',');
            if (parts.Length == 2 && 
                int.TryParse(parts[0], out int va) && 
                int.TryParse(parts[1], out int ha))
            {
                return new BoardCoord(va, ha);
            }
            return new BoardCoord(-1, -1);
        }

        /// <summary>
        /// Zero coordinate (0,0)
        /// </summary>
        public static BoardCoord Zero => new(0, 0);

        /// <summary>
        /// Invalid coordinate (-1,-1)
        /// </summary>
        public static BoardCoord Invalid => new(-1, -1);

        public static BoardCoord Left => new(-1, 0);
        public static BoardCoord Right => new(1, 0);
        public static BoardCoord Up => new(0, -1);
        public static BoardCoord Down => new(0, 1);
        public static BoardCoord UpLeft => new(-1, 1);
        public static BoardCoord UpRight => new(1, 1);
        public static BoardCoord DownLeft => new(-1, -1);
        public static BoardCoord DownRight => new(1, -1);
        public static BoardCoord[] AllDirections => new BoardCoord[]
        {
            Down, Up, Right, Left,
            UpRight, UpLeft, DownRight, DownLeft
        };
        public static BoardCoord[] StraightDirections => new BoardCoord[]
        {
            Down, Up, Right, Left
        };
        public static BoardCoord[] DiagonalDirections => new BoardCoord[]
        {
            UpRight, UpLeft, DownRight, DownLeft
        };
        #endregion
    }
}