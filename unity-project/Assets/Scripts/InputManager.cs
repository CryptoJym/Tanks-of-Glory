using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input using Unity's new Input System
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("Action References")]
    [SerializeField] private InputActionAsset inputActions;
    
    [Header("Target Components")]
    [SerializeField] private TankController tankController;
    [SerializeField] private WeaponSystem weaponSystem;
    [SerializeField] private CameraController cameraController;
    
    // Input action references
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction fireAction;
    private InputAction secondaryFireAction;
    private InputAction reloadAction;
    private InputAction switchWeaponAction;
    private InputAction changeCameraAction;
    private InputAction toggleZoomAction;
    private InputAction pauseAction;
    
    // Internal state
    private bool isZoomed = false;
    private bool isPaused = false;
    
    private void Awake()
    {
        // Find components if not assigned
        if (tankController == null)
        {
            tankController = GetComponent<TankController>();
        }
        
        if (weaponSystem == null)
        {
            weaponSystem = GetComponent<WeaponSystem>();
        }
        
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }
        
        // Set up input actions
        if (inputActions != null)
        {
            // Get action maps
            var gameplayMap = inputActions.FindActionMap("Gameplay");
            
            // Get individual actions
            moveAction = gameplayMap.FindAction("Move");
            lookAction = gameplayMap.FindAction("Look");
            fireAction = gameplayMap.FindAction("Fire");
            secondaryFireAction = gameplayMap.FindAction("SecondaryFire");
            reloadAction = gameplayMap.FindAction("Reload");
            switchWeaponAction = gameplayMap.FindAction("SwitchWeapon");
            changeCameraAction = gameplayMap.FindAction("ChangeCamera");
            toggleZoomAction = gameplayMap.FindAction("ToggleZoom");
            pauseAction = gameplayMap.FindAction("Pause");
            
            // Set up callbacks
            moveAction.performed += OnMove;
            moveAction.canceled += OnMove;
            
            lookAction.performed += OnLook;
            lookAction.canceled += OnLook;
            
            fireAction.performed += OnFire;
            fireAction.canceled += OnFire;
            
            secondaryFireAction.performed += OnSecondaryFire;
            
            reloadAction.performed += OnReload;
            
            switchWeaponAction.performed += OnSwitchWeapon;
            
            changeCameraAction.performed += OnChangeCamera;
            
            toggleZoomAction.performed += OnToggleZoom;
            
            pauseAction.performed += OnPause;
        }
        else
        {
            Debug.LogError("Input actions asset not assigned to InputManager!");
        }
    }
    
    private void OnEnable()
    {
        // Enable the action map
        if (inputActions != null)
        {
            inputActions.FindActionMap("Gameplay").Enable();
        }
    }
    
    private void OnDisable()
    {
        // Disable the action map
        if (inputActions != null)
        {
            inputActions.FindActionMap("Gameplay").Disable();
        }
    }
    
    // Input callbacks
    
    private void OnMove(InputAction.CallbackContext context)
    {
        if (tankController != null)
        {
            // Forward input value to tank controller
            Vector2 inputValue = context.ReadValue<Vector2>();
            tankController.OnMove(new InputValue { Get = () => inputValue });
        }
    }
    
    private void OnLook(InputAction.CallbackContext context)
    {
        if (tankController != null)
        {
            // Forward input value to tank controller
            Vector2 inputValue = context.ReadValue<Vector2>();
            tankController.OnLook(new InputValue { Get = () => inputValue });
        }
    }
    
    private void OnFire(InputAction.CallbackContext context)
    {
        if (weaponSystem != null)
        {
            // Create input value with pressed state
            bool isPressed = context.phase == InputActionPhase.Performed;
            tankController.OnFire(new InputValue { isPressed = isPressed });
        }
    }
    
    private void OnSecondaryFire(InputAction.CallbackContext context)
    {
        if (weaponSystem != null)
        {
            // Create input value with pressed state
            bool isPressed = context.phase == InputActionPhase.Performed;
            tankController.OnSecondaryFire(new InputValue { isPressed = isPressed });
            
            // Apply camera effects for secondary fire
            if (cameraController != null && isPressed)
            {
                cameraController.ApplyShake(0.2f, 0.3f);
            }
        }
    }
    
    private void OnReload(InputAction.CallbackContext context)
    {
        if (weaponSystem != null)
        {
            // Reload weapons
            weaponSystem.ReloadPrimaryWeapon();
            weaponSystem.ReloadSecondaryWeapon();
        }
    }
    
    private void OnSwitchWeapon(InputAction.CallbackContext context)
    {
        // Switch weapon implementation would go here
        // This depends on how weapon switching is handled in your system
    }
    
    private void OnChangeCamera(InputAction.CallbackContext context)
    {
        if (cameraController != null)
        {
            // Cycle camera view mode
            cameraController.CycleViewMode();
        }
    }
    
    private void OnToggleZoom(InputAction.CallbackContext context)
    {
        if (cameraController != null)
        {
            // Toggle zoom state
            isZoomed = !isZoomed;
            cameraController.ToggleZoom(isZoomed);
        }
    }
    
    private void OnPause(InputAction.CallbackContext context)
    {
        // Toggle pause state
        isPaused = !isPaused;
        
        // Use game manager to handle pausing
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
    }
    
    // Additional methods
    
    /// <summary>
    /// Handle player taking damage for camera effects
    /// </summary>
    public void OnPlayerDamaged(float damageAmount)
    {
        if (cameraController != null)
        {
            // Scale shake based on damage amount
            float shakeIntensity = Mathf.Clamp01(damageAmount / 100f) * 0.5f;
            cameraController.ApplyShake(shakeIntensity, 0.3f);
        }
    }
    
    /// <summary>
    /// Reset input state (e.g., after respawn)
    /// </summary>
    public void ResetInputState()
    {
        isZoomed = false;
        
        if (cameraController != null)
        {
            cameraController.ToggleZoom(false);
        }
    }
}

// Custom InputValue class to simplify interfacing with TankController
// This matches the parameter type that TankController expects
public class InputValue
{
    // Delegate for getting vector values
    public System.Func<Vector2> Get;
    
    // Flag for button pressed state
    public bool isPressed;
} 