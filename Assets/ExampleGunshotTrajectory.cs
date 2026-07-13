using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SimpleBulletLine : MonoBehaviour
{
    [Header("References")]
    public Transform startAnchor;

    [Header("Gun")]
    public GameObject gunPrefab;

    public Vector3 gunPositionOffset = new Vector3(0, -0.05f, 0);
    public Vector3 gunRotationOffset = Vector3.zero;

    [Header("Line Settings")]
    public float lineLength = 1.5f;
    public float lineWidth = 0.05f;
    public Color lineColor = Color.red;

    [Header("Direction Offset")]
    public Vector3 directionOffset = new Vector3(0.5f, 0.3f, 0f);

    private LineRenderer lineRenderer;
    private GameObject gunInstance;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        lineRenderer.material =
            new Material(Shader.Find("Unlit/Color"));

        lineRenderer.material.color = lineColor;
        lineRenderer.material.renderQueue = 5000;
        lineRenderer.material.SetInt("_ZWrite", 0);
        lineRenderer.material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);

        if (gunPrefab != null)
        {
            gunInstance = Instantiate(gunPrefab);
        }
    }

    void Update()
    {
        if (gunInstance == null || startAnchor == null)
            return;

        Vector3 baseDirection =
    -gunInstance.transform.forward;

Vector3 direction =
    (baseDirection + directionOffset).normalized;

// kleine offset om z-fighting te vermijden
Vector3 startPoint =
    startAnchor.position +
    direction * 0.01f;
    
        Vector3 endPoint =
            startPoint + direction * lineLength;

        // lijn tekenen
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);

        // geweer rotatie
        Quaternion gunRotation =
            Quaternion.LookRotation(startPoint - endPoint) *
            Quaternion.Euler(gunRotationOffset);

        gunInstance.transform.rotation = gunRotation;

        // geweer positie + offset
        gunInstance.transform.position =
            endPoint +
            gunRotation * gunPositionOffset;
    }
}