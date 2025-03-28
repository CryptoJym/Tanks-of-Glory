using UnityEngine;

/// <summary>
/// Stores player-specific data and information
/// </summary>
public class PlayerInfo : MonoBehaviour
{
    [Header("Player Identity")]
    [SerializeField] private string playerName = "Commander";
    [SerializeField] private int playerIndex = 0;
    [SerializeField] private bool isAI = false;
    [SerializeField] private int teamIndex = 0;
    
    [Header("Player Stats")]
    [SerializeField] private int level = 1;
    [SerializeField] private int experiencePoints = 0;
    [SerializeField] private int kills = 0;
    [SerializeField] private int deaths = 0;
    [SerializeField] private int assists = 0;
    [SerializeField] private float damageDealt = 0f;
    [SerializeField] private float damageTaken = 0f;
    
    [Header("Tank Configuration")]
    [SerializeField] private string tankModel = "M1A2 Abrams";
    [SerializeField] private TankClass tankClass = TankClass.Medium;
    [SerializeField] private TankLoadout currentLoadout;
    
    // Events
    public delegate void PlayerStatChangedHandler(string statName, float newValue);
    public event PlayerStatChangedHandler OnStatChanged;
    
    public enum TankClass
    {
        Light,
        Medium,
        Heavy,
        Artillery
    }
    
    [System.Serializable]
    public class TankLoadout
    {
        public string loadoutName = "Default";
        public WeaponConfig primaryWeapon;
        public WeaponConfig secondaryWeapon;
        public ArmorConfig armorConfig;
        public EngineConfig engineConfig;
        public SpecialEquipment[] specialEquipment;
    }
    
    [System.Serializable]
    public class WeaponConfig
    {
        public string weaponName = "Main Cannon";
        public int weaponLevel = 1;
        public float damageModifier = 1.0f;
        public float rangeModifier = 1.0f;
        public float fireRateModifier = 1.0f;
        public float reloadTimeModifier = 1.0f;
        public AmmoType ammoType = AmmoType.Standard;
    }
    
    [System.Serializable]
    public class ArmorConfig
    {
        public string armorName = "Standard Armor";
        public int armorLevel = 1;
        public float frontArmorModifier = 1.0f;
        public float sideArmorModifier = 1.0f;
        public float rearArmorModifier = 1.0f;
        public float topArmorModifier = 1.0f;
        public float weightModifier = 1.0f;
    }
    
    [System.Serializable]
    public class EngineConfig
    {
        public string engineName = "Standard Engine";
        public int engineLevel = 1;
        public float speedModifier = 1.0f;
        public float accelerationModifier = 1.0f;
        public float fuelEfficiencyModifier = 1.0f;
    }
    
    [System.Serializable]
    public class SpecialEquipment
    {
        public string equipmentName = "None";
        public int equipmentLevel = 1;
        public EquipmentType equipmentType = EquipmentType.None;
        public float effectModifier = 1.0f;
    }
    
    public enum AmmoType
    {
        Standard,
        ArmorPiercing,
        HighExplosive,
        HEAT,
        Sabot
    }
    
    public enum EquipmentType
    {
        None,
        ImprovedOptics,
        EnhancedRadio,
        SmokeDischargers,
        RepairKit,
        FirstAidKit,
        FireExtinguisher,
        BoostModule,
        StealthModule,
        ScannerModule
    }
    
    private Health healthComponent;
    private TankController tankController;
    private WeaponSystem weaponSystem;
    
    // Properties
    public string PlayerName => playerName;
    public int PlayerIndex => playerIndex;
    public bool IsAI => isAI;
    public int TeamIndex => teamIndex;
    public int Level => level;
    public int Kills => kills;
    public int Deaths => deaths;
    public int Assists => assists;
    public string TankModel => tankModel;
    public TankClass CurrentTankClass => tankClass;
    
    private void Awake()
    {
        // Get references to other components
        healthComponent = GetComponent<Health>();
        tankController = GetComponent<TankController>();
        weaponSystem = GetComponent<WeaponSystem>();
        
        // Subscribe to health events
        if (healthComponent != null)
        {
            healthComponent.OnDeath.AddListener(OnPlayerDeath);
            healthComponent.OnDamaged.AddListener(OnPlayerDamaged);
        }
    }
    
    private void Start()
    {
        // Apply loadout to components
        if (currentLoadout != null)
        {
            ApplyTankLoadout();
        }
        
        // Register with game manager if not AI
        if (!isAI && GameManager.Instance != null)
        {
            // Any player-specific registration here
        }
    }
    
    /// <summary>
    /// Apply the current loadout to the tank components
    /// </summary>
    private void ApplyTankLoadout()
    {
        // Apply weapon stats
        if (weaponSystem != null)
        {
            // Code to apply weapon loadout to weapon system
            // This would need to access the primary and secondary weapon fields in the weapon system
            // which might require a public interface or events
        }
        
        // Apply armor stats to health component
        if (healthComponent != null)
        {
            // Apply armor modifiers to health/armor values
            float baseHealth = 100f * GetTankClassHealthMultiplier();
            float modifiedHealth = baseHealth;
            
            if (currentLoadout.armorConfig != null)
            {
                // Example: Apply armor level to health
                modifiedHealth *= (1 + (currentLoadout.armorConfig.armorLevel * 0.1f));
            }
            
            // Set final health value
            healthComponent.SetMaxHealth(modifiedHealth, true);
        }
        
        // Apply engine stats to movement controller
        if (tankController != null)
        {
            // Code to apply engine modifiers to tank controller
            // This would need a public interface in the tank controller to adjust speed values
        }
    }
    
