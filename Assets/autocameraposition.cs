

using UnityEngine;

public class AutoCameraPosition : MonoBehaviour
{
    public Transform character;
    public Renderer characterRenderer;
    public Camera mainCamera;

    void Start()
    {
        if (characterRenderer == null && character != null)
            characterRenderer = character.GetComponentInChildren<Renderer>();

        if (characterRenderer == null || mainCamera == null)
        {
            Debug.LogError("Renderer of Camera niet ingesteld!");
            return;
        }

        UpdateCameraPosition();
    }

    public void UpdateCameraPosition()
    {
        Bounds bounds = characterRenderer.bounds;

        float verticalFOV = mainCamera.fieldOfView * Mathf.Deg2Rad;
        float horizontalFOV = 2 * Mathf.Atan(Mathf.Tan(verticalFOV / 2) * mainCamera.aspect);

        float distanceHeight = bounds.size.y / (2 * Mathf.Tan(verticalFOV / 2));
        float distanceWidth = bounds.size.x / (2 * Mathf.Tan(horizontalFOV / 2));

        float distance = Mathf.Max(distanceHeight, distanceWidth) * 1.2f;

        Vector3 target = bounds.center;
        Vector3 newPos = target - character.forward * distance;
        mainCamera.transform.position = newPos;
        mainCamera.transform.LookAt(target);

        Debug.Log($"Person size: {bounds.size}, Distance: {distance}");
    }
    
    void LateUpdate()
    {
    UpdateCameraPosition();
    }
}
