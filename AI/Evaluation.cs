using ChessWPF.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.AI
{
    public class Evaluation
    {
        public const int PawnValue = 100;
        public const int KnightValue = 300;
        public const int BishopValue = 315;
        public const int RookValue = 500;
        public const int QueenValue = 900;

        private Board board;

        public int Evaluate(Board board)
        {
            this.board = board;

            int whiteEval = CountMaterial(Board.WhiteIndex);
            int blackEval = CountMaterial(Board.BlackIndex);

            int evaluation = whiteEval - blackEval;

            int perspective = (board.IsWhiteToMove()) ? 1 : -1;

            return evaluation * perspective;
        }

        private int CountMaterial(int colourIndex)
        {
            int material = 0;

            material += board.GetPieceList(colourIndex, Piece.Pawn).GetCount();
            material += board.GetPieceList(colourIndex, Piece.Knight).GetCount();
            material += board.GetPieceList(colourIndex, Piece.Bishop).GetCount();
            material += board.GetPieceList(colourIndex, Piece.Rook).GetCount();
            material += board.GetPieceList(colourIndex, Piece.Queen).GetCount();

            return material;
        }
    }
}
