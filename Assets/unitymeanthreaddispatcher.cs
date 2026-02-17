
using UnityEngine;
using System.Collections;

public class TestDispatcher : MonoBehaviour
{
    public IEnumerator ThisWillBeExecutedOnTheMainThread()
    {
        Debug.Log("This is executed from the main thread");
        yield return null;
    }

    public void ExampleMainThreadCall()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(ThisWillBeExecutedOnTheMainThread());
    }
}