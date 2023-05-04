using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFan;

namespace UniFan.Res
{
    public abstract class Res : SimpleRC, IRes
    {

        protected string _assetName;
        public string AssetName
        {
            protected set
            {
                this._assetName = value;
            }
            get
            {
                return this._assetName;
            }
        }

        private ResState _resState = ResState.Waiting;

        public ResState State
        {
            protected set
            {
                this._resState = value;
                if (State == ResState.Ready)
                {
                    NotifyResEvent(true);
                    //如果加载成功，但是引用是空的，需要通知ResManager去清除
                    if (RefCount <= 0)
                    {
                        ResManager.NotifyResManagerClear();
                    }
                }
            }
            get { return this._resState; }
        }

        public abstract ResType ResType { get; }

        protected UnityEngine.Object _asset;
        public virtual UnityEngine.Object Asset
        {
            protected set { this._asset = value; }
            get { return this._asset; }
        }

        public float Progress
        {
            get
            {
                switch (State)
                {
                    case ResState.Loading:
                        return CalculateProgress();
                    case ResState.Ready:
                        return 1;
                }

                return 0;
            }
        }

        protected virtual float CalculateProgress()
        {
            return 0;
        }

        private event Action<bool, IAsyncTask> _resListener;


        protected Res(string assetName)
        {
            AssetName = assetName;
        }

        public Res()
        {

        }

        protected bool CheckLoadAble()
        {
            return State == ResState.Waiting;
        }

        /// <summary>
        /// 取消了同步加载
        /// </summary>
        protected void WarnCancelSyncLoad()
        {
#if GameDev || UNITY_EDITOR
            if (State == ResState.Loading)
            {
                //有资源在异步加载，需要注意
                Debug.LogWarning($"Res is in loading, cancel sync load {AssetName}");
            }
#endif
        }

        public virtual IEnumerator DoIEnumeratorTask(Action finishCallback)
        {
            finishCallback();
            yield break;
        }

        public virtual List<IRes> GetAndRetainDependResList()
        {
            return null;
        }

        public virtual bool Load()
        {
            return false;
        }

        public virtual void LoadAsync()
        {

        }

        private void NotifyResEvent(bool result)
        {
            if (_resListener != null)
            {
                _resListener(result, this);
            }
            _resListener = null;
        }

        protected void OnResLoadFaild()
        {
            State = ResState.Waiting;
            NotifyResEvent(false);
        }



        public bool ReleaseRes()
        {
            if (State == ResState.Loading)
            {
                return false;
            }

            if (State != ResState.Ready)
            {
                return true;
            }
            OnReleaseRes();

            _resListener = null;

            return true;
        }

        protected virtual void OnReleaseRes()
        {
            if (Asset != null)
            {
                //GameObject删不了,只有借助AssetBundle.Unload(true)和Resources.UnloadUnusedAssets才能删除
                if (Asset is GameObject)
                {

                }
                else
                {
                    Resources.UnloadAsset(Asset);
                }

                Asset = null;
            }
            State = ResState.Waiting;
        }

        protected override void OnZeroRef()
        {
            if (State == ResState.Loading)
            {
                return;
            }

            ReleaseRes();
        }

        public void DoAsyncTask()
        {
            if (State == ResState.Ready)
            {
                NotifyResEvent(true);
                return;
            }
            if (!CheckLoadAble())
            {
                return;
            }
            LoadAsync();
        }

        public void RegisterAsyncTaskCallback(Action<bool, IAsyncTask> listener)
        {
            if (listener == null)
            {
                return;
            }

            _resListener += listener;
        }

        public void RemoveAsyncTaskCallback(Action<bool, IAsyncTask> listener)
        {
            if (listener == null)
            {
                return;
            }

            if (_resListener == null)
            {
                return;
            }

            _resListener -= listener;
        }

        public override string ToString()
        {
            return string.Format("ResType:{0}\t Name:{1}\t State:{2}\t RefCount:{3}", this.ResType.ToString(), AssetName, State, RefCount);
        }

        #region Class Pool
        public virtual uint MaxStore => 30;

        public virtual void OnReset()
        {
            _assetName = string.Empty;
            State = ResState.Waiting;
            _resListener = null;
            _asset = null;
        }

        public abstract void Put2Pool();
        #endregion

    }

}

