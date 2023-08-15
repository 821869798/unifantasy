using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFan
{
    public class FSMSystem<TStateId, TEventId> : IDisposable
        where TStateId : IComparable
        where TEventId : IComparable
    {
        public TStateId currentStateId { get; protected set; }

        public virtual string currentStateName => currentStateId.ToString();

        public FSMState<TStateId, TEventId> currentState { get; protected set; }

        public bool isActive { protected set; get; }


        private Dictionary<TStateId, FSMState<TStateId, TEventId>> _states { get; } = new Dictionary<TStateId, FSMState<TStateId, TEventId>>();

        public int Count => _states.Count;

        public event Action<TStateId> onFsmStarted;
        public event Action<TStateId, TStateId> onStateChanged;

        private EqualityComparer<TStateId> _stateIdComparer;

        public FSMSystem()
        {
            _stateIdComparer = EqualityComparer<TStateId>.Default;
        }

        public virtual void AddState(FSMState<TStateId, TEventId> state)
        {
            if (_states.ContainsKey(state.stateId))
            {
                Debug.LogError("[FSMSystem|AddState] Can't add state: state is already present,stateId:" + state.stateId);
                return;
            }
            state.SetFSM(this);
            _states.Add(state.stateId, state);
            state.OnInit();
        }

        public void Start(TStateId initStateId)
        {
            if (!TryGetState(initStateId, out var nextState))
            {
                Debug.LogError("[FSMSystem|Start] start failed,not found state,stateId:" + initStateId);
                return;
            }
            currentStateId = initStateId;
            currentState = nextState;
            Activate();

            currentState.active = true;
            currentState.OnEnter(default(TStateId));

            onFsmStarted?.Invoke(currentStateId);
        }

        public void ChangeState(TStateId stateId)
        {
            if (!isActive)
            {
                Debug.LogError("[FSMSystem|ChangeState] fsm not start or not active");
                return;
            }
            if (_stateIdComparer.Equals(stateId, currentStateId))
            {
                return;
            }
            if (!currentState.IsNextStateValid(stateId))
            {
                return;
            }
            if (!TryGetState(stateId, out var nextState))
            {
                Debug.LogError("[FSMSystem|ChangeState] change state is not found,stateId:" + stateId);
                return;
            }
            var oldState = currentState;
            if (oldState != null)
            {
                oldState.active = false;
                oldState.OnExit();
            }
            currentState = nextState;
            currentStateId = stateId;
            currentState.active = true;
            currentState.OnEnter(oldState.stateId);

            onStateChanged?.Invoke(oldState.stateId, currentState.stateId);
        }

        public void Activate()
        {
            if (currentState == null)
            {
                Debug.LogError("[FSMSystem|Activate] Can't activate FSMSystem: No starting state has been defined");
                return;
            }
            isActive = true;
        }

        public void DeActivate()
        {
            isActive = false;
        }

        public virtual void UpdateFSM(float deltaTime)
        {
            if (isActive)
            {
                currentState?.OnUpdate(deltaTime);
            }
        }

        public virtual bool TryGetState(TStateId stateId, out FSMState<TStateId, TEventId> value)
        {
            return _states.TryGetValue(stateId, out value);
        }

        public virtual bool TryGetState<T>(TStateId stateId, out T result) where T : FSMState<TStateId, TEventId>
        {
            if (_states.TryGetValue(stateId, out var value))
            {
                result = value as T;
                return result != null;
            }
            result = default(T);
            return false;
        }


        public virtual void BroadcastEvent(TEventId eventId, params object[] args)
        {
            if (currentState != null)
            {
                currentState.OnEvent(eventId, args);
            }
        }

        public virtual void Dispose()
        {
            if (currentState != null)
            {
                currentState.active = false;
                currentState.OnExit();
            }
            foreach (var kv in _states)
            {
                kv.Value.Dispose();
            }
            _states.Clear();
        }
    }
}
