# Tank Commander - Prefabs

This folder contains all prefab assets used in the Tank Commander game. Below is an overview of the main prefab structures and how they are configured.

## Tank Prefabs

### Player Tank Structure

```
PlayerTank (Root GameObject)
├── Body (3D Model)
│   ├── Mesh Renderer + Collider
│   └── Effects (exhaust, dust, etc.)
├── Turret (3D Model)
│   ├── Mesh Renderer + Collider
│   ├── FirePoint (Transform)
│   └── SecondaryFirePoint (Transform)
├── Wheels (Visual only)
│   ├── LeftTread (Animated)
│   └── RightTread (Animated)
└── Components
    ├── TankController
    ├── WeaponSystem
    ├── Health
    ├── PlayerInfo
    ├── InputManager
    ├── Rigidbody
    └── Audio Sources
```

### Enemy Tank Structure

```
EnemyTank (Root GameObject)
├── Body (3D Model)
│   ├── Mesh Renderer + Collider
│   └── Effects (exhaust, dust, etc.)
├── Turret (3D Model)
│   ├── Mesh Renderer + Collider
│   ├── FirePoint (Transform)
│   └── SecondaryFirePoint (Transform)
├── Wheels (Visual only)
│   ├── LeftTread (Animated)
│   └── RightTread (Animated)
└── Components
    ├── TankController
    ├── WeaponSystem
    ├── Health
    ├── TankAI
    ├── NavMeshAgent
    ├── Rigidbody
    └── Audio Sources
```

## Tank Class Variations

Each tank type has specific configuration differences:

### Light Tank
- Higher speed and acceleration
- Lower health and armor
- Quicker turret rotation
- Lighter weapons with faster fire rate

### Medium Tank
- Balanced stats
- Medium health and armor
- Average speed and acceleration
- Versatile weapons

### Heavy Tank
- High health and armor
- Slower speed and acceleration
- Powerful primary weapon
- Slower turret rotation

### Artillery Tank
- Very low armor
- Slow movement
- Extremely powerful long-range weapon
- Long reload times

## Weapon Prefabs

### Projectiles

```
Projectile (Root GameObject)
├── Mesh/Visual Effects
├── Trail Effect
├── Collider (usually sphere)
├── Rigidbody
├── Projectile Script
└── Audio Source
```

### Explosion Effects

```
Explosion (Root GameObject)
├── Particle Systems
│   ├── Initial Flash
│   ├── Smoke
│   ├── Debris
│   └── Shockwave
├── Light Source (timed)
└── Audio Source
```

## Other Prefabs

### Camera Rig

```
CameraRig (Root GameObject)
├── Main Camera
│   └── Audio Listener
└── CameraController
```

### Level Objects

- Destructible Barriers
- Terrain Elements
- Spawn Points
- Pickup Items
- Visual Effects

## How to Use Prefabs

1. Drag the appropriate tank prefab into your scene
2. Configure the specific tank properties in the inspector
3. For player tanks, ensure the input system is configured
4. For AI tanks, set up patrol points if needed

## Adding New Tank Prefabs

1. Duplicate an existing prefab as a starting point
2. Replace the 3D models with your new tank
3. Adjust the component values to balance the tank
4. Update colliders to match the new geometry
5. Test the tank's performance in the game

## Performance Notes

- Tank prefabs use LOD (Level of Detail) for optimal performance
- Destruction effects are optimized to limit particle count
- Physics colliders use simplified shapes for performance
- Mobile versions use reduced effects and simplified models 