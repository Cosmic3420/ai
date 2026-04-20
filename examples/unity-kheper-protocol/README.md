# Unity Kheper Protocol MVP (Controller + Combat + UI)

This example provides a more complete **vertical slice foundation** for a 2D/2.5D fighting prototype:

- playable movement and normals
- Ma'at risk/reward combat state
- energy-based special moves
- animation + hit reaction hooks
- menu + character select + HUD plumbing

## Included scripts

- `PlayerCombatController.cs`
  - Move / jump / dash
  - 3-hit normal combo (`J`)
  - Ma'at balance system (buff/penalty)
  - Optional animator sync (`Speed`, `IsGrounded`, `AttackTrigger`, `ComboStep`, `IsDashing`)
  - `MaatChanged` event for UI sync
  - optional hit-stop/camera shake impact feedback
- `SpecialMoves.cs`
  - Kheper Energy regen and spending
  - `K`: Wolf Summon AoE
  - `L`: Dash Strike
  - `isUsingAbility` gate to prevent overlap/spam conflicts
  - `EnergyChanged` event for UI sync
- `EnemyCombatTarget.cs`
  - Damage intake
  - Knockback
  - Hit stun coroutine
  - optional `Hit` / `Die` animator triggers
  - hit flash + delayed death cleanup
- `MainMenuController.cs`
  - Play / options / quit flow
- `CharacterSelectController.cs`
  - Vora/Sabra selection with preview
  - scene transition into match
- `FighterSpawner.cs`
  - spawns selected fighter prefab into match scene
  - auto-binds player systems to HUD
- `GameHUD.cs`
  - updates Ma'at and Energy sliders
  - supports dynamic binding after runtime spawn

## Unity setup (quick start)

### 1) Player prefabs

Create `Vora` and `Sabra` prefabs with:

- `Rigidbody2D` (freeze Z rotation)
- `BoxCollider2D` (or equivalent)
- `PlayerCombatController`
- `SpecialMoves`
- optional `Animator`
- child transforms:
  - `GroundCheck` near feet
  - `AttackPoint` in front of body

Set these layers/masks in Inspector:

- `groundLayer` for floor collision
- `enemyLayer` for attack overlap checks

### 2) Enemy test target

Create a dummy opponent with:

- `Rigidbody2D`
- Collider
- `EnemyCombatTarget`
- optional `Animator` with `Hit` and `Die` triggers

### 3) Main menu scene

- Add `MainMenuController` to a manager object.
- Assign:
  - `mainPanel`
  - `optionsPanel`
  - `gameSceneName` (must match your game scene)
- Hook buttons:
  - Play -> `PlayGame()`
  - Options -> `OpenOptions()`
  - Back -> `CloseOptions()`
  - Quit -> `QuitGame()`

### 4) Character select scene

- Add `CharacterSelectController`.
- Hook buttons:
  - Vora -> `SelectVora()`
  - Sabra -> `SelectSabra()`
  - Confirm -> `ConfirmSelection()`
- Optional: assign preview image and sprites.

### 5) Game scene

- Add `FighterSpawner` to a bootstrap object.
- Assign:
  - `spawnPoint`
  - `voraPrefab`
  - `sabraPrefab`
  - `hud`
- Add `GameHUD` to your canvas HUD object.
- Assign the Ma'at and Energy sliders.

### 6) Animation timing (recommended)

In `PlayerCombatController`:

- set `useAnimationEvents = true`
- keep `DealDamage()` public
- trigger `DealDamage()` via attack animation event on the actual impact frame

This prevents "instant hit on button press" feel and keeps damage timing aligned to animation.

### 7) Build settings

Add scenes in this order:

1. `MainMenu`
2. `CharacterSelect`
3. `GameScene`

## Controls

- Move: `A/D` or arrow keys (`Horizontal` axis)
- Jump: `Space`
- Dash: `Left Shift`
- Normal combo: `J`
- Wolf Summon: `K`
- Dash Strike: `L`

## Notes

- Ma'at states:
  - Low (`<= 20`): imbalance movement/damage penalty
  - High (`>= 80`): harmony damage bonus
- Scripts use `Rigidbody2D.velocity` for broad Unity compatibility.
- Camera shake helper is intentionally minimal; replace with Cinemachine impulse in production.
