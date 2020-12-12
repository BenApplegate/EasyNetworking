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
        private TcpListener listener;
        private int maxClients;
        private int port;
        private int dataBufferSize = 1024;

        private bool shouldClose = false;
        
        private List<Thread> Threads = new List<Thread>();

        public delegate void EasyNetworkingCallback(TcpClient client, int ID);

        public EasyNetworkingCallback clientConnection = null;
        public EasyNetworkingCallback serverFullConnection = null;

        public EasyPacket Packet;


        public EasyServer(int maxConnections, int PORT, EasyPacket packetObject)
        {
            Packet = packetObject;
            port = PORT;
            maxClients = maxConnections;
            ConnectedClients = new Dictionary<int, TcpClient>();
            for (int i = 0; i < maxConnections; i++)
            {
                ConnectedClients.Add(i, null);
            }
        }

        public void StartServer()
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start(10);
            Console.WriteLine($"Started server on port {port}");
            listener.BeginAcceptTcpClient(new AsyncCallback(HandleConnection), listener);
        }

        private void HandleConnection(IAsyncResult ar)
        {
            try
            {
                TcpClient client = listener.EndAcceptTcpClient(ar);

                Console.WriteLine($"Incomming connection from {client.Client.RemoteEndPoint}");
                for (int i = 0; i < maxClients; i++)
                {
                    if (ConnectedClients[i] == null)
                    {
                        ConnectedClients[i] = client;
                        Thread clientThread = new Thread(() => ReceiveData(i, client));

                        clientConnection?.Invoke(ConnectedClients[i], i);

                        Threads.Add(clientThread);
                        clientThread.Start();

                        break;
                    }
                    else if (i == maxClients - 1 && ConnectedClients[i] != null)
                    {
                        Console.WriteLine("Server is full, disconnecting new client");

                        serverFullConnection?.Invoke(client, -1);

                        client.Close();
                        return;
                    }
                }

                listener.BeginAcceptTcpClient(new AsyncCallback(HandleConnection), listener);
            }
            catch (Exception e)
            {
                
            }

        }

        private void SendData(IAsyncResult ar)
        {
            NetworkStream stream = (NetworkStream) ar.AsyncState;
            stream.EndWrite(ar);
        }

        public void SendPacketToId(byte[] data, int id)
        {
            ConnectedClients[id].GetStream().BeginWrite(data, 0, data.Length, new AsyncCallback(SendData),
                ConnectedClients[id].GetStream());
        }
        
        public void SendPacketToTCP(byte[] data, TcpClient client)
        {
            client.GetStream().BeginWrite(data, 0, data.Length, new AsyncCallback(SendData),
                client.GetStream());
            
        }


        public void SendPacketToAll(byte[] data)
        {
            for (int i = 0; i < maxClients; i++)
            {
                if (ConnectedClients[i] != null)
                {
                    TcpClient client = ConnectedClients[i];
                    client.GetStream().BeginWrite(data, 0, data.Length, new AsyncCallback(SendData),
                        client.GetStream());
                }
            }
        }
        
        public void SendPacketToAllbutOne(byte[] data, int id)
        {
            for (int i = 0; i < maxClients; i++)
            {
                if (ConnectedClients[i] != null && i != id)
                {
                    TcpClient client = ConnectedClients[i];
                    client.GetStream().BeginWrite(data, 0, data.Length, new AsyncCallback(SendData),
                        client.GetStream());
                }
            }
        }
        
        private void ReceiveData(int id, TcpClient client)
        {
            
            NetworkStream stream = ConnectedClients[id].GetStream();
            while (true)
            {
                if(shouldClose) return;
                if (stream.DataAvailable)
                {
                    byte[] bytes = new byte[client.Available];
                    int bytesRead = stream.Read(bytes, 0, bytes.Length);

                    if(shouldClose) return;

                    try
                    {
                        Packet.handleDataFromClient(bytes, id);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"ERROR: {e}");
                    }
                    
                    
                }
            }
        }

        private void JoinThreads()
        {
            for (int i = 0; i < Threads.Count; i++)
            {
                Threads[i].Join();
            }
        }

        public void Stop()
        {
            shouldClose = true;
            JoinThreads();
            listener.Stop();
        }
    }
}