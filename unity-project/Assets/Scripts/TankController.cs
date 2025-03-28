using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Main tank controller that handles movement, rotation, and input
/// </summary>
public class TankController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float turretRotationSpeed = 3f;
    [SerializeField] private float accelerationTime = 0.5f;
    [SerializeField] private float decelerationTime = 0.3f;
    [SerializeField] private Transform turretTransform;

    [Header("Physics Settings")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private float slopeLimit = 45f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem exhaustParticles;
    [SerializeField] private AudioSource engineAudioSource;
    [SerializeField] private float minEnginePitch = 0.5f;
    [SerializeField] private float maxEnginePitch = 1.2f;

    // Internal variables
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float currentSpeed;
    private float speedVelocity; // For smoothing
    private Rigidbody tankRigidbody;
    private bool isGrounded;
    private Vector3 groundNormal;
    private float originalDrag;
    private bool isFiring;

    private void Awake()
    {
        tankRigidbody = GetComponent<Rigidbody>();
        originalDrag = tankRigidbody.drag;
        
        if (turretTransform == null)
        {
            turretTransform = transform.Find("Turret");
            if (turretTransform == null)
            {
                Debug.LogError("Turret transform not assigned and not found!");
            }
        }
    }

    private void Update()
    {
        // Check if grounded
        CheckGrounded();
        
        // Handle tank rotation based on movement input
        RotateTank();
        
        // Handle turret rotation (look input)
        RotateTurret();
        
        // Update effects
        UpdateEffects();
    }

    private void FixedUpdate()
    {
        // Apply movement
        MoveTank();
        
        // Apply gravity and handle slope physics
        ApplyGravity();
    }

    private void CheckGrounded()
    {
        // Perform a raycast to check if we're grounded
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer);
        
        if (isGrounded)
        {
            groundNormal = hit.normal;
            
            // Adjust drag based on slope
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            if (slopeAngle > slopeLimit)
            {
                tankRigidbody.drag = originalDrag * 0.1f; // Reduce drag on steep slopes
            }
            else
            {
                tankRigidbody.drag = originalDrag;
            }
        }
        else
        {
            groundNormal = Vector3.up;
            tankRigidbody.drag = originalDrag * 0.1f; // Reduce drag in air
        }
    }

    private void MoveTank()
    {
        // Calculate target speed based on input
        float targetSpeed = moveInput.y * moveSpeed;
        
        // Smooth acceleration/deceleration
        float accelerationRate = (targetSpeed > currentSpeed) ? accelerationTime : decelerationTime;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, accelerationRate);
        
        // Calculate move direction based on tank's forward direction
        Vector3 moveDirection = transform.forward * currentSpeed;
        
        // Project movement onto ground plane when on slopes
        if (isGrounded && groundNormal != Vector3.up)
        {
            moveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal);
        }
        
        // Apply movement to rigidbody
        tankRigidbody.velocity = new Vector3(moveDirection.x, tankRigidbody.velocity.y, moveDirection.z);
    }

    private void RotateTank()
    {
        // Rotate the tank based on horizontal input
        float rotation = moveInput.x * rotationSpeed * Time.deltaTime;
        transform.Rotate(0, rotation, 0);
    }

    private void RotateTurret()
    {
        if (lookInput.sqrMagnitude > 0.01f && turretTransform != null)
        {
            // Calculate the target rotation based on look input
            float angle = Mathf.Atan2(lookInput.x, lookInput.y) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);
            
            // Smoothly rotate the turret
            turretTransform.localRotation = Quaternion.Slerp(
                turretTransform.localRotation,
                targetRotation,
                turretRotationSpeed * Time.deltaTime
            );
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            // Apply enhanced gravity when in air
            tankRigidbody.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    private void UpdateEffects()
    {
        // Adjust engine sound based on speed
        if (engineAudioSource != null)
        {
            float speedFactor = Mathf.Abs(currentSpeed) / moveSpeed;
            engineAudioSource.pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, speedFactor);
            engineAudioSource.volume = Mathf.Lerp(0.5f, 1.0f, speedFactor * 0.5f + 0.5f);
        }
        
        // Control exhaust particles based on movement
        if (exhaustParticles != null)
        {
            var emission = exhaustParticles.emission;
            if (Mathf.Abs(currentSpeed) > 0.1f)
            {
                if (!exhaustParticles.isEmitting)
                {
                    exhaustParticles.Play();
                }
                emission.rateOverTime = Mathf.Lerp(5, 20, Mathf.Abs(currentSpeed) / moveSpeed);
            }
            else
            {
                if (exhaustParticles.isEmitting)
                {
                    exhaustParticles.Stop();
                }
            }
        }
    }

    // Input handling methods (called by the New Input System)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnFire(InputValue value)
    {
        isFiring = value.isPressed;
        
        // Trigger weapon system if implemented
        WeaponSystem weaponSystem = GetComponent<WeaponSystem>();
        if (weaponSystem != null && isFiring)
        {
            weaponSystem.FirePrimaryWeapon();
        }
    }

    public void OnSecondaryFire(InputValue value)
    {
        if (value.isPressed)
        {
            // Trigger secondary weapon system if implemented
            WeaponSystem weaponSystem = GetComponent<WeaponSystem>();
            if (weaponSystem != null)
            {
                weaponSystem.FireSecondaryWeapon();
            }
        }
    }
} 