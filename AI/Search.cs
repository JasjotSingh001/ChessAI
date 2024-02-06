using ChessWPF.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace ChessWPF.AI
{
    public class Search
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private MoveGenerator moveGenerator = new MoveGenerator();
        private Board board;

        private MoveOrdering moveOrdering;
        private Evaluation evaluation;

        private Move bestMove;

        private const int immediateMateScore = 100000;
        private const int positiveInfinity = 9999999;
        private const int negativeInfinity = -positiveInfinity;

        private int numNodes = 0;
        private int numQNodes = 0;

        public Search(Board board) 
        {
            this.board = board;
            evaluation = new Evaluation();
            moveOrdering = new MoveOrdering(moveGenerator);
        }

        public Move StartSearch()
        {
            numNodes = 0;
            numQNodes = 0;
            SearchPositions(5, 0, negativeInfinity, positiveInfinity);
            return bestMove;
        }
        private int SearchPositions(int depth, int plyFromRoot, int alpha, int beta)
        {
            if (depth == 0)
            {
                return evaluation.Evaluate(board);
            }

            List<Move> moves = moveGenerator.GenerateMoves(board);
            if (moves.Count == 0)
            {
                if (moveGenerator.InCheck)
                {
                    int mateScore = immediateMateScore - plyFromRoot;
                    return -mateScore;
                }
                //Stalemate
                return 0;
            }

            Move bestMoveThisPosition = Move.InvalidMove;
            moveOrdering.OrderMoves(board, moves);

            for (int i = 0; i < moves.Count; i++)
            {
                board.MakeMove(moves[i]);
                int eval = -SearchPositions(depth - 1, plyFromRoot + 1, -beta, -alpha);
                board.UnmakeMove(moves[i]);

                numNodes++;

                if (eval >= beta)
                {
                    return beta;
                }

                if (eval > alpha)
                {
                    bestMoveThisPosition = moves[i];
                    alpha = eval;
                }
            }

            bestMove = bestMoveThisPosition;

            logger.Info("Number of nodes evaluated in regular search: " + numNodes);

            return alpha;
        }

        private int QuiescenceSearch(int alpha, int beta)
        {
            int eval = evaluation.Evaluate(board);
            
            if (eval >= beta)
            {
                return beta;
            }
            if (eval > alpha)
            {
                alpha = eval;
            }

            List<Move> moves = moveGenerator.GenerateMoves(board, false);
            moveOrdering.OrderMoves(board, moves);

            for (int i = 0; i < moves.Count; i++) 
            {
                board.MakeMove(moves[i]);
                eval = -QuiescenceSearch(-beta, -alpha);
                board.UnmakeMove(moves[i]);

                numQNodes++;

                if (eval > alpha)
                {
                    alpha = eval;
                }
            }

            logger.Info("Number of nodes evaluated in quiescence search: " + numQNodes);

            return alpha;
        }

        public int MoveGenerationTest(int depth)
        {
            if (depth == 0)
            {
                return 1;
            }

            int num = 0;

            List<Move> moves = moveGenerator.GenerateMoves(board);

            for (int i = 0; i < moves.Count; i++) 
            {
                board.MakeMove(moves[i]);
                num += MoveGenerationTest(depth - 1);
                board.UnmakeMove(moves[i]);
            }

            return num;
        }
    }
}
