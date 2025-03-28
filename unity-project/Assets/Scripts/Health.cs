using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles health, damage, and destruction
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isInvulnerable = false;
    [SerializeField] private float invulnerabilityTime = 0.5f;
    
    [Header("Armor Settings")]
    [SerializeField] private float armorValue = 0f;
    [SerializeField] private float armorDamageReduction = 0.5f;
    [SerializeField] private bool hasDirectionalArmor = false;
    [SerializeField] private float frontArmorMultiplier = 0.75f;
    [SerializeField] private float rearArmorMultiplier = 1.5f;
    [SerializeField] private float sideArmorMultiplier = 1.0f;
    [SerializeField] private float topArmorMultiplier = 1.25f;
    
    [Header("Destruction")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 2f;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private bool dropItemOnDeath = false;
    [SerializeField] private GameObject[] dropItems;
    [SerializeField] private float dropChance = 0.5f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject damageEffect;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private Renderer[] damageRenderers;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float flashTime = 0.1f;
    
    [Header("Events")]
    public UnityEvent OnDeath;
    public UnityEvent<float, float> OnHealthChanged;
    public UnityEvent<float> OnDamaged;
    public UnityEvent<float> OnHealed;
    
    // Internal variables
    private Material[] originalMaterials;
    private bool isDead = false;
    private bool isInvulnerabilityActive = false;
    
    private void Awake()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Store original materials if using damage flashing
        if (damageRenderers.Length > 0 && damageMaterial != null)
        {
            originalMaterials = new Material[damageRenderers.Length];
            for (int i = 0; i < damageRenderers.Length; i++)
            {
                if (damageRenderers[i] != null)
                {
                    originalMaterials[i] = damageRenderers[i].material;
                }
            }
        }
    }
    
    private void Start()
    {
        // Notify listeners of initial health
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Apply damage to the health component
    /// </summary>
    /// <param name="damageAmount">Amount of raw damage</param>
    /// <param name="damageDirection">Optional direction of damage for directional armor</param>
    /// <returns>Actual damage applied</returns>
    public float TakeDamage(float damageAmount, Vector3 damageDirection = default)
    {
        // Check if we can take damage
        if (isDead || isInvulnerable || isInvulnerabilityActive)
        {
            return 0f;
        }
        
        // Calculate actual damage after armor reduction
        float actualDamage = CalculateDamage(damageAmount, damageDirection);
        
        // Apply damage
        currentHealth -= actualDamage;
        
        // Clamp health to 0
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        
        // Notify listeners
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamaged?.Invoke(actualDamage);
        
        // Show damage effects
        if (actualDamage > 0)
        {
            ShowDamageEffects();
        }
        
        // Check for death
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
        
        // Start brief invulnerability
        if (invulnerabilityTime > 0)
        {
            StartCoroutine(InvulnerabilityCoroutine());
        }
        
        return actualDamage;
    }
    
    /// <summary>
    /// Calculate actual damage based on armor and direction
    /// </summary>
    private float CalculateDamage(float damageAmount, Vector3 damageDirection)
    {
        // No armor, return full damage
        if (armorValue <= 0)
        {
            return damageAmount;
        }
        
        // Calculate armor reduction
        float damageReduction = armorValue * armorDamageReduction;
        float modifiedDamage = damageAmount;
        
        // Apply directional armor if applicable
        if (hasDirectionalArmor && damageDirection != default)
        {
            // Normalize damage direction relative to this object
            Vector3 localDirection = transform.InverseTransformDirection(damageDirection.normalized);
            
            // Determine which face was hit based on the largest component
            float armorMultiplier = 1.0f;
            
            if (Mathf.Abs(localDirection.z) > Mathf.Abs(localDirection.x) && Mathf.Abs(localDirection.z) > Mathf.Abs(localDirection.y))
            {
                // Front or rear hit
                armorMultiplier = localDirection.z > 0 ? rearArmorMultiplier : frontArmorMultiplier;
            }
            else if (Mathf.Abs(localDirection.y) > Mathf.Abs(localDirection.x))
            {
                // Top or bottom hit
                armorMultiplier = localDirection.y > 0 ? topArmorMultiplier : sideArmorMultiplier;
            }
            else
            {
                // Side hit
                armorMultiplier = sideArmorMultiplier;
            }
            
            // Apply directional multiplier to damage
            modifiedDamage *= armorMultiplier;
        }
        
        // Apply armor reduction
        modifiedDamage = Mathf.Max(1, modifiedDamage - damageReduction);
        
        return modifiedDamage;
    }
    
    /// <summary>
    /// Heal the health component
    /// </summary>
    public void Heal(float healAmount)
    {
        if (isDead)
            return;
            
        float oldHealth = currentHealth;
        currentHealth += healAmount;
        
        // Clamp to max health
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        // Calculate actual healing
        float actualHeal = currentHealth - oldHealth;
        
        // Notify listeners if healing occurred
        if (actualHeal > 0)
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnHealed?.Invoke(actualHeal);
        }
    }
    
    /// <summary>
    /// Handle death
    /// </summary>
    private void Die()
    {
        if (isDead)
            return;
            
        isDead = true;
        
        // Trigger death event
        OnDeath?.Invoke();
        
        // Spawn death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }
        
        // Play death sound
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }
        
        // Handle item drops
        if (dropItemOnDeath && dropItems.Length > 0 && Random.value <= dropChance)
        {
            int randomIndex = Random.Range(0, dropItems.Length);
            if (dropItems[randomIndex] != null)
            {
                Instantiate(dropItems[randomIndex], transform.position, Quaternion.identity);
            }
        }
        
        // Destroy the game object if configured
        if (destroyOnDeath)
        {
            // Disable components instead of destroying immediately
            DisableComponents();
            
            // Destroy with delay
            Destroy(gameObject, destroyDelay);
        }
    }
    
    /// <summary>
    /// Disable components on death
    /// </summary>
    private void DisableComponents()
    {
        // Disable renderers
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
        
        // Disable colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
        
        // Disable rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Disable scripts that might affect gameplay
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            // Don't disable this script
            if (script != this)
            {
                script.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// Show visual feedback for taking damage
    /// </summary>
    private void ShowDamageEffects()
    {
        // Spawn damage effect
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, transform.rotation);
        }
        
        // Play damage sound
        if (damageSound != null)
        {
            AudioSource.PlayClipAtPoint(damageSound, transform.position);
        }
        
        // Flash damage material
        if (damageRenderers.Length > 0 && damageMaterial != null)
        {
            StartCoroutine(FlashDamageMaterial());
        }
    }
    
    /// <summary>
    /// Flash the damage material briefly
    /// </summary>
    private IEnumerator FlashDamageMaterial()
    {
        // Apply damage material
        for (int i = 0; i < damageRenderers.Length; i++)
        {
            if (damageRenderers[i] != null)
            {
                damageRenderers[i].material = damageMaterial;
            }
        }
        
        // Wait for flash time
        yield return new WaitForSeconds(flashTime);
        
        // Restore original materials
        for (int i = 0; i < damageRenderers.Length; i++)
        {
            if (damageRenderers[i] != null && i < originalMaterials.Length && originalMaterials[i] != null)
            {
                damageRenderers[i].material = originalMaterials[i];
            }
        }
    }
    
    /// <summary>
    /// Provide brief invulnerability after being hit
    /// </summary>
    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerabilityActive = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerabilityActive = false;
    }
    
    /// <summary>
    /// Set maximum health and optionally heal to full
    /// </summary>
    public void SetMaxHealth(float newMaxHealth, bool fullHeal = false)
    {
        maxHealth = newMaxHealth;
        
        if (fullHeal)
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        else if (currentHealth > maxHealth)
        {
            // Clamp current health to new max
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
    
    /// <summary>
    /// Get current health percentage (0-1)
    /// </summary>
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
    
    /// <summary>
    /// Check if the entity is dead
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
    
    /// <summary>
    /// Set invulnerability status
    /// </summary>
    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }
    
    /// <summary>
    /// Get current health value
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    /// <summary>
    /// Get maximum health value
    /// </summary>
    public float GetMaxHealth()
    {
        return maxHealth;
    }
} 