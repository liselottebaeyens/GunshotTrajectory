using UnityEngine;
using UnityEngine.EventSystems;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;          // Het model waar rond gedraaid wordt
    public float distance = 15.0f;     // Startafstand
    public float zoomSpeed = 2.0f;
    public float rotationSpeed = 100.0f;
    public float minDistance = 2f;
    public float maxDistance = 15f;

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
{
    x = 0f;
    y = 10f;
}
    void LateUpdate()
{
    if (target == null)
        return;

         // 👇 HIER moet de check staan!
    if (EventSystem.current.IsPointerOverGameObject())
        return;

    if (Input.GetMouseButton(0))
    {
        x += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        y -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        y = Mathf.Clamp(y, -30f, 70f);
    }

    distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
    distance = Mathf.Clamp(distance, minDistance, maxDistance);

    Quaternion rotation = Quaternion.Euler(y, x, 0);
    Vector3 position = rotation * new Vector3(0, 0, -distance) + target.position;

    transform.position = position;
    transform.LookAt(target);
}
}