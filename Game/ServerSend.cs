using System;

namespace TicTacToeServer
{
    /// <summary>Sent from server to client.</summary>
    public enum ServerPackets
    {
        Welcome = 1,
        RegisteredAsPlayer = 2,
        RegisterAsPlayerError = 3,
        SendStatus = 4,
        SendStatusForAll = 5

    }
    public class ServerSend
    {

        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server.Clients[toClient].Tcp.SendData(packet);
        }
        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();

            for (int i = 1; i <= Server.MaxClients; i++)
            {
                if(Server.Clients[i].Tcp.Socket == null) continue;
                Server.Clients[i].Tcp?.SendData(packet);
            }
        }
        private static void SendTCPDataToAllExcept(int exceptClient, Packet packet)
        {
            packet.WriteLength();

            for (int i = 1; i <= Server.MaxClients; i++)
            {
                if(Server.Clients[i].Tcp.Socket == null) continue; 
                if(i == exceptClient) continue;
                Server.Clients[i].Tcp?.SendData(packet);
            }
        }
        public static void Welcome(int toClient, string msg)
        {
            ServerHandle.InitializeHandlers();
            using (Packet packet = new Packet((int)ServerPackets.Welcome))
            {
                packet.Write(msg);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }    
        }

        internal static void SendStatus(int toClient)
        {
            const int packetId = (int)ServerPackets.SendStatus;
            Console.WriteLine($"Sending packet with id {packetId}");

            using (Packet packet = new Packet(packetId))
            {
                packet.Write(toClient);
                packet.Write((int) GameLogic.Status);
                for (int i = 0; i < 9; i++)
                {
                    if(GameLogic.Slots == null)
                        packet.Write(0);
                    else
                        packet.Write(GameLogic.Slots[i]);
                }
                SendTCPData(toClient, packet);
            }    
        } 
        internal static void SendStatusForAll()
        {
            const int packetId = (int)ServerPackets.SendStatusForAll;
            Console.WriteLine($"Sending packet with id {packetId}");
            using (Packet packet = new Packet(packetId))
            {
                packet.Write((int) GameLogic.Status);
                for (int i = 0; i < 9; i++)
                {
                    if(GameLogic.Slots == null)
                        packet.Write(0);
                    else
                        packet.Write(GameLogic.Slots[i]);
                }
                SendTCPDataToAll(packet);
            }    
        }

        internal static void RegisteredAsPlayer(int toClient)
        {
            const int packetId = (int)ServerPackets.RegisteredAsPlayer;
            Console.WriteLine($"Sending packet with id {packetId}");

            using (Packet packet = new Packet(packetId))
            {
                packet.Write(toClient);
                packet.Write(GameLogic.Players[toClient].PlayerId);

                SendTCPData(toClient, packet);
            }    
        }

        internal static void RegisterAsPlayerError(int toClient)
        {
            const int packetId = (int)ServerPackets.RegisterAsPlayerError;
            Console.WriteLine($"Sending packet with id {packetId}");
            using (Packet packet = new Packet(packetId))
            {
                packet.Write(toClient);
                packet.Write("already_has_2_players");

                SendTCPData(toClient, packet);
            }    
            
        }

    }
}