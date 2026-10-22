using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace MyGame
{
    /// <summary>
    /// 局域网 TCP。一帧里只处理已经到达的数据，包格式是 4 字节长度加 1 字节类型加正文。
    /// </summary>
    public sealed class NetMgr : Module
    {
        public const byte Hello = 1;
        public const byte Leave = 2;
        public const byte Move = 3;

        private const int HeaderSize = 5;

        private sealed class Peer
        {
            public TcpClient Client;
            public readonly List<byte> Inbox = new List<byte>(256);
        }

        private TcpListener mListener;
        private Peer mClient;
        private readonly List<Peer> mPeers = new List<Peer>();
        private readonly byte[] mReadBuffer = new byte[1024];

        public bool IsServer { get; private set; }

        public void StartServer(int port)
        {
            Stop();
            mListener = new TcpListener(IPAddress.Any, port);
            mListener.Start();
            IsServer = true;
        }

        public void Connect(string host, int port)
        {
            Stop();
            var client = new TcpClient();
            client.Connect(host, port);
            mClient = new Peer { Client = client };
            IsServer = false;
        }

        public void Send(byte type, byte[] body)
        {
            var bodyLength = body == null ? 0 : body.Length;
            var packet = new byte[HeaderSize + bodyLength];
            var length = bodyLength + 1;
            packet[0] = (byte)length;
            packet[1] = (byte)(length >> 8);
            packet[2] = (byte)(length >> 16);
            packet[3] = (byte)(length >> 24);
            packet[4] = type;
            if (bodyLength > 0)
            {
                Buffer.BlockCopy(body, 0, packet, HeaderSize, bodyLength);
            }

            if (IsServer)
            {
                for (var i = mPeers.Count - 1; i >= 0; i--)
                {
                    Write(mPeers[i].Client, packet);
                }
            }
            else if (mClient != null)
            {
                Write(mClient.Client, packet);
            }
        }

        protected override void OnUpdate(int deltaMs)
        {
            if (mListener != null && mListener.Pending())
            {
                mPeers.Add(new Peer { Client = mListener.AcceptTcpClient() });
            }

            if (IsServer)
            {
                for (var i = mPeers.Count - 1; i >= 0; i--)
                {
                    if (!ReadAvailable(mPeers[i]))
                    {
                        mPeers[i].Client.Close();
                        mPeers.RemoveAt(i);
                    }
                }
            }
            else if (mClient != null && !ReadAvailable(mClient))
            {
                mClient.Client.Close();
                mClient = null;
            }
        }

        protected override void OnDestroy()
        {
            Stop();
        }

        private void Stop()
        {
            for (var i = 0; i < mPeers.Count; i++)
            {
                mPeers[i].Client.Close();
            }

            mPeers.Clear();
            if (mClient != null)
            {
                mClient.Client.Close();
                mClient = null;
            }

            if (mListener != null)
            {
                mListener.Stop();
                mListener = null;
            }

            IsServer = false;
        }

        private static void Write(TcpClient client, byte[] packet)
        {
            if (client == null || !client.Connected)
            {
                return;
            }

            client.GetStream().Write(packet, 0, packet.Length);
        }

        private bool ReadAvailable(Peer peer)
        {
            var client = peer.Client;
            if (client == null || !client.Connected)
            {
                return false;
            }

            var stream = client.GetStream();
            while (stream.DataAvailable)
            {
                var count = stream.Read(mReadBuffer, 0, mReadBuffer.Length);
                if (count <= 0)
                {
                    return false;
                }

                for (var i = 0; i < count; i++)
                {
                    peer.Inbox.Add(mReadBuffer[i]);
                }
            }

            while (peer.Inbox.Count >= HeaderSize)
            {
                var length = peer.Inbox[0] | (peer.Inbox[1] << 8) | (peer.Inbox[2] << 16) | (peer.Inbox[3] << 24);
                if (length < 1 || peer.Inbox.Count < 4 + length)
                {
                    break;
                }

                var type = peer.Inbox[4];
                var body = new byte[length - 1];
                for (var i = 0; i < body.Length; i++)
                {
                    body[i] = peer.Inbox[5 + i];
                }

                peer.Inbox.RemoveRange(0, 4 + length);
                App.Events.Emit(new NetPacket { Type = type, Body = body });
            }

            return true;
        }
    }

    public struct NetPacket
    {
        public byte Type;
        public byte[] Body;
    }
}
