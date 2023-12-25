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
        public const int Bishop = 4;
        public const int Rook = 5;
        public const int Queen = 6;

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
    }
}
