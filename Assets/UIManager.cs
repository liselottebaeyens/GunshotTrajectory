using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject entryRicoPanel;
    public GameObject uiGamePanel;
    public TMP_InputField entryX;
    public TMP_InputField entryY;
    public TMP_InputField entryZ;

    public TMP_InputField dirX;
    public TMP_InputField dirY;
    public TMP_InputField dirZ;
    
    public BulletPathVisualizer visualizer;

    private void ClearInputs()
{
    entryX.text = "";
    entryY.text = "";
    entryZ.text = "";

    dirX.text = "";
    dirY.text = "";
    dirZ.text = "";
}
    void Start()
    {
        // 👉 MAIN MENU = beide zichtbaar
        SetActiveSafe(startPanel, true);
        SetActiveSafe(entryRicoPanel, true);
        SetActiveSafe(uiGamePanel, false);
    }

    public void StartApp()
    {
        // 👉 game starten = menu weg
        SetActiveSafe(startPanel, false);
        SetActiveSafe(entryRicoPanel, false);
        SetActiveSafe(uiGamePanel, true);
    }

public void GoHome()
{
    StartCoroutine(ReloadScene());
}

private IEnumerator ReloadScene()
{
    SocketReceiver receiver = FindFirstObjectByType<SocketReceiver>();

    if (receiver != null)
    {
        receiver.StopServer();
    }

    yield return new WaitForSeconds(0.5f);

    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}

    private void SetActiveSafe(GameObject obj, bool state)
    {
        if (obj != null)
            obj.SetActive(state);
    }
}