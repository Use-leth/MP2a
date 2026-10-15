# Damage amplifier test

Exit Play mode, refresh Assets, then open RoomScaleDefense. Setup runs after compilation. If key 5 is unavailable, run Tools > MP2a > Setup Damage Amplifier.

1. Build a bottle cap, spring punch, or soda tower. Press 5 and hover a nearby free cell: towers within 1.5m get a yellow preview marker. Build costs 20 SCRAP.
2. With no attack towers in range it waits without charging. With 1/2/3 attack towers it requires 6/8/10 seconds; fans and other amplifiers do not count. Press SPACE and hover the amplifier to see CONNECTED and CHARGE TIME. It provides 4 seconds of x1.50 damage. A yellow ring expands when the boost starts; the charge bar drains during the boost and yellow links connect affected towers.
3. Hover an affected attack tower: its label shows AMP multiplier, remaining seconds, and final damage/DPS. The yellow marker disappears when the boost expires. Charging repeats automatically.
4. Hover the amplifier and press E: upgrades cost 15 then 30 SCRAP, increasing damage to x1.75 then x2.00. Insufficient funds and maximum level should reject the purchase.
5. Add a fan beside the same tower: blue speed and yellow damage bonuses can coexist. Multiple amplifiers use the strongest active bonus, not multiplied bonuses.
6. Sell an active amplifier with right click: its damage bonus should disappear. Attack tower upgrades remain intact. Build phases pause charging and active duration along with combat.
7. During charging, build or sell an attack tower in range: the required time changes within 0.1 seconds, while elapsed charge seconds are retained. Removing all attack towers resets charge. An active boost keeps its four-second duration.

## Projectile collisions

After refreshing outside Play, setup adds a kinematic Rigidbody and trigger SphereCollider to Projectile, CapProjectile and PunchProjectile, and a trigger BoxCollider to RoomScaleEnemy. If needed run Tools > MP2a > Setup Projectile Collisions.

- Build bottle cap and spring punch towers, start a wave. Both projectiles should hit rats, play their existing impact effects, reduce health, and disappear. Soda should still work normally.
- Watch a crowded group: each projectile damages only one rat once, including when enemies overlap. Shots can hit a rat before the original target.
- For a direct callback check, use a single attack tower, select its live target, and temporarily disable that rat's BoxCollider in Play. In an isolated shot it should receive no projectile damage despite the projectile reaching its center; enabling the collider should allow the hit. Stop Play to discard this test change.
- Keep the current projectile speeds for this test; much faster projectiles require a separate tunneling check.

Model replacement: replace the Visual child while retaining the root scripts, collider, and references. Reassign chargeBar if replacing that mesh. Preserve AmplifierStatus and ChargePulse, or assign replacement objects to the public fields. Attack prefabs retain TowerDamageBoost and its yellowMarker reference.

Validation: runtime and editor assemblies compiled with Unity's compiler. The setup includes isolated editor checks for charge threshold, boost value, duration, range exclusion, and expiry; these execute when Unity runs setup. Play mode appearance and interaction still require the steps above.
