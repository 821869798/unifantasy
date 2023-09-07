using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MsgDispatcher
{
    static public Dictionary<int, Delegate> eventTable = new Dictionary<int, Delegate>();

    //Message handlers that should never be removed, regardless of calling Cleanup
    static public List<int> permanentMessages = new List<int>();
    #region Helper methods
    //Marks a certain message as permanent.
    static private void MarkAsPermanent(int eventType)
    {
#if LOG_ALL_MESSAGES
        Debug.Log("Messenger MarkAsPermanent \t\"" + eventType + "\"");
#endif
        if (!permanentMessages.Contains(eventType))
        {
            permanentMessages.Add(eventType);
        }
    }

    static public void MarkAsPermanent(MsgEventType eventType)
    {
        MarkAsPermanent((int)eventType);
    }


    static public void Cleanup()
    {
        List<int> messagesToRemove = new List<int>();

        foreach (var pair in eventTable)
        {
            bool wasFound = false;

            foreach (int message in permanentMessages)
            {
                if (pair.Key == message)
                {
                    wasFound = true;
                    break;
                }
            }

            if (!wasFound)
                messagesToRemove.Add(pair.Key);
        }

        foreach (var message in messagesToRemove)
        {
            eventTable.Remove(message);
        }
    }

    static public void PrintEventTable()
    {
        Debug.Log("\t\t\t=== MESSENGER PrintEventTable ===");

        foreach (var pair in eventTable)
        {
            Debug.Log("\t\t\t" + pair.Key + "\t\t" + pair.Value);
        }

        Debug.Log("\n");
    }
    #endregion

    #region Message logging and exception throwing
    static private void OnListenerAdding(int eventType, Delegate listenerBeingAdded)
    {
        if (!eventTable.ContainsKey(eventType))
        {
            eventTable.Add(eventType, null);
        }

        Delegate d = eventTable[eventType];
        if (d != null && d.GetType() != listenerBeingAdded.GetType())
        {
            throw new ListenerException(string.Format("Attempting to add listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being added has type {2}", eventType, d.GetType().Name, listenerBeingAdded.GetType().Name));
        }
    }

    static private void OnListenerRemoved(int eventType)
    {
        if (eventTable.ContainsKey(eventType) && eventTable[eventType] == null)
        {
            eventTable.Remove(eventType);
        }
    }

    static private void OnBroadcasting(int eventType)
    {
#if REQUIRE_LISTENER
        if (!eventTable.ContainsKey(eventType)) {
            throw new BroadcastException(string.Format("Broadcasting message \"{0}\" but no listener found. Try marking the message with Messenger.MarkAsPermanent.", eventType));
        }
#endif
    }

    static private BroadcastException CreateBroadcastSignatureException(int eventType)
    {
        return new BroadcastException(string.Format("Broadcasting message \"{0}\" but listeners have a different signature than the broadcaster.", eventType));
    }

    public class BroadcastException : Exception
    {
        public BroadcastException(string msg)
            : base(msg)
        {
        }
    }

    public class ListenerException : Exception
    {
        public ListenerException(string msg)
            : base(msg)
        {
        }
    }
    #endregion

    #region AddListener
    //No parameters
    static private void AddListener(int eventType, Callback handler)
    {
        OnListenerAdding(eventType, handler);
        //先删除保证同一个委托只注册一次
        eventTable[eventType] = (Callback)eventTable[eventType] - handler;
        eventTable[eventType] = (Callback)eventTable[eventType] + handler;
    }

    //Single parameter
    static private void AddListener<T>(int eventType, Callback<T> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = (Callback<T>)eventTable[eventType] - handler;
        eventTable[eventType] = (Callback<T>)eventTable[eventType] + handler;
    }

    //Two parameters
    static private void AddListener<T, U>(int eventType, Callback<T, U> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = (Callback<T, U>)eventTable[eventType] - handler;
        eventTable[eventType] = (Callback<T, U>)eventTable[eventType] + handler;
    }

    //Three parameters
    static private void AddListener<T, U, V>(int eventType, Callback<T, U, V> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = (Callback<T, U, V>)eventTable[eventType] - handler;
        eventTable[eventType] = (Callback<T, U, V>)eventTable[eventType] + handler;
    }

    //Four parameters
    static private void AddListener<T, U, V, W>(int eventType, Callback<T, U, V, W> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = (Callback<T, U, V, W>)eventTable[eventType] - handler;
        eventTable[eventType] = (Callback<T, U, V, W>)eventTable[eventType] + handler;
    }

    //Five parameters
    static private void AddListener<T, U, V, W, X>(int eventType, Callback<T, U, V, W, X> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = (Callback<T, U, V, W, X>)eventTable[eventType] - handler;
        eventTable[eventType] = (Callback<T, U, V, W, X>)eventTable[eventType] + handler;
    }

    //Six parameters
    static private void AddListener<T, U, V, W, X, Y>(int eventType, Callback<T, U, V, W, X, Y> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = (Callback<T, U, V, W, X, Y>)eventTable[eventType] - handler;
        eventTable[eventType] = (Callback<T, U, V, W, X, Y>)eventTable[eventType] + handler;
    }

    //No parameters
    static public void AddListener(MsgEventType eventType, Callback handler)
    {
        AddListener((int)eventType, handler);
    }

    //Single parameter
    static public void AddListener<T>(MsgEventType eventType, Callback<T> handler)
    {
        AddListener((int)eventType, handler);
    }

    //Two parameters
    static public void AddListener<T, U>(MsgEventType eventType, Callback<T, U> handler)
    {
        AddListener((int)eventType, handler);
    }

    //Three parameters
    static public void AddListener<T, U, V>(MsgEventType eventType, Callback<T, U, V> handler)
    {
        AddListener((int)eventType, handler);
    }

    static public void AddListener<T, U, V, W>(MsgEventType eventType, Callback<T, U, V, W> handler)
    {
        AddListener((int)eventType, handler);
    }

    static public void AddListener<T, U, V, W, X>(MsgEventType eventType, Callback<T, U, V, W, X> handler)
    {
        AddListener((int)eventType, handler);
    }

    static public void AddListener<T, U, V, W, X, Y>(MsgEventType eventType, Callback<T, U, V, W, X, Y> handler)
    {
        AddListener((int)eventType, handler);
    }
    #endregion

    #region RemoveListener
    //No parameters
    static private void RemoveListener(int eventType, Callback handler)
    {
        //OnListenerRemoving(eventType, handler);
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = (Callback)eventTable[eventType] - handler;
        }
        OnListenerRemoved(eventType);
    }

    //Single parameter
    static private void RemoveListener<T>(int eventType, Callback<T> handler)
    {
        //OnListenerRemoving(eventType, handler);
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = (Callback<T>)eventTable[eventType] - handler;
        }
        OnListenerRemoved(eventType);
    }

    //Two parameters
    static private void RemoveListener<T, U>(int eventType, Callback<T, U> handler)
    {
        //OnListenerRemoving(eventType, handler);
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = (Callback<T, U>)eventTable[eventType] - handler;
        }
        OnListenerRemoved(eventType);
    }

    //Three parameters
    static private void RemoveListener<T, U, V>(int eventType, Callback<T, U, V> handler)
    {
        //OnListenerRemoving(eventType, handler);
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = (Callback<T, U, V>)eventTable[eventType] - handler;
        }
        OnListenerRemoved(eventType);
    }

    //Four parameters
    static private void RemoveListener<T, U, V, W>(int eventType, Callback<T, U, V, W> handler)
    {
        //OnListenerRemoving(eventType, handler);
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = (Callback<T, U, V, W>)eventTable[eventType] - handler;
        }
        OnListenerRemoved(eventType);
    }

    //Five parameters
    static private void RemoveListener<T, U, V, W, X>(int eventType, Callback<T, U, V, W, X> handler)
    {
        //OnListenerRemoving(eventType, handler);
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = (Callback<T, U, V, W, X>)eventTable[eventType] - handler;
        }
        OnListenerRemoved(eventType);
    }

    //Six parameters
    static private void RemoveListener<T, U, V, W, X, Y>(int eventType, Callback<T, U, V, W, X, Y> handler)
    {
        //OnListenerRemoving(eventType, handler);
        if (eventTable.ContainsKey(eventType))
        {
            eventTable[eventType] = (Callback<T, U, V, W, X, Y>)eventTable[eventType] - handler;
        }
        OnListenerRemoved(eventType);
    }

    static public void RemoveListener(MsgEventType eventType, Callback handler)
    {
        RemoveListener((int)eventType, handler);
    }

    //Single parameter
    static public void RemoveListener<T>(MsgEventType eventType, Callback<T> handler)
    {
        RemoveListener((int)eventType, handler);
    }

    //Two parameters
    static public void RemoveListener<T, U>(MsgEventType eventType, Callback<T, U> handler)
    {
        RemoveListener((int)eventType, handler);
    }

    //Three parameters
    static public void RemoveListener<T, U, V>(MsgEventType eventType, Callback<T, U, V> handler)
    {
        RemoveListener((int)eventType, handler);
    }

    //Four parameters
    static public void RemoveListener<T, U, V, W>(MsgEventType eventType, Callback<T, U, V, W> handler)
    {
        RemoveListener((int)eventType, handler);
    }

    //Five parameters
    static public void RemoveListener<T, U, V, W, X>(MsgEventType eventType, Callback<T, U, V, W, X> handler)
    {
        RemoveListener((int)eventType, handler);
    }

    //Six parameters
    static public void RemoveListener<T, U, V, W, X, Y>(MsgEventType eventType, Callback<T, U, V, W, X, Y> handler)
    {
        RemoveListener((int)eventType, handler);
    }
    #endregion

    #region Broadcast
    //No parameters
    static private void Broadcast(int eventType)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (eventTable.TryGetValue(eventType, out d))
        {
            Callback callback = d as Callback;

            if (callback != null)
            {
                callback();
            }
            else
            {
                throw CreateBroadcastSignatureException(eventType);
            }
        }
    }

    //Single parameter
    static private void Broadcast<T>(int eventType, T arg1)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (eventTable.TryGetValue(eventType, out d))
        {
            Callback<T> callback = d as Callback<T>;

            if (callback != null)
            {
                callback(arg1);
            }
            else
            {
                throw CreateBroadcastSignatureException(eventType);
            }
        }
    }

    //Two parameters
    static private void Broadcast<T, U>(int eventType, T arg1, U arg2)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (eventTable.TryGetValue(eventType, out d))
        {
            Callback<T, U> callback = d as Callback<T, U>;

            if (callback != null)
            {
                callback(arg1, arg2);
            }
            else
            {
                throw CreateBroadcastSignatureException(eventType);
            }
        }
    }

    //Three parameters
    static private void Broadcast<T, U, V>(int eventType, T arg1, U arg2, V arg3)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (eventTable.TryGetValue(eventType, out d))
        {
            Callback<T, U, V> callback = d as Callback<T, U, V>;

            if (callback != null)
            {
                callback(arg1, arg2, arg3);
            }
            else
            {
                throw CreateBroadcastSignatureException(eventType);
            }
        }
    }

    static private void Broadcast<T, U, V, W>(int eventType, T arg1, U arg2, V arg3, W arg4)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (eventTable.TryGetValue(eventType, out d))
        {
            Callback<T, U, V, W> callback = d as Callback<T, U, V, W>;

            if (callback != null)
            {
                callback(arg1, arg2, arg3, arg4);
            }
            else
            {
                throw CreateBroadcastSignatureException(eventType);
            }
        }
    }

    static private void Broadcast<T, U, V, W, X>(int eventType, T arg1, U arg2, V arg3, W arg4, X arg5)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (eventTable.TryGetValue(eventType, out d))
        {
            Callback<T, U, V, W, X> callback = d as Callback<T, U, V, W, X>;

            if (callback != null)
            {
                callback(arg1, arg2, arg3, arg4, arg5);
            }
            else
            {
                throw CreateBroadcastSignatureException(eventType);
            }
        }
    }

    static private void Broadcast<T, U, V, W, X, Y>(int eventType, T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
        Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (eventTable.TryGetValue(eventType, out d))
        {
            Callback<T, U, V, W, X, Y> callback = d as Callback<T, U, V, W, X, Y>;

            if (callback != null)
            {
                callback(arg1, arg2, arg3, arg4, arg5, arg6);
            }
            else
            {
                throw CreateBroadcastSignatureException(eventType);
            }
        }
    }

    //No parameters
    static public void Broadcast(MsgEventType eventType)
    {
        Broadcast((int)eventType);
    }

    //Single parameter
    static public void Broadcast<T>(MsgEventType eventType, T arg1)
    {
        Broadcast((int)eventType, arg1);
    }

    //Two parameters
    static public void Broadcast<T, U>(MsgEventType eventType, T arg1, U arg2)
    {
        Broadcast((int)eventType, arg1, arg2);
    }

    //Three parameters
    static public void Broadcast<T, U, V>(MsgEventType eventType, T arg1, U arg2, V arg3)
    {
        Broadcast((int)eventType, arg1, arg2, arg3);
    }

    //Four parameters
    static public void Broadcast<T, U, V, W>(MsgEventType eventType, T arg1, U arg2, V arg3, W arg4)
    {
        Broadcast((int)eventType, arg1, arg2, arg3, arg4);
    }

    //Five parameters
    static public void Broadcast<T, U, V, W, X>(MsgEventType eventType, T arg1, U arg2, V arg3, W arg4, X arg5)
    {
        Broadcast((int)eventType, arg1, arg2, arg3, arg4, arg5);
    }

    //Six parameters
    static public void Broadcast<T, U, V, W, X, Y>(MsgEventType eventType, T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6)
    {
        Broadcast((int)eventType, arg1, arg2, arg3, arg4, arg5, arg6);
    }
    #endregion
}
