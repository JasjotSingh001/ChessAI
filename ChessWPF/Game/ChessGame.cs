using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.Game
{
    public class ChessGame
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public const string StartFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private Board chessBoard = new Board();

        private MoveGenerator moveGenerator;
        private List<Move> moves = new List<Move>();
        public ChessGame()
        {
            chessBoard.LoadFromFEN(StartFEN);
            moveGenerator = new MoveGenerator();
            moves = moveGenerator.GenerateMoves(chessBoard);
        }

        public void GameLoop()
        {
            moves = moveGenerator.GenerateMoves(chessBoard);

            logger.Info("Number of moves: " + moves.Count);

            if (moves.Count == 0)
            {
                logger.Info("Checkmate");
            }
        }

        public bool CanMakeMove(int startIndex, int endIndex)
        {
            Move move = new Move(startIndex, endIndex);

            //Deal with promotion cases later
            if (moves.Any(move1 => move1.StartSquare == move.StartSquare && move1.EndSquare == move.EndSquare)) 
            {
                logger.Info("Move made was in list of moves generated");
                return true;
            }
            logger.Info("Move made was NOT in list of moves generated");
            return false;
        }

        public int MakeMove(int startIndex, int endIndex)
        {
            Move moveWithAnyFlags;

            if (CanMakeMove(startIndex, endIndex))
            {
                Move move = new Move(startIndex, endIndex);
                moveWithAnyFlags = moves.First(move1 => move1.StartSquare == move.StartSquare && move1.EndSquare == move.EndSquare);

                logger.Info("flag: " + moveWithAnyFlags.MoveFlag);

                chessBoard.MakeMove(moveWithAnyFlags);
                GameLoop();

                return moveWithAnyFlags.MoveFlag;
            }

            return 0;
        }

        public void MakeMove(Move move)
        {
            //Move moveWithAnyFlags;

            /*if (CanMakeMove(startIndex, endIndex))
            {
                Move move = new Move(startIndex, endIndex);
                moveWithAnyFlags = moves.First(move1 => move1.StartSquare == move.StartSquare && move1.EndSquare == move.EndSquare);

                logger.Info("flag: " + moveWithAnyFlags.MoveFlag);

                chessBoard.MakeMove(moveWithAnyFlags);
                GameLoop();

                return moveWithAnyFlags.MoveFlag;
            }*/

            chessBoard.MakeMove(move);
            GameLoop();
        }

        public Board ChessBoard
        {
            get { return chessBoard; }
        }

        public int ColourToMoveIndex
        {
            get
            {
                return chessBoard.ColourToMoveIndex();
            }
        }

        public bool WhiteToMove
        {
            get
            {
                return chessBoard.IsWhiteToMove();
            }
        }

        public List<Move> Moves
        {
            get
            {
                return moves;
            }
        }
    }
}
