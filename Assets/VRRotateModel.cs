using UnityEngine;
using UnityEngine.XR;

public class VRRotateModel : MonoBehaviour
{
    public float rotationSpeed = 100f;

    void Update()
    {
        InputDevice rightHand =
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        Vector2 joystick;

        if (rightHand.TryGetFeatureValue(
            CommonUsages.primary2DAxis,
            out joystick))
        {
            transform.Rotate(
                Vector3.up,
                joystick.x * rotationSpeed * Time.deltaTime
            );
        }
    }
}