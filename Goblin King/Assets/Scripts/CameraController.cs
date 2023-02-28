using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public float moveSpeed, manualMoveSpeed = 15f;
    private Vector3 moveTarget;
    private Vector2 moveInput;

    private float targetRotation;
    private float currentAngle;
    public float rotateSpeed;

    private void Awake()
    {
        instance = this;
        moveTarget = transform.position;
    }

    private void Update()
    {
        if (moveTarget != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
        }

        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");
        moveInput.Normalize();

        if (moveInput != Vector2.zero)
        {
            transform.position += ((transform.forward * (moveInput.y * manualMoveSpeed)) + (transform.right * (moveInput.x * manualMoveSpeed))) * Time.deltaTime;

            moveTarget = transform.position;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetMoveTarget(GameManager.instance.activePlayer.transform.position);
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            currentAngle++;

            if (currentAngle >= 4)
            {
                currentAngle = 0;
            }
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            currentAngle--;

            if (currentAngle < 0)
            {
                currentAngle = 3;
            }
        }

        targetRotation = (90f * currentAngle) + 45f;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, targetRotation, 0f), rotateSpeed * Time.deltaTime);
    }

    public void SetMoveTarget(Vector3 newTarget)
    {
        moveTarget = newTarget;
    }
}
