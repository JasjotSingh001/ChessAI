using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.Game
{
    public readonly struct Move
    {
        public readonly struct Flag
        {
            public const int EnPassantCapture = 1;
            public const int Castling = 2;
            public const int PromoteToQueen = 3;
            public const int PromoteToKnight = 4;
            public const int PromoteToRook = 5;
            public const int PromoteToBishop = 6;
            public const int PawnTwoForward = 7;
        }

        public const ushort startSquareMask = 0b0000000000111111;
        public const ushort endSquareMask = 0b0000111111000000;
        public const ushort flagMask = 0b1111000000000000;

        //Stored as 16bit value, composed of the 3 maks above
        public readonly ushort moveValue;

        public int StartSquare
        {
            get
            {
                return moveValue & startSquareMask;
            }
        }

        public int EndSquare
        {
            get
            {
                return (moveValue & endSquareMask) >> 6;
            }
        }

        public int MoveFlag
        {
            get
            {
                return moveValue >> 12;
            }
        }

        public bool IsPromotion
        {
            get
            {
                int flag = MoveFlag;
                return flag == Flag.PromoteToQueen || flag == Flag.PromoteToRook || flag == Flag.PromoteToKnight || flag == Flag.PromoteToBishop;
            }
        }

        public int PromotionPieceType
        {
            get
            {
                switch (MoveFlag)
                {
                    case Flag.PromoteToRook:
                        return Piece.Rook;
                    case Flag.PromoteToKnight:
                        return Piece.Knight;
                    case Flag.PromoteToBishop:
                        return Piece.Bishop;
                    case Flag.PromoteToQueen:
                        return Piece.Queen;
                    default:
                        return Piece.None;
                }
            }
        }

        public static Move InvalidMove
        {
            get
            {
                return new Move(0);
            }
        }

        public string ConvertToBinary()
        {
            return Convert.ToString(startSquareMask & endSquareMask, 2);
        }

        public Move(ushort moveValue)
        {
            this.moveValue = moveValue;
        }

        public Move(int startSquare, int endSquare)
        {
            moveValue = (ushort)(startSquare | endSquare << 6);
        }

        public Move(int startSquare, int endSquare, int flag)
        {
            moveValue = (ushort)(startSquare | endSquare << 6 | flag << 12);
        }
    }
}
