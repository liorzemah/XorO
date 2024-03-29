﻿using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace Server
{
    class Program
    {
        //static string SERVER_PASSWORD = "12345";
        enum StateMsgs {
            StartGame = 1,
            WaitForPlayer = 2,
            WaitForTwoPlayers = 3,
            SelectPosition = 4,
            SendBoard = 5,
            PositionAlreadyExit = 6,
            YouLose = 7,
            YouWin = 8,
        }
        static Dictionary<string, string> userDictionary = new Dictionary<string, string>();
        static uint countOfPlayers = 0;
        static string currentUser = "";
        static Dictionary<StateMsgs, string> msgsDictionary = new Dictionary<StateMsgs, string>();
        static Board board = new Board();
        static char playerTurn = 'X';
        static List<int> availablePositions = new List<int>(){ 1,2,3,4,5,6,7,8,9 };
        static char playerWin = 'N';

        static void Main(string[] args)
        {
            userDictionary.Add("alice", "alice111");
            userDictionary.Add("bob", "bob222");
            //add noga
            userDictionary.Add("noga", "noga111");
            //add noa
            userDictionary.Add("noa", "noa222");
            //add karin
            userDictionary.Add("karin", "karin333");

            msgsDictionary.Add(StateMsgs.StartGame, "Start Game");
            msgsDictionary.Add(StateMsgs.WaitForPlayer, "Wait for player");
            msgsDictionary.Add(StateMsgs.WaitForTwoPlayers, "wait for two players");
            msgsDictionary.Add(StateMsgs.SelectPosition, "Select position (1-9): ");
            //msgsDictionary.Add(StateMsgs.SendBoard, "");
            msgsDictionary.Add(StateMsgs.PositionAlreadyExit, "Position already catch");
            msgsDictionary.Add(StateMsgs.YouLose, "You lose!");
            msgsDictionary.Add(StateMsgs.YouWin, "You win!");

            try
            {

                IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
                TcpListener myListener = new TcpListener(ipAddr, 7);
                myListener.Start();
                Console.WriteLine("The srver is running at port: " + myListener.LocalEndpoint);

                Console.WriteLine(msgsDictionary[StateMsgs.WaitForTwoPlayers]);

                while (countOfPlayers < 2)
                {
                    Socket socket = myListener.AcceptSocket();
                    Console.WriteLine("Connection accepted from" + socket.RemoteEndPoint);
                    ParameterizedThreadStart pts1 = new ParameterizedThreadStart(HandleClient);
                    Thread t1 = new Thread(pts1);
                    t1.Start(socket);

                }

                myListener.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error...." + e.ToString());
            }
            Console.WriteLine("nClient closed connection");
            Console.ReadLine();

        }

        static void HandleClient(object data)
        {
            Socket t_socket = data as Socket;
            byte[] binDataIn = new byte[255];
            if (DoAuthenticate(t_socket, binDataIn))
            {
                countOfPlayers++;
                if (countOfPlayers == 1)
                {
                    Console.WriteLine(msgsDictionary[StateMsgs.WaitForPlayer]);
                }
                MakeSession(t_socket, binDataIn);

            }
            else
                t_socket.Close();
        }

        private static bool DoAuthenticate(Socket socket, byte[] binDataIn)
        {
            ASCIIEncoding asciiEnc = new ASCIIEncoding();

            if (CheckUser(socket, binDataIn))
            {

                byte[] binDataOut = binDataOut = asciiEnc.GetBytes("1");
                socket.Send(binDataOut, 0, binDataOut.Length, SocketFlags.None);

                if (CheckPassword(socket, binDataIn))
                {
                    binDataOut = binDataOut = asciiEnc.GetBytes("1");
                    socket.Send(binDataOut, 0, binDataOut.Length, SocketFlags.None);
                    return true;
                }

            }
            else
            {
                byte[] binDataOut = binDataOut = asciiEnc.GetBytes("0");
                socket.Send(binDataOut, 0, binDataOut.Length, SocketFlags.None);
            }


            return false;

        }

        private static bool CheckUser(Socket socket, byte[] binDataIn)
        {
            bool flag = false;
            int k = socket.Receive(binDataIn);
            ASCIIEncoding asciiEnc = new ASCIIEncoding();
            string user = asciiEnc.GetString(binDataIn, 0, k);
            if (userDictionary.ContainsKey(user))
            {
                currentUser = user;
                flag = true;
            }

            return flag;

        }

        private static bool CheckPassword(Socket socket, byte[] binDataIn)
        {
            bool flag = false;
            int k = socket.Receive(binDataIn);
            ASCIIEncoding asciiEnc = new ASCIIEncoding();
            string pass = asciiEnc.GetString(binDataIn, 0, k);
            if (userDictionary[currentUser].Equals(pass))
            {
                flag = true;
            }

            return flag;

        }

        private static void SendMessage(Socket socket ,String message)
        {
            byte[] sendMsgBytes = Encoding.ASCII.GetBytes(message);
            socket.Send(sendMsgBytes);
        }

        private static void MakeSession(Socket socket, byte[] binDataIn)
        {
            //this is the session with client!!
            bool waitingOutputSent = false;
            bool startGame = false;
            char player = 'O';

            while (true)
            {
                if (countOfPlayers == 1 && !waitingOutputSent)
                {
                    SendMessage(socket, msgsDictionary[StateMsgs.WaitForPlayer]);
                    waitingOutputSent = true;
                    player = 'X';
                }
                else if (countOfPlayers == 2)
                {
                    if (!startGame)
                    {
                        Console.WriteLine(msgsDictionary[StateMsgs.StartGame]);
                        SendMessage(socket, msgsDictionary[StateMsgs.StartGame]);
                        startGame = true;
                    }

                    if (player == 'X')
                    {
                        if (playerWin == 'X')
                        {
                            Console.WriteLine("X: win");
                            SendMessage(socket, msgsDictionary[StateMsgs.YouWin]);
                            return;
                        }
                        else if (playerWin == 'O')
                        {
                            Console.WriteLine("X : lose");
                            SendMessage(socket, msgsDictionary[StateMsgs.YouLose]);
                            return;
                        }
                        if (playerTurn == 'X')
                        {
                            PlayTurn(socket, binDataIn, 'X');
                            playerTurn = 'O';
                        }
                    }
                    else // player == 'O'
                    {
                        if (playerWin == 'O')
                        {
                            Console.WriteLine("O: win");
                            SendMessage(socket, msgsDictionary[StateMsgs.YouWin]);
                            return;
                        }
                        else if (playerWin == 'X')
                        {
                            Console.WriteLine("O : lose");
                            SendMessage(socket, msgsDictionary[StateMsgs.YouLose]);
                            return;
                        }
                        if (playerTurn == 'O')
                        {
                            PlayTurn(socket, binDataIn, 'O');
                            playerTurn = 'X';
                        }
                    }

                }  
            }
            socket.Close();
        }

        // Return true if player win the game;
        private static void PlayTurn(Socket socket, byte[] binDataIn, char player)
        {
            bool turnEnd = false;
            int pos = 0;
            Console.WriteLine(board.ToString());
            SendMessage(socket, board.ToString());

            Console.WriteLine(msgsDictionary[StateMsgs.SelectPosition] + " from " + player);
            SendMessage(socket, msgsDictionary[StateMsgs.SelectPosition]);
            do
            {
                int size = socket.Receive(binDataIn);
                if (size == 0) break;
                ASCIIEncoding asciiEnc = new ASCIIEncoding();
                string recvPosition = asciiEnc.GetString(binDataIn, 0, size);
                pos = int.Parse(recvPosition);

                if (availablePositions.Contains(pos))
                {
                    board.Update(pos, player);
                    availablePositions.Remove(pos);
                    turnEnd = true;
                }
                else
                {
                    Console.WriteLine(player + ": " + msgsDictionary[StateMsgs.PositionAlreadyExit]);
                    SendMessage(socket, msgsDictionary[StateMsgs.PositionAlreadyExit]);
                }
            } while (!turnEnd);

            if(pos != 0 && checkWinner(pos, player))
            {
                playerWin = player;
                Console.WriteLine(playerWin + ": win");
                SendMessage(socket, msgsDictionary[StateMsgs.YouWin]);
            }
        }

        private static bool checkWinner(int lastPos, char player)
        {
            //check end conditions
            const int boardSize = 3;
            char[,] boardTemp = board.GetBoard();
            int row = (lastPos -1) / boardSize;
            int col = (lastPos -1) % boardSize;
            //check col
            for (int i = 0; i < boardSize; i++)
            {
                if (boardTemp[row,i] != player)
                    break;
                if (i == boardSize - 1)
                {
                    return true;
                }
            }

            //check row
            for (int i = 0; i < boardSize; i++)
            {
                if (boardTemp[i, col] != player)
                    break;
                if (i == boardSize - 1)
                {
                    return true;
                }
            }

            //check diag
            if (lastPos / boardSize == (lastPos % boardSize - 1))
            {
                //we're on a diagonal
                for (int i = 0; i < boardSize; i++)
                {
                    if (boardTemp[i,i] != player)
                        break;
                    if (i == boardSize - 1)
                    {
                        return true;
                    }
                }
            }

            //check anti diag (thanks rampion)
            if (row + col == boardSize - 1)
            {
                for (int i = 0; i < boardSize; i++)
                {
                    if (boardTemp[i,(boardSize - 1) - i] != player)
                        break;
                    if (i == boardSize - 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
