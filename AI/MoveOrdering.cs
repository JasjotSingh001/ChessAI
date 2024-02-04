using ChessWPF.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.AI
{
    public class MoveOrdering
    {
        private int[] moveScores;
        private const int maxMoveCount = 218;

        private const int squareControlledByOpponentPawnPenalty = 350;
        private const int capturedPieceValueMultiplier = 10;

        private MoveGenerator moveGenerator;
        private Move invalidMove;

        public MoveOrdering(MoveGenerator moveGenerator)
        {
            this.moveGenerator = moveGenerator;
            invalidMove = Move.InvalidMove;

            moveScores = new int[maxMoveCount];
        }

        public void OrderMoves(Board board, List<Move> moves)
        {
            for (int i = 0; i < moves.Count; i++)
            {
                int score = 0;
                int movePieceType = Piece.PieceType(board.GetBoard()[moves[i].StartSquare]);
                int capturePieceType = Piece.PieceType(board.GetBoard()[moves[i].EndSquare]);
                int flag = moves[i].MoveFlag;

                if (capturePieceType != Piece.None)
                {
                    // Order moves to try capturing the most valuable opponent piece with least valuable of own pieces first
                    // The capturedPieceValueMultiplier is used to make even 'bad' captures like QxP rank above non-captures
                    score = capturedPieceValueMultiplier * GetPieceValue(capturePieceType) - GetPieceValue(movePieceType);
                }

                if (movePieceType == Piece.Pawn)
                {

                    if (flag == Move.Flag.PromoteToQueen)
                    {
                        score += Evaluation.QueenValue;
                    }
                    else if (flag == Move.Flag.PromoteToKnight)
                    {
                        score += Evaluation.KnightValue;
                    }
                    else if (flag == Move.Flag.PromoteToRook)
                    {
                        score += Evaluation.RookValue;
                    }
                    else if (flag == Move.Flag.PromoteToBishop)
                    {
                        score += Evaluation.BishopValue;
                    }
                }
                else
                {
                    // Penalize moving piece to a square attacked by opponent pawn
                    if (ContainsSquare(moveGenerator.opponentPawnAttackMap, moves[i].EndSquare))
                    {
                        score -= squareControlledByOpponentPawnPenalty;
                    }
                }

                moveScores[i] = score;
            }

            Sort(moves);
        }

        private static int GetPieceValue(int pieceType)
        {
            switch (pieceType)
            {
                case Piece.Queen:
                    return Evaluation.QueenValue;
                case Piece.Rook:
                    return Evaluation.RookValue;
                case Piece.Knight:
                    return Evaluation.KnightValue;
                case Piece.Bishop:
                    return Evaluation.BishopValue;
                case Piece.Pawn:
                    return Evaluation.PawnValue;
                default:
                    return 0;
            }
        }

        private void Sort(List<Move> moves)
        {
            // Sort the moves list based on scores
            for (int i = 0; i < moves.Count - 1; i++)
            {
                for (int j = i + 1; j > 0; j--)
                {
                    int swapIndex = j - 1;
                    if (moveScores[swapIndex] < moveScores[j])
                    {
                        (moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
                        (moveScores[j], moveScores[swapIndex]) = (moveScores[swapIndex], moveScores[j]);
                    }
                }
            }
        }

        private bool ContainsSquare(ulong bitboard, int square)
        {
            return ((bitboard >> square) & 1) != 0;
        }
    }
}
