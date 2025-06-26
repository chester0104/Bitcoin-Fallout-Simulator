using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSens = 100f;
    public float pitchMin = -90f;
    public float pitchMax = 90f;
    Transform playerBody;
    float pitch;

    void Start()
    {
        playerBody = transform.parent.transform;
        SetCursorState(false);
    }

    void Update()
    {
        // Only process mouse look when game is not paused
        if (!PauseMenuBehavior.isGamePaused && Time.timeScale > 0)
        {
            float moveX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
            float moveY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;

            // yaw at the player
            if (playerBody)
                playerBody.Rotate(Vector3.up * moveX);

            // pitch at the camera
            pitch -= moveY;
            pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
            transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }
    }

    public void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}