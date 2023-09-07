using Cysharp.Threading.Tasks;
using System;
using UniFan;
using UnityEngine;

namespace HotCode.Framework
{
    /// <summary>
    /// 计时器管理器
    /// </summary>
    public partial class TimerManager : ManagerSingleton<TimerManager>
    {

        protected uint _idGenerator = 0;
        private BetterLinearMap<TimerUnit> _timers = new BetterLinearMap<TimerUnit>();
        private BetterList<uint, TimerUnit> _timersToAdd = new BetterList<uint, TimerUnit>();

        public ulong _frame;

        /// <summary>
        /// 当前帧
        /// </summary>
        public ulong currentFrame => _frame;

        public override int managerPriority => -1000;

        /// <summary>
        /// 生成新id
        /// </summary>
        /// <returns></returns>
        protected virtual uint GenTimerId()
        {
            do
            {
                _idGenerator++;
            }
            while (_idGenerator == 0 || _timers.ContainId(_idGenerator));
            return _idGenerator;
        }

        protected override void InitManager()
        {
            base.InitManager();
            MonoDriver.Instance.lateUpdateHandle += OnLateUpdate;
        }

        /// <summary>
        /// 添加帧刷函数
        /// </summary>
        /// <param name="interval">计时间隔，单位取决于参数timerType，当timerType == TimerType.Frame时，单位为帧，否则为秒</param>
        /// <param name="handler">帧刷处理函数</param>
        /// <param name="loop">循环次数，默认为1</param>
        /// <param name="timerType">计时类型，默认为带缩放的时间，单位秒</param>
        /// <returns>timer id</returns>
        public uint StartTimer(Action handler, float interval, int loop = 1, TimerType timerType = TimerType.Time)
        {
            if (loop == 0)
            {
                Debug.LogWarning("loop == 0,unable to execute。");
            }
            var timer = ClassPool.Get<TimerUnit>();

            var id = GenTimerId();
            _timersToAdd.Add(id, timer);
            timer.Init(timerType, id, interval, handler, loop);

            return id;
        }

        /// <summary>
        /// 开始单次计时器
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="timerType"></param>
        /// <returns></returns>
        public uint StartOneTimer(float interval, TimerType timerType = TimerType.Time)
        {
            return StartTimer(null, interval, 1, timerType);
        }

        /// <summary>
        /// 等待完成
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async UniTask AwaitTimer(uint id)
        {
            if (!_timersToAdd.TryGetValue(id, out var timer))
            {
                timer = _timers.GetValue(id);
            }
            if (timer != null)
            {
                await timer.AwaitCount();
            }
        }

        /// <summary>
        /// 移除帧刷函数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="invokeCallback"></param>
        public void StopTimer(uint id, bool invokeComplete = false)
        {
            var timer = _timers.GetValue(id);
            if (timer != null)
            {
                if (!timer.expired)
                {
                    timer.Stop(invokeComplete);
                }
                return;
            }
            if (_timersToAdd.TryGetValue(id, out timer))
            {
                PutBack(timer);
                _timersToAdd.Remove(id);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            float unscaledDeltaTime = Time.unscaledDeltaTime;
            //先按添加顺序执行
            for (int i = 0; i < _timers.IndexCount; i++)
            {
                var timer = _timers.GetValueByIndex(i);
                if (timer != null)
                {
                    timer.OnUpdate(deltaTime, unscaledDeltaTime);
                }
            }
            //再统一移除失效的Timer
            for (int i = 0; i < _timers.IndexCount; i++)
            {
                var timer = _timers.GetValueByIndex(i);
                if (timer != null && timer.expired)
                {
                    _timers.Remove(timer.id);
                    PutBack(timer);
                }
            }

            _frame++;
        }

        /// <summary>
        /// 需要在LateUpdate中添加
        /// </summary>
        private void OnLateUpdate()
        {
            if (_timersToAdd.Count > 0)
            {
                foreach (var timer in _timersToAdd)
                {
                    _timers.Add(timer.id, timer);
                }
                _timersToAdd.Clear();
            }

        }

        /// <summary>
        /// 移除所有计时器
        /// </summary>
        /// <param name="includeInfinite">是否移除无限循环计时器</param>
        public void RemoveAllTimer(bool includeInfinite)
        {

            for (int i = 0; i < _timers.IndexCount; i++)
            {
                var timer = _timers.GetValueByIndex(i);
                if (timer != null)
                {
                    timer.Stop(false);
                }
            }

            _timers.Clear();
            _timersToAdd.Clear();
        }

        /// <summary>
        /// 放回池子
        /// </summary>
        /// <param name="timer"></param>
        private void PutBack(TimerUnit timer)
        {
            ClassPool.Put<TimerUnit>(timer);
        }


    }
}
