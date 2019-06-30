using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Board
    {
        private char[,] board = new char[,] { { '1', '2', '3' }, { '4', '5', '6' }, { '7', '8', '9' } };
        private const uint boardSize = 3;
        public Board() { }

        public String ToString()
        {
            String output;
            var builder = new StringBuilder();
            builder.Append("Display current board: \n");
            for (uint i=0;i< boardSize; i++)
            {
                builder.AppendFormat("{0} | {1} | {2} \n", board[i,0] , board[i,1], board[i,2]);
            }
            output = builder.ToString();
            return output;
        }

        public char[,] GetBoard()
        {
            return board;
        }

        public void Update(int pos, char player)
        {
            if (pos < 4)
            {
                board[0, pos - 1] = player;
            }
            else if (pos < 7)
            {
                board[1, pos - 4] = player;
            }
            else
            {
                board[2, pos - 7] = player;
            }
        }
    }
}
