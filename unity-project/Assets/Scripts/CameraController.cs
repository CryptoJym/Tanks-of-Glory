using UnityEngine;
using System.Collections;

/// <summary>
/// Camera controller that follows the player's tank with different view modes
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -8);
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float heightDamping = 2f;
    [SerializeField] private float distanceDamping = 2f;
    
    [Header("Collision Settings")]
    [SerializeField] private bool avoidWallClipping = true;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private LayerMask collisionLayers;
    
    [Header("View Modes")]
    [SerializeField] private ViewMode currentViewMode = ViewMode.ThirdPerson;
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0, 5, -8);
    [SerializeField] private Vector3 topDownOffset = new Vector3(0, 15, -2);
    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0, 2, 0);
    [SerializeField] private Transform turretTransform;
    
    [Header("Transition Settings")]
    [SerializeField] private float viewTransitionSpeed = 3f;
    [SerializeField] private bool isTransitioning = false;
    
    [Header("Camera Effects")]
    [SerializeField] private float cameraShakeAmount = 0.1f;
    [SerializeField] private float recoilAmount = 0.5f;
    [SerializeField] private float cameraFOV = 60f;
    [SerializeField] private float zoomFOV = 40f;
    
    // Internal variables
    private Vector3 targetPosition;
    private Vector3 currentOffset;
    private Vector3 targetOffset;
    private Camera mainCamera;
    private float originalFOV;
    private Vector3 recoilOffset = Vector3.zero;
    private Coroutine shakeCoroutine;
    private Coroutine zoomCoroutine;
    private Transform targetTurret;
    
    // Enums
    public enum ViewMode
    {
        ThirdPerson,
        TopDown,
        FirstPerson,
        FixedAngle
    }
    
    /// <summary>
    /// Initialize camera
    /// </summary>
    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        originalFOV = mainCamera.fieldOfView;
        currentOffset = thirdPersonOffset;
        targetOffset = thirdPersonOffset;
    }
    
    /// <summary>
    /// Find player if not set
    /// </summary>
    private void Start()
    {
        if (target == null && autoFindPlayer)
        {
            FindPlayer();
        }
        
        // Initialize camera position
        if (target != null)
        {
            UpdateCameraPosition(true);
        }
    }
    
    /// <summary>
    /// Update camera position each frame
    /// </summary>
    private void LateUpdate()
    {
        if (target == null)
        {
            if (autoFindPlayer)
            {
                FindPlayer();
            }
            return;
        }
        
        UpdateCameraPosition(false);
    }
    
    /// <summary>
    /// Find the player in the scene
    /// </summary>
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            target = player.transform;
            
            // Try to find turret transform for first person view
            turretTransform = player.transform.Find("Turret");
            targetTurret = turretTransform;
        }
    }
    
    /// <summary>
    /// Update the camera position based on current settings
    /// </summary>
    private void UpdateCameraPosition(bool immediate)
    {
        // Handle view transition
        if (isTransitioning)
        {
            currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * viewTransitionSpeed);
            
            // Check if transition is complete
            if (Vector3.Distance(currentOffset, targetOffset) < 0.01f)
            {
                currentOffset = targetOffset;
                isTransitioning = false;
            }
        }
        
        // Calculate target position based on view mode
        switch (currentViewMode)
        {
            case ViewMode.ThirdPerson:
                UpdateThirdPersonCamera(immediate);
                break;
                
            case ViewMode.TopDown:
                UpdateTopDownCamera(immediate);
                break;
                
            case ViewMode.FirstPerson:
                UpdateFirstPersonCamera(immediate);
                break;
                
            case ViewMode.FixedAngle:
                // Fixed angle doesn't move with the player
                break;
        }
        
        // Apply camera shake and recoil effects
        if (!immediate)
        {
            transform.position += recoilOffset;
        }
    }
    
    /// <summary>
    /// Update camera position for third person view
    /// </summary>
    private void UpdateThirdPersonCamera(bool immediate)
    {
        // Calculate target position
        Vector3 forward = target.forward * lookAheadDistance;
        Vector3 targetCenterPos = target.position + forward;
        
        // Calculate desired position
        Vector3 desiredPosition = targetCenterPos + target.TransformDirection(currentOffset);
        
        // Handle collision avoidance
        if (avoidWallClipping)
        {
            AdjustForCollisions(ref desiredPosition, targetCenterPos);
        }
        
        // Set position smoothly or immediately
        if (immediate)
        {
            transform.position = desiredPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
        }
        
        // Look at target
        Vector3 lookAtPosition = target.position + target.forward * lookAheadDistance;
        transform.LookAt(lookAtPosition);
    }
    
    /// <summary>
    /// Update camera position for top-down view
    /// </summary>
    private void UpdateTopDownCamera(bool immediate)
    {
        Vector3 desiredPosition = target.position + currentOffset;
        
        // Set position smoothly or immediately
        if (immediate)
        {
            transform.position = desiredPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
        }
        
        // Look directly down at target
        transform.LookAt(target.position);
    }
    
    /// <summary>
    /// Update camera position for first person view
    /// </summary>
    private void UpdateFirstPersonCamera(bool immediate)
    {
        // Use turret transform if available, otherwise use tank transform
        Transform viewTransform = (targetTurret != null) ? targetTurret : target;
        
        // Calculate position inside the turret/tank
        Vector3 desiredPosition = viewTransform.position + viewTransform.TransformDirection(currentOffset);
        
        // Set position smoothly or immediately
        if (immediate)
        {
            transform.position = desiredPosition;
            transform.rotation = viewTransform.rotation;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, viewTransform.rotation, Time.deltaTime * rotationSpeed);
        }
    }
    
    /// <summary>
    /// Adjust camera position to avoid clipping through walls
    /// </summary>
    private void AdjustForCollisions(ref Vector3 desiredPosition, Vector3 targetPosition)
    {
        // Direction from target to camera
        Vector3 direction = (desiredPosition - targetPosition).normalized;
        float targetDistance = Vector3.Distance(targetPosition, desiredPosition);
        
        // Cast a ray from target to desired camera position
        RaycastHit hit;
        if (Physics.Raycast(targetPosition, direction, out hit, targetDistance, collisionLayers))
        {
            // If hit something, adjust camera position
            float adjustedDistance = hit.distance - 0.5f; // Keep a small buffer
            
            // Ensure minimum distance
            adjustedDistance = Mathf.Max(adjustedDistance, minDistance);
            
            // Recalculate position
            desiredPosition = targetPosition + direction * adjustedDistance;
        }
    }
    
    /// <summary>
    /// Switch to a different view mode
    /// </summary>
    public void SetViewMode(ViewMode mode)
    {
        // Skip if already in this mode
        if (currentViewMode == mode && !isTransitioning)
            return;
            
        // Cancel any active zoom
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
            mainCamera.fieldOfView = originalFOV;
        }
        
        // Start transition
        isTransitioning = true;
        currentViewMode = mode;
        
        // Set target offset based on mode
        switch (mode)
        {
            case ViewMode.ThirdPerson:
                targetOffset = thirdPersonOffset;
                break;
                
            case ViewMode.TopDown:
                targetOffset = topDownOffset;
                break;
                
            case ViewMode.FirstPerson:
                targetOffset = firstPersonOffset;
                // Find turret if not already set
                if (targetTurret == null && target != null)
                {
                    targetTurret = target.Find("Turret");
                    if (targetTurret == null)
                    {
                        targetTurret = target; // Fallback to tank transform
                    }
                }
                break;
                
            case ViewMode.FixedAngle:
                // Fixed angle maintains current position
                targetOffset = currentOffset;
                break;
        }
    }
    
    /// <summary>
    /// Toggle through view modes
    /// </summary>
    public void CycleViewMode()
    {
        // Get next view mode
        ViewMode nextMode = currentViewMode;
        switch (currentViewMode)
        {
            case ViewMode.ThirdPerson:
                nextMode = ViewMode.TopDown;
                break;
                
            case ViewMode.TopDown:
                nextMode = ViewMode.FirstPerson;
                break;
                
            case ViewMode.FirstPerson:
                nextMode = ViewMode.ThirdPerson;
                break;
                
            default:
                nextMode = ViewMode.ThirdPerson;
                break;
        }
        
        SetViewMode(nextMode);
    }
    
    /// <summary>
    /// Apply camera shake effect
    /// </summary>
    public void ApplyShake(float intensity, float duration)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        shakeCoroutine = StartCoroutine(ShakeCamera(intensity, duration));
    }
    
    /// <summary>
    /// Apply recoil effect to camera
    /// </summary>
    public void ApplyRecoil(float amount)
    {
        // Apply recoil in the opposite direction of camera's forward view
        StartCoroutine(RecoilEffect(amount));
    }
    
    /// <summary>
    /// Shake camera for specified duration
    /// </summary>
    private IEnumerator ShakeCamera(float intensity, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Reduce intensity over time
            float currentIntensity = Mathf.Lerp(intensity, 0, elapsed / duration);
            
            // Apply random offset to camera
            Vector3 shakeOffset = Random.insideUnitSphere * currentIntensity * cameraShakeAmount;
            transform.position += shakeOffset;
            
            elapsed += Time.deltaTime;
            yield return null;
            
            // Remove the offset on next frame (will be reapplied if still shaking)
            transform.position -= shakeOffset;
        }
        
        shakeCoroutine = null;
    }
    
    /// <summary>
    /// Apply recoil effect
    /// </summary>
    private IEnumerator RecoilEffect(float amount)
    {
        // Apply backward recoil
        recoilOffset = -transform.forward * amount * recoilAmount;
        
        // Gradually reduce recoil
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Reduce recoil over time
            recoilOffset = Vector3.Lerp(-transform.forward * amount * recoilAmount, Vector3.zero, elapsed / duration);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        recoilOffset = Vector3.zero;
    }
    
    /// <summary>
    /// Toggle zoom view
    /// </summary>
    public void ToggleZoom(bool isZoomed)
    {
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }
        
        zoomCoroutine = StartCoroutine(ZoomEffect(isZoomed));
    }
    
    /// <summary>
    /// Zoom camera FOV
    /// </summary>
    private IEnumerator ZoomEffect(bool isZoomed)
    {
        float targetFOV = isZoomed ? zoomFOV : originalFOV;
        float startFOV = mainCamera.fieldOfView;
        float duration = 0.25f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.fieldOfView = targetFOV;
        zoomCoroutine = null;
    }
    
    /// <summary>
    /// Set a new target to follow
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        // Try to find turret for first-person view
        if (target != null)
        {
            targetTurret = target.Find("Turret");
        }
    }
    
    /// <summary>
    /// Set custom camera offsets
    /// </summary>
    public void SetCameraOffsets(Vector3 thirdPerson, Vector3 topDown, Vector3 firstPerson)
    {
        thirdPersonOffset = thirdPerson;
        topDownOffset = topDown;
        firstPersonOffset = firstPerson;
        
        // Update current offset if needed
        if (currentViewMode == ViewMode.ThirdPerson)
        {
            targetOffset = thirdPersonOffset;
        }
        else if (currentViewMode == ViewMode.TopDown)
        {
            targetOffset = topDownOffset;
        }
        else if (currentViewMode == ViewMode.FirstPerson)
        {
            targetOffset = firstPersonOffset;
        }
        
        // Start transition to new offset
        isTransitioning = true;
    }
} 