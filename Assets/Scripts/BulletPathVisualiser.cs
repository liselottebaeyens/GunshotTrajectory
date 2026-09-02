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
    // ===== SLICER =====

    Vector3 s_sternum = ApplyRasToUnity(sternumSlicerMm);
    Vector3 s_left = ApplyRasToUnity(clavicleLSlicerMm);
    Vector3 s_right = ApplyRasToUnity(clavicleRSlicerMm);

    // ===== UNITY =====

    Vector3 u_sternum = sternumUnity.position;
    Vector3 u_left = clavicleUnityL.position;
    Vector3 u_right = clavicleUnityR.position;

    // =====================================================
    // SCALE
    // =====================================================

    float slicerWidth = Vector3.Distance(s_left, s_right);
    float unityWidth = Vector3.Distance(u_left, u_right);

    scaleFactor = unityWidth / slicerWidth;

    // SCALE toepassen op slicerpunten

    s_sternum *= scaleFactor;
    s_left *= scaleFactor;
    s_right *= scaleFactor;

    // =====================================================
    // SLICER BASIS
    // =====================================================

    Vector3 s_x = (s_right - s_left).normalized;

    Vector3 s_mid = (s_left + s_right) * 0.5f;

    Vector3 s_y = (s_sternum - s_mid).normalized;

    Vector3 s_z = Vector3.Cross(s_x, s_y).normalized;

    // orthogonaal maken

    s_y = Vector3.Cross(s_z, s_x).normalized;

    // =====================================================
    // UNITY BASIS
    // =====================================================

    Vector3 u_x = (u_right - u_left).normalized;

    Vector3 u_mid = (u_left + u_right) * 0.5f;

    Vector3 u_y = (u_sternum - u_mid).normalized;

    Vector3 u_z = Vector3.Cross(u_x, u_y).normalized;

    u_y = Vector3.Cross(u_z, u_x).normalized;

    // =====================================================
    // ROTATIEMATRICES
    // =====================================================

    Matrix4x4 slicerBasis = Matrix4x4.identity;

    slicerBasis.SetColumn(0, new Vector4(s_x.x, s_x.y, s_x.z, 0));
    slicerBasis.SetColumn(1, new Vector4(s_y.x, s_y.y, s_y.z, 0));
    slicerBasis.SetColumn(2, new Vector4(s_z.x, s_z.y, s_z.z, 0));

    Matrix4x4 unityBasis = Matrix4x4.identity;

    unityBasis.SetColumn(0, new Vector4(u_x.x, u_x.y, u_x.z, 0));
    unityBasis.SetColumn(1, new Vector4(u_y.x, u_y.y, u_y.z, 0));
    unityBasis.SetColumn(2, new Vector4(u_z.x, u_z.y, u_z.z, 0));

    // =====================================================
    // VOLLEDIGE ROTATIE
    // =====================================================

    Matrix4x4 rotationMatrix =
        unityBasis * slicerBasis.inverse;

    alignmentRotation =
        rotationMatrix.rotation;

    // =====================================================
    // OFFSET
    // =====================================================

    Vector3 transformedSternum =
        alignmentRotation * s_sternum;

    alignmentOffset =
        u_sternum - transformedSternum;

    Debug.Log("Alignment calculated");
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
        Vector3 direction =
    (alignmentRotation * ApplyRasToUnity(dirRAS)).normalized;

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