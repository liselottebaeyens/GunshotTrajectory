using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    void Start()
    {
        // Laad jouw UI-scene
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Additive);

        // Laad jouw mannequin/gameplay scene
        SceneManager.LoadScene("GameScene", LoadSceneMode.Additive);
    }
}