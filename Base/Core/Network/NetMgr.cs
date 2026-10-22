//---------------------------------------------------------------------------------------
// Copyright (c) WithMe8 2023-2030
// Author: rongaihua
// Date: 2026-10-22
// Desc: 局域网 TCP。一帧只处理已到达的数据
//---------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MyGame
{

    /// <summary>
    /// 一帧里读到的一个完整包。
    /// </summary>
    public struct NetPacket
    {

        /// <summary>
        /// 来源连接 id。本机作为客户端时为 0。
        /// </summary>
        public int PeerId;

        /// <summary>
        /// 包类型。
        /// </summary>
        public byte Type;

        /// <summary>
        /// 去掉长度和类型之后的正文。
        /// </summary>
        public byte[] Body;
    }

    /// <summary>
    /// 客户端连上房主。
    /// </summary>
    public struct NetConnected
    {
    }

    /// <summary>
    /// 连接失败或断开。
    /// </summary>
    public struct NetFailed
    {

        /// <summary>
        /// 失败原因。
        /// </summary>
        public string Reason;
    }

    /// <summary>
    /// 房主侧一条连接关闭。
    /// </summary>
    public struct NetPeerClosed
    {

        /// <summary>
        /// 关闭的连接 id。
        /// </summary>
        public int PeerId;
    }

    /// <summary>
    /// 局域网 TCP。一帧里只处理已经到达的数据，包格式是 4 字节长度加 1 字节类型加正文。
    /// 写出失败的连接先标记，等这一轮收发结束再关闭，避免处理消息时把连接列表拆掉。
    /// </summary>
    public sealed class NetMgr : Module
    {

        /// <summary>
        /// 加入房间。
        /// </summary>
        public const byte Hello = 1;

        /// <summary>
        /// 离开房间。
        /// </summary>
        public const byte Leave = 2;

        /// <summary>
        /// 位置同步。
        /// </summary>
        public const byte Move = 3;

        /// <summary>
        /// 长度 4 字节加类型 1 字节。
        /// </summary>
        private const int HeaderSize = 5;

        /// <summary>
        /// 长度字段允许的最大值。
        /// </summary>
        private const int MaxPacket = 256;

        /// <summary>
        /// 一条 TCP 连接和它还没切完的字节。
        /// </summary>
        private sealed class Peer
        {

            /// <summary>
            /// 连接 id。
            /// </summary>
            public int Id;

            /// <summary>
            /// 本轮收发结束后再关闭。
            /// </summary>
            public bool Dead;

            /// <summary>
            /// 底层连接。
            /// </summary>
            public TcpClient Client;

            /// <summary>
            /// 尚未切出完整包的字节。
            /// </summary>
            public readonly List<byte> Inbox = new List<byte>(256);
        }

        /// <summary>
        /// 房主监听。
        /// </summary>
        private TcpListener mListener;

        /// <summary>
        /// 本机作为客户端时的连接。
        /// </summary>
        private Peer mClient;

        /// <summary>
        /// 正在进行的连接任务。
        /// </summary>
        private Task mConnectTask;

        /// <summary>
        /// 发起连接时的会话号，用来丢掉已经 Stop 的结果。
        /// </summary>
        private int mConnectSession;

        /// <summary>
        /// 每次 Stop 加一，使旧连接的完成回调失效。
        /// </summary>
        private int mSession;

        /// <summary>
        /// 下一个连接 id。
        /// </summary>
        private int mNextPeerId = 1;

        /// <summary>
        /// 正在 OnUpdate 里收包的层数。为 0 才能立刻回收死连接。
        /// </summary>
        private int mUpdating;

        /// <summary>
        /// 正在回收死连接的层数，防止重入。
        /// </summary>
        private int mCollecting;

        /// <summary>
        /// 正在执行 Stop，避免重入。
        /// </summary>
        private bool mInStop;

        /// <summary>
        /// 房主已接受的连接。
        /// </summary>
        private readonly List<Peer> mPeers = new List<Peer>();

        /// <summary>
        /// 一次 Read 的缓冲。
        /// </summary>
        private readonly byte[] mReadBuffer = new byte[1024];

        /// <summary>
        /// 组包缓冲，不够时加长。
        /// </summary>
        private byte[] mSendBuffer = new byte[256];

        /// <summary>
        /// 当前是房主监听，而不是客户端。
        /// </summary>
        public bool IsServer { get; private set; }

        /// <summary>
        /// 停掉旧连接并开始监听。
        /// </summary>
        /// <param name="port">监听端口。</param>
        public void StartServer(int port)
        {
            Stop();
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            mListener = listener;
            IsServer = true;
        }

        /// <summary>
        /// 停掉旧连接并异步连向房主。结果在后续 Update 里派发。
        /// </summary>
        /// <param name="host">房主地址。</param>
        /// <param name="port">端口。</param>
        public void Connect(string host, int port)
        {
            Stop();
            var client = new TcpClient();
            mConnectSession = mSession;
            mConnectTask = client.ConnectAsync(host, port);
            mClient = new Peer { Client = client };
            IsServer = false;
        }

        /// <summary>
        /// 把整段正文发给当前全部有效连接。
        /// </summary>
        /// <param name="type">包类型。</param>
        /// <param name="body">正文。可为 null。</param>
        public void Send(byte type, byte[] body)
        {
            Send(type, body, body == null ? 0 : body.Length);
        }

        /// <summary>
        /// 把正文的前一段发给当前全部有效连接。
        /// </summary>
        /// <param name="type">包类型。</param>
        /// <param name="body">正文。可为 null。</param>
        /// <param name="length">采用的字节数。</param>
        public void Send(byte type, byte[] body, int length)
        {
            var size = Pack(type, body, length);
            if (IsServer)
            {
                for (var i = 0; i < mPeers.Count; i++)
                {
                    var peer = mPeers[i];
                    if (peer.Dead)
                    {
                        continue;
                    }

                    if (!Write(peer.Client, mSendBuffer, size))
                    {
                        peer.Dead = true;
                    }
                }
            }
            else if (mClient != null && mConnectTask == null && !mClient.Dead)
            {
                if (!Write(mClient.Client, mSendBuffer, size))
                {
                    mClient.Dead = true;
                }
            }

            if (mUpdating == 0)
            {
                CollectDead();
            }
        }

        /// <summary>
        /// 只发给房主侧的一条连接。非房主时不发送。
        /// </summary>
        /// <param name="peerId">连接 id。</param>
        /// <param name="type">包类型。</param>
        /// <param name="body">正文。</param>
        /// <param name="length">采用的字节数。</param>
        public void SendTo(int peerId, byte type, byte[] body, int length)
        {
            if (!IsServer)
            {
                return;
            }

            var size = Pack(type, body, length);
            for (var i = 0; i < mPeers.Count; i++)
            {
                var peer = mPeers[i];
                if (peer.Id != peerId || peer.Dead)
                {
                    continue;
                }

                if (!Write(peer.Client, mSendBuffer, size))
                {
                    peer.Dead = true;
                }

                break;
            }

            if (mUpdating == 0)
            {
                CollectDead();
            }
        }

        /// <summary>
        /// 关掉监听和全部连接，并让进行中的连接结果作废。
        /// </summary>
        public void Stop()
        {
            if (mInStop)
            {
                return;
            }

            mInStop = true;
            mSession++;
            Observe(mConnectTask);
            mConnectTask = null;

            for (var i = 0; i < mPeers.Count; i++)
            {
                Close(mPeers[i].Client);
            }

            mPeers.Clear();
            if (mClient != null)
            {
                Close(mClient.Client);
                mClient = null;
            }

            if (mListener != null)
            {
                mListener.Stop();
                mListener = null;
            }

            IsServer = false;
            mInStop = false;
        }

        /// <summary>
        /// 处理连接完成、接受新连接，并读取已经到达的字节。
        /// </summary>
        /// <param name="deltaMs">距上一帧的毫秒数。本方法不使用它。</param>
        protected override void OnUpdate(int deltaMs)
        {
            mUpdating++;
            PumpConnect();
            AcceptPending();

            if (IsServer)
            {
                for (var i = mPeers.Count - 1; i >= 0; i--)
                {
                    if (!IsServer || i >= mPeers.Count)
                    {
                        break;
                    }

                    var peer = mPeers[i];
                    if (peer.Dead)
                    {
                        continue;
                    }

                    if (!ReadAvailable(peer))
                    {
                        peer.Dead = true;
                    }
                }
            }
            else if (mClient != null && mConnectTask == null && !mClient.Dead && !ReadAvailable(mClient))
            {
                mClient.Dead = true;
            }

            mUpdating--;
            if (mUpdating == 0)
            {
                CollectDead();
            }
        }

        /// <summary>
        /// 销毁时停掉网络。
        /// </summary>
        protected override void OnDestroy()
        {
            Stop();
        }

        /// <summary>
        /// 连接任务完成后派发成功或失败。会话已变则丢弃结果。
        /// </summary>
        private void PumpConnect()
        {
            if (mConnectTask == null || !mConnectTask.IsCompleted)
            {
                return;
            }

            var task = mConnectTask;
            var session = mConnectSession;
            mConnectTask = null;
            var error = task.Exception;
            if (session != mSession || mClient == null)
            {
                return;
            }

            if (error != null || !mClient.Client.Connected)
            {
                var reason = error != null ? error.GetBaseException().Message : "连接失败";
                Stop();
                App.Events.Emit(new NetFailed { Reason = reason });
                return;
            }

            mClient.Client.NoDelay = true;
            App.Events.Emit(new NetConnected());
        }

        /// <summary>
        /// 接受一个已经在监听队列里的连接。
        /// </summary>
        private void AcceptPending()
        {
            if (mListener == null || !mListener.Pending())
            {
                return;
            }

            var client = mListener.AcceptTcpClient();
            client.NoDelay = true;
            mPeers.Add(new Peer { Id = mNextPeerId++, Client = client });
        }

        /// <summary>
        /// 关闭已标记的连接并派发断开。处理消息期间不调用。
        /// </summary>
        private void CollectDead()
        {
            if (mCollecting > 0)
            {
                return;
            }

            mCollecting++;
            var guard = 0;
            while (guard++ < 8)
            {
                Peer dead = null;
                if (mClient != null && mClient.Dead)
                {
                    dead = mClient;
                    mClient = null;
                }
                else
                {
                    for (var i = mPeers.Count - 1; i >= 0; i--)
                    {
                        if (!mPeers[i].Dead)
                        {
                            continue;
                        }

                        dead = mPeers[i];
                        mPeers.RemoveAt(i);
                        break;
                    }
                }

                if (dead == null)
                {
                    break;
                }

                Close(dead.Client);
                if (IsServer)
                {
                    App.Events.Emit(new NetPeerClosed { PeerId = dead.Id });
                }
                else
                {
                    App.Events.Emit(new NetFailed { Reason = "连接断开" });
                }
            }

            mCollecting--;
        }

        /// <summary>
        /// 把类型和正文写入发送缓冲。
        /// </summary>
        /// <param name="type">包类型。</param>
        /// <param name="body">正文。</param>
        /// <param name="bodyLength">正文长度。</param>
        /// <returns>要写出的总字节数。</returns>
        private int Pack(byte type, byte[] body, int bodyLength)
        {
            if (body == null || bodyLength < 0)
            {
                bodyLength = 0;
            }

            if (body != null && bodyLength > body.Length)
            {
                bodyLength = body.Length;
            }

            var size = HeaderSize + bodyLength;
            if (mSendBuffer.Length < size)
            {
                mSendBuffer = new byte[size];
            }

            var length = bodyLength + 1;
            mSendBuffer[0] = (byte)length;
            mSendBuffer[1] = (byte)(length >> 8);
            mSendBuffer[2] = (byte)(length >> 16);
            mSendBuffer[3] = (byte)(length >> 24);
            mSendBuffer[4] = type;
            if (bodyLength > 0)
            {
                Buffer.BlockCopy(body, 0, mSendBuffer, HeaderSize, bodyLength);
            }

            return size;
        }

        /// <summary>
        /// 读取这条连接上已经到达的字节，并切出完整包。
        /// </summary>
        /// <param name="peer">连接。</param>
        /// <returns>连接仍然可用时为 true。</returns>
        private bool ReadAvailable(Peer peer)
        {
            var client = peer.Client;
            if (client == null || !client.Connected)
            {
                return false;
            }

            try
            {
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
            }
            catch (Exception)
            {
                return false;
            }

            while (peer.Inbox.Count >= HeaderSize)
            {
                var length = peer.Inbox[0] | (peer.Inbox[1] << 8) | (peer.Inbox[2] << 16) | (peer.Inbox[3] << 24);
                if (length < 1 || length > MaxPacket)
                {
                    return false;
                }

                if (peer.Inbox.Count < 4 + length)
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
                App.Events.Emit(new NetPacket { PeerId = peer.Id, Type = type, Body = body });
                if (peer.Dead || (IsServer ? mListener == null : mClient == null))
                {
                    return !peer.Dead;
                }
            }

            return true;
        }

        /// <summary>
        /// 写出一段字节。失败不抛出。
        /// </summary>
        /// <param name="client">连接。</param>
        /// <param name="packet">缓冲。</param>
        /// <param name="size">字节数。</param>
        /// <returns>写成功时为 true。</returns>
        private static bool Write(TcpClient client, byte[] packet, int size)
        {
            try
            {
                if (client == null || !client.Connected)
                {
                    return false;
                }

                client.GetStream().Write(packet, 0, size);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 关闭连接。关闭失败忽略。
        /// </summary>
        /// <param name="client">连接。</param>
        private static void Close(TcpClient client)
        {
            if (client == null)
            {
                return;
            }

            try
            {
                client.Close();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 观察任务异常，避免连接被取消后变成未观察异常。
        /// </summary>
        /// <param name="task">连接任务。</param>
        private static void Observe(Task task)
        {
            if (task == null)
            {
                return;
            }

            if (task.IsCompleted)
            {
                var ignored = task.Exception;
                return;
            }

            task.ContinueWith(
                finished =>
                {
                    var ignored = finished.Exception;
                },
                TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
