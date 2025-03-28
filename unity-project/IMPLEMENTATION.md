# Tank Commander - Implementation Guide

## Project Overview

Tank Commander is a 3D tank combat game inspired by classic N64 tank games. This document outlines the technical implementation details of the game's core systems and how they interact with each other.

## Core Systems

### Tank Movement and Controls

The tank movement system is built around the `TankController` class, which handles:
- Tank movement using physics-based rigidbody
- Tank and turret rotation
- Input handling
- Ground detection and slope handling

The `InputManager` provides the bridge between Unity's new Input System and the tank controller, allowing for flexible control schemes across different platforms.

### Weapon System

The weapon system is implemented in the `WeaponSystem` class, providing:
- Primary and secondary weapons
- Ammo management and reloading
- Damage calculations
- Projectile spawning and physics

The `Projectile` class handles the behavior of fired projectiles, including:
- Movement and lifetime
- Collision detection
- Explosion effects and area damage
- Impact forces on physics objects

### Health and Damage

The `Health` class manages:
- Health points and armor values
- Damage application with directional armor
- Death handling and effects
- Team-based damage filtering

### AI System

The AI system uses the `TankAI` class to create computer-controlled opponents with:
- Multiple behavior types (patrol, guard, chase, ambush, flee)
- NavMesh-based pathfinding
- Target detection and line-of-sight checks
- Dynamic behavior switching based on situation

### Camera System

The `CameraController` provides multiple camera views:
- Third-person following camera
- Top-down strategic view
- First-person from turret
- Fixed angle camera
- Smooth transitions between views
- Camera effects (shake, recoil, zoom)

### Game Management

The `GameManager` handles global game state:
- Game modes (Campaign, Arcade, Multiplayer)
- Level loading and setup
- Player spawning and team management
- Score tracking
- Match timing
- Game state transitions (menu, playing, paused, game over)

### Player Information

The `PlayerInfo` class stores player-specific data:
- Player identity (name, team, etc.)
- Tank configuration and loadouts
- Stats tracking (kills, deaths, damage, etc.)
- Experience and progression

## Code Organization

### Scripts Folder Structure

- **Core**: Core game systems
  - `TankController.cs` - Tank movement and controls
  - `WeaponSystem.cs` - Weapon management and firing
  - `Health.cs` - Health, damage, and death handling
  - `Projectile.cs` - Projectile behavior and effects
- **AI**: AI systems
  - `TankAI.cs` - Enemy tank AI behaviors
  - `PatrolPoint.cs` - Patrol path management
- **Player**: Player-specific systems
  - `PlayerInfo.cs` - Player stats and configuration
  - `InputManager.cs` - Input handling
- **Camera**: Camera systems
  - `CameraController.cs` - Camera behavior and effects
- **Managers**: Global managers
  - `GameManager.cs` - Game state and mode management
  - `AudioManager.cs` - Audio playback and management
  - `UIManager.cs` - HUD and menu management
- **UI**: User interface
  - `HUD.cs` - In-game HUD
  - `MainMenu.cs` - Main menu functionality
  - `PauseMenu.cs` - Pause menu functionality

## System Interactions

### Input Flow
1. Input actions captured by Unity Input System
2. Processed by `InputManager`
3. Forwarded to `TankController` for movement
4. Weapon actions sent to `WeaponSystem`
5. Camera controls sent to `CameraController`

### Combat Flow
1. Player/AI fires weapon using `WeaponSystem`
2. `Projectile` is spawned and moves through physics system
3. On collision, `Projectile` calls `Health.TakeDamage()` on target
4. `Health` applies damage modifiers based on armor and direction
5. If health depletes, `Health.Die()` is called
6. Death event triggers score updates in `GameManager` and `PlayerInfo`

### Game Loop
1. `GameManager` handles game initialization and mode selection
2. Levels are loaded and set up with players and AI
3. Match timer and score tracking during gameplay
4. Win/loss conditions checked each frame
5. End-of-match statistics and progression updates
6. Return to menu or level progression

## Input System

The game uses Unity's new Input System with the following action maps:
- **Gameplay**: In-game controls for tank movement, firing, etc.
- **UI**: Menu navigation and interaction

The `TankControls.inputactions` asset defines all input bindings for keyboard/mouse and gamepad control schemes.

## Adding New Features

### New Weapon Types
1. Add a new weapon configuration in `WeaponSystem.WeaponStats`
2. Create a new projectile prefab if needed
3. Implement specific behavior in a derived `Projectile` class
4. Add UI elements to display the weapon

### New Tank Types
1. Create a new tank model and prefab
2. Configure base stats in `PlayerInfo.TankClass`
3. Add specific handling in `TankController` if needed
4. Update UI to display new tank options

### New Game Modes
1. Add new mode to `GameManager.GameMode` enum
2. Implement mode-specific setup in `GameManager.SetupLevel()`
3. Create mode-specific win conditions
4. Add UI for mode selection and display

## Performance Considerations

- Use object pooling for frequently spawned objects (projectiles, effects)
- Implement proper LOD systems for tanks and environments
- Use NavMesh obstacles for dynamic environment changes
- Optimize AI updates using appropriate update intervals
- Minimize physics interactions in high-density scenarios

## Debugging Tools

- Enable debug visualization in `TankController` to see ground checks
- `TankAI` includes gizmo visualization for detection ranges and paths
- `Health` component can be set to invulnerable for testing
- `GameManager` allows skipping levels and adjusting time scales 