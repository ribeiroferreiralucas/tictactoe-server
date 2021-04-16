using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace TicTacToeServer 
{
    static class Server 
    {
        public static int MaxClients {get ; private set;}
        public static int Port {get ; private set;}
        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int fromClient, Packet packet);
        public delegate void DisconnectedClientHandler(int client);
        public static readonly Dictionary<int, PacketHandler> PacketHandlers = new Dictionary<int, PacketHandler>();
        public static DisconnectedClientHandler DisconnectedClientHandlerListener;
        private static TcpListener _tcpListener;

        public static void Start(int maxPlayers, int port)
        {
            MaxClients = maxPlayers;
            Port = port;
            Console.WriteLine($"Starting server...");
            InitializeServerData();
            
            _tcpListener = new TcpListener(IPAddress.Any, Port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Server stated on {port}.");
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxClients; i++)
            {
                if (Clients[i].Tcp.Socket != null)
                    continue;
                
                Clients[i].Tcp.Connect(client);
                return;
            }

            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server full");

        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxClients; i++)
            {
                Clients.Add(i, new Client(i));
            }

            Console.WriteLine("Initialized packets.");
        }

    }
}