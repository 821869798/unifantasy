using Cysharp.Threading.Tasks;
using System;
using UniFan;
using UnityEngine;

namespace HotCode.Framework
{
    internal class TimerUnit : IReusableClass
    {
        public uint id { get; private set; }

        /// <summary>
        /// 计时器类型
        /// </summary>
        public TimerType timerType { get; private set; }

        /// <summary>
        /// 帧刷间隔
        /// </summary>
        public float interval { get; private set; }

        /// <summary>
        /// 帧刷处理函数
        /// </summary>
        public Action handler { get; private set; }

        /// <summary>
        /// 循环次数（-1代表无限循环）
        /// </summary>
        public int loop { get; private set; }

        /// <summary>
        /// 计数器
        /// </summary>
        protected float _counter = 0f;

        /// <summary>
        /// 是否已失效
        /// </summary>
        public bool expired { get; private set; }

        /// <summary>
        /// 支持Unitask
        /// </summary>
        public AutoResetUniTaskCompletionSource autoResetUniTask { get; set; }

        public TimerUnit()
        {

        }

        public void Init(TimerType timerType, uint id, float interval, Action handler, int loop)
        {
            this.id = id;
            this.timerType = timerType;
            this.interval = interval;
            this.handler = handler;
            this.loop = loop;
        }

        public UniTask AwaitCount()
        {
            if (autoResetUniTask != null)
            {
                return autoResetUniTask.Task;
            }
            autoResetUniTask = AutoResetUniTaskCompletionSource.Create();
            return autoResetUniTask.Task;
        }

        private void InvokeTaskResult()
        {
            if (autoResetUniTask == null)
            {
                return;
            }
            var autoTask = autoResetUniTask;
            autoResetUniTask = null;
            autoTask.TrySetResult();
        }

        private void InvokeTaskCancel()
        {
            if (autoResetUniTask == null)
            {
                return;
            }
            var autoTask = autoResetUniTask;
            autoResetUniTask = null;
            autoTask.TrySetCanceled();
        }

        public void OnUpdate(float deltaTime, float unscaledDeltaTime)
        {
            if (expired)
            {
                return;
            }

            switch (this.timerType)
            {
                case TimerType.Time:
                    _counter += deltaTime;
                    break;
                case TimerType.UnscaledTime:
                    _counter += unscaledDeltaTime;
                    break;
                case TimerType.Frame:
                    _counter += 1;
                    break;
            }

            if (_counter >= interval)
            {
                if (loop < 0)
                {
                    _counter -= interval;
                    CallComplete();
                    return;
                }
                loop--;
                if (loop <= 0)
                {
                    expired = true;
                    CallComplete();
                }
                else
                {
                    _counter -= interval;
                    CallComplete();
                }
            }
        }

        public void Stop(bool invokeComplete)
        {
            if (expired)
            {
                return;
            }
            expired = true;
            if (invokeComplete)
            {
                CallComplete();
            }
            else
            {
                InvokeTaskCancel();
            }
        }

        protected virtual void CallComplete()
        {
            try
            {
                handler?.Invoke();
                InvokeTaskResult();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occurred in timer. Message : {e.Message}\n{e.StackTrace}");
            }
        }

        public uint MaxStore => 50;

        public void OnReset()
        {
            handler = null;
            expired = false;
            _counter = 0;
            InvokeTaskCancel();
        }
    }
}
