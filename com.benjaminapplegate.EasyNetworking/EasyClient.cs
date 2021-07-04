using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace com.benjaminapplegate.EasyNetworking
{
    public class EasyClient
    {
        private TcpClient Connection = null;
        

        private string _ip;
        private int _port;
        private bool _hasClosed = false;

        private bool _safeToClose = true;

        private byte[] _readBuffer;

        public delegate void ClientCallback();

        public ClientCallback SuccessfulClient = null;
        public ClientCallback FailedClient = null;
        
        public delegate void PacketHandler(Packet packet);

        private Dictionary<int, PacketHandler> _packetHandlers = new Dictionary<int, PacketHandler>();
        
        public EasyClient(string ip, int port)
        {
            _ip = ip;
            _port = port;
            Connection = new TcpClient();
        }

        public void AddPacketHandler(int packetType, PacketHandler handler)
        {
            _packetHandlers.Add(packetType, handler);
        }
        
        public bool IsConnected()
        {
            return Connection.Connected;
        }
        
        public void ConnectToServer()
        {
            try
            {
                Connection.Connect(_ip, _port);
                _readBuffer = new byte[1024];
                Connection.GetStream().BeginRead(_readBuffer, 0, 1024, ReceiveDataCallback, null);

                SuccessfulClient?.Invoke();

            }
            catch (Exception e)
            {
                FailedClient?.Invoke();
            }
            
        }

        private void ReceiveDataCallback(IAsyncResult result)
        {
            try
            {
                if (!Connection.Connected)
                {
                    Console.WriteLine("Server seems to have closed connection, Disconnecting");
                    Connection.Close();
                    return;
                }
                int bytesRead = Connection.GetStream().EndRead(result);
                if (bytesRead < 1)
                {
                    Console.WriteLine("Server seems to have closed connection, Disconnecting");
                    Connection.Close();
                    return;
                }
                byte[] data = new byte[bytesRead];
                Array.Copy(_readBuffer, data, bytesRead);
                Packet packet = new Packet(data);
                _packetHandlers[packet.ReadInt()]?.Invoke(packet);
                if(_hasClosed) return;
                Connection.GetStream().BeginRead(_readBuffer, 0, 1024, ReceiveDataCallback, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error getting data from the server, the server probably closed");
                Connection.Close();
            }

        }

        private void SendData(IAsyncResult ar)
        {
            NetworkStream stream = (NetworkStream) ar.AsyncState;
            stream.EndWrite(ar);
            _safeToClose = true;
        }
        
        public void SendPacketToServer(Packet packet)
        {
            _safeToClose = false;
            Connection.GetStream().BeginWrite(packet.GetBytes(), 0, packet.GetBytes().Length, SendData,
                Connection.GetStream());
        }

        public void Disconnect()
        {
            while (!_safeToClose);
            _hasClosed = true;
            Connection.Close();
        }
    }
}