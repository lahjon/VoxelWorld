using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    bool isMovementPressed;
    float rotationFactorPerFrame = 1;
    float allowPlayerRotation = 0.1f;
    float desiredRotationSpeed = 0.1f;
    float startAnimTime = 0.3f;
    float stopAnimTime = 0.15f;
    float speed;
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        Debug.Log(playerInput);

        playerInput.Player.Move.started += OnMovementInput;
        playerInput.Player.Move.performed += OnMovementInput;
        playerInput.Player.Move.canceled += OnMovementInput;
    }
    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void HandleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;

        Quaternion currentRotation = transform.rotation;
        if (isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame);
        }

    }

    void HandleAnimation()
    {
        // bool isWalking = animator.GetBool("isWalking");
        // bool isRunning = animator.GetBool("isRunning");

        // if (isMovementPressed && !isWalking)
        // {
        //     animator.SetBool("isWalking", true);
        // }
        // else if (!isMovementPressed && !isWalking)
        // {
        //     animator.SetBool("isWalking", false);
        // }
        speed = currentMovement.sqrMagnitude;

        //Physically move player

		if (speed > allowPlayerRotation)
			animator.SetFloat ("Blend", speed, startAnimTime, Time.deltaTime);
		else if (speed < allowPlayerRotation)
			animator.SetFloat ("Blend", speed, stopAnimTime, Time.deltaTime);
    }
    void OnEnable()
    {
        playerInput.Player.Enable();
    }
    void OnDisable()
    {
        playerInput.Player.Disable();
    }
    void Update()
    {
        characterController.Move(currentMovement * Time.deltaTime * 10);
        HandleRotation();
        HandleAnimation();
    }
}
