
using UnityEngine;

public class SlicerToUnityMapper : MonoBehaviour
{
    [Header("References")]
    public Transform mannequinAnchor; // bv. sternum of hips in Unity
    public Vector3 slicerAnchorMm;    // hetzelfde anatomisch punt uit Slicer (in mm, RAS)
    public GameObject testSphere;

    [Header("Test point from Slicer (mm, RAS)")]
    public Vector3 slicerPointMm = new Vector3(-13.222f, -8.827f, -153.609f);

    private Matrix4x4 rotationMatrix;
    private Vector3 localOffset;

    void Start()
    {
        // Rotatiematrix RAS -> Unity
        rotationMatrix = Matrix4x4.identity;
        rotationMatrix.SetColumn(0, new Vector4(1, 0, 0, 0));   // X = R
        rotationMatrix.SetColumn(1, new Vector4(0, 0, -1, 0));  // Y = S
        rotationMatrix.SetColumn(2, new Vector4(0, 1, 0, 0));   // Z = -A

        // Bereken local offset zodat slicerAnchor naar mannequinAnchor gaat
        Vector3 slicerAnchorUnity = ApplyTransform(slicerAnchorMm);
        localOffset = -slicerAnchorUnity; // verschuif slicer-origin naar mannequin-local origin

        // Converteer testpunt
        Vector3 slicerPointUnityLocal = ApplyTransform(slicerPointMm) + localOffset;

        // Pas mannequin transform toe (positie + rotatie)
        Vector3 unityPointWorld = mannequinAnchor.position + mannequinAnchor.rotation * slicerPointUnityLocal;

        if (testSphere != null)
        {
            testSphere.transform.position = unityPointWorld;
        }

        Debug.Log($"Unity point: {unityPointWorld}");
    }

    Vector3 ApplyTransform(Vector3 slicerMm)
    {
        Vector3 scaled = slicerMm * 0.001f; // mm -> m
        return rotationMatrix.MultiplyVector(scaled);
    }
}
