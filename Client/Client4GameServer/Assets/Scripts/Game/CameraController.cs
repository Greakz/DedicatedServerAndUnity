using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerManager player;

    public float HorizontalSensitivity = 100f;
    public float VerticalSensitivity = 100f;

    private float watchAngle = 45.0f;
    private float cameraDistance = 4.0f;

    private float verticalRotation;
    private float horizontalRotation;

    private void Start()
    {
        verticalRotation = transform.localEulerAngles.x;
        horizontalRotation = player.transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Look();
        Debug.DrawRay(transform.position, transform.forward * cameraDistance, Color.blue);
        Debug.DrawRay(player.transform.position, (player.transform.up * 1), Color.green);
    }

    private void Look()
    {

        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (Input.mouseScrollDelta.y != 0)
            {
                cameraDistance = Mathf.Clamp(cameraDistance - (Input.mouseScrollDelta.y * 0.2f), 1f, 15f);
            }

            // Rotate Player Horizontal
            float _mouseHorizontal = Input.GetAxis("Mouse X");
            horizontalRotation += _mouseHorizontal * HorizontalSensitivity * Time.deltaTime;
            player.transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);

            // Rotate Camera Vertical
            float _mouseVertical = Input.GetAxis("Mouse Y") * (VerticalSensitivity / 100.0f);
            Vector3 playerEye = (player.transform.position + (Vector3.up * 1.5f));
            Debug.DrawRay(playerEye, (player.transform.forward * 1), Color.red);


            watchAngle = Mathf.Clamp(watchAngle + _mouseVertical, -45.0f, 89.0f);


            Vector3 playerPointToCamera = Quaternion.AngleAxis(watchAngle, player.transform.right) *
                                          (-cameraDistance * player.transform.forward.normalized);

            this.gameObject.transform.position = playerEye + playerPointToCamera;

            this.gameObject.transform.LookAt(playerEye);

            Debug.Log(this.watchAngle);
        } else if (Input.GetMouseButton(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}