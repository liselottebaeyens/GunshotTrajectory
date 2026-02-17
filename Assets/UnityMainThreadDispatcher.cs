
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private readonly Queue<Action> _actions = new Queue<Action>();
    private readonly Queue<IEnumerator> _coroutines = new Queue<IEnumerator>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            var go = new GameObject("UnityMainThreadDispatcher");
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            UnityEngine.Object.DontDestroyOnLoad(go);
        }
        return _instance;
    }

    public void Enqueue(Action action)
    {
        lock (_actions) _actions.Enqueue(action);
    }

    public void Enqueue(IEnumerator coroutine)
    {
        lock (_coroutines) _coroutines.Enqueue(coroutine);
    }

    void Update()
    {
        lock (_actions)
        {
            while (_actions.Count > 0)
            {
                var a = _actions.Dequeue();
                a?.Invoke();
            }
        }
        lock (_coroutines)
        {
            while (_coroutines.Count > 0)
            {
                StartCoroutine(_coroutines.Dequeue());
            }
        }
    }
}