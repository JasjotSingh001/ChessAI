﻿using ChessWPF.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for LocalGamePage.xaml
    /// </summary>
    public partial class LocalGamePage : Page
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private ChessGame Game;

        public LocalGamePage()
        {
            logger.Info("Setting up game page");
            InitializeComponent();

            Game = new ChessGame();
        }

        private void DrawInitialPosition()
        {
            ChessBoardControl.LoadPosition(Game.ChessBoard.GetBoard());
        }
    }
}
