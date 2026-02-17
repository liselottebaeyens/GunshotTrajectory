
using UnityEngine;

public class HideCanvasOnApply : MonoBehaviour
{
    [SerializeField] private GameObject canvasToHide;

    public void OnApplyButtonClicked()
    {
        if (canvasToHide != null)
        {
            Debug.Log("Hiding canvas: " + canvasToHide.name);
            canvasToHide.SetActive(false);
        }
        else
        {
            Debug.LogError("CanvasToHide is not assigned in the Inspector!");
        }
       }
}