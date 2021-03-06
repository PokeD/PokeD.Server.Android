﻿using System.Net;
using System.Net.Sockets;

using Aragas.Core.Wrappers;

namespace PokeD.Server.Android.WrapperInstances
{
    public class SocketTCPListener : ITCPListener
    {
        public ushort Port { get; }
        public bool AvailableClients => Listener.Poll(0, SelectMode.SelectRead);

        private Socket Listener { get; }

        private bool IsDisposed { get; set; }


        internal SocketTCPListener(ushort port)
        {
            Port = port;

            var endpoint = new IPEndPoint(IPAddress.Any, Port);
            Listener = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };

            Listener.Bind(endpoint);
        }

        public void Start()
        {
            if (IsDisposed)
                return;

            Listener.Listen(1000);
        }
        public void Stop()
        {
            if (IsDisposed)
                return;

            Listener.Close();
        }

        public ITCPClient AcceptTCPClient()
        {
            if (IsDisposed)
                return null;

            return TCPClientFactoryInstance.CreateTCPClient(Listener.Accept());
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            Listener?.Dispose();
        }
    }

    public class TCPServerWrapperInstance : ITCPListenerWrapper
    {  
        public ITCPListener CreateTCPListener(ushort port) { return new SocketTCPListener (port); }
    }
}
