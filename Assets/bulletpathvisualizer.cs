using UnityEngine;
using TMPro;

[RequireComponent(typeof(LineRenderer))] 
public class BulletPathVisualizer : MonoBehaviour
{
    [Header("Oorsprong 3D slicer bepalen")]
    public Transform mannequinAnchor;   // bv. sternum, moet je eerst bepaald hebben in 3D slicer en dan maken dat deze midden in het volume staat in Unity
    public Vector3 slicerAnchorMm;      // anatomisch punt uit Slicer (mm, RAS) 
                                        // opzich wel handig als je ander punt dan de oorsprong neemt in 3D slicer, mss nog handiger dan die op (0,0,0) te zetten 

    public GameObject entrySphere;      // sphere voor entry
    public GameObject exitSphere;       // sphere voor exit

    [Header("Coördinaten vanuit 3D slicer (mm, RAS)")]
    public TMP_InputField entryx;
    public TMP_InputField entryy;
    public TMP_InputField entryz;
    public TMP_InputField exitx;
    public TMP_InputField exity;
    public TMP_InputField exitz;
    private Vector3 EntryMm
    {
        get
        {
            float x = float.Parse(entryx.text); //parse om om te zetten van text nr float 
            float y = float.Parse(entryy.text);
            float z = float.Parse(entryz.text);
            return new Vector3(x, y, z);
        }
    }

    private Vector3 ExitMm
    {
        get
        {
            float x = float.Parse(exitx.text);
            float y = float.Parse(exity.text);
            float z = float.Parse(exitz.text);
            return new Vector3(x, y, z);
        }
    }

    [Header("Lijninstellingen")]
    public float extraLength = 0.2f;    // verlenging in meter // hier later nog een schuifknop van maken
    public Color lineColor = Color.red; // kleur van de lijn // later ook een kleurkiezer van maken 
    public float lineWidth = 0.01f; //dikte van de lijn, die je mss later ook nog kan aanpassen 
    public bool updateEveryFrame = false; // toggle voor dynamische update

    [Header("Pistoolinstellingen")]
    public GameObject gunPrefab;        // Sleep hier jouw pistool prefab in
    public Vector3 gunPositionOffset;   // offset voor positie
    public Vector3 gunRotationOffset;   // offset voor rotatie
    private GameObject gunInstance;

    private Matrix4x4 rasToUnity; // 4x4 matrix voor RAS -> Unity transformatie 
    private Vector3 localOffset;
    private LineRenderer lineRenderer;

    void Awake()  // gebeurt bij de startup van het script, nog voor start wordt uitgevoerd 
    {
        // Rotatiematrix RAS -> Unity 
        rasToUnity = Matrix4x4.identity; // 4x4 matrix want kan alles: rotatie, translatie, schaal in één object 
        rasToUnity.SetColumn(0, new Vector4(1, 0, 0, 0));   // X = R
        rasToUnity.SetColumn(1, new Vector4(0, 0, -1, 0));  // Y = S
        rasToUnity.SetColumn(2, new Vector4(0, 1, 0, 0));   // Z = -A

        // Setup LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = lineColor;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.positionCount = 2;
    }

    void Start()
    {
        Vector3 slicerAnchorLocal = ApplyRasToUnity(slicerAnchorMm);
        localOffset = -slicerAnchorLocal;

        UpdateVisualization(); 
    }

    void Update()
    {
        if (updateEveryFrame)
        {
            UpdateVisualization();
        }
    }

    private void UpdateVisualization()
    {
        Vector3 entryPos = ToWorld(EntryMm);
        Vector3 exitPos = ToWorld(ExitMm);

        // Zet spheres op positie
        if (entrySphere)
        {
            entrySphere.transform.position = entryPos;
            SetSphereColor(entrySphere, Color.red);
        }
        if (exitSphere)
        {
            exitSphere.transform.position = exitPos;
            SetSphereColor(exitSphere, Color.blue);
        }

        // Bereken verlengde exit
        Vector3 direction = -(exitPos - entryPos).normalized;
        Vector3 extendedExit = entryPos + direction * extraLength;

        // Update LineRenderer
        lineRenderer.SetPosition(0, entryPos);
        lineRenderer.SetPosition(1, extendedExit);

        // Pistool toevoegen en oriënteren
       
// Pistool toevoegen en oriënteren
        if (gunPrefab != null)
        {
        if (gunInstance == null)
        {
        gunInstance = Instantiate(gunPrefab, entryPos, Quaternion.identity);
        }

        gunInstance.transform.position = extendedExit + gunPositionOffset;
        gunInstance.transform.rotation = Quaternion.LookRotation(-direction) * Quaternion.Euler(gunRotationOffset);
        }
    }
    private void SetSphereColor(GameObject sphere, Color color) //kan alleen aangeroepen worden binnen deze class, en een void omdat er niets teruggegeven wordt
    {
        Renderer r = sphere.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = color;
        }
    }

    private Vector3 ApplyRasToUnity(Vector3 slicerMm) //dit is geen void, dus geeft iets terug, namelijk een Vector3
    {
        Vector3 meters = slicerMm * 0.001f;
        return rasToUnity.MultiplyVector(meters);
    }

    private Vector3 ToWorld(Vector3 slicerMmPoint)
    {
        
    // Zet mm naar meters en pas RAS -> Unity matrix toe
    Vector3 localPoint = ApplyRasToUnity(slicerMmPoint) + localOffset;

    // Voeg correctie van 180° rond de Y-as toe
    Quaternion rotationCorrection = Quaternion.Euler(0, 180, 0);

    // Bereken wereldpositie met mannequinAnchor en correctie
       return mannequinAnchor.position + (mannequinAnchor.rotation * rotationCorrection) * localPoint;

    }

    
    public void OnApplyButtonClicked()
    {
    Debug.Log("Apply clicked, updating visualization...");
    UpdateVisualization();
    }
}