    /// <summary>
    /// Get health multiplier based on tank class
    /// </summary>
    private float GetTankClassHealthMultiplier()
    {
        switch (tankClass)
        {
            case TankClass.Light:
                return 0.8f;
            case TankClass.Medium:
                return 1.0f;
            case TankClass.Heavy:
                return 1.5f;
            case TankClass.Artillery:
                return 0.7f;
            default:
                return 1.0f;
        }
    }
    
    /// <summary>
    /// Set player name
    /// </summary>
    public void SetPlayerName(string name)
    {
        playerName = name;
    }
    
    /// <summary>
    /// Set player index
    /// </summary>
    public void SetPlayerIndex(int index)
    {
        playerIndex = index;
    }
    
    /// <summary>
    /// Set team index for team-based modes
    /// </summary>
    public void SetTeamIndex(int index)
    {
        teamIndex = index;
    }
    
    /// <summary>
    /// Add experience points
    /// </summary>
    public void AddExperience(int xp)
    {
        experiencePoints += xp;
        
        // Check for level up
        int newLevel = CalculateLevelFromXP(experiencePoints);
        if (newLevel > level)
        {
            LevelUp(newLevel);
        }
        
        // Notify listeners
        OnStatChanged?.Invoke("Experience", experiencePoints);
    }
    
    /// <summary>
    /// Calculate level based on XP
    /// </summary>
    private int CalculateLevelFromXP(int xp)
    {
        // Simple level calculation: each level requires level * 100 XP
        int calculatedLevel = 1;
        int xpRequired = 100;
        int totalXpRequired = xpRequired;
        
        while (xp >= totalXpRequired && calculatedLevel < 50) // Cap at level 50
        {
            calculatedLevel++;
            xpRequired = calculatedLevel * 100;
            totalXpRequired += xpRequired;
        }
        
        return calculatedLevel;
    }
    
    /// <summary>
    /// Handle level up
    /// </summary>
    private void LevelUp(int newLevel)
    {
        int levelsGained = newLevel - level;
        level = newLevel;
        
        // Apply level up benefits
        // This would be implemented based on progression system design
        
        // Notify listeners
        OnStatChanged?.Invoke("Level", level);
        
        // Show level up UI/effects
        // Implementation depends on UI system
    }
    
    /// <summary>
    /// Record a kill
    /// </summary>
    public void AddKill()
    {
        kills++;
        
        // Add experience for kill
        AddExperience(100);
        
        // Update game manager score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateScore(playerIndex, 1);
        }
        
        // Notify listeners
        OnStatChanged?.Invoke("Kills", kills);
    }
    
    /// <summary>
    /// Record an assist
    /// </summary>
    public void AddAssist()
    {
        assists++;
        
        // Add experience for assist
        AddExperience(50);
        
        // Notify listeners
        OnStatChanged?.Invoke("Assists", assists);
    }
    
    /// <summary>
    /// Record damage dealt to an enemy
    /// </summary>
    public void AddDamageDealt(float damage)
    {
        damageDealt += damage;
        
        // Add experience based on damage
        AddExperience(Mathf.RoundToInt(damage * 0.1f));
        
        // Notify listeners
        OnStatChanged?.Invoke("DamageDealt", damageDealt);
    }
    
    /// <summary>
    /// Handle player taking damage
    /// </summary>
    private void OnPlayerDamaged(float damage)
    {
        damageTaken += damage;
        
        // Notify listeners
        OnStatChanged?.Invoke("DamageTaken", damageTaken);
    }
    
    /// <summary>
    /// Handle player death
    /// </summary>
    private void OnPlayerDeath()
    {
        deaths++;
        
        // Notify game manager of death
        if (GameManager.Instance != null && !isAI)
        {
            // Game manager might handle respawning, score updates, etc.
        }
        
        // Notify listeners
        OnStatChanged?.Invoke("Deaths", deaths);
    }
    
    /// <summary>
    /// Change tank loadout
    /// </summary>
    public void SetTankLoadout(TankLoadout newLoadout)
    {
        currentLoadout = newLoadout;
        ApplyTankLoadout();
    }
    
    /// <summary>
    /// Get end-of-match stats
    /// </summary>
    public MatchStats GetMatchStats()
    {
        return new MatchStats
        {
            playerName = playerName,
            playerIndex = playerIndex,
            level = level,
            kills = kills,
            deaths = deaths,
            assists = assists,
            damageDealt = damageDealt,
            damageTaken = damageTaken,
            tankModel = tankModel
        };
    }
    
    /// <summary>
    /// Structure to hold match statistics
    /// </summary>
    public struct MatchStats
    {
        public string playerName;
        public int playerIndex;
        public int level;
        public int kills;
        public int deaths;
        public int assists;
        public float damageDealt;
        public float damageTaken;
        public string tankModel;
        
        public float KillDeathRatio => deaths > 0 ? (float)kills / deaths : kills;
    }
} 