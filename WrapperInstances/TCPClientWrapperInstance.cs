using System.IO;
using System.Net;
using System.Net.Sockets;

using Aragas.Core.Wrappers;

namespace PokeD.Server.Android.WrapperInstances
{
    public class SocketTCPClient: ITCPClient
    {
        public int RefreshConnectionInfoTime { get; set; }

        public string IP => !IsDisposed && Client != null && Client.Connected ? (Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() : "";
        public ushort Port => (ushort)(!IsDisposed && Client != null && Client.Connected ? (Client.RemoteEndPoint as IPEndPoint)?.Port : 0);
        public bool Connected => !IsDisposed && Client != null && Client.Connected;
        public int DataAvailable => !IsDisposed && Client != null ? Client.Available : 0;

        private Socket Client { get; }
        private Stream Stream { get; set; }

        private bool IsDisposed { get; set; }


        public SocketTCPClient()
        {
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        }
        internal SocketTCPClient(Socket socket)
        {
            Client = socket;
            Stream = new NetworkStream(Client);
        }
        
        public ITCPClient Connect(string ip, ushort port)
        {
            if (Connected)
                Disconnect();

            Client.Connect(ip, port);
            Stream = new NetworkStream(Client);

            return this;
        }
        public ITCPClient Disconnect()
        {
            if (Connected)
                Client.Disconnect(false);

            return this;
        }

        public void WriteByteArray(byte[] array)
        {
            if (IsDisposed)
                return;

            try
            {
                var length = array.Length;

                var bytesSend = 0;
                while (bytesSend < length)
                    bytesSend += Client.Send(array, bytesSend, length - bytesSend, 0);
            }
            catch (IOException) { Dispose(); }
            catch (SocketException) { Dispose(); }
        }
        public byte[] ReadByteArray(int length)
        {
            if (IsDisposed)
                return new byte[0];

            try
            {
                var array = new byte[length];

                var bytesReceive = 0;
                while (bytesReceive < length)
                    bytesReceive += Client.Receive(array, bytesReceive, length - bytesReceive, 0);

                return array;
            }
            catch (IOException) { Dispose(); return new byte[0]; }
            catch (SocketException) { Dispose(); return new byte[0]; }
        }

        public Stream GetStream() { return Stream; }

        public void Dispose()
        {
            if (Connected)
                Disconnect();

            IsDisposed = true;

            Client?.Dispose();
            Stream?.Dispose();
        }
    }

    public class TCPClientFactoryInstance : ITCPClientFactory
    {
        public ITCPClient CreateTCPClient() => new SocketTCPClient();
        internal static ITCPClient CreateTCPClient(Socket socket) => new SocketTCPClient(socket);
    }
}
