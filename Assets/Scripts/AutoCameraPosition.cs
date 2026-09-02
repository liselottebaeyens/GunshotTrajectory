using UnityEngine;

public class AutoCameraPosition : MonoBehaviour
{
    [Header("Target")]
    public Transform character;
    public Renderer characterRenderer;

    [Header("Pivot")]
    public Transform pivot;

    [Header("View Settings")]
    public float distanceMultiplier = 1.2f;
    public float defaultYaw = 0f;
    public float defaultPitch = 20f;

    private Camera cam;
    private Bounds cachedBounds;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (characterRenderer == null && character != null)
            characterRenderer = character.GetComponentInChildren<Renderer>();

        if (characterRenderer == null)
        {
            Debug.LogError("CharacterRenderer niet ingesteld!");
            return;
        }

        if (pivot == null)
        {
            Debug.LogError("Pivot niet ingesteld!");
            return;
        }

        CacheBounds();
        PositionCamera(defaultYaw, defaultPitch);
    }

    void CacheBounds()
    {
        cachedBounds = characterRenderer.bounds;
    }

    public void PositionCamera(float yaw, float pitch)
    {
        Bounds b = cachedBounds;

        float verticalFOV = cam.fieldOfView * Mathf.Deg2Rad;
        float horizontalFOV = 2 * Mathf.Atan(Mathf.Tan(verticalFOV / 2) * cam.aspect);

        float distHeight = b.size.y / (2 * Mathf.Tan(verticalFOV / 2));
        float distWidth = b.size.x / (2 * Mathf.Tan(horizontalFOV / 2));

        float distance = Mathf.Max(distHeight, distWidth) * distanceMultiplier;

        pivot.position = character.position;
        pivot.rotation = Quaternion.Euler(pitch, yaw, 0);

        transform.localPosition = new Vector3(0, 0, -distance);
        transform.LookAt(character.position);
    }
}