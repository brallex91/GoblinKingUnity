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

    public Transform theCamera;
    public float rangeCameraViewAngle = 30f;
    private float targetCameraViewAngle;
    private bool isRangeView;

    private void Awake()
    {
        instance = this;
        moveTarget = transform.position;
    }

    private void Start()
    {
        targetCameraViewAngle = 45f;
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

        if (isRangeView == false)
        {
            targetRotation = (90f * currentAngle) + 45f;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, targetRotation, 0f), rotateSpeed * Time.deltaTime);

        theCamera.localRotation = Quaternion.Slerp(theCamera.localRotation, Quaternion.Euler(targetCameraViewAngle, 0f, 0f), rotateSpeed * Time.deltaTime);
    }

    public void SetMoveTarget(Vector3 newTarget)
    {
        moveTarget = newTarget;

        targetCameraViewAngle = 45f;
        isRangeView = false;
    }

    public void SetRangeView()
    {
        moveTarget = GameManager.instance.activePlayer.transform.position;

        targetRotation = GameManager.instance.activePlayer.transform.rotation.eulerAngles.y;

        targetCameraViewAngle = rangeCameraViewAngle;

        isRangeView = true;
    }
}
