using ChessWPF.Game;
using ChessWPF.Interface;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ChessWPF
{
    /// <summary>
    /// Interaction logic for ChessBoardControl.xaml
    /// </summary>
    public partial class ChessBoardControl : UserControl
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        private SolidColorBrush LightColorBrush = (SolidColorBrush) new BrushConverter().ConvertFrom("#F1D9B4");
        private SolidColorBrush DarkColorBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#B48963");

        private Dictionary<int, Image> PieceList = new Dictionary<int, Image>();

        private bool FromWhitePerspective = true;

        private DrawingImage bPDrawingImage;
        private DrawingImage bNDrawingImage;
        private DrawingImage bBDrawingImage;
        private DrawingImage bRDrawingImage;
        private DrawingImage bQDrawingImage;
        private DrawingImage bKDrawingImage;

        private DrawingImage wPDrawingImage;
        private DrawingImage wNDrawingImage;
        private DrawingImage wBDrawingImage;
        private DrawingImage wRDrawingImage;
        private DrawingImage wQDrawingImage;
        private DrawingImage wKDrawingImage;

        public ChessBoardControl()
        {
            InitializeComponent();
            SetupDrawingImages();
        }

        public void SetupBoard()
        {
            logger.Info("Setting up board");

            int nextX = 0;
            int nextY = 0;

            bool nextSquareToFillIsLight = true;

            for (int i = 0; i < 64; i++)
            {
                if (nextX >= 8)
                {
                    nextX = 0;
                    nextY++;
                    nextSquareToFillIsLight = !nextSquareToFillIsLight;
                }

                Rectangle rect = new Rectangle
                {
                    Fill = nextSquareToFillIsLight ? LightColorBrush : DarkColorBrush
                };

                ChessBoardGrid.Children.Add(rect);
                Grid.SetRow(rect, nextY);
                Grid.SetColumn(rect, nextX);

                nextSquareToFillIsLight = !nextSquareToFillIsLight;
                nextX++;
            }
        }

        public void LoadPosition(int[] board)
        {
            logger.Info("Loading position");

            for (int i = 0; i < 64; i++)
            {
                int row, column;
                if (FromWhitePerspective) 
                {
                    column = i % 8;
                    row = 7 - ((i / 8) % 8);
                } else
                {
                    row = i / 8;
                    column = 7 - (i % 8);
                }

                Image image = new Image();
                switch (board[i])
                {
                    case Piece.WhitePawn:
                        image.Source = wPDrawingImage;
                        image.Width = ChessBoardGrid.ColumnDefinitions.ElementAt(0).ActualWidth * 0.55;
                        break;
                    case Piece.WhiteKnight:
                        image.Source = wNDrawingImage;
                        image.Width = ChessBoardGrid.ColumnDefinitions.ElementAt(0).ActualWidth * 0.80;
                        break;
                    case Piece.WhiteBishop:
                        image.Source = wBDrawingImage;
                        image.Width = ChessBoardGrid.ColumnDefinitions.ElementAt(0).ActualWidth * 0.80;
                        break;
                    case Piece.WhiteRook:
                        image.Source = wRDrawingImage;
                        image.Width = ChessBoardGrid.ColumnDefinitions.ElementAt(0).ActualWidth * 0.70;
                        break;
                    case Piece.WhiteQueen: 
                        image.Source = wQDrawingImage;
                        image.Width = ChessBoardGrid.ColumnDefinitions.ElementAt(0).ActualWidth * 0.80;
                        break;
                    case Piece.WhiteKing:
                        image.Source = wKDrawingImage;
                        image.Width = ChessBoardGrid.ColumnDefinitions.ElementAt(0).ActualWidth * 0.80;
                        break;
                    case Piece.BlackPawn:
                        image.Source = bPDrawingImage;
                        image.Width = ChessBoardGrid.ColumnDefinitions.ElementAt(0).ActualWidth * 0.55;
                        break;
                    case Piece.BlackKnight:
                        image.Source = bNDrawingImage;
                        image.Width = ChessBoardGrid.ColumnDefinitions.ElementAt(0).ActualWidth * 1.5;
                        break;
                    case Piece.BlackBishop:
                        image.Source= bBDrawingImage;
                        image.Width = ChessBoardGrid.ColumnDefinitions.ElementAt(0).ActualWidth * 0.80;
                        break;
                    case Piece.BlackRook:
                        image.Source = bRDrawingImage;
                        image.Width = ChessBoardGrid.ColumnDefinitions.ElementAt(0).ActualWidth * 0.70;
                        break;
                    case Piece.BlackQueen:
                        image.Source = bQDrawingImage;
                        image.Width = ChessBoardGrid.ColumnDefinitions.ElementAt(0).ActualWidth * 0.80;
                        break;
                    case Piece.BlackKing:
                        image.Source = bKDrawingImage;
                        image.Width = ChessBoardGrid.ColumnDefinitions.ElementAt(0).ActualWidth * 0.80;
                        break;
                }

                PieceList.Add(i, image);

                ChessBoardGrid.Children.Add(image);
                Grid.SetColumn(image, column);
                Grid.SetRow(image, row);
            }
        }

        public void MovePiece(int startIndex, int endIndex) //need flags for en passant, castling, etc.
        {
            int[] startColumnRow = ConvertIndexToColumnRow(startIndex);
            int[] endColumnRow = ConvertIndexToColumnRow(endIndex);

            //hacky solution, consider changing it later
            Image previousPiece = PieceList.GetValueOrDefault(startIndex);
            PieceList.Remove(startIndex);

            Image nextPiece = PieceList.GetValueOrDefault(endIndex);
            PieceList.Remove(endIndex);

            ChessBoardGrid.Children.Remove(nextPiece);
            Grid.SetColumn(previousPiece, endColumnRow[0]);
            Grid.SetRow(previousPiece, endColumnRow[1]);

            PieceList.Add(endIndex, previousPiece);
        }

        public void MovePiece(int startIndex, int endIndex, int flag)
        {
            if (flag == 0 || flag == Move.Flag.PawnTwoForward)
            {
                int[] startColumnRow = ConvertIndexToColumnRow(startIndex);
                int[] endColumnRow = ConvertIndexToColumnRow(endIndex);

                //hacky solution, consider changing it later
                Image previousPiece = PieceList.GetValueOrDefault(startIndex);
                PieceList.Remove(startIndex);

                Image nextPiece = PieceList.GetValueOrDefault(endIndex);
                PieceList.Remove(endIndex);

                ChessBoardGrid.Children.Remove(nextPiece);
                Grid.SetColumn(previousPiece, endColumnRow[0]);
                Grid.SetRow(previousPiece, endColumnRow[1]);

                PieceList.Add(endIndex, previousPiece);
            } else if (flag == Move.Flag.EnPassantCapture)
            {
                MovePiece(startIndex, endIndex);
                if (endIndex > startIndex)
                {
                    RemovePiece(endIndex - 8);
                } else
                {
                    RemovePiece(endIndex + 8);
                }
            } else if (flag == Move.Flag.Castling)
            {
                if (endIndex == 6)
                {
                    MovePiece(4, 6);
                    MovePiece(7, 5);
                } else if (endIndex == 2)
                {
                    MovePiece(4, 2);
                    MovePiece(0, 3);
                } else if (endIndex == 62)
                {
                    MovePiece(60, 62);
                    MovePiece(63, 61);
                } else
                {
                    MovePiece(60, 58);
                    MovePiece(56, 59);
                }
            } else
            {

            }
        }

        public void RemovePiece(int pieceIndex)
        {
            Image pieceToBeRemoved = PieceList.GetValueOrDefault(pieceIndex);
            PieceList.Remove(pieceIndex);
            ChessBoardGrid.Children.Remove(pieceToBeRemoved);
        }

        public int Grid_HitTest (MouseEventArgs e)
        {
            UIElement element = (UIElement)ChessBoardGrid.InputHitTest(e.GetPosition(ChessBoardGrid));

            int column = Grid.GetColumn(element);
            int row = Grid.GetRow(element);

            logger.Info(column + " " + row);
            //logger.Info("Conversion: " + ConvertColumnRowToIndex(column, row));

            return ConvertColumnRowToIndex(column, row);
        }

        private int ConvertColumnRowToIndex(int column, int row)
        {
            if (FromWhitePerspective)
            {
                row = 7 - row;
                return (8 * row) + column;
            }

            column = 7 - column;
            return (8 * row) + column;
        }

        private int[] ConvertIndexToColumnRow(int index)
        {
            int column, row;
            if (FromWhitePerspective)
            {
                column = index % 8;
                row = 7 - (index / 8);

                return new int[] { column, row };
            }
            column = 7 - (index % 8);
            row = index / 8;

            return new int[] { column, row };
        }

        private void SetupDrawingImages()
        {
            bPDrawingImage = (DrawingImage)FindResource("bPDrawingImage");
            bNDrawingImage = (DrawingImage)FindResource("bNDrawingImage");
            bBDrawingImage = (DrawingImage)FindResource("bBDrawingImage");
            bRDrawingImage = (DrawingImage)FindResource("bRDrawingImage");
            bQDrawingImage = (DrawingImage)FindResource("bQDrawingImage");
            bKDrawingImage = (DrawingImage)FindResource("bKDrawingImage");

            wPDrawingImage = (DrawingImage)FindResource("wPDrawingImage");
            wNDrawingImage = (DrawingImage)FindResource("wNDrawingImage");
            wBDrawingImage = (DrawingImage)FindResource("wBDrawingImage");
            wRDrawingImage = (DrawingImage)FindResource("wRDrawingImage");
            wQDrawingImage = (DrawingImage)FindResource("wQDrawingImage");
            wKDrawingImage = (DrawingImage)FindResource("wKDrawingImage");
        }
    }
}