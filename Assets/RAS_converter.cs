using UnityEngine;

public class RAS_converter : MonoBehaviour
{
    public GameObject testSphere;

    public Vector3 raspoint = new Vector3(-13.222f, -8.827f,-153.609f);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 unitypoint = RAS_to_Unity(raspoint);
        if (testSphere != null)
        {
            testSphere.transform.position = unitypoint;
        }

    }
    Vector3 RAS_to_Unity (Vector3 ras)
    {
        return new Vector3(ras.x,ras.z,-ras.y)/1000f;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
