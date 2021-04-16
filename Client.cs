using System;
using System.Net.Sockets;

namespace TicTacToeServer
{

    class Client 
    {
        public delegate void DisconnectedClientHandler(int client);
        public static DisconnectedClientHandler DisconnectedClientHandlerListener;
        public static int DataBufferSize = 4096;
        public int Id;
        public TCP Tcp;
        
        public Client(int id)
        {
            Id = id;
            Tcp = new TCP(id);
        }

        public void Disconnect()
        {
            Console.WriteLine($"{Tcp.Socket.Client.RemoteEndPoint} has disconnected");
            Tcp.Disconnect();
            DisconnectedClientHandlerListener?.Invoke(Id);
        }

        public class TCP 
        {
            public TcpClient Socket;

            private Packet _receivedData;
            private NetworkStream _stream;
            private byte[] _receiveBuffer;
            private readonly int _id;
            public TCP(int id)
            {
                _id = id;
            }

            public void Connect(TcpClient socket)
            {
                Socket = socket;
                Socket.ReceiveBufferSize = DataBufferSize;
                Socket.SendBufferSize = DataBufferSize;

                _stream = socket.GetStream();

                _receivedData = new Packet();
                _receiveBuffer = new byte[DataBufferSize];

                _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(_id, "Welcome to the server!");
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if(packet == null)
                        return;
                    
                    _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Error sending data to player {_id} via TCP: {ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLenght = _stream.EndRead(result);
                    if(byteLenght <= 0 )
                    {
                        Server.Clients[_id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLenght];
                    Array.Copy(_receiveBuffer, data, byteLenght);

                    _receivedData.Reset(HandleData(data));
                    _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Error receiving TCP data: {ex}");
                    Server.Clients[_id].Disconnect();
                }
            }

            private bool HandleData(byte[] data)
            {
                int packetLenght = 0;

                _receivedData.SetBytes(data);

                if(_receivedData.UnreadLength() >= 4)
                {
                    packetLenght = _receivedData.ReadInt();
                    if(packetLenght <= 0)
                    {
                        return true;
                    }
                }

                while(packetLenght> 0 && packetLenght <= _receivedData.UnreadLength())
                {
                    byte[] packetBytes = _receivedData.ReadBytes(packetLenght);
                    ThreadManager.ExecuteOnMainThread(()=>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            Server.PacketHandlers[packetId](_id, packet);
                        }
                    });

                }
                packetLenght = 0;
                if(_receivedData.UnreadLength() >= 4)
                {
                    packetLenght = _receivedData.ReadInt();
                    if(packetLenght <= 0)
                    {
                        return true;
                    }
                }

                return packetLenght <= 1;
            }

            public void Disconnect()
            {
                Socket.Close();
                _stream = null;
                _receivedData = null;
                _receiveBuffer = null;
                Socket = null;
            }   

        }

    }
    
}