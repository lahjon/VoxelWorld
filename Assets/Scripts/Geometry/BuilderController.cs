using UnityEngine;
using UnityEngine.InputSystem;

public class BuilderController : MonoBehaviour
{
    [HideInInspector] public PlayerInput playerInput;
    CharacterController characterController;
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    bool isMovementPressed;
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = VoxelManager.instance.playerInput;

        playerInput.Builder.Move.started += OnMovementInput;
        playerInput.Builder.Move.performed += OnMovementInput;
        playerInput.Builder.Move.canceled += OnMovementInput;
        playerInput.Builder.Look.started += OnLook;
        playerInput.Builder.Look.canceled += OnLook;
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void OnLook(InputAction.CallbackContext context)
    {
        var v = context.ReadValue<Vector2>();
        Debug.Log(v);
    }

    void OnEnable()
    {
        playerInput.Builder.Enable();
    }
    void OnDisable()
    {
        playerInput.Builder.Disable();
    }
    void Update()
    {
        characterController.Move(currentMovement);
    }

}
