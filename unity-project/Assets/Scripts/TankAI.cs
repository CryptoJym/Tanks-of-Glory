using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls AI tank behavior and decision making
/// </summary>
public class TankAI : MonoBehaviour
{
    [System.Serializable]
    public enum AIBehaviorType
    {
        Patrol,
        Guard,
        Chase,
        Ambush,
        Flee
    }

    [Header("AI Settings")]
    [SerializeField] private AIBehaviorType behaviorType = AIBehaviorType.Chase;
    [SerializeField] private float detectionRange = 30f;
    [SerializeField] private float attackRange = 20f;
    [SerializeField] private float fleeHealthThreshold = 0.3f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private Transform turretTransform;
    [SerializeField] private float turretRotationSpeed = 3f;
    [SerializeField] private float fieldOfView = 90f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float minDistanceToTarget = 10f;
    [SerializeField] private float stoppingDistance = 5f;
    [SerializeField] private float waypointReachedDistance = 3f;

    [Header("Combat")]
    [SerializeField] private float firingCooldown = 2f;
    [SerializeField] private float aimErrorMargin = 5f;
    [SerializeField] private bool leadTarget = true;
    [SerializeField] private float maxPredictionTime = 2f;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 3f;
    [SerializeField] private bool randomPatrol = false;

    // Internal variables
    private NavMeshAgent navAgent;
    private WeaponSystem weaponSystem;
    private Health health;
    private Rigidbody tankRigidbody;
    private Transform currentTarget;
    private Vector3 lastKnownTargetPosition;
    private float lastFiringTime;
    private int currentPatrolIndex = 0;
    private bool isWaitingAtPatrolPoint = false;
    private AIBehaviorType originalBehavior;
    private bool targetIsVisible = false;

    private void Awake()
    {
        // Get components
        navAgent = GetComponent<NavMeshAgent>();
        weaponSystem = GetComponent<WeaponSystem>();
        health = GetComponent<Health>();
        tankRigidbody = GetComponent<Rigidbody>();
        
        // Find turret if not set
        if (turretTransform == null)
        {
            turretTransform = transform.Find("Turret");
        }
        
        // Configure NavMeshAgent
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.angularSpeed = rotationSpeed;
            navAgent.stoppingDistance = stoppingDistance;
            navAgent.updateRotation = true;
        }
        
