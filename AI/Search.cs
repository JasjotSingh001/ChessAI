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

        private Evaluation evaluation;
        private Move bestMove;

        private const int immediateMateScore = 100000;
        private const int positiveInfinity = 9999999;
        private const int negativeInfinity = -positiveInfinity;

        public Search(Board board) 
        {
            this.board = board;
            evaluation = new Evaluation();
        }

        public Move StartSearch()
        {
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

            Move bestMoveThisPosition = new Move();

            for (int i = 0; i < moves.Count; i++)
            {
                board.MakeMove(moves[i]);
                int eval = -SearchPositions(depth - 1, plyFromRoot + 1, -beta, -alpha);
                board.UnmakeMove(moves[i]);

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

            for (int i = 0; i < moves.Count; i++) 
            {
                board.MakeMove(moves[i]);
                eval = -QuiescenceSearch(-beta, -alpha);
                board.UnmakeMove(moves[i]);

                if (eval > alpha)
                {
                    alpha = eval;
                }
            }
            return alpha;
        }
    }
}
