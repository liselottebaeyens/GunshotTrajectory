using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [HideInInspector]
    public bool useSlicer = false;

    public BulletPathVisualizer visualizer;

    public void ConnectToSlicer()
{
    Debug.Log("CONNECT BUTTON GEKLIKT");

    useSlicer = true;

    if (visualizer != null)
        visualizer.useLiveData = true;

    if (SocketReceiver != null)
    {
        SocketReceiver.StopServer();   // 🔥 BELANGRIJK
        SocketReceiver.StartServer();  // 🔥 opnieuw starten
    }
}

    public void UseManualInput()
    {
        useSlicer = false;

        if (visualizer != null)
            visualizer.useLiveData = false;

        Debug.Log("Manual mode");
    }

    public SocketReceiver SocketReceiver;

    public GameObject mainMenuUI;

    public void DeleteMainMenu()
    {
        mainMenuUI.SetActive(false); // 🔥 menu weg

        useSlicer = true;

        if (SocketReceiver != null)
            SocketReceiver.StartServer();
    }
    
}