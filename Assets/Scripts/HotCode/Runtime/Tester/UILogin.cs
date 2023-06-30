using UniFan;
using HotCode.Framework;
using System;
using UnityEngine;
using UnityEngine.UI;
using UniFan.Network;
using System.Text;
using System.Net;
using Cysharp.Threading.Tasks;

namespace HotCode.FrameworkPlay
{
    public class UILogin : UIBaseWindow
    {
        /// <summary>
        /// 静态配置
        /// </summary>
        private static UICreateConfig _createConfig = new UICreateConfig()
        {
            prefabName = nameof(UILogin),
            parentPath = string.Empty,
            layer = EUILayer.Normal,
            permanent = false,
        };

        /// <summary>
        /// 创建UI的配置
        /// </summary>
        public override IUICreateConfig createConfig => _createConfig;

        #region Template Generate,don't modify
        protected partial class UIB_UILogin
        {
            #region ObjectBinding Generate 
            public UniFan.ExImage image { protected set; get; }
            public UniFan.ExButton button { protected set; get; }
            protected virtual void InitBinding(ObjectBinding __binding)
            {
                var __tbv0 = __binding.GetVariableByName("image");
                this.image = __tbv0.GetValue<UniFan.ExImage>();
                var __tbv1 = __binding.GetVariableByName("button");
                this.button = __tbv1.GetValue<UniFan.ExButton>();
            }

            #endregion ObjectBinding Generate 
        }
        #endregion Template Generate,don't modify

        /// <summary>
        /// 可以自定义修改的
        /// </summary>
        protected partial class UIB_UILogin
        {
            public virtual void StartBinding(GameObject __go)
            {
                var binding = __go.GetComponent<ObjectBinding>();
                if (binding != null)
                {
                    this.InitBinding(binding);
                }
            }
        }
        protected UIB_UILogin ui { get; set; }

        protected override void BeforeInit()
        {
            ui = new UIB_UILogin();
            ui.StartBinding(gameObject);
        }

        protected override void OnInit()
        {
            this.ui.button.onClick.AddListener(BtnClicked);
            //TestNetwork();
        }

        NetChannel netChannel;
        private void TestNetwork()
        {
            //ISocket socket = SocketFactory.Instance.Create("ws", new Uri("ws://127.0.0.1:7801"));
            //netChannel = new NetChannel(socket, new WSMsgCodec());

            ISocket socket = SocketFactory.Instance.Create("tcp", new System.Net.IPEndPoint(IPAddress.Parse("127.0.0.1"), 7801));
            //ISocket socket = SocketFactory.Instance.Create("kcp", new System.Net.IPEndPoint(IPAddress.Parse("127.0.0.1"), 7801));
            netChannel = new NetChannel(socket, new LTVMsgCodec());

            MonoDriver.Instance.updateHandle += (t) =>
            {
                if (netChannel != null)
                {
                    netChannel.OnUpdate(t, Time.unscaledDeltaTime);
                }
            };
            netChannel.OnPacket += (net, packet) =>
            {
                var rawData = packet.Output();
                var str = Encoding.UTF8.GetString(rawData.Array, rawData.Offset, rawData.Count);
                Debug.Log($"recv message:{str}");
                packet.Put();
            };
            netChannel.OnConnecting += (net, ip) =>
            {
                Debug.Log($"start connecting!");
            };
            netChannel.OnConnected += (net) =>
            {
                Debug.Log($"net connected!");
                TestSendMsg("hello server!");
                TestSendMsg("test message1!");
                TestSendMsg("test message2!");
                TestSendMsg("test message3!");
                TestSendMsg("test message4!");
                TestSendMsg("test message5!");
            };
            netChannel.OnClosed += (net, e) =>
            {
                Debug.Log($"net disconnected:{e}");
            };
            netChannel.Connect();
        }

        private void TestSendMsg(string str)
        {
            if (netChannel == null || !netChannel.Connected)
            {
                return;
            }
            IMsgPacket packet = netChannel.MsgCodec.CreatePacket();
            packet.Encode(Encoding.UTF8.GetBytes(str));
            netChannel.Send(packet);
            packet.Put();
        }



        private void BtnClicked()
        {
            Debug.Log("Button Clicked");
            TestSendMsg("Button Clicked");
            this.ui.image.color = Color.blue;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
        }
    }
}