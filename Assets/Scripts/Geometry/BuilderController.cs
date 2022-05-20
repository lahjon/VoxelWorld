using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuilderController : MonoBehaviour
{
    [HideInInspector] public PlayerInput playerInput;
    CharacterController characterController;
    Vector2 currentMovementInput, mouseMovement;
    Vector3 currentMovement;
    bool invertY = true;
    bool isMovementPressed;
    public bool isLMBPressed;
    public bool isShiftPressed => Keyboard.current.leftShiftKey.isPressed;
    public bool isCtrlPressed => Keyboard.current.leftCtrlKey.isPressed;
    public bool isRMBDown => Mouse.current != null ? Mouse.current.rightButton.isPressed : false;
    public bool isRMBUp => Mouse.current != null ? !Mouse.current.rightButton.isPressed : false;
    public float positionLerpTime = 0.2f;
    public float rotationLerpTime = 0.2f;
    public float boostMultiplier = 2f;
    public float mouseSensitivityFactor = 1.5f;
    public float speed = 50f;
    CameraState targetCameraState = new CameraState();
    CameraState interpolatingCameraState = new CameraState();
    VoxelManager voxelManager;
    public System.Action buildCommandStart;
    public System.Action buildCommandInProgess;
    public System.Action buildCommandEnd;
    BuildCommand _buildCommand;
    public BuildCommand BuildCommand
    {
        get => _buildCommand;
        set
        {
            _buildCommand = value;
            switch (_buildCommand)
            {
                case BuildCommand.Add:
                    buildCommandStart = null;
                    buildCommandInProgess = voxelManager.PerformAddingVoxels;
                    buildCommandEnd = voxelManager.StopAddingVoxels;
                    break;
                case BuildCommand.Erase:
                    buildCommandStart = voxelManager.StartRemovingVoxels;
                    buildCommandInProgess = voxelManager.PerformRemoveVoxels;
                    buildCommandEnd = voxelManager.StopRemovingVoxels;
                    break;
                case BuildCommand.DrawLine:
                    buildCommandStart = voxelManager.PerformDrawLine;
                    buildCommandInProgess = null;
                    buildCommandEnd = null;
                    break;
                case BuildCommand.RemoveLine:
                    buildCommandStart = voxelManager.PerformRemoveLine;
                    buildCommandInProgess = null;
                    buildCommandEnd = null;
                    break;
                case BuildCommand.Extrude:
                    buildCommandStart = voxelManager.PerformExtrude;
                    buildCommandInProgess = null;
                    buildCommandEnd = null;
                    break;
                case BuildCommand.PullIn:
                    buildCommandStart = voxelManager.PerformPullIn;
                    buildCommandInProgess = null;
                    buildCommandEnd = null;
                    break;
                case BuildCommand.PaintColor:
                    buildCommandStart = null;
                    buildCommandInProgess = voxelManager.PerformPaintColor;
                    buildCommandEnd = voxelManager.StopPaintingVoxels;
                    break;
                case BuildCommand.SampleColor:
                    buildCommandStart = voxelManager.SampleColor;
                    buildCommandInProgess = null;
                    buildCommandEnd = null;
                    break;
                case BuildCommand.FillColor:
                    buildCommandStart = voxelManager.PerformFillColor;
                    buildCommandInProgess = null;
                    buildCommandEnd = null;
                    break;
                case BuildCommand.BoxDragAdd:
                    buildCommandStart = voxelManager.StartBoxDragAdd;
                    buildCommandInProgess = voxelManager.PerformBoxDragAdd;
                    buildCommandEnd = voxelManager.StopBoxDragAdd;
                    break;
                case BuildCommand.BoxDragRemove:
                    buildCommandStart = voxelManager.StartBoxDragRemove;
                    buildCommandInProgess = voxelManager.PerformBoxDragRemove;
                    buildCommandEnd = voxelManager.StopBoxDragRemove;
                    break;
                case BuildCommand.BoxDragSelect:
                    buildCommandStart = voxelManager.StartBoxDragSelect;
                    buildCommandInProgess = voxelManager.PerformBoxDragSelect;
                    buildCommandEnd = voxelManager.StopBoxDragSelect;
                    break;
                default:
                    buildCommandStart = null;
                    buildCommandInProgess = null;
                    buildCommandEnd = null;
                    break;
            }
        }
    }
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        voxelManager = VoxelManager.instance;
        playerInput = voxelManager.playerInput;
        targetCameraState.SetFromTransform(transform);
        interpolatingCameraState.SetFromTransform(transform);

        playerInput.Builder.Move.started += OnMovementInput;
        playerInput.Builder.Move.performed += OnMovementInput;
        playerInput.Builder.Move.canceled += OnMovementInput;
        playerInput.Builder.Look.started += OnLook;
        playerInput.Builder.Look.canceled += OnLook;
        playerInput.Builder.PrimaryAction.started += PrimaryActionPressed;
        playerInput.Builder.PrimaryAction.canceled += PrimaryActionReleased;
    }
    void Start()
    {
        BuildCommand = BuildCommand.Add;
    }

    void PrimaryActionPressed(InputAction.CallbackContext context)
    {
        isLMBPressed = true;
        buildCommandStart?.Invoke();
    }
    void PrimaryActionHeld()
    {
        buildCommandInProgess?.Invoke();
    }
    void PrimaryActionReleased(InputAction.CallbackContext context)
    {
        isLMBPressed = false;
        buildCommandEnd?.Invoke();
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
        mouseMovement = context.ReadValue<Vector2>();
    }

    void Rotate()
    {
        if (invertY) mouseMovement.y = -mouseMovement.y;

        targetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
        targetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
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
        if (isRMBDown)
        {
            Rotate();
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (isLMBPressed)
        {
            PrimaryActionHeld();
        }
        Vector3 translation = isShiftPressed ? currentMovement * Time.deltaTime * speed * boostMultiplier : currentMovement * speed * Time.deltaTime;

        float positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
        float rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
        interpolatingCameraState.LerpTowards(targetCameraState, positionLerpPct, rotationLerpPct);
        targetCameraState.Translate(translation);
        
        //interpolatingCameraState.UpdateRotation(Camera.main.transform);
        //characterController.Move(translation);

        interpolatingCameraState.UpdateTransform(transform);

        if (isRMBUp)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // debug
        if (Keyboard.current.escapeKey.isPressed)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; 
            #endif
        }

    }

}

