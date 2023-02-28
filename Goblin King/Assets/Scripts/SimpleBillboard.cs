using UnityEngine;

public class SimpleBillboard : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.rotation = CameraController.instance.transform.rotation;
    }
}
