using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EventController
{
    private List<string> m_permanentEvents;
    private Dictionary<string, Delegate> m_theRouter;


    public EventController()
    {

        m_permanentEvents = new List<string>();
        m_theRouter = new Dictionary<string, Delegate>();
    }
    //实例：
    //EventDispatcher.AddEventListener(Events.OtherEvent.MainCameraComplete, MainCameraCompleted);
    public void AddEventListener(string eventType, Action handler)
    {
        this.OnListenerAdding(eventType, handler);

        this.m_theRouter[eventType] = (Action)Delegate.Combine((Action)this.m_theRouter[eventType], handler);
    }

    //实例：
    //   EventDispatcher.AddEventListener<LoginResult>(Events.FrameWorkEvent.Login, OnLoginResp);
    //   EventDispatcher.AddEventListener<BaseAttachedInfo>(Events.FrameWorkEvent.EntityAttached, OnEntityAttached);
    //   EventDispatcher.AddEventListener<CellAttachedInfo>(Events.FrameWorkEvent.EntityCellAttached, OnEntityCellAttached);
    //   EventDispatcher.AddEventListener<CellAttachedInfo>(Events.FrameWorkEvent.AOINewEntity, AOINewEntity);
    //   EventDispatcher.AddEventListener<uint>(Events.FrameWorkEvent.AOIDelEvtity, AOIDelEntity);
    //   EventDispatcher.AddEventListener<DefCheckResult>(Events.FrameWorkEvent.CheckDef, OnCheckDefMD5);
    //   EventDispatcher.AddEventListener<int>(Events.GearEvent.SwitchLightMapFog, SwitchLightMapFog);
    //   EventDispatcher.AddEventListener<string>(Events.FrameWorkEvent.ReConnectKey, ReConnectKeyHandler);
    public void AddEventListener<T>(string eventType, Action<T> handler)
    {
        this.OnListenerAdding(eventType, handler);

        this.m_theRouter[eventType] = (Action<T>)Delegate.Combine((Action<T>)this.m_theRouter[eventType], handler);
    }

    //实例：
    //EventDispatcher.AddEventListener<int, bool>(Events.InstanceEvent.InstanceLoaded, SceneLoaded);
    //EventDispatcher.AddEventListener<int, float>(Events.OtherEvent.ChangeDummyRate, ChangeDummyRate);
    public void AddEventListener<T, U>(string eventType, Action<T, U> handler)
    {
        this.OnListenerAdding(eventType, handler);

        this.m_theRouter[eventType] = (Action<T, U>)Delegate.Combine((Action<T, U>)this.m_theRouter[eventType], handler);
    }
    //实例：
    //EventDispatcher.AddEventListener<string, int, string>(Events.FrameWorkEvent.BaseLogin, OnBaseLogin);
    public void AddEventListener<T, U, V>(string eventType, Action<T, U, V> handler)
    {
        this.OnListenerAdding(eventType, handler);

        this.m_theRouter[eventType] = (Action<T, U, V>)Delegate.Combine((Action<T, U, V>)this.m_theRouter[eventType], handler);
    }
    public void AddEventListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
    {
        this.OnListenerAdding(eventType, handler);

        this.m_theRouter[eventType] = (Action<T, U, V, W>)Delegate.Combine((Action<T, U, V, W>)this.m_theRouter[eventType], handler);
    }


    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="handler"></param>
    public void RemoveEventListener(string eventType, Action handler)
    {
        if (this.OnListenerRemoveing(eventType, handler))
        {
            this.m_theRouter[eventType] = (Action)Delegate.Remove((Action)this.m_theRouter[eventType], handler);
            this.OnListenerRemoved(eventType);
        }
    }

    public void RemoveEventListener<T>(string eventType, Action<T> handler)
    {
        if (this.OnListenerRemoveing(eventType, handler))
        {
            this.m_theRouter[eventType] = (Action<T>)Delegate.Remove((Action<T>)this.m_theRouter[eventType], handler);
            this.OnListenerRemoved(eventType);
        }
    }
    public void RemoveEventListener<T, U>(string eventType, Action<T, U> handler)
    {
        if (this.OnListenerRemoveing(eventType, handler))
        {
            this.m_theRouter[eventType] = (Action<T, U>)Delegate.Remove((Action<T, U>)this.m_theRouter[eventType], handler);
            this.OnListenerRemoved(eventType);
        }
    }
    /**
     * 
     * EventDispatcher.AddEventListener<string, int, string>("login", OnBaseLogin);
     * 
     * private void OnBaseLogin(string ip, int port, string token){
     * 
     * }
     * */
    public void RemoveEventListener<T, U, V>(string eventType, Action<T, U, V> handler)
    {
        if (this.OnListenerRemoveing(eventType, handler))
        {
            this.m_theRouter[eventType] = (Action<T, U, V>)Delegate.Remove((Action<T, U, V>)this.m_theRouter[eventType], handler);
            this.OnListenerRemoved(eventType);
        }
    }
    public void RemoveEventListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
    {
        if (this.OnListenerRemoveing(eventType, handler))
        {
            this.m_theRouter[eventType] = (Action<T, U, V, W>)Delegate.Remove((Action<T, U, V, W>)this.m_theRouter[eventType], handler);
            this.OnListenerRemoved(eventType);
        }
    }
    /// <summary>
    /// TriggerEvent
    /// </summary>
    /// <param name="eventType"></param>
    public void TriggerEvent(string eventType)
    {
        Delegate delegate2;
        if (this.m_theRouter.TryGetValue(eventType, out delegate2))
        {
            Delegate[] invocationList = delegate2.GetInvocationList();

            for (int i = 0; i < invocationList.Length; i++)
            {
                Action action = invocationList[i] as Action;
                if (action == null)
                {
                    throw new Exception();
                }
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    Debug.Log("exception=" + exception);
                }
            }

        }

    }
    /// <summary>
    /// 
    ///    ClassABC baseInfo = new ClassABC();
    ///    baseInfo.name = "gandecheng111";
    ///     baseInfo.age = 100;
    ///    EventDispatcher.TriggerEvent<ClassABC>("gandecheng", baseInfo);
    ///    
    ///        UInt32 entityID = (UInt32)Arguments[0];
    ///        EventDispatcher.TriggerEvent<uint>("AOIDelEvtity", entityID);

    /// </summary>
    /// <param name="eventType"></param>
    public void TriggerEvent<T>(string eventType, T arg1)
    {
        Delegate delegate2;
        if (this.m_theRouter.TryGetValue(eventType, out delegate2))
        {
            Delegate[] invocationList = delegate2.GetInvocationList();

            for (int i = 0; i < invocationList.Length; i++)
            {
                Action<T> action = invocationList[i] as Action<T>;
                if (action == null)
                {
                    throw new Exception();
                }
                try
                {
                    action(arg1);
                }
                catch (Exception exception)
                {
                    Debug.Log("exception=" + exception);
                }
            }

        }

    }

    public void TriggerEvent<T, U>(string eventType, T arg1, U arg2)
    {
        Delegate delegate2;
        if (this.m_theRouter.TryGetValue(eventType, out delegate2))
        {
            Delegate[] invocationList = delegate2.GetInvocationList();

            for (int i = 0; i < invocationList.Length; i++)
            {
                Action<T, U> action = invocationList[i] as Action<T, U>;
                if (action == null)
                {
                    throw new Exception();
                }
                try
                {
                    action(arg1, arg2);
                }
                catch (Exception exception)
                {
                    Debug.Log("exception=" + exception);
                }
            }

        }

    }
    /**
            string ip = Arguments[0] as String;
            int port = (Int32)(UInt16)Arguments[1];
            string token = Arguments[2] as String;
            EventDispatcher.TriggerEvent<string, int, string>("login", ip, port, token);
     * 
     * */
    // 
    public void TriggerEvent<T, U, V>(string eventType, T arg1, U arg2, V arg3)
    {
        Delegate delegate2;
        if (this.m_theRouter.TryGetValue(eventType, out delegate2))
        {
            Delegate[] invocationList = delegate2.GetInvocationList();

            for (int i = 0; i < invocationList.Length; i++)
            {
                Action<T, U, V> action = invocationList[i] as Action<T, U, V>;
                if (action == null)
                {
                    throw new Exception();
                }
                try
                {
                    action(arg1, arg2, arg3);
                }
                catch (Exception exception)
                {
                    Debug.Log("exception=" + exception);
                }
            }

        }

    }

    public void TriggerEvent<T, U, V, W>(string eventType, T arg1, U arg2, V arg3, W arg4)
    {
        Delegate delegate2;
        if (this.m_theRouter.TryGetValue(eventType, out delegate2))
        {
            Delegate[] invocationList = delegate2.GetInvocationList();

            for (int i = 0; i < invocationList.Length; i++)
            {
                Action<T, U, V, W> action = invocationList[i] as Action<T, U, V, W>;
                if (action == null)
                {
                    throw new Exception();
                }
                try
                {
                    action(arg1, arg2, arg3, arg4);
                }
                catch (Exception exception)
                {
                    Debug.Log("exception=" + exception);
                }
            }

        }

    }

    /// <summary>
    /// 核心函数
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="listenerBeingAdded"></param>
    private void OnListenerAdding(string eventType, Delegate listenerBeingAdded)
    {
        if (!this.m_theRouter.ContainsKey(eventType))
        {
            this.m_theRouter.Add(eventType, null);
        }
        Delegate delegate2 = this.m_theRouter[eventType];
        if ((delegate2 != null) && (delegate2.GetType() != listenerBeingAdded.GetType()))
        {
            Debug.Log("try to add not correct event");
        }
    }
    private void OnListenerRemoved(string eventType)
    {
        if (this.m_theRouter.ContainsKey(eventType) && (this.m_theRouter[eventType] == null))
        {
            this.m_theRouter.Remove(eventType);
        }

    }
    private bool OnListenerRemoveing(string eventType, Delegate listenerBeingAdded)
    {
        if (!this.m_theRouter.ContainsKey(eventType))
        {
            return false;
        }
        Delegate delegate2 = this.m_theRouter[eventType];
        if ((delegate2 != null) && (delegate2.GetType() != listenerBeingAdded.GetType()))
        {
            Debug.Log("try to add not correct event");
        }
        return true;
    }




    public void Cleanup()
    {
        List<string> list = new List<string>();
        foreach (KeyValuePair<string, Delegate> pair in this.m_theRouter)
        {
            bool flag = false;
            foreach (string str in this.m_permanentEvents)
            {
                if (pair.Key == str)
                {
                    flag = true;
                    break;
                }

            }
            if (!flag)
            {
                list.Add(pair.Key);
            }
        }
        foreach (string str in list)
        {
            this.m_theRouter.Remove(str);
        }

    }

    public bool ContainsEvent(string eventType)
    {
        return this.m_theRouter.ContainsKey(eventType);
    }

    public void MarkAsPermanent(string evnetType)
    {
        this.m_permanentEvents.Add(evnetType);
    }


    public Dictionary<string, Delegate> TheRouter
    {
        get
        {
            return this.m_theRouter;
        }
    }
}