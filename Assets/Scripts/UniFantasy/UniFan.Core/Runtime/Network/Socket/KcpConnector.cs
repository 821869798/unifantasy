using System.Net.Sockets;
using System.Net;
using KcpProject;
using System;

namespace UniFan.Network
{
    internal class KcpConnector : UdpConnector
    {
        private KCP kcp = null;
        private uint nextUpdateTime = 0;

        public bool WriteDelay { get; set; }
        public bool AckNoDelay { get; set; }

        private KCPByteBuffer kcpRecvBuffer = KCPByteBuffer.Allocate(1024 * 32);

        private KcpConfig kcpConfig;

        public class KcpConfig
        {
            public int nodelay = 0;
            public int interval = 30;
            public int fastresend = 2;
            public int nocwnd = 1;
            public float receiveTimeout = 15;    //超时时间，秒
        }

        public KcpConnector(string name, IPEndPoint ipEndPoint, KcpConfig kcpConfig = null)
    : base(name, ipEndPoint, 0)
        {
            if (kcpConfig != null)
            {
                this.kcpConfig = kcpConfig;
            }
            else
            {
                this.kcpConfig = new KcpConfig();
            }
            this.CfgReceiveTimeout = this.kcpConfig.receiveTimeout;
        }

        protected override void Initial()
        {
            PendingSentBuffer = new RingBuffer(2);
            ReceiveDataStates = new SocketReceiveStates(4096);
            SentDataStates = new SocketSentStates(2);
            ConnectDataStates = new ConnectStates();
        }

        protected virtual void InitKcp()
        {
            kcp = new KCP((uint)(new System.Random().Next(1, Int32.MaxValue)), RawSend);
            kcp.NoDelay(this.kcpConfig.nodelay, this.kcpConfig.interval, this.kcpConfig.fastresend, this.kcpConfig.nocwnd);
            kcp.SetStreamMode(true);
        }

        public override void Connect(Action<ConnectResults, Exception> callback = null)
        {
            if (kcp == null)
            {
                InitKcp();
            }
            base.Connect(callback);
        }

        protected override void OnReset()
        {
            base.OnReset();
            kcpRecvBuffer.Clear();
        }

        private void RawSend(byte[] data, int length)
        {
            if (Socket != null)
            {
                Socket.Send(data, length, SocketFlags.None);
            }
        }

        public override SendResults Send(byte[] data, int offset, int count)
        {
            if (data == null || data.Length == 0 || count <= 0)
            {
                return SendResults.Success;
            }
            lock (SyncRoot)
            {
                if (!Connected || needToClose)
                {
                    return SendResults.Faild;
                }
                var waitsnd = kcp.WaitSnd;
                if (waitsnd < kcp.SndWnd && waitsnd < kcp.RmtWnd)
                {

                    var sendBytes = 0;
                    do
                    {
                        var n = Math.Min((int)kcp.Mss, count - sendBytes);
                        kcp.Send(data, offset + sendBytes, n);
                        sendBytes += n;
                    } while (sendBytes < count);

                    waitsnd = kcp.WaitSnd;
                    if (waitsnd >= kcp.SndWnd || waitsnd >= kcp.RmtWnd || !WriteDelay)
                    {
                        kcp.Flush(false);
                    }
                    sentBytes += count;
                    return SendResults.Success;
                }
            }
            //TriggerOnBufferFull(data);
            return SendResults.Faild;
        }

        public override void OnUpdate(float deltaTime, float unsacaleTime)
        {
            base.OnUpdate(deltaTime, unsacaleTime);
            if (Status == SocketStatus.Establish)
            {
                if (0 == nextUpdateTime || kcp.CurrentMS >= nextUpdateTime)
                {
                    kcp.Update();
                    nextUpdateTime = kcp.Check();
                }
            }
        }

        protected override void OnReceiveBytes(ref ArraySegment<byte> receive)
        {
            var inputN = kcp.Input(receive.Array, receive.Offset, receive.Count, true, AckNoDelay);
            if (inputN < 0)
            {
                return;
            }

            kcpRecvBuffer.Clear();

            //读完所有完整的消息
            for (; ; )
            {
                var size = kcp.PeekSize();
                if (size < 0) break;

                kcpRecvBuffer.EnsureWritableBytes(size);

                var n = kcp.Recv(kcpRecvBuffer.RawBuffer, kcpRecvBuffer.WriterIndex, size);
                if (n > 0)
                {
                    kcpRecvBuffer.WriterIndex += n;
                }
            }

            // 有数据待接收
            if (kcpRecvBuffer.ReadableBytes > 0)
            {
                var bytes = new ArraySegment<byte>(kcpRecvBuffer.RawBuffer, kcpRecvBuffer.ReaderIndex, kcpRecvBuffer.ReadableBytes);
                base.OnReceiveBytes(ref bytes);
            }

        }
    }
}