public enum BuildCommand
{
    Add,
    Erase,
    DrawLine,
    RemoveLine,
    Extrude,
    PullIn,
    PaintColor,
    SampleColor,
    FillColor,
    BoxDragAdd,
    BoxDragRemove,
    BoxDragSelect,
    Move

}

class CameraState
{
    public float yaw;
    public float pitch;
    public float roll;
    public float x;
    public float y;
    public float z;

    public void SetFromTransform(Transform t)
    {
        pitch = t.eulerAngles.x;
        yaw = t.eulerAngles.y;
        roll = t.eulerAngles.z;
        x = t.position.x;
        y = t.position.y;
        z = t.position.z;
    }

    public void Translate(Vector3 translation)
    {
        Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

        x += rotatedTranslation.x;
        y += rotatedTranslation.y;
        z += rotatedTranslation.z;
    }

    public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
    {
        yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
        pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
        roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);
        
        x = Mathf.Lerp(x, target.x, positionLerpPct);
        y = Mathf.Lerp(y, target.y, positionLerpPct);
        z = Mathf.Lerp(z, target.z, positionLerpPct);
    }

    public void UpdateTransform(Transform t)
    {
        t.eulerAngles = new Vector3(pitch, yaw, roll);
        t.position = new Vector3(x, y, z);
    }

    public void UpdateRotation(Transform t)
    {
        t.eulerAngles = new Vector3(pitch, yaw, roll);
    }
}
