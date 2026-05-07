using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class BulletPathVisualizer : MonoBehaviour
{
    [Header("Anchor")]
    public Transform sternumUnity;
    public Transform clavicleUnityL;
    public Transform clavicleUnityR;
    public Vector3 sternumSlicerMm;
    public Vector3 clavicleLSlicerMm;
    public Vector3 clavicleRSlicerMm;
    private float scaleFactor;
    private Quaternion alignmentRotation;
    private Vector3 alignmentOffset;

    public GameObject errorText;

    [Header("UI Input")]
    public TMP_InputField entryX;
    public TMP_InputField entryY;
    public TMP_InputField entryZ;

    public TMP_InputField dirX;
    public TMP_InputField dirY;
    public TMP_InputField dirZ;
    public UIManager uiManager;

    [Header("Visualisatie")]
    public GameObject entrySphere;
    public float lineLength = 0.5f;
    public float lineWidth = 0.01f;
    public Color lineColor = Color.red;

    [Header("Gun")]
    public GameObject gunPrefab;
    public Vector3 gunPositionOffset;
    public Vector3 gunRotationOffset;

    [Header("UI")]
    public Slider lineLengthSlider;

    [Header("Mode")]
    public bool useLiveData = false; // 👈 BELANGRIJK

    private GameObject gunInstance;
    private LineRenderer lineRenderer;
    private Matrix4x4 rasToUnity;

    void Awake()
    {
        rasToUnity = Matrix4x4.identity;
        rasToUnity.SetColumn(0, new Vector4(-1, 0, 0, 0));
        rasToUnity.SetColumn(1, new Vector4(0, 0, 1, 0));
        rasToUnity.SetColumn(2, new Vector4(0, -1, 0, 0));

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = lineColor;
    }

    void CalculateAlignment()
{
    // ---- SLICER PUNTEN ----

    Vector3 s_sternum = ApplyRasToUnity(sternumSlicerMm);
    Vector3 s_left = ApplyRasToUnity(clavicleLSlicerMm);
    Vector3 s_right = ApplyRasToUnity(clavicleRSlicerMm);

    // ---- UNITY PUNTEN ----

    Vector3 u_sternum = sternumUnity.position;
    Vector3 u_left = clavicleUnityL.position;
    Vector3 u_right = clavicleUnityR.position;

    // ---- SCHAAL ----

    float slicerWidth = Vector3.Distance(s_left, s_right);
    float unityWidth = Vector3.Distance(u_left, u_right);

    scaleFactor = unityWidth / slicerWidth;

    Debug.Log("Scale factor: " + scaleFactor);

    // ---- ROTATIE ----

    Vector3 slicerDir = (s_right - s_left).normalized;
    Vector3 unityDir = (u_right - u_left).normalized;

    alignmentRotation =
        Quaternion.FromToRotation(slicerDir, unityDir);

    // ---- OFFSET ----

    Vector3 transformedSternum =
        alignmentRotation * (s_sternum * scaleFactor);

    alignmentOffset =
        u_sternum - transformedSternum;
}

    void Start()
    {
        CalculateAlignment();

        if (lineLengthSlider != null)
        {
            lineLengthSlider.minValue = 0.2f;
            lineLengthSlider.maxValue = 5f;
            lineLengthSlider.value = lineLength;
            lineLengthSlider.onValueChanged.AddListener(OnSliderChanged);
        }
    }

    void OnSliderChanged(float value)
    {
        UpdateVisualizationNEW();
    }

public void OnManualApplyButtonClicked()
{
    useLiveData = false;

    if (!InputsAreValid())
    {
        if (errorText != null)
            errorText.SetActive(true);

        return;
    }

    if (errorText != null)
        errorText.SetActive(false);

    UpdateVisualizationNEW();

    if (uiManager != null)
        uiManager.StartApp();
}

public void OnLiveDataButtonClicked()
{
    useLiveData = true;

    UpdateVisualizationNEW();

    if (uiManager != null)
        uiManager.StartApp();
}

    public void UpdateVisualizationNEW()
    {

        Debug.Log("VISUALIZER UPDATE RUN");

        Vector3 entryMm;
        Vector3 dirRAS;

        if (useLiveData)
{
    if (!BulletData.hasData)
    {
        Debug.LogWarning("Nog geen live data ontvangen");
        return;
    }

    entryMm = BulletData.entryPoint;
    dirRAS = BulletData.direction;

    Debug.Log("LIVE ENTRY: " + entryMm);
    Debug.Log("LIVE DIR: " + dirRAS);
}

        else
        {
            entryMm = new Vector3(
                ParseInput(entryX),
                ParseInput(entryY),
                ParseInput(entryZ)
            );

            dirRAS = new Vector3(
                ParseInput(dirX),
                ParseInput(dirY),
                ParseInput(dirZ)
            );

            Debug.Log("ENTRY MM: " + entryMm);
            Debug.Log("DIRECTION RAS: " + dirRAS);

        }


        Vector3 entryPos = ToWorld(entryMm);
        Vector3 direction = ApplyRasToUnity(dirRAS).normalized;

        float currentLength = lineLengthSlider != null ? lineLengthSlider.value : lineLength;

        Vector3 endPoint = entryPos + direction * currentLength;

        if (entrySphere != null)
            entrySphere.transform.position = entryPos;
        
        Debug.Log("ENTRY POS UNITY: " + entryPos);
        Debug.Log("END POINT: " + endPoint);

        lineRenderer.SetPosition(0, entryPos);
        lineRenderer.SetPosition(1, endPoint);

        if (gunPrefab != null)
        {
            if (gunInstance == null)
                gunInstance = Instantiate(gunPrefab, null);

            Vector3 gunDirection = (entryPos - endPoint).normalized;

            gunInstance.transform.rotation =
                Quaternion.LookRotation(gunDirection) *
                Quaternion.Euler(gunRotationOffset);

            gunInstance.transform.position =
                endPoint + gunInstance.transform.rotation * gunPositionOffset;
        }
    }

    private float ParseInput(TMP_InputField field)
    {
        if (field != null && float.TryParse(field.text, out float value))
            return value;

        return 0f;
    }

    private Vector3 ApplyRasToUnity(Vector3 mm)
    {
        return rasToUnity.MultiplyVector(mm * 0.001f);
    }

private Vector3 ToWorld(Vector3 slicerPointMm)
{
    Vector3 point = ApplyRasToUnity(slicerPointMm);

    Vector3 transformed =
        alignmentRotation * (point * scaleFactor);

    return transformed + alignmentOffset;
}
    private bool IsValid(string text)
{
    return float.TryParse(
        text,
        System.Globalization.NumberStyles.Float,
        System.Globalization.CultureInfo.InvariantCulture,
        out _
    );
}

private bool InputsAreValid()
{
    return
        IsValid(entryX.text) &&
        IsValid(entryY.text) &&
        IsValid(entryZ.text) &&
        IsValid(dirX.text) &&
        IsValid(dirY.text) &&
        IsValid(dirZ.text);
}

}