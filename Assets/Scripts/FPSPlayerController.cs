using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSPlayerController : MonoBehaviour
{
    public float speed = 10f;
    public float jumpHeight = 0.4f;
    public float gravity = 9.81f;
    public float airControl = 10f;
    public float dashingPower = 10f;
    public float dashingTime = 0.5f;
    public float dashingCooldown = 0.5f;
    public AudioClip dashSFX;

    private Vector3 input;
    private Vector3 moveDirection;
    private CharacterController characterController;

    private bool hasDoubleJump = true;
    private bool isDashing = false;
    private bool canDash = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (isDashing)
            return;

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        input = (transform.right * moveHorizontal + transform.forward * moveVertical).normalized;

        if (characterController.isGrounded)
        {
            hasDoubleJump = true;
            moveDirection = input;
            moveDirection.y = -1f; // Stick to ground

            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = Mathf.Sqrt(2 * jumpHeight * gravity);
            }
        }
        else
        {
            // Handle air movement
            input.y = moveDirection.y;
            moveDirection = Vector3.Lerp(moveDirection, input, airControl * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && hasDoubleJump)
            {
                moveDirection.y = Mathf.Sqrt(2 * jumpHeight * gravity);
                hasDoubleJump = false;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            {
                StartCoroutine(Dash());
            }
        }

        // Apply gravity
        moveDirection.y -= gravity * Time.deltaTime;

        // Move player
        characterController.Move(moveDirection * speed * Time.deltaTime);
    }

    public void IncreaseSpeed(float speedIncrease)
    {
        speed += speedIncrease;
    }
    
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        Vector3 dashDirection = input.magnitude > 0.1f ? input : transform.forward;

        // Flatten the dash direction (remove vertical component)
        dashDirection.y = 0f;
        dashDirection.Normalize();

        if (dashSFX)
            AudioSource.PlayClipAtPoint(dashSFX, transform.position);

        float timer = 0f;

        while (timer < dashingTime)
        {
            // Move only in the horizontal plane during dash
            characterController.Move(dashDirection * dashingPower * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

}
