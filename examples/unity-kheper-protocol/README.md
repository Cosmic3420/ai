# Unity Kheper Protocol MVP (Controller + Combat + UI)

This example provides a more complete **vertical slice foundation** for a 2D/2.5D fighting prototype:

- playable movement and normals
- Ma'at risk/reward combat state
- energy-based special moves
- menu + character select + HUD plumbing

## Included scripts

- `PlayerCombatController.cs`
  - Move / jump / dash
  - 3-hit normal combo (`J`)
  - Ma'at balance system (buff/penalty)
  - optional animator sync (`Speed`, `IsGrounded`, `AttackTrigger`, `ComboStep`, `IsDashing`)
  - animation-event friendly `DealDamage()` + optional hit-stop
  - `MaatChanged` event for UI sync
- `SpecialMoves.cs`
  - Kheper Energy regen and spending
  - `K`: Wolf Summon AoE
  - `L`: Dash Strike
  - `EnergyChanged` event for UI sync
- `EnemyCombatTarget.cs`
  - Damage intake
  - Knockback + hit-stun timing hook
  - optional hit/death animator triggers
  - Hit flash + death/despawn
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


### 5.1) Animation + hit reaction wiring

For **Player Animator**, create states such as: `Idle`, `Run`, `Jump`, `Attack1`, `Attack2`, `Attack3`, `Dash`, `Special`.

Set the controller parameter names to match defaults in `PlayerCombatController`:

- `Speed` (float)
- `IsGrounded` (bool)
- `AttackTrigger` (trigger)
- `ComboStep` (int)
- `IsDashing` (bool)

For frame-accurate impacts:

1. Keep `attackUsesAnimationEvents = true` on `PlayerCombatController`.
2. Add an animation event at the impact frame in each attack clip.
3. Call `DealDamage()` from the animation event.

For **Enemy Animator**, set optional triggers:

- `Hit`
- `Die`

These map to `EnemyCombatTarget` defaults.

### 6) Build settings

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

- `DealDamage()` is public so you can trigger it from animation events for frame-accurate attacks.
- Ma'at states:
  - Low (`<= 20`): imbalance movement/damage penalty
  - High (`>= 80`): harmony damage bonus
- Scripts use `Rigidbody2D.velocity` for broad Unity compatibility.
- If you do not use animation events yet, set `attackUsesAnimationEvents = false` for immediate damage on button press.
