using System;
using System.Collections.Generic;
using CommanderChess.Core;
using CommanderChess.Pieces;

namespace CommanderChess.Utils
{
    /// <summary>
    /// FEN (Forsyth-Edwards Notation) parser and generator
    /// </summary>
    public static class FENParser
    {
        /// <summary>
        /// Generate FEN string from current board state
        /// </summary>
        public static string ToFEN(ChessPiece[,] board, PlayerSide activePlayer, bool whiteCanCastleKingside, 
            bool whiteCanCastleQueenside, bool blackCanCastleKingside, bool blackCanCastleQueenside,
            BoardPosition? enPassantTarget, int halfmoveClock, int fullmoveNumber)
        {
            var parts = new List<string>();

            // Piece placement
            parts.Add(GetPiecePlacement(board));

            // Active color
            parts.Add(activePlayer == PlayerSide.White ? "w" : "b");

            // Castling availability
            string castling = "";
            if (whiteCanCastleKingside) castling += "K";
            if (whiteCanCastleQueenside) castling += "Q";
            if (blackCanCastleKingside) castling += "k";
            if (blackCanCastleQueenside) castling += "q";
            if (string.IsNullOrEmpty(castling)) castling = "-";
            parts.Add(castling);

            // En passant target
            parts.Add(enPassantTarget.HasValue ? enPassantTarget.Value.ToNotation() : "-");

            // Halfmove clock
            parts.Add(halfmoveClock.ToString());

            // Fullmove number
            parts.Add(fullmoveNumber.ToString());

            return string.Join(" ", parts);
        }

        private static string GetPiecePlacement(ChessPiece[,] board)
        {
            var ranks = new List<string>();

            for (int row = 7; row >= 0; row--)
            {
                string rank = "";
                int emptyCount = 0;

                for (int col = 0; col < 8; col++)
                {
                    var piece = board[row, col];
                    
                    if (piece == null)
                    {
                        emptyCount++;
                    }
                    else
                    {
                        if (emptyCount > 0)
                        {
                            rank += emptyCount.ToString();
                            emptyCount = 0;
                        }
                        rank += GetPieceChar(piece);
                    }
                }

                if (emptyCount > 0)
                {
                    rank += emptyCount.ToString();
                }

                ranks.Add(rank);
            }

            return string.Join("/", ranks);
        }

        private static char GetPieceChar(ChessPiece piece)
        {
            char c = piece.Type switch
            {
                PieceType.King => 'K',
                PieceType.Queen => 'Q',
                PieceType.Rook => 'R',
                PieceType.Bishop => 'B',
                PieceType.Knight => 'N',
                PieceType.Pawn => 'P',
                PieceType.Commander => 'C', // Extended FEN for Commander
                _ => '?'
            };

            return piece.Side == PlayerSide.White ? c : char.ToLower(c);
        }

        /// <summary>
        /// Parse FEN string and set up board
        /// </summary>
        public static void FromFEN(string fen, ChessPiece[,] board, out PlayerSide activePlayer,
            out bool whiteCanCastleKingside, out bool whiteCanCastleQueenside,
            out bool blackCanCastleKingside, out bool blackCanCastleQueenside,
            out BoardPosition? enPassantTarget, out int halfmoveClock, out int fullmoveNumber)
        {
            // Initialize outputs with defaults
            activePlayer = PlayerSide.White;
            whiteCanCastleKingside = false;
            whiteCanCastleQueenside = false;
            blackCanCastleKingside = false;
            blackCanCastleQueenside = false;
            enPassantTarget = null;
            halfmoveClock = 0;
            fullmoveNumber = 1;

            string[] parts = fen.Split(' ');
            if (parts.Length < 1)
                return;

            // Clear board
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    board[row, col] = null;
                }
            }

            // Parse piece placement
            string[] ranks = parts[0].Split('/');
            for (int i = 0; i < ranks.Length && i < 8; i++)
            {
                int row = 7 - i;
                int col = 0;

                foreach (char c in ranks[i])
                {
                    if (char.IsDigit(c))
                    {
                        col += c - '0';
                    }
                    else
                    {
                        var piece = CreatePieceFromChar(c, new BoardPosition(row, col));
                        if (piece != null)
                        {
                            board[row, col] = piece;
                        }
                        col++;
                    }
                }
            }

            // Parse active color
            if (parts.Length >= 2)
            {
                activePlayer = parts[1] == "b" ? PlayerSide.Black : PlayerSide.White;
            }

            // Parse castling
            if (parts.Length >= 3 && parts[2] != "-")
            {
                whiteCanCastleKingside = parts[2].Contains('K');
                whiteCanCastleQueenside = parts[2].Contains('Q');
                blackCanCastleKingside = parts[2].Contains('k');
                blackCanCastleQueenside = parts[2].Contains('q');
            }

            // Parse en passant
            if (parts.Length >= 4 && parts[3] != "-")
            {
                enPassantTarget = BoardPosition.FromNotation(parts[3]);
            }

            // Parse halfmove clock
            if (parts.Length >= 5 && int.TryParse(parts[4], out int hmc))
            {
                halfmoveClock = hmc;
            }

            // Parse fullmove number
            if (parts.Length >= 6 && int.TryParse(parts[5], out int fmn))
            {
                fullmoveNumber = fmn;
            }
        }

        private static ChessPiece CreatePieceFromChar(char c, BoardPosition position)
        {
            PlayerSide side = char.IsUpper(c) ? PlayerSide.White : PlayerSide.Black;
            char upper = char.ToUpper(c);

            PieceType type = upper switch
            {
                'K' => PieceType.King,
                'Q' => PieceType.Queen,
                'R' => PieceType.Rook,
                'B' => PieceType.Bishop,
                'N' => PieceType.Knight,
                'P' => PieceType.Pawn,
                'C' => PieceType.Commander,
                _ => PieceType.None
            };

            if (type == PieceType.None)
                return null;

            return ChessPiece.Create(type, side, position);
        }

        /// <summary>
        /// Get starting position FEN for standard chess
        /// </summary>
        public static string StandardStartFEN => "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        /// <summary>
        /// Get starting position FEN for Commander Chess (with Commanders replacing one knight each)
        /// </summary>
        public static string CommanderStartFEN => "rnbqkbcr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBCR w KQkq - 0 1";
    }
}
