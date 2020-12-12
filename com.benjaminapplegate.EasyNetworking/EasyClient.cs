using System;
using System.Net.Sockets;
using System.Threading;

namespace com.benjaminapplegate.EasyNetworking
{
    public class EasyClient
    {
        private TcpClient Connection = null;
        private bool serverClosed = false;

        private int dataBufferSize = 1024;

        private string ip;
        private int port;

        private Thread receiveThread;

        private bool safeToClose = true;

        public EasyPacket Packet;

        public delegate void ConnectionCallback();

        public ConnectionCallback successfulConnection = null;
        public ConnectionCallback failedConnection = null;
        
        public EasyClient(string IP, int PORT, EasyPacket packetObject)
        {
            Packet = packetObject;
            ip = IP;
            port = PORT;
            Connection = new TcpClient()
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };
        }

        public bool isConnected()
        {
            return Connection.Connected;
        }
        
        public void ConnectToServer()
        {
            try
            {
                Connection.Connect(ip, port);
                receiveThread = new Thread(new ThreadStart(ReceiveData));
                receiveThread.Start();

                successfulConnection?.Invoke();

            }
            catch (Exception e)
            {
                failedConnection?.Invoke();
            }
            
        }

        private void ReceiveData()
        {
            NetworkStream stream = Connection.GetStream();
                while (true)
                {
                    if (serverClosed) return;
                    if (stream.DataAvailable)
                    {
                        try
                        {
                            byte[] bytes = new byte[Connection.Available];
                            int bytesRead = stream.Read(bytes, 0, bytes.Length);
                            try
                            {
                                Packet.handleDataFromServer(bytes);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"ERROR handling data: {e}");
                            }
                        }
                        catch (Exception e)
                        {
                            if (!serverClosed)
                            {
                                Console.WriteLine($"Error receiving data: {e}");
                            }
                        }
                        
                        
                    }
                }

        }
        
        private void SendData(IAsyncResult ar)
        {
            NetworkStream stream = (NetworkStream) ar.AsyncState;
            stream.EndWrite(ar);
            safeToClose = true;
        }
        
        public void SendPacketToServer(byte[] data)
        {
            safeToClose = false;
            Connection.GetStream().BeginWrite(data, 0, data.Length, new AsyncCallback(SendData),
                Connection.GetStream());
        }

        public void Disconnect()
        {
            while (!safeToClose) ;
            serverClosed = true;
            Connection.Close();
        }
    }
}