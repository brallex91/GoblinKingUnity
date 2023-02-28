using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public float rotateSpeed;

    private void Update()
    {
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + (rotateSpeed * Time.deltaTime), 0f);
    }
}
