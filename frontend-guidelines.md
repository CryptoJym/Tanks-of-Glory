# Tank Commander: Frontend Guidelines

## Design Philosophy

The frontend of Tank Commander aims to balance nostalgic N64-era aesthetics with modern usability and accessibility. The user interface should be intuitive, responsive, and immersive while maintaining the distinctive low-poly charm of classic tank games.

## Visual Identity

### Color Palette

- **Primary Colors**: Military greens (#4B5320, #556B2F) and metallic grays (#71797E, #848884)
- **Secondary Colors**: Desert tan (#D2B48C), urban concrete (#A9A9A9), snow white (#F5F5F5)
- **Accent Colors**: Warning red (#FF0000), ammunition yellow (#FFD700), health green (#00FF00)
- **UI Elements**: Dark interface (#2C3539) with high contrast text (#FFFFFF)

### Typography

- **Primary Font**: "Armored" (custom font for headings and titles)
- **Secondary Font**: "Battalion" (for UI elements and buttons)
- **Body Text**: "Combat Sans" (clean, readable sans-serif for instructions and descriptions)
- **Font Hierarchy**:
  - H1: 32px, bold
  - H2: 24px, bold
  - H3: 18px, semibold
  - Body: 16px, regular
  - Small text: 14px, regular
  - Button text: 16px, bold

### Iconography

- **Style**: Bold, silhouette-based icons with clear meaning
- **Consistency**: Similar visual weight and style across all icons
- **Accessibility**: All icons accompanied by text labels or tooltips
- **Animation**: Subtle animations for interactive elements (hover, click)

## User Interface Components

### HUD (Heads-Up Display)

- **Health Bar**: Located top-left, visually distinct with color gradient
- **Ammunition Counter**: Bottom-right, showing current weapon and ammo count
- **Minimap**: Bottom-left, showing terrain and enemy positions
- **Radar**: Optional circular overlay showing enemy positions
- **Score/Timer**: Top-center for match information and time remaining
- **Objective Indicators**: Dynamic markers for mission objectives

### Menus

- **Main Menu**: 
  - 3D background with rotating tank models
  - Clear navigation with large, accessible buttons
  - Visual hierarchy emphasizing play options

- **Pause Menu**:
  - Overlay on slightly dimmed game background
  - Quick access to resume, settings, and exit
  - Simple, uncluttered layout

- **Settings Menu**:
  - Categorized tabs (Graphics, Audio, Controls, Gameplay)
  - Visual sliders and toggles for adjustments
  - Preview functionality for visual/audio changes

### Game Lobby

- **Player Cards**: Visual representation of players and selected tanks
- **Map Selection**: Thumbnail previews with voting system
- **Game Mode Options**: Visual icons with descriptive text
- **Ready Status**: Clear indicators for player readiness
- **Chat System**: Collapsible chat interface with team/all options

## Interaction Design

### Controls

- **Keyboard/Mouse**:
  - WASD for tank movement
  - Mouse for turret aiming
  - Left-click to fire
  - Right-click for secondary weapon/ability
  - Space for boost/special action
  - Tab for scoreboard
  - Esc for pause menu

- **Controller**:
  - Left stick for movement
  - Right stick for turret aiming
  - Right trigger to fire
  - Left trigger for secondary weapon/ability
  - A/X for boost/special action
  - Back/Select for scoreboard
  - Start for pause menu

### Feedback Systems

- **Visual Feedback**:
  - Hit indicators showing direction of damage
  - Screen effects for critical damage
  - Particle effects for environment interaction
  - Visual cues for power-up acquisition

- **Audio Feedback**:
  - Distinctive sounds for hits, misses, and critical hits
  - Spatial audio for positional awareness
  - Warning sounds for low health/ammunition
  - Voice announcements for game events

- **Haptic Feedback** (where supported):
  - Varied intensity for different weapons
  - Environmental feedback (terrain types)
  - Damage indicators through vibration patterns

## Animation Guidelines

- **UI Animations**:
  - Menu transitions: 200-300ms easing functions
  - Button hover/press: 100ms subtle scale/color change
  - Notification appear/disappear: 250ms fade with slight movement

- **Game Animations**:
  - Tank movement: Weight-appropriate acceleration/deceleration
  - Turret rotation: Smooth but with mechanical feel
  - Projectile effects: Exaggerated trajectories for visual clarity
  - Explosion effects: Stylized but readable, with appropriate screen shake

## Accessibility

- **Text Readability**:
  - Minimum 16px for body text
  - High contrast against backgrounds
  - Option for larger text sizes

- **Color Blindness**:
  - Alternative color schemes for common color vision deficiencies
  - Shape differentiation in addition to color coding
  - Customizable team colors

- **Audio Accessibility**:
  - Subtitles for all voice announcements
  - Visual alternatives for audio cues
  - Independent volume controls for effects, music, and voice

- **Control Customization**:
  - Fully remappable controls
  - Sensitivity adjustments
  - Toggle options for actions requiring button holds

## Performance Considerations

- **Target Frame Rate**: Consistent 60 FPS across supported platforms
- **Asset Optimization**:
  - LOD (Level of Detail) implementation for distant objects
  - Texture atlasing for UI elements
  - Polygon budget appropriate for target platforms
- **Rendering Pipeline**:
  - Forward rendering for performance on lower-end hardware
  - Optional enhanced effects for capable systems

## Implementation Guidelines

### Code Structure

- **Component-Based UI**: Reusable, maintainable UI components
- **State Management**: Clear data flow and state updates
- **Event System**: Consistent approach to handling user interactions
- **Responsive Design**: UI scaling based on screen resolution and aspect ratio

### Asset Workflow

- **Naming Convention**: `category_item_variant_state.extension`
- **Directory Structure**:
  ```
  ├── UI
  │   ├── Menus
  │   ├── HUD
  │   ├── Icons
  │   └── Animations
  ├── Models
  │   ├── Tanks
  │   ├── Environment
  │   ├── Effects
  │   └── Weapons
  ├── Textures
  │   ├── Vehicles
  │   ├── Terrain
  │   ├── UI
  │   └── Effects
  └── Audio
      ├── Music
      ├── SFX
      ├── Ambient
      └── Voice
  ```

### Quality Assurance

- **Responsive Testing**: Verify UI at multiple resolutions and aspect ratios
- **Cross-Platform Verification**: Test on all target platforms
- **Performance Profiling**: Monitor frame rate and memory usage
- **Accessibility Validation**: Verify with accessibility tools and guidelines

## Documentation

- **Style Guide**: Living document with all visual elements and usage examples
- **Component Library**: Interactive documentation of UI components
- **Animation Reference**: Timing and easing references for consistent motion
- **Control Mapping**: Default control schemes for all input methods 