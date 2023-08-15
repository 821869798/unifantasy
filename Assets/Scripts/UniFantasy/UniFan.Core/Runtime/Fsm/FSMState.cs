using System;

namespace UniFan
{
    public abstract class FSMState<TStateId, TEventId> : IDisposable
        where TStateId : IComparable
        where TEventId : IComparable
    {
        /// <summary>
        /// 当前状态id
        /// </summary>
        public abstract TStateId stateId { get; }

        /// <summary>
        /// 是否是当前的state
        /// </summary>
        public virtual bool active { get; internal set; }

        /// <summary>
        /// 当前状态机
        /// </summary>
        public FSMSystem<TStateId, TEventId> fsm { private set; get; }


        internal void SetFSM(FSMSystem<TStateId, TEventId> fsm)
        {
            this.fsm = fsm;
        }

        public void ChangeState(TStateId stateId, bool force = false)
        {
            if (!this.active && !force)
            {
                return;
            }
            fsm.ChangeState(stateId);
        }

        /// <summary>
        /// 加入之后的初始化
        /// </summary>
        public abstract void OnInit();

        /// <summary>
        ///尝试转换的下个状态是否合法
        /// </summary>
        /// <param name="nextId"></param>
        /// <returns></returns>
        public abstract bool IsNextStateValid(TStateId nextId);

        /// <summary>
        /// 进入该状态的时候
        /// </summary>
        public abstract void OnEnter(TStateId prevStateId);

        /// <summary>
        /// 离开该状态的时候
        /// </summary>
        public abstract void OnExit();

        /// <summary>
        /// 销毁的时候
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void OnUpdate(float deltaTime)
        {

        }

        /// <summary>
        /// 监听事件
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="args"></param>
        public virtual void OnEvent(TEventId eventId, params object[] args)
        {

        }


    }
}
