public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> actions = new Queue<Action>();
    private static MainThreadDispatcher instance = null;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
            {
                actions.Dequeue().Invoke();
            }
        }
    }

    public static MainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            throw new Exception("Could not find the MainThreadDispatcher GameObject. Please ensure you have added this script to an empty GameObject in your scene.");
        }

        return instance;
    }

    void OnDestroy()
    {
        instance = null;
    }

    public void Enqueue(IEnumerator action)
    {
        lock (actions)
        {
            actions.Enqueue(() => { StartCoroutine(action); });
        }
    }

    public void Enqueue(Action action)
    {
        Enqueue(ActionWrapper(action));
    }

    public void Enqueue<T1>(Action<T1> action, T1 param1)
    {
        Enqueue(ActionWrapper(action, param1));
    }

    public void Enqueue<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
    {
        Enqueue(ActionWrapper(action, param1, param2));
    }

    public void Enqueue<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
    {
        Enqueue(ActionWrapper(action, param1, param2, param3));
    }

    public void Enqueue<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4)
    {
        Enqueue(ActionWrapper(action, param1, param2, param3, param4));
    }

    IEnumerator ActionWrapper(Action action)
    {
        action();
        yield return null;
    }

    IEnumerator ActionWrapper<T1>(Action<T1> action, T1 param1)
    {
        action(param1);
        yield return null;
    }

    IEnumerator ActionWrapper<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2)
    {
        action(param1, param2);
        yield return null;
    }

    IEnumerator ActionWrapper<T1, T2, T3>(Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
    {
        action(param1, param2, param3);
        yield return null;
    }

    IEnumerator ActionWrapper<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4)
    {
        action(param1, param2, param3, param4);
        yield return null;
    }
}