using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFan
{
    //异步任务序列,风格类似DOTween的Sequence
    //同一序列一起执行,同一序列全部执行完,才执行下一个序列
    public class AsyncTaskSequence : IReusableClass
    {
        //有一个任务失败的时候是否终止
        public bool IsFailTermination
        {
            protected set;
            get;
        }

        //当前任务序列的索引
        private int _curSequenceIndex = 0;
        public int CurSequenceIndex { protected set { this._curSequenceIndex = value; } get { return this._curSequenceIndex; } }
        //总任务序列的数量
        private int _sequenceCount = 0;
        public int SequenceCount { protected set { this._sequenceCount = value; } get { return this._sequenceCount; } }
        //当前任务序列的数量
        private int _curSequenceTaskCount = 0;
        public int CurSequenceTaskCount { protected set { this._curSequenceTaskCount = value; } get { return this._curSequenceTaskCount; } }
        //总任务的数量
        public int TaskCount { get { return this._allTask.Count; } }
        //已经结束的任务,包括失败的任务
        private int _finishTaskCount = 0;
        public int FinishTaskCount { protected set { this._finishTaskCount = value; } get { return this._finishTaskCount; } }
        //所有的任务
        protected Dictionary<IAsyncTask, bool> _allTask = new Dictionary<IAsyncTask, bool>();
        //任务的序列
        protected List<List<IAsyncTask>> _taskSequence = new List<List<IAsyncTask>>();
        //任务事件
        public event Action<AsyncTaskSequence, bool, IAsyncTask> OnOneTaskFinish;


        public event Action<AsyncTaskSequence> OnAllTaskFinish;

        //是否正在运行
        private bool _isRunning = false;
        public bool IsRunning { protected set { this._isRunning = value; } get { return this._isRunning; } }

        public List<IAsyncTask> GetLastSequence()
        {
            if (_taskSequence.Count == 0)
            {
                return null;
            }
            return _taskSequence[_taskSequence.Count - 1];
        }

        //添加一个任务到下一个序列中
        public AsyncTaskSequence Append(IAsyncTask task)
        {
            if (IsRunning)
            {
                Debug.LogError("Task is Runing,can't to add task");
                return this;
            }
            if (task == null)
            {
                return this;
            }
            if (_allTask.ContainsKey(task))
            {
                //Debug.LogWarning("can't not add repeat task:" + task);
                return this;
            }
            _allTask.Add(task, false);
            int seIndex = SequenceCount;
            _taskSequence.Add(ListPool<IAsyncTask>.Get());
            _taskSequence[seIndex].Add(task);
            SequenceCount++;
            return this;
        }

        //添加一个任务到当前的序列中
        public AsyncTaskSequence Join(IAsyncTask task)
        {
            if (IsRunning)
            {
                Debug.LogError("Task is Runing,can't to add task");
                return this;
            }
            if (task == null)
            {
                return this;
            }
            if (_allTask.ContainsKey(task))
            {
                //Debug.LogWarning("can't not add repeat task:" + task);
                return this; ;
            }
            _allTask.Add(task, false);
            int seIndex = SequenceCount - 1;
            if (seIndex < 0)
            {
                seIndex = 0;
                _taskSequence.Add(ListPool<IAsyncTask>.Get());
                SequenceCount++;
            }
            _taskSequence[seIndex].Add(task);
            return this;
        }

        //开始任务
        public AsyncTaskSequence Start()
        {
            if (IsRunning)
            {
                Debug.LogError("Task is Already Runing");
                return this;
            }
            if (SequenceCount == 0)
            {
                Debug.LogWarning("no async task to do!");
                if (OnAllTaskFinish != null)
                {
                    OnAllTaskFinish(this);
                }
            }
            else
            {
                IsRunning = true;
                CurSequenceIndex = 0;
                FinishTaskCount = 0;
                //开始下一个序列任务
                DoNextSequence();
            }
            return this;
        }

        private void DoNextSequence()
        {
            //所有任务结束
            if (CurSequenceIndex >= SequenceCount)
            {
                return;
            }
            List<IAsyncTask> taskList = _taskSequence[CurSequenceIndex];
            CurSequenceTaskCount = taskList.Count;
            CurSequenceIndex++;
            for (int i = 0; i < taskList.Count; i++)
            {
                taskList[i].RegisterAsyncTaskCallback(OnAsyncTaskCallback);
                taskList[i].DoAsyncTask();
                //下面别再做操作，防止瞬间完成任务,去清空taskList,导致异常
            }
        }

        public void Stop()
        {
            OnReset();
        }

        private void OnAsyncTaskCallback(bool result, IAsyncTask task)
        {
            if (_allTask.TryGetValue(task, out var isComplete) == false || isComplete)
            {
                return;
            }
            _allTask[task] = true;
            FinishTaskCount++;
            task.RemoveAsyncTaskCallback(OnAsyncTaskCallback);
            if (OnOneTaskFinish != null)
            {
                OnOneTaskFinish(this, result, task);
            }
            if (!result)
            {
                if (IsFailTermination)
                {
                    Stop();
                    return;
                }
            }
            if (FinishTaskCount >= TaskCount)
            {
                IsRunning = false;
                if (OnAllTaskFinish != null)
                {
                    OnAllTaskFinish(this);
                }
                return;
            }
            if (--CurSequenceTaskCount <= 0)
            {
                DoNextSequence();
            }
        }

        public void OnReset()
        {
            IsRunning = false;
            CurSequenceIndex = 0;
            SequenceCount = 0;
            CurSequenceTaskCount = 0;
            FinishTaskCount = 0;
            foreach (var task in _allTask)
            {
                if (!task.Value)
                    task.Key?.RemoveAsyncTaskCallback(OnAsyncTaskCallback);
            }
            _allTask.Clear();
            for (int i = 0; i < _taskSequence.Count; i++)
            {
                ListPool<IAsyncTask>.Put(_taskSequence[i]);
            }
            _taskSequence.Clear();
            OnOneTaskFinish = null;
            OnAllTaskFinish = null;
        }


        #region Class Pool
        public uint MaxStore => 20;

        public static AsyncTaskSequence Create(bool isFailTermination = false)
        {
            AsyncTaskSequence ts = ClassPool.Get<AsyncTaskSequence>();
            ts.IsFailTermination = isFailTermination;
            return ts;
        }

        public void Put2Pool()
        {
            ClassPool.Put<AsyncTaskSequence>(this);
        }
        #endregion
    }

}

