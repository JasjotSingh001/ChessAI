using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.Game
{
    public static class Piece
    {
        public const int None = 0;
        public const int King = 1;
        public const int Pawn = 2;
        public const int Knight = 3;
        public const int Bishop = 5;
        public const int Rook = 6;
        public const int Queen = 7;

        public const int White = 8;
        public const int Black = 16;

        public const int WhiteKing = White | King; //9
        public const int WhitePawn = White | Pawn; //10
        public const int WhiteKnight = White | Knight; //11
        public const int WhiteBishop = White | Bishop; //12
        public const int WhiteRook = White | Rook; //13
        public const int WhiteQueen = White | Queen; //14

        public const int BlackKing = Black | King; //17
        public const int BlackPawn = Black | Pawn; //18
        public const int BlackKnight = Black | Knight; //19
        public const int BlackBishop = Black | Bishop; //20
        public const int BlackRook = Black | Rook; //21
        public const int BlackQueen = Black | Queen; //22

        private const int typeMask = 0b00111;
        private const int blackMask = 0b10000;
        private const int whiteMask = 0b01000;
        private const int colourMask = whiteMask | blackMask;

        public static bool IsColour(int piece, int colour)
        {
            return (piece & colourMask) == colour;
        }

        public static int Colour(int piece)
        {
            return piece & colourMask;
        }

        public static int PieceType(int piece)
        {
            return piece & typeMask;
        }

        public static bool IsRookOrQueen(int piece)
        {
            return (piece & 0b110) == 0b110;
        }

        public static bool IsBishopOrQueen(int piece)
        {
            return (piece & 0b101) == 0b101;
        }

        public static bool IsSlidingPiece(int piece)
        {
            return (piece & 0b100) != 0;
        }
    }
}
