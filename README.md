# Tank Commander

A 3D tank combat game inspired by classic N64 tank games, featuring arena battles, a single-player campaign, and multiplayer modes.

![Tank Commander Logo](path_to_logo.png)

## Overview

Tank Commander delivers nostalgic polygon-based graphics with modern gameplay mechanics and online capabilities. The game includes:

- **Campaign Mode**: Story-driven single-player experience with progressive missions
- **Arcade Mode**: Quick play sessions with customizable settings
- **Multiplayer**: Local split-screen and online multiplayer modes (deathmatch, team battle, capture the flag)

## Features

- Intuitive tank controls with realistic physics
- Multiple camera perspectives (third-person, top-down, first-person)
- Variety of tanks with different stats and capabilities
- Weapon customization and loadout system
- AI opponents with dynamic behavior
- Destructible environments
- Progression system with unlockable content

## Development Setup

### Requirements
- Unity 2022.3 LTS
- Visual Studio 2022 or Visual Studio Code
- Git LFS for large asset management

### Getting Started
1. Clone the repository: `git clone https://github.com/yourusername/tank-commander.git`
2. Install Unity 2022.3 LTS
3. Open the project in Unity
4. Install the required packages from the Package Manager:
   - Universal Render Pipeline (URP)
   - Input System
   - Multiplayer Tools
   - Cinemachine
   - Post Processing

## Project Structure

- `unity-project/`: Unity project folder
  - `Assets/Scripts/`: C# scripts for game functionality
  - `Assets/Models/`: 3D models for tanks, environments, etc.
  - `Assets/Prefabs/`: Reusable game objects
  - `Assets/Scenes/`: Game scenes (menus, levels, etc.)
  - `Assets/InputSystem/`: Input action assets
- `docs/`: Design documents and technical guides
- `art/`: Concept art and asset design files

## Controls
- **Movement**: WASD
- **Turret Rotation**: Mouse
- **Fire**: Left Mouse Button
- **Special Weapon**: Right Mouse Button
- **Toggle Camera**: C
- **Zoom**: Middle Mouse Button
- **Pause**: ESC

## Development Roadmap

See the [TIMELINE.md](unity-project/TIMELINE.md) file for a detailed development schedule.

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Commit your changes: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature-name`
5. Open a pull request

## License

This project is licensed under the [MIT License](LICENSE.md) - see the LICENSE.md file for details.

## Acknowledgements

- [Unity Technologies](https://unity.com/)
- [Asset attribution for third-party resources]
- [Any other acknowledgements] 