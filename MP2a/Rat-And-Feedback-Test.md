# Rat and combat visual pass

Save work and exit Play. Refresh Assets; setup runs after compilation. Manual entry: Tools > MP2a > Setup Rat And Combat Visuals. No downloaded source prefabs, materials or animation clips are modified. RatRunLoop.anim and RatRun.controller are game-specific copies/assets.

- Build key 1: check the can base, paired bottle launchers and cap magazine; upgrade indicator still changes color. Confirm projectile and muzzle effect origins look sensible from the player camera.
- Start a wave: rats should run continuously rather than cycle demo actions, face the path at corners, remain on the floor, take projectile hits, show health loss and drop scrap. On defeat their movement and animation should stop. If model forward is offset, RatMovementVisual.facingOffset is an Inspector adjustment.
- Place an amplifier near an attack tower: while charging yellow sparks grow denser; at release a short burst accompanies the existing pulse. A gold aura appears on each actually boosted attack tower and stops emitting at expiry/removal.
- Place a fan near the same tower: blue particles accompany the existing airflow line. Both color effects coexist while both buffs are active. Preview markers alone should not start buff particles.
- Between waves the amplifier does not charge; spawned particles finish normally. Existing active buff state may remain visible while its gameplay timer is paused.
- Verify particles do not obstruct clicks: these effects have no collider, no distortion mesh and at most 50 live particles per system.

Runtime/editor compilation passed. Final appearance, model orientation and animation rig compatibility require Unity Play-mode verification.
