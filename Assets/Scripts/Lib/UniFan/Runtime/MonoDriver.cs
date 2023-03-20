using UnityEngine;
using System;
using System.Diagnostics;

namespace UniFan
{
    public class MonoDriver : SingletonMono<MonoDriver>
    {
        public event Action<float> updateHandle;

        public event Action<float> fixedUpdateHandle;

        public event Action lateUpdateHandle;

        public event Action onApplicationQuit;

        public event Action<bool> onApplicationPause;

        //数据持久化用的
        public event Action onMobileApplicationPause;

        private static int LastestUpdateFrame = 0;

        private void Update()
        {
            DebugCheckDuplicateDriver();

            if (updateHandle != null)
            {
                updateHandle(Time.deltaTime);
            }
        }

        [Conditional("GameDev")]
        private void DebugCheckDuplicateDriver()
        {
            if (LastestUpdateFrame > 0)
            {
                if (LastestUpdateFrame == Time.frameCount)
                    UnityEngine.Debug.LogWarning($"There are two {nameof(MonoDriver)} in the scene. Please ensure there is always exactly one driver in the scene.");
            }

            LastestUpdateFrame = Time.frameCount;
        }


        private void FixedUpdate()
        {
            if (fixedUpdateHandle != null)
            {
                fixedUpdateHandle(Time.fixedDeltaTime);
            }
        }

        private void LateUpdate()
        {
            if (lateUpdateHandle != null)
            {
                lateUpdateHandle();
            }
        }

#if UNITY_EDITOR

        public Action drawGizmos;
        private void OnDrawGizmos()
        {
            drawGizmos?.Invoke();
        }

#endif


        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            drawGizmos = null;
#endif

            updateHandle = null;
            lateUpdateHandle = null;
            fixedUpdateHandle = null;
            if (onApplicationQuit != null)
            {
                onApplicationQuit();
                onApplicationQuit = null;
            }
        }


        #region 退出相关

        public void SetOnApplicationPause(Action action)
        {
#if UNITY_EDITOR
            onApplicationQuit += action;
#else
        onMobileApplicationPause += action;
#endif
        }

        public void RemoveApplicationPause(Action action)
        {
#if UNITY_EDITOR
            onApplicationQuit -= action;
#else
        onMobileApplicationPause -= action;
#endif

        }
        private void OnApplicationPause(bool pause)
        {
            //切后台算手机的退出
            if (onMobileApplicationPause != null && pause)
            {
                onMobileApplicationPause();
            }

            onApplicationPause?.Invoke(pause);
        }

        #endregion
    }

}