        // Store original behavior
        originalBehavior = behaviorType;
    }

    private void Start()
    {
        // Handle health events
        if (health != null)
        {
            health.OnDamaged.AddListener(OnTankDamaged);
        }
        
        // Initialize based on behavior type
        InitializeBehavior();
    }

    private void Update()
    {
        // Update behavior based on health if needed
        UpdateBehaviorBasedOnHealth();
        
        // Execute current behavior
        switch (behaviorType)
        {
            case AIBehaviorType.Patrol:
                PatrolBehavior();
                break;
            case AIBehaviorType.Guard:
                GuardBehavior();
                break;
            case AIBehaviorType.Chase:
                ChaseBehavior();
                break;
            case AIBehaviorType.Ambush:
                AmbushBehavior();
                break;
            case AIBehaviorType.Flee:
                FleeBehavior();
                break;
        }
        
        // Always check for targets
        ScanForTargets();
        
        // Rotate turret if we have a target
        if (currentTarget != null)
        {
            RotateTurretTowardsTarget();
            
            // Check if we can fire
            if (CanFireAtTarget())
            {
                FireAtTarget();
            }
        }
    }

    /// <summary>
    /// Initialize behavior on start
    /// </summary>
    private void InitializeBehavior()
    {
        switch (behaviorType)
        {
            case AIBehaviorType.Patrol:
                if (patrolPoints.Length > 0)
                {
                    if (randomPatrol)
                    {
                        currentPatrolIndex = Random.Range(0, patrolPoints.Length);
                    }
                    
                    // Start patrolling
                    MoveToPoint(patrolPoints[currentPatrolIndex].position);
                }
                break;
                
            case AIBehaviorType.Guard:
                // Stay in place and watch for targets
                break;
                
            case AIBehaviorType.Ambush:
                // Hide and wait for targets
                break;
        }
    }
    
    /// <summary>
    /// Logic for patrolling between waypoints
    /// </summary>
    private void PatrolBehavior()
    {
        // Skip if no patrol points or waiting at point
        if (patrolPoints.Length == 0 || isWaitingAtPatrolPoint)
            return;
            
        // Check if reached current patrol point
        if (navAgent != null && !navAgent.pathPending)
        {
            if (navAgent.remainingDistance <= waypointReachedDistance)
            {
                StartCoroutine(WaitAtPatrolPoint());
            }
        }
        
        // If target is found during patrol, switch to chase
        if (currentTarget != null && targetIsVisible)
        {
            // Switch to chase behavior temporarily
            SwitchBehavior(AIBehaviorType.Chase);
        }
    }
    
    /// <summary>
    /// Logic for guarding an area
    /// </summary>
    private void GuardBehavior()
    {
        // If target is found while guarding, rotate towards but don't chase
        if (currentTarget != null && targetIsVisible)
        {
            // Rotate towards target but stay in place
            RotateTowardsTarget();
        }
    }
    
    /// <summary>
    /// Logic for chasing and engaging targets
    /// </summary>
    private void ChaseBehavior()
    {
        if (currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            
            // Update last known position
            if (targetIsVisible)
            {
                lastKnownTargetPosition = currentTarget.position;
            }
            
            // If target is visible and within range, move to optimal attack position
            if (targetIsVisible)
            {
                if (distanceToTarget > attackRange)
                {
                    // Move closer to target
                    MoveToPoint(currentTarget.position);
                }
                else if (distanceToTarget < minDistanceToTarget)
                {
                    // Back up if too close
                    Vector3 retreatPosition = transform.position + (transform.position - currentTarget.position).normalized * minDistanceToTarget;
                    MoveToPoint(retreatPosition);
                }
                else
                {
                    // At good position, stop and aim
                    StopMoving();
                    RotateTowardsTarget();
                }
            }
            else
            {
                // Target not visible, move to last known position
                MoveToPoint(lastKnownTargetPosition);
                
                // If reached last known position and target still not found, return to original behavior
                if (Vector3.Distance(transform.position, lastKnownTargetPosition) <= waypointReachedDistance)
                {
                    SwitchBehavior(originalBehavior);
                }
            }
        }
        else
        {
            // No target, return to original behavior
            SwitchBehavior(originalBehavior);
        }
    }
    
    /// <summary>
    /// Logic for ambushing targets
    /// </summary>
    private void AmbushBehavior()
    {
        // If target is found while ambushing, wait until in range then attack
        if (currentTarget != null && targetIsVisible)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            
            // Only engage if target is close enough for ambush
            if (distanceToTarget <= attackRange * 0.7f) // Closer range for ambush
            {
                // Switch to chase for active engagement
                SwitchBehavior(AIBehaviorType.Chase);
            }
        }
    }
    
    /// <summary>
    /// Logic for fleeing from threats
    /// </summary>
    private void FleeBehavior()
    {
        if (currentTarget != null)
        {
            // Find a position away from the target
            Vector3 fleeDirection = (transform.position - currentTarget.position).normalized;
            Vector3 fleePosition = transform.position + fleeDirection * 20f;
            
            // Ensure flee position is on navmesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleePosition, out hit, 10f, NavMesh.AllAreas))
            {
                MoveToPoint(hit.position);
            }
            
            // Check if health is recovered enough to stop fleeing
            if (health != null && health.GetHealthPercent() > fleeHealthThreshold * 1.5f)
            {
                // Return to original behavior
                SwitchBehavior(originalBehavior);
            }
        }
        else
        {
            // No threat, return to original behavior
            SwitchBehavior(originalBehavior);
        }
    }
    
    /// <summary>
    /// Wait at patrol point before moving to next one
    /// </summary>
    private IEnumerator WaitAtPatrolPoint()
    {
        isWaitingAtPatrolPoint = true;
        
        // Stop moving
        StopMoving();
        
        // Wait at the patrol point
        yield return new WaitForSeconds(patrolWaitTime);
        
        // Move to next patrol point
        MoveToNextPatrolPoint();
        
        isWaitingAtPatrolPoint = false;
    }
    
    /// <summary>
    /// Move to the next patrol point in sequence
    /// </summary>
    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
            return;
            
        // Update patrol index
        if (randomPatrol)
        {
            // Choose a random patrol point (different from current)
            int nextIndex = currentPatrolIndex;
            while (nextIndex == currentPatrolIndex && patrolPoints.Length > 1)
            {
                nextIndex = Random.Range(0, patrolPoints.Length);
            }
            currentPatrolIndex = nextIndex;
        }
        else
        {
            // Move to next point in sequence
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
        
        // Move to the next point
        MoveToPoint(patrolPoints[currentPatrolIndex].position);
    }
    
    /// <summary>
    /// Move to a specified point using NavMeshAgent
    /// </summary>
    private void MoveToPoint(Vector3 position)
    {
        if (navAgent != null && navAgent.isActiveAndEnabled)
        {
            navAgent.SetDestination(position);
            navAgent.isStopped = false;
        }
    }
    
    /// <summary>
    /// Stop moving
    /// </summary>
    private void StopMoving()
    {
        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }
    }
    
    /// <summary>
    /// Rotate towards current target
    /// </summary>
    private void RotateTowardsTarget()
    {
        if (currentTarget == null)
            return;
            
        // Get direction to target
        Vector3 targetDirection = (currentTarget.position - transform.position).normalized;
        targetDirection.y = 0; // Keep rotation on horizontal plane
        
        // Rotate towards target
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime * 0.1f);
        }
    }
    
    /// <summary>
    /// Rotate turret to face target
    /// </summary>
    private void RotateTurretTowardsTarget()
    {
        if (currentTarget == null || turretTransform == null)
            return;
            
        // Calculate target position with leading if enabled
        Vector3 targetPosition = currentTarget.position;
        if (leadTarget)
        {
            targetPosition = PredictTargetPosition();
        }
        
        // Get direction to target
        Vector3 targetDirection = (targetPosition - turretTransform.position).normalized;
        
        // Calculate target rotation (only around Y axis)
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Vector3 eulerAngles = targetRotation.eulerAngles;
        
        // Optional: Add aiming error
        if (aimErrorMargin > 0)
        {
            eulerAngles.y += Random.Range(-aimErrorMargin, aimErrorMargin);
        }
        
        // Apply rotation to turret
        Quaternion finalRotation = Quaternion.Euler(0, eulerAngles.y, 0);
        turretTransform.rotation = Quaternion.Slerp(turretTransform.rotation, finalRotation, turretRotationSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Predict target position for leading shots
    /// </summary>
    private Vector3 PredictTargetPosition()
    {
        if (currentTarget == null)
            return Vector3.zero;
            
        // Try to get target velocity
        Rigidbody targetRb = currentTarget.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            // Get projectile speed from weapon system
            float projectileSpeed = 50f; // Default value
            WeaponSystem.WeaponStats primaryWeapon = typeof(WeaponSystem).GetField("primaryWeapon", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(weaponSystem) as WeaponSystem.WeaponStats;
            if (primaryWeapon != null)
            {
                projectileSpeed = primaryWeapon.projectileSpeed;
            }
            
            // Calculate time to hit based on distance and projectile speed
            float distance = Vector3.Distance(turretTransform.position, currentTarget.position);
            float timeToHit = distance / projectileSpeed;
            
            // Limit prediction time
            timeToHit = Mathf.Min(timeToHit, maxPredictionTime);
            
            // Predict future position
            Vector3 futurePosition = currentTarget.position + targetRb.velocity * timeToHit;
            return futurePosition;
        }
        
        // Fallback to current position if no rigidbody
        return currentTarget.position;
    }
    
    /// <summary>
    /// Scan for potential targets
    /// </summary>
    private void ScanForTargets()
    {
        // Find potential targets within detection range
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, targetLayers);
        
        Transform bestTarget = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in colliders)
        {
            // Skip self
            if (col.transform == transform)
                continue;
                
            // Check if target is in field of view
            Vector3 directionToTarget = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);
            
            // Check visibility based on behavior type
            bool inFieldOfView = angle <= fieldOfView * 0.5f;
            bool isVisible = false;
            
            // Always check visibility for chase behavior, otherwise only if in FOV
            if (behaviorType == AIBehaviorType.Chase || inFieldOfView)
            {
                isVisible = IsTargetVisible(col.transform);
            }
            
            // Include target if visible or if in ambush mode (can sense targets outside FOV)
            if (isVisible || behaviorType == AIBehaviorType.Ambush)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = col.transform;
                    targetIsVisible = isVisible;
                }
            }
        }
        
        // Update current target
        currentTarget = bestTarget;
    }
    
    /// <summary>
    /// Check if target is visible (not blocked by obstacles)
    /// </summary>
    private bool IsTargetVisible(Transform target)
    {
        if (target == null)
            return false;
            
        // Cast ray to check for obstacles
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, directionToTarget, out hit, distanceToTarget, obstacleLayers))
        {
            // Something is blocking the view
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Check if we can fire at the current target
    /// </summary>
    private bool CanFireAtTarget()
    {
        if (currentTarget == null || !targetIsVisible)
            return false;
            
        // Check cooldown
        if (Time.time - lastFiringTime < firingCooldown)
            return false;
            
        // Check if target is within attack range
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        if (distanceToTarget > attackRange)
            return false;
            
        // Check if turret is facing target
        if (turretTransform != null)
        {
            Vector3 directionToTarget = (currentTarget.position - turretTransform.position).normalized;
            float angle = Vector3.Angle(turretTransform.forward, directionToTarget);
            
            // Only fire if aimed properly
            if (angle > aimErrorMargin * 2)
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Fire weapons at current target
    /// </summary>
    private void FireAtTarget()
    {
        if (weaponSystem != null)
        {
            bool fired = weaponSystem.FirePrimaryWeapon();
            if (fired)
            {
                lastFiringTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// Handle tank taking damage
    /// </summary>
    private void OnTankDamaged(float damageAmount)
    {
        // If in patrol or guard mode and damaged, start chasing attacker
        if ((behaviorType == AIBehaviorType.Patrol || behaviorType == AIBehaviorType.Guard) && 
            currentTarget == null)
        {
            // Try to find who damaged us
            ScanForTargets();
            
            // If found a target, switch to chase
            if (currentTarget != null)
            {
                SwitchBehavior(AIBehaviorType.Chase);
            }
        }
    }
    
    /// <summary>
    /// Update behavior based on health
    /// </summary>
    private void UpdateBehaviorBasedOnHealth()
    {
        if (health != null)
        {
            // Check if health is low enough to flee
            if (health.GetHealthPercent() <= fleeHealthThreshold && behaviorType != AIBehaviorType.Flee)
            {
                SwitchBehavior(AIBehaviorType.Flee);
            }
        }
    }
    
    /// <summary>
    /// Switch to a different behavior
    /// </summary>
    private void SwitchBehavior(AIBehaviorType newBehavior)
    {
        // Skip if already in this behavior
        if (behaviorType == newBehavior)
            return;
            
        // Store original behavior if switching to a temporary behavior
        if (newBehavior == AIBehaviorType.Chase || newBehavior == AIBehaviorType.Flee)
        {
            if (behaviorType != AIBehaviorType.Chase && behaviorType != AIBehaviorType.Flee)
            {
                originalBehavior = behaviorType;
            }
        }
        
        // Switch behavior
        behaviorType = newBehavior;
        
        // Initialize new behavior
        switch (newBehavior)
        {
            case AIBehaviorType.Patrol:
                if (patrolPoints.Length > 0)
                {
                    MoveToPoint(patrolPoints[currentPatrolIndex].position);
                }
                break;
                
            case AIBehaviorType.Flee:
                // Initial flee setup is handled in FleeBehavior
                break;
        }
    }
    
    /// <summary>
    /// Set patrol points at runtime
    /// </summary>
    public void SetPatrolPoints(Transform[] points)
    {
        patrolPoints = points;
        currentPatrolIndex = 0;
        
        // If in patrol mode, start with the first point
        if (behaviorType == AIBehaviorType.Patrol && points.Length > 0)
        {
            MoveToPoint(points[0].position);
        }
    }
    
    /// <summary>
    /// Set behavior at runtime
    /// </summary>
    public void SetBehavior(AIBehaviorType behavior)
    {
        SwitchBehavior(behavior);
    }
    
    /// <summary>
    /// Draw gizmos for visualization in editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw field of view
        Gizmos.color = Color.white;
        float halfFOV = fieldOfView * 0.5f;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;
        Gizmos.DrawRay(transform.position, leftRayDirection * detectionRange);
        Gizmos.DrawRay(transform.position, rightRayDirection * detectionRange);
    }
} 