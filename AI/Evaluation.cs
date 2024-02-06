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

        private const float endgameMaterialStart = RookValue * 2 + BishopValue + KnightValue;

        private Board board;

        public int Evaluate(Board board)
        {
            this.board = board;

            int whiteEval = 0;
            int blackEval = 0;

            int whiteMaterial = CountMaterial(Board.WhiteIndex);
            int blackMaterial = CountMaterial(Board.BlackIndex);

            int whiteMaterialWithoutPawns = whiteMaterial - board.GetPieceList(Board.WhiteIndex, Piece.Pawn).GetCount() * PawnValue;
            int blackMaterialWithoutPawns = blackMaterial - board.GetPieceList(Board.BlackIndex, Piece.Pawn).GetCount() * PawnValue;

            float whiteEndgamePhaseWeight = EndgamePhaseWeight(whiteMaterialWithoutPawns);
            float blackEndgamePhaseWeight = EndgamePhaseWeight(blackMaterialWithoutPawns);

            whiteEval += whiteMaterial;
            blackEval += blackMaterial;
            whiteEval += EvaluatePieceSquareTables(Board.WhiteIndex, blackEndgamePhaseWeight);
            blackEval += EvaluatePieceSquareTables(Board.BlackIndex, whiteEndgamePhaseWeight);

            int evaluation = whiteEval - blackEval;

            int perspective = (board.IsWhiteToMove()) ? 1 : -1;

            return evaluation * perspective;
        }

        private int CountMaterial(int colourIndex)
        {
            int material = 0;

            material += board.GetPieceList(colourIndex, Piece.Pawn).GetCount() * PawnValue;
            material += board.GetPieceList(colourIndex, Piece.Knight).GetCount() * KnightValue;
            material += board.GetPieceList(colourIndex, Piece.Bishop).GetCount() * BishopValue;
            material += board.GetPieceList(colourIndex, Piece.Rook).GetCount() * RookValue;
            material += board.GetPieceList(colourIndex, Piece.Queen).GetCount() * QueenValue;

            return material;
        }

        private int EvaluatePieceSquareTables(int colourIndex, float endgamePhaseWeight)
        {
            int value = 0;
            bool isWhite = colourIndex == Board.WhiteIndex;
            value += EvaluatePieceSquareTable(PieceSquareTable.pawns, board.GetPieceList(colourIndex, Piece.Pawn), isWhite);
            value += EvaluatePieceSquareTable(PieceSquareTable.rooks, board.GetPieceList(colourIndex, Piece.Rook), isWhite);
            value += EvaluatePieceSquareTable(PieceSquareTable.knights, board.GetPieceList(colourIndex, Piece.Knight), isWhite);
            value += EvaluatePieceSquareTable(PieceSquareTable.bishops, board.GetPieceList(colourIndex, Piece.Bishop), isWhite);
            value += EvaluatePieceSquareTable(PieceSquareTable.queens, board.GetPieceList(colourIndex, Piece.Queen), isWhite);
            int kingEarlyPhase = PieceSquareTable.Read(PieceSquareTable.kingMiddle, board.GetKingIndex(colourIndex), isWhite);
            value += (int)(kingEarlyPhase * (1 - endgamePhaseWeight));
            //value += PieceSquareTable.Read (PieceSquareTable.kingMiddle, board.KingSquare[colourIndex], isWhite);

            return value;
        }

        private static int EvaluatePieceSquareTable(int[] table, PieceList pieceList, bool isWhite)
        {
            int value = 0;
            for (int i = 0; i < pieceList.GetCount(); i++)
            {
                value += PieceSquareTable.Read(table, pieceList[i], isWhite);
            }
            return value;
        }

       private float EndgamePhaseWeight(int materialCountWithoutPawns)
        {
            const float multiplier = 1 / endgameMaterialStart;
            return 1 - Math.Min(1, materialCountWithoutPawns * multiplier);
        }
    }
}
