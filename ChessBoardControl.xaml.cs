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

        private List<int> PiecesOnBoard = new List<int>();
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

            ChessGame Game = new ChessGame();
            LoadPosition(Game.ChessBoard.GetBoard());
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
                    column = i % 8;
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

                ChessBoardGrid.Children.Add(image);
                Grid.SetRow(image, row);
                Grid.SetColumn(image, column);
            }
        }

        private void ChessBoardGrid_Loaded(object sender, RoutedEventArgs e)
        {
            SetupBoard();
        }
    }
}
