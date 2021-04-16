using System;

namespace TicTacToeServer
{
    /// <summary>Sent from client to server.</summary>
    public enum ClientPackets
    {
        WelcomeReceived = 1,
        RegisterAsPlayer = 2,
        PlayInSlot = 3,
        RequestGameStatusUpdate = 4
    }


    public class ServerHandle
    {
        

        public static void InitializeHandlers()
        {
            Server.PacketHandlers.Clear();
            Server.PacketHandlers.Add((int) ClientPackets.WelcomeReceived, ServerHandle.WelcomeReceived);
            Server.PacketHandlers.Add((int) ClientPackets.RegisterAsPlayer, ServerHandle.RegisterAsPlayer);
            Server.PacketHandlers.Add((int) ClientPackets.PlayInSlot, ServerHandle.PlayInSlot);
            Server.PacketHandlers.Add((int) ClientPackets.RequestGameStatusUpdate, ServerHandle.RequestGameStatusUpdate);
            
            Client.DisconnectedClientHandlerListener = ClientDisconnectedFromServer;

            Console.WriteLine($"Initialized all packet Handlers");
        }

        private static void ClientDisconnectedFromServer(int client)
        {
            Console.WriteLine($"Player has disconnected from server {client}");
            GameLogic.DisconnectedPlayer(client);
        }

        private static void RegisterAsPlayer(int fromClient, Packet packet)
        {
            
            int clientIdCheck = packet.ReadInt();
            if(fromClient != clientIdCheck)
            {
                Console.WriteLine($"Client (ID: {fromClient} has assumed the wrong client ID ({clientIdCheck})!)");
                Console.WriteLine($"Disconnecting client {fromClient}...");
                
                Server.Clients[fromClient].Disconnect();
                return;
            }

            Console.WriteLine($"Registering client {fromClient} as player");
            
            GameLogic.RegisterClientAsPlayer(fromClient);
        }

        public static void WelcomeReceived(int fromClient, Packet packet)
        {
            string msg = packet.ReadString();
            Console.WriteLine(msg);
            int clientIdCheck = packet.ReadInt();

            Console.WriteLine($"{Server.Clients[fromClient].Tcp.Socket.Client.RemoteEndPoint} connected successfully and is now client {fromClient}");
            if(fromClient != clientIdCheck)
            {
                Console.WriteLine($"Client (ID: {fromClient} has assumed the wrong client ID ({clientIdCheck})!)");
                Console.WriteLine($"Disconnecting client {fromClient}...");
                
                Server.Clients[fromClient].Disconnect();
            }
        }

        internal static void PlayInSlot(int fromClient, Packet packet)
        {  
            int clientId = packet.ReadInt();
            int slotIndex = packet.ReadInt();
            
            Console.WriteLine($"Player {fromClient} playin is Slot {slotIndex}");

            GameLogic.PlayerPlay(fromClient, slotIndex);
        }
        
        
        private static void RequestGameStatusUpdate(int fromClient, Packet packet)
        {
            int clientId = packet.ReadInt();
            
            ServerSend.SendStatus(fromClient);
        }

    }
}