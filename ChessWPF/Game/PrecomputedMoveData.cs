using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.Game
{
    public static class PrecomputedMoveData
    {
        public static readonly int[] DirectionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 };
        public static readonly int[][] NumSquaresToEdge = new int[64][];

        // Stores array of indices for each square a knight can land on from any square on the board
        // KnightMoves[0] is equal to {10, 17}, meaning a knight on a1 can jump to c2 and b3
        public static readonly byte[][] KnightMoves = new byte[64][];
        public static readonly byte[][] KingMoves = new byte[64][];

        // Pawn attack directions for white and black (NW, NE; SW SE), refers to indices of DirectionOffsets, e.g 4 = DirectionOffsets[4] = 7
        public static readonly byte[][] PawnAttackDirections = {
            new byte[] { 4, 6 },
            new byte[] { 7, 5 }
        };

        public static readonly int[][] PawnAttacksWhite = new int[64][];
        public static readonly int[][] PawnAttacksBlack = new int[64][];
        public static readonly int[] DirectionLookup = new int[127];

        public static readonly ulong[] KingAttackBitboards = new ulong[64];
        public static readonly ulong[] KnightAttackBitboards = new ulong[64];
        public static readonly ulong[][] PawnAttackBitboards = new ulong[64][];

        public static readonly ulong[] RookMoves = new ulong[64];
        public static readonly ulong[] BishopMoves = new ulong[64];
        public static readonly ulong[] QueenMoves = new ulong[64];

        static PrecomputedMoveData()
        {
            int[] allKnightJumps = { 15, 17, -17, -15, 10, -6, 6, -10 };

            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                int x = squareIndex % 8;
                int y = squareIndex / 8;

                int north = 7 - y;
                int south = y;
                int west = x;
                int east = 7 - x;

                NumSquaresToEdge[squareIndex] = new int[8];
                NumSquaresToEdge[squareIndex][0] = north;
                NumSquaresToEdge[squareIndex][1] = south;
                NumSquaresToEdge[squareIndex][2] = west;
                NumSquaresToEdge[squareIndex][3] = east;
                NumSquaresToEdge[squareIndex][4] = Min(north, west);
                NumSquaresToEdge[squareIndex][5] = Min(south, east);
                NumSquaresToEdge[squareIndex][6] = Min(north, east);
                NumSquaresToEdge[squareIndex][7] = Min(south, west);

                List<byte> legalKnightJumps = new List<byte>();
                ulong knightBitboard = 0;
                foreach (int knightJumpDelta in allKnightJumps)
                {
                    int knightJumpSquare = squareIndex + knightJumpDelta;
                    if (knightJumpSquare >= 0 && knightJumpSquare < 64)
                    {
                        int knightSquareY = knightJumpSquare / 8;
                        int knightSquareX = knightJumpSquare - knightSquareY * 8;
                        // Ensure knight has moved max of 2 squares on x/y axis (to reject indices that have wrapped around side of board)
                        int maxCoordMoveDst = Max(Abs(x - knightSquareX), Abs(y - knightSquareY));
                        if (maxCoordMoveDst == 2)
                        {
                            legalKnightJumps.Add((byte)knightJumpSquare);
                            knightBitboard |= 1ul << knightJumpSquare;
                        }
                    }
                }

                KnightMoves[squareIndex] = legalKnightJumps.ToArray();
                KnightAttackBitboards[squareIndex] = knightBitboard;

                // Calculate all squares king can move to from current square (not including castling)
                List<byte> legalKingMoves = new List<byte>();
                foreach (int kingMoveDelta in DirectionOffsets)
                {
                    int kingMoveSquare = squareIndex + kingMoveDelta;
                    if (kingMoveSquare >= 0 && kingMoveSquare < 64)
                    {
                        int kingSquareY = kingMoveSquare / 8;
                        int kingSquareX = kingMoveSquare - kingSquareY * 8;

                        // Ensure king has moved max of 1 square on x/y axis (to reject indices that have wrapped around side of board)
                        int maxCoordMoveDst = Max(Abs(x - kingSquareX), Abs(y - kingSquareY));
                        if (maxCoordMoveDst == 1)
                        {
                            legalKingMoves.Add((byte)kingMoveSquare);
                            KingAttackBitboards[squareIndex] |= 1ul << kingMoveSquare;
                        }
                    }
                }
                KingMoves[squareIndex] = legalKingMoves.ToArray();

                List<int> pawnCapturesWhite = new List<int>();
                List<int> pawnCapturesBlack = new List<int>();
                PawnAttackBitboards[squareIndex] = new ulong[2];
                if (x > 0)
                {
                    if (y < 7)
                    {
                        pawnCapturesWhite.Add(squareIndex + 7);
                        PawnAttackBitboards[squareIndex][0] |= 1ul << (squareIndex + 7);
                    }
                    if (y > 0)
                    {
                        pawnCapturesBlack.Add(squareIndex - 9);
                        PawnAttackBitboards[squareIndex][1] |= 1ul << (squareIndex - 9);
                    }
                }
                if (x < 7)
                {
                    if (y < 7)
                    {
                        pawnCapturesWhite.Add(squareIndex + 9);
                        PawnAttackBitboards[squareIndex][0] |= 1ul << (squareIndex + 9);
                    }
                    if (y > 0)
                    {
                        pawnCapturesBlack.Add(squareIndex - 7);
                        PawnAttackBitboards[squareIndex][1] |= 1ul << (squareIndex - 7);
                    }
                }
                PawnAttacksWhite[squareIndex] = pawnCapturesWhite.ToArray();
                PawnAttacksBlack[squareIndex] = pawnCapturesBlack.ToArray();

                // Rook moves
                for (int directionIndex = 0; directionIndex < 4; directionIndex++)
                {
                    int currentDirOffset = DirectionOffsets[directionIndex];
                    for (int n = 0; n < NumSquaresToEdge[squareIndex][directionIndex]; n++)
                    {
                        int targetSquare = squareIndex + currentDirOffset * (n + 1);
                        RookMoves[squareIndex] |= 1ul << targetSquare;
                    }
                }
                // Bishop moves
                for (int directionIndex = 4; directionIndex < 8; directionIndex++)
                {
                    int currentDirOffset = DirectionOffsets[directionIndex];
                    for (int n = 0; n < NumSquaresToEdge[squareIndex][directionIndex]; n++)
                    {
                        int targetSquare = squareIndex + currentDirOffset * (n + 1);
                        BishopMoves[squareIndex] |= 1ul << targetSquare;
                    }
                }
                QueenMoves[squareIndex] = RookMoves[squareIndex] | BishopMoves[squareIndex];
            }

            for (int i = 0; i < 127; i++)
            {
                int offset = i - 63;
                int absOffset = Abs(offset);
                int absDir = 1;
                if (absOffset % 9 == 0)
                {
                    absDir = 9;
                }
                else if (absOffset % 8 == 0)
                {
                    absDir = 8;
                }
                else if (absOffset % 7 == 0)
                {
                    absDir = 7;
                }

                DirectionLookup[i] = absDir * Sign(offset);
            }
        }
    }
}
