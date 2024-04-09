using Cysharp.Threading.Tasks;
using HotCode.Framework;
using System.Net;
using System.Text;
using UniFan;
using UniFan.Network;
using UnityEngine;

namespace HotCode.Game
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
            public UnityEngine.UI.ExLoopVerticalScrollRect verticalLoopScroll { protected set; get; }
            protected virtual void InitBinding(ObjectBinding __binding)
            {
                __binding.TryGetVariableValue<UniFan.ExImage>("image", out var __tbv0);
                this.image = __tbv0;
                __binding.TryGetVariableValue<UniFan.ExButton>("button", out var __tbv1);
                this.button = __tbv1;
                __binding.TryGetVariableValue<UnityEngine.UI.ExLoopVerticalScrollRect>("verticalLoopScroll", out var __tbv2);
                this.verticalLoopScroll = __tbv2;
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

            TestLoadRes();

            TestTimer().Forget();

            TestLoopScrollView();

            //TestNetwork();
        }

        private void BtnClicked()
        {
            Debug.Log("Button Clicked");
            TestSendMsg("Button Clicked");
            this.ui.image.color = Color.blue;
        }

        #region 加载资源测试

        private void TestLoadRes()
        {
            var sp = this.GetWindowResloader().LoadABAsset<Sprite>(PathConstant.GetAtlasSpritePath("Common", "common_white"));
            Debug.Log(sp);
        }

        #endregion

        #region 计时器测试

        private async UniTaskVoid TestTimer()
        {
            var timerId = TimerManager.Instance.StartOneTimer(1f);
            await timerId.AwaitTimer();
            Debug.Log("Timer Complete");

            // stop timer,can call timerId.StopTimer()
            // timerId.StopTimer();
        }

        #endregion

        #region 循环列表测试
        // 循环列表示例，针对Item很多的情况。
        private LoopScrollAdapter<UINTestScrollItem> _loopScrollAdapter;

        private void TestLoopScrollView()
        {
            // 使用循环列表适配器初始化(更上层的封装，业务写起来更简单)，也可以自己使用原始的对象写(需要多写些代码)。
            _loopScrollAdapter = new LoopScrollAdapter<UINTestScrollItem>(this.ui.verticalLoopScroll).BindItemNodeChanged(OnScrollItemChanged);
            // 根据数据来设置Item数量
            _loopScrollAdapter.RefillCells(1000);
        }

        private void OnScrollItemChanged(UINTestScrollItem item, int index)
        {
            // 实际需要根据index获取数据，然后刷新用数据刷新Item；
            // 例如 var data = this.dataList[index]; item.RefreshByData(data);
            item.RefreshScrollItem(index);
        }


        #endregion

        #region 网络测试

        NetChannel netChannel;
        private void TestNetwork()
        {
            //ISocket socket = SocketFactory.Instance.Create("ws", new Uri("ws://127.0.0.1:7801"));
            //netChannel = new NetChannel(socket, new WSMsgCodec());

            ISocket socket = SocketFactory.Instance.Create("tcp", new System.Net.IPEndPoint(NetworkUtility.ParseIpAddress("127.0.0.1"), 7801));
            //ISocket socket = SocketFactory.Instance.Create("kcp", new System.Net.IPEndPoint(NetworkUtility.ParseIpAddress("127.0.0.1"), 7801));
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
                var rawData = packet.OutputSpan();
                var str = Encoding.UTF8.GetString(rawData);
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

        #endregion

        protected override void OnDispose()
        {
            base.OnDispose();
        }
    }
}