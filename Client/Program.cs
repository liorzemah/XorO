using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Client
{
    class Program
    {
        enum StateMsgs
        {
            StartGame = 1,
            WaitForPlayer = 2,
            WaitForTwoPlayers = 3,
            SelectPosition = 4,
            SendBoard = 5,
            PositionAlreadyExit = 6,
            YouLose = 7,
            YouWin = 8,
        }

        static Dictionary<string, StateMsgs> msgsDictionary = new Dictionary<string, StateMsgs>();

        static void Main(string[] args)
        {
            msgsDictionary.Add("Start Game", StateMsgs.StartGame);
            msgsDictionary.Add("Wait for player", StateMsgs.WaitForPlayer);
            msgsDictionary.Add("wait for two players", StateMsgs.WaitForTwoPlayers);
            msgsDictionary.Add("Select position (1-9): ", StateMsgs.SelectPosition);
            //msgsDictionary.Add(StateMsgs.SendBoard, "");
            msgsDictionary.Add("Position already catch", StateMsgs.PositionAlreadyExit);
            msgsDictionary.Add("You lose!", StateMsgs.YouLose);
            msgsDictionary.Add("You win!", StateMsgs.YouWin);

            Socket socket = new Socket(AddressFamily.InterNetwork,
                             SocketType.Stream, ProtocolType.Tcp);
            try
            {

                IPAddress hostAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint hostEndPoint = new IPEndPoint(hostAddress, 7);
                Console.WriteLine("Connecting.....");
                socket.Connect(hostEndPoint);

                if (socket.Connected) Console.WriteLine("connected");
                if (DoAuthenticate(socket))
                    MakeSessionWithServer(socket);
                else
                    socket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Eror....." + e.StackTrace);
            }
            Console.ReadLine();
        }

        private static bool DoAuthenticate(Socket socket)
        {
            ASCIIEncoding asciiEnc = new ASCIIEncoding();
            string serverNotify = EnterAndCheckUsername(socket, asciiEnc);
            if (serverNotify.Equals("1"))
            {
                serverNotify = EnterAndCheckPassword(socket, asciiEnc);
                if (serverNotify.Equals("1"))
                {
                    Console.WriteLine("Authenticate successfully!!");
                    return true;
                }
                else
                {
                    Console.WriteLine("password not match!!");
                    return false;

                }
            }

            else
            {
                Console.WriteLine("user not exist!!");
            }
            return false;

        }

        private static string EnterAndCheckUsername(Socket socket, ASCIIEncoding asciiEnc)
        {
            Console.WriteLine("Enter username:");
            string clientUsername = Console.ReadLine();
            byte[] binDataOut = asciiEnc.GetBytes(clientUsername);
            socket.Send(binDataOut, 0, binDataOut.Length, SocketFlags.None);
            byte[] binDataIn = new byte[255];
            int k = socket.Receive(binDataIn, 0, 255, SocketFlags.None);
            string serverNotify = asciiEnc.GetString(binDataIn, 0, k);
            return serverNotify;
        }

        private static string EnterAndCheckPassword(Socket socket, ASCIIEncoding asciiEnc)
        {
            Console.WriteLine("Enter password:");
            string clientpassword = Console.ReadLine();
            byte[] binDataOut = asciiEnc.GetBytes(clientpassword);
            socket.Send(binDataOut, 0, binDataOut.Length, SocketFlags.None);
            byte[] binDataIn = new byte[255];
            int k = socket.Receive(binDataIn, 0, 255, SocketFlags.None);
            string serverNotify = asciiEnc.GetString(binDataIn, 0, k);
            return serverNotify;
        }

        private static void SendMessage(Socket socket, String message)
        {
            byte[] sendMsgBytes = Encoding.ASCII.GetBytes(message);
            socket.Send(sendMsgBytes, 0, sendMsgBytes.Length, SocketFlags.None);
        }


        private static void MakeSessionWithServer(Socket socket)
        {
            
            ASCIIEncoding asciiEnc = new ASCIIEncoding();
            string msg;
            do
            {
                msg = RecvMsg(socket, asciiEnc);
                Console.WriteLine(msg);
            } while (msgsDictionary[msg] != StateMsgs.StartGame);

            while (true)
            {
                // Start Game:

                msg = RecvMsg(socket, asciiEnc);
                Console.WriteLine(msg);  // Print board or Get PositionAlreadyExit message or win/lose

                if (!msgsDictionary.ContainsKey(msg)) // It's board;
                {
                    msg = RecvMsg(socket, asciiEnc); // Select position message

                }

                if (msgsDictionary[msg] == StateMsgs.YouWin || msgsDictionary[msg] == StateMsgs.YouLose)
                {
                    Console.WriteLine("End Game");
                    socket.Close();
                    return;
                }

                bool getPos = false;
                do
                {
                    Console.WriteLine("Select position (1-9): ");
                    string input = Console.ReadLine();
                    Console.WriteLine("Selected position: " + input);
                    try
                    {
                        int pos = int.Parse(input);
                        if (pos < 1 || pos > 9)
                            throw new InvalidDataException();
                        SendMessage(socket, pos.ToString());
                        getPos = true;
                    }
                    catch
                    {
                        Console.WriteLine("Invalid argument please write number between 1-9");
                    }
                } while (!getPos);
            }

            socket.Close();
        }

        private static string RecvMsg(Socket socket, ASCIIEncoding asciiEnc)
        {
            string msg;
            byte[] binDataIn = new byte[255];
            int size = socket.Receive(binDataIn, 0, 255, SocketFlags.None);
            msg = asciiEnc.GetString(binDataIn, 0, size);
            return msg;
        }
    }
}