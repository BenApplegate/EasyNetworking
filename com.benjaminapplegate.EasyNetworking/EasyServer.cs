using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace com.benjaminapplegate.EasyNetworking
{
    public class EasyServer
    {
        public Dictionary<int, TcpClient> ConnectedClients;
        private Dictionary<int, byte[]> _receiveBuffers = new Dictionary<int, byte[]>();
        private TcpListener _tcpListener;
        private int _maxClients;
        private int _port;

        public delegate void ServerCallback(TcpClient client, int id);

        public ServerCallback clientConnection = null;
        public ServerCallback serverFullConnection = null;

        public delegate void PacketHandler(int fromClient, Packet packet);

        private Dictionary<int, PacketHandler> _packetHandlers = new Dictionary<int, PacketHandler>();

        public EasyServer(int maxConnections, int port)
        {
            _port = port;
            _maxClients = maxConnections;
            ConnectedClients = new Dictionary<int, TcpClient>();
            for (int i = 0; i < maxConnections; i++)
            {
                ConnectedClients.Add(i, null);
            }
        }

        public void AddPacketHandler(int packetType, PacketHandler handler)
        {
            _packetHandlers.Add(packetType, handler);
        }
        public void StartServer()
        {
            _tcpListener = new TcpListener(IPAddress.Any, _port);
            _tcpListener.Start(10);
            Console.WriteLine($"Started server on port {_port}");
            _tcpListener.BeginAcceptTcpClient(HandleConnection, _tcpListener);
            
        }

        private void HandleConnection(IAsyncResult ar)
        {
            try
            {
                TcpClient client = _tcpListener.EndAcceptTcpClient(ar);

                Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}");
                for (int i = 0; i < _maxClients; i++)
                {
                    if (ConnectedClients[i] == null)
                    {
                        ConnectedClients[i] = client;
                        _receiveBuffers[i] = new byte[1024];
                        
                        client.GetStream().BeginRead(_receiveBuffers[i], 0, 1024, ReceiveDataCallback, i);
                        clientConnection?.Invoke(client, i);
                        break;
                    }
                    else if (i == _maxClients - 1 && ConnectedClients[i] != null)
                    {
                        Console.WriteLine("Server is full, disconnecting new client");

                        serverFullConnection?.Invoke(client, -1);

                        client.Close();
                        return;
                    }
                }

                _tcpListener.BeginAcceptTcpClient(HandleConnection, _tcpListener);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error accepting connection: {e}");
            }

        }

        private void ReceiveDataCallback(IAsyncResult result)
        {
            try
            {
                if (ConnectedClients[(int) result.AsyncState] == null) return;
                int bytesRead = ConnectedClients[(int)result.AsyncState].GetStream().EndRead(result);
                if (bytesRead < 1)
                {
                    Console.WriteLine("Client seems to have stopped communicating, closing connection");
                    ConnectedClients[(int)result.AsyncState].Close();
                    return;
                }
                
                byte[] data = new byte[bytesRead];
                Array.Copy(_receiveBuffers[(int)result.AsyncState], data, bytesRead);
                Packet packet = new Packet(data);
                _packetHandlers[packet.ReadInt()]?.Invoke((int) result.AsyncState, packet);
                if (ConnectedClients[(int) result.AsyncState] == null) return;
                ConnectedClients[(int) result.AsyncState].GetStream().BeginRead(_receiveBuffers[(int) result.AsyncState], 0, 1024, ReceiveDataCallback, result.AsyncState);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Getting Data: " + e);
            }
        }
        
        private void SendData(IAsyncResult ar)
        {
            NetworkStream stream = (NetworkStream) ar.AsyncState;
            stream.EndWrite(ar);
        }

        public void SendPacketToId(Packet packet, int id)
        {
            ConnectedClients[id].GetStream().BeginWrite(packet.GetBytes(), 0, packet.GetBytes().Length, SendData,
                ConnectedClients[id].GetStream());
        }
        
        public void SendPacketToTcpClient(Packet packet, TcpClient client)
        {
            client.GetStream().BeginWrite(packet.GetBytes(), 0, packet.GetBytes().Length, SendData,
                client.GetStream());
            
        }


        public void SendPacketToAll(Packet packet)
        {
            for (int i = 0; i < _maxClients; i++)
            {
                if (ConnectedClients[i] != null)
                {
                    TcpClient client = ConnectedClients[i];
                    client.GetStream().BeginWrite(packet.GetBytes(), 0, packet.GetBytes().Length, SendData,
                        client.GetStream());
                }
            }
        }
        
        public void SendPacketToAllButOne(Packet packet, int id)
        {
            for (int i = 0; i < _maxClients; i++)
            {
                if (ConnectedClients[i] != null && i != id)
                {
                    TcpClient client = ConnectedClients[i];
                    client.GetStream().BeginWrite(packet.GetBytes(), 0, packet.GetBytes().Length, SendData,
                        client.GetStream());
                }
            }
        }

        public void Stop()
        {
            _tcpListener.Stop();
        }
    }
}