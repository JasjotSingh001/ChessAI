using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessWPF.Game
{
    public class PieceList
    {
        // Indices of squares occupied by given piece type (only elements up to Count are valid, the rest are unused/garbage)
        private int[] occupiedSquares;

        // Map to go from index of a square, to the index in the occupiedSquares array where that square is stored
        private int[] map;
        private int numPieces;

        public PieceList(int maxPieceCount = 16)
        {
            occupiedSquares = new int[maxPieceCount];
            map = new int[64];
            numPieces = 0;
        }

        public void AddPieceAtIndex(int index)
        {
            occupiedSquares[numPieces] = index;
            map[index] = numPieces;
            numPieces++;
        }

        public void RemovePieceAtIndex(int index)
        {
            int pieceIndex = map[index]; // get the index of this element in the occupiedSquares array
            occupiedSquares[pieceIndex] = occupiedSquares[numPieces - 1]; // move last element in array to the place of the removed element
            map[occupiedSquares[pieceIndex]] = pieceIndex; // update map to point to the moved element's new location in the array
            numPieces--;
        }

        public void MovePiece(int startSquare, int endSquare)
        {
            int pieceIndex = map[startSquare]; // get the index of this element in the occupiedSquares array
            occupiedSquares[pieceIndex] = endSquare;
            map[endSquare] = pieceIndex;
        }

        public int GetCount()
        {
            return numPieces;
        }

        public int this[int index] => occupiedSquares[index];
    }
}
